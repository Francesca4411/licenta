using System.Globalization;

namespace StudyManagement.Helpers;

public static class StudyUiFormat
{
    public static string HexToRgba(string hex, double alpha)
    {
        if (string.IsNullOrWhiteSpace(hex)) return $"rgba(124,58,237,{alpha.ToString(CultureInfo.InvariantCulture)})";
        var clean = hex.Trim().TrimStart('#');
        if (clean.Length != 6) return $"rgba(124,58,237,{alpha.ToString(CultureInfo.InvariantCulture)})";

        try
        {
            var r = int.Parse(clean.Substring(0, 2), NumberStyles.HexNumber);
            var g = int.Parse(clean.Substring(2, 2), NumberStyles.HexNumber);
            var b = int.Parse(clean.Substring(4, 2), NumberStyles.HexNumber);
            return $"rgba({r},{g},{b},{alpha.ToString(CultureInfo.InvariantCulture)})";
        }
        catch
        {
            return $"rgba(124,58,237,{alpha.ToString(CultureInfo.InvariantCulture)})";
        }
    }

    public static DateTime GetWeekStartMonday(DateTime date)
    {
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return date.Date.AddDays(-diff);
    }

    public static string FormatHoursMinutes(int totalMinutes)
    {
        if (totalMinutes <= 0) return "0h 0m";
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        return $"{h}h {m}m";
    }
}
