using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("CR_DOMAIN_VALUES", Schema = "APPL_CORE")]
public class DomainValue
{
    [Column("DMV_ID")]
    public required string Id { get; set; }

    [Column("DMV_DOM_ID")]
    public int DomainId { get; set; }

    [Column("DMV_SYS_ID")]
    public int SystemId { get; set; }

    [Column("DMV_CGP_ID")]
    public int CompanyGroupId { get; set; }

    [Column("DMV_DESC_NTV")]
    public required string NameInEnglish { get; set; }

    [Column("DMV_DESC_FRN")]
    public string? NameInArabic { get; set; }

    [Column("BROKER_CODE")]
    public string? BrokerCode { get; set; }

    [Column("DMV_CREATED_BY")]
    public required string CreatedBy { get; set; }

    [Column("DMV_CREATION_DATE")]
    public DateTime CreationDate { get; set; }

    [Column("DMV_MODIFIED_BY")]
    public string? ModifiedBy { get; set; }

    [Column("DMV_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; }

    // [Column("DMV_SYS_ABBRV_NTV")]
    // public string? DMV_SYS_ABBRV_NTV { get; set; }

    // [Column("DMV_SYS_ABBRV_FRN")]
    // public string? DMV_SYS_ABBRV_FRN { get; set; }

    // [Column("DMV_USR_ABBRV_NTV")]
    // public string? DMV_USR_ABBRV_NTV { get; set; }

    // [Column("DMV_USR_ABBRV_FRN")]
    // public string? DMV_USR_ABBRV_FRN { get; set; }

    // [Column("DMV_ENDT_TRN_TYPE")]
    // public string? DMV_ENDT_TRN_TYPE { get; set; }

    // [Column("DMV_POLICY_TYPE")]
    // public string? DMV_POLICY_TYPE { get; set; }

    // [Column("DMV_CLASS_TYPE")]
    // public string? DMV_CLASS_TYPE { get; set; }

    // [Column("BROKER_CREDIT_LIMIT")]
    // public double? BROKER_CREDIT_LIMIT { get; set; }

    // [Column("BROKER_BALANCE")]
    // public double? BROKER_BALANCE { get; set; }
}

public class TypeOfCoverage : DomainValue { }
public class PolicyType : DomainValue { }