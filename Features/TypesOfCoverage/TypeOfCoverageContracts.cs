using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.DTOs
{
    public record ViewTypeOfCoverage([property: JsonIgnore] DomainValue TypeOfCoverage)
    {
        public string TypeOfCoverageId { get; set; } = TypeOfCoverage.Id;
        public string TypeOfCoverageNameNative { get; set; } = TypeOfCoverage.NameInEnglish;
        public string? TypeOfCoverageNameForeign { get; set; } = TypeOfCoverage.NameInArabic;
    }
}
