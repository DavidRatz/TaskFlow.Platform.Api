namespace TaskFlow.Platform.Domain.Abstractions;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
