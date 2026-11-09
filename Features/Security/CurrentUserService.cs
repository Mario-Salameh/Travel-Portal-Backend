using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TrustSystem.Backend.Application.Enums;
using TrustSystem.Backend.Data;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Services;

public class CurrentUserService(IHttpContextAccessor http, APPL_CORE db)
{
    private readonly IHttpContextAccessor _http = http;
    private readonly APPL_CORE _db = db;

    private ClaimsPrincipal User =>
        _http.HttpContext!.User;

    public bool IsAuthenticated =>
        _http.HttpContext?.User.Identity?.IsAuthenticated == true;

    public int Role =>
        int.Parse(User.FindFirstValue(ClaimTypes.Role)
        ?? throw new ApplicationException("Unauthorized Access"));

    public string UserCode =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new ApplicationException("Unauthorized Access");

    public int CompanyGroupId =>
        int.Parse(User.FindFirstValue("CompanyGroupId")
        ?? throw new ApplicationException("Unauthorized Access"));

    public UserType UserType => (UserType)Role;

    // Will be retieved only if there is addititonal info requested from the backend service other than the ones
    // that are included in the JWT token
    private User? _userInstance;
    public User UserInstance()
        => _userInstance

        // Throw an error if there is no retrieved user
        ?? throw new ApplicationException("User wasn't retrieved. Call System administrator.");

    public async Task RetrieveUser()
    {
        // Check if the user is already retrieved
        if (_userInstance is not null)
            return;

        // Trying to get the information of a logged-in user through bearer token
        try
        {
            var userCode = UserCode;
            var companyGroupId = CompanyGroupId;

            // Trying to retrieve user through Bearer token
            var users = await _db.Users
            .Where(e => e.Code == userCode && e.CompanyGroupId == companyGroupId)
            .Include(e => e.Office)
            .AsNoTracking()
            .ToListAsync();

            _userInstance = users.Single();
        }

        // There is no logged-in user, Behave as a Customer
        catch (Exception)
        {
            var customerAccounts = await _db.Users
            .Where(user => user.Role == 9 && user.CompanyGroupId == 8)
            .Include(e => e.Office)
            .AsNoTracking()
            .ToListAsync();

            // assigning customer account
            _userInstance = customerAccounts.FirstOrDefault()

            // if there is no customer account
            ?? throw new ApplicationException("Customer account not found. Contact System Administrator.");
        }
    }
}