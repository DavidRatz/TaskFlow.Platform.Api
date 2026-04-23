using Microsoft.AspNetCore.Http;
using TaskFlow.Platform.Application.Auth.Exceptions;
using TaskFlow.Platform.Application.Common.Exceptions;

namespace TaskFlow.Platform.Presentation.Auth.EndpointFilters;

public class AuthenticationEndpointFilters : IEndpointFilter
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
        catch (RegistrationFailedException ex)
        {
            var errors = ex.Errors.Select(e => new { message = e }).ToArray();
            return Results.UnprocessableEntity(new { errors });
        }
        catch (InvalidCredentialsException)
        {
            return Results.BadRequest(new { errors = new[] { new { message = "Identifiants invalides" } } });
        }
        catch (BadRequestException ex)
        {
            return Results.BadRequest(new { errors = new[] { new { message = ex.Message } } });
        }
        catch (UnauthorizedAuthException)
        {
            return Results.Json(new { errors = new[] { new { message = "Unauthorized access" } } }, statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (Exception)
        {
            return Results.Problem($"An error occurred while processing the request");
        }
    }
}
