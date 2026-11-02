using Microsoft.AspNetCore.Mvc;
using TrustSystem.Backend.Contracts;
using TrustSystem.Backend.Repositories;

namespace TrustSystem.Backend.Endpoints;

public static class AgenciesCommissionEndpoints
{
    public static RouteGroupBuilder MapAgenciesCommissionEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/agencies-commission")
                       .WithTags("Agencies Commission")
                       .RequireAuthorization("admin");

        group.MapGet("/", async (AgencyCommissionRepository repository) =>
        {
            var agenciesCommissions = await repository.GetAllAgencyCommissionsAsync();
            return Results.Ok(agenciesCommissions.Select(v => new ViewAgencyCommission(v)));
        })
        .Produces<List<ViewAgencyCommission>>();

        group.MapPost("/", async (AgencyCommissionRepository repository, CreateAgencyCommissionRequest request) =>
        {
            await repository.SaveAgencyCommissionAsync(request);
            return Results.Ok();
        });

        group.MapPut("/", async (AgencyCommissionRepository repository, [FromBody] UpdateAgencyCommissionRequest request) =>
        {
            await repository.UpdateAgencyCommissionAsync(request);
            return Results.NoContent();
        });

        group.MapDelete("/{brokerCode}", async (AgencyCommissionRepository repository, [FromRoute] string brokerCode) =>
        {
            await repository.DeleteAgencyCommissionAsync(brokerCode);
            return Results.NoContent();
        });

        return group;
    }
}
