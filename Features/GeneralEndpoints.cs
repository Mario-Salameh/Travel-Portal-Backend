using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Application.Interfaces;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Features.Accounts;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Application.Endpoints;

public static class GeneralEndpoints
{
    public static RouteGroupBuilder MapGeneralEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/general")
                       .WithParameterValidation()
                       .WithTags("General");

        group.MapGet("/countries", async (APPL_CORE db) =>
        {
            var countries = await db.CR_COUNTRIES.AsNoTracking()
                .ToListAsync();
            return Results.Ok(countries);
        })
        .Produces<List<Country>>();

        group.MapGet("/accounts-list", async (AccountRepository _repo, CurrentUserService currentUser, [FromQuery] string accountCode = "") =>
        {
            var accounts = await _repo.GetAllAccountsAsync(accountCode);
            return Results.Ok(accounts.Select(v => new SelectMenuRecord { Code = v.Code, Label = v.NameInEnglish }).ToList());
        })
        .Produces<List<SelectMenuRecord>>()
        .RequireAuthorization("admin");

        return group;
    }
}