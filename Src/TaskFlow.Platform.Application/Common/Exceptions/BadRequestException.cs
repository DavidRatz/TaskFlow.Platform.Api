namespace TaskFlow.Platform.Application.Common.Exceptions;

public sealed class BadRequestException(string message = "Bad request") : Exception(message);
