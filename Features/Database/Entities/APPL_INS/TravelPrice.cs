using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("ALG_TRAVEL_RATE")]
public class TravelPrice
{
    [Column("ATR_ZONE_CODE")]
    public required string TypeOfCoverageId { get; set; }

    [Column("ATR_PERIOD")]
    public int Duration { get; set; }

    [Column("ATR_TYPE")]
    public int PlanType { get; set; }

    [Column("ATR_AGE_FROM")]
    public int AgeFrom { get; set; }

    [Column("ATR_AGE_TO")]
    public int AgeTo { get; set; }

    [Column("ATR_CMP_ID")]
    public int CompanyId { get; set; }

    [Column("ATR_CGP_ID")]
    public int CompanyGroupId { get; set; }

    [Column("ATR_CODE")]
    public string? AttributeCode { get; set; }

    [Column("ATR_AFRICA_AMOUNT")]
    public double? ReInsuranceAmount { get; set; }

    [Column("ATR_NET_PREMIUM")]
    public double? NetPremiumAmount { get; set; }

    [Column("ATR_RET")]
    public double? RetentionAmount { get; set; }

    [Column("ATR_COMM")]
    public double? CommissionAmount { get; set; }



    [Column("ATR_TRUST_AMMAN_AMOUNT")]
    public double? TrustAmmanAmount { get; set; }

    [Column("ATR_FEES1")]
    public double? Fees1 { get; set; }

    [Column("ATR_FEES2")]
    public double? Fees2 { get; set; }

    [Column("ATR_FEES3")]
    public double? Fees3 { get; set; }

    [Column("ATR_FEES4")]
    public double? Fees4 { get; set; }

    [Column("ATR_CODE2")]
    public string? CodeOp { get; set; }


    [Column("ATR_CREATED_BY")]
    public required string CreatedBy { get; set; }

    [Column("ATR_CREATION_DATE")]
    public DateTime CreationDate { get; set; }

    [Column("ATR_MODIFIED_BY")]
    public string? ModifiedBy { get; set; }

    [Column("ATR_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; }
}
