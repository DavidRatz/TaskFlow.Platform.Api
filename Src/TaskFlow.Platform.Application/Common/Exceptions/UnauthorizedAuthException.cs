namespace TaskFlow.Platform.Application.Common.Exceptions;

public sealed class UnauthorizedAuthException(string message = "Unauthorized access") : Exception(message);
