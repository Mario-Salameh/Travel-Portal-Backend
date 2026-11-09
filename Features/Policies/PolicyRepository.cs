using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Infrastructure.Repositories;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Repositories;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Features.Policies;

public class PolicyRepository(
    APPL_INS db,
    AgencyCommissionRepository agencyCommissionRepository,
    SuperAdminRepository controlPanelRepository,
    CurrentUserService currentUserService,
    ILogger<PolicyRepository> Logger
    )
{
    private readonly APPL_INS _db = db;
    private readonly CurrentUserService _currentUserService = currentUserService;

    // Defining constants
    const string policyType = "T01";
    const string policyClass = "T";
    const string policyDocumentType = "1";

    public async Task<List<TV_POLICY_PACKAGE>> GetAllGroupPolicies(InquiryGroupPolicyRequestDTO? inquiryForm, bool customerPortalPolicies = false)
    {
        var query = _db.TV_POLICY_PACKAGE.AsQueryable();

        if (_currentUserService.Role != 2)
        {
            await _currentUserService.RetrieveUser();

            var currentUser = _currentUserService.UserInstance();
            var officeId = currentUser.CompanyOfficeId;
        
            query = query.Where(v =>
                v.TPP_OFC_ID == officeId);
        }

        if (customerPortalPolicies)
            query = query.Where(v => v.CreatedBy!.Role == 9);

        if (inquiryForm != null)
        {
            if (!string.IsNullOrWhiteSpace(inquiryForm.companyName))
                query = query.Where(p => p.TPP_NAME_NTV!.Contains(inquiryForm.companyName));

            if (!string.IsNullOrWhiteSpace(inquiryForm.policyRefNumber))
            {
                var refNo = inquiryForm.policyRefNumber;
                if (refNo.EndsWith(" C", StringComparison.Ordinal))
                    refNo = refNo[..^2];

                query = query.Where(p =>
                    (p.TPP_OFC_ID + " " + p.TPP_PLT_ID + " " + p.TPP_YEAR + " " + p.TPP_POL_NO)
                    .Contains(refNo));
            }

            if (inquiryForm.fromDate != null)
                query = query.Where(p =>
                    p.TPP_ISSUE_DATE >= inquiryForm.fromDate.Value.ToDateTime(TimeOnly.MinValue));

            if (inquiryForm.toDate != null)
                query = query.Where(p =>
                    p.TPP_ISSUE_DATE <= inquiryForm.toDate.Value.ToDateTime(TimeOnly.MaxValue));
        }

        query = query.Where(p => p.TPP_CGP_ID == 8)
        .Include(v => v.subPolicies!).ThenInclude(v => v.additionalInfo)
        .Include(v => v.CreatedBy)
        .AsNoTracking();
        
        return await query.OrderByDescending(p => p.TPP_ISSUE_DATE)
            .ToListAsync();
    }

    public async Task<List<TV_POLICIES>> GetAllSinglePolicies(InquirySinglePolicyRequestDTO? inquiryForm, bool customerPortalPolicies = false)
    {
        var query = _db.TV_POLICIES.AsQueryable();

        if (_currentUserService.Role != 2)
        {
            await _currentUserService.RetrieveUser();
            User UserInstance = _currentUserService.UserInstance();
            query = query.Where(v => v.POL_OFC_ID == UserInstance.CompanyOfficeId);
        }

        if (customerPortalPolicies)
            query = query.Where(v => v.CreatedBy!.Role == 9);

        if (inquiryForm != null)
        {
            if (!string.IsNullOrWhiteSpace(inquiryForm.firstName))
                query = query.Where(v => v.POL_PASSEGER_1ST_NAME_NTV!.Contains(inquiryForm.firstName));

            if (!string.IsNullOrWhiteSpace(inquiryForm.lastName))
                query = query.Where(v => v.POL_PASSEGER_LAST_NAME_NTV!.Contains(inquiryForm.lastName));

            if (!string.IsNullOrEmpty(inquiryForm.passportNumber))
                query = query.Where(v => v.POL_PASSEGER_PASSPORT_NUMBER!.Contains(inquiryForm.passportNumber));

            if (!string.IsNullOrWhiteSpace(inquiryForm.passportNumber))
                query = query.Where(p => p.POL_PASSEGER_PASSPORT_NUMBER!.Contains(inquiryForm.passportNumber));

            if (inquiryForm.policyType != null)
                query = query.Where(p => p.POL_ATR_TYPE == inquiryForm.policyType);

            if (!string.IsNullOrWhiteSpace(inquiryForm.policyRefNumber))
                query = query.Where(p => p.POL_REF_KEY.Contains(inquiryForm.policyRefNumber));

            if (!string.IsNullOrWhiteSpace(inquiryForm.officeId))
                query = query.Where(p => p.POL_OFC_ID.Contains(inquiryForm.officeId));

            if (inquiryForm.period != null)
                query = query.Where(p => p.POL_PERIOD == inquiryForm.period);

            if (inquiryForm.fromDate != null)
                query = query.Where(p =>
                    p.POL_ISSUE_DATE >= inquiryForm.fromDate.Value.ToDateTime(TimeOnly.MinValue));

            if (inquiryForm.toDate != null)
                query = query.Where(p =>
                    p.POL_ISSUE_DATE <= inquiryForm.toDate.Value.ToDateTime(TimeOnly.MaxValue));
        }

        query = query.Where(p =>
            p.POL_CGP_ID == 8 &&
            p.POL_ATR_TYPE != 3 &&
            !_db.TV_POLICIES.Any(endorsed =>
                endorsed.POL_CGP_ID == p.POL_CGP_ID &&
                endorsed.POL_ATR_TYPE == p.POL_ATR_TYPE &&
                endorsed.POL_STATUS == "C" &&
                endorsed.POL_REF_KEY == p.POL_REF_KEY + " C"))
            .AsNoTracking();

        return await query.Include(v => v.CreatedBy)
                          .Include(v => v.additionalInfo)
                          .OrderByDescending(p => p.POL_ISSUE_DATE)
                          .ToListAsync();
    }

    public async Task<TV_POLICIES> GetSinglePolicyByRefNumberAsync(InquirySinglePolicyRequestDTO inquiry)
    {
        var results = await GetAllSinglePolicies(inquiry);
        if (results.Count != 1)
            throw new ApplicationException($"Expected exactly 1 policy for ref number {inquiry.policyRefNumber}, but found {results.Count}.");
        return results.First();
    }

    public async Task<TV_POLICY_PACKAGE> GetGroupPolicyByRefNumberAsync(InquiryGroupPolicyRequestDTO inquiry)
    {
        var results = await GetAllGroupPolicies(inquiry);
        if (results.Count != 1)
            throw new ApplicationException($"Expected exactly 1 group policy for ref number {inquiry.policyRefNumber}, but found {results.Count}.");
        return results.First();
    }

    public async Task<CustomerViewPolicy?> EndorsePolicy(string PolicyRefNumber, bool JustCheck)
    {
        // Check reference number format
        string? officeId, policyType, policyNumber;
        int policyYear;

        // Analyzing reference number to get the right info from reference number
        var parts = PolicyRefNumber.Split(' ');
        if (parts.Length != 4)
            throw new ApplicationException("Invalid policy reference number format.");

        officeId = parts[0];
        policyType = parts[1];
        policyYear = int.Parse(parts[2]);
        policyNumber = parts[3];

        // Check whether the policy is group or single policy
        var resultsToEndorse = await _db.TV_POLICIES.Where(policy => policy.POL_REF_KEY == PolicyRefNumber && policy.POL_CGP_ID == 8).ToListAsync();

        // Check if there is a result
        if (resultsToEndorse.Count == 0)
            throw new ApplicationException("Policy not found.");

        // Differentiate through ATR type for the first policy
        if (resultsToEndorse[0].POL_ATR_TYPE == 3)
        {
            // Start group policy configuration
            // # Check if group policy was endorsed before or not
            var endorsedPolicies = await _db.TV_POLICIES.Where(p => p.POL_REF_KEY == (PolicyRefNumber + " C") && p.POL_CGP_ID == 8)
            .AsNoTracking()
            .ToListAsync();

            if (endorsedPolicies.Count != 0)
                throw new ApplicationException("Policy already endorsed.");

            // Getting group policy instance
            var groupPolicies = await _db.TV_POLICY_PACKAGE.Where(policy => policy.TPP_OFC_ID == officeId &&
                                                                     policy.TPP_PLT_ID == policyType &&
                                                                     policy.TPP_YEAR == policyYear &&
                                                                     policy.TPP_POL_NO == policyNumber &&
                                                                     policy.TPP_CGP_ID == 8)
                                                    .Include(policy => policy.subPolicies)
                                                    .ToListAsync();

            var groupPolicy = groupPolicies.Single();

            // If justcheck was true, return the policy object to show it on the frontend side before endorsing
            if (JustCheck)
                return new CustomerViewGroupPolicy(groupPolicy);

            else
            {
                // Endorsing all of single policies in the group policy
                foreach (var policyToEndorse in resultsToEndorse)
                    // Endorse each policy in the group
                    await _db.EndorcementPolicy_Async(new DB_TV_PAL_PACKAGE.EndorcementPolicyInputDTO
                    {
                        p_pol_ref_key = policyToEndorse.POL_REF_KEY,
                        p_pol_int_serial = policyToEndorse.POL_INT_SERIAL.ToString(),
                        p_user_code = _currentUserService.UserCode,
                        p_pol_trn_type = "4",
                        p_cgp_id = 8
                    });

                await _db.TV_POLICY_PACKAGE
                    .Where(p => p.TPP_OFC_ID == groupPolicy.TPP_OFC_ID &&
                                p.TPP_PLT_ID == groupPolicy.TPP_PLT_ID &&
                                p.TPP_YEAR == groupPolicy.TPP_YEAR &&
                                p.TPP_POL_NO == groupPolicy.TPP_POL_NO)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.TPP_STATUS, "C"));

                return null;
            }
        }
        else if (resultsToEndorse.Count > 1)
            throw new ApplicationException("Multiple policies with the same reference number found. Contact system administrator immediately.");
        else
        {
            // Start Single policy configuration
            // Check if single policy was endorsed before or not
            var endorsedPolicy = await _db.TV_POLICIES.Where(p => p.POL_REF_KEY == (PolicyRefNumber + " C") && p.POL_CGP_ID == 8)
            .AsNoTracking()
            .ToListAsync();

            if (endorsedPolicy.Count != 0)
                throw new ApplicationException("Policy already endorsed.");

            // Endorse single policy (if justcheck flag is false, if it was true it will just return policy object to show it on the frontend side before endorsing)
            if (!JustCheck)
            {
                await _db.EndorcementPolicy_Async(new DB_TV_PAL_PACKAGE.EndorcementPolicyInputDTO
                {
                    p_pol_ref_key = PolicyRefNumber,
                    p_cgp_id = 8,
                    p_pol_int_serial = "0",
                    p_pol_trn_type = "4",
                    p_user_code = _currentUserService.UserCode
                });

                return null;
            }
            else
                return new CustomerViewSinglePolicy(resultsToEndorse[0]);
        }
    }

    public async Task<CustomerViewPolicy> UpdatePolicy(string policyRefNumber, CreatePolicyRequest req, int? serial = null)
    {
        // If Serial was provided, it will be a single policy or a part of group policy as a single entity
        if (serial is not null)
        {
            // Getting the policy
            var singlePolicies = await _db.TV_POLICIES.Where(policy =>
                policy.POL_CGP_ID == 8 &&
                policy.POL_REF_KEY == policyRefNumber &&
                policy.POL_INT_SERIAL == serial
            )
            .Include(p => p.additionalInfo)
            .ToListAsync();

            var singlePolicy = singlePolicies.Single();
            CreateSinglePolicyRequest updatedData = (req as CreateSinglePolicyRequest)!;

            // Regular users (role = 1) can't update the policy unless it's status is draft
            if (_currentUserService.Role == 1 && singlePolicy.PolicyStatus != PolicyStatus.Draft)
                throw new ApplicationException("You can't update a policy that is not in draft status.");

            if (singlePolicy.PolicyStatus == PolicyStatus.Draft)
            {
                // Updating all mutable fields and recalculating premium (serial and voucher are preserved inside)
                await UpdateSinglePolicyFields(singlePolicy, updatedData);

                await _db.SaveChangesAsync();
                return new CustomerViewSinglePolicy(singlePolicy);
            }

            else if (singlePolicy.PolicyStatus == PolicyStatus.Generated)
            {
                // in this case, we will endorse the single policy and will create a complete new one with the new information
                await EndorsePolicy(singlePolicy.POL_REF_KEY, false);

                // creating new policy
                var result = await SavePolicy(updatedData);

                return result;
            }

            else throw new ApplicationException("Can't update policy, check policy status.");
        }

        else
        {
            // Getting the policy
            PolicyReferenceKey key = new(policyRefNumber);
            var groupPolicies = await _db.TV_POLICY_PACKAGE.Where(policy =>
                policy.TPP_CGP_ID == 8 &&
                policy.TPP_OFC_ID == key.OfficeCode &&
                policy.TPP_YEAR == key.Year &&
                policy.TPP_POL_NO == key.PolicyNumber
            )
            .Include(policy => policy.subPolicies)
            .ToListAsync();

            var groupPolicy = groupPolicies.Single();
            CreateGroupPolicyRequest updatedData = (req as CreateGroupPolicyRequest)!;

            if (updatedData.policies.Count < 10)
                throw new ApplicationException("You can't update a group policy with less than 10 members.");

            // Regular users (role = 1) can't update the policy unless it's status is draft
            if (_currentUserService.Role == 1 && groupPolicy.PolicyStatus != PolicyStatus.Draft)
                throw new ApplicationException("You can't update a policy that is not in draft status.");

            if (groupPolicy.PolicyStatus == PolicyStatus.Draft)
            {
                // Recalculating type of coverage in case destination or plan changed
                int newTypeOfCoverage = TypeOfCoverageRepository.GetTypeOfCoverageCode(updatedData.destinationCountryCode, updatedData.planType);
                double groupDiscountPercent = GetAppliedGroupDiscountPercent(
                    updatedData.policies.Count,
                    updatedData.manualDiscountPercent
                );

                // Updating the group policy header fields (serial/policy number are preserved)
                groupPolicy.TPP_NAME_NTV = updatedData.companyNameNative;
                groupPolicy.TPP_NAME_FRN = updatedData.companyNameForeign;
                groupPolicy.TPP_INCEPTION_DATE = updatedData.startDate.ToDateTime(TimeOnly.MinValue);
                groupPolicy.TPP_EXPIRY_DATE = updatedData.startDate.AddDays(updatedData.duration).ToDateTime(TimeOnly.MinValue);
                groupPolicy.TPP_PERIOD = updatedData.duration;
                groupPolicy.TPP_SOURCE = updatedData.originCountryCode;
                groupPolicy.TPP_DESTINATION = updatedData.destinationCountryCode;
                groupPolicy.TPP_AREA_CODE = newTypeOfCoverage.ToString();
                groupPolicy.TPP_MEMBERS = updatedData.policies.Count;
                groupPolicy.TPP_MODIFIED_BY = _currentUserService.UserCode;
                groupPolicy.TPP_MODIFICATION_DATE = DateTime.Now;

                // Snapshot voucher numbers by position before deleting
                var oldVouchers = groupPolicy.subPolicies!
                    .OrderBy(p => p.POL_INT_SERIAL)
                    .Select(p => p.POL_VHH_NUM)
                    .ToList();

                // Delete all existing sub-policies; re-insert to handle member additions/removals cleanly
                _db.TV_POLICIES.RemoveRange(groupPolicy.subPolicies!);

                await _currentUserService.RetrieveUser();
                User userInstance = _currentUserService.UserInstance();

                for (int i = 0; i < updatedData.policies.Count; i++)
                {
                    var subPolicyData = updatedData.policies[i];

                    // Reuse old voucher positionally; generate a new one only for added members
                    string voucherNumber = (i < oldVouchers.Count && oldVouchers[i] != null)
                        ? oldVouchers[i]!
                        : (await GenerateVoucherNumber()).ToString();

                    var subPolicyRequest = new CreateSinglePolicyRequest
                    {
                        planType = 3, // Always group type
                        startDate = updatedData.startDate,
                        duration = updatedData.duration,
                        originCountryCode = updatedData.originCountryCode,
                        destinationCountryCode = updatedData.destinationCountryCode,

                        firstNameNative = subPolicyData.firstNameNative,
                        fatherNameNative = subPolicyData.fatherNameNative,
                        lastNameNative = subPolicyData.lastNameNative,
                        firstNameForeign = subPolicyData.firstNameForeign,
                        fatherNameForeign = subPolicyData.fatherNameForeign,
                        lastNameForeign = subPolicyData.lastNameForeign,
                        addressNative = subPolicyData.addressNative,
                        addressForeign = subPolicyData.addressForeign,
                        phoneNumber = subPolicyData.phoneNumber,
                        fax = subPolicyData.fax,
                        passportNumber = subPolicyData.passportNumber,
                        birthDate = subPolicyData.birthDate,
                        passengerGender = subPolicyData.passengerGender,
                        nationalityCountryCode = subPolicyData.nationalityCountryCode,

                        tripCancellationFlag = subPolicyData.tripCancellationFlag,
                        winterSkiFlag = subPolicyData.winterSkiFlag,
                        covidFlag = subPolicyData.covidFlag,
                    };

                    var newSubPolicy = await SaveSinglePolicy(subPolicyRequest, newTypeOfCoverage, userInstance, voucherNumber, groupPolicy.TPP_POL_NO, i + 1);
                    newSubPolicy.POL_PREMIUM_AMOUNT *= 1 - groupDiscountPercent / 100;
                }

                await _db.SaveChangesAsync();
                return new CustomerViewGroupPolicy(groupPolicy);
            }

            else if (groupPolicy.PolicyStatus == PolicyStatus.Generated)
            {
                string polReferenceKey = groupPolicy.TPP_OFC_ID + " " + policyType + " " + groupPolicy.TPP_YEAR + " " + groupPolicy.TPP_POL_NO;

                // in this case, we will endorse the single policy and will create a complete new one with the new information
                await EndorsePolicy(polReferenceKey, false);

                // creating new policy
                var result = await SavePolicy(updatedData);

                return result;
            }

            else throw new ApplicationException("Can't update policy, check policy status.");
        }
    }

    // Updates all mutable fields of a single policy entity in place and recalculates its premium.
    // Preserved (never touched): POL_INT_SERIAL, POL_VHH_NUM, POL_REF_KEY, POL_NO, POL_POLICY_NO,
    //                            POL_CREATED_BY, POL_CREATION_DATE, and all company/office identifiers.
    async Task UpdateSinglePolicyFields(TV_POLICIES policy, CreateSinglePolicyRequest updatedData)
    {
        // Recalculating type of coverage in case destination or plan type changed
        int newTypeOfCoverage = TypeOfCoverageRepository.GetTypeOfCoverageCode(updatedData.destinationCountryCode, updatedData.planType);

        // Sending request to stored procedure to get the new premium prices
        var calc = await _db.CalculateTvAmount_Async(new DB_TV_PAL_PACKAGE.CalculateTvAmountInputDTO
        {
            p_period = updatedData.duration,
            p_age = DateTime.Now.Year - updatedData.birthDate.Year,
            p_zone_code = newTypeOfCoverage.ToString(),
            p_atr_type = GetPremiumCalculationPlanType(updatedData.planType),
            p_winter_flag = updatedData.winterSkiFlag ? "W" : "0",
            p_pol_accedent_code = updatedData.tripCancellationFlag ? "1" : "0",
            p_add_f01 = updatedData.covidFlag ? 1 : 0
        }, new DB_TV_PAL_PACKAGE.CalculateTvAmountInputOutputDTO { });

        // Updating personal info
        policy.POL_PASSEGER_1ST_NAME_NTV = updatedData.firstNameNative;
        policy.POL_PASSEGER_FATHER_NAME_NTV = updatedData.fatherNameNative;
        policy.POL_PASSEGER_LAST_NAME_NTV = updatedData.lastNameNative;
        policy.POL_PASSEGER_1ST_NAME_FRN = updatedData.firstNameForeign;
        policy.POL_PASSEGER_FATHER_NAME_FRN = updatedData.fatherNameForeign;
        policy.POL_PASSEGER_LAST_NAME_FRN = updatedData.lastNameForeign;
        policy.POL_PASSEGER_ADDRESS_NTV = updatedData.addressNative;
        policy.POL_PASSEGER_ADDRESS_FRN = updatedData.addressForeign;
        policy.POL_PASSEGER_TELEPHONE = updatedData.phoneNumber;
        policy.POL_PASSEGER_FAX = updatedData.fax;
        policy.POL_PASSEGER_PASSPORT_NUMBER = updatedData.passportNumber;
        policy.POL_PASSEGER_BIRTH_DATE = updatedData.birthDate.ToDateTime(TimeOnly.MinValue);
        policy.POL_PASSEGER_AGE = DateTime.Now.Year - updatedData.birthDate.Year;
        policy.POL_PASSEGER_GENDER = updatedData.passengerGender;
        policy.POL_PASSEGER_NATIONALITY_ID = updatedData.nationalityCountryCode;

        // Updating trip info
        policy.POL_INCEPTION_DATE = updatedData.startDate.ToDateTime(TimeOnly.MinValue);
        policy.POL_PERIOD = updatedData.duration;
        policy.POL_EXPIRY_DATE = updatedData.startDate.AddDays(updatedData.duration).ToDateTime(TimeOnly.MinValue);
        policy.POL_SOURCE = updatedData.originCountryCode;
        policy.POL_DESTINATION = updatedData.destinationCountryCode;
        policy.POL_AREA_CODE = newTypeOfCoverage.ToString();
        policy.POL_ATR_TYPE = updatedData.planType;
        policy.POL_WINTER_FLAG = updatedData.winterSkiFlag ? "W" : null;
        policy.POL_ACCEDENT_CODE = updatedData.tripCancellationFlag ? "1" : "0";

        if (policy.additionalInfo is not null)
            policy.additionalInfo.ADD_F01 = updatedData.covidFlag ? "1" : "0";

        // Updating premium and all fee fields from the recalculated result
        policy.POL_PREMIUM_AMOUNT = calc.p_prem ?? 0;
        policy.POL_PREMIUM_AMOUNT_LC = calc.p_pol_premium_amount_lc ?? 0;
        policy.POL_PREMIUM_AMOUNT_PC = calc.p_pol_premium_amount_pc ?? 0;
        policy.POL_PREMIUM_AMOUNT2 = calc.p_prem2;
        policy.POL_PREMIUM_AMOUNT2_LC = calc.p_pol_premium_amount2_lc;
        policy.POL_PREMIUM_AMOUNT2_PC = calc.p_pol_premium_amount2_pc;
        policy.POL_FEES1 = calc.p_fees1;
        policy.POL_FEES1_LC = calc.p_pol_fees1_lc;
        policy.POL_FEES1_PC = calc.p_pol_fees1_pc;
        policy.POL_FEES2 = calc.p_fees2;
        policy.POL_FEES2_LC = calc.p_pol_fees2_lc;
        policy.POL_FEES2_PC = calc.p_pol_fees2_pc;
        policy.POL_FEES3 = calc.p_fees3;
        policy.POL_FEES3_LC = calc.p_pol_fees3_lc;
        policy.POL_FEES3_PC = calc.p_pol_fees3_pc;
        policy.POL_FEES4_LC = calc.p_pol_fees4_lc;
        policy.POL_FEES4_PC = calc.p_pol_fees4_pc;
        policy.POL_REINS_PREMIUM = calc.p_ccr ?? 0;
        policy.POL_REINS_PREMIUM_LC = calc.p_pol_reins_premium_lc ?? 0;
        policy.POL_REINS_PREMIUM_PC = calc.p_pol_reins_premium_pc ?? 0;
        policy.POL_REINS_PREMIUM1 = calc.p_afr;
        policy.POL_REINS_PREMIUM1_LC = calc.p_pol_reins_premium1_lc;
        policy.POL_REINS_PREMIUM1_PC = calc.p_pol_reins_premium1_pc;
        policy.POL_REINS_PREMIUM2 = calc.p_ccr;
        policy.POL_REINS_PREMIUM2_LC = calc.p_pol_reins_premium_lc;
        policy.POL_REINS_PREMIUM2_PC = calc.p_pol_reins_premium_pc;
        policy.POL_TA_COMM = calc.p_comm ?? 0;
        policy.POL_TA_COMM_LC = calc.p_pol_ta_comm_lc ?? 0;
        policy.POL_TA_COMM_PC = calc.p_pol_ta_comm_pc ?? 0;
        policy.POL_TA_COMM_PCNT = calc.p_pol_ta_comm_pcnt;
        policy.POL_TRUST_COMM = calc.p_acc_amman ?? 0;
        policy.POL_TRUST_COMM_LC = calc.p_pol_trust_comm_lc ?? 0;
        policy.POL_TRUST_COMM_PC = calc.p_pol_trust_comm_pc ?? 0;
        policy.POL_TRUST_COMM2 = calc.p_acc_amman;
        policy.POL_TRUST_COMM2_LC = calc.p_pol_trust_comm2_lc ?? 0;
        policy.POL_TRUST_COMM2_PC = calc.p_pol_trust_comm2_pc ?? 0;
        policy.POL_RETENTION_AMOUNT_ = calc.p_ret ?? 0;
        policy.POL_RETENTION_AMOUNT__LC = calc.p_pol_retention_amount__lc ?? 0;
        policy.POL_RETENTION_AMOUNT__PC = calc.p_pol_retention_amount__pc ?? 0;
        policy.POL_RETENTION_AMOUNT2_ = calc.p_ret2;
        policy.POL_RETENTION_AMOUNT2__LC = calc.p_pol_retention_amount2__lc;
        policy.POL_RETENTION_AMOUNT2__PC = calc.p_pol_retention_amount2__pc;
        policy.POL_CUR_ID = calc.p_pol_cur_id ?? "";
        policy.POL_CUR_ID_LC = calc.p_pol_cur_id_lc ?? "";
        policy.POL_CUR_ID_PC = calc.p_pol_cur_id_pc ?? "";
        policy.POL_EX_RATE_LC = calc.p_pol_ex_rate_lc ?? 0;
        policy.POL_EX_RATE_PC = calc.p_pol_ex_rate_pc ?? 0;
        policy.POL_TRAVEL_CODE = calc.p_pol_travel_code;

        // Tracking the modification
        policy.POL_MODIFIED_BY = _currentUserService.UserCode;
        policy.POL_MODIFICATION_DATE = DateTime.Now;
    }

    public async Task<CustomerViewPolicy> SavePolicy(CreatePolicyRequest req)
    {
        // Retrieve the correct User
        await _currentUserService.RetrieveUser();
        User UserInstance = _currentUserService.UserInstance();

        // Check if user is customer, if user is customer then check if customer is enabled by Trust Holding or not
        if (UserInstance.IsCustomer())
            if (!await controlPanelRepository.IsCustomerPortalEnabledAsync(UserInstance.CompanyGroupId))
                throw new ApplicationException("Customer Portal is not enabled by Trust Holding.");

        // Getting the right type of coverage based on PlanType and Destination country code
        int typeOfCoverage = TypeOfCoverageRepository.GetTypeOfCoverageCode(req.destinationCountryCode, req.planType);

        // this is policy serial to be assigned to policy object
        string? policySerial = await GeneratePolicyNumber();

        if (req is CreateSinglePolicyRequest request)
        {
            // Generate voucher number
            string voucherNumber = (await GenerateVoucherNumber()).ToString();

            var policy = await SaveSinglePolicy(request, typeOfCoverage, UserInstance, voucherNumber, policySerial, 0);

            await _db.SaveChangesAsync();
            return new CustomerViewSinglePolicy(policy);
        }
        else if (req is CreateGroupPolicyRequest groupRequest)
        {
            var groupPolicy = await SaveGroupPolicy(groupRequest, typeOfCoverage, UserInstance, policySerial);

            await _db.SaveChangesAsync();
            return new CustomerViewGroupPolicy(groupPolicy);
        }
        else throw new ApplicationException("Unknown policy request type.");
    }

    public async Task<CalculatePremiumResponseDTO> CalculatePremiumAsync(
        CalculatePremiumRequestDTO[] requests,
        double? manualDiscountPercent = null
    )
    {
        double basePremium = 0;
        foreach (var request in requests)
        {
            int typeOfCoverage = TypeOfCoverageRepository.GetTypeOfCoverageCode(request.destinationCountryCode!, request.planType);

            var procedureResults = await _db.CalculateTvAmount_Async(new DB_TV_PAL_PACKAGE.CalculateTvAmountInputDTO
            {
                p_period = request.duration,
                p_age = request.passengerAge,
                p_zone_code = typeOfCoverage.ToString() ?? "",
                p_atr_type = GetPremiumCalculationPlanType(request.planType),

                p_winter_flag = request.winterSkiFlag ? "W" : "0",
                p_pol_accedent_code = request.tripCancellationFlag ? "1" : "0",
                p_add_f01 = request.covidFlag ? 1 : 0

            }, new DB_TV_PAL_PACKAGE.CalculateTvAmountInputOutputDTO { });

            basePremium += procedureResults.p_prem ?? 0;
        }

        double automaticDiscountPercent = GetAutomaticGroupDiscountPercent(requests.Length);
        double appliedDiscountPercent = GetAppliedGroupDiscountPercent(
            requests.Length,
            manualDiscountPercent
        );
        double totalPremium = basePremium * (1 - appliedDiscountPercent / 100);

        return new CalculatePremiumResponseDTO
        {
            basePremium = basePremium,
            automaticDiscountPercent = automaticDiscountPercent,
            appliedDiscountPercent = appliedDiscountPercent,
            totalPremium = totalPremium
        };
    }

    async Task<string> GeneratePolicyNumber()
    {
        var getserialResults = await _db.GetSerial_Async(new DB_TV_PAL_PACKAGE.GetSerialInputDTO
        {
            p_pol_ofc_id = _currentUserService.UserInstance().CompanyOfficeId,
            p_pol_cbr_id = _currentUserService.UserInstance().CompanyBranchId,
            p_pol_cgp_id = _currentUserService.UserInstance().CompanyGroupId.ToString(),
            p_pol_cmp_id = _currentUserService.UserInstance().CompanyId.ToString()
        });
        return getserialResults?.ReturnValue?.ToString().PadLeft(5, '0')


        ?? throw new ApplicationException("Failed to generate policy number.");
    }

    async Task<string> GenerateVoucherNumber()
    {
        await _currentUserService.RetrieveUser();
        var companyBranchId = _currentUserService.UserInstance().CompanyBranchId;

        int desiredVoucherNumber = (await _db.TV_POLICIES.Where(p => p.POL_CGP_ID == 8 &&
                                                                    p.POL_VHH_FSY_YEAR == DateTime.Now.Year &&
                                                                    p.POL_VHH_VHT_ID == "TD" &&
                                                                    p.POL_CBR_ID == companyBranchId)
                                        .MaxAsync(policy => (int?)Convert.ToInt32(policy.POL_VHH_NUM)) ?? 0) + 1;

        return desiredVoucherNumber.ToString().PadLeft(6, '0');
    }

    async Task<TV_POLICIES> SaveSinglePolicy(
        CreateSinglePolicyRequest request,
        int typeOfCoverage,
        User UserInstance,

        string voucherNumber,
        string policySerial,
        int serial
    )
    {
        // Calculating Premium
        var calculateTvAmountResult = await _db.CalculateTvAmount_Async(new DB_TV_PAL_PACKAGE.CalculateTvAmountInputDTO
        {
            p_period = request.duration,
            p_age = DateTime.Now.Year - request.birthDate.Year,
            p_zone_code = typeOfCoverage.ToString(),
            p_atr_type = GetPremiumCalculationPlanType(request.planType),

            p_winter_flag = request.winterSkiFlag ? "W" : "0",
            p_pol_accedent_code = request.tripCancellationFlag ? "1" : "0",
            p_add_f01 = request.covidFlag ? 1 : 0

        }, new DB_TV_PAL_PACKAGE.CalculateTvAmountInputOutputDTO { });

        // Getting commission that we get if it was from customer portal
        var (newCustomerFeesPercent, existingCustomerFeesPercent)
        = await controlPanelRepository.GetCompanyNewAndExistingCustomerFeesPercentAsync(UserInstance.CompanyGroupId);

        var policyYear = DateTime.Now.Year;
        var polCreatedBy = UserInstance.Code;

        // Calculate broker values
        var brokerCommission = await agencyCommissionRepository.GetBrokerCommission(UserInstance.Office?.Id ??
            throw new ApplicationException("User doesn't have an office")
        );
        var brokerAmount = calculateTvAmountResult.p_prem * (brokerCommission / 100);

        Logger.LogCritical("Broker Commission: " + brokerCommission.ToString());
        Logger.LogCritical("Broker Amount: " + brokerAmount.ToString());

        var newPolicy = new TV_POLICIES
        {
            // Default Values or Useless info (I don't understand some of them but they are required in the legacy system so to maintain compatibility)
            POL_CLS_ID = policyClass,
            POL_DCT_ID = policyDocumentType,
            POL_ENDT_NO = "0",
            POL_ENDT_YEAR = 0,
            POL_ENDT_SERIAL = 0,
            POL_TA_BRANCH_CODE = "01",
            POL_PLT_ID = policyType,
            POL_VHH_PERIOD_NO = DateTime.Now.Month,
            POL_VHH_FSY_YEAR = DateTime.Now.Year,
            POL_VHH_VHT_ID = "TD",
            POL_VHH_DPT_ID = "9",
            POL_TRN_TYPE = "1",
            POL_PAYMENT_TERM = "1",
            POL_INT_SERIAL = serial,

            // Useless info (Can be provided from a given info or they are not used by the new system)
            POL_DISC_ID = 0,
            POL_PRINT_FLAG = 0,
            POL_TA_SLT_TYPE = 5,
            POL_YEAR = policyYear,
            POL_PROD_YEAR = DateTime.Now.Year,
            POL_PROD_MONTH = DateTime.Now.Month,
            POL_FEES1 = calculateTvAmountResult.p_fees1,
            POL_FEES2 = calculateTvAmountResult.p_fees2,
            POL_FEES3 = calculateTvAmountResult.p_fees3,
            POL_REINS_PREMIUM1 = calculateTvAmountResult.p_afr,
            POL_REINS_PREMIUM2 = calculateTvAmountResult.p_ccr,
            POL_FEES1_LC = calculateTvAmountResult.p_pol_fees1_lc,
            POL_FEES1_PC = calculateTvAmountResult.p_pol_fees1_pc,
            POL_FEES2_LC = calculateTvAmountResult.p_pol_fees2_lc,
            POL_FEES2_PC = calculateTvAmountResult.p_pol_fees2_pc,
            POL_FEES3_LC = calculateTvAmountResult.p_pol_fees3_lc,
            POL_FEES3_PC = calculateTvAmountResult.p_pol_fees3_pc,
            POL_FEES4 = calculateTvAmountResult.p_fees4,
            POL_FEES4_LC = calculateTvAmountResult.p_pol_fees4_lc,
            POL_FEES4_PC = calculateTvAmountResult.p_pol_fees4_pc,
            POL_TRUST_COMM2 = calculateTvAmountResult.p_acc_amman,
            POL_REINS_PREMIUM = calculateTvAmountResult.p_ccr ?? 0,
            POL_PREMIUM_AMOUNT2 = calculateTvAmountResult.p_prem2,
            POL_RETENTION_AMOUNT2_ = calculateTvAmountResult.p_ret2,
            POL_CUR_ID = calculateTvAmountResult.p_pol_cur_id ?? "",
            POL_PREMIUM_AMOUNT = calculateTvAmountResult.p_prem ?? 0,
            POL_RETENTION_AMOUNT_ = calculateTvAmountResult.p_ret ?? 0,
            POL_TRAVEL_CODE = calculateTvAmountResult.p_pol_travel_code,
            POL_CUR_ID_LC = calculateTvAmountResult.p_pol_cur_id_lc ?? "",
            POL_CUR_ID_PC = calculateTvAmountResult.p_pol_cur_id_pc ?? "",
            POL_TA_COMM_LC = calculateTvAmountResult.p_pol_ta_comm_lc ?? 0,
            POL_EX_RATE_LC = calculateTvAmountResult.p_pol_ex_rate_lc ?? 0,
            POL_EX_RATE_PC = calculateTvAmountResult.p_pol_ex_rate_pc ?? 0,
            POL_TA_COMM_PC = calculateTvAmountResult.p_pol_ta_comm_pc ?? 0,
            POL_TRUST_COMM_LC = calculateTvAmountResult.p_pol_trust_comm_lc ?? 0,
            POL_TRUST_COMM_PC = calculateTvAmountResult.p_pol_trust_comm_pc ?? 0,
            POL_TRUST_COMM2_LC = calculateTvAmountResult.p_pol_trust_comm2_lc ?? 0,
            POL_TRUST_COMM2_PC = calculateTvAmountResult.p_pol_trust_comm2_pc ?? 0,
            POL_REINS_PREMIUM2_LC = calculateTvAmountResult.p_pol_reins_premium_lc,
            POL_REINS_PREMIUM2_PC = calculateTvAmountResult.p_pol_reins_premium_pc,
            POL_REINS_PREMIUM1_LC = calculateTvAmountResult.p_pol_reins_premium1_lc,
            POL_REINS_PREMIUM1_PC = calculateTvAmountResult.p_pol_reins_premium1_pc,
            POL_PREMIUM_AMOUNT2_LC = calculateTvAmountResult.p_pol_premium_amount2_lc,
            POL_PREMIUM_AMOUNT2_PC = calculateTvAmountResult.p_pol_premium_amount2_pc,
            POL_REINS_PREMIUM_PC = calculateTvAmountResult.p_pol_reins_premium_pc ?? 0,
            POL_REINS_PREMIUM_LC = calculateTvAmountResult.p_pol_reins_premium_lc ?? 0,
            POL_PREMIUM_AMOUNT_LC = calculateTvAmountResult.p_pol_premium_amount_lc ?? 0,
            POL_PREMIUM_AMOUNT_PC = calculateTvAmountResult.p_pol_premium_amount_pc ?? 0,
            POL_RETENTION_AMOUNT2__LC = calculateTvAmountResult.p_pol_retention_amount2__lc,
            POL_RETENTION_AMOUNT2__PC = calculateTvAmountResult.p_pol_retention_amount2__pc,
            POL_RETENTION_AMOUNT__LC = calculateTvAmountResult.p_pol_retention_amount__lc ?? 0,
            POL_RETENTION_AMOUNT__PC = calculateTvAmountResult.p_pol_retention_amount__pc ?? 0,

            // Those are for the brokers
            POL_TA_COMM_PCNT = brokerCommission,
            POL_TA_COMM = brokerAmount,

            // This property will be used by customer portal management fees
            POL_TRUST_COMM = (calculateTvAmountResult.p_prem * (double)newCustomerFeesPercent) ?? 0,

            // Secondary Info
            POL_NO = policySerial,
            POL_CMP_ID = UserInstance.CompanyId,
            POL_CGP_ID = UserInstance.CompanyGroupId,
            POL_SST_SERIAL = int.Parse(policySerial),
            POL_CBR_ID = UserInstance.CompanyBranchId,
            POL_OFC_ID = UserInstance.CompanyOfficeId,

            // Base Info
            // # Personal Info 
            POL_PASSEGER_1ST_NAME_NTV = request.firstNameNative,
            POL_PASSEGER_FATHER_NAME_NTV = request.fatherNameNative,
            POL_PASSEGER_LAST_NAME_NTV = request.lastNameNative,

            POL_PASSEGER_1ST_NAME_FRN = request.firstNameForeign,
            POL_PASSEGER_FATHER_NAME_FRN = request.fatherNameForeign,
            POL_PASSEGER_LAST_NAME_FRN = request.lastNameForeign,

            POL_PASSEGER_ADDRESS_NTV = request.addressNative,
            POL_PASSEGER_ADDRESS_FRN = request.addressForeign,

            POL_PASSEGER_TELEPHONE = request.phoneNumber,
            POL_PASSEGER_FAX = request.fax,

            POL_PASSEGER_PASSPORT_NUMBER = request.passportNumber,
            POL_PASSEGER_BIRTH_DATE = request.birthDate.ToDateTime(TimeOnly.MinValue),
            POL_PASSEGER_AGE = DateTime.Now.Year - request.birthDate.Year,
            POL_PASSEGER_GENDER = request.passengerGender,
            POL_PASSEGER_NATIONALITY_ID = request.nationalityCountryCode,

            // # Policy Info
            POL_POLICY_NO = policySerial,
            POL_REF_KEY = UserInstance.CompanyOfficeId + " T01 " + policyYear + " " + policySerial,
            POL_ATR_TYPE = request.planType,
            POL_CREATED_BY = polCreatedBy,

            POL_VHH_NUM = voucherNumber?.ToString(),
            POL_TA_CODE = (UserInstance.Role == 2) ?
                request.AccountCode ?? UserInstance.CashAccount : UserInstance.CashAccount,

            POL_AREA_CODE = typeOfCoverage.ToString(),
            POL_STATUS = "I",

            POL_ISSUE_DATE = DateTime.Now,
            POL_CREATION_DATE = DateTime.Now,

            POL_INCEPTION_DATE = request.startDate.ToDateTime(TimeOnly.MinValue),
            POL_PERIOD = request.duration,
            POL_EXPIRY_DATE = request.startDate.AddDays(request.duration).ToDateTime(TimeOnly.MinValue),

            POL_WINTER_FLAG = request.winterSkiFlag ? "W" : null,
            POL_ACCEDENT_CODE = request.tripCancellationFlag ? "1" : "0",

            POL_SOURCE = request.originCountryCode,
            POL_DESTINATION = request.destinationCountryCode,
        };
        await _db.TV_POLICIES.AddAsync(newPolicy);

        var newPolicyAdditional = new TV_POLICIES_ADDITIONAL()
        {
            ADD_POL_NO = policySerial,
            ADD_POL_YEAR = policyYear,
            ADD_POL_ENDT_NO = "0",
            ADD_POL_ENDT_YEAR = 0,
            ADD_POL_CLS_ID = policyClass,
            ADD_POL_PLT_ID = policyType,
            ADD_POL_CMP_ID = UserInstance.CompanyId,
            ADD_POL_CGP_ID = UserInstance.CompanyGroupId,
            ADD_POL_DCT_ID = policyDocumentType,
            ADD_POL_OFC_ID = UserInstance.CompanyOfficeId,
            ADD_POL_CBR_ID = UserInstance.CompanyBranchId,
            ADD_POL_INT_SERIAL = serial,

            ADD_F01 = request.covidFlag ? "1" : "0"
        };
        await _db.TV_POLICIES_ADDITIONAL.AddAsync(newPolicyAdditional);

        return newPolicy;
    }

    async Task<TV_POLICY_PACKAGE> SaveGroupPolicy(
        CreateGroupPolicyRequest groupRequest,
        int typeOfCoverage,
        User UserInstance,

        string policySerialNumber
    )
    {
        if (groupRequest.policies.Count < 10)
            throw new ApplicationException("Group policy must contain at least 10 sub-policies.");

        double groupDiscountPercent = GetAppliedGroupDiscountPercent(
            groupRequest.policies.Count,
            groupRequest.manualDiscountPercent
        );

        var groupPolicy = new TV_POLICY_PACKAGE
        {
            TPP_STATUS = "I",
            TPP_DCT_ID = "1",
            TPP_CLS_ID = "T",
            TPP_ENDT_YEAR = 0,
            TPP_ENDT_NO = "0",
            TPP_CUR_ID = "31",
            TPP_CUR_ID_LC = "31",
            TPP_CUR_ID_PC = "00",
            TPP_DISC_ID = 9,
            TPP_PRINT_FLAG = 0,
            TPP_ATR_TYPE = 3,
            TPP_PAYMENT_TERM = "2",
            TPP_EX_RATE_LC = 1,
            TPP_EX_RATE_PC = 3.725,

            TPP_PLT_ID = policyType,
            TPP_YEAR = DateTime.Now.Year,

            TPP_POL_NO = policySerialNumber,
            TPP_PERIOD = groupRequest.duration,
            TPP_CMP_ID = UserInstance.CompanyId,
            TPP_CGP_ID = UserInstance.CompanyGroupId,
            TPP_CBR_ID = UserInstance.CompanyBranchId,
            TPP_OFC_ID = UserInstance.CompanyOfficeId,
            TPP_NAME_NTV = groupRequest.companyNameNative,
            TPP_NAME_FRN = groupRequest.companyNameForeign,
            TPP_ISSUE_DATE = DateTime.Now,
            TPP_INCEPTION_DATE = groupRequest.startDate.ToDateTime(TimeOnly.MinValue),
            TPP_EXPIRY_DATE = groupRequest.startDate.AddDays(groupRequest.duration).ToDateTime(TimeOnly.MinValue),
            TPP_AREA_CODE = typeOfCoverage.ToString(),
            TPP_SOURCE = groupRequest.originCountryCode,
            TPP_DESTINATION = groupRequest.destinationCountryCode,

            TPP_CREATED_BY = UserInstance.Code,
            TPP_CREATION_DATE = DateTime.Now,

            TPP_TA_CODE = UserInstance.CashAccount,
            TPP_MEMBERS = groupRequest.policies.Count,
        };

        await _db.AddAsync(groupPolicy);
        groupPolicy.subPolicies = [];

        // Generate voucher for sub policies
        string voucherNumber = (await GenerateVoucherNumber()).ToString();

        // Add Policies
        for (int index = 1; index <= groupRequest.policies.Count; index++)
        {
            var subPolicy = groupRequest.policies[index - 1];

            // Preparing payload to save single policy in DB
            var createSinglePolicyPayload = new CreateSinglePolicyRequest
            {
                planType = 3,
                originCountryCode = groupRequest.originCountryCode,
                destinationCountryCode = groupRequest.destinationCountryCode,

                firstNameNative = subPolicy.firstNameNative,
                fatherNameNative = subPolicy.fatherNameNative,
                lastNameNative = subPolicy.lastNameNative,

                firstNameForeign = subPolicy.firstNameForeign,
                fatherNameForeign = subPolicy.fatherNameForeign,
                lastNameForeign = subPolicy.lastNameForeign,

                addressForeign = subPolicy.addressForeign,
                addressNative = subPolicy.addressNative,
                birthDate = subPolicy.birthDate,

                duration = groupRequest.duration,
                passportNumber = subPolicy.passportNumber,
                nationalityCountryCode = subPolicy.nationalityCountryCode,
                passengerGender = subPolicy.passengerGender,
                phoneNumber = subPolicy.phoneNumber,
                fax = subPolicy.fax,

                startDate = groupRequest.startDate,
                AccountCode = groupRequest.AccountCode,

                covidFlag = subPolicy.covidFlag,
                tripCancellationFlag = subPolicy.tripCancellationFlag,
                winterSkiFlag = subPolicy.winterSkiFlag,
            };

            // Saving subPolicies
            await SaveSinglePolicy(
                createSinglePolicyPayload,
                typeOfCoverage,
                UserInstance,

                voucherNumber,
                policySerialNumber,
                index
            );
        }

        // Apply the effective group discount to every member's premium.
        foreach (var subPolicy in groupPolicy.subPolicies)
            subPolicy.POL_PREMIUM_AMOUNT *= 1 - groupDiscountPercent / 100;

        return groupPolicy;
    }

    private double GetAppliedGroupDiscountPercent(int memberCount, double? manualDiscountPercent)
    {
        double automaticDiscountPercent = GetAutomaticGroupDiscountPercent(memberCount);
        if (!manualDiscountPercent.HasValue ||
            !_currentUserService.IsAuthenticated ||
            _currentUserService.Role != 2)
            return automaticDiscountPercent;

        if (manualDiscountPercent.Value is < 0 or > 100)
            throw new ApplicationException("Manual group discount must be between 0 and 100 percent.");

        return manualDiscountPercent.Value;
    }

    private static int GetPremiumCalculationPlanType(int planType) =>
        planType == 3 ? 1 : planType;

    private static double GetAutomaticGroupDiscountPercent(int memberCount) =>
        memberCount switch
        {
            < 10 => 0,
            <= 20 => 5,
            <= 50 => 10,
            <= 100 => 15,
            <= 250 => 20,
            _ => 25
        };
}