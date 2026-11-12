namespace Content.Server.Administration.Logs;

/// <summary>
/// Allows explicit admin log ID searches using <c>id:123</c> syntax.
/// </summary>
public sealed partial class AdminLogsEui
{
    private static string? NormalizeLogSearch(string? search, out int? logId)
    {
        logId = null;
        if (search == null)
            return null;

        var trimmed = search.Trim();
        const string logIdPrefix = "id:";
        if (!trimmed.StartsWith(logIdPrefix, StringComparison.OrdinalIgnoreCase) ||
            !int.TryParse(trimmed.AsSpan(logIdPrefix.Length), out var parsedId))
            return search;
        logId = parsedId;
        return null;

    }
}
