using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;
namespace TrustSystem.Backend.DTOs
{
    public record CompanyGroupViewDTO([property: JsonIgnore] CompanyGroup companyGroup)
    {
        public int cgpId { get; set; } = companyGroup.Id;
        public string companyGroupName { get; set; } = companyGroup.NameInEnglish;
    }
}
