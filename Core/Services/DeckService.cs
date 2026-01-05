using Core.Data;
using Core.Dto.Common;
using Core.Dto.Deck;
using Core.Model;
using Core.Model.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class DeckService(DataContext context, IFlashcardAlgorithmService flashcardAlgorithmService, ITimeService timeService) : IDeckService
{
    public async Task<ResponseResult<Deck>> Create(string creatorId, CreateDeckRequest request)
    {
        // Check if deck with same name already exists for this user
        bool deckExists = await context.Decks
            .AnyAsync(d => d.CreatorId == creatorId && d.Name == request.Name);

        if (deckExists)
        {
            return ResponseResult<Deck>.Failure(
                ErrorCode.AlreadyExists,
                $"A deck with the name '{request.Name}' already exists."
            );
        }

        Deck deck = new()
        {
            CreatorId = creatorId,
            Name = request.Name,
            Description = request.Description,
            Option = request.OptionRequest == null ? null : (DeckOption)request.OptionRequest
        };

        await context.Decks.AddAsync(deck);
        await context.SaveChangesAsync();

        return ResponseResult<Deck>.Success(deck);
    }

    public async Task<ResponseResult<bool>> Update(int id, string creatorId, UpdateDeckRequest request)
    {
        Deck? deck = await context.Decks.FirstOrDefaultAsync(d => d.Id == id && d.CreatorId == creatorId);

        if (deck is null)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                $"Deck with ID {id} not found or you don't have permission to update it."
            );
        }

        // Check if another deck with the same name exists (excluding current deck)
        bool nameExists = await context.Decks
            .AnyAsync(d => d.CreatorId == creatorId && d.Name == request.Name && d.Id != id);

        if (nameExists)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.AlreadyExists,
                $"Another deck with the name '{request.Name}' already exists."
            );
        }

        deck.Name = request.Name;
        deck.Description = request.Description;
        deck.Option = request.OptionRequest is null ? null : (DeckOption)request.OptionRequest;

        await context.SaveChangesAsync();

        return ResponseResult<bool>.Success(true);
    }

    public async Task<ResponseResult<bool>> Delete(int id, string creatorId)
    {
        int affectedRows = await context.Decks
            .Where(x => x.CreatorId == creatorId && x.Id == id)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                $"Deck with ID {id} not found or you don't have permission to delete it."
            );
        }

        return ResponseResult<bool>.Success(true);
    }

    public async Task<ResponseResult<DeckStatisticsResponse>> GetStatistics(string creatorId, int id)
    {
        var deck = await context.Decks
            .Where(x => x.CreatorId == creatorId && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                RawLearning = x.Cards.Count(c => c.State == CardState.Learning || c.State == CardState.Relearning),
                RawNew = x.Cards.Count(c => c.State == CardState.New),
                RawReview = x.Cards.Count(c => c.State == CardState.Review),
                RawReviewEaseFactor = x.Cards.Where(c => c.State == CardState.Review).Sum(card => card.EaseFactor),
                RawSuspended = x.Cards.Count(c => c.State == CardState.Suspended),
                x.DailyCounts
            }).FirstOrDefaultAsync();

        if (deck is null)
        {
            return ResponseResult<DeckStatisticsResponse>.Failure(
                ErrorCode.NotFound,
                $"Deck with ID {id} not found or you don't have permission to access its statistics."
            );
        }

        DeckStatisticsResponse statistics = new(
            deck.Id,
            deck.Name,
            deck.Description,
            flashcardAlgorithmService.EstimateRetention(deck.RawReviewEaseFactor),
            new DeckCardCountsResponse(deck.RawLearning, deck.RawReview, deck.RawNew, deck.RawSuspended),
            deck.DailyCounts.Select(x => new DeckStatisticDailyCountResponse(x.Date, x.CardState, x.Count)).ToList()
        );

        return ResponseResult<DeckStatisticsResponse>.Success(statistics);
    }

    public async Task<ResponseResult<DeckSummaryResponse>> GetSummary(string creatorId, int id)
    {
        DateTime today = timeService.UtcNow.Date;

        var deck = await context.Decks
            .Where(x => x.CreatorId == creatorId && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                LearningCount = x.Cards.Count(c => c.State == CardState.Learning || c.State == CardState.Relearning),
                RawNew = x.Cards.Count(c => c.State == CardState.New),
                RawReview = x.Cards.Count(c => c.State == CardState.Review && c.DueDate <= today),
                NewLimit = x.Option != null ? x.Option.NewLimitPerDay : x.Creator.DeckOption.NewLimitPerDay,
                ReviewLimit = x.Option != null ? x.Option.ReviewLimitPerDay : x.Creator.DeckOption.ReviewLimitPerDay
            })
            .FirstOrDefaultAsync();

        if (deck == null)
        {
            return ResponseResult<DeckSummaryResponse>.Failure(
                ErrorCode.NotFound,
                $"Deck with ID {id} not found or you don't have permission to access its summary."
            );
        }

        DeckSummaryResponse summary = new(
            deck.Id,
            deck.Name,
            deck.Description,
            new DeckDueCountResponse(
                deck.LearningCount,
                Math.Min(deck.RawReview, deck.ReviewLimit),
                Math.Min(deck.RawNew, deck.NewLimit)
            )
        );

        return ResponseResult<DeckSummaryResponse>.Success(summary);
    }
}