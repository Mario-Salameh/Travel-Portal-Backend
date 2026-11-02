using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Endpoints;

public static class CompanyGroupsEndpoints
{
    public static RouteGroupBuilder MapCompanyGroupsEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/company-groups")
                       .WithTags("Company Groups")
                       .RequireAuthorization("admin");

        group.MapGet("/", async (APPL_CORE db, [FromQuery] int cgpId) =>
        {
            var users = await db.CR_COMP_GROUPS
                .Where(e => e.Id == cgpId)
                .AsNoTracking()
                .Select(e => new CompanyGroupViewDTO(e))
                .ToListAsync();

            return Results.Ok(users);
        });

        return group;
    }
}
