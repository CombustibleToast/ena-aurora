namespace Content.Shared._AS.Utils;

/// <summary>
/// Helper class to format the round timer for us
/// </summary>
public static class RoundTimerUtils
{
    public static string ToString(TimeSpan ts)
    {
        return $"{ts.TotalHours:00}:{ts:mm\\:ss}";
    }
}
