using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("ST_UW_INSUREDS")]
public class Account
{
    [Column("INS_CODE")]
    public required string Code { get; set; }

    [Column("INS_CGP_ID")]
    public int CompanyGroupId { get; set; }

    [Column("INS_SLT_ID")]
    public int SltId { get; set; }

    [Column("INS_NAME_NTV")]
    public required string NameInEnglish { get; set; }

    [Column("INS_NAME_FRN")]
    public string? NameInArabic { get; set; }

    [Column("INT_CAT_ID")]
    public string? CatId { get; set; }

    [Column("INS_DC_LIMIT")]
    public int? CreditLimit { get; set; }

    [Column("INS_CREATED_BY")]
    public string? CreatedBy { get; set; }

    [Column("INS_CREATION_DATE")]
    public DateTime? CreationDate { get; set; }

    [Column("INS_MODIFIED_BY")]
    public string? ModifiedBy { get; set; }

    [Column("INS_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; }

    [Column("INS_REG_ID_NO")]
    public required string RegistrationId { get; set; }

    [Column("INS_ATTR2")]
    public string? BrokerCommission { get; set; }
}
