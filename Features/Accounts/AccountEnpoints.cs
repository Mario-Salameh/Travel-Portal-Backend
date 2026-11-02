using Microsoft.AspNetCore.Mvc;
using TrustSystem.Backend.Contracts;
using TrustSystem.Backend.Features.Accounts;

namespace TrustSystem.Backend.Endpoints;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/accounts")
                       .WithTags("Accounts");

        group.MapGet("/", async (AccountRepository _repo, [FromQuery] string accountCode = "") =>
        {
            var accounts = await _repo.GetAllAccountsAsync(accountCode);
            return Results.Ok(accounts.Select(v => new ViewAccount(v)).ToList());
        })
        .Produces<List<ViewAccount>>()
        .RequireAuthorization("admin");

        group.MapPost("/", async (AccountRepository _repo, [FromBody] CreateAccountRequest createDto) =>
        {
            await _repo.SaveAccountAsync(createDto);
            return Results.Ok();
        })
        .RequireAuthorization("admin");

        group.MapPut("/", async (AccountRepository _repo, [FromBody] UpdateAccountRequest updateDto) =>
        {
            await _repo.UpdateAccountAsync(updateDto);
            return Results.NoContent();
        })
        .RequireAuthorization("admin");

        group.MapDelete("/{accountCode}", async (AccountRepository _repo, [FromRoute] string accountCode) =>
        {
            await _repo.DeleteAccountAsync(accountCode);
            return Results.NoContent();
        })
        .RequireAuthorization("admin");

        return group;
    }
}
