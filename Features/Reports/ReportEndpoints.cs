using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Application.Enums;
using TrustSystem.Backend.Application.Reports;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Infrastructure.Repositories;
using TrustSystem.Backend.Services;

public static class ReportEndpoints
{
    public static RouteGroupBuilder MapReportsEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/reports")
                       .WithParameterValidation()
                       .WithTags("Reports");

        group.MapGet("/travel-data", async (APPL_INS db, CSVService cSVService, [FromQuery] int cgpId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate) =>
        {
            var policies = await db.TV_POLICIES
                .Where(p => p.POL_CGP_ID == cgpId && p.POL_CREATION_DATE >= fromDate.ToDateTime(TimeOnly.MinValue) && p.POL_CREATION_DATE <= toDate.ToDateTime(TimeOnly.MinValue))
                .Include(p => p.TypeOfCoverage)
                            .Include(p => p.CompanyOffice)
                            .Include(p => p.PaymentAccount)
                            .Include(p => p.DestinationCountry)
                            .Include(p => p.CreatedBy)
                            .AsNoTracking()
                            .OrderByDescending(p => p.POL_CREATION_DATE)
                            .ToListAsync();

            if (policies == null || policies.Count == 0)
                return Results.NotFound("No policies found for the given criteria.");

            var policyTypes = await db.PolicyTypes.Where(p => p.CompanyGroupId == 8)
                                                  .AsNoTracking()
                                                  .ToDictionaryAsync(p => int.Parse(p.Id), p => p.NameInEnglish);

            var travelPrices = await db.TravelPrices.Where(p => p.CompanyGroupId == 8)
                                                    .AsNoTracking()
                                                    .ToListAsync();

            return cSVService.BuildFileResult(policies, "Policies Report.csv",
                ("Office Name", p => p.CompanyOffice?.NameInEnglish ?? "N/A"),
                ("Policy Type", p => policyTypes.GetValueOrDefault(p.POL_ATR_TYPE, "Unknown")),
                ("Voucher number", p => p.POL_VHH_NUM),
                ("Policy No", p => p.POL_REF_KEY),
                ("Serial", p => p.POL_INT_SERIAL),
                ("Name / English", p => $"{p.POL_PASSEGER_1ST_NAME_NTV} {p.POL_PASSEGER_LAST_NAME_NTV}"),
                ("Name / Arabic", p => $"{p.POL_PASSEGER_1ST_NAME_FRN} {p.POL_PASSEGER_LAST_NAME_FRN}"),
                ("Birth Date", p => p.POL_PASSEGER_BIRTH_DATE.ToString("dd/MM/yyyy")),
                ("Passport No", p => p.POL_PASSEGER_PASSPORT_NUMBER),
                ("Issue Date", p => p.POL_ISSUE_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                ("Destination", p => p.DestinationCountry!.NameInEnglish),
                ("Inc Date", p => p.POL_INCEPTION_DATE.ToString("dd/MM/yyyy")),
                ("Period", p => p.POL_PERIOD),
                ("Gross Premium", p => p.POL_PREMIUM_AMOUNT),
                ("Reinsurance Amount", p =>
                {
                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                    return travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                               tp.Duration == p.POL_PERIOD &&
                                                               tp.PlanType == fixedPlanType &&
                                                               tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                               tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                               tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                }
            ),
                ("Agent Comm", p =>
                {
                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                    var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                 tp.Duration == p.POL_PERIOD &&
                                                                 tp.PlanType == fixedPlanType &&
                                                                 tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                 tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                 tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                    return Math.Round(ri * 0.05, 2);
                }
            ),
                ("Type of Coverage", p => p.TypeOfCoverage!.NameInEnglish),
                ("RI Basic Prem", p =>
                {
                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                    var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                 tp.Duration == p.POL_PERIOD &&
                                                                 tp.PlanType == fixedPlanType &&
                                                                 tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                 tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                 tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                    return ri - Math.Round(ri * 0.05, 2);
                }
            ),
                ("User", p => p.CreatedBy!.NameInEnglish),
                ("Payment Account", p => $"{p.POL_TA_CODE} - {p.PaymentAccount?.NameInEnglish ?? "N/A"}")
            );
        })
        .RequireAuthorization("admin")
        .Produces<FileContentResult>(200, "text/csv");

        group.MapGet("/insured-clients", async (APPL_INS db, CSVService cSVService, [FromQuery] int cgpId) =>
        {
            var policyTypes = await db.PolicyTypes.Where(p => p.CompanyGroupId == 8)
                                                  .AsNoTracking()
                                                  .ToDictionaryAsync(p => int.Parse(p.Id), p => p.NameInEnglish);
            var travelPrices = await db.TravelPrices.Where(p => p.CompanyGroupId == 8)
                                                    .AsNoTracking()
                                                    .ToListAsync();

            var policies = await db.TV_POLICIES
                .Where(p => p.POL_CGP_ID == cgpId && DateTime.Now >= p.POL_INCEPTION_DATE && DateTime.Now <= p.POL_EXPIRY_DATE)
                .Include(p => p.TypeOfCoverage)
                            .Include(p => p.CompanyOffice)
                            .Include(p => p.PaymentAccount)
                            .Include(p => p.DestinationCountry)
                            .Include(p => p.CreatedBy)
                            .AsNoTracking()
                            .OrderByDescending(p => p.POL_CREATION_DATE)
                            .ToListAsync();

            if (policies == null || policies.Count == 0)
                return Results.NotFound("No policies found for the given criteria.");

            return cSVService.BuildFileResult(policies, "Policies Report.csv",
                ("Office Name", p => p.CompanyOffice?.NameInEnglish ?? "N/A"),
                ("Policy Type", p => policyTypes.GetValueOrDefault(p.POL_ATR_TYPE, "Unknown")),
                ("Voucher number", p => p.POL_VHH_NUM),
                ("Policy No", p => p.POL_REF_KEY),
                ("Serial", p => p.POL_INT_SERIAL),
                ("Name / English", p => $"{p.POL_PASSEGER_1ST_NAME_NTV} {p.POL_PASSEGER_LAST_NAME_NTV}"),
                ("Name / Arabic", p => $"{p.POL_PASSEGER_1ST_NAME_FRN} {p.POL_PASSEGER_LAST_NAME_FRN}"),
                ("Birth Date", p => p.POL_PASSEGER_BIRTH_DATE.ToString("dd/MM/yyyy")),
                ("Passport No", p => p.POL_PASSEGER_PASSPORT_NUMBER),
                ("Issue Date", p => p.POL_ISSUE_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                ("Destination", p => p.DestinationCountry!.NameInEnglish),
                ("Inc Date", p => p.POL_INCEPTION_DATE.ToString("dd/MM/yyyy")),
                ("Period", p => p.POL_PERIOD),
                ("Gross Premium", p => p.POL_PREMIUM_AMOUNT),
                ("Reinsurance Amount", p =>
                {
                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                    return travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                               tp.Duration == p.POL_PERIOD &&
                                                               tp.PlanType == fixedPlanType &&
                                                               tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                               tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                               tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                }
            ),
                ("Agent Comm", p =>
                {
                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                    var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                 tp.Duration == p.POL_PERIOD &&
                                                                 tp.PlanType == fixedPlanType &&
                                                                 tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                 tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                 tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                    return Math.Round(ri * 0.05, 2);
                }
            ),
                ("Type of Coverage", p => p.TypeOfCoverage!.NameInEnglish),
                ("RI Basic Prem", p =>
                {
                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                    var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                 tp.Duration == p.POL_PERIOD &&
                                                                 tp.PlanType == fixedPlanType &&
                                                                 tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                 tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                 tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                    return ri - Math.Round(ri * 0.05, 2);
                }
            ),
                ("User", p => p.CreatedBy!.NameInEnglish),
                ("Payment Account", p => $"{p.POL_TA_CODE} - {p.PaymentAccount?.NameInEnglish ?? "N/A"}")
            );
        })
        .RequireAuthorization("admin")
        .Produces<FileContentResult>(200, "text/csv");

        group.MapPost("/policy", async (
            APPL_INS db,
            PolicyDocumentService policyDocumentService,
            CurrentUserService currentUserService,
            [FromBody] PrintPolicyRequestDTO request) =>
        {
            if (string.IsNullOrWhiteSpace(request.policyNumber))
                return Results.BadRequest("Policy number is required.");

            await currentUserService.RetrieveUser();
            var currentUser = currentUserService.UserInstance();
            var policyRefNumber = request.policyNumber.Trim();
            var isDraft = request.printAsDraft;

            // report to return
            byte[]? report;

            // Searching for policy (Assume it is a single policy)
            var policiesQuery = db.TV_POLICIES
                .Where(p => p.POL_REF_KEY == policyRefNumber &&
                            p.POL_CGP_ID == currentUserService.CompanyGroupId);

            // Regular agents can only print policies issued by their own office.
            if (currentUserService.Role == 1)
                policiesQuery = policiesQuery.Where(p => p.POL_OFC_ID == currentUser.CompanyOfficeId);

            var policies = await policiesQuery
                .Include(p => p.DestinationCountry)
                .Include(p => p.NationalityCountry)
                .Include(p => p.TypeOfCoverage)
                .Include(p => p.CompanyOffice)
                .Include(p => p.additionalInfo)
                .AsNoTracking()
                .ToListAsync();

            if (policies.Count == 0)
                return Results.NotFound("Policy not found.");

            // Check if policy type was 3 (part of a group policy)
            if (policies[0].POL_ATR_TYPE == 3)
            {
                // Getting group policy
                PolicyReferenceKey policyKey = new PolicyReferenceKey(policyRefNumber);
                var groupPolicyQuery = db.TV_POLICY_PACKAGE.Where(policy =>
                    policy.TPP_CGP_ID == currentUserService.CompanyGroupId &&
                    policy.TPP_OFC_ID == policyKey.OfficeCode &&
                    policy.TPP_YEAR == policyKey.Year &&
                    policy.TPP_POL_NO == policyKey.PolicyNumber
                );

                if (currentUserService.Role == 1)
                    groupPolicyQuery = groupPolicyQuery.Where(policy =>
                        policy.TPP_OFC_ID == currentUser.CompanyOfficeId);

                var groupPolicies = await groupPolicyQuery

                .Include(p => p.DestinationCountry)
                .Include(p => p.subPolicies!)
                .ThenInclude(p => p.NationalityCountry)
                .Include(p => p.subPolicies!)
                .ThenInclude(p => p.additionalInfo)
                .Include(p => p.TypeOfCoverage)
                .Include(p => p.CompanyOffice)

                .ToListAsync();

                var groupPolicy = groupPolicies.SingleOrDefault();
                if (groupPolicy is null)
                    return Results.NotFound("Group policy not found.");

                // Generating report
                report = await policyDocumentService.GenerateReport(groupPolicy, isDraft);
            }

            // it is a single policy (must be 1 record so we will use "Single()" with it)
            else
            {
                // Getting single policy
                var singlePolicy = policies.Single();

                // Once an endorsement exists, only the endorsed (C) document can be printed.
                if (!policyRefNumber.EndsWith(" C", StringComparison.OrdinalIgnoreCase))
                {
                    var endorsedVersionCount = await db.TV_POLICIES.CountAsync(p =>
                        p.POL_CGP_ID == currentUserService.CompanyGroupId &&
                        p.POL_REF_KEY == policyRefNumber + " C");

                    if (endorsedVersionCount > 0)
                        return Results.Conflict("The original policy is unavailable after endorsement. Print the endorsed policy instead.");
                }

                // Generating report
                report = await policyDocumentService.GenerateReport(singlePolicy, isDraft);
            }

            // Check if report is ready
            if (report == null)
                return Results.NotFound("Report could not be generated.");

            return Results.File(report, "application/pdf", "Policy Report.pdf");
        })
        .RequireAuthorization("regular")
        .Produces<FileContentResult>(StatusCodes.Status200OK, "application/pdf")
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/policy-customer", async (APPL_INS db, PolicyDocumentService PolicyDocumentService, [FromBody] RetrievePolicyRequestDTO req) =>
        {
            // report to return
            byte[]? report;

            if (req is RetrieveSinglePolicyRequestDTO singleRequest)
            {
                // Searching for policy (Assume it is a single policy)
                var policies = await db.TV_POLICIES
                    .Where(p => p.POL_PASSEGER_LAST_NAME_NTV == singleRequest.passengerLastNameNative &&
                                p.POL_PASSEGER_PASSPORT_NUMBER == singleRequest.passengerPassportNumber &&
                                p.POL_CGP_ID == 8)
                    .Include(p => p.DestinationCountry)
                    .Include(p => p.NationalityCountry)
                    .Include(p => p.TypeOfCoverage)
                    .Include(p => p.CompanyOffice)
                    .Include(p => p.additionalInfo)
                    .OrderByDescending(p => p.POL_CREATION_DATE)
                    .AsNoTracking()
                    .ToListAsync();

                var policy = policies.First();
                report = await PolicyDocumentService.GenerateReport(policy, false);
            }
            else if (req is RetrieveGroupPolicyRequestDTO groupRequest)
            {
                // Getting group policy
                PolicyReferenceKey policyKey = new PolicyReferenceKey(groupRequest.policyRefNumber);
                var groupPolicies = await db.TV_POLICY_PACKAGE.Where(policy =>
                    policy.TPP_CGP_ID == 8 &&
                    policy.TPP_OFC_ID == policyKey.OfficeCode &&
                    policy.TPP_YEAR == policyKey.Year &&
                    policy.TPP_POL_NO == policyKey.PolicyNumber &&
                    policy.TPP_NAME_NTV == groupRequest.companyName

                )

                .Include(p => p.DestinationCountry)
                .Include(p => p.subPolicies!)
                .ThenInclude(p => p.NationalityCountry)
                .Include(p => p.subPolicies!)
                .ThenInclude(p => p.additionalInfo)
                .Include(p => p.TypeOfCoverage)
                .Include(p => p.CompanyOffice)

                .ToListAsync();
                var groupPolicy = groupPolicies.Single();

                // Generating report
                report = await PolicyDocumentService.GenerateReport(groupPolicy, false);
            }
            else throw new ApplicationException("Invalid Request Format");

            // Check if report is ready
            if (report == null)
                return Results.NotFound("Report could not be generated.");

            return Results.File(report, "application/pdf", "Policy Report.pdf");
        })
        .Produces<FileContentResult>(200, "application/pdf");

        group.MapGet("agent-report", async (
            APPL_INS db,
            CSVService cSVService,
            CurrentUserService currentUserService,
            SuperAdminRepository controlPanelRepository,
            ILogger<Program> Logger,

            [FromQuery] ProductionReportType productionReportType,
            [FromQuery] DateOnly DateFrom,
            [FromQuery] DateOnly DateTo,
            [FromQuery] string? officeId
        ) =>
        {
            var policyTypes = await db.PolicyTypes.Where(p => p.CompanyGroupId == 8)
                                                  .AsNoTracking()
                                                  .ToDictionaryAsync(p => int.Parse(p.Id), p => p.NameInEnglish);
            var travelPrices = await db.TravelPrices.Where(p => p.CompanyGroupId == 8)
                                                    .AsNoTracking()
                                                    .ToListAsync();

            switch (productionReportType)
            {
                case ProductionReportType.ReInsurance:
                    {
                        if (currentUserService.UserType is not UserType.ADMIN)
                            throw new ApplicationException("You don't have access to that report");

                        var policies = await db.TV_POLICIES
                            .Where(p => p.POL_CGP_ID == 8 &&
                                        p.POL_CREATION_DATE >= DateFrom.ToDateTime(TimeOnly.MinValue) &&
                                        p.POL_CREATION_DATE <= DateTo.ToDateTime(TimeOnly.MaxValue) &&
                                        (string.IsNullOrWhiteSpace(officeId) || p.POL_OFC_ID == officeId))

                            .Include(p => p.TypeOfCoverage)
                            .AsNoTracking()
                            .ToListAsync();

                        if (policies.Count == 0)
                            throw new ApplicationException("No policies found for the given criteria.");

                        return cSVService.BuildFileResult(policies, "PRODUCTION RE-INSURANCE.csv",
                            ("Plan", p => p.POL_ATR_TYPE),
                            ("Type Of Coverage", p => p.TypeOfCoverage!.NameInEnglish),
                            ("Policy Type", p => policyTypes.GetValueOrDefault(p.POL_ATR_TYPE, "Unknown")),
                            ("Voucher Number", p => $"{p.POL_PLT_ID} {p.POL_OFC_ID} {p.POL_YEAR} {p.POL_VHH_NUM}"),
                            ("Policy Number", p => p.POL_REF_KEY),
                            ("Name / English", p => $"{p.POL_PASSEGER_1ST_NAME_NTV} {p.POL_PASSEGER_LAST_NAME_NTV}"),
                            ("Name / Arabic", p => $"{p.POL_PASSEGER_1ST_NAME_FRN} {p.POL_PASSEGER_LAST_NAME_FRN}"),
                            ("Birth Date", p => p.POL_PASSEGER_BIRTH_DATE.ToString("dd/MM/yyyy")),
                            ("Passport Number", p => p.POL_PASSEGER_PASSPORT_NUMBER),
                            ("Issue Date", p => p.POL_ISSUE_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                            // ("ORG POL Issue Date", p => p.POL_CREATION_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                            ("Inception Date", p => p.POL_INCEPTION_DATE.ToString("dd/MM/yyyy")),
                            ("Period", p => p.POL_PERIOD),
                            ("Total RI", p =>
                            {
                                int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                                string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                                return travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                          tp.Duration == p.POL_PERIOD &&
                                                                          tp.PlanType == fixedPlanType &&
                                                                          tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                          tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                          tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                            }
                        ),
                            ("Commission", p =>
                            {
                                int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                                string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                                var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                            tp.Duration == p.POL_PERIOD &&
                                                                            tp.PlanType == fixedPlanType &&
                                                                            tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                            tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                            tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                                return Math.Round(ri * 0.05, 2);
                            }
                        ),
                            ("Net to RI", p =>
                            {
                                int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                                string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                                var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                            tp.Duration == p.POL_PERIOD &&
                                                                            tp.PlanType == fixedPlanType &&
                                                                            tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                            tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                            tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                                return ri - Math.Round(ri * 0.05, 2);
                            }
                        ),
                            ("Optional Parameters", p =>
                            {
                                var opts = new List<string>();
                                if (p.POL_WINTER_FLAG == "W") opts.Add("Winter Ski");
                                if (p.POL_ACCEDENT_CODE == "1") opts.Add("Trip Cancellation");
                                return string.Join(" | ", opts);
                            }
                        )
                        );
                    }

                case ProductionReportType.CustomerPortalManagementFees:
                    {
                        if (currentUserService.UserType is not UserType.ADMIN)
                            throw new ApplicationException("You don't have access to that report");

                        var policies = await db.TV_POLICIES
                            .Where(p => p.POL_CGP_ID == 8 &&
                                        p.POL_CREATION_DATE >= DateFrom.ToDateTime(TimeOnly.MinValue) &&
                                        p.POL_CREATION_DATE <= DateTo.ToDateTime(TimeOnly.MaxValue) &&
                                        p.POL_CREATED_BY == "CSTMR_USER" &&
                                        (string.IsNullOrWhiteSpace(officeId) || p.POL_OFC_ID == officeId))
                            .Include(p => p.TypeOfCoverage)
                            .AsNoTracking()
                            .ToListAsync();

                        if (policies.Count == 0)
                            throw new ApplicationException("No policies found for the given criteria.");

                        var (newCustomerFee, _) = await controlPanelRepository.GetCompanyNewAndExistingCustomerFeesPercentAsync(8);

                        return cSVService.BuildFileResult(policies, "PRODUCTION CUSTOMER PORTAL FEES.csv",
                            ("Plan", p => p.POL_ATR_TYPE),
                            ("Type Of Coverage", p => p.TypeOfCoverage!.NameInEnglish),
                            ("Policy Type", p => policyTypes.GetValueOrDefault(p.POL_ATR_TYPE, "Unknown")),
                            ("Voucher Number", p => $"{p.POL_PLT_ID} {p.POL_OFC_ID} {p.POL_YEAR} {p.POL_VHH_NUM}"),
                            ("Policy Number", p => p.POL_REF_KEY),
                            ("Name / English", p => $"{p.POL_PASSEGER_1ST_NAME_NTV} {p.POL_PASSEGER_LAST_NAME_NTV}"),
                            ("Name / Arabic", p => $"{p.POL_PASSEGER_1ST_NAME_FRN} {p.POL_PASSEGER_LAST_NAME_FRN}"),
                            ("Birth Date", p => p.POL_PASSEGER_BIRTH_DATE.ToString("dd/MM/yyyy")),
                            ("Passport Number", p => p.POL_PASSEGER_PASSPORT_NUMBER),
                            ("Issue Date", p => p.POL_ISSUE_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                            // ("ORG POL Issue Date", p => p.POL_CREATION_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                            ("Inception Date", p => p.POL_INCEPTION_DATE.ToString("dd/MM/yyyy")),
                            ("Period", p => p.POL_PERIOD),
                            ("Optional Parameters", p =>
                            {
                                var opts = new List<string>();
                                if (p.POL_WINTER_FLAG == "W") opts.Add("Winter Ski");
                                if (p.POL_ACCEDENT_CODE == "1") opts.Add("Trip Cancellation");
                                return string.Join(" | ", opts);
                            }
                        ),
                            ("Total Premium", p => p.POL_PREMIUM_AMOUNT),
                            ("Customer Portal Management Fee Percent", p => newCustomerFee),
                            ("Customer Portal Fee", p => p.POL_TRUST_COMM)
                        );
                    }

                case ProductionReportType.Policies:
                    {
                        var policies = await db.TV_POLICIES
                            .Where(p => p.POL_CGP_ID == 8 &&
                                        p.POL_CREATION_DATE >= DateFrom.ToDateTime(TimeOnly.MinValue) &&
                                        p.POL_CREATION_DATE <= DateTo.ToDateTime(TimeOnly.MaxValue))

                            .Include(p => p.TypeOfCoverage)
                            .Include(p => p.CompanyOffice)
                            .Include(p => p.PaymentAccount)
                            .Include(p => p.DestinationCountry)
                            .Include(p => p.CreatedBy)
                            .AsNoTracking()
                            .OrderByDescending(p => p.POL_CREATION_DATE)
                            .ToListAsync();

                        if (policies.Count == 0)
                            return Results.NotFound("No policies found for the given criteria.");

                        if (currentUserService.Role == 2)
                        {
                            return cSVService.BuildFileResult(policies, "Policies Report.csv",
                                ("Office Name", p => p.CompanyOffice?.NameInEnglish ?? "N/A"),
                                ("Policy Type", p => policyTypes.GetValueOrDefault(p.POL_ATR_TYPE, "Unknown")),
                                ("Voucher number", p => p.POL_VHH_NUM),
                                ("Policy No", p => p.POL_REF_KEY),
                                ("Serial", p => p.POL_INT_SERIAL),
                                ("Name / English", p => $"{p.POL_PASSEGER_1ST_NAME_NTV} {p.POL_PASSEGER_LAST_NAME_NTV}"),
                                ("Name / Arabic", p => $"{p.POL_PASSEGER_1ST_NAME_FRN} {p.POL_PASSEGER_LAST_NAME_FRN}"),
                                ("Birth Date", p => p.POL_PASSEGER_BIRTH_DATE.ToString("dd/MM/yyyy")),
                                ("Passport No", p => p.POL_PASSEGER_PASSPORT_NUMBER),
                                ("Issue Date", p => p.POL_ISSUE_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                                // ("ORG POL Issue Date", p => p.POL_CREATION_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                                ("Destination", p => p.DestinationCountry?.NameInEnglish ?? "THIS POLILCY HAS NO COUNTRY, CONTACT SYSTEM ADMIN"),
                                ("Inc Date", p => p.POL_INCEPTION_DATE.ToString("dd/MM/yyyy")),
                                ("Period", p => p.POL_PERIOD),
                                ("Gross Premium", p => p.POL_PREMIUM_AMOUNT),
                                ("Reinsurance Amount", p =>
                                {
                                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                                    return travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                               tp.Duration == p.POL_PERIOD &&
                                                                               tp.PlanType == fixedPlanType &&
                                                                               tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                               tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                               tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                                }
                            ),
                                ("Agent Comm", p =>
                                {
                                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                                    var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                                 tp.Duration == p.POL_PERIOD &&
                                                                                 tp.PlanType == fixedPlanType &&
                                                                                 tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                                 tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                                 tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                                    return Math.Round(ri * 0.05, 2);
                                }
                            ),
                                ("Type of Coverage", p => p.TypeOfCoverage!.NameInEnglish),
                                ("RI Basic Prem", p =>
                                {
                                    int fixedPlanType = p.POL_ATR_TYPE == 3 ? 1 : p.POL_ATR_TYPE;
                                    string fixedTypeOfCoverage = p.POL_ATR_TYPE == 3 ? "2" : p.POL_AREA_CODE;
                                    var ri = travelPrices.SingleOrDefault(tp => tp.TypeOfCoverageId == fixedTypeOfCoverage &&
                                                                                 tp.Duration == p.POL_PERIOD &&
                                                                                 tp.PlanType == fixedPlanType &&
                                                                                 tp.AgeFrom <= p.POL_PASSEGER_AGE &&
                                                                                 tp.AgeTo >= p.POL_PASSEGER_AGE &&
                                                                                 tp.CompanyGroupId == 8)?.ReInsuranceAmount ?? 0;
                                    return ri - Math.Round(ri * 0.05, 2);
                                }
                            ),
                                ("User", p => p.CreatedBy!.NameInEnglish),
                                ("Payment Account", p => $"{p.POL_TA_CODE} - {p.PaymentAccount?.NameInEnglish ?? "N/A"}")
                            );
                        }

                        // If the user is not an admin, then just returning his offices
                        await currentUserService.RetrieveUser();
                        policies = [.. policies.Where(p => p.POL_OFC_ID == currentUserService.UserInstance().USR_OFC_ID)];

                        return cSVService.BuildFileResult(policies, "Policies Report.csv",
                            ("Policy Type", p => policyTypes.GetValueOrDefault(p.POL_ATR_TYPE, "Unknown")),
                            ("Policy No", p => p.POL_REF_KEY),
                            ("Serial", p => p.POL_INT_SERIAL),
                            ("Name / English", p => $"{p.POL_PASSEGER_1ST_NAME_NTV} {p.POL_PASSEGER_LAST_NAME_NTV}"),
                            ("Name / Arabic", p => $"{p.POL_PASSEGER_1ST_NAME_FRN} {p.POL_PASSEGER_LAST_NAME_FRN}"),
                            ("Birth Date", p => p.POL_PASSEGER_BIRTH_DATE.ToString("dd/MM/yyyy")),
                            ("Passport No", p => p.POL_PASSEGER_PASSPORT_NUMBER),
                            ("Issue Date", p => p.POL_ISSUE_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                            // ("ORG POL Issue Date", p => p.POL_CREATION_DATE.ToString("dd/MM/yyyy HH:mm:ss")),
                            ("Destination", p => p.DestinationCountry?.NameInEnglish),
                            ("Inc Date", p => p.POL_INCEPTION_DATE.ToString("dd/MM/yyyy")),
                            ("Period", p => p.POL_PERIOD),
                            ("Gross Premium", p => p.POL_PREMIUM_AMOUNT),
                            ("Type of Coverage", p => p.TypeOfCoverage?.NameInEnglish),
                            ("Broker Commission Percent", p => p.POL_TA_COMM_PCNT),
                            ("Broker Commission Amount", p => p.POL_TA_COMM)
                        );
                    }

                default:
                    return Results.BadRequest("Invalid report type.");
            }
        })
        .RequireAuthorization("regular")
        .Produces<FileContentResult>(200, "text/csv");

        // BI Dashboards
        group.MapGet("bi-report-1", async (APPL_INS db, [FromQuery] int? year) =>
        {
            const int cgpId = 8;
            int reportYear = year ?? DateTime.Now.Year;

            var yearStart = new DateTime(reportYear, 1, 1);
            var yearEnd = new DateTime(reportYear, 12, 31, 23, 59, 59);
            var prevYearStart = new DateTime(reportYear - 1, 1, 1);
            var prevYearEnd = new DateTime(reportYear - 1, 12, 31, 23, 59, 59);

            var policies = await db.TV_POLICIES
                .Where(p => p.POL_CGP_ID == cgpId &&
                            p.POL_CREATION_DATE >= yearStart &&
                            p.POL_CREATION_DATE <= yearEnd)
                .AsNoTracking()
                .Select(p => new
                {
                    p.POL_PREMIUM_AMOUNT,
                    p.POL_ATR_TYPE,
                    p.POL_CREATION_DATE,
                    p.POL_INCEPTION_DATE,
                    p.POL_EXPIRY_DATE,
                    p.POL_PERIOD,
                    p.POL_TA_COMM,
                    p.POL_STATUS,
                    TypeOfCoverageName = p.TypeOfCoverage!.NameInEnglish
                })
                .ToListAsync();

            var prevPolicies = await db.TV_POLICIES
                .Where(p => p.POL_CGP_ID == cgpId &&
                            p.POL_CREATION_DATE >= prevYearStart &&
                            p.POL_CREATION_DATE <= prevYearEnd)
                .AsNoTracking()
                .Select(p => new { p.POL_PREMIUM_AMOUNT, p.POL_TA_COMM, p.POL_STATUS })
                .ToListAsync();

            var activeCount = await db.TV_POLICIES
                .Where(p => p.POL_CGP_ID == cgpId &&
                            DateTime.Now >= p.POL_INCEPTION_DATE &&
                            DateTime.Now <= p.POL_EXPIRY_DATE)
                .AsNoTracking()
                .CountAsync();

            // ── Current-year aggregates
            var totalPremium = policies.Sum(p => p.POL_PREMIUM_AMOUNT);
            var policyCount = policies.Count;
            var totalComm = policies.Sum(p => p.POL_TA_COMM) ?? 0;

            var prevPremium = prevPolicies.Sum(p => p.POL_PREMIUM_AMOUNT);
            var prevCount = prevPolicies.Count;
            var prevComm = prevPolicies.Sum(p => p.POL_TA_COMM) ?? 0d;

            static double PctChange(double current, double previous) =>
                previous != 0 ? Math.Round((current - previous) / previous * 100, 1) : 0;

            static string FormatChange(double pct) =>
                $"{(pct >= 0 ? "+" : "")}{pct}%";

            // ── KPI Cards
            var premiumChange = PctChange(totalPremium, prevPremium);
            var countChange = PctChange(policyCount, prevCount);
            var commChange = PctChange(totalComm, prevComm);

            // Endorsement KPI
            var totalEndorsed = policies.Count(p => p.POL_STATUS == "C");
            var prevEndorsed = prevPolicies.Count(p => p.POL_STATUS == "C");
            var endorsedChange = PctChange(totalEndorsed, prevEndorsed);

            var kpiCards = new List<KpiCard>
            {
                new("Gross Premium",    $"${totalPremium:N0}",      FormatChange(premiumChange), "bi-currency-dollar", premiumChange >= 0),
                new("Policies Issued",  policyCount.ToString("N0"), FormatChange(countChange),   "bi-file-text-fill",  countChange >= 0),
                new("Active Insured",   activeCount.ToString("N0"), "Live",                      "bi-people-fill",     true),
                new("Total Endorsed", totalEndorsed.ToString("N0"),         FormatChange(endorsedChange),    "bi-graph-up-arrow",  endorsedChange >= 0),
            };

            // ── Shared helpers
            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            var planNames = new Dictionary<int, string>
            {
                { 1, "Individual" },
                { 2, "Group" },
                { 3, "Group Member" },
                {8, "In-Bound"},
                {11, "Student"}
            };

            var presentTypes = policies
                .Select(p => p.POL_ATR_TYPE)
                .Distinct()
                .Where(t => planNames.ContainsKey(t))
                .OrderBy(t => t)
                .ToList();

            var coverageAreas = policies
                .Select(p => p.TypeOfCoverageName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            // ── 1. Line Chart – Monthly Gross Premium per Plan Type
            var monthlyByType = policies
                .GroupBy(p => (Month: p.POL_CREATION_DATE.Month, p.POL_ATR_TYPE))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.POL_PREMIUM_AMOUNT));

            var lineChartLines = presentTypes.Select(type => new ChartLine(
                planNames[type],
                [.. Enumerable.Range(1, 12).Select(m => monthlyByType.GetValueOrDefault((m, type), 0d))]
            )).ToList();

            // ── 2. Bar Chart – Policy Count by Coverage Area per Plan Type
            var countByAreaType = policies
                .GroupBy(p => (p.TypeOfCoverageName, p.POL_ATR_TYPE))
                .ToDictionary(g => g.Key, g => g.Count());

            var barChartBars = presentTypes.Select(type => new ChartBar(
                planNames[type],
                [.. coverageAreas.Select(area => (double)countByAreaType.GetValueOrDefault((area, type), 0))]
            )).ToList();

            // ── 3. Donut Chart – Policy Distribution by Plan Type
            var countByType = policies
                .GroupBy(p => p.POL_ATR_TYPE)
                .ToDictionary(g => g.Key, g => g.Count());

            var policyDistribution = presentTypes
                .Select(type => new ChartSlice(
                    (double)countByType.GetValueOrDefault(type, 0),
                    planNames[type]
                ))
                .OrderByDescending(d => d.Value)
                .ToList();

            // ── 4. Scatter Chart – Travel Period (days) vs Gross Premium (up to 200 points)
            var scatterPoints = policies
                .Take(200)
                .Select(p => new double[] { p.POL_PERIOD, p.POL_PREMIUM_AMOUNT })
                .ToList();

            // ── 5. Heatmap – Policy Count by Month × Coverage Area
            var countByMonthArea = policies
                .GroupBy(p => (Month: p.POL_CREATION_DATE.Month, Area: p.TypeOfCoverageName))
                .ToDictionary(g => g.Key, g => g.Count());

            var heatmapData = coverageAreas
                .SelectMany((area, areaIdx) =>
                    Enumerable.Range(0, 12).Select(monthIdx =>
                        new int[] { monthIdx, areaIdx, countByMonthArea.GetValueOrDefault((monthIdx + 1, area), 0) }
                    )
                )
                .ToList();

            var maxHeatmapValue = heatmapData.Count > 0 ? heatmapData.Max(d => d[2]) : 0;

            return Results.Ok(new BiReport1Response(
                KpiCards: kpiCards,
                LineChartConfig: new LineChartDto(
                    Title: $"Monthly Gross Premium by Plan Type ({reportYear})",
                    XAxisValues: months,
                    Lines: lineChartLines,
                    Area: true
                ),
                BarChartConfig: new BarChartDto(
                    Title: $"Policy Volume by Coverage Area & Plan Type ({reportYear})",
                    XAxisValues: [.. coverageAreas],
                    Bars: barChartBars
                ),
                DonutChartConfig: new DonutChartDto(
                    Title: $"Policy Distribution by Plan Type ({reportYear})",
                    Data: policyDistribution
                ),
                ScatterChartConfig: new ScatterChartDto(
                    Title: "Travel Period (days) vs Gross Premium",
                    XName: "Period (days)",
                    YName: "Gross Premium",
                    Points: scatterPoints
                ),
                HeatmapChartConfig: new HeatmapChartDto(
                    Title: $"Monthly Policy Volume by Coverage Area ({reportYear})",
                    SeriesName: "Policy Count",
                    XLabels: months,
                    YLabels: [.. coverageAreas],
                    Data: heatmapData,
                    MaxValue: maxHeatmapValue
                )
            ));
        })
        //.RequireAuthorization("admin")
        .Produces<BiReport1Response>();

        return group;
    }
}
