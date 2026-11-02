using Microsoft.AspNetCore.Mvc;
using TrustSystem.Backend.Application.Enums;
using TrustSystem.Backend.DTOs;
using TrustSystem.Backend.Infrastructure.Repositories;
using TrustSystem.Backend.Services;

namespace TrustSystem.Backend.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/auth")
                        .WithTags("Login & Registration (No Auth Required)");

        group.MapPost("/admins", async (UserRepository userRepository,
                                        JwtService jwt,
                                        IConfiguration _config,
                                        [FromBody] AdminLoginDTO request) =>
        {
            var (UserState, UserInstance) = await userRepository.LoginToUser(request.Username, request.Password);

            if (UserInstance is not null)
            {
                var token = jwt.GenerateToken(UserInstance.Code,
                                              UserInstance.CompanyGroupId.ToString(),
                                              UserInstance.Role.ToString());

                var response = new AdminLoginResponseDTO
                {
                    Token = token,
                    Username = UserInstance.Code,

                    Role = UserInstance.Role,
                    Officename = UserInstance.Office?.NameInEnglish ?? "N/A",
                    CashAccount = UserInstance.CashAccount,

                    ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"]!)),
                    UserState = UserState
                };

                return Results.Ok(response);
            }
            else
                return Results.BadRequest(
                        UserState switch
                        {
                            UserState.EXPIRED => "User account is expired. Please contact support.",
                            UserState.LOCKED => "User account is locked. Please contact support.",
                            _ => "Invalid Username or Password."
                        }
                    );
        })
        .Produces<AdminLoginResponseDTO>();

        return group;
    }
}
