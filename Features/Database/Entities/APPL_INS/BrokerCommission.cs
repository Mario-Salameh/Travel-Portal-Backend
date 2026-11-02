using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("ST_BROKER_STD_COMM")]
public class BrokerCommission
{
    [Column("STC_CGP_ID")]
    public required int CompanyGroupId { get; set; }

    [Column("STC_COMM_PCNT")]
    public required double? CommissionPercentage { get; set; }

    [Column("STC_BROKER_CODE")]
    public required string BrokerCode { get; set; }

    [Column("STC_CREATED_BY")]
    public string? CreatedBy { get; set; }

    [Column("STC_CREATION_DATE")]
    public DateTime? CreationDate { get; set; }

    [Column("STC_MODIFIED_BY")]
    public string? ModifiedBy { get; set; }

    [Column("STC_MODIFICATION_DATE")]
    public DateTime? ModificationDate { get; set; }



    [Column("STC_CMP_ID")]
    public int STC_CMP_ID { get; set; } = 1;

    [Column("STC_CLS_ID")]
    public string STC_CLS_ID { get; set; } = "T";

    [Column("STC_BROKER_SLT_ID")]
    public string STC_BROKER_SLT_ID { get; set; } = "4";

    [Column("STC_PLT_ID")]
    public string STC_PLT_ID { get; set; } = "T01";

    [Column("STC_BUT_ID")]
    public string STC_BUT_ID { get; set; } = "1";

    [Column("STC_IRS_TYPE")]
    public string STC_IRS_TYPE { get; set; } = "4";

    [Column("STC_IRS_ID")]
    public string STC_IRS_ID { get; set; } = "1";

    [Column("STC_PREM_IRS_ID")]
    public string STC_PREM_IRS_ID { get; set; } = "0";

    [Column("STC_PREM_IRS_TYPE")]
    public string STC_PREM_IRS_TYPE { get; set; } = "0";

    [Column("SRC_FROM_SI")]
    public double? SRC_FROM_SI { get; set; } = 0;

    [Column("SRC_TO_SI")]
    public double? SRC_TO_SI { get; set; } = 9999;
}
