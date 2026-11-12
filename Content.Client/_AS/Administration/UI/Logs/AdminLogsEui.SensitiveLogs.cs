using Content.Client.UserInterface.Controls;
using Content.Shared.Administration;
using Content.Shared.Administration.Logs;
using Content.Shared.Eui;
using Robust.Client.UserInterface.Controls;
using static Content.Shared.Administration.Logs.AdminLogsEuiMsg;

namespace Content.Client.Administration.UI.Logs;

public sealed partial class AdminLogsEui
{
    private void InitializeSensitiveLogs()
    {
        LogsControl.SensitiveLogRevealRequested += RequestSensitiveLogReveal;
    }

    private void RequestSensitiveLogReveal(SharedAdminLog log)
    {
        var subject = AdminLogsControl.GetSensitiveSubject(log);
        if (LogsControl.HasActiveInvestigation(subject))
        {
            SendMessage(new RevealSensitiveLogRequest(log.Id, string.Empty));
            return;
        }

        const string field = "reason";
        var entry = new QuickDialogEntry(
            field,
            QuickDialogEntryType.LongText,
            Loc.GetString("admin-logs-reveal-reason"),
            Loc.GetString("admin-logs-reveal-reason-placeholder"));
        var entries = new List<QuickDialogEntry>();
        entries.Add(entry);
        var dialog = new DialogWindow(
            Loc.GetString("admin-logs-reveal-title"),
            entries);
        var investigationNotice = new Label
        {
            Text = Loc.GetString("admin-logs-reveal-investigation-notice"),
            HorizontalExpand = true,
        };
        dialog.FindControl<BoxContainer>("Prompts")!.AddChild(investigationNotice);
        investigationNotice.SetPositionFirst();

        dialog.OnConfirmed += responses =>
        {
            // Reason must be at least 3 characters long.
            var reason = responses[field].Trim();
            if (reason.Length is < 3 or > 512)
                return;

            LogsControl.SetInvestigationActive(subject);

            SendMessage(new RevealSensitiveLogRequest(log.Id, reason));
        };
        dialog.OpenCentered();
    }

    private bool HandleSensitiveLogMessage(EuiMessageBase msg)
    {
        if (msg is not RevealedSensitiveLog revealed)
            return false;

        LogsControl.RevealLog(revealed.Log);
        return true;
    }
}
