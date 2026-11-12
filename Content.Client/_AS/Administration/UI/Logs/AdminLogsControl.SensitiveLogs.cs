using System.Linq;
using Content.Client.Administration.UI.CustomControls;
using Content.Client.UserInterface.Controls;
using Content.Shared.Administration.Logs;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.Logs;

public sealed partial class AdminLogsControl
{
    private const float SensitiveLogButtonWidth = 110f;

    public event Action<SharedAdminLog>? SensitiveLogRevealRequested;

    private readonly HashSet<string> _activeInvestigations = [];
    private bool _showSensitiveLogControls;

    private static bool MatchesLogSearch(SharedAdminLog log, string search)
    {
        // Sensitive results have already been filtered by the server.
        // The original text is never sent to the client until viewed.
        return log.Sensitive || log.Message.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    // Shows the control for viewing sensitive admin logs
    private Control CreateSensitiveLogControl(AdminLogLabel label, SharedAdminLog log)
    {
        if (!log.Sensitive || !_showSensitiveLogControls)
            return label;

        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Visible = label.Visible,
        };
        label.HorizontalExpand = true;
        row.AddChild(label);

        var subject = GetSensitiveSubject(log);
        var investigationActive = _activeInvestigations.Contains(subject);
        var reveal = new Button
        {
            Text = Loc.GetString("admin-logs-reveal-sensitive"),
            MinWidth = SensitiveLogButtonWidth,
            MaxWidth = SensitiveLogButtonWidth,
            Visible = !investigationActive,
        };
        reveal.OnPressed += _ => SensitiveLogRevealRequested?.Invoke(log);
        row.AddChild(reveal);

        var confirmReveal = new ConfirmButton
        {
            Text = Loc.GetString("admin-logs-reveal-sensitive"),
            ConfirmationText = Loc.GetString("admin-logs-reveal-confirm"),
            MinWidth = SensitiveLogButtonWidth,
            MaxWidth = SensitiveLogButtonWidth,
            Visible = investigationActive,
        };
        confirmReveal.OnPressed += _ => SensitiveLogRevealRequested?.Invoke(log);
        row.AddChild(confirmReveal);
        return row;
    }

    public void SetInvestigationActive(string subject)
    {
        _activeInvestigations.Add(subject);

        foreach (var child in LogsContainer.Children)
        {
            if (child is not BoxContainer row)
                continue;

            var label = row.Children.OfType<AdminLogLabel>().FirstOrDefault();
            if (label == null || GetSensitiveSubject(label.Log) != subject)
                continue;

            foreach (var button in row.Children.OfType<Button>())
                button.Visible = button is ConfirmButton;
        }
    }

    public void SetSensitiveLogControlsVisible(bool value)
    {
        _showSensitiveLogControls = value;
    }

    public bool HasActiveInvestigation(string subject)
    {
        return _activeInvestigations.Contains(subject);
    }

    public static string GetSensitiveSubject(SharedAdminLog log)
    {
        if (log.Players.Length > 0)
            return $"UID {log.Players[0]}";

        var separator = log.Message.IndexOf(':');
        return (separator >= 0 ? log.Message[..separator] : log.Message).Trim();
    }

    private void UpdateSeparatorVisibility(Control child, int index)
    {
        if (child is BoxContainer &&
            index + 1 < LogsContainer.ChildCount &&
            LogsContainer.GetChild(index + 1) is HSeparator separator)
        {
            separator.Visible = child.Visible;
        }
    }

    public void RevealLog(SharedAdminLog revealed)
    {
        foreach (var child in LogsContainer.Children)
        {
            if (child is not BoxContainer row)
                continue;

            var label = row.Children.OfType<AdminLogLabel>().FirstOrDefault();
            if (label == null || label.Log.Id != revealed.Id)
                continue;

            label.Reveal(revealed);
            foreach (var button in row.Children.OfType<Button>())
                button.Visible = false;
            return;
        }
    }

    public IEnumerable<AdminLogLabel> VisibleLogLabels()
    {
        foreach (var child in LogsContainer.Children)
        {
            if (!child.Visible)
                continue;

            var label = GetLogLabel(child);
            if (label != null)
                yield return label;
        }
    }

    private static AdminLogLabel? GetLogLabel(Control control)
    {
        if (control is AdminLogLabel label)
            return label;

        return control is BoxContainer row
            ? row.Children.OfType<AdminLogLabel>().FirstOrDefault()
            : null;
    }
}
