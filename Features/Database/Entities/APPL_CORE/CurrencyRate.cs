using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("CR_CURRENCY_RATES")]
public class CurrencyRate
{
    [Column("CRR_ID")]
    public int Id { get; set; }

    [Column("CRR_CGP_ID")]
    public required int CompanyGroupId { get; set; }

    [Column("CRR_EX_RATE")]
    public required double ExchangeRate { get; set; }

    [Column("CRR_START_DATE")]
    public required DateTime StartDate { get; set; }


    [Column("CRR_CUR_ID_PAR")]
    public string BaseCurrencyCode { get; set; } = "31";

    [Column("CRR_CUR_ID")]
    public string CurrencyCode { get; set; } = "00";


    [Column("CRR_CREATED_BY")]
    public required string CreatedBy { get; set; }

    [Column("CRR_CREATION_DATE")]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [Column("CRR_MODIFIED_BY")]
    public string? ModifiedBy { get; set; } = null;

    [Column("CRR_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; } = null;
}
