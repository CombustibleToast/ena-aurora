// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

// Intentional to extend its partial type.
namespace Content.Shared._NF.ShuttleRecords;

/// <summary>
/// Persistent shuttle registration data linked to a character profile and shuttle-local station record.
/// </summary>
[Virtual, NetSerializable, Serializable]
public class ShuttleCrewRecord(int profileId, string name, uint stationRecordId)
{
    [ViewVariables]
    public int ProfileId { get; set; } = profileId;

    [ViewVariables]
    public string Name { get; set; } = name;

    [ViewVariables]
    public uint StationRecordId { get; set; } = stationRecordId;

    [ViewVariables]
    public GeneralStationRecord? Record { get; set; }

    [ViewVariables]
    public TimeSpan RegisteredAt { get; set; }

    [ViewVariables]
    public TimeSpan? ExpiresAt { get; set; }

    [ViewVariables]
    public TimeSpan? ArchivedAt { get; set; }

    [ViewVariables]
    public ShuttleCrewArchiveReason? ArchiveReason { get; set; }

    [ViewVariables]
    public string? ArchivedBy { get; set; }

    [ViewVariables]
    public string? ArchiveDetails { get; set; }

    /// Prevents the shuttle owner from restoring records archived through self-removal or character deletion.
    [ViewVariables]
    public bool OwnerRestoreBlocked { get; set; }

    public bool Archived => ArchiveReason != null;
}
[Serializable, NetSerializable]
public enum ShuttleCrewArchiveReason : byte
{
    SelfRemoved,
    OwnerRemoved,
    Expired,
    CharacterDeleted,
    System,
}

/// A note attached to one shuttle crewmember profile.
[Virtual, NetSerializable, Serializable]
public class ShuttleRecordNote(int id, int targetProfileId, int? authorProfileId, string author, string title, string description, TimeSpan createdAt)
{
    public int Id { get; set; } = id;
    public int TargetProfileId { get; set; } = targetProfileId;
    public int? AuthorProfileId { get; set; } = authorProfileId;
    public string Author { get; set; } = author;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public TimeSpan CreatedAt { get; set; } = createdAt;
}

public partial class ShuttleRecord
{
    [ViewVariables]
    public int? OwnerProfileId { get; set; }

    [ViewVariables]
    public bool CrewRegistrationEnabled { get; set; }

    [ViewVariables]
    public List<ShuttleCrewRecord> Crew { get; set; } = [];

    [ViewVariables]
    public List<ShuttleRecordNote> Notes { get; set; } = [];

    [ViewVariables]
    public int NextNoteId { get; set; }
}
