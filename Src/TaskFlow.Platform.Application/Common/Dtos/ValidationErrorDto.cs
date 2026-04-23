namespace TaskFlow.Platform.Application.Common.Dtos;

public sealed record ValidationErrorDto(string Message, string Rule, string Field);
