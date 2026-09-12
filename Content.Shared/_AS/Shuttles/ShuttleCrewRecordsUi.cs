// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._NF.ShuttleRecords;
using Robust.Shared.Serialization;

// Intentional to extend its partial type.
namespace Content.Shared.StationRecords;

/// <summary>
/// UI state for the general station-records console.
/// <see cref="RecordListing"/> contains the records matching the current filter.
/// It may be null when the database is unavailable or no records match.
///
/// <see cref="SelectedKey"/> identifies the selected record, while
/// <see cref="Record"/> contains that record's data when it is available.
/// </summary>
public sealed partial class GeneralStationRecordConsoleState
{
    /// Notes belonging to the shuttle associated with the console.
    public List<ShuttleRecordNoteView>? ShuttleNotes { get; set; }

    /// Active and archived crew registrations for the shuttle associated with the console.
    public List<ShuttleCrewRecordView>? ShuttleCrew { get; set; }

    /// Shuttle-local station record protected from archival and deletion as the owner's record.
    public uint? ShuttleOwnerStationRecordId { get; set; }

    /// <summary>
    /// Whether this console is associated with shuttle records that accept notes.
    /// Note creation itself is intentionally available to every viewer.
    /// </summary>
    public bool CanCreateShuttleNotes { get; set; }
}

/// Live presence displayed for active shuttle crew records. Archived entries intentionally omit presence.
[Serializable, NetSerializable]
public enum ShuttleCrewPresence : byte
{
    wtf, // Associated character cannot be found
    Active, // Character exists and player is online
    SSD, // Character exists but player is offline
    Cryo, // Character exists but is in cryo
}

/// Client-facing shuttle crew data. <see cref="Record"/> is populated for archived entries only.
[Serializable, NetSerializable]
public sealed class ShuttleCrewRecordView
{
    public int ProfileId { get; set; }
    public uint StationRecordId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan RegisteredAt { get; set; }
    public TimeSpan? ExpiresAt { get; set; }
    public TimeSpan? ArchivedAt { get; set; }
    public ShuttleCrewArchiveReason? ArchiveReason { get; set; }
    public string? ArchivedBy { get; set; }
    public string? ArchiveDetails { get; set; }
    public bool OwnerRestoreBlocked { get; set; }
    public ShuttleCrewPresence Presence { get; set; }
    public bool CanManage { get; set; }
    public bool IsShuttleOwner { get; set; }
    public GeneralStationRecord? Record { get; set; }
}

[Serializable, NetSerializable]
public sealed class ShuttleRecordNoteView(int id, int targetProfileId, string author, string title, string description, TimeSpan createdAt, bool canDelete)
{
    public int Id { get; set; } = id;
    public int TargetProfileId { get; set; } = targetProfileId;
    public string Author { get; set; } = author;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public TimeSpan CreatedAt { get; set; } = createdAt;
    public bool CanDelete { get; set; } = canDelete;
}

[Serializable, NetSerializable]
public sealed class AddShuttleRecordNote(int targetProfileId, string title, string description) : BoundUserInterfaceMessage
{
    public int TargetProfileId { get; set; } = targetProfileId;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
}

[Serializable, NetSerializable]
public sealed class DeleteShuttleRecordNote(int id) : BoundUserInterfaceMessage
{
    public int Id { get; set; } = id;
}

/// Per-viewer permissions sent separately from shared UI state so simultaneous users retain correct controls.
[Serializable, NetSerializable]
public sealed class ShuttleRecordViewerPermissions(bool canManage, HashSet<int> deletableNoteIds) : BoundUserInterfaceMessage
{
    public bool CanManage { get; } = canManage;
    public HashSet<int> DeletableNoteIds { get; } = deletableNoteIds;
}

[Serializable, NetSerializable]
public sealed class UpdateShuttleCrewRecord(uint stationRecordId, int minutes) : BoundUserInterfaceMessage
{
    public uint StationRecordId { get; } = stationRecordId;
    public int Minutes { get; } = minutes;
}

[Serializable, NetSerializable]
public sealed class RestoreShuttleCrewRecord(uint stationRecordId) : BoundUserInterfaceMessage
{
    public uint StationRecordId { get; } = stationRecordId;
}

[Serializable, NetSerializable]
public sealed class ArchiveShuttleCrewRecord(uint stationRecordId, string reason) : BoundUserInterfaceMessage
{
    public uint StationRecordId { get; } = stationRecordId;
    public string Reason { get; } = reason;
}
