using Core.Services.Interface;

namespace Api.BackgroundService;

public class DailyCleanupService(IServiceProvider serviceProvider, ILogger<DailyCleanupService> logger)
    : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Calculate the delay until the next execution (e.g., Midnight)
            DateTime now = DateTime.UtcNow;
            DateTime nextRunTime = now.Date.AddDays(1); // Next midnight
            TimeSpan delay = nextRunTime - now;

            logger.LogInformation("Scheduled next run in {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);

                // 2. Perform the task
                using IServiceScope scope = serviceProvider.CreateScope();
                IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.UpdateStreakDaily();

                logger.LogInformation("Daily task completed successfully at {Time}", DateTime.UtcNow);
            }
            catch (OperationCanceledException)
            {
                /* Shutting down */
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during daily task execution");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retry
            }
        }
    }
}