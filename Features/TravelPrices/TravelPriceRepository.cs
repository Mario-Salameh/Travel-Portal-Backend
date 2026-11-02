using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Repositories;

public class TravelPriceRepository(APPL_INS db, CurrentUserService currentUserService)
{
    private readonly APPL_INS _db = db;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<List<TravelPrice>> GetTravelPricesAsync()
    {
        return await _db.TravelPrices
            .Where(tp => tp.CompanyGroupId == _currentUserService.CompanyGroupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SaveTravelPriceAsync(CreateTravelPriceRequest request)
    {
        var travelPrice = new TravelPrice
        {
            TypeOfCoverageId = request.TypeOfCoverage.ToString(),
            Duration = request.Period,
            PlanType = request.PlanType,
            AgeFrom = request.AgeFrom,
            AgeTo = request.AgeTo,
            CompanyGroupId = _currentUserService.CompanyGroupId,
            NetPremiumAmount = request.Premium,
            CommissionAmount = request.Commission,
            RetentionAmount = request.Retention,
            ReInsuranceAmount = request.ReInsurance,
            TrustAmmanAmount = request.Amman,

            CreatedBy = _currentUserService.UserCode,
            CreationDate = DateTime.UtcNow,

            ModifiedBy = null,
            ModificationDate = null,
            Fees1 = request.Fees1,
            Fees2 = request.Fees2,
            Fees3 = request.Fees3,
            Fees4 = request.Fees4,
            AttributeCode = request.Code.ToString(),
            CodeOp = request.CodeOp.ToString(),
        };

        _db.TravelPrices.Add(travelPrice);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateTravelPriceAsync(int planType,
                                             int typeOfCoverage,
                                             int period,
                                             int currentAgeFrom,
                                             int currentAgeTo,
                                             UpdateTravelPriceRequest request)
    {
        var travelPrices = await _db.TravelPrices.Where(tp =>
                tp.CompanyGroupId == _currentUserService.CompanyGroupId &&
                tp.PlanType == planType &&
                tp.TypeOfCoverageId == typeOfCoverage.ToString() &&
                tp.Duration == period &&
                tp.AgeFrom == currentAgeFrom &&
                tp.AgeTo == currentAgeTo).ToListAsync();

        if (travelPrices.Count != 1)
            throw new ApplicationException("Travel price not found.");

        var travelPrice = travelPrices[0];

        bool keyChanged = (request.AgeFrom is not null && request.AgeFrom.Value != travelPrice.AgeFrom)
                       || (request.AgeTo is not null && request.AgeTo.Value != travelPrice.AgeTo);

        if (keyChanged)
        {
            _db.TravelPrices.Remove(travelPrice);
            await _db.SaveChangesAsync();

            var newTravelPrice = new TravelPrice
            {
                CompanyId = 1,
                TypeOfCoverageId = travelPrice.TypeOfCoverageId,
                Duration = travelPrice.Duration,
                PlanType = travelPrice.PlanType,
                AgeFrom = request.AgeFrom ?? travelPrice.AgeFrom,
                AgeTo = request.AgeTo ?? travelPrice.AgeTo,
                CompanyGroupId = travelPrice.CompanyGroupId,
                AttributeCode = travelPrice.AttributeCode,
                NetPremiumAmount = request.Premium ?? travelPrice.NetPremiumAmount,
                RetentionAmount = request.Retention ?? travelPrice.RetentionAmount,
                CommissionAmount = request.Commission ?? travelPrice.CommissionAmount,
                ReInsuranceAmount = request.ReInsurance ?? travelPrice.ReInsuranceAmount,
                TrustAmmanAmount = request.Amman ?? travelPrice.TrustAmmanAmount,
                Fees1 = request.Fees1 ?? travelPrice.Fees1,
                Fees2 = request.Fees2 ?? travelPrice.Fees2,
                Fees3 = request.Fees3 ?? travelPrice.Fees3,
                Fees4 = request.Fees4 ?? travelPrice.Fees4,
                CodeOp = request.CodeOp.ToString(),
                CreatedBy = travelPrice.CreatedBy,
                CreationDate = travelPrice.CreationDate,
                ModifiedBy = _currentUserService.UserCode,
                ModificationDate = DateTime.UtcNow,
            };

            _db.TravelPrices.Add(newTravelPrice);
        }
        else
        {
            if (request.Premium is not null)
                travelPrice.NetPremiumAmount = request.Premium;

            if (request.Retention is not null)
                travelPrice.RetentionAmount = request.Retention;

            if (request.Commission is not null)
                travelPrice.CommissionAmount = request.Commission;

            if (request.ReInsurance is not null)
                travelPrice.ReInsuranceAmount = request.ReInsurance;

            if (request.Amman is not null)
                travelPrice.TrustAmmanAmount = request.Amman;

            if (request.Fees1 is not null)
                travelPrice.Fees1 = request.Fees1;

            if (request.Fees2 is not null)
                travelPrice.Fees2 = request.Fees2;

            if (request.Fees3 is not null)
                travelPrice.Fees3 = request.Fees3;

            if (request.Fees4 is not null)
                travelPrice.Fees4 = request.Fees4;

            travelPrice.CodeOp = request.CodeOp.ToString();

            travelPrice.ModifiedBy = _currentUserService.UserCode;
            travelPrice.ModificationDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteTravelPriceAsync(int planType,
                                             int typeOfCoverage,
                                             int period,
                                             int AgeFrom,
                                             int AgeTo)
    {
        var travelPrices = await _db.TravelPrices.Where(tp =>
                tp.CompanyGroupId == _currentUserService.CompanyGroupId &&
                tp.PlanType == planType &&
                tp.TypeOfCoverageId == typeOfCoverage.ToString() &&
                tp.Duration == period &&
                tp.AgeFrom == AgeFrom &&
                tp.AgeTo == AgeTo).ToListAsync();

        if (travelPrices.Count != 1)
            throw new ApplicationException("Travel price not found.");

        _db.TravelPrices.Remove(travelPrices[0]);
        await _db.SaveChangesAsync();
    }
}