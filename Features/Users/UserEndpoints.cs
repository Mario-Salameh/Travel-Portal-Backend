using Microsoft.AspNetCore.Mvc;
using TrustSystem.Backend.Application.Contracts;
using TrustSystem.Backend.Infrastructure.Repositories;

namespace TrustSystem.Backend.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/users")
                       .WithTags("Users")
                       .RequireAuthorization("admin");

        group.MapGet("/", async (UserRepository userRepository) =>
        {
            var users = await userRepository.GetAllUsersAsync();

            return Results.Ok(users.Select(user => new ViewUser(user)).ToList());
        })
        .Produces<List<ViewUser>>();

        group.MapPost("/", async (UserRepository userRepository, [FromBody] CreateUserRequest request) =>
        {
            await userRepository.AddUserAsync(request);
            return Results.Created();
        });

        group.MapPut("/{UserCode}", async (UserRepository userRepository, [FromRoute] string UserCode, [FromBody] UpdateUserRequest request) =>
        {
            await userRepository.UpdateUserAsync(UserCode, request);
            return Results.Ok();
        });

        group.MapDelete("/", async (UserRepository userRepository, [FromBody] DeleteUserRequest request) =>
        {
            await userRepository.DeleteUserAsync(request.UserCode);
            return Results.Ok();
        });

        // UNLOCK USER
        group.MapPut("/{UserCode}/unlock", async (UserRepository userRepository, [FromRoute] string UserCode) =>
        {
            await userRepository.LockUnlockUserAsync(UserCode);
            return Results.Ok();
        });

        return group;
    }
}
