using System;
using System.ComponentModel.DataAnnotations;
using TrustSystem.Backend.Application.Enums;

namespace TrustSystem.Backend.DTOs
{
    public class AdminLoginDTO
    {
        [Required(AllowEmptyStrings = true)]
        public required string Username { get; set; }

        [Required(AllowEmptyStrings = true)]
        public required string Password { get; set; }
    }

    public class AdminLoginResponseDTO
    {
        public required string Token { get; set; }
        public required string Username { get; set; }
        public required int Role { get; set; }
        public string? CashAccount { get; set; }

        public required string Officename { get; set; }
        public DateTime ExpiresAt { get; set; }

        public required UserState UserState { get; set; }
    }
}
