using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TrustSystem.Backend.Application.Reports;
using TrustSystem.Backend.Application.Services;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Features.Accounts;
using TrustSystem.Backend.Features.Policies;
using TrustSystem.Backend.Infrastructure.Repositories;
using TrustSystem.Backend.Repositories;
using TrustSystem.Backend.Services;

public static class ServiceCollectionBuilders
{
    public static IConfiguration Configuration = new ConfigurationBuilder().Build();
    public static IServiceCollection AddDatabaseServices(this IServiceCollection Services)
    {
        Services.AddOracle<APPL_CORE>(Configuration.GetConnectionString("APPL_CORE"));
        Services.AddOracle<APPL_INS>(Configuration.GetConnectionString("APPL_INS"));
        return Services;
    }

    public static IServiceCollection AddTravelSystemServices(this IServiceCollection Services)
    {
        // Repositories
        Services.AddScoped<PolicyRepository>();
        Services.AddScoped<AccountRepository>();
        Services.AddScoped<AgencyCommissionRepository>();
        Services.AddScoped<TravelPriceRepository>();
        Services.AddScoped<TypeOfCoverageRepository>();
        Services.AddScoped<UserRepository>();
        Services.AddScoped<SuperAdminRepository>();

        // Services
        Services.AddScoped<PolicyService>();
        Services.AddScoped<PolicyDocumentService>();
        Services.AddScoped<CSVService>();

        return Services;
    }

    public static IServiceCollection AddSecurityServices(this IServiceCollection Services)
    {
        Services.AddHttpContextAccessor();
        Services.AddScoped<CurrentUserService>();

        Services.AddScoped<JwtService>();
        Services.AddCors(options =>
        {
            options.AddPolicy("LanDev", policy =>
            {
                policy
                    .SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrWhiteSpace(origin))
                            return false;

                        var uri = new Uri(origin);

                        return
                            uri.Host == "localhost" ||
                            uri.Host == "127.0.0.1" ||
                            uri.Host.StartsWith("192.168.") ||
                            uri.Host.StartsWith("10.");
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!))
            };
        });
        Services.AddAuthorizationBuilder()
            .AddPolicy("super admin", policy =>
                policy.RequireRole("0"))
            .AddPolicy("admin", policy =>
                policy.RequireRole("2", "0"))
            .AddPolicy("regular", policy =>
                policy.RequireRole("2", "1", "0"));

        return Services;
    }

    public static IServiceCollection AddSwaggerDocumentationServices(this IServiceCollection Services)
    {
        Services.AddEndpointsApiExplorer();
        Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Service Nexus REST API",
                Version = "v1",
                Description = File.ReadAllText("Core/Docs/swagger-description.txt"),
                License = new OpenApiLicense { Name = "INTERNAL USE ONLY" }
            });

            c.UseOneOfForPolymorphism();
        });

        return Services;
    }
}