using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;
namespace TrustSystem.Backend.DTOs
{
    public class CreateCurrencyRateDTO
    {
        [Required]
        public required DateOnly startingDate { get; set; }

        [Required]
        public required double exchangeRate { get; set; }
    }

    public record CurrencyRateViewDTO([property: JsonIgnore] CurrencyRate currencyRate)
    {
        public int id { get; set; } = currencyRate.Id;
        public DateTime startDate { get; set; } = currencyRate.StartDate;
        public double exchangeRate { get; set; } = currencyRate.ExchangeRate;
    }
}
