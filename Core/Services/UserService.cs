using Core.Data;
using Core.Dto.Deck;
using Core.Dto.User;
using Core.Model;
using Core.Model.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

// Use a datetimeprovider for better testing
public class UserService(DataContext context, IFlashcardAlgorithmService flashcardAlgorithmService) : IUserService
{
    public async Task<int> UpdateProfileImage(string id, string profileImage)
    {
        return await context.Users
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.ProfileImageUrl, profileImage));
    }

    public async Task<UserResponse?> Get(string id)
    {
        return await context.Users.Where(x => x.Id == id).Select(x => (UserResponse)x).FirstOrDefaultAsync();
    }

    public async Task<int> Update(string id, UpdateUserRequest request)
    {
        User? user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return 0;
        user.AiProviders = request.AiProviders.Select(provider => (UserAiProvider)provider).ToList();
        user.DeckOption =  request.DeckOption;
        await context.SaveChangesAsync();
        return 1;
    }

    public async Task<UserDashboardResponse?> GetUserDashboard(string userId)
    {
        DateTime today = DateTime.UtcNow;

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

        if (result == null) return null;

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

        double retentionRate = validEaseFactors.Any()
            ? flashcardAlgorithmService.EstimateRetention(validEaseFactors.Average())
            : 0.0;

        return new UserDashboardResponse(result.UserStreaks.Count, retentionRate, deckResponses);
    }

    // TODO: Maybe store timezone for more accurate value
    public async Task UpdateStreakDaily()
    {
        DateOnly yesterdayDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        await context.Users
            .Where(x => !x.UserStreaks.Contains(yesterdayDate))
            .ExecuteUpdateAsync(x => x.SetProperty(u => u.UserStreaks, []));
    }
}