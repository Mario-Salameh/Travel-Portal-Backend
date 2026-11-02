using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;

namespace TrustSystem.Backend.Endpoints;

public static class PeriodEndpoints
{
    public static RouteGroupBuilder MapPeriodEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/periods")
                       .WithTags("Periods");

        group.MapGet("/", async (APPL_CORE db, [FromQuery] int atrType) =>
        {
            var periods = await db.CR_DOMAIN_VALUES
                .Where(e => e.DomainId == PlanMapper.periods[atrType] && e.CompanyGroupId == 8)
                .Select(t => new PeriodResponseDTO(t))
                .AsNoTracking()
                .ToListAsync();

            if (periods.Count == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok(periods);
        })
        .Produces<List<PeriodResponseDTO>>(StatusCodes.Status200OK);

        return group;
    }
}

class PlanMapper
{
    public static Dictionary<int, int> periods = new()
    {
        { 1, 500 },
        { 3, 500 },
        { 8, 560 },
        { 11, 570 },
    };
}
