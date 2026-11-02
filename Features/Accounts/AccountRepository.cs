using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Contracts;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Features.Accounts;

public class AccountRepository(APPL_INS db, CurrentUserService currentUserService)
{
    private readonly APPL_INS _db = db;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<List<Account>> GetAllAccountsAsync(string accountCode = "")
    {
        var sql = @"
        SELECT * FROM (
            SELECT *
            FROM ST_UW_INSUREDS 
            WHERE INS_CGP_ID = 8 AND UPPER(INS_CODE) LIKE UPPER(:accountCode)
            ORDER BY INS_CREATION_DATE DESC
        ) WHERE ROWNUM <= 100
        ";

        var accounts = await _db.Accounts
            .FromSqlRaw(sql, new OracleParameter("accountCode", $"%{accountCode}%"))
            .AsNoTracking()
            .ToListAsync();

        return accounts;
    }

    public async Task SaveAccountAsync(CreateAccountRequest createDto)
    {
        var newAccount = new Account
        {
            CompanyGroupId = _currentUserService.CompanyGroupId,
            CreatedBy = _currentUserService.UserCode,

            Code = createDto.AccountCode,
            NameInEnglish = createDto.AccountNameNative,
            NameInArabic = createDto.AccountNameForiegn,
            SltId = createDto.SltId,
            CatId = createDto.CatId,

            CreditLimit = createDto.CreditLimit,
            BrokerCommission = createDto.BrokerCommission,
            RegistrationId = createDto.AccountCode,
            CreationDate = DateTime.UtcNow
        };

        _db.Accounts.Add(newAccount);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAccountAsync(UpdateAccountRequest updateDto)
    {
        var accounts = await _db.Accounts.Where(a => a.Code == updateDto.AccountCode && a.CompanyGroupId == 8).ToListAsync();
        if (accounts.Count == 0)
            throw new ApplicationException("Account not found.");

        var account = accounts[0];
        if (updateDto.AccountNameNative != null)
            account.NameInEnglish = updateDto.AccountNameNative;

        if (updateDto.AccountNameForiegn != null)
            account.NameInArabic = updateDto.AccountNameForiegn;

        if (updateDto.SltId.HasValue)
            account.SltId = updateDto.SltId.Value;

        if (updateDto.CatId != null)
            account.CatId = updateDto.CatId;

        if (updateDto.CreditLimit.HasValue)
            account.CreditLimit = updateDto.CreditLimit.Value;

        if (updateDto.BrokerCommission != null)
            account.BrokerCommission = updateDto.BrokerCommission;

        account.ModifiedBy = _currentUserService.UserCode;
        account.ModificationDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(string accountCode)
    {
        var accounts = await _db.Accounts.Where(a => a.Code == accountCode && a.CompanyGroupId == 8).ToListAsync();
        if (accounts.Count == 0)
            throw new ApplicationException("Account not found.");

        _db.Accounts.Remove(accounts[0]);
        await _db.SaveChangesAsync();
    }
}