namespace TaskFlow.Platform.Application.Common.Exceptions;

public class ResourceNotFoundException(string message = "Resource not found") : Exception(message);
