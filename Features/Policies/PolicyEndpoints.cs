using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Application.Interfaces;
using TrustSystem.Backend.Application.Services;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Services;
namespace TrustSystem.Backend.Features.Policies;

public static class PolicyEndpoints
{
    public static RouteGroupBuilder MapPolicyEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/policies")
                       .WithParameterValidation()
                       .WithTags("Policies");

        // CUSTOMER ENDPOINTS
        group.MapPost("/is-paid", async (APPL_INS db, [FromBody] IsPaidRequestDTO req) =>
        {
            var policies = await db.TV_POLICIES
                .Where(p => p.POL_REF_KEY == req.policyNumber && p.POL_CGP_ID == 8)
                .AsNoTracking()
                .ToListAsync();

            var policy = policies.First();

            if (policy.POL_ATR_TYPE == 3)
            {
                // Group Policy
                var parts = req.policyNumber.Split(' ');
                string OfficeId = parts[0];
                string PolicyType = parts[1];
                int Year = int.Parse(parts[2]);
                string PolicyNumber = parts[3];

                var groupPolicies = await db.TV_POLICY_PACKAGE
                .Where(p => p.TPP_OFC_ID == OfficeId &&
                p.TPP_PLT_ID == PolicyType &&
                p.TPP_YEAR == Year &&
                p.TPP_POL_NO == PolicyNumber && p.TPP_CGP_ID == 8)
                    .AsNoTracking()
                    .ToListAsync();

                var groupPolicy = groupPolicies.Single();
                return Results.Ok(new IsPaidResponseDTO
                {
                    isPaid = groupPolicy.TPP_PRINT_FLAG != 0
                });
            }

            return Results.Ok(new IsPaidResponseDTO
            {
                isPaid = policy.POL_PRINT_FLAG != 0
            });
        })
        .Produces<IsPaidResponseDTO>();

        group.MapPost("/retrieve", async (PolicyRepository policyRepository, [FromBody] RetrievePolicyRequestDTO req) =>
        {
            if (req is RetrieveSinglePolicyRequestDTO request)
            {
                var results = await policyRepository.GetSinglePolicyByRefNumberAsync(new InquirySinglePolicyRequestDTO
                {
                    passportNumber = request.passengerPassportNumber,
                    lastName = request.passengerLastNameNative
                });

                return Results.Ok(new CustomerViewSinglePolicy(results));
            }
            else if (req is RetrieveGroupPolicyRequestDTO groupRequest)
            {
                var results = await policyRepository.GetGroupPolicyByRefNumberAsync(new InquiryGroupPolicyRequestDTO
                {
                    companyName = groupRequest.companyName,
                    policyRefNumber = groupRequest.policyRefNumber
                });
                return Results.Ok(new CustomerViewGroupPolicy(results));
            }
            return Results.NotFound("Policy not found.");
        })
        .WithSummary("Retrieve Policy by it's details")
        .Produces<CustomerViewPolicy>();

        group.MapGet("/customer", async (APPL_INS db, [FromServices] PolicyService policyService) =>
        {
            var policies = await policyService.GetAllCustomerPortalPolicies();
            return Results.Ok(policies);
        })
        .WithSummary("Get Policies")
        .Produces<List<AgentViewPolicy>>();

        // AGENT ENDPOINTS
        group.MapPost("/endorse/{policyRefNumber}", async (PolicyRepository policyRepository, [FromRoute] string policyRefNumber, [FromQuery] bool justCheck) =>
        {
            var pol = await policyRepository.EndorsePolicy(policyRefNumber, justCheck);
            return Results.Ok(pol);
        })
        .Produces<CustomerViewPolicy?>();

        group.MapGet("/types", async (APPL_INS db) =>
        {
            var policyTypes = await db.PolicyTypes.Where(p => p.CompanyGroupId == 8)
                                                  .Select(p => new SelectMenuRecord
                                                  {
                                                      Code = p.Id,
                                                      Label = p.NameInEnglish
                                                  })
                                                  .AsNoTracking()
                                                  .ToListAsync();

            return Results.Ok(policyTypes);
        })
        .Produces<List<SelectMenuRecord>>();

