using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Middleware;

public static class JwtMiddleware
{
    public static void AddCustomJwtMiddleware(this WebApplicationBuilder builder)
    {
        ConfigurationManager configuration = builder.Configuration;
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = configuration["JWT:ValidAudience"] ??
                                    throw new Exception("Valid Audience JWT Not Found"),
                    ValidIssuer = configuration["JWT:ValidIssuer"] ?? throw new Exception("Valid Issuer JWT Not Found"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? throw new InvalidOperationException()))
                };
            });
    }
}