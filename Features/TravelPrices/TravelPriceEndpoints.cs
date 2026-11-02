using Microsoft.AspNetCore.Mvc;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Repositories;

namespace TrustSystem.Backend.Endpoints;

public static class TravelPriceEndpoints
{
    public static RouteGroupBuilder MapTravelPriceEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/travel-prices")
                       .WithTags("Travel Prices")
                       .RequireAuthorization("admin");

        group.MapGet("/", async (TravelPriceRepository travelPriceRepository) =>
        {
            var travelPrices = await travelPriceRepository.GetTravelPricesAsync();

            return Results.Ok(travelPrices.Select(tp => new ViewTravelPrice(tp)).ToList());
        })
        .Produces<List<ViewTravelPrice>>();

        group.MapPost("/", async (TravelPriceRepository travelPriceRepository, CreateTravelPriceRequest request) =>
        {
            await travelPriceRepository.SaveTravelPriceAsync(request);
            return Results.Ok();
        });

        group.MapPut("/", async (TravelPriceRepository travelPriceRepository,
                                 [FromQuery] int planType,
                                 [FromQuery] int typeOfCoverage,
                                 [FromQuery] int period,
                                 [FromQuery] int currentAgeFrom,
                                 [FromQuery] int currentAgeTo,
                                 [FromBody] UpdateTravelPriceRequest request) =>
        {
            await travelPriceRepository.UpdateTravelPriceAsync(planType, typeOfCoverage, period, currentAgeFrom, currentAgeTo, request);
            return Results.NoContent();
        });

        group.MapDelete("/", async (TravelPriceRepository travelPriceRepository,
                                   [FromQuery] int planType,
                                   [FromQuery] int typeOfCoverage,
                                   [FromQuery] int period,
                                   [FromQuery] int AgeFrom,
                                   [FromQuery] int AgeTo) =>
        {
            await travelPriceRepository.DeleteTravelPriceAsync(planType, typeOfCoverage, period, AgeFrom, AgeTo);
            return Results.NoContent();
        });

        return group;
    }
}
