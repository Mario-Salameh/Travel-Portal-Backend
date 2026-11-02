using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;
namespace TrustSystem.Backend.DTOs
{
    public class CreateTravelPriceRequest
    {
        [Required]
        public required int PlanType { get; set; }

        [Required]
        public required int TypeOfCoverage { get; set; }

        [Required]
        public required int Period { get; set; }

        [Required]
        public required int AgeFrom { get; set; }

        [Required]
        public required int AgeTo { get; set; }

        [Required]
        public required double ReInsurance { get; set; }

        [Required]
        public required double Premium { get; set; }

        [Required]
        public required double Retention { get; set; }

        [Required]
        public required double Commission { get; set; }

        [Required]
        public required double Amman { get; set; }

        [Required]
        public required double Fees1 { get; set; }

        [Required]
        public required double Fees2 { get; set; }

        [Required]
        public required double Fees3 { get; set; }

        [Required]
        public required double Fees4 { get; set; }

        [Required]
        public required int Code { get; set; }

        [Required]
        public required int CodeOp { get; set; }
    }

    public class UpdateTravelPriceRequest
    {
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
        public double? ReInsurance { get; set; }
        public double? Premium { get; set; }
        public double? Retention { get; set; }
        public double? Commission { get; set; }
        public double? Amman { get; set; }
        public double? Fees1 { get; set; }
        public double? Fees2 { get; set; }
        public double? Fees3 { get; set; }
        public double? Fees4 { get; set; }
        public int? CodeOp { get; set; }
    }

    public record ViewTravelPrice([property: JsonIgnore] TravelPrice TravelPrice)
    {
        public int PlanType { get; set; } = TravelPrice.PlanType;
        public int TypeOfCoverage { get; set; } = int.Parse(TravelPrice.TypeOfCoverageId);
        public int Period { get; set; } = TravelPrice.Duration;
        public int AgeFrom { get; set; } = TravelPrice.AgeFrom;
        public int AgeTo { get; set; } = TravelPrice.AgeTo;
        public int Code { get; set; } = int.Parse(TravelPrice.AttributeCode ?? "0");
        public double ReInsurance { get; set; } = TravelPrice.ReInsuranceAmount ?? 0;
        public double Premium { get; set; } = TravelPrice.NetPremiumAmount ?? 0;
        public double Retention { get; set; } = TravelPrice.RetentionAmount ?? 0;
        public double Commission { get; set; } = TravelPrice.CommissionAmount ?? 0;

        public double Amman { get; set; } = TravelPrice.TrustAmmanAmount ?? 0;
        public double Fees1 { get; set; } = TravelPrice.Fees1 ?? 0;
        public double Fees2 { get; set; } = TravelPrice.Fees2 ?? 0;
        public double Fees3 { get; set; } = TravelPrice.Fees3 ?? 0;
        public double Fees4 { get; set; } = TravelPrice.Fees4 ?? 0;
        public int CodeOp { get; set; } = int.Parse(TravelPrice.CodeOp ?? "0");
    }
}
