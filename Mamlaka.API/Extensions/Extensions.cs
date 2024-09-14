namespace Mamlaka.API.Extensions;
public static class Extensions
{
    public static DateTime ToEastAfricanTime(this DateTime value)
    {
        TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(value, currentTimeZone);
    }
    public static long ToEpoch(this DateTime value)
    {
        return new DateTimeOffset(value).ToUnixTimeSeconds();
    }
}
