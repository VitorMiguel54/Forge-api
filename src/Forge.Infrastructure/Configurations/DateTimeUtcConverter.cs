using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Forge.Infrastructure.Configurations;

internal static class DateTimeUtcConverter
{
    public static readonly ValueConverter<DateTime, DateTime> Instance = new(
        dateTime => dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime(),
        dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
}
