using System.Security.Claims;
using Carter;
using TaskFlow.Platform.Application.Users.Commands.DeleteMe;
using TaskFlow.Platform.Application.Users.Commands.UpdateMe;
using TaskFlow.Platform.Application.Users.Dtos;
using TaskFlow.Platform.Application.Users.Queries.GetMe;
using TaskFlow.Platform.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskFlow.Platform.Presentation.Me.EndpointFilters;
using TaskFlow.Platform.Presentation.Me.Requests;

namespace TaskFlow.Platform.Presentation.Me.Endpoints;

public sealed class MeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/me")
            .RequireAuthorization()
            .AddEndpointFilter<MeEndpointFilters>();

        group.MapGet("/", GetMeAsync)
            .RequireAuthorization()
            .WithName("Me.Get")
            .WithTags("Me");

        group.MapPut("/", UpdateMeAsync)
            .RequireAuthorization()
            .WithName("Me.Update")
            .WithTags("Me");

        group.MapDelete("/", DeleteMeAsync)
            .RequireAuthorization()
            .WithName("Me.Delete")
            .WithTags("Me");
    }

    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal principal,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var idUser = principal.GetUserId();
        var user = await mediator.Send(new GetMeQuery(idUser), cancellationToken);

        return Results.Ok(new
        {
            data = ToUserJson(user),
        });
    }

    private static async Task<IResult> UpdateMeAsync(
        HttpContext httpContext,
        UpdateMeRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();
        var address = request.Address is null
            ? null
            : new UpdateAuthenticatedUserAddressDto(
                request.Address.Street,
                request.Address.City,
                request.Address.PostalCode,
                request.Address.Country);
        var result = await mediator.Send(
            new UpdateMeCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.Phone,
                request.LegalName,
                request.VatNumber,
                address),
            cancellationToken);

        return Results.Ok(new
        {
            data = ToUserJson(result),
        });
    }

    private static async Task<IResult> DeleteMeAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();
        var jti = httpContext.User.GetJti();
        await mediator.Send(
            new DeleteMeCommand(userId, jti, httpContext.Request.Headers.Authorization.ToString()),
            cancellationToken);

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
            vatNumber = user.VatNumber
        };
    }
}
