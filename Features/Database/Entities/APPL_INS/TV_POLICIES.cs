using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("TV_POLICIES")]
public class TV_POLICIES : Policy
{
    [Column("POL_NO")]
    public required string POL_NO { get; set; }

    [Column("POL_CLS_ID")]
    public required string POL_CLS_ID { get; set; }

    [Column("POL_YEAR")]
    public int POL_YEAR { get; set; }

    [Column("POL_DCT_ID")]
    public required string POL_DCT_ID { get; set; }

    [Column("POL_CBR_ID")]
    public required string POL_CBR_ID { get; set; }

    [Column("POL_OFC_ID")]
    public required string POL_OFC_ID { get; set; }
    public CompanyOffice? CompanyOffice { get; set; }

    [Column("POL_CMP_ID")]
    public int POL_CMP_ID { get; set; }

    [Column("POL_CGP_ID")]
    public int POL_CGP_ID { get; set; }

    [Column("POL_SST_SERIAL")]
    public int POL_SST_SERIAL { get; set; }

    [Column("POL_ENDT_NO")]
    public required string POL_ENDT_NO { get; set; }

    [Column("POL_ENDT_YEAR")]
    public int POL_ENDT_YEAR { get; set; }

    [Column("POL_ENDT_SERIAL")]
    public int? POL_ENDT_SERIAL { get; set; }

    [Column("POL_INCEPTION_DATE")]
    public DateTime POL_INCEPTION_DATE { get; set; }

    [Column("POL_EXPIRY_DATE")]
    public DateTime POL_EXPIRY_DATE { get; set; }

    [Column("POL_PERIOD")]
    public int POL_PERIOD { get; set; }

    [Column("POL_PASSEGER_1ST_NAME_NTV")]
    public string? POL_PASSEGER_1ST_NAME_NTV { get; set; }

    [Column("POL_PASSEGER_LAST_NAME_NTV")]
    public string? POL_PASSEGER_LAST_NAME_NTV { get; set; }

    [Column("POL_PASSEGER_1ST_NAME_FRN")]
    public string? POL_PASSEGER_1ST_NAME_FRN { get; set; }

    [Column("POL_PASSEGER_LAST_NAME_FRN")]
    public string? POL_PASSEGER_LAST_NAME_FRN { get; set; }

    [Column("POL_PASSEGER_ADDRESS_NTV")]
    public string? POL_PASSEGER_ADDRESS_NTV { get; set; }

    [Column("POL_PASSEGER_ADDRESS_FRN")]
    public string? POL_PASSEGER_ADDRESS_FRN { get; set; }

    [Column("POL_PASSEGER_TELEPHONE")]
    public string? POL_PASSEGER_TELEPHONE { get; set; }

    [Column("POL_PASSEGER_FAX")]
    public string? POL_PASSEGER_FAX { get; set; }

    [Column("POL_PASSEGER_PASSPORT_NUMBER")]
    public string? POL_PASSEGER_PASSPORT_NUMBER { get; set; }

    [Column("POL_PASSEGER_BIRTH_DATE")]
    public DateTime POL_PASSEGER_BIRTH_DATE { get; set; }

    [Column("POL_PASSEGER_AGE")]
    public int POL_PASSEGER_AGE { get; set; }

    [Column("POL_REF_KEY")]
    public required string POL_REF_KEY { get; set; }

    [Column("POL_ISSUE_DATE")]
    public DateTime POL_ISSUE_DATE { get; set; }

    [Column("POL_WINTER_FLAG")]
    public string? POL_WINTER_FLAG { get; set; }

    [Column("POL_STATUS")]
    public string? POL_STATUS { get; set; }
    public override PolicyStatus PolicyStatus
    {
        get
        {
            if (POL_STATUS == "C") return PolicyStatus.Endorsed;
            if (POL_PRINT_FLAG > 0) return PolicyStatus.Generated;
            else return PolicyStatus.Draft;
        }
    }

    [Column("POL_MERGE_FLAG")]
    public int? POL_MERGE_FLAG { get; set; }

    [Column("POL_AREA_CODE")]
    public required string POL_AREA_CODE { get; set; }
    public TypeOfCoverage? TypeOfCoverage { get; set; }

