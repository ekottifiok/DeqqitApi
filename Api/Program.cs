using System.Text.Json.Serialization;
using Api.BackgroundService;
using Api.Helpers;
using Api.Middleware;
using Api.Services;
using Core.Data;
using Core.Model;
using Core.Services;
using Core.Services.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ==================== Database Configuration ====================
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    if (!builder.Environment.IsDevelopment()) return;
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

// ==================== OpenTelemetry Configuration ====================
// builder.Services.AddOpenTelemetry()
//     .ConfigureResource(resource => resource
//         .AddService(serviceName: builder.Environment.ApplicationName))
//     .WithMetrics(metrics => metrics
//         .AddAspNetCoreInstrumentation()
//         .AddConsoleExporter((exporterOptions, metricReaderOptions) =>
//         {
//             metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
//         }));

// ==================== Background Services ====================
builder.Services.AddHostedService<DailyCleanupService>();

// ==================== Core Services Registration ====================
// Business Logic Services
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<INoteTypeService, NoteTypeService>();
builder.Services.AddScoped<IUserService, UserService>();

// Helper Services
builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<IUserBotCodeService, UserBotCodeService>();
builder.Services.AddScoped<IUserBotService, UserBotService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Template & Algorithm Services
builder.Services.AddScoped<ICssService, CssService>();
builder.Services.AddScoped<IFlashcardAlgorithmService, FlashcardAlgorithmService>();
builder.Services.AddScoped<IHtmlService, HtmlService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

// Auth & Time Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITimeService, TimeService>();

// ==================== Identity Configuration ====================
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/-._@+";
    })
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>();

// ==================== Authentication & Authorization ====================
builder.AddCustomJwtMiddleware();

// ==================== MVC & API Configuration ====================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddRouting(options => options.LowercaseUrls = true);

// ==================== OpenAPI/Swagger Configuration ====================
builder.Services.AddOpenApi();
builder.Services.AddOpenApiConfiguration(builder.Environment);

// ==================== Error Handling ====================
builder.Services.AddProblemDetails();

// ==================== Application Building ====================
WebApplication app = builder.Build();

// ==================== Middleware Pipeline ====================
// Database Initialization
app.Services.UseSeedDatabaseMiddleware();

// Configure the HTTP request pipeline
app.UseOpenApiConfiguration();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();