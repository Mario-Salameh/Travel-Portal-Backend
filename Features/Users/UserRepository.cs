using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Application.Contracts;
using TrustSystem.Backend.Application.Enums;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Models;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Infrastructure.Repositories;

public class UserRepository(APPL_CORE db, CurrentUserService currentUserService)
{
    private readonly APPL_CORE _db = db;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<User?> GetUserByCodeAsync(string userCode)
    {
        var users = await _db.Users
            .Where(e => e.Code == userCode && e.CompanyGroupId == 8)
            .Include(e => e.Office).AsNoTracking().ToListAsync();

        return users.FirstOrDefault();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _db.Users
            .Where(e => e.CompanyGroupId == 8)
            .Include(e => e.Office)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddUserAsync(CreateUserRequest request)
    {
        await _currentUserService.RetrieveUser();
        User UserInstance = _currentUserService.UserInstance();

        var user = new User
        {
            Code = request.UserCode,
            NameInEnglish = request.UserNameEng,
            NameInArabic = request.UserNameNative,
            Password = request.UserPassword,
            Role = request.UserType,
            GroupId = 8,
            ActiveFlag = request.ActiveStatus,

            InactiveStartDate = request.UserInactiveStartDate?.ToDateTime(TimeOnly.MinValue),
            ExpiryTime = request.UserExpiryTime,
            NumberOfSessions = request.UserNumberOfSessions,
            FailedLoginAttempt = 0,
            FirstLoginFlag = 1,
            CompanyOfficeId = request.OfficeId,

            CompanyId = UserInstance.CompanyId,
            CompanyGroupId = UserInstance.CompanyGroupId,

            DefaultLanguage = request.UserDefaultLang,
            Email = request.UserEmail,
            Phone = request.UserPhone,

            PasswordExpiryDate = request.PasswordExpiryDate?.ToDateTime(TimeOnly.MinValue),
            PasswordExpiryFlag = request.PasswordExpiryFlag,

            CashAccount = request.UserCashAccount,

            CreatedBy = _currentUserService.UserCode,
            CreationDate = DateTime.UtcNow,
        };

        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(string userCode, UpdateUserRequest request)
    {
        var users = await _db.Users.Where(user => user.Code == userCode &&
                            user.CompanyGroupId == _currentUserService.CompanyGroupId).ToListAsync();

        if (users.Count != 1)
            throw new ApplicationException("User not found.");

        var user = users[0];

        user.NameInEnglish = request.UserNameEng ?? user.NameInEnglish;
        user.NameInArabic = request.UserNameNative ?? user.NameInArabic;
        user.Password = request.UserPassword ?? user.Password;
        user.Role = request.UserType ?? user.Role;
        user.Email = request.UserEmail ?? user.Email;
        user.Phone = request.UserPhone ?? user.Phone;
        user.CashAccount = request.UserCashAccount ?? user.CashAccount;
        user.CompanyOfficeId = request.OfficeId ?? user.CompanyOfficeId;

        user.ModifiedBy = _currentUserService.UserCode;
        user.ModificationDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string userCode)
    {
        var checkPhase = await _db.Users
            .Where(u => u.Code == userCode && u.CompanyGroupId == 8).ToListAsync();
        var specifiedUser = checkPhase.Single();

        if (specifiedUser.IsCustomer())
            throw new ApplicationException("Cannot delete a customer portal specific user.");

        var deletedCount = await _db.Users
            .Where(u => u.Code == userCode && u.CompanyGroupId == 8)
            .ExecuteDeleteAsync();

        if (deletedCount == 0)
            throw new ApplicationException($"User with code {userCode} not found.");
    }

    // Extension Methods for User
    public async Task<(UserState, User?)> LoginToUser(string UserCode, string Password)
    {
        var user = await GetUserByCodeAsync(UserCode);
        if (user is null)
            return (UserState.NOTFOUND, null);

        if (DateTime.UtcNow > user.InactiveStartDate || user.ActiveFlag == 0)
            return (UserState.EXPIRED, null);

        if (user.IsLockedDueToFailedAttempts())
            return (UserState.LOCKED, null);

        if (DateTime.UtcNow > user.PasswordExpiryDate && user.PasswordExpiryFlag == 1)
            return (UserState.REQUIREPASSWORDCHANGE, user);

        if (Password != user.Password)
        {
            user.FailedLoginAttempt++;
            await _db.SaveChangesAsync();

            return (UserState.INVALID, null);
        }

        user.FailedLoginAttempt = 0;
        await _db.SaveChangesAsync();
        return (UserState.VALID, user);
    }

    public async Task LockUnlockUserAsync(string userCode)
    {
        var users = await _db.Users
            .Where(u => u.Code == userCode && u.CompanyGroupId == 8).ToListAsync();

        if (users.Count != 1)
            throw new ApplicationException("User not found.");

        var user = users[0];

        if (user.IsLockedDueToFailedAttempts())
            user.FailedLoginAttempt = 0;
        else
            user.FailedLoginAttempt = 7;

        await _db.SaveChangesAsync();
    }
}