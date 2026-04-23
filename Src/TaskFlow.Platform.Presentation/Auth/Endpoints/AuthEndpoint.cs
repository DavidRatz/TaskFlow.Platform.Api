using Carter;
using TaskFlow.Platform.Application.Auth.Commands.ForgotPassword;
using TaskFlow.Platform.Application.Auth.Commands.Login;
using TaskFlow.Platform.Application.Auth.Commands.Logout;
using TaskFlow.Platform.Application.Auth.Commands.Register;
using TaskFlow.Platform.Application.Auth.Commands.ResetPassword;
using TaskFlow.Platform.Application.Auth.Commands.SetPassword;
using TaskFlow.Platform.Application.Auth.Dtos;
using TaskFlow.Platform.Application.Users.Dtos;
using TaskFlow.Platform.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskFlow.Platform.Presentation.Auth.EndpointFilters;
using TaskFlow.Platform.Presentation.Auth.Requests;

namespace TaskFlow.Platform.Presentation.Auth.Endpoints;

public class AuthEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").AddEndpointFilter<AuthenticationEndpointFilters>();

        group.MapPost("/login", LoginAsync)
            .WithName("Auth.Login")
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("Auth.Register")
            .WithTags("Auth");

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Auth.Logout")
            .WithTags("Auth");

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("Auth.ForgotPassword")
            .WithTags("Auth");

        group.MapPost("/set-password", SetPasswordAsync)
            .WithName("Auth.SetPassword")
            .WithTags("Auth");

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("Auth.ResetPassword")
            .WithTags("Auth");
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);

        return Results.Ok(new
        {
            data = new
            {
                user = ToUserJson(result.User),
                token = result.Token,
            },
        });
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var address = new RegisterAddressDto(request.Address.Street, request.Address.City, request.Address.PostalCode, request.Address.Country);

        var result = await mediator.Send(
            new RegisterCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Phone,
                address,
                request.LegalName,
                request.VatNumber),
            cancellationToken);

        return Results.Created("/auth/register", new
        {
            data = new
            {
                user = ToUserJson(result.User),
                token = result.Token,
            },
        });
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();
        var jti = httpContext.User.GetJti();
        await mediator.Send(
            new LogoutCommand(userId, jti, httpContext.Request.Headers.Authorization.ToString()),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> SetPasswordAsync(
        SetPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new SetPasswordCommand(request.Token, request.Password), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ResetPasswordCommand(request.Token, request.Password), cancellationToken);
        return Results.NoContent();
    }

    private static object ToUserJson(UserDto user)
    {
        return new
        {
            id = user.Id,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            phone = user.Phone,
            addressId = user.AddressId,
            legalName = user.LegalName,
            vatNumber = user.VatNumber,
        };
    }
}
