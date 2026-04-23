using Carter;
using TaskFlow.Platform.Application.Users.Commands.CreateUser;
using TaskFlow.Platform.Application.Users.Commands.DeleteUser;
using TaskFlow.Platform.Application.Users.Commands.UpdateUser;
using TaskFlow.Platform.Application.Users.Queries.GetAllUsers;
using TaskFlow.Platform.Application.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TaskFlow.Platform.Presentation.Users.EndpointFilters;
using TaskFlow.Platform.Presentation.Users.Requests;

namespace TaskFlow.Platform.Presentation.Users.Endpoints;

public sealed class UsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .RequireAuthorization()
            .AddEndpointFilter<UsersEndpointFilters>();

        group.MapGet("/", GetAllAsync)
            .RequireAuthorization()
            .WithName("Users.GetAll")
            .WithTags("Users");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization()
            .WithName("Users.GetById")
            .WithTags("Users");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization()
            .WithName("Users.Create")
            .WithTags("Users");

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .WithName("Users.Update")
            .WithTags("Users");

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .WithName("Users.Delete")
            .WithTags("Users");
    }

    private static async Task<IResult> GetAllAsync(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool? asc,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAllUsersQuery(page, pageSize, search, sortBy, asc),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);

        return Results.Ok(new { data = result });
    }

    private static async Task<IResult> CreateAsync(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var address = request.Address is null
            ? null
            : new CreateUserAddressDto(
                request.Address.Street,
                request.Address.City,
                request.Address.PostalCode,
                request.Address.Country);

        var result = await mediator.Send(
            new CreateUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Type,
                address),
            cancellationToken);

        return Results.Created($"/users/{result.Id}", new { data = result });
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var address = request.Address is null
            ? null
            : new UpdateUserAddressDto(
                request.Address.Street,
                request.Address.City,
                request.Address.PostalCode,
                request.Address.Country);

        var result = await mediator.Send(
            new UpdateUserCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Phone,
                request.LegalName,
                request.VatNumber,
                address),
            cancellationToken);

        return Results.Ok(new { data = result });
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteUserCommand(id), cancellationToken);

        return Results.NoContent();
    }
}
