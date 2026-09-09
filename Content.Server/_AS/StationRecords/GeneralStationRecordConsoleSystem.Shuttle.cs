// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server._NF.ShuttleRecords;
using Content.Server.StationRecords.Components;
using Content.Shared.Interaction;
using Content.Shared.StationRecords;
using Robust.Shared.Timing;

namespace Content.Server.StationRecords.Systems;

public sealed partial class GeneralStationRecordConsoleSystem
{
    [Dependency] private ShuttleRecordsSystem _shuttleRecords = null!;
    [Dependency] private IGameTiming _timing = null!;
    private TimeSpan _nextShuttlePresenceRefresh;
    private readonly Dictionary<EntityUid, int> _crewStateHashes = new();

    private void InitializeShuttleRecords(BoundUserInterfaceRegisterExt.Subscriber<GeneralStationRecordConsoleComponent> subs)
    {
        SubscribeLocalEvent<GeneralStationRecordConsoleComponent, AfterInteractUsingEvent>(OnStationRecordsIdSwipe);
        subs.Event<BoundUIOpenedEvent>(OnShuttleRecordsUiOpened);
        subs.Event<BoundUIClosedEvent>(OnShuttleRecordsUiClosed);
        subs.Event<AddShuttleRecordNote>(OnAddShuttleNote);
        subs.Event<DeleteShuttleRecordNote>(OnDeleteShuttleNote);
        subs.Event<UpdateShuttleCrewRecord>(OnUpdateShuttleCrew);
        subs.Event<RestoreShuttleCrewRecord>(OnRestoreShuttleCrew);
        subs.Event<ArchiveShuttleCrewRecord>(OnArchiveShuttleCrew);
    }

    private void OnStationRecordsIdSwipe(Entity<GeneralStationRecordConsoleComponent> ent, ref AfterInteractUsingEvent args)
    {
        _shuttleRecords.HandleStationRecordsIdSwipe(ent.Owner, ref args);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_timing.CurTime < _nextShuttlePresenceRefresh)
            return;

        _nextShuttlePresenceRefresh = _timing.CurTime + TimeSpan.FromSeconds(5);
        var consoles = EntityQueryEnumerator<GeneralStationRecordConsoleComponent>();
        while (consoles.MoveNext(out var uid, out var console))
        {
            if (!_ui.IsUiOpen(uid, GeneralStationRecordConsoleKey.Key) ||
                !_shuttleRecords.HasConsoleShuttleRecord(uid))
            {
                _crewStateHashes.Remove(uid);
                continue;
            }

            _shuttleRecords.RefreshCrewRecords();
            if (!_shuttleRecords.TryGetCrewStateHash(uid, out var stateHash) ||
                (_crewStateHashes.TryGetValue(uid, out var previousHash) && previousHash == stateHash))
                continue;

            _crewStateHashes[uid] = stateHash;
            UpdateUserInterface((uid, console));
        }
    }

    private void OnShuttleRecordsUiOpened(Entity<GeneralStationRecordConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        _shuttleRecords.RefreshCrewRecords();
        _shuttleRecords.EnsureOwnerCrewRecord(ent.Owner, args.Actor); // Aurora's Song - Backfill owners created before crew records.
        if (_shuttleRecords.TryGetCrewStateHash(ent.Owner, out var stateHash))
            _crewStateHashes[ent.Owner] = stateHash;
        UpdateUserInterface(ent);
    }

    private void OnShuttleRecordsUiClosed(Entity<GeneralStationRecordConsoleComponent> ent, ref BoundUIClosedEvent args)
    {
        if (!_ui.IsUiOpen(ent.Owner, GeneralStationRecordConsoleKey.Key))
            _crewStateHashes.Remove(ent.Owner);
    }

    private void OnAddShuttleNote(Entity<GeneralStationRecordConsoleComponent> ent, ref AddShuttleRecordNote args)
    {
        if (_shuttleRecords.TryAddShuttleNote(ent.Owner, args.Actor, args.TargetProfileId, args.Title, args.Description))
            UpdateUserInterface(ent);
    }

    private void OnDeleteShuttleNote(Entity<GeneralStationRecordConsoleComponent> ent, ref DeleteShuttleRecordNote args)
    {
        if (_shuttleRecords.TryDeleteShuttleNote(ent.Owner, args.Actor, args.Id))
            UpdateUserInterface(ent);
    }

    private void OnUpdateShuttleCrew(Entity<GeneralStationRecordConsoleComponent> ent, ref UpdateShuttleCrewRecord args)
    {
        if (_shuttleRecords.TryUpdateCrewRecord(ent.Owner, args.Actor, args.StationRecordId, args.Minutes))
            UpdateUserInterface(ent);
    }

    private void OnRestoreShuttleCrew(Entity<GeneralStationRecordConsoleComponent> ent, ref RestoreShuttleCrewRecord args)
    {
        if (_shuttleRecords.TryRestoreCrewAsOwner(ent.Owner, args.Actor, args.StationRecordId))
            UpdateUserInterface(ent);
    }

    private void OnArchiveShuttleCrew(Entity<GeneralStationRecordConsoleComponent> ent, ref ArchiveShuttleCrewRecord args)
    {
        if (_shuttleRecords.TryRemoveCrewAsOwner(ent.Owner, args.Actor, args.StationRecordId, args.Reason))
            UpdateUserInterface(ent);
    }

    private void SetStationRecordsUiState(EntityUid uid, GeneralStationRecordConsoleState state)
    {
        state.ShuttleNotes = _shuttleRecords.GetShuttleNoteViews(uid, null);
        state.ShuttleCrew = _shuttleRecords.GetShuttleCrewViews(uid, false);
        state.ShuttleOwnerStationRecordId = state.ShuttleCrew?.FirstOrDefault(member => member.IsShuttleOwner)?.StationRecordId;
        state.CanCreateShuttleNotes = state.ShuttleCrew != null;
        _ui.SetUiState(uid, GeneralStationRecordConsoleKey.Key, state);

        foreach (var actor in _ui.GetActors(uid, GeneralStationRecordConsoleKey.Key))
        {
            var profileId = _shuttleRecords.GetUserProfileId(actor);
            var canManage = _shuttleRecords.IsRegisteredShuttleOwner(uid, actor);
            var deletableNotes = _shuttleRecords.GetDeletableShuttleNoteIds(uid, profileId);
            _ui.ServerSendUiMessage(uid,
                GeneralStationRecordConsoleKey.Key,
                new ShuttleRecordViewerPermissions(canManage, deletableNotes),
                actor);
        }
    }
}
