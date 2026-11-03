// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.StationRecords;

public sealed partial record GeneralStationRecord
{
    [DataField]
    public List<StationRecordNote> Notes = [];
}

[Serializable, NetSerializable]
public sealed class StationRecordNote(int id, int? authorProfileId, string author, string title, string description, TimeSpan createdAt)
{
    public int Id { get; set; } = id;
    public int? AuthorProfileId { get; set; } = authorProfileId;
    public string Author { get; set; } = author;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public TimeSpan CreatedAt { get; set; } = createdAt;
}
