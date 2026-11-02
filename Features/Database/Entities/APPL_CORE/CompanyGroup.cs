using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("CR_COMP_GROUPS")]
public class CompanyGroup
{
    [Column("CGP_ID")]
    public int Id { get; set; }

    [Column("CGP_NAME_FRN")]
    public string? NameInArabic { get; set; }

    [Column("CGP_NAME_NTV")]
    public required string NameInEnglish { get; set; }

    [Column("CGP_CREATED_BY")]
    public required string CreatedBy { get; set; }

    [Column("CGP_CREATION_DATE")]
    public DateTime CreationDate { get; set; }

    [Column("CGP_MODIFIED_BY")]
    public string? ModifiedBy { get; set; }

    [Column("CGP_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; }
}
