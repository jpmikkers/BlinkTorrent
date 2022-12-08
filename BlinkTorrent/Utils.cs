using System.Globalization;

public static class Utils
{
    public static string ConvertSpeed(double bytesPerSecond)
    {
        return $"{ConvertSize(bytesPerSecond)}/s";
    }

    public static string ConvertSize(double bytes)
    {
        int[] precisions = new int[] { 0, 1, 2 };
        string[] prefixes = new string[] { "", "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "Zi", "Yi" };
        int t = (Math.Abs(bytes) < 1024.0) ? 0 : Math.Min(prefixes.Length - 1, (int)Math.Log(Math.Abs(bytes), 1024.0));
        int precision = precisions[Math.Min(precisions.Length - 1, t)];
        return string.Format(CultureInfo.InvariantCulture, string.Format("{{0:F{0}}} {{1}}B", precision), bytes / Math.Pow(1024.0, t), prefixes[t]);
    }
}
