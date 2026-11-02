
using Microsoft.AspNetCore.Mvc;
using TrustSystem.Backend.Infrastructure.Repositories;

public static class PolicyEndpoints
{
    public static RouteGroupBuilder MapSuperAdminEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/companies")
                       .RequireAuthorization("super admin")
                       .WithParameterValidation()
                       .WithTags("Companies Management");

        group.MapGet("/", async (SuperAdminRepository controlPanelRepository) =>
        {
            var companies = await controlPanelRepository.GetCompaniesAsync();
            return Results.Ok(companies);
        })
        .Produces<List<CompanyInfo>>();

        group.MapPut("/", async (SuperAdminRepository controlPanelRepository, [FromBody] UpdateCompanyRequest company) =>
        {
            await controlPanelRepository.UpdateCompanyAsync(company);
            return Results.NoContent();
        });

        return group;
    }
}