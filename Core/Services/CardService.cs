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
    IFlashcardAlgorithmService algorithmService)
    : BaseService, ICardService
{
    public async Task<PaginationResult<Card>> Get(string creatorId, int deckId, PaginationRequest<int> request)
    {
        IQueryable<Card> query = context.Cards.AsNoTracking()
            .Where(x => x.Note.DeckId == deckId && x.Note.CreatorId == creatorId);

        return await PaginateAsync(query, request);
    }

    public async Task<int> UpdateCardState(string creatorId, int id, UpdateCardStateRequest request)
    {
        IQueryable<Card> query = context.Cards.Where(x => x.Id == id && x.Note.CreatorId == creatorId);
        (int Interval, int Repetitions, double EaseFactor, DateTime DueDate, int StepIndex) result =
            Card.FlashcardData();
        return request switch
        {
            UpdateCardStateRequest.Suspend => await query.ExecuteUpdateAsync(x =>
                x.SetProperty(u => u.State, CardState.Suspended)),
            UpdateCardStateRequest.Unsuspend or UpdateCardStateRequest.Reset => await query.ExecuteUpdateAsync(x => x
                .SetProperty(u => u.State, CardState.New)
                .SetProperty(u => u.Interval, result.Interval)
                .SetProperty(u => u.Repetitions, result.Repetitions)
                .SetProperty(u => u.EaseFactor, result.EaseFactor)
                .SetProperty(u => u.DueDate, DateTime.UtcNow)
                .SetProperty(u => u.StepIndex, result.StepIndex)
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };
    }

    public async Task<CardResponse?> GetNextStudyCard(string creatorId, int deckId)
    {
        DateTime now = DateTime.UtcNow;

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

        if (deck == null) return null;

        IQueryable<Card> baseQuery = context.Cards
            .Include(c => c.Template)
            .Include(c => c.Note)
            .Where(c => c.Note.DeckId == deckId && c.Note.CreatorId == creatorId && c.State != CardState.Suspended);

        // --- PRIORITY 1: URGENT LEARNING ---
        // Learning/Relearning cards always come first because they are time-sensitive (1m, 10m).

        Card? learningCard =
            await ApplySortOrder(
                    baseQuery.Where(c =>
                        (c.State == CardState.Learning || c.State == CardState.Relearning) &&
                        c.DueDate <= now),
                    deck.Option.SortOrder)
                .FirstOrDefaultAsync();

        if (learningCard != null) return await MapToResponse(learningCard);

        // --- PRIORITY 2: THE MIX (Review & New) ---
        int reviewsLeft = Math.Max(0, deck.Option.ReviewLimitPerDay - deck.ReviewDone);
        int newLeft = Math.Max(0, deck.Option.NewLimitPerDay - deck.NewDone);

        if (deck.Option.InterdayLearningMix)
        {
            // ADVANCED: Interleave New and Review
            // If the user wants to see 3 reviews then 1 new, we check a ratio or simply pick the most "overdue"
            // For now, a simple 'Mix' picks whichever is more urgent or uses the SortOrder across both sets.
            Card? mixedCard = await ApplySortOrder(
                    baseQuery.Where(c =>
                        (c.State == CardState.Review && c.DueDate <= now && reviewsLeft > 0) ||
                        (c.State == CardState.New && newLeft > 0)),
                    deck.Option.SortOrder)
                .FirstOrDefaultAsync();

            if (mixedCard != null) return await MapToResponse(mixedCard);
        }
        else
        {
            // CLASSIC: Finish Reviews, then start New
            if (reviewsLeft > 0)
            {
                Card? reviewCard =
                    await ApplySortOrder(
                        baseQuery.Where(c => c.State == CardState.Review && c.DueDate <= now),
                        deck.Option.SortOrder).FirstOrDefaultAsync();
                if (reviewCard != null) return await MapToResponse(reviewCard);
            }

            if (newLeft > 0)
            {
                Card? newCard =
                    await ApplySortOrder(baseQuery.Where(c => c.State == CardState.New), deck.Option.SortOrder)
                        .FirstOrDefaultAsync();
                if (newCard != null) return await MapToResponse(newCard);
            }
        }

        return null;
    }

    // TODO: Add the User Request
    public async Task<bool> SubmitCardReview(string creatorId, int id, CardSubmitRequest request)
    {
        DateTime now = DateTime.UtcNow;
        DateOnly today = DateOnly.FromDateTime(now);

        // 1. Fetch Card with essential relations
        Card? card = await context.Cards
            .Include(x => x.Note)
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == id && x.Note.CreatorId == creatorId && x.State != CardState.Suspended);

        if (card is null) return false;

        // 2. Fetch User with Tracking (needed to update Streaks)
        // We fetch the whole user to avoid the "Owned Entity Projection" crash
        User? user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == creatorId);

        if (user is null) return false;

        // 3. Find AI Provider
        UserAiProvider? provider = user.AiProviders.FirstOrDefault(x => x.Id == request.UserProviderId);
        if (provider is null) return false;

        // 4. AI Evaluation
        IAiService aiService = AiServiceFactory.GetUserService(provider);
        (string front, string back) = (
            await templateService.Parse(card.Template.Front, card.Note.Data),
            await templateService.Parse(card.Template.Back, card.Note.Data)
        );

        int? flashcardQuality = await aiService.CheckAnswer(front, request.Answer, back);
        if (flashcardQuality is null) return false;

        // 5. SRS Algorithm Calculation
        FlashcardResult result = algorithmService.Calculate((int)flashcardQuality, card.State, card.Interval,
            card.EaseFactor,
            card.StepIndex);

        // 6. Update Daily Statistics (Including the missing DATE check)
        DeckDailyCount dailyCount = await context.DailyCounts
                                        .FirstOrDefaultAsync(x => x.DeckId == card.Note.DeckId
                                                                  && x.CardState == result.State
                                                                  && x.Date == today)
                                    ?? new DeckDailyCount
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
        return true;
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

    private IQueryable<Card> ApplySortOrder(IQueryable<Card> query, DeckOptionSortOrder sortOrder)
    {
        return sortOrder switch
        {
            DeckOptionSortOrder.Random => query.OrderBy(c => EF.Functions.Random()),
            DeckOptionSortOrder.DateAdded => query.OrderBy(c => c.Id),
            DeckOptionSortOrder.DueDate => query.OrderBy(c => c.DueDate), // Default Anki behavior
            _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
        };
    }
}