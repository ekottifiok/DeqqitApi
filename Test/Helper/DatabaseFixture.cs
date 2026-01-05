using Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Test.Helper;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly IConfigurationRoot _config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.testing.json", true, true)
        .AddEnvironmentVariables()
        .Build();

    public DbContextOptions<DataContext> DbContextOptions { get; private set; }

    public async Task InitializeAsync()
    {
        string connectionString = _config.GetConnectionString("DefaultConnection") 
                                  ?? throw new InvalidOperationException("Connection string not found");
        
        DbContextOptions = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(connectionString)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        using DataContext context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await SeedAsync(context);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public DataContext CreateContext() => new DataContext(DbContextOptions);

    protected virtual Task SeedAsync(DataContext context) 
    {
        return Task.CompletedTask;
    }
}