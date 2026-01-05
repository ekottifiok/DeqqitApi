using Core.Data;
using Core.Dto.Common;
using Core.Dto.Deck;
using Core.Dto.User;
using Core.Model;
using Core.Model.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class UserService(DataContext context, IFlashcardAlgorithmService flashcardAlgorithmService, ITimeService timeService) : IUserService
{
    public async Task<ResponseResult<bool>> UpdateProfileImage(string id, string profileImage)
    {
        int affectedRows = await context.Users
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.ProfileImageUrl, profileImage));

        if (affectedRows == 0)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                $"User with ID '{id}' not found."
            );
        }

        return ResponseResult<bool>.Success(true);
    }

    public async Task<ResponseResult<UserResponse>> Get(string id)
    {
        UserResponse? userResponse = await context.Users
            .Where(x => x.Id == id)
            .Select(x => (UserResponse)x)
            .FirstOrDefaultAsync();

        if (userResponse == null)
        {
            return ResponseResult<UserResponse>.Failure(
                ErrorCode.NotFound,
                $"User with ID '{id}' not found."
            );
        }

        return ResponseResult<UserResponse>.Success(userResponse);
    }

    public async Task<ResponseResult<int?>> GetDefaultProviderId(string userId)
    {
        User? user = await context.Users
            .Include(u => u.AiProviders)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return ResponseResult<int?>.Failure(
                ErrorCode.NotFound,
                $"User with ID '{userId}' not found."
            );
        }

        int? providerId = user.AiProviders.FirstOrDefault()?.Id;
        
        if (providerId == null)
        {
            return ResponseResult<int?>.Failure(
                ErrorCode.NotFound,
                $"User with ID '{userId}' does not have any AI providers configured."
            );
        }        
        return ResponseResult<int?>.Success(providerId);
    }

    public async Task<ResponseResult<bool>> Update(string id, UpdateUserRequest request)
    {
        User? user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                $"User with ID '{id}' not found."
            );
        }

        user.AiProviders = request.AiProviders.Select(provider => (UserAiProvider)provider).ToList();
        user.DeckOption = request.DeckOption;
        await context.SaveChangesAsync();

        return ResponseResult<bool>.Success(true);
    }

    public async Task<ResponseResult<UserDashboardResponse>> GetUserDashboard(string userId)
    {
        DateTime today = timeService.UtcNow;

        // COMBINED QUERY: One trip to the database for everything
        var result = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.UserStreaks,
                Decks = u.Decks.Select(x => new
                {
                    x.Id,
                    x.Name,
                    LearningCount =
                        x.Cards.Count(c => c.State == CardState.Learning || c.State == CardState.Relearning),
                    RawNew = x.Cards.Count(c => c.State == CardState.New),
                    RawReview = x.Cards.Count(c => c.State == CardState.Review && c.DueDate <= today),

                    // FIX: Cast to (double?) so it handles empty collections without crashing
                    // This was the cause of "Nullable object must have a value"
                    AvgEase = x.Cards.Where(c => c.State == CardState.Review)
                        .Select(c => (double?)c.EaseFactor)
                        .Average(),

                    NewLimit = x.Option != null ? x.Option.NewLimitPerDay : u.DeckOption.NewLimitPerDay,
                    ReviewLimit = x.Option != null ? x.Option.ReviewLimitPerDay : u.DeckOption.ReviewLimitPerDay
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return ResponseResult<UserDashboardResponse>.Failure(
                ErrorCode.NotFound,
                $"User with ID '{userId}' not found."
            );
        }

        // Mapping to DTOs
        IEnumerable<UserDeckSummaryResponse> deckResponses = result.Decks.Select(d => new UserDeckSummaryResponse(
            d.Id,
            d.Name,
            new DeckDueCountResponse(
                d.LearningCount,
                Math.Min(d.RawReview, d.ReviewLimit),
                Math.Min(d.RawNew, d.NewLimit)
            )
        ));

        // Calculate Retention
        // .Average() on the C# side also needs protection from empty lists
        List<double> validEaseFactors = result.Decks
            .Where(d => d.AvgEase.HasValue)
            .Select(d => d.AvgEase!.Value)
            .ToList();

        double retentionRate = validEaseFactors.Count != 0
            ? flashcardAlgorithmService.EstimateRetention(validEaseFactors.Average())
            : 0.0;

        UserDashboardResponse dashboardResponse =
            new UserDashboardResponse(result.UserStreaks.Count, retentionRate, deckResponses);

        return ResponseResult<UserDashboardResponse>.Success(dashboardResponse);
    }

    public async Task<ResponseResult<bool>> UpdateStreakDaily()
    {
        DateOnly yesterdayDate = DateOnly.FromDateTime(timeService.UtcNow.AddDays(-1));

        int affectedRows = await context.Users
            .Where(x => !x.UserStreaks.Contains(yesterdayDate))
            .ExecuteUpdateAsync(x => x.SetProperty(u => u.UserStreaks, []));

        return ResponseResult<bool>.Success(true);
    }
}