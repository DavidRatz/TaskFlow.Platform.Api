using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Platform.Presentation.Me.EndpointFilters;

public sealed class MeEndpointFilters : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (ValidationFailedException ex)
        {
            var errors = ex.Errors.Select(e => new { message = e.Message, rule = e.Rule, field = e.Field }).ToArray();
            return Results.UnprocessableEntity(new { errors });
        }
        catch (UnauthorizedAuthException)
        {
            return Results.Json(new { errors = new[] { new { message = "Unauthorized access" } } }, statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (BadRequestException ex)
        {
            return Results.Json(new { errors = new[] { new { message = ex.Message } } }, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (ConflictException ex)
        {
            return Results.Json(new { errors = new[] { new { message = ex.Message } } }, statusCode: StatusCodes.Status409Conflict);
        }
        catch (ResourceNotFoundException ex)
        {
            return Results.Json(new { errors = new[] { new { message = ex.Message } } }, statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger<MeEndpointFilters>();
            logger.LogError(ex, "Unhandled exception in endpoint {Path}", context.HttpContext.Request.Path);
            return Results.Problem($"An error occurred: {ex.Message}");
        }
    }
}
