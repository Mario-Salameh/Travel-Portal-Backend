using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;
namespace TrustSystem.Backend.DTOs
{
    public class CreateAgencyRequest
    {
        [Required]
        public required string agencyNameNative { get; set; }
        public string? agencyNameForiegn { get; set; } = null;
        public string? agencyAccountCode { get; set; } = null;
    }

    public class UpdateAgencyRequest
    {
        public string? agencyNameNative { get; set; }
        public string? agencyNameForiegn { get; set; } = null;
        public string? agencyAccountCode { get; set; } = null;
    }

    public record ViewAgency([property: JsonIgnore] DomainValue agencyGroup)
    {
        public int agencyCode => int.Parse(agencyGroup.Id);
        public string agencyNameNative => agencyGroup.NameInEnglish;
        public string? agencyNameForiegn => agencyGroup.NameInArabic;
        public string? agencyAccountCode => agencyGroup.BrokerCode;
    }
}
