using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;
namespace TrustSystem.Backend.DTOs
{
    public class CreateOfficeRequest
    {
        [Required]
        public required string OfficeId { get; set; }

        [Required]
        public required string NameInEnglish { get; set; }

        public string? NameInArabic { get; set; }

        public int BrokerSltId { get; set; } = 4;

        public string? BrokerCode { get; set; }

        public int SpecialDiscount { get; set; }
    }

    public class UpdateOfficeRequest
    {
        public string? NameInEnglish { get; set; }
        public string? NameInArabic { get; set; }
        public int? BrokerSltId { get; set; }
        public string? BrokerCode { get; set; }
        public int? SpecialDiscount { get; set; }
    }

    public record ViewOffice([property: JsonIgnore] CompanyOffice office)
    {
        public string OfficeId { get; set; } = office.Id;
        public string NameInEnglish { get; set; } = office.NameInEnglish;
        public string? NameInArabic { get; set; } = office.NameInArabic;
        public int? BrokerSltId { get; set; } = office.BrokerSltId;
        public string? BrokerCode { get; set; } = office.BrokerCode;
        public int? SpecialDiscount { get; set; } = office.SpecialDiscount;
    }
}