    [Column("POL_TA_BRANCH_CODE")]
    public required string POL_TA_BRANCH_CODE { get; set; }

    [Column("POL_TA_SLT_TYPE")]
    public int POL_TA_SLT_TYPE { get; set; }

    [Column("POL_TA_CODE")]
    public required string POL_TA_CODE { get; set; }
    public Account? PaymentAccount { get; set; }

    [Column("POL_CUR_ID")]
    public required string POL_CUR_ID { get; set; }

    [Column("POL_CUR_ID_LC")]
    public required string POL_CUR_ID_LC { get; set; }

    [Column("POL_CUR_ID_PC")]
    public required string POL_CUR_ID_PC { get; set; }

    [Column("POL_EX_RATE_LC")]
    public double POL_EX_RATE_LC { get; set; }

    [Column("POL_EX_RATE_PC")]
    public double POL_EX_RATE_PC { get; set; }

    [Column("POL_PREMIUM_AMOUNT")]
    public double POL_PREMIUM_AMOUNT { get; set; }

    [Column("POL_PREMIUM_AMOUNT_LC")]
    public double POL_PREMIUM_AMOUNT_LC { get; set; }

    [Column("POL_PREMIUM_AMOUNT_PC")]
    public double POL_PREMIUM_AMOUNT_PC { get; set; }

    [Column("POL_REINS_PREMIUM")]
    public double POL_REINS_PREMIUM { get; set; }

    [Column("POL_REINS_PREMIUM_LC")]
    public double POL_REINS_PREMIUM_LC { get; set; }

    [Column("POL_REINS_PREMIUM_PC")]
    public double POL_REINS_PREMIUM_PC { get; set; }

    [Column("POL_TA_COMM")]
    public double? POL_TA_COMM { get; set; }

    [Column("POL_TA_COMM_LC")]
    public double? POL_TA_COMM_LC { get; set; }

    [Column("POL_TA_COMM_PC")]
    public double? POL_TA_COMM_PC { get; set; }

    [Column("POL_TRUST_COMM")]
    public double POL_TRUST_COMM { get; set; }

    [Column("POL_TRUST_COMM_LC")]
    public double POL_TRUST_COMM_LC { get; set; }

    [Column("POL_TRUST_COMM_PC")]
    public double POL_TRUST_COMM_PC { get; set; }

    [Column("POL_RETENTION_AMOUNT_")]
    public double POL_RETENTION_AMOUNT_ { get; set; }

    [Column("POL_RETENTION_AMOUNT__LC")]
    public double POL_RETENTION_AMOUNT__LC { get; set; }

    [Column("POL_RETENTION_AMOUNT__PC")]
    public double POL_RETENTION_AMOUNT__PC { get; set; }

    [Column("POL_TA_COMM_PCNT")]
    public double? POL_TA_COMM_PCNT { get; set; }

    [Column("POL_POL_NO")]
    public string? POL_POL_NO { get; set; }

    [Column("POL_POL_YEAR")]
    public int? POL_POL_YEAR { get; set; }

    [Column("POL_POL_ENDT_NO")]
    public string? POL_POL_ENDT_NO { get; set; }

    [Column("POL_POL_ENDT_YEAR")]
    public int? POL_POL_ENDT_YEAR { get; set; }

    [Column("POL_POL_CLS_ID")]
    public string? POL_POL_CLS_ID { get; set; }

    [Column("POL_POL_CBR_ID")]
    public string? POL_POL_CBR_ID { get; set; }

    [Column("POL_POL_OFC_ID")]
    public string? POL_POL_OFC_ID { get; set; }

    [Column("POL_POL_DCT_ID")]
    public string? POL_POL_DCT_ID { get; set; }

    [Column("POL_CREATED_BY")]
    public required string POL_CREATED_BY { get; set; }
    public User? CreatedBy { get; set; }

    [Column("POL_CREATION_DATE")]
    public DateTime POL_CREATION_DATE { get; set; }

    [Column("POL_MODIFIED_BY")]
    public string? POL_MODIFIED_BY { get; set; }

    [Column("POL_MODIFICATION_DATE")]
    public DateTime? POL_MODIFICATION_DATE { get; set; }

    [Column("POL_PLT_ID")]
    public required string POL_PLT_ID { get; set; }

