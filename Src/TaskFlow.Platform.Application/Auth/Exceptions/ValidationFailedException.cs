using TaskFlow.Platform.Application.Common.Dtos;

namespace TaskFlow.Platform.Application.Auth.Exceptions;

public sealed class ValidationFailedException(IReadOnlyCollection<ValidationErrorDto> errors) : Exception("Validation failed")
{
    public IReadOnlyCollection<ValidationErrorDto> Errors { get; } = errors;
}
