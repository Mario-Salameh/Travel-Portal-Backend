using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Application.Interfaces;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Endpoints;

public static class OfficesEndpoints
{
    public static RouteGroupBuilder MapOfficesEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/offices")
                       .WithTags("Offices");

        group.MapPost("/", async (APPL_CORE db, CurrentUserService currentUserService, [FromBody] CreateOfficeRequest request) =>
        {
            var office = new CompanyOffice
            {
                Id = request.OfficeId,
                CompanyBranchId = "F",
                CompanyId = 1,
                CompanyGroupId = currentUserService.CompanyGroupId,
                NameInEnglish = request.NameInEnglish,
                NameInArabic = request.NameInArabic,

                CreatedBy = currentUserService.UserCode,
                CreationDate = DateTime.UtcNow,

                BrokerSltId = request.BrokerSltId,
                BrokerCode = request.BrokerCode,
                SpecialDiscount = request.SpecialDiscount,
            };

            db.CR_OFFICES.Add(office);
            await db.SaveChangesAsync();

            return Results.Created();
        })
        .RequireAuthorization("admin");

        group.MapPut("/{officeId}", async (APPL_CORE db, CurrentUserService currentUserService, [FromRoute] string officeId, [FromBody] UpdateOfficeRequest request) =>
        {
            var offices = await db.CR_OFFICES
                .Where(e => e.Id == officeId && e.CompanyGroupId == currentUserService.CompanyGroupId).ToListAsync();

            if (offices.Count != 1)
                return Results.NotFound();

            var office = offices[0];
            if (request.NameInEnglish != null)
                office.NameInEnglish = request.NameInEnglish;

            if (request.NameInArabic != null)
                office.NameInArabic = request.NameInArabic;

            if (request.BrokerSltId.HasValue)
                office.BrokerSltId = request.BrokerSltId.Value;

            if (request.BrokerCode != null)
                office.BrokerCode = request.BrokerCode;

            if (request.SpecialDiscount.HasValue)
                office.SpecialDiscount = request.SpecialDiscount.Value;

            office.ModifiedBy = currentUserService.UserCode;
            office.ModificationDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok();
        })
        .RequireAuthorization("admin");

        group.MapGet("/", async (APPL_CORE db, CurrentUserService currentUserService) =>
        {
            var offices = await db.CR_OFFICES
                .Where(e => e.CompanyGroupId == currentUserService.CompanyGroupId)
                .Select(e => new ViewOffice(e))
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(offices);
        })
        .RequireAuthorization("admin")
        .Produces<List<ViewOffice>>();

        group.MapGet("/records", async (APPL_CORE db, CurrentUserService currentUserService) =>
        {
            if (currentUserService.Role == 2)
            {
                var offices = await db.CR_OFFICES
                .Where(e => e.CompanyGroupId == 8)
                .Select(e => new SelectMenuRecord
                {
                    Code = e.Id,
                    Label = e.NameInEnglish
                })
                .AsNoTracking()
                .ToListAsync();

                return Results.Ok(offices);

            }
            else
            {
                await currentUserService.RetrieveUser();
                User UserInstance = currentUserService.UserInstance();

                return Results.Ok(new List<SelectMenuRecord>
                {
                    new() {
                        Code = UserInstance.CompanyOfficeId!,
                        Label = UserInstance.Office!.NameInEnglish
                    }
                });
            }
        })
        .Produces<List<SelectMenuRecord>>()
        .RequireAuthorization("regular");

        group.MapDelete("/{officeId}", async (APPL_CORE db, CurrentUserService currentUserService, [FromRoute] string officeId) =>
        {
            var offices = await db.CR_OFFICES
                .Where(e => e.Id == officeId && e.CompanyGroupId == currentUserService.CompanyGroupId)
                .ToListAsync();

            if (offices.Count != 1)
                return Results.NotFound();

            db.CR_OFFICES.Remove(offices[0]);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireAuthorization("admin");

        return group;
    }
}
