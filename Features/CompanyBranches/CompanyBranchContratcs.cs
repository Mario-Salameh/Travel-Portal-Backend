using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;
namespace TrustSystem.Backend.DTOs
{
    public record CompanyBranchViewDTO([property: JsonIgnore] CompanyBranch companyBranch)
    {
        public string Id { get; set; } = companyBranch.Id;
        public string NameInEnglish { get; set; } = companyBranch.NameInEnglish;
    }
}
