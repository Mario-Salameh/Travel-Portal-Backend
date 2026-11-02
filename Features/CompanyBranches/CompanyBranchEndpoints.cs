using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;

namespace TrustSystem.Backend.Endpoints;

public static class CompanyBranchesEndpoints
{
    public static RouteGroupBuilder MapCompanyBranchesEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/company-branches")
                       .WithTags("Company Branches")
                       .RequireAuthorization("admin");

        group.MapGet("/", async (APPL_CORE db, [FromQuery] int cgpId) =>
        {
            var users = await db.CR_COMPANY_BRANCHES
                .Where(e => e.CompanyGroupId == cgpId)
                .Select(e => new CompanyBranchViewDTO(e))
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(users);
        });

        return group;
    }
}
