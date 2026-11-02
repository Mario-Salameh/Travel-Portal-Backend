using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("CR_COMPANY_BRANCHES")]
public class CompanyBranch
{
    [Column("CBR_ID")]
    public required string Id { get; set; }

    [Column("CBR_CGP_ID")]
    public int CompanyGroupId { get; set; }

    [Column("CBR_NAME_NTV")]
    public required string NameInEnglish { get; set; }

    [Column("CBR_NAME_FRN")]
    public string? NameInArabic { get; set; }
}
