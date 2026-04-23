using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TaskFlow.Platform.Application.Emails.Queries;
using TaskFlow.Platform.Presentation.Emails.EndpointFilters;

namespace TaskFlow.Platform.Presentation.Emails.Endpoints;

public sealed class EmailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/emails")
            .RequireAuthorization()
            .AddEndpointFilter<EmailsEndpointFilters>();

        group.MapGet("/", GetEmailsAsync)
            .RequireAuthorization()
            .WithName("Emails.GetAll")
            .WithTags("Emails");
    }

    private static async Task<IResult> GetEmailsAsync(
        [FromQuery] string provider,
        [FromQuery] int? maxEmails,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetEmailsQuery(provider, (maxEmails ?? 10)),
            cancellationToken);

        return Results.Ok(result);
    }
}
