namespace TaskFlow.Platform.Application.Auth.Exceptions;

public sealed class RegistrationFailedException(IReadOnlyCollection<string> errors) : Exception("Registration failed")
{
    public IReadOnlyCollection<string> Errors { get; } = errors;
}
