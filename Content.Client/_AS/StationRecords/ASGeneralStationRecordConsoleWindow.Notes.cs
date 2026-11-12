// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Shared.StationRecords;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._AS.StationRecords;

public sealed partial class ASGeneralStationRecordConsoleWindow
{
    private const int MaxNotesPerCrewmember = 10;

    private ASShuttleNoteEditorWindow? _newNoteDialog;
    private ASShuttleNoteViewWindow? _viewNoteWindow;

    private void PopulateShuttleNotes(int? targetProfileId)
    {
        ShuttleNotesList.RemoveAllChildren();
        var relevant = targetProfileId is { } profileId ? _shuttleNotes?.Where(note => note.TargetProfileId == profileId).ToList() : null;
        NewShuttleNoteButton.Disabled = targetProfileId == null || !_canCreateShuttleNotes || relevant?.Count >= MaxNotesPerCrewmember;

        if (relevant == null || relevant.Count == 0)
        {
            ShuttleNotesList.AddChild(new Label
            {
                Text = Loc.GetString(targetProfileId == null
                    ? "general-station-record-console-note-select-record"
                    : "general-station-record-console-note-empty"),
                FontColorOverride = Color.FromHex("#888888"),
            });
            return;
        }

        foreach (var note in relevant.OrderByDescending(note => note.CreatedAt))
        {
            var card = new PanelContainer
            {
                HorizontalExpand = true,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = Color.FromHex("#181B22"),
                    BorderColor = Color.FromHex("#303A66"),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    ContentMarginLeftOverride = 6,
                    ContentMarginTopOverride = 4,
                    ContentMarginRightOverride = 6,
                    ContentMarginBottomOverride = 5,
                },
            };
            var body = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, HorizontalExpand = true };
            var heading = new BoxContainer { HorizontalExpand = true };
            heading.AddChild(new Label { Text = note.Title, StyleClasses = { "LabelKeyText" } });
            heading.AddChild(new Control { HorizontalExpand = true });
            var view = new Button { Text = Loc.GetString("general-station-record-console-note-view") };
            view.OnPressed += _ => OpenNoteView(note);
            heading.AddChild(view);
            if (note.CanDelete)
            {
                var delete = new Button { Text = Loc.GetString("general-station-record-console-note-delete") };
                delete.OnPressed += _ => OnShuttleNoteDeleted?.Invoke(note.TargetProfileId, note.Id);
                heading.AddChild(delete);
            }
            body.AddChild(heading);
            body.AddChild(new Label
            {
                Text = $"{note.Author} - {FormatNoteTime(note.CreatedAt)}",
                FontColorOverride = Color.FromHex("#888888"),
            });
            var description = new RichTextLabel { HorizontalExpand = true };
            description.SetMessage(FormattedMessage.FromUnformatted(TruncateNotePreview(note.Description)));
            body.AddChild(description);
            card.AddChild(body);
            ShuttleNotesList.AddChild(card);
        }
    }

    private void OpenNewNoteDialog()
    {
        if (_selectedProfileId is not { } targetProfileId || NewShuttleNoteButton.Disabled)
            return;
        if (_newNoteDialog != null)
        {
            _newNoteDialog.MoveToFront();
            return;
        }

        _newNoteDialog = new ASShuttleNoteEditorWindow();
        _newNoteDialog.OnSaved += (title, description) => OnShuttleNoteAdded?.Invoke(targetProfileId, title, description);
        _newNoteDialog.OnClose += () => _newNoteDialog = null;
        _newNoteDialog.OpenCentered();
    }

    private void OpenNoteView(ShuttleRecordNoteView note)
    {
        _viewNoteWindow?.Close();
        _viewNoteWindow = new ASShuttleNoteViewWindow(note, FormatNoteTime(note.CreatedAt));
        _viewNoteWindow.OnClose += () => _viewNoteWindow = null;
        _viewNoteWindow.OpenCentered();
    }

    private static string TruncateNotePreview(string text)
    {
        const int maxLength = 180;
        var preview = text.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return preview.Length <= maxLength ? preview : $"{preview[..maxLength].TrimEnd()}…";
    }

    private static string FormatNoteTime(TimeSpan time)
    {
        return $"{(int) time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
    }
}
