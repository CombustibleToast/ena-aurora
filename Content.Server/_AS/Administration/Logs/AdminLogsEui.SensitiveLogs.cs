using System.Linq;
using Content.Shared.Administration;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Eui;
using static Content.Shared.Administration.Logs.AdminLogsEuiMsg;

namespace Content.Server.Administration.Logs;

/// <summary>
/// Redacts sensitive chat logs and manages authorized, audited investigations when they are revealed.
/// </summary>
public sealed partial class AdminLogsEui
{
    private readonly Dictionary<int, SharedAdminLog> _sensitiveLogs = new();
    private readonly Dictionary<string, SensitiveLogInvestigation> _investigations = new();
    private bool _investigationsConcluded;

    private void ClearSensitiveLogRequestCache()
    {
        _sensitiveLogs.Clear();
    }

    private bool HandleSensitiveLogMessage(EuiMessageBase msg)
    {
        if (msg is not RevealSensitiveLogRequest reveal)
            return false;

        // If a sensitive log is attempted to be viewed without the correct permissions it will
        // send a log. This is not normally possible under any circumstances as the "view" button is not visible
        // under normal circumstances unless you are using a modified client or the logs mispredict when someone has their permission flag removed.
        if (!_adminManager.HasAdminFlag(Player, AdminFlags.SensitiveLogs))
        {
            _adminLogs.Add(LogType.Action,
                LogImpact.High,
                $"{Player:actor} attempted to reveal sensitive log #{reveal.LogId} without the SENSITIVELOGS permission");
            return true;
        }

        if (!_sensitiveLogs.TryGetValue(reveal.LogId, out var log))
        {
            return true;
        }

        var subject = GetSensitiveSubject(log);
        var reason = reveal.Reason.Trim();
        if (!_investigations.TryGetValue(subject, out var investigation))
        {
            if (reason.Length is < 3 or > 512)
                return true;

            investigation = new SensitiveLogInvestigation(reason, GetSensitiveSubjectDisplayName(log));
            _investigations.Add(subject, investigation);
            _adminLogs.Add(LogType.Action,
                LogImpact.High,
                $"{Player:actor} started a sensitive-log investigation into {investigation.SubjectName} from log #{log.Id} in round {_filter.Round}. Reason: {reason}");
        }
        else
        {
            _adminLogs.Add(LogType.Action,
                LogImpact.Medium,
                $"{Player:actor} revealed sensitive log #{log.Id} for {investigation.SubjectName} in round {_filter.Round} as part of an active investigation. Reason: {investigation.Reason}");
        }

        investigation.OpenedLogs.Add(log.Id);
        SendMessage(new RevealedSensitiveLog(log));
        return true;
    }

    private void ConcludeInvestigations()
    {
        if (_investigationsConcluded)
            return;

        _investigationsConcluded = true;
        foreach (var investigation in _investigations.Values)
        {
            var openedLogs = string.Join(", ", investigation.OpenedLogs.OrderBy(id => id).Select(id => $"#{id}"));
            _adminLogs.Add(LogType.Action,
                LogImpact.High,
                $"{Player:actor} concluded a sensitive-log investigation into {investigation.SubjectName}. Opened logs: {openedLogs}. Reason: {investigation.Reason}");
        }

        _investigations.Clear();
    }

    private static string GetSensitiveSubject(SharedAdminLog log)
    {
        if (log.Players.Length > 0)
            return $"UID {log.Players[0]}";

        var separator = log.Message.IndexOf(':');
        return (separator >= 0 ? log.Message[..separator] : log.Message).Trim();
    }

    private string GetSensitiveSubjectDisplayName(SharedAdminLog log)
    {
        foreach (var playerId in log.Players)
        {
            if (_players.TryGetValue(playerId, out var playerName))
                return playerName;
        }

        return "an unidentified sender";
    }

    private void RedactSensitiveLogs(List<SharedAdminLog> logs)
    {
        for (var i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            if (!IsSensitive(log))
                continue;

            _sensitiveLogs[log.Id] = log;
            logs[i] = log with
            {
                Message = RedactSensitiveMessage(log.Message), // Redacts any detected sensitive logs
                Sensitive = true,
            };
        }
    }

    // Redacts the message before we send it to the client.
    private static string RedactSensitiveMessage(string message)
    {
        var bodySeparator = message.IndexOf(':');
        var prefix = bodySeparator >= 0 ? message[..(bodySeparator + 1)] : message;

        return $"{prefix} [Sensitive chat hidden, click Reveal to view]";
    }

    // Determines whether a log is sensitive, this is stupid but works.
    private static bool IsSensitive(SharedAdminLog log)
    {
        return log.Sensitive;
    }

    private sealed class SensitiveLogInvestigation(string reason, string subjectName)
    {
        public string Reason { get; } = reason;
        public string SubjectName { get; } = subjectName;
        public HashSet<int> OpenedLogs { get; } = [];
    }
}
