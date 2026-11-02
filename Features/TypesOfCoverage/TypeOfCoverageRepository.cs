using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Repositories;

public class TypeOfCoverageRepository(APPL_CORE db)
{
    private readonly APPL_CORE _db = db;

    private readonly Dictionary<int, List<string>> typesOfCoverage = new()
    {
        { 1, new List<string> { "1", "2" } },
        { 3, new List<string> { "1", "2" } },
        { 8, new List<string> { "0" } },
        { 11, new List<string> { "11", "12" } },
    };

    public async Task<List<DomainValue>> GetTypesOfCoverageAsync(int planType)
    {
        var result = await _db.CR_DOMAIN_VALUES
                .Where(e => e.DomainId == 104 && e.CompanyGroupId == 8 && typesOfCoverage[planType].Contains(e.Id))
                .AsNoTracking()
                .ToListAsync();

        return result;
    }

    public static int GetTypeOfCoverageCode(string destinationCountryCode, int planType)
    {
        int typeOfCoverage;

        // Extracting the right type of coverage
        int fixedPlanType = (planType == 3) ? 1 : planType;

        if (fixedPlanType == 8)
            typeOfCoverage = 0;
        else if (destinationCountryCode == "ALL")
            if (fixedPlanType == 11)
                typeOfCoverage = 12;
            else if (fixedPlanType == 1)
                typeOfCoverage = 2;
            else
                // Will though an error in this state by default!
                typeOfCoverage = fixedPlanType;
        else
            typeOfCoverage = fixedPlanType;

        return typeOfCoverage;
    }
}