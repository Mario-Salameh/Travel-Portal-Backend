using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("TV_POLICY_PACKAGE")]
public class TV_POLICY_PACKAGE : Policy
{
    [Column("TPP_POL_NO")]
    public required string TPP_POL_NO { get; set; }

    [Column("TPP_CLS_ID")]
    public required string TPP_CLS_ID { get; set; }

    [Column("TPP_PLT_ID")]
    public string? TPP_PLT_ID { get; set; }

    [Column("TPP_YEAR")]
    public int TPP_YEAR { get; set; }

    [Column("TPP_DCT_ID")]
    public required string TPP_DCT_ID { get; set; }

    [Column("TPP_CBR_ID")]
    public required string TPP_CBR_ID { get; set; }

    [Column("TPP_OFC_ID")]
    public required string TPP_OFC_ID { get; set; }
    public CompanyOffice? CompanyOffice { get; set; }

    [Column("TPP_CMP_ID")]
    public int TPP_CMP_ID { get; set; }

    [Column("TPP_CGP_ID")]
    public int TPP_CGP_ID { get; set; }

    [Column("TPP_ENDT_NO")]
    public required string TPP_ENDT_NO { get; set; }

    [Column("TPP_ENDT_YEAR")]
    public int TPP_ENDT_YEAR { get; set; }

    [Column("TPP_NAME_NTV")]
    public string? TPP_NAME_NTV { get; set; }

    [Column("TPP_NAME_FRN")]
    public string? TPP_NAME_FRN { get; set; }

    [Column("TPP_INCEPTION_DATE")]
    public DateTime TPP_INCEPTION_DATE { get; set; }

    [Column("TPP_EXPIRY_DATE")]
    public DateTime TPP_EXPIRY_DATE { get; set; }

    [Column("TPP_PERIOD")]
    public int TPP_PERIOD { get; set; }

    [Column("TPP_ISSUE_DATE")]
    public DateTime TPP_ISSUE_DATE { get; set; }

    [Column("TPP_WINTER_FLAG")]
    public string? TPP_WINTER_FLAG { get; set; }

    [Column("TPP_STATUS")]
    public string? TPP_STATUS { get; set; }
    public override PolicyStatus PolicyStatus
    {
        get
        {
            if (TPP_STATUS == "C") return PolicyStatus.Endorsed;
            if (TPP_PRINT_FLAG > 0) return PolicyStatus.Generated;
            else return PolicyStatus.Draft;
        }
    }

    [Column("TPP_MERGE_FLAG")]
    public int? TPP_MERGE_FLAG { get; set; }

    [Column("TPP_AREA_CODE")]
    public required string TPP_AREA_CODE { get; set; }
    public TypeOfCoverage? TypeOfCoverage { get; set; }

    [Column("TPP_CUR_ID")]
    public required string TPP_CUR_ID { get; set; }

    [Column("TPP_CUR_ID_LC")]
    public string? TPP_CUR_ID_LC { get; set; }

    [Column("TPP_CUR_ID_PC")]
    public string? TPP_CUR_ID_PC { get; set; }

    [Column("TPP_EX_RATE_LC")]
    public double? TPP_EX_RATE_LC { get; set; }

    [Column("TPP_EX_RATE_PC")]
    public double? TPP_EX_RATE_PC { get; set; }

    [Column("TPP_SOURCE")]
    public string? TPP_SOURCE { get; set; }

    [Column("TPP_DESTINATION")]
    public string? TPP_DESTINATION { get; set; }
    public Country? DestinationCountry { get; set; }

    [Column("TPP_CREATED_BY")]
    public required string TPP_CREATED_BY { get; set; }
    public User? CreatedBy { get; set; }

    [Column("TPP_CREATION_DATE")]
    public DateTime TPP_CREATION_DATE { get; set; }

    [Column("TPP_MODIFIED_BY")]
    public string? TPP_MODIFIED_BY { get; set; }

    [Column("TPP_MODIFICATION_DATE")]
    public DateTime? TPP_MODIFICATION_DATE { get; set; }

    [Column("TPP_FEES1")]
    public double? TPP_FEES1 { get; set; }

    [Column("TPP_FEES1_LC")]
    public double? TPP_FEES1_LC { get; set; }

    [Column("TPP_FEES1_PC")]
    public double? TPP_FEES1_PC { get; set; }

    [Column("TPP_FEES2")]
    public double? TPP_FEES2 { get; set; }

    [Column("TPP_FEES2_LC")]
    public double? TPP_FEES2_LC { get; set; }

    [Column("TPP_FEES2_PC")]
    public double? TPP_FEES2_PC { get; set; }

    [Column("TPP_FEES3")]
    public double? TPP_FEES3 { get; set; }

    [Column("TPP_FEES3_LC")]
    public double? TPP_FEES3_LC { get; set; }

    [Column("TPP_FEES3_PC")]
    public double? TPP_FEES3_PC { get; set; }

    [Column("TPP_FEES4")]
    public double? TPP_FEES4 { get; set; }

    [Column("TPP_FEES4_LC")]
    public double? TPP_FEES4_LC { get; set; }

    [Column("TPP_FEES4_PC")]
    public double? TPP_FEES4_PC { get; set; }

    [Column("TPP_DISC_ID")]
    public int? TPP_DISC_ID { get; set; }

    [Column("TPP_MEMBERS")]
    public int? TPP_MEMBERS { get; set; }

    [Column("TPP_PRINT_FLAG")]
    public int? TPP_PRINT_FLAG { get; set; }

    [Column("TPP_ATR_TYPE")]
    public int? TPP_ATR_TYPE { get; set; }

    [Column("TPP_TA_CODE")]
    public string? TPP_TA_CODE { get; set; }

    [Column("TPP_PAYMENT_TERM")]
    public string? TPP_PAYMENT_TERM { get; set; }

    [Column("TPP_ADDRESS_NTV")]
    public string? TPP_ADDRESS_NTV { get; set; }

    [Column("TPP_ADDRESS_FRN")]
    public string? TPP_ADDRESS_FRN { get; set; }

    [Column("TPP_CRC")]
    public string? TPP_CRC { get; set; }

    public ICollection<TV_POLICIES>? subPolicies { get; set; }

    public override int PlanType => TPP_ATR_TYPE ?? throw new ApplicationException("Policy doesn't have a valid type");

    public bool IsPaid()
    {
        return TPP_PRINT_FLAG > 0;
    }

    public bool IsCreatedByCustomerPortal => TPP_CREATED_BY == "CSTMR_USER";

    public decimal TotalPremium
    {
        get
        {
            decimal total = 0;
            foreach (var subPolicy in subPolicies ?? Enumerable.Empty<TV_POLICIES>())
                total += (decimal)subPolicy.POL_PREMIUM_AMOUNT;

            return total;
        }
    }

    public override TypeOfCoverage? TypeOfCoverageGetter => TypeOfCoverage;

    public override string PolicyNumberGetter => TPP_OFC_ID + " " + TPP_PLT_ID + " " + TPP_YEAR + TPP_POL_NO;

    public override bool CovidFlag => subPolicies?.Any(subPolicy => subPolicy.additionalInfo?.CovidFlag ?? false) ?? false;
    public override bool WinterFlag => subPolicies?.Any(subPolicy => subPolicy.WinterFlag) ?? false;
    public override bool AccidentFlag => subPolicies?.Any(subPolicy => subPolicy.AccidentFlag) ?? false;

    public override string VoucherNumber
        => (subPolicies?.FirstOrDefault()
            ?? throw new ApplicationException($"Group Policy ({TPP_POL_NO}) doesn't have any member!"))
            .VoucherNumber;
}
