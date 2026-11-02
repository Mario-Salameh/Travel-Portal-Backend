using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Endpoints;

public static class CurrencyRatesEndpoints
{
    public static RouteGroupBuilder MapCurrencyRatesEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/currency-rates")
                       .WithTags("Currency Rates")
                       .RequireAuthorization("admin");

        group.MapPost("/", async (APPL_CORE db, CurrentUserService currentUserService, [FromQuery] int cgpId, [FromBody] CreateCurrencyRateDTO request) =>
        {
            var currencyRate = new CurrencyRate
            {
                Id = db.CR_CURRENCY_RATES.Where(e => e.CompanyGroupId == cgpId &&
                                                     e.BaseCurrencyCode == "31" &&
                                                     e.CurrencyCode == "00").Max(e => e.Id) + 1,
                ExchangeRate = request.exchangeRate,
                StartDate = request.startingDate.ToDateTime(TimeOnly.MinValue),
                CompanyGroupId = cgpId,

                CreatedBy = currentUserService.UserCode,
                CreationDate = DateTime.UtcNow
            };

            await db.CR_CURRENCY_RATES.AddAsync(currencyRate);
            await db.SaveChangesAsync();

            return Results.Ok(new CurrencyRateViewDTO(currencyRate));
        })
        .Produces<CurrencyRateViewDTO>();

        group.MapGet("/", async (APPL_CORE db, [FromQuery] int cgpId) =>
        {
            var currencyRates = await db.CR_CURRENCY_RATES
                .Where(e => e.CompanyGroupId == cgpId && e.CurrencyCode == "00"
                                                  && e.BaseCurrencyCode == "31"
                                                  && (e.StartDate.Year == DateTime.UtcNow.Year || e.StartDate.Year == DateTime.UtcNow.Year - 1))
                .Select(e => new CurrencyRateViewDTO(e))
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(currencyRates);
        })
        .Produces<List<CurrencyRateViewDTO>>();

        group.MapDelete("/{crrId}", async (APPL_CORE db, [FromRoute] int crrId, [FromQuery] int cgpId) =>
        {
            var currencyRatesList = await db.CR_CURRENCY_RATES
                .Where(e => e.Id == crrId &&
                            e.CompanyGroupId == cgpId &&
                            e.BaseCurrencyCode == "31" &&
                            e.CurrencyCode == "00")
                .ToListAsync();

            if (currencyRatesList.Count == 0)
                return Results.NotFound();
            else if (currencyRatesList.Count > 1)
                return Results.InternalServerError();

            db.CR_CURRENCY_RATES.RemoveRange(currencyRatesList);
            await db.SaveChangesAsync();

            return Results.Ok();
        });

        return group;
    }
}
