// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT
using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Shared.Administration;
using Content.Shared.StationRecords;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

// Intentional to extend its partial type.
namespace Content.Client._AS.StationRecords;

/// <summary>
///     The client-side UI for the general station-records console.
/// </summary>
public sealed partial class ASGeneralStationRecordConsoleWindow
{
    private const int MaxNotesPerCrewmember = 10;

    private DialogWindow? _archiveReasonDialog;
    private ASShuttleNoteEditorWindow? _newNoteDialog;
    private ASShuttleNoteViewWindow? _viewNoteWindow;

    // Start Active/Archived shuttle crew tabs and owner controls
    private void PopulateShuttleCrew(List<ShuttleCrewRecordView>? crew)
    {
        ArchivedRecordListing.Clear();
        if (crew == null)
        {
            CrewTabs.Visible = false;
            return;
        }

        CrewTabs.Visible = true;
        var archived = crew.Where(x => x.ArchiveReason.HasValue).ToList();
        if (_viewingArchivedProfile is { } selectedProfile && archived.All(x => x.ProfileId != selectedProfile))
            _viewingArchivedProfile = null;
        CrewTabs.SetTabTitle(0, Loc.GetString("general-station-record-console-crew-tab-active"));
        CrewTabs.SetTabTitle(1, Loc.GetString("general-station-record-console-crew-tab-archived"));
        foreach (var entry in archived)
        {
            var item = ArchivedRecordListing.AddItem(entry.Name); // Archived entry
            item.Metadata = entry;
        }
        ArchivedRecordListing.SortItemsByText();
    }

    private void PopulateArchivedRecord(ShuttleCrewRecordView crew)
    {
        RecordContainer.RemoveAllChildren();
        RecordPortraitContainer.RemoveAllChildren();
        RecordHeaderContainer.RemoveAllChildren();
        RecordContainerStatus.Visible = false;
        RecordContainer.Visible = true;
        if (crew.Record != null)
        {
            var record = new ASGeneralRecord(crew.Record, false, null, _prototypeManager, crew);
            record.OnCrewRestored = OnShuttleCrewRestored;
            AttachRecordView(record);
        }
        // Archived entries intentionally omit live/SSD presence
        _selectedCrew = crew;
        PopulateShuttleNotes(crew.ProfileId);
    }

    // Shuttle notes
    private void PopulateShuttleNotes(int? targetProfileId)
    {
        ShuttleNotesList.RemoveAllChildren();
        var relevant = targetProfileId is { } profileId ? _shuttleNotes?.Where(note => note.TargetProfileId == profileId).ToList() : null;
        NewShuttleNoteButton.Disabled = targetProfileId == null ||
                                        !_canCreateShuttleNotes ||
                                        relevant?.Count >= MaxNotesPerCrewmember;

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

        // fucked up
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
                delete.OnPressed += _ => OnShuttleNoteDeleted?.Invoke(note.Id);
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
        if (_selectedCrew == null || NewShuttleNoteButton.Disabled)
            return;
        if (_newNoteDialog != null)
        {
            _newNoteDialog.MoveToFront();
            return;
        }

        var targetProfileId = _selectedCrew.ProfileId;
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

    // Gets a formatted string for a note's creation time, in HH:MM:SS format.
    private static string FormatNoteTime(TimeSpan time)
    {
        return $"{(int) time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
    }

    private void OpenArchiveReasonDialog(uint stationRecordId)
    {
        if (_archiveReasonDialog != null)
        {
            _archiveReasonDialog.MoveToFront();
            return;
        }

        const string field = "reason";
        var entries = new List<QuickDialogEntry>
        {
            new(field, QuickDialogEntryType.LongText,
                Loc.GetString("general-station-record-console-archive-reason-label"),
                Loc.GetString("general-station-record-console-archive-reason-placeholder")),
        };
        _archiveReasonDialog = new DialogWindow(Loc.GetString("general-station-record-console-archive-window-title"), entries);
        _archiveReasonDialog.OnConfirmed += responses =>
        {
            var reason = responses[field].Trim();
            if (reason.Length is < 1 or > 30)
                return;

            OnShuttleCrewArchived?.Invoke(stationRecordId, reason);
        };
        _archiveReasonDialog.OnClose += () => _archiveReasonDialog = null;
        _archiveReasonDialog.OpenCentered();
    }
}
