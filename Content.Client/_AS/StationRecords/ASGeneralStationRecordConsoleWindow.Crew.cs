// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT
using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Shared.Administration;
using Content.Shared.StationRecords;

// Intentional to extend its partial type.
namespace Content.Client._AS.StationRecords;

/// <summary>
///     The client-side UI for the general station-records console.
/// </summary>
public sealed partial class ASGeneralStationRecordConsoleWindow
{
    private DialogWindow? _archiveReasonDialog;

    // Start Active/Archived shuttle crew tabs and owner controls
    private void PopulateShuttleCrew(List<ShuttleCrewRecordView>? crew)
    {
        ArchivedRecordListing.Clear();
        if (crew == null)
        {
            CrewTabs.Visible = true;
            CrewTabs.SetTabVisible(0, true);
            CrewTabs.SetTabVisible(1, false);
            CrewTabs.TabsVisible = false;
            CrewTabs.CurrentTab = 0;
            return;
        }

        CrewTabs.Visible = true;
        CrewTabs.SetTabVisible(0, true);
        CrewTabs.SetTabVisible(1, true);
        CrewTabs.TabsVisible = true;
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
        _selectedProfileId = crew.ProfileId;
        PopulateShuttleNotes(_selectedProfileId);
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
