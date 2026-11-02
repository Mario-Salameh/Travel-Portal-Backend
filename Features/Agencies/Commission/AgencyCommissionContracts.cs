using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.Contracts;

public class CreateAgencyCommissionRequest
{
    [Required]
    public required string BrokerCode { get; set; }

    [Required]
    public required double CommissionPercent { get; set; }
}

public class UpdateAgencyCommissionRequest
{
    [Required]
    public required string BrokerCode { get; set; }

    [Required]
    public required double CommissionPercent { get; set; }
}

public record ViewAgencyCommission([property: JsonIgnore] BrokerCommission AgencyCommission)
{
    public string BrokerCode { get; set; } = AgencyCommission.BrokerCode;
    public string IrsId { get; set; } = AgencyCommission.STC_IRS_ID;
    public double? CommissionPercent { get; set; } = AgencyCommission.CommissionPercentage;
}