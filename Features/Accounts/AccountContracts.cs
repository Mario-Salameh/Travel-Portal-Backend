using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Contracts;

public class CreateAccountRequest
{
    [Required]
    public required string AccountCode { get; set; }
    [Required]
    public required string AccountNameNative { get; set; }
    public string? AccountNameForiegn { get; set; } = null;

    [Required]
    public required int SltId { get; set; }
    [Required]
    public required string CatId { get; set; }

    public int? CreditLimit { get; set; }
    public string? BrokerCommission { get; set; }
}

public class UpdateAccountRequest
{
    [Required]
    public required string AccountCode { get; set; }

    public string? AccountNameNative { get; set; }
    public string? AccountNameForiegn { get; set; } = null;
    public int? SltId { get; set; }
    public string? CatId { get; set; }
    public int? CreditLimit { get; set; }
    public string? BrokerCommission { get; set; }
}

public record ViewAccount([property: JsonIgnore] Account Account)
{
    public string AccountCode { get; set; } = Account.Code;
    public string? AccountNameNative { get; set; } = Account.NameInEnglish;
    public string? AccountNameForiegn { get; set; } = Account.NameInArabic;
    public int? SltId { get; set; } = Account.SltId;
    public string? CatId { get; set; } = Account.CatId;
    public int? CreditLimit { get; set; } = Account.CreditLimit;
    public string? BrokerCommission { get; set; } = Account.BrokerCommission;
}