        group.MapDelete("/delete-draft", async (APPL_INS db, [FromQuery] int cgpId) =>
        {
            var today = DateTime.UtcNow.Date;
            var policies = await db.TV_POLICIES
                .Where(p => p.POL_CGP_ID == cgpId &&
                            p.POL_PRINT_FLAG == 0 &&
                            p.POL_ISSUE_DATE < today)
                .ToListAsync();

            db.TV_POLICIES.RemoveRange(policies);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                DeletedCount = policies.Count
            });
        })
        .RequireAuthorization("admin");

        group.MapPost("/inquiry", async (
            PolicyRepository policyRepository,
            CurrentUserService currentUserService,
            ILogger<Program> Logger,
            [FromBody] InquiryPolicyRequestDTO req
        ) =>
        {
            if (req is InquirySinglePolicyRequestDTO request)
            {
                var policies = await policyRepository.GetAllSinglePolicies(request);

                if (currentUserService.Role != 2)
                {
                    await currentUserService.RetrieveUser();
                    var currentUser = currentUserService.UserInstance();
                    policies = [.. policies.Where(policy => policy.POL_OFC_ID == currentUser.CompanyOfficeId)];
                }

                // Logger.LogCritical(policies.Where(p => p.POL_REF_KEY == "A00 T01 2026 15624").Select(p => new AgentViewSinglePolicy(p)).ToList()[0].ToString());
                Logger.LogInformation("Policy inquiry returned {PolicyCount} policies.", policies.Count);
                return Results.Ok(policies.Select(p => new AgentViewSinglePolicy(p)));
            }
            else if (req is InquiryGroupPolicyRequestDTO groupRequest)
            {
                var groupPolicies = await policyRepository.GetAllGroupPolicies(groupRequest);

                if (currentUserService.Role != 2)
                {
                    await currentUserService.RetrieveUser();
                    var currentUser = currentUserService.UserInstance();

                    groupPolicies = [.. groupPolicies.Where(policy => policy.TPP_OFC_ID == currentUser.CompanyOfficeId)];
                }
                return Results.Ok(groupPolicies.Select(p => new AgentViewGroupPolicy(p)));
            }
            return Results.NotFound();
        })
        .RequireAuthorization("regular")
        .Produces<List<AgentViewPolicy>>();

        group.MapPost("/premium", async (
            PolicyRepository policyRepository,
            [FromBody] CalculatePremiumRequestDTO[] req,
            [FromQuery] double? manualDiscountPercent
        ) =>
        {
            var result = await policyRepository.CalculatePremiumAsync(req, manualDiscountPercent);
            return Results.Ok(result);
        })
        .WithSummary("Calculate Premium")
        .Produces<CalculatePremiumResponseDTO>()
        .AllowAnonymous();

        group.MapGet("/{PolicyRefNumber}", async (PolicyRepository policyRepository, APPL_INS db, [FromRoute] string PolicyRefNumber, [FromQuery] int? serial) =>
        {
            var results = await db.TV_POLICIES.Where(p => p.POL_REF_KEY == PolicyRefNumber).ToListAsync();
            if (results.Count == 0)
            {
                return Results.NotFound("Policy not found.");
            }
            if (results[0].POL_ATR_TYPE != 3)
            {
                var policy = await policyRepository.GetSinglePolicyByRefNumberAsync(new InquirySinglePolicyRequestDTO
                {
                    policyRefNumber = PolicyRefNumber,
                    serial = serial
                });
                return Results.Ok(new CustomerViewSinglePolicy(policy));
            }
            else
            {
                var policy = await policyRepository.GetGroupPolicyByRefNumberAsync(new InquiryGroupPolicyRequestDTO
                {
                    policyRefNumber = PolicyRefNumber,
                });
                return Results.Ok(new CustomerViewGroupPolicy(policy));
            }
        })
        .Produces<CustomerViewPolicy>()
        .RequireAuthorization("regular");

        group.MapPut("/", async (PolicyRepository policyRepository, [FromQuery] string policyRefNumber, [FromBody] CreatePolicyRequest req, [FromQuery] int? serial = null) =>
        {
            var policy = await policyRepository.UpdatePolicy(policyRefNumber, req, serial);
            return Results.Ok(policy);
        })
        .WithSummary("Update Policy")
        .Produces<CustomerViewPolicy>();

        // MUTUAL ENDPOINTS
        group.MapPost("/", async (PolicyRepository policyRepository, [FromBody] CreatePolicyRequest req, CurrentUserService currentUserService) =>
        {
            var result = await policyRepository.SavePolicy(req);
            return Results.Ok(result);
        })
        .WithSummary("Insert New Policy")
        .Produces<CustomerViewPolicy>();

        return group;
    }
}