using System.Text.Json.Serialization;
using Api.BackgroundService;
using Api.Helpers;
using Api.Middleware;
using Core.Data;
using Core.Model;
using Core.Services;
using Core.Services.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<DataContext>(options =>
    options.EnableSensitiveDataLogging().UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHostedService<DailyCleanupService>();

// Add services to the container.
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<INoteTypeService, NoteTypeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<ICssService, CssService>();
builder.Services.AddScoped<IHtmlService, HtmlService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IFlashcardAlgorithmService, FlashcardAlgorithmService>();


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/-._@+";
    })
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>();
builder.AddCustomJwtMiddleware();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApiConfiguration(builder.Environment);
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseOpenApiConfiguration();

// app.Services.UseSeedDatabaseMiddleware();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();