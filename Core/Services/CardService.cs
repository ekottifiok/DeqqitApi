using Core.Data;
using Core.Dto.Card;
using Core.Dto.Common;
using Core.Model;
using Core.Model.Helper;
using Core.Services.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class CardService(
    DataContext context,
    ITemplateService templateService,
    IFlashcardAlgorithmService algorithmService, ITimeService timeService)
    : BaseService, ICardService
{
    public async Task<PaginationResult<Card>> Get(string creatorId, int deckId, PaginationRequest<int> request)
    {
        IQueryable<Card> query = context.Cards.AsNoTracking()
            .Where(x => x.Note.DeckId == deckId && x.Note.CreatorId == creatorId);

        return await PaginateAsync(query, request);
    }

    public async Task<ResponseResult<Card>> Get(string creatorId, int id)
    {
        Card? card = await context.Cards
            .FirstOrDefaultAsync(x => x.Note.CreatorId == creatorId && x.Id == id);
            
        if (card == null)
        {
            return ResponseResult<Card>.Failure(
                ErrorCode.NotFound,
                $"Card with ID {id} not found or you don't have permission to access it."
            );
        }
        
        return ResponseResult<Card>.Success(card);
    }

    public async Task<ResponseResult<bool>> UpdateCardState(string creatorId, int id, UpdateCardStateRequest request)
    {
        (int Interval, int Repetitions, double EaseFactor, DateTime DueDate, int StepIndex) result =
            Card.FlashcardData();
            
        int affectedRows = request switch
        {
            UpdateCardStateRequest.Suspend => await context.Cards
                .Where(x => x.Id == id && x.Note.CreatorId == creatorId)
                .ExecuteUpdateAsync(x => x.SetProperty(u => u.State, CardState.Suspended)),
                
            UpdateCardStateRequest.Unsuspend or UpdateCardStateRequest.Reset => await context.Cards
                .Where(x => x.Id == id && x.Note.CreatorId == creatorId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(u => u.State, CardState.New)
                    .SetProperty(u => u.Interval, result.Interval)
                    .SetProperty(u => u.Repetitions, result.Repetitions)
                    .SetProperty(u => u.EaseFactor, result.EaseFactor)
                    .SetProperty(u => u.DueDate, timeService.UtcNow)
                    .SetProperty(u => u.StepIndex, result.StepIndex)
                ),
                
            _ => 0
        };

        if (affectedRows == 0)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                $"Card with ID {id} not found or you don't have permission to update it."
            );
        }
        
        return ResponseResult<bool>.Success(true);
    }

    public async Task<ResponseResult<CardResponse>> GetNextStudyCard(string creatorId, int deckId)
    {
        DateTime now = timeService.UtcNow;

        // 1. Fetch Deck Settings and Daily Progress
        var deck = await context.Decks
            .AsNoTracking()
            .Where(d => d.Id == deckId)
            .Select(d => new
            {
                Option = d.Option ?? d.Creator.DeckOption,
                NewDone = d.DailyCounts
                    .Where(dc => dc.Date == DateOnly.FromDateTime(now) && dc.CardState == CardState.New)
                    .Sum(dc => dc.Count),
                ReviewDone = d.DailyCounts
                    .Where(dc => dc.Date == DateOnly.FromDateTime(now) && dc.CardState == CardState.Review)
                    .Sum(dc => dc.Count)
            })
            .FirstOrDefaultAsync();

        if (deck == null)
        {
            return ResponseResult<CardResponse>.Failure(
                ErrorCode.NotFound,
                $"Deck with ID {deckId} not found."
            );
        }

        IQueryable<Card> baseQuery = context.Cards
            .Include(c => c.Template)
            .Include(c => c.Note)
            .Where(c => c.Note.DeckId == deckId && c.Note.CreatorId == creatorId && c.State != CardState.Suspended);

        // --- PRIORITY 1: URGENT LEARNING ---
        Card? learningCard =
            await ApplySortOrder(
                    baseQuery.Where(c =>
                        (c.State == CardState.Learning || c.State == CardState.Relearning) &&
                        c.DueDate <= now),
                    deck.Option.SortOrder)
                .FirstOrDefaultAsync();

        if (learningCard != null) 
        {
            CardResponse response = await MapToResponse(learningCard);
            return ResponseResult<CardResponse>.Success(response);
        }

        // --- PRIORITY 2: THE MIX (Review & New) ---
        int reviewsLeft = Math.Max(0, deck.Option.ReviewLimitPerDay - deck.ReviewDone);
        int newLeft = Math.Max(0, deck.Option.NewLimitPerDay - deck.NewDone);

        if (deck.Option.InterdayLearningMix)
        {
            // ADVANCED: Interleave New and Review
            Card? mixedCard = await ApplySortOrder(
                    baseQuery.Where(c =>
                        (c.State == CardState.Review && c.DueDate <= now && reviewsLeft > 0) ||
                        (c.State == CardState.New && newLeft > 0)),
                    deck.Option.SortOrder)
                .FirstOrDefaultAsync();

            if (mixedCard == null)
                return ResponseResult<CardResponse>.Failure(
                    ErrorCode.NotFound,
                    "No cards available for study at this time."
                );
            CardResponse response = await MapToResponse(mixedCard);
            return ResponseResult<CardResponse>.Success(response);
        }

        // CLASSIC: Finish Reviews, then start New
        if (reviewsLeft > 0)
        {
            Card? reviewCard =
                await ApplySortOrder(
                    baseQuery.Where(c => c.State == CardState.Review && c.DueDate <= now),
                    deck.Option.SortOrder).FirstOrDefaultAsync();
            if (reviewCard != null)
            {
                CardResponse response = await MapToResponse(reviewCard);
                return ResponseResult<CardResponse>.Success(response);
            }
        }

        if (newLeft <= 0)
        {
            return ResponseResult<CardResponse>.Failure(
                ErrorCode.InvalidState,
                "Daily new card limit reached for this deck."
            );
        }

        Card? newCard =
            await ApplySortOrder(baseQuery.Where(c => c.State == CardState.New), deck.Option.SortOrder)
                .FirstOrDefaultAsync();
        if (newCard == null)
            return ResponseResult<CardResponse>.Failure(
                ErrorCode.NotFound,
                "No cards available for study at this time."
            );
        {
            CardResponse response = await MapToResponse(newCard);
            return ResponseResult<CardResponse>.Success(response);
        }

    }

    public async Task<ResponseResult<CardResponse>> SubmitCardReview(string creatorId, int id, CardSubmitRequest request)
    {
        DateTime now = timeService.UtcNow;
        DateOnly today = DateOnly.FromDateTime(now);

        // 1. Fetch Card with essential relations
        Card? card = await context.Cards
            .Include(x => x.Note)
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == id && x.Note.CreatorId == creatorId && x.State != CardState.Suspended);

        if (card is null)
        {
            return ResponseResult<CardResponse>.Failure(
                ErrorCode.NotFound,
                $"Card with ID {id} not found or you don't have permission to review it."
            );
        }

        // 2. Fetch User with Tracking
        User? user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == creatorId);
            
        if (user == null)
        {
            return ResponseResult<CardResponse>.Failure(
                ErrorCode.NotFound,
                "User not found."
            );
        }

        // 3. Find AI Provider
        UserAiProvider? provider = user.AiProviders.FirstOrDefault(x => x.Id == request.UserProviderId);
        if (provider is null)
        {
            return ResponseResult<CardResponse>.Failure(
                ErrorCode.NotFound,
                $"AI provider with ID {request.UserProviderId} not found or you don't have permission to use it."
            );
        }

        // 4. AI Evaluation
        IAiService aiService = AiServiceFactory.GetUserService(provider);
        (string front, string back) = (
            await templateService.Parse(card.Template.Front, card.Note.Data),
            await templateService.Parse(card.Template.Back, card.Note.Data)
        );

        int? flashcardQuality = await aiService.CheckAnswer(front, request.Answer, back);
        if (flashcardQuality is null)
        {
            return ResponseResult<CardResponse>.Failure(
                ErrorCode.InvalidState,
                "Unable to evaluate answer with AI provider."
            );
        }

        // 5. SRS Algorithm Calculation
        FlashcardResult result = algorithmService.Calculate((int)flashcardQuality, card.State, card.Interval,
            card.EaseFactor, card.StepIndex);

        // 6. Update Daily Statistics
        DeckDailyCount? dailyCount = await context.DailyCounts
            .FirstOrDefaultAsync(x => x.DeckId == card.Note.DeckId
                                      && x.CardState == result.State
                                      && x.Date == today);
        dailyCount ??= new DeckDailyCount
        {
            DeckId = card.Note.DeckId,
            CardState = result.State,
            Date = today
        };

        if (dailyCount.Id == 0) context.DailyCounts.Add(dailyCount);
        dailyCount.Count += 1;

        // 7. Apply Results to Card
        card.State = result.State;
        card.Interval = result.Interval;
        card.Repetitions = result.Repetitions;
        card.EaseFactor = result.EaseFactor;
        card.DueDate = now.AddMinutes(result.Interval);
        card.StepIndex = result.LearningStepIndex;

        // 8. Update Streak
        if (!user.UserStreaks.Contains(today)) user.UserStreaks.Add(today);

        await context.SaveChangesAsync();
        
        CardResponse response = await MapToResponse(card);
        return ResponseResult<CardResponse>.Success(response);
    }

    private async Task<CardResponse> MapToResponse(Card card)
    {
        return new CardResponse(
            card.Id,
            card.State,
            card.EaseFactor,
            await templateService.Parse(card.Template.Front, card.Note.Data),
            await templateService.Parse(card.Template.Back, card.Note.Data)
        );
    }

    private static IQueryable<Card> ApplySortOrder(IQueryable<Card> query, DeckOptionSortOrder sortOrder)
    {
        return sortOrder switch
        {
            DeckOptionSortOrder.Random => query.OrderBy(c => EF.Functions.Random()),
            DeckOptionSortOrder.DateAdded => query.OrderBy(c => c.Id),
            DeckOptionSortOrder.DueDate => query.OrderBy(c => c.DueDate),
            _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
        };
    }
}