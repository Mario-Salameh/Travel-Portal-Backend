using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Contracts;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Repositories;

public class AgencyCommissionRepository(APPL_INS db, APPL_CORE _dbCore, CurrentUserService currentUserService)
{
    private readonly APPL_INS _db = db;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<double> GetBrokerCommission(string officeId)
    {
        try
        {
            var brokerCode = await GetBrokerCodeFromOfficeId(officeId);
            return await GetBrokerPercentage(brokerCode);
        }
        catch (ApplicationException) { return 0; }
    }

    private async Task<string> GetBrokerCodeFromOfficeId(string officeId)
    {
        var offices = await _dbCore.CR_OFFICES
                .Where(o => o.CompanyGroupId == 8 && o.Id == officeId)
                .Select(o => new ViewOffice(o))
                .AsNoTracking()
                .ToListAsync();
        var office = offices.FirstOrDefault()
            ?? throw new ApplicationException($"There is no office for that office ID: {officeId}");

        var agencies = await _dbCore.CR_DOMAIN_VALUES
                .Where(a => a.CompanyGroupId == 8 && a.DomainId == 510 && a.BrokerCode == office.BrokerCode)
                .Select(a => new ViewAgency(a))
                .AsNoTracking()
                .ToListAsync();

        var agency = agencies.FirstOrDefault()
            ?? throw new ApplicationException($"There is no broker for that office: {officeId}");

        return agency.agencyAccountCode
            ?? throw new ApplicationException("There is no broker code for that agency");
    }

    private async Task<double> GetBrokerPercentage(string brokerCode)
    {
        var percentages = await _db.BrokerCommission
            .Where(e => e.CompanyGroupId == 8 && e.BrokerCode == brokerCode)
            .ToListAsync();

        return percentages.FirstOrDefault()?.CommissionPercentage ?? 0;
    }

    public async Task<List<BrokerCommission>> GetAllAgencyCommissionsAsync()
    {
        return await _db.BrokerCommission
            .Where(e => e.CompanyGroupId == 8)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SaveAgencyCommissionAsync(CreateAgencyCommissionRequest data)
    {
        var agencyCommission = new BrokerCommission
        {
            CompanyGroupId = 8,
            BrokerCode = data.BrokerCode,
            CommissionPercentage = data.CommissionPercent,

            CreatedBy = _currentUserService.UserCode,
            CreationDate = DateTime.UtcNow
        };

        await _db.BrokerCommission.AddAsync(agencyCommission);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAgencyCommissionAsync(UpdateAgencyCommissionRequest data)
    {
        var agencyCommissions = await _db.BrokerCommission
            .Where(e => e.BrokerCode == data.BrokerCode && e.CompanyGroupId == 8)
            .ToListAsync();

        if (agencyCommissions.Count == 0)
            throw new ApplicationException("Agency Commission not found.");

        var agencyCommission = agencyCommissions[0];
        agencyCommission.BrokerCode = data.BrokerCode;
        agencyCommission.CommissionPercentage = data.CommissionPercent;

        agencyCommission.ModifiedBy = _currentUserService.UserCode;
        agencyCommission.ModificationDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAgencyCommissionAsync(string brokerCode)
    {
        var agencyCommissions = await _db.BrokerCommission
            .Where(e => e.BrokerCode == brokerCode && e.CompanyGroupId == 8)
            .ToListAsync();

        if (agencyCommissions.Count == 0)
            throw new ApplicationException("Agency Commission not found.");

        _db.BrokerCommission.Remove(agencyCommissions[0]);
        await _db.SaveChangesAsync();
    }
}