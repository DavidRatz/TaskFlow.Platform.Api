using TaskFlow.Platform.Domain.Abstractions;

namespace TaskFlow.Platform.Infrastructure.Common;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
