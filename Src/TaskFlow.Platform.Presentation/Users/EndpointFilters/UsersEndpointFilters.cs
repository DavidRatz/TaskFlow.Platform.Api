using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace TaskFlow.Platform.Presentation.Users.EndpointFilters;

public sealed class UsersEndpointFilters : IEndpointFilter
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
        catch (ResourceNotFoundException ex)
        {
            return Results.Json(new { errors = new[] { new { message = ex.Message } } }, statusCode: StatusCodes.Status404NotFound);
        }
        catch (RegistrationFailedException ex)
        {
            return Results.Json(new { errors = ex.Errors.Select(e => new { message = e }).ToArray() }, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while processing the request");
        }
    }
}
