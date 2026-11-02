using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using TrustSystem.Backend.Endpoints;
using TrustSystem.Backend.Application.Endpoints;
using TrustSystem.Backend.Features.Policies;

// ---------------------------------- CONFIGURING SERVICES
var builder = WebApplication.CreateBuilder(args);
ServiceCollectionBuilders.Configuration = builder.Configuration;

builder.Services.AddDatabaseServices();
builder.Services.AddTravelSystemServices();
builder.Services.AddSecurityServices();
builder.Services.AddSwaggerDocumentationServices();

// ---------------------------------- CONFIGURING MIDDLEWARES
var app = builder.Build();

// AUTHENTICATION & AUTHORIZATION
app.UseCors("LanDev");
app.UseAuthentication();
app.UseAuthorization();

// ENABLE FORWARDING FOR PRODUCTION RELEASE
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// SWAGGER DOCUMENTATION
if (/*app.Environment.IsDevelopment()*/ true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger83748347";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service Nexus V1");
    });
    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/swagger83748347"),
        appBuilder =>
        {
            appBuilder.Use(async (context, next) =>
            {
                var remoteIp = context.Connection.RemoteIpAddress;

                var allowedSubnet = Microsoft.AspNetCore.HttpOverrides.IPNetwork.Parse("10.10.110.0/24");
                if (remoteIp is not null && (allowedSubnet.Contains(remoteIp) || IPAddress.IsLoopback(remoteIp)))
                    await next();
                else
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                return;
            });
        }
    );
}

// ---------------------------------- CONFIGURING ENDPOINTS
var travelSystemApi = app.MapGroup("/")
                         .WithParameterValidation()
                         .AddEndpointFilter<ExceptionEndpointFilter>();

travelSystemApi.MapAuthEndpoints();
travelSystemApi.MapContactMessagesEndpoints();
travelSystemApi.MapTypeOfCoverageEndpoints();

travelSystemApi.MapPeriodEndpoints();
travelSystemApi.MapPolicyEndpoints();
travelSystemApi.MapPlanEndpoints();

travelSystemApi.MapCurrencyRatesEndpoints();
travelSystemApi.MapAgenciesCommissionEndpoints();
travelSystemApi.MapUsersEndpoints();
travelSystemApi.MapGeneralEndpoints();

travelSystemApi.MapCompanyGroupsEndpoints();
travelSystemApi.MapCompanyBranchesEndpoints();

travelSystemApi.MapOfficesEndpoints();
travelSystemApi.MapReportsEndpoints();
travelSystemApi.MapAccountEndpoints();
travelSystemApi.MapAgencyEndpoints();

travelSystemApi.MapTravelPriceEndpoints();
travelSystemApi.MapSuperAdminEndpoints();

app.Run();