namespace TaskFlow.Platform.Application.Common.Exceptions;

public class ConflictException(string message = "Conflict") : Exception(message);
