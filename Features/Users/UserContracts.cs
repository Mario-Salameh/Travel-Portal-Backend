using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Application.Contracts;

public class CreateUserRequest
{
    [Required]
    public required string OfficeId { get; set; }

    [Required]
    public required string UserCode { get; set; }

    [Required]
    public required string UserNameEng { get; set; }

    public string? UserNameNative { get; set; }

    [Required]
    public required string UserPassword { get; set; }

    [Required]
    public required int UserType { get; set; }

    [Required]
    public required int ActiveStatus { get; set; }

    public DateOnly? UserInactiveStartDate { get; set; }

    public string? UserWorkingTime { get; set; }

    public int? UserExpiryTime { get; set; }
    public int? UserNumberOfSessions { get; set; }

    public bool FailedLoginLocked { get; set; }

    public bool FirstLoginFlag { get; set; }

    public int UserDefaultLang { get; set; } = 1;

    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }

    public DateOnly? PasswordExpiryDate { get; set; }

    public int PasswordExpiryFlag { get; set; } = 1;
    public required string UserCashAccount { get; set; }
}

public class UpdateUserRequest
{
    public string? OfficeId { get; set; }

    public string? UserNameEng { get; set; }

    public string? UserNameNative { get; set; }

    public string? UserPassword { get; set; }

    public int? UserType { get; set; }

    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }

    public string? UserCashAccount { get; set; }
}

public class DeleteUserRequest
{
    [Required]
    public required string UserCode { get; set; }
}

public record ViewUser([property: JsonIgnore] User User)
{
    public string UserCode { get; set; } = User.Code;
    public int UserType { get; set; } = User.Role;
    public string UserOffice { get; set; } = User.Office?.NameInEnglish ?? "NOT FOUND";
    public bool IsLocked { get; set; } = User.IsLockedDueToFailedAttempts();

    public string? OfficeId { get; set; } = User.CompanyOfficeId;
    public string? NameInEnglish { get; set; } = User.NameInEnglish;
    public string? NameInArabic { get; set; } = User.NameInArabic;
    public string? UserEmail { get; set; } = User.Email;
    public string? UserPhone { get; set; } = User.Phone;
    public string? UserCashAccount { get; set; } = User.CashAccount;
}