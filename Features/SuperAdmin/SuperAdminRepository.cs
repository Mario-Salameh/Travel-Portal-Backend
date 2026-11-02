using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Infrastructure.Repositories;

public class SuperAdminRepository(APPL_CORE db)
{
    public async Task<List<CompanyInfo>> GetCompaniesAsync()
    {
        var companies = await db.CR_DOMAIN_VALUES.Where(dv =>
            dv.DomainId == 00
        )
        .Select(dv => new CompanyInfo(dv))
        .ToListAsync();

        return companies;
    }

    public async Task<(decimal, decimal)> GetCompanyNewAndExistingCustomerFeesPercentAsync(int companyGroupId)
    {
        var domainValues = await db.CR_DOMAIN_VALUES.Where(dv =>
            dv.CompanyGroupId == companyGroupId && dv.DomainId == 00
        )
        .Select(dv => new CompanyInfo(dv))
        .ToListAsync();
        var domainValue = domainValues.Single();

        return (domainValue.NewCustomerManagementFeesPercent / 100, domainValue.ExistingCustomerManagementFeesPercent / 100);
    }

    public async Task<bool> IsCustomerPortalEnabledAsync(int companyGroupId)
    {
        var domainValues = await db.CR_DOMAIN_VALUES.Where(dv =>
            dv.CompanyGroupId == companyGroupId && dv.DomainId == 00
        )
        .Select(dv => new CompanyInfo(dv))
        .ToListAsync();
        var domainValue = domainValues.Single();

        return domainValue.IsEnabled;
    }

    public async Task UpdateCompanyAsync(UpdateCompanyRequest company)
    {
        var domainValues = await db.CR_DOMAIN_VALUES.Where(dv =>
            dv.CompanyGroupId == company.CompanyGroupId && dv.DomainId == 00
        ).ToListAsync();
        var domainValue = domainValues.Single();

        // Get original name
        var temp = new CompanyInfo(domainValue);

        domainValue.NameInEnglish = JsonSerializer.Serialize(new
        {
            name = temp.Name,
            isEnabled = company.IsEnabled,
            existingCustomerManagementFeesPercent = company.ExistingCustomerManagementFeesPercent,
            newCustomerManagementFeesPercent = company.NewCustomerManagementFeesPercent
        });
        await db.SaveChangesAsync();
    }

}

public record UpdateCompanyRequest(
    int CompanyGroupId,
    bool IsEnabled,
    decimal ExistingCustomerManagementFeesPercent,
    decimal NewCustomerManagementFeesPercent
);

public record CompanyInfo([property: JsonIgnore] DomainValue DomainValue)
{
    public int CompanyGroupId => DomainValue.CompanyGroupId;

    private sealed record CompanyJsonDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("isEnabled")] bool IsEnabled,
        [property: JsonPropertyName("existingCustomerManagementFeesPercent")] decimal ExistingCustomerManagementFeesPercent,
        [property: JsonPropertyName("newCustomerManagementFeesPercent")] decimal NewCustomerManagementFeesPercent
    );

    private CompanyJsonDto Parsed => JsonSerializer.Deserialize<CompanyJsonDto>(DomainValue.NameInEnglish)!;

    public string Name => Parsed.Name;
    public bool IsEnabled => Parsed.IsEnabled;
    public decimal ExistingCustomerManagementFeesPercent => Parsed.ExistingCustomerManagementFeesPercent;
    public decimal NewCustomerManagementFeesPercent => Parsed.NewCustomerManagementFeesPercent;
}