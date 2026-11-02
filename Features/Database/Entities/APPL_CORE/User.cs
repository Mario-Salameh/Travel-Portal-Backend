using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("TV_USERS", Schema = "APPL_CORE")]
public class User
{
    [Column("USR_CODE")]
    public required string Code { get; set; }

    [Column("USR_NAME_NTV")]
    public required string NameInEnglish { get; set; }

    [Column("USR_NAME_FRN")]
    public string? NameInArabic { get; set; }

    [Column("USR_PASSWORD")]
    public required string Password { get; set; }

    [Column("USR_TYPE")]
    public int Role { get; set; }

    [Column("USR_UGP_ID")]
    public int GroupId { get; set; } = 8;

    [Column("USR_ACTIVE_FLAG")]
    public int? ActiveFlag { get; set; } = 1;

    [Column("USR_INACTIVE_START_DATE")]
    public DateTime? InactiveStartDate { get; set; }

    [Column("USR_WORKING_TIME")]
    public int? WorkingTime { get; set; }

    [Column("USR_EXPIRY_TIME")]
    public int? ExpiryTime { get; set; }

    [Column("USR_NO_OF_SESSION")]
    public int? NumberOfSessions { get; set; }

    [Column("USR_FAILD_LOGIN_ATTEMPT")]
    public int? FailedLoginAttempt { get; set; }

    [Column("USR_FIRST_LOGIN_FLAG")]
    public int? FirstLoginFlag { get; set; }

    [Column("USR_OFC_ID")]
    public string? USR_OFC_ID { get; set; }
    [NotMapped]
    public string CompanyOfficeId
    {
        get => USR_OFC_ID ?? throw new ApplicationException("User doesn't have an office ID. Contact System Administrator.");
        set => USR_OFC_ID = value;
    }

    [Column("USR_CBR_ID")]
    public string? USR_CBR_ID { get; set; }
    [NotMapped]
    public string CompanyBranchId
    {
        get => USR_CBR_ID ?? CompanyOfficeId[..1];
        set => USR_CBR_ID = value;
    }

    [Column("USR_CMP_ID")]
    public int CompanyId { get; set; }

    [Column("USR_CGP_ID")]
    public int CompanyGroupId { get; set; }

    [Column("USR_DEFAULT_LANG")]
    public int? DefaultLanguage { get; set; }

    [Column("USR_EMAIL_ID")]
    public string? Email { get; set; }

    [Column("USR_PHONE_ID")]
    public string? Phone { get; set; }

    [Column("USR_PASSWORD_EXPIRY_DATE")]
    public DateTime? PasswordExpiryDate { get; set; }

    [Column("USR_PASSWORD_EXPIRY_FLAG")]
    public int? PasswordExpiryFlag { get; set; }

    [Column("USR_CASH_ACCOUNT")]
    public string? USR_CASH_ACCOUNT { get; set; }
    [NotMapped]
    public string CashAccount
    {
        get => USR_CASH_ACCOUNT ?? "N/A";
        set => USR_CASH_ACCOUNT = value;
    }

    [Column("USR_CREATED_BY")]
    public string? CreatedBy { get; set; }

    [Column("USR_CREATION_DATE")]
    public DateTime? CreationDate { get; set; }

    [Column("USR_MODIFIED_BY")]
    public string? ModifiedBy { get; set; }

    [Column("USR_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; }

    public CompanyOffice? Office { get; set; }

    public bool IsLockedDueToFailedAttempts() => FailedLoginAttempt > 6;
    public bool IsCustomer() => Role == 9;
}