    [Column("POL_POL_PLT_ID")]
    public string? POL_POL_PLT_ID { get; set; }

    [Column("POL_VHH_NUM")]
    public string? POL_VHH_NUM { get; set; }

    [Column("POL_VHH_PERIOD_NO")]
    public int? POL_VHH_PERIOD_NO { get; set; }

    [Column("POL_VHH_FSY_YEAR")]
    public int? POL_VHH_FSY_YEAR { get; set; }

    [Column("POL_VHH_VHT_ID")]
    public string? POL_VHH_VHT_ID { get; set; }

    [Column("POL_VHH_DPT_ID")]
    public string? POL_VHH_DPT_ID { get; set; }

    [Column("POL_RI_VHH_NUM")]
    public string? POL_RI_VHH_NUM { get; set; }

    [Column("POL_RI_VHH_PERIOD_NO")]
    public int? POL_RI_VHH_PERIOD_NO { get; set; }

    [Column("POL_RI_VHH_FSY_YEAR")]
    public int? POL_RI_VHH_FSY_YEAR { get; set; }

    [Column("POL_RI_VHH_VHT_ID")]
    public string? POL_RI_VHH_VHT_ID { get; set; }

    [Column("POL_RI_VHH_DPT_ID")]
    public string? POL_RI_VHH_DPT_ID { get; set; }

    [Column("POL_TRN_TYPE")]
    public string? POL_TRN_TYPE { get; set; }

    [Column("POL_FEES1")]
    public double? POL_FEES1 { get; set; }

    [Column("POL_FEES1_LC")]
    public double? POL_FEES1_LC { get; set; }

    [Column("POL_FEES1_PC")]
    public double? POL_FEES1_PC { get; set; }

    [Column("POL_FEES2")]
    public double? POL_FEES2 { get; set; }

    [Column("POL_FEES2_LC")]
    public double? POL_FEES2_LC { get; set; }

    [Column("POL_FEES2_PC")]
    public double? POL_FEES2_PC { get; set; }

    [Column("POL_FEES3")]
    public double? POL_FEES3 { get; set; }

    [Column("POL_FEES3_LC")]
    public double? POL_FEES3_LC { get; set; }

    [Column("POL_FEES3_PC")]
    public double? POL_FEES3_PC { get; set; }

    [Column("POL_PKG_SERIAL")]
    public int? POL_PKG_SERIAL { get; set; }

    [Column("POL_PKG_SUB_SERIAL")]
    public int? POL_PKG_SUB_SERIAL { get; set; }

    [Column("POL_REINS_PREMIUM1")]
    public double? POL_REINS_PREMIUM1 { get; set; }

    [Column("POL_REINS_PREMIUM1_LC")]
    public double? POL_REINS_PREMIUM1_LC { get; set; }

    [Column("POL_REINS_PREMIUM1_PC")]
    public double? POL_REINS_PREMIUM1_PC { get; set; }

    [Column("POL_REINS_PREMIUM2")]
    public double? POL_REINS_PREMIUM2 { get; set; }

    [Column("POL_REINS_PREMIUM2_LC")]
    public double? POL_REINS_PREMIUM2_LC { get; set; }

    [Column("POL_REINS_PREMIUM2_PC")]
    public double? POL_REINS_PREMIUM2_PC { get; set; }

    [Column("POL_POLICY_NO")]
    public string? POL_POLICY_NO { get; set; }

    [Column("POL_DISC_ID")]
    public double? POL_DISC_ID { get; set; }

    [Column("POL_AGENT_CODE")]
    public string? POL_AGENT_CODE { get; set; }

    [Column("POL_CUR2")]
    public string? POL_CUR2 { get; set; }

    [Column("POL_PROD_YEAR")]
    public int? POL_PROD_YEAR { get; set; }

    [Column("POL_PROD_MONTH")]
    public int? POL_PROD_MONTH { get; set; }

    [Column("POL_PAYMENT_TERM")]
    public string? POL_PAYMENT_TERM { get; set; }

    [Column("POL_TRAVEL_CODE")]
    public string? POL_TRAVEL_CODE { get; set; }

    [Column("POL_FEES4")]
    public double? POL_FEES4 { get; set; }

    [Column("POL_FEES4_LC")]
    public double? POL_FEES4_LC { get; set; }

    [Column("POL_FEES4_PC")]
    public double? POL_FEES4_PC { get; set; }

    [Column("POL_ATR_TYPE")]
    public int POL_ATR_TYPE { get; set; }

    [Column("POL_PASSEGER_GENDER")]
    public int? POL_PASSEGER_GENDER { get; set; }

    [Column("POL_PASSEGER_NATIONALITY_ID")]
    public string? POL_PASSEGER_NATIONALITY_ID { get; set; }
    public Country? NationalityCountry { get; set; }

    [Column("POL_SOURCE")]
    public string? POL_SOURCE { get; set; }
    public Country? OriginCountry { get; set; }

    [Column("POL_DESTINATION")]
    public string? POL_DESTINATION { get; set; }
    public Country? DestinationCountry { get; set; }

    [Column("POL_INT_SERIAL")]
    public int POL_INT_SERIAL { get; set; }

    [Column("POL_POL_INT_SERIAL")]
    public int? POL_POL_INT_SERIAL { get; set; }

    [Column("POL_PASSEGER_FATHER_NAME_NTV")]
    public string? POL_PASSEGER_FATHER_NAME_NTV { get; set; }

    [Column("POL_PASSEGER_FATHER_NAME_FRN")]
    public string? POL_PASSEGER_FATHER_NAME_FRN { get; set; }

    [Column("POL_PRINT_FLAG")]
    public int? POL_PRINT_FLAG { get; set; }

    [Column("POL_RI_MAPFREE_VHH_NUM")]
    public string? POL_RI_MAPFREE_VHH_NUM { get; set; }

    [Column("POL_RI_TRUST_VHH_NUM")]
    public string? POL_RI_TRUST_VHH_NUM { get; set; }

    [Column("POL_TRUST_COMM2")]
    public double? POL_TRUST_COMM2 { get; set; }

    [Column("POL_TRUST_COMM2_LC")]
    public double? POL_TRUST_COMM2_LC { get; set; }

    [Column("POL_TRUST_COMM2_PC")]
    public double? POL_TRUST_COMM2_PC { get; set; }

    [Column("POL_ACCEDENT_CODE")]
    public string? POL_ACCEDENT_CODE { get; set; }

    [Column("POL_RETENTION_AMOUNT2_")]
    public double? POL_RETENTION_AMOUNT2_ { get; set; }

    [Column("POL_RETENTION_AMOUNT2__LC")]
    public double? POL_RETENTION_AMOUNT2__LC { get; set; }

    [Column("POL_RETENTION_AMOUNT2__PC")]
    public double? POL_RETENTION_AMOUNT2__PC { get; set; }

    [Column("POL_PREMIUM_AMOUNT2")]
    public double? POL_PREMIUM_AMOUNT2 { get; set; }

    [Column("POL_PREMIUM_AMOUNT2_LC")]
    public double? POL_PREMIUM_AMOUNT2_LC { get; set; }

    [Column("POL_PREMIUM_AMOUNT2_PC")]
    public double? POL_PREMIUM_AMOUNT2_PC { get; set; }

    public TV_POLICY_PACKAGE? groupPolicy { get; set; }
    public TV_POLICIES_ADDITIONAL? additionalInfo { get; set; }

    public override int PlanType => POL_ATR_TYPE;

    public bool IsPaid() => POL_PRINT_FLAG > 0;
    public bool IsCreatedByCustomerPortal => POL_CREATED_BY == "CSTMR_USER";
    public override TypeOfCoverage? TypeOfCoverageGetter => TypeOfCoverage;

    public override string PolicyNumberGetter => POL_POLICY_NO!;
    public override bool CovidFlag => additionalInfo?.CovidFlag ?? false;
    public override bool WinterFlag => POL_WINTER_FLAG == "W";
    public override bool AccidentFlag => POL_ACCEDENT_CODE == "1";

    public override string VoucherNumber => string.Join(" ",
        POL_VHH_VHT_ID ?? "TD",
        POL_OFC_ID,
        POL_YEAR,
        POL_VHH_NUM
    )
    // (POL_VHH_VHT_ID ?? "TD") + POL_OFC_ID + POL_YEAR + POL_VHH_NUM
        ?? "NO VOUCHER";
}

