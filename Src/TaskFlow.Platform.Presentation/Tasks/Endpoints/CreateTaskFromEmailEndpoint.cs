using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskFlow.Platform.Application.Tasks.Commands;
using TaskFlow.Platform.Application.Tasks.Queries.GetAllTasks;
////using TaskFlow.Platform.Infrastructure.Emails.Options;
using TaskFlow.Platform.Presentation.Tasks.Requests;

namespace TaskFlow.Platform.Presentation.Tasks.Endpoints;

public class CreateTaskFromEmailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .RequireAuthorization()
            .WithName("Tasks.GetAll")
            .WithTags("Tasks");

        group.MapPost("/from-email", CreateTaskFromEmailAsync)
            .WithName("Tasks.CreateFromEmail")
            .WithTags("Tasks");
    }

    private static async Task<IResult> GetAllAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAllTasksQuery(),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateTaskFromEmailAsync(
        CreateTaskFromEmailRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTaskFromEmailCommand(
                request.Subject,
                request.Message,
                request.Email),
            cancellationToken);

        return Results.Created($"/tasks/{result.Id}", new { data = result });
    }
}
