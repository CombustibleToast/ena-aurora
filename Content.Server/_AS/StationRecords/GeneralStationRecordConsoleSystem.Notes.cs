// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.GameTicking;
using Content.Server.StationRecords.Components;
using Content.Shared.StationRecords;

namespace Content.Server.StationRecords.Systems;

public sealed partial class GeneralStationRecordConsoleSystem
{
    [Dependency] private GameTicker _gameTicker = null!;

    private void OnAddShuttleNote(Entity<GeneralStationRecordConsoleComponent> ent, ref AddShuttleRecordNote args)
    {
        var changed = _shuttleRecords.HasConsoleShuttleRecord(ent.Owner)
            ? _shuttleRecords.TryAddShuttleNote(ent.Owner, args.Actor, args.TargetProfileId, args.Title, args.Description)
            : TryAddStationNote(ent.Owner, args.Actor, args.TargetProfileId, args.Title, args.Description);
        if (changed)
            UpdateUserInterface(ent);
    }

    private void OnDeleteShuttleNote(Entity<GeneralStationRecordConsoleComponent> ent, ref DeleteShuttleRecordNote args)
    {
        var changed = _shuttleRecords.HasConsoleShuttleRecord(ent.Owner)
            ? _shuttleRecords.TryDeleteShuttleNote(ent.Owner, args.Actor, args.Id)
            : TryDeleteStationNote(ent.Owner, args.Actor, args.TargetProfileId, args.Id);
        if (changed)
            UpdateUserInterface(ent);
    }

    private bool TryAddStationNote(EntityUid consoleUid, EntityUid user, int targetProfileId, string title, string description)
    {
        title = title.Trim();
        description = description.Trim();
        if (title.Length is < 1 or > 100 || description.Length is < 1 or > 1000 ||
            !TryGetStationRecord(consoleUid, targetProfileId, out var stationUid, out var record) ||
            record.Notes.Count >= 10)
            return false;

        var nextId = _stationRecords.GetRecordsOfType<GeneralStationRecord>(stationUid)
            .SelectMany(entry => entry.Item2.Notes)
            .Select(note => note.Id)
            .DefaultIfEmpty(-1)
            .Max() + 1;
        record.Notes.Add(new StationRecordNote(nextId,
            _shuttleRecords.GetUserProfileId(user),
            Name(user),
            title,
            description,
            _timing.CurTime.Subtract(_gameTicker.RoundStartTimeSpan)));
        _stationRecords.Synchronize(stationUid);
        return true;
    }

    private bool TryDeleteStationNote(EntityUid consoleUid, EntityUid user, int targetProfileId, int noteId)
    {
        var profileId = _shuttleRecords.GetUserProfileId(user);
        if (profileId == null || !TryGetStationRecord(consoleUid, targetProfileId, out var stationUid, out var record) ||
            record.Notes.FirstOrDefault(note => note.Id == noteId && note.AuthorProfileId == profileId) is not { } note)
            return false;

        record.Notes.Remove(note);
        _stationRecords.Synchronize(stationUid);
        return true;
    }

    private List<ShuttleRecordNoteView> GetStationNoteViews(EntityUid consoleUid)
    {
        if (_station.GetOwningStation(consoleUid) is not { } stationUid || !HasComp<StationRecordsComponent>(stationUid))
            return [];

        return _stationRecords.GetRecordsOfType<GeneralStationRecord>(stationUid)
            .Select(entry => entry.Item2)
            .Where(record => record.ProfileId != null)
            .SelectMany(record => record.Notes.Select(note => new ShuttleRecordNoteView(note.Id,
                record.ProfileId!.Value,
                note.Author,
                note.Title,
                note.Description,
                note.CreatedAt,
                false)))
            .ToList();
    }

    private HashSet<int> GetDeletableStationNoteIds(EntityUid consoleUid, int? profileId)
    {
        if (profileId == null || _station.GetOwningStation(consoleUid) is not { } stationUid || !HasComp<StationRecordsComponent>(stationUid))
            return [];

        return _stationRecords.GetRecordsOfType<GeneralStationRecord>(stationUid)
            .SelectMany(entry => entry.Item2.Notes)
            .Where(note => note.AuthorProfileId == profileId)
            .Select(note => note.Id)
            .ToHashSet();
    }

    private bool TryGetStationRecord(EntityUid consoleUid, int profileId, out EntityUid stationUid, out GeneralStationRecord record)
    {
        stationUid = default;
        record = null!;
        if (_station.GetOwningStation(consoleUid) is not { } station || !HasComp<StationRecordsComponent>(station))
            return false;

        stationUid = station;
        record = _stationRecords.GetRecordsOfType<GeneralStationRecord>(station)
            .Select(entry => entry.Item2)
            .FirstOrDefault(entry => entry.ProfileId == profileId)!;
        return true;
    }
}
