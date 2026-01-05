using Core.Data;
using Core.Services;
using Core.Services.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramBot.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        IConfiguration config = context.Configuration;
        string connectionString = config.GetConnectionString("DefaultConnection")
                                  ?? throw new InvalidOperationException("Connection string not found");

        // Database
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(connectionString)
                .EnableSensitiveDataLogging());

        // Core Services
        services.AddScoped<IFlashcardAlgorithmService, FlashcardAlgorithmService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<ITimeService, TimeService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IDeckService, DeckService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IUserBotCodeService, UserBotCodeService>();
        services.AddScoped<IUserBotService, UserBotService>();
        services.AddScoped<IUserService, UserService>();


        // Bot Services
        services.AddSingleton<BotService>();
        services.AddScoped<UpdateHandler>();
        services.AddScoped<AuthHandler>();
        services.AddScoped<DeckHandler>();
        services.AddScoped<StudyHandler>();

        // Hosted Service
        services.AddHostedService(provider => provider.GetRequiredService<BotService>());
    })
    .Build();

await host.RunAsync();