using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Endpoints;

public static class AgencyEndpoints
{
    public static RouteGroupBuilder MapAgencyEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/agencies")
                       .WithTags("Agencies")
                       .RequireAuthorization("admin");

        group.MapGet("/", async (APPL_CORE db) =>
        {
            var agencies = await db.CR_DOMAIN_VALUES
                .Where(a => a.CompanyGroupId == 8 && a.DomainId == 510)
                .Select(a => new ViewAgency(a))
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(agencies);
        })
        .Produces<List<ViewAgency>>();

        group.MapPost("/", async (APPL_CORE db, CurrentUserService currentUserService, [FromBody] CreateAgencyRequest createDto) =>
        {
            var newAgency = new DomainValue
            {
                CompanyGroupId = 8,
                SystemId = 3,
                DomainId = 510,


                Id = db.CR_DOMAIN_VALUES.Where(d => d.DomainId == 510 && d.CompanyGroupId == 8)
                                            .Max(d => Convert.ToInt32(d.Id)) + 1 + "",
                NameInEnglish = createDto.agencyNameNative,
                NameInArabic = createDto.agencyNameForiegn,
                BrokerCode = createDto.agencyAccountCode,

                CreatedBy = currentUserService.UserCode,
                CreationDate = DateTime.UtcNow
            };

            db.CR_DOMAIN_VALUES.Add(newAgency);
            await db.SaveChangesAsync();

            return Results.Ok();
        });

        group.MapPut("/{agencyCode}", async (APPL_CORE db, CurrentUserService currentUserService, [FromRoute] string agencyCode, [FromBody] UpdateAgencyRequest updateDto) =>
        {
            var agencies = await db.CR_DOMAIN_VALUES.Where(a => a.DomainId == 510 && a.CompanyGroupId == 8 && a.Id == agencyCode).ToListAsync();
            if (!agencies.Any())
                return Results.NotFound();

            var agency = agencies[0];
            if (updateDto.agencyNameNative != null)
                agency.NameInEnglish = updateDto.agencyNameNative;

            if (updateDto.agencyNameForiegn != null)
                agency.NameInArabic = updateDto.agencyNameForiegn;

            if (updateDto.agencyAccountCode != null)
                agency.BrokerCode = updateDto.agencyAccountCode;

            agency.ModifiedBy = currentUserService.UserCode;
            agency.ModificationDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok();
        });

        group.MapDelete("/{agencyCode}", async (APPL_CORE db, ILogger<Program> Logger, [FromRoute] string agencyCode) =>
        {
            var agencies = await db.CR_DOMAIN_VALUES.Where(a => a.DomainId == 510 && a.CompanyGroupId == 8 && a.Id == agencyCode).ToListAsync();
            if (!agencies.Any())
                return Results.NotFound();

            foreach (var agency in agencies)
            {
                Logger.LogWarning("{agenciesCount}", agencies.Count);
                Logger.LogWarning("Deleting agency with ID: {AgencyId}", agency.Id);
            }

            db.CR_DOMAIN_VALUES.Remove(agencies[0]);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return group;
    }
}
