namespace ERPWebApp.Data.Extensions;

public static class DateTimeExtension
{
    /// <summary>
    /// Converts a UTC DateTime to the specified time zone.
    /// If the DateTime is not in UTC, it is returned unchanged.
    /// </summary>
    /// <param name="dateTime">The UTC DateTime to convert.</param>
    /// <param name="timeZoneInfo">The target time zone (must be valid).</param>
    /// <returns>The DateTime converted to the specified time zone, or unchanged if not UTC.</returns>
    public static DateTime ToLocalTimeIfUtc(this DateTime dateTime, TimeZoneInfo timeZoneInfo)
    {
        if (timeZoneInfo == null)
            throw new ArgumentNullException(nameof(timeZoneInfo), "Time zone cannot be null.");

        // Convert only if the DateTime is explicitly in UTC
        return dateTime.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo)
            : dateTime;
    }
}