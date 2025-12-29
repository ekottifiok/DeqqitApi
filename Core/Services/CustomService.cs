// using System.ComponentModel.DataAnnotations;
// using LearningCards.Data;
// using LearningCards.Model;
// using Microsoft.EntityFrameworkCore;
//
// namespace LearningCards.NewServices;
//
// public record UserDashboardResponse(int Streak, double RetentionRate, List<UserDeckSummaryResponse> Decks);
//
// public record UserDeckSummaryResponse(int Id, string Name, DeckDueCountResponse DeckDueCount);
//
// public record DeckDueCountResponse(int Learning, int Review, int New);
//
// public record DeckStatisticsResponse(
//     int Id,
//     string Name,
//     string Description,
//     double RetentionRate,
//     DeckCardCountsResponse CardCounts,
//     List<DeckDailyStatsResponse> DailyCounts);
//
// public record DeckCardCountsResponse(int Learning, int Review, int New, int Suspended);
//
// public record DeckDailyStatsResponse(DateOnly Date, CardState CardState, int Count);
//
// public class UserService(DataContext context, IFlashcardAlgorithm flashcardAlgorithm)
// {
//     public async Task<UserDashboardResponse?> GetUserDashboard(string userId)
//     {
//         var today = DateTime.UtcNow;
//
//         // Single optimized query for all dashboard data
//         var dashboardData = await context.Users
//             .Where(u => u.Id == userId)
//             .Select(u => new
//             {
//                 StreakCount = u.UserStreaks.Count,
//                 Decks = u.Decks.Select(d => new
//                 {
//                     d.Id,
//                     d.Name,
//                     LearningCards = d.Cards.Count(c => c.State == CardState.Learning),
//                     NewCards = d.Cards.Count(c => c.State == CardState.New),
//                     ReviewDueCards = d.Cards.Count(c => 
//                         c.State == CardState.Review && c.DueDate <= today),
//                     ReviewEaseFactor = d.Cards
//                         .Where(c => c.State == CardState.Review)
//                         .Average(c => (double?)c.EaseFactor),
//                     NewCardLimit = d.Option != null 
//                         ? d.Option.NewCardsPerDay 
//                         : u.DeckOption.NewCardsPerDay,
//                     ReviewCardLimit = d.Option != null 
//                         ? d.Option.ReviewLimitPerDay 
//                         : u.DeckOption.ReviewLimitPerDay
//                 }).ToList()
//             })
//             .FirstOrDefaultAsync();
//
//         if (dashboardData == null) return null;
//
//         var deckResponses = new List<UserDeckSummaryResponse>(dashboardData.Decks.Count);
//         double totalEaseFactor = 0;
//         int decksWithReviews = 0;
//
//         foreach (var deck in dashboardData.Decks)
//         {
//             deckResponses.Add(new UserDeckSummaryResponse(
//                 deck.Id,
//                 deck.Name,
//                 new DeckDueCountResponse(
//                     deck.LearningCards,
//                     Math.Min(deck.ReviewDueCards, deck.ReviewCardLimit),
//                     Math.Min(deck.NewCards, deck.NewCardLimit)
//                 )
//             ));
//
//             if (deck.ReviewEaseFactor.HasValue)
//             {
//                 totalEaseFactor += deck.ReviewEaseFactor.Value;
//                 decksWithReviews++;
//             }
//         }
//
//         var retentionRate = decksWithReviews > 0
//             ? flashcardAlgorithm.EstimateRetention(totalEaseFactor / decksWithReviews)
//             : 0;
//
//         return new UserDashboardResponse(dashboardData.StreakCount, retentionRate, deckResponses);
//     }
//
//     public async Task UpdateUserStreaks()
//     {
//         var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
//         
//         // Find users who have study activity from yesterday
//         var activeUserIds = await context.StudySessions
//             .Where(s => s.CreatedAt.Date == DateTime.UtcNow.AddDays(-1).Date)
//             .Select(s => s.UserId)
//             .Distinct()
//             .ToListAsync();
//
//         // Update streaks for active users
//         foreach (var userId in activeUserIds)
//         {
//             await context.Users
//                 .Where(u => u.Id == userId && !u.UserStreaks.Contains(yesterday))
//                 .ExecuteUpdateAsync(setters => setters
//                     .SetProperty(u => u.UserStreaks, u => u.UserStreaks.Add(yesterday)));
//         }
//
//         // For users without activity yesterday, check if they have any recent activity
//         // and reset if they missed a day (implementation depends on streak rules)
//     }
// }
//
// public class DeckService(DataContext context, IFlashcardAlgorithm flashcardAlgorithm)
// {
//     public async Task<DeckStatisticsResponse?> GetDeckStatistics(string creatorId, int deckId)
//     {
//         var deckStats = await context.Decks
//             .Where(d => d.CreatorId == creatorId && d.Id == deckId)
//             .Select(d => new
//             {
//                 d.Id,
//                 d.Name,
//                 d.Description,
//                 LearningCards = d.Cards.Count(c => c.State == CardState.Learning),
//                 ReviewCards = d.Cards.Count(c => c.State == CardState.Review),
//                 NewCards = d.Cards.Count(c => c.State == CardState.New),
//                 SuspendedCards = d.Cards.Count(c => c.State == CardState.Suspended),
//                 ReviewEaseFactorSum = d.Cards
//                     .Where(c => c.State == CardState.Review)
//                     .Sum(c => (double?)c.EaseFactor) ?? 0,
//                 ReviewCardCount = d.Cards.Count(c => c.State == CardState.Review),
//                 DailyStats = d.DailyCounts
//                     .OrderByDescending(dc => dc.Date)
//                     .Select(dc => new DeckDailyStatsResponse(dc.Date, dc.CardState, dc.Count))
//                     .ToList()
//             })
//             .FirstOrDefaultAsync();
//
//         if (deckStats == null) return null;
//
//         var averageEaseFactor = deckStats.ReviewCardCount > 0
//             ? deckStats.ReviewEaseFactorSum / deckStats.ReviewCardCount
//             : 0;
//
//         var retentionRate = flashcardAlgorithm.EstimateRetention(averageEaseFactor);
//
//         return new DeckStatisticsResponse(
//             deckStats.Id,
//             deckStats.Name,
//             deckStats.Description ?? string.Empty,
//             retentionRate,
//             new DeckCardCountsResponse(
//                 deckStats.LearningCards,
//                 deckStats.ReviewCards,
//                 deckStats.NewCards,
//                 deckStats.SuspendedCards
//             ),
//             deckStats.DailyStats
//         );
//     }
//
//     public async Task<UserDeckSummaryResponse?> GetDeckSummary(string creatorId, int deckId)
//     {
//         var today = DateTime.UtcNow.Date;
//
//         var deckData = await context.Decks
//             .Where(d => d.CreatorId == creatorId && d.Id == deckId)
//             .Select(d => new
//             {
//                 d.Id,
//                 d.Name,
//                 LearningCards = d.Cards.Count(c => c.State == CardState.Learning),
//                 NewCards = d.Cards.Count(c => c.State == CardState.New),
//                 ReviewDueCards = d.Cards.Count(c => 
//                     c.State == CardState.Review && c.DueDate <= today),
//                 NewCardLimit = d.Option != null 
//                     ? d.Option.NewCardsPerDay 
//                     : d.Creator.DeckOption.NewCardsPerDay,
//                 ReviewCardLimit = d.Option != null 
//                     ? d.Option.ReviewLimitPerDay 
//                     : d.Creator.DeckOption.ReviewLimitPerDay
//             })
//             .FirstOrDefaultAsync();
//
//         if (deckData == null) return null;
//
//         return new UserDeckSummaryResponse(
//             deckData.Id,
//             deckData.Name,
//             new DeckDueCountResponse(
//                 deckData.LearningCards,
//                 Math.Min(deckData.ReviewDueCards, deckData.ReviewCardLimit),
//                 Math.Min(deckData.NewCards, deckData.NewCardLimit)
//             )
//         );
//     }
// }
//
// public class CardService(DataContext context)
// {
//     public async Task<int> UpdateCardState(string creatorId, int cardId, UpdateCardStateRequest request)
//     {
//         var query = context.Cards
//             .Where(c => c.Id == cardId && c.Note.CreatorId == creatorId);
//
//         return request switch
//         {
//             UpdateCardStateRequest.Suspend => await query
//                 .ExecuteUpdateAsync(setters => setters
//                     .SetProperty(c => c.State, CardState.Suspended)),
//
//             UpdateCardStateRequest.Unsuspend or UpdateCardStateRequest.Reset => await query
//                 .ExecuteUpdateAsync(setters => setters
//                     .SetProperty(c => c.State, CardState.New)
//                     .SetProperty(c => c.EaseFactor, Card.DefaultEaseFactor)
//                     .SetProperty(c => c.Interval, TimeSpan.Zero)
//                     .SetProperty(c => c.Repetitions, 0)),
//
//             _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
//         };
//     }
//
//     public async Task<Card?> GetNextStudyCard(string creatorId, int deckId)
//     {
//         var currentTime = DateTime.UtcNow;
//         var today = currentTime.Date;
//
//         var deckLimits = await context.Decks
//             .Where(d => d.Id == deckId && d.CreatorId == creatorId)
//             .Select(d => new
//             {
//                 NewCardLimit = d.Option != null 
//                     ? d.Option.NewCardsPerDay 
//                     : d.Creator.DeckOption.NewCardsPerDay,
//                 ReviewCardLimit = d.Option != null 
//                     ? d.Option.ReviewLimitPerDay 
//                     : d.Creator.DeckOption.ReviewLimitPerDay,
//                 TodayNewCards = d.DailyCounts
//                     .Where(dc => dc.CardState == CardState.New && dc.Date == DateOnly.FromDateTime(today))
//                     .Sum(dc => (int?)dc.Count) ?? 0,
//                 TodayReviews = d.DailyCounts
//                     .Where(dc => dc.CardState == CardState.Review && dc.Date == DateOnly.FromDateTime(today))
//                     .Sum(dc => (int?)dc.Count) ?? 0
//             })
//             .FirstOrDefaultAsync();
//
//         if (deckLimits == null) return null;
//
//         // Priority 1: Learning cards due today
//         var learningCard = await context.Cards
//             .Where(c => c.Note.DeckId == deckId && 
//                        c.Note.CreatorId == creatorId &&
//                        c.State == CardState.Learning &&
//                        c.DueDate <= currentTime)
//             .OrderBy(c => c.DueDate)
//             .FirstOrDefaultAsync();
//
//         if (learningCard != null) return learningCard;
//
//         // Priority 2: New cards within daily limit
//         if (deckLimits.TodayNewCards < deckLimits.NewCardLimit)
//         {
//             var newCard = await context.Cards
//                 .Where(c => c.Note.DeckId == deckId && 
//                            c.Note.CreatorId == creatorId &&
//                            c.State == CardState.New)
//                 .OrderBy(c => c.Id) // Or another deterministic ordering
//                 .FirstOrDefaultAsync();
//
//             if (newCard != null) return newCard;
//         }
//
//         // Priority 3: Review cards within daily limit
//         if (deckLimits.TodayReviews < deckLimits.ReviewCardLimit)
//         {
//             var reviewCard = await context.Cards
//                 .Where(c => c.Note.DeckId == deckId && 
//                            c.Note.CreatorId == creatorId &&
//                            c.State == CardState.Review &&
//                            c.DueDate <= currentTime)
//                 .OrderBy(c => c.DueDate)
//                 .FirstOrDefaultAsync();
//
//             if (reviewCard != null) return reviewCard;
//         }
//
//         return null;
//     }
//
//     public async Task<bool> SubmitCardReview(string creatorId, int cardId, CardReviewRequest review)
//     {
//         var card = await context.Cards
//             .Include(c => c.Note)
//             .ThenInclude(n => n.Deck)
//             .FirstOrDefaultAsync(c => c.Id == cardId && 
//                                      c.Note.CreatorId == creatorId && 
//                                      c.State != CardState.Suspended);
//
//         if (card == null) return false;
//
//         // Update card using flashcard algorithm
//         flashcardAlgorithm.UpdateCard(card, review.Quality);
//
//         // Update user streak
//         var today = DateOnly.FromDateTime(DateTime.UtcNow);
//         var user = await context.Users.FindAsync(creatorId);
//         if (user != null && !user.UserStreaks.Contains(today))
//         {
//             user.UserStreaks.Add(today);
//         }
//
//         // Update daily count
//         var deckDailyCount = await context.DailyCounts
//             .FirstOrDefaultAsync(dc => 
//                 dc.DeckId == card.Note.DeckId && 
//                 dc.CardState == card.State && 
//                 dc.Date == today);
//
//         if (deckDailyCount == null)
//         {
//             deckDailyCount = new DeckDailyCount
//             {
//                 DeckId = card.Note.DeckId,
//                 CardState = card.State,
//                 Date = today,
//                 Count = 0
//             };
//             context.DailyCounts.Add(deckDailyCount);
//         }
//
//         deckDailyCount.Count++;
//
//         await context.SaveChangesAsync();
//         return true;
//     }
// }
//
// public record CardReviewRequest
// {
//     [Required]
//     [Range(0, 5)]
//     public int Quality { get; set; }
// }
//
// public class UpdateCardRequest
// {
//     [Required] 
//     public UpdateCardStateRequest State { get; set; }
// }
//
// public enum UpdateCardStateRequest
// {
//     Suspend,
//     Unsuspend,
//     Reset
// }

