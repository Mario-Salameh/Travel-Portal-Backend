using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.DTOs
{
    public record PeriodResponseDTO([property: JsonIgnore] DomainValue period)
    {
        public string periodId { get; set; } = period.Id;
        public string periodNameNative { get; set; } = period.NameInEnglish;
        public string? periodNameForeign { get; set; } = period.NameInArabic;
    }
}
