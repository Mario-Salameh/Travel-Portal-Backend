using Microsoft.AspNetCore.Mvc;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Repositories;

namespace TrustSystem.Backend.Endpoints;

public static class TypeOfCoverageEndpoints
{
    public static RouteGroupBuilder MapTypeOfCoverageEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/types-of-coverage")
                       .WithTags("Types of Coverage");

        group.MapGet("/", async (TypeOfCoverageRepository typeOfCoverageRepository, [FromQuery] int planType) =>
        {
            var typesOfCoverage = await typeOfCoverageRepository.GetTypesOfCoverageAsync(planType);
            return Results.Ok(typesOfCoverage.Select(v => new ViewTypeOfCoverage(v)).ToList());
        })
        .Produces<List<ViewTypeOfCoverage>>();

        return group;
    }
}
