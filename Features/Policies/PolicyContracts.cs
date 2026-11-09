using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.DTOs
{
    [JsonConverter(typeof(PolicyRequestConverter))]
    public abstract class CreatePolicyRequest
    {
        [Required]
        public required int planType { get; set; }

        [Required]
        public required DateOnly startDate { get; set; }
        [Required]
        public required int duration { get; set; }
        [Required]
        public required string originCountryCode { get; set; }
        [Required]
        public required string destinationCountryCode { get; set; }

        public string? AccountCode { get; set; }
    }

    public class CreateSinglePolicyRequest : CreatePolicyRequest
    {
        [Required]
        public required string firstNameNative { get; set; }
        [Required]
        public required string lastNameNative { get; set; }
        public string? firstNameForeign { get; set; }
        public string? lastNameForeign { get; set; }

        public string? addressNative { get; set; }
        public string? addressForeign { get; set; }


        [Required]
        public required string phoneNumber { get; set; }
        public string? fax { get; set; }
        [Required]
        public required string passportNumber { get; set; }

        [Required]
        public required DateOnly birthDate { get; set; }


        public int passengerGender { get; set; } = 1;

        [Required]
        public string? nationalityCountryCode { get; set; }

        public string? fatherNameNative { get; set; }
        public string? fatherNameForeign { get; set; }

        public bool tripCancellationFlag { get; set; } = false;
        public bool winterSkiFlag { get; set; } = false;
        public bool covidFlag { get; set; } = false;
    }

    public class CreateGroupPolicyRequest : CreatePolicyRequest
    {
        [Required]
        public required string companyNameNative { get; set; }
        public string? companyNameForeign { get; set; }

        [Required]
        public required List<GroupSubPolicy> policies { get; set; }

        [Range(0, 100)]
        public double? manualDiscountPercent { get; set; }
    }

    public class GroupSubPolicy
    {
        [Required]
        public required string firstNameNative { get; set; }
        [Required]
        public required string lastNameNative { get; set; }
        public string? firstNameForeign { get; set; }
        public string? lastNameForeign { get; set; }

        public string? addressNative { get; set; }
        public string? addressForeign { get; set; }


        [Required]
        public required string phoneNumber { get; set; }
        public string? fax { get; set; }
        [Required]
        public required string passportNumber { get; set; }

        [Required]
        public required DateOnly birthDate { get; set; }

        public int passengerGender { get; set; } = 1;
        public string? nationalityCountryCode { get; set; }

        public string? fatherNameNative { get; set; }
        public string? fatherNameForeign { get; set; }

        public bool tripCancellationFlag { get; set; } = false;
        public bool winterSkiFlag { get; set; } = false;
        public bool covidFlag { get; set; } = false;
    }

    public class PrintPolicyRequestDTO
    {
        public bool printAsDraft { get; set; } = true;

        [Required]
        public required string policyNumber { get; set; }
    }

    public class CancelPolicyRequestDTO
    {
        [Required]
        public required string policyNumber { get; set; }

        [Required]
        public required int policySerial { get; set; }

        public string userCode { get; set; } = "Travel System";
        public int trnType = 1;
    }

    [JsonPolymorphic]
    [JsonDerivedType(typeof(AgentViewSinglePolicy))]
    [JsonDerivedType(typeof(AgentViewGroupPolicy))]
    public abstract record AgentViewPolicy(DateOnly issueDate, PolicyStatus status)
    {
        public PolicyStatus policyStatus { get; set; } = status;
    }

    public record AgentViewGroupPolicy([property: JsonIgnore] TV_POLICY_PACKAGE policy) : AgentViewPolicy(DateOnly.FromDateTime(policy.TPP_ISSUE_DATE)
    , policy.TPP_STATUS == "C" ? PolicyStatus.Endorsed : (policy.TPP_PRINT_FLAG > 0 ? PolicyStatus.Generated : PolicyStatus.Draft))
    {
       public string policyNumber { get; set; } = $"{policy.TPP_OFC_ID} {policy.TPP_PLT_ID} {policy.TPP_YEAR} {policy.TPP_POL_NO}{(policy.TPP_STATUS == "C" ? " C" : "")}";
        public DateOnly startDate { get; set; } = DateOnly.FromDateTime(policy.TPP_INCEPTION_DATE);
        public int duration { get; set; } = policy.TPP_PERIOD;

        public string companyNameNative { get; set; } = policy.TPP_NAME_NTV ?? "";
        public string? companyNameForeign { get; set; } = policy.TPP_NAME_FRN ?? "";
        public string originCountryCode { get; set; } = policy.TPP_SOURCE ?? "";
        public string destinationCountryCode { get; set; } = policy.TPP_DESTINATION ?? "";

        public int typeOfCoverage { get; set; } = int.Parse(policy.TPP_AREA_CODE);
        public int? planType { get; set; } = 3;

        public string createdBy { get; set; } = policy.TPP_CREATED_BY ?? "";
        public string officeId { get; set; } = policy.TPP_OFC_ID ?? "";
        public bool isEndorsed { get; set; } = policy.TPP_STATUS == "C";
        public List<AgentViewSinglePolicy> policies { get; set; } =
        policy.subPolicies?.Select(policy => new AgentViewSinglePolicy(policy)).OrderBy(p => p.serial).ToList()
        ?? [];
    }

    public record AgentViewSinglePolicy([property: JsonIgnore] TV_POLICIES policy) : AgentViewPolicy(DateOnly.FromDateTime(policy.POL_ISSUE_DATE)
    , policy.POL_STATUS == "C" ? PolicyStatus.Endorsed : (policy.POL_PRINT_FLAG > 0 ? PolicyStatus.Generated : PolicyStatus.Draft))
    {
        public string referenceKey { get; set; } = policy.POL_REF_KEY ?? "";
        public DateOnly startDate { get; set; } = DateOnly.FromDateTime(policy.POL_INCEPTION_DATE);
        public int duration { get; set; } = policy.POL_PERIOD;

        public string firstName { get; set; } = policy.POL_PASSEGER_1ST_NAME_NTV ?? "";
        public string lastName { get; set; } = policy.POL_PASSEGER_LAST_NAME_NTV ?? "";
        public string? firstNameForeign { get; set; } = policy.POL_PASSEGER_1ST_NAME_FRN ?? "";
        public string? lastNameForeign { get; set; } = policy.POL_PASSEGER_LAST_NAME_FRN ?? "";

        public string? addressNative { get; set; } = policy.POL_PASSEGER_ADDRESS_NTV ?? "";
        public string? addressForeign { get; set; } = policy.POL_PASSEGER_ADDRESS_FRN ?? "";

        public string phoneNumber { get; set; } = policy.POL_PASSEGER_TELEPHONE ?? "";
        public string? fax { get; set; } = policy.POL_PASSEGER_FAX ?? "";
        public string passportNumber { get; set; } = policy.POL_PASSEGER_PASSPORT_NUMBER ?? "";

        public DateOnly birthDate { get; set; } = DateOnly.FromDateTime(policy.POL_PASSEGER_BIRTH_DATE);
        public int typeOfCoverage { get; set; } = int.Parse(policy.POL_AREA_CODE);
        public int? planType { get; set; } = policy.POL_ATR_TYPE;

        public int? passengerGender { get; set; } = policy.POL_PASSEGER_GENDER ?? null;
        public string? nationalityCountry { get; set; } = policy.POL_PASSEGER_NATIONALITY_ID;
        public string originCountryCode { get; set; } = policy.POL_SOURCE ?? "";
        public string destinationCountryCode { get; set; } = policy.POL_DESTINATION ?? "";

        public string? passengerFatherNameNative { get; set; } = policy.POL_PASSEGER_FATHER_NAME_NTV;
        public string? passengerFatherNameForeign { get; set; } = policy.POL_PASSEGER_FATHER_NAME_FRN;

        public bool tripCancellationFlag { get; set; } = policy.POL_ACCEDENT_CODE == "1" ? true : false;
        public bool winterSkiFlag { get; set; } = policy.POL_WINTER_FLAG == "W" ? true : false;
        public bool covidFlag { get; set; } = policy.CovidFlag;

        public DateOnly creationDate { get; set; } = DateOnly.FromDateTime(policy.POL_CREATION_DATE);
        public double totalPremium { get; set; } = policy.POL_PREMIUM_AMOUNT;
        public int transactionType { get; set; } = int.Parse(policy.POL_TRN_TYPE ?? "0");
        public string officeId { get; set; } = policy.POL_OFC_ID ?? "";
        public string createdBy { get; set; } = policy.POL_CREATED_BY ?? "";
        public int serial { get; set; } = policy.POL_INT_SERIAL;
    }

    public abstract record CustomerViewPolicy(int? planType, PolicyStatus status)
    {
        public int? planType { get; set; } = planType;
        public PolicyStatus policyStatus { get; set; } = status;
    }
    public record CustomerViewSinglePolicy([property: JsonIgnore] TV_POLICIES policy) : CustomerViewPolicy(policy.POL_ATR_TYPE
    , policy.POL_STATUS == "C" ? PolicyStatus.Endorsed : (policy.POL_PRINT_FLAG > 0 ? PolicyStatus.Generated : PolicyStatus.Draft))
    {
        public string referenceKey { get; set; } = policy.POL_REF_KEY ?? "";
        public string fullNameNative { get; set; } = $"{policy.POL_PASSEGER_1ST_NAME_NTV} {policy.POL_PASSEGER_FATHER_NAME_NTV} {policy.POL_PASSEGER_LAST_NAME_NTV}";
        public string? fullNameForeign { get; set; } = $"{policy.POL_PASSEGER_1ST_NAME_FRN} {policy.POL_PASSEGER_FATHER_NAME_FRN} {policy.POL_PASSEGER_LAST_NAME_FRN}";
        public DateOnly birthDate { get; set; } = DateOnly.FromDateTime(policy.POL_PASSEGER_BIRTH_DATE);
        public int? passengerGender { get; set; } = policy.POL_PASSEGER_GENDER ?? null;
        public string? nationalityCountry { get; set; } = policy.POL_PASSEGER_NATIONALITY_ID;
        public string passportNumber { get; set; } = policy.POL_PASSEGER_PASSPORT_NUMBER ?? "";
        public int duration { get; set; } = policy.POL_PERIOD;
        public string destinationCountryCode { get; set; } = policy.POL_DESTINATION ?? "";
        public DateOnly startDate { get; set; } = DateOnly.FromDateTime(policy.POL_INCEPTION_DATE);

        public string firstName { get; set; } = policy.POL_PASSEGER_1ST_NAME_NTV ?? "";
        public string lastName { get; set; } = policy.POL_PASSEGER_LAST_NAME_NTV ?? "";
        public string? firstNameForeign { get; set; } = policy.POL_PASSEGER_1ST_NAME_FRN ?? "";
        public string? lastNameForeign { get; set; } = policy.POL_PASSEGER_LAST_NAME_FRN ?? "";
        public int typeOfCoverage { get; set; } = int.Parse(policy.POL_AREA_CODE);
        public string? addressNative { get; set; } = policy.POL_PASSEGER_ADDRESS_NTV ?? "";
        public string? addressForeign { get; set; } = policy.POL_PASSEGER_ADDRESS_FRN ?? "";
        public string phoneNumber { get; set; } = policy.POL_PASSEGER_TELEPHONE ?? "";

        public bool tripCancellationFlag { get; set; } = policy.POL_ACCEDENT_CODE == "1" ? true : false;
        public bool winterSkiFlag { get; set; } = policy.POL_WINTER_FLAG == "W" ? true : false;
        public bool covidFlag { get; set; } = policy.CovidFlag;

        public double totalPremium { get; set; } = policy.POL_PREMIUM_AMOUNT;
        public double isActive { get; set; } = policy.POL_STATUS == "I" ? 1 : 0;
    }

    public record CustomerViewGroupPolicy([property: JsonIgnore] TV_POLICY_PACKAGE policy) : CustomerViewPolicy(3
    , policy.TPP_STATUS == "C" ? PolicyStatus.Endorsed : (policy.TPP_PRINT_FLAG > 0 ? PolicyStatus.Generated : PolicyStatus.Draft))
    {
        public string policyNumber { get; set; } = $"{policy.TPP_OFC_ID} {policy.TPP_PLT_ID} {policy.TPP_YEAR} {policy.TPP_POL_NO}{(policy.TPP_STATUS == "C" ? " C" : "")}";
        public DateOnly startDate { get; set; } = DateOnly.FromDateTime(policy.TPP_INCEPTION_DATE);
        public int duration { get; set; } = policy.TPP_PERIOD;

        public string companyNameNative { get; set; } = policy.TPP_NAME_NTV ?? "";
        public string? companyNameForeign { get; set; } = policy.TPP_NAME_FRN ?? "";
        public string originCountryCode { get; set; } = policy.TPP_SOURCE ?? "";
        public string destinationCountryCode { get; set; } = policy.TPP_DESTINATION ?? "";

        public int typeOfCoverage { get; set; } = int.Parse(policy.TPP_AREA_CODE);
        public List<AgentViewSinglePolicy> policies { get; set; } = policy.subPolicies?.Select(policy => new AgentViewSinglePolicy(policy)).ToList() ?? new List<AgentViewSinglePolicy>();
    }

    public class CalculatePremiumRequestDTO
    {
        [Required]
        public required int duration { get; set; }

        [Required]
        public required int passengerAge { get; set; }

        public string? destinationCountryCode { get; set; }

        [Required]
        public required int planType { get; set; }

        public bool winterSkiFlag { get; set; } = false;

        public bool tripCancellationFlag { get; set; } = false;

        public bool covidFlag { get; set; } = false;
    }

    public class CalculatePremiumResponseDTO
    {
        public double? basePremium { get; set; }
        public double automaticDiscountPercent { get; set; }
        public double appliedDiscountPercent { get; set; }
        public double? totalPremium { get; set; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "policyType")]
    [JsonDerivedType(typeof(RetrieveSinglePolicyRequestDTO), "single")]
    [JsonDerivedType(typeof(RetrieveGroupPolicyRequestDTO), "group")]
    public abstract class RetrievePolicyRequestDTO
    {
        [JsonIgnore]
        public string? PolicyType { get; set; }
    }

    public class RetrieveSinglePolicyRequestDTO : RetrievePolicyRequestDTO
    {
        [Required]
        public required string passengerLastNameNative { get; set; }
        [Required]
        public required string passengerPassportNumber { get; set; }
    }

    public class RetrieveGroupPolicyRequestDTO : RetrievePolicyRequestDTO
    {
        [Required]
        public required string policyRefNumber { get; set; }
        [Required]
        public required string companyName { get; set; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "single_group")]
    [JsonDerivedType(typeof(InquirySinglePolicyRequestDTO), "single")]
    [JsonDerivedType(typeof(InquiryGroupPolicyRequestDTO), "group")]
    public abstract class InquiryPolicyRequestDTO
    {
        // [JsonIgnore]
        public string? Single_group { get; set; }
        public string? policyRefNumber { get; set; }
        public DateOnly? toDate { get; set; }
        public DateOnly? fromDate { get; set; }
        public string? officeId { get; set; }
    }

    public class InquirySinglePolicyRequestDTO : InquiryPolicyRequestDTO
    {
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? passportNumber { get; set; }
        public int? policyType { get; set; }
        public string? policyNum { get; set; }
        public int? period { get; set; }
        public int? serial { get; set; }
    }

    public class InquiryGroupPolicyRequestDTO : InquiryPolicyRequestDTO
    {
        public string? companyName { get; set; }
    }

    public class IsPaidRequestDTO
    {
        [Required]
        public required string policyNumber { get; set; }
    }

    public class IsPaidResponseDTO
    {
        public required bool isPaid { get; set; }
    }
}

public class PolicyRequestConverter : JsonConverter<CreatePolicyRequest>
{
    public override CreatePolicyRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var planType = root.TryGetProperty("planType", out var planTypeElement)
            ? planTypeElement.GetInt32()
            : 1;

        var targetType = planType == 3
            ? typeof(CreateGroupPolicyRequest)
            : typeof(CreateSinglePolicyRequest);

        return (CreatePolicyRequest)JsonSerializer.Deserialize(root.GetRawText(), targetType, options)!;
    }

    public override void Write(Utf8JsonWriter writer, CreatePolicyRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}