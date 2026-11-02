using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("CR_OFFICES")]
public class CompanyOffice
{
    [Column("OFC_ID")]
    public required string Id { get; set; }

    [Column("OFC_CGP_ID")]
    public int CompanyGroupId { get; set; }

    [Column("OFC_CBR_ID")]
    public required string CompanyBranchId { get; set; }

    [Column("OFC_CMP_ID")]
    public int CompanyId { get; set; } = 1;


    [Column("OFC_NAME_NTV")]
    public required string NameInEnglish { get; set; }

    [Column("OFC_NAME_FRN")]
    public string? NameInArabic { get; set; }


    [Column("OFC_CREATED_BY")]
    public required string CreatedBy { get; set; }

    [Column("OFC_CREATION_DATE")]
    public DateTime CreationDate { get; set; }

    [Column("OFC_MODIFIED_BY")]
    public string? ModifiedBy { get; set; }

    [Column("OFC_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; }

    [Column("OFC_BROKER2_SLT_ID")]
    public int? BrokerSltId { get; set; }

    [Column("OFC_BROKER_CODE")]
    public string? BrokerCode { get; set; }

    [Column("OFC_DISC")]
    public int? SpecialDiscount { get; set; }

    public ICollection<User>? Users { get; set; }
}
