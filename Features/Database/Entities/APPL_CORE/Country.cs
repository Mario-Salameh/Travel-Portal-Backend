using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("CR_COUNTRIES", Schema = "APPL_CORE")]
public class Country
{
    [Column("CNT_ID")]
    public string? Id { get; set; }

    [Column("CNT_NAME_NTV")]
    public string? NameInEnglish { get; set; }

    [Column("CNT_NAME_FRN")]
    public string? NameInArabic { get; set; }

    [Column("CNT_NATIONALITY_NTV")]
    public string? NationalityInEnglish { get; set; }

    [Column("CNT_NATIONALITY_FRN")]
    public string? NationalityInArabic { get; set; }
}
