using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Endpoints;

public static class ContactMessageEndpoints
{
    public static RouteGroupBuilder MapContactMessagesEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/contact-messages")
                       .WithTags("Contact Messages");

        group.MapPost("/", async (APPL_CORE db, CreateContactMessageRequest request) =>
        {
            var contactMessage = new ContactMessage
            {
                FullName = request.fullName,
                EmailAddress = request.emailAddress,
                PhoneNumber = request.phoneNumber,
                Message = request.message,
                SubmissionDate = DateTime.UtcNow
            };

            db.ContactMessages.Add(contactMessage);
            await db.SaveChangesAsync();

            return Results.Ok();
        });

        group.MapPut("/{messageId}", async (APPL_CORE db, [FromRoute] int messageId) =>
        {
            var contactMessage = await db.ContactMessages
                .Where(m => m.MessageId == messageId).ToListAsync();

            if (contactMessage.Count != 1)
                return Results.NotFound();

            contactMessage[0].IsRead = 1;
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization("admin");

        group.MapPut("/", async (APPL_CORE db) =>
        {
            var contactMessages = await db.ContactMessages
                .Where(m => m.IsRead == 0).ToListAsync();

            if (contactMessages.Count < 1)
                return Results.NoContent();

            foreach (var msg in contactMessages)
            {
                msg.IsRead = 1;
            }

            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization("admin");

        group.MapGet("/", async (APPL_CORE db, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        {
            var totalCount = await db.ContactMessages
            .CountAsync();

            var sql = @"
            SELECT * FROM (
                SELECT t.*, ROWNUM rnum
                FROM (
                    SELECT
                    MESSAGE_ID,
                    FULL_NAME,
                    EMAIL_ADDRESS,
                    PHONE_NUMBER,
                    MESSAGE,
                    SUBMISSION_DATE,
                    IS_READ

                    FROM TV_CONTACT_MESSAGES
                    ORDER BY MESSAGE_ID DESC
                ) t
                WHERE ROWNUM <= :maxRow
            )
            WHERE rnum > :minRow";

            int minRow = (page - 1) * pageSize;
            int maxRow = page * pageSize;

            var contactMessages = await db.ContactMessages
            .FromSqlRaw(sql, new OracleParameter("maxRow", maxRow),
                            new OracleParameter("minRow", minRow))
            .AsNoTracking()
            .ToListAsync();

            return Results.Ok(new AgentViewContactMessages(contactMessages, totalCount));
        })
        .WithSummary("Get Contact Messages")
        .RequireAuthorization("admin")
        .Produces<AgentViewContactMessages>();

        return group;
    }
}
