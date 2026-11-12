// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._AS.Shuttles;
using Content.Server.Access.Systems;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared._NF.ShuttleRecords;
using Content.Shared.Access.Components;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Medical.SuitSensors;
using Content.Shared.SSDIndicator;
using Content.Shared.StationRecords;
using Robust.Shared.Audio;

// Intentional to extend its partial type.
namespace Content.Server._NF.ShuttleRecords;

public sealed partial class ShuttleRecordsSystem
{
    private const int MaxNotesPerCrewmember = 10;
    private const int MaxNoteDescriptionLength = 1000;

    [Dependency] private StationSystem _station = null!;
    [Dependency] private StationRecordsSystem _stationRecords = null!;
    [Dependency] private IdCardSystem _idCard = null!;

    private TimeSpan _nextCrewExpirationCheck;
    private TimeSpan _nextCrewPresenceRefresh;
    private readonly Dictionary<EntityUid, int> _cryoBodies = new();
    private readonly HashSet<int> _cryoProfiles = [];
    private readonly Dictionary<int, ShuttleCrewPresence> _crewPresence = new();

    private static readonly SoundSpecifier CrewRecordsConfirmSound = new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg");
    private static readonly SoundSpecifier CrewRecordsErrorSound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");

    private void InitializeASCrewRecords()
    {
        SubscribeLocalEvent<ASCrewEnteredCryoEvent>(OnCrewEnteredCryo);
        SubscribeLocalEvent<ASCrewLeftCryoEvent>(OnCrewLeftCryo);
    }

    private void OnCrewEnteredCryo(ASCrewEnteredCryoEvent args)
    {
        HandleCrewEnteredCryo(args.Body);
    }

    private void OnCrewLeftCryo(ASCrewLeftCryoEvent args)
    {
        HandleCrewLeftCryo(args.Body);
    }

    // TODO: As of current this is only called when someone has the console open since there's no gameplay reason to check for expired crew frequently.
    // This may change should there be a gameplay reason to check for expired crew more frequently, such as access controls.
    /// <summary>
    /// Processes expired and missing shuttle crew records at most once per cleanup interval.
    /// This is called while a shuttle records console is being viewed.
    /// </summary>
    public void RefreshCrewRecords()
    {
        if (_gameTiming.CurTime < _nextCrewExpirationCheck)
            return;

        _nextCrewExpirationCheck = _gameTiming.CurTime + TimeSpan.FromSeconds(15);
        ExpireTemporaryCrew();
    }

    /// Handles ID registration toggling for owners and registration or self-removal for crewmembers.
    public void HandleStationRecordsIdSwipe(EntityUid consoleUid, ref AfterInteractUsingEvent args)
    {
        var idUid = args.Used;
        if (args.Handled || !args.CanReach)
            return;

        // Shuttle crew registration only applies when this console's grid has a shuttle record.
        if (!TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord))
            return;

        var hasKeyStorage = TryComp(idUid, out StationRecordKeyStorageComponent? keyStorage);
        var hasIdCard = TryComp(idUid, out IdCardComponent? idCard);
        if (!hasKeyStorage && !hasIdCard)
            return;

        args.Handled = true;
        if (!hasKeyStorage || !hasIdCard || keyStorage == null || idCard is not { ProfileId: { } profileId } || keyStorage.Key is not { } sourceKey)
        {
            _popup.PopupEntity(Loc.GetString("general-station-record-console-shuttle-crew-invalid-id"), consoleUid, args.User);
            PlayCrewRecordsSound(consoleUid, false);
            return;
        }

        var owningStation = _station.GetOwningStation(consoleUid);
        if (owningStation is not { } stationUid)
        {
            _popup.PopupEntity(Loc.GetString("general-station-record-console-shuttle-crew-no-station"), consoleUid, args.User);
            PlayCrewRecordsSound(consoleUid, false);
            return;
        }

        if (string.Equals(idCard.FullName, shuttleRecord.OwnerName, StringComparison.OrdinalIgnoreCase) &&
            shuttleRecord.OwnerProfileId != profileId)
        {
            shuttleRecord.OwnerProfileId = profileId;
            TryUpdateRecord(shuttleRecord);
        }

        var isOwner = shuttleRecord.OwnerProfileId == profileId;
        if (isOwner)
        {
            EnsureOwnerCrewRecord(shuttleRecord, profileId, sourceKey, stationUid, idCard.FullName);
            shuttleRecord.CrewRegistrationEnabled = !shuttleRecord.CrewRegistrationEnabled;
            TryUpdateRecord(shuttleRecord);
            _popup.PopupEntity(Loc.GetString(shuttleRecord.CrewRegistrationEnabled
                ? "general-station-record-console-shuttle-crew-registration-enabled"
                : "general-station-record-console-shuttle-crew-registration-disabled-owner"),
                consoleUid,
                args.User);
            PlayCrewRecordsSound(consoleUid, true);
            return;
        }

        var existingMember = shuttleRecord.Crew.FirstOrDefault(member => member.ProfileId == profileId);
        if (existingMember != null)
        {
            if (existingMember.Archived)
            {
                if (!shuttleRecord.CrewRegistrationEnabled ||
                    !_stationRecords.TryGetRecord<GeneralStationRecord>(sourceKey, out var restoredSource))
                {
                    _popup.PopupEntity(Loc.GetString("general-station-record-console-shuttle-crew-registration-disabled"), consoleUid, args.User);
                    PlayCrewRecordsSound(consoleUid, false);
                    return;
                }

                RestoreCrewStationRecord(shuttleRecord, existingMember, restoredSource, stationUid, consoleUid, args.User);
                return;
            }

            ArchiveCrewStationRecord(shuttleRecord, existingMember, stationUid, consoleUid, args.User, ShuttleCrewArchiveReason.SelfRemoved);
            return;
        }

        if (!shuttleRecord.CrewRegistrationEnabled)
        {
            _popup.PopupEntity(Loc.GetString("general-station-record-console-shuttle-crew-registration-disabled"), consoleUid, args.User);
            PlayCrewRecordsSound(consoleUid, false);
            return;
        }

        if (!_stationRecords.TryGetRecord<GeneralStationRecord>(sourceKey, out var sourceRecord))
        {
            _popup.PopupEntity(Loc.GetString("general-station-record-console-shuttle-crew-invalid-id"), consoleUid, args.User);
            PlayCrewRecordsSound(consoleUid, false);
            return;
        }

        var newKey = _stationRecords.AddRecordEntry(stationUid, sourceRecord with { });
        if (!newKey.IsValid())
        {
            PlayCrewRecordsSound(consoleUid, false);
            return;
        }

        shuttleRecord.Crew.Add(new ShuttleCrewRecord(profileId, sourceRecord.Name, newKey.Id)
        {
            RegisteredAt = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan),
            Record = sourceRecord with { },
        });
        TryUpdateRecord(shuttleRecord);
        _stationRecords.Synchronize(newKey);
        _popup.PopupEntity(Loc.GetString("general-station-record-console-shuttle-crew-added", ("name", sourceRecord.Name)), consoleUid, args.User);
        PlayCrewRecordsSound(consoleUid, true);
        _adminLogger.Add(LogType.ShuttleRecordsUsage, LogImpact.Low, $"{ToPrettyString(args.User):actor} added {sourceRecord.Name} to shuttle station records for {shuttleRecord.Name} {shuttleRecord.Suffix}.");
    }

    /// Returns whether the user's current ID profile owns the shuttle associated with the console.
    public bool IsRegisteredShuttleOwner(EntityUid consoleUid, EntityUid user)
    {
        return TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) && _idCard.TryFindIdCard(user, out var idCard) && idCard.Comp.ProfileId is { } profileId && shuttleRecord.OwnerProfileId == profileId;
    }

    /// Returns whether a station record is the protected owner record for the console's shuttle.
    public bool IsShuttleOwnerStationRecord(EntityUid consoleUid, uint stationRecordId)
    {
        return TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) &&
               shuttleRecord.Crew.Any(member => member.ProfileId == shuttleRecord.OwnerProfileId && member.StationRecordId == stationRecordId);
    }

    /// <summary>
    /// Ensures an older or incomplete shuttle record contains a valid active record for its owner.
    /// <remarks>
    /// Because the owner is apparently in a quantum fucking state of being a person and being some secret other thing.
    /// </remarks>
    /// </summary>
    public void EnsureOwnerCrewRecord(EntityUid consoleUid, EntityUid user)
    {
        if (!TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) ||
            !_idCard.TryFindIdCard(user, out var idCard) ||
            idCard.Comp.ProfileId is not { } profileId ||
            !string.Equals(idCard.Comp.FullName, shuttleRecord.OwnerName, StringComparison.OrdinalIgnoreCase) ||
            !TryComp<StationRecordKeyStorageComponent>(idCard.Owner, out var keyStorage) ||
            keyStorage.Key is not { } sourceKey ||
            _station.GetOwningStation(consoleUid) is not { } stationUid)
            return;

        var existingOwner = shuttleRecord.Crew.FirstOrDefault(member => member.ProfileId == profileId);
        if (shuttleRecord.OwnerProfileId == profileId && existingOwner is { Archived: false } && _stationRecords.TryGetRecord<GeneralStationRecord>(new StationRecordKey(existingOwner.StationRecordId, stationUid), out _))
            return;

        var changed = shuttleRecord.OwnerProfileId != profileId;
        shuttleRecord.OwnerProfileId = profileId;
        if (EnsureOwnerCrewRecord(shuttleRecord, profileId, sourceKey, stationUid, idCard.Comp.FullName) || changed)
            TryUpdateRecord(shuttleRecord);
    }

    // ᓚᘏᗢ genuinely half this fucking code is dedicated entirely to making sure the owner is counted as an actual fucking person and not a quantum ghost.
    private bool EnsureOwnerCrewRecord(ShuttleRecord shuttleRecord, int profileId, StationRecordKey sourceKey, EntityUid stationUid, string? ownerName)
    {
        var existing = shuttleRecord.Crew.FirstOrDefault(member => member.ProfileId == profileId);
        GeneralStationRecord? sourceRecord = null;
        var stationRecordId = GetStationRecordIdByProfile(stationUid, profileId);
        if (stationRecordId is { } currentId)
            _stationRecords.TryGetRecord(new StationRecordKey(currentId, stationUid), out sourceRecord);

        if (sourceRecord == null)
            _stationRecords.TryGetRecord(sourceKey, out sourceRecord);
        sourceRecord ??= existing?.Record;
        if (sourceRecord == null)
            return false;

        if (stationRecordId == null)
        {
            var newKey = _stationRecords.AddRecordEntry(stationUid, sourceRecord with { });
            if (!newKey.IsValid())
                return false;
            stationRecordId = newKey.Id;
            _stationRecords.Synchronize(newKey);
        }

        if (existing != null)
        {
            var changed = existing.StationRecordId != stationRecordId.Value ||
                          existing.Archived ||
                          existing.Name != (ownerName ?? sourceRecord.Name);
            existing.StationRecordId = stationRecordId.Value;
            existing.Name = ownerName ?? sourceRecord.Name;
            if (existing.Archived)
                existing.RegisteredAt = CurrentRoundTime();
            existing.ExpiresAt = null;
            existing.ArchivedAt = null;
            existing.ArchiveReason = null;
            existing.ArchivedBy = null;
            existing.ArchiveDetails = null;
            existing.OwnerRestoreBlocked = false;
            existing.Record = sourceRecord with { };
            return changed;
        }

        shuttleRecord.Crew.Add(new ShuttleCrewRecord(profileId, ownerName ?? sourceRecord.Name, stationRecordId.Value)
        {
            RegisteredAt = CurrentRoundTime(),
            Record = sourceRecord with { },
        });
        return true;
    }

    private uint? GetStationRecordIdByProfile(EntityUid stationUid, int profileId)
    {
        foreach (var (recordId, record) in _stationRecords.GetRecordsOfType<GeneralStationRecord>(stationUid))
        {
            if (record.ProfileId == profileId)
                return recordId;
        }

        return null;
    }

    /// Archives a non-owner crewmember after validating the shuttle owner and supplied reason.
    public bool TryRemoveCrewAsOwner(EntityUid consoleUid, EntityUid user, uint stationRecordId, string reason)
    {
        reason = reason.Trim();
        if (!IsRegisteredShuttleOwner(consoleUid, user) ||
            reason.Length is < 1 or > 200 ||
            !TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) ||
            shuttleRecord.Crew.FirstOrDefault(record => record.StationRecordId == stationRecordId) is not { } member ||
            member.ProfileId == shuttleRecord.OwnerProfileId ||
            _station.GetOwningStation(consoleUid) is not { } stationUid)
            return false;

        ArchiveCrewStationRecord(shuttleRecord, member, stationUid, consoleUid, user, ShuttleCrewArchiveReason.OwnerRemoved, reason);
        return true;
    }

    /// Builds the crew data sent to the records UI. Saved station records are included only for archived entries.
    public List<ShuttleCrewRecordView>? GetShuttleCrewViews(EntityUid consoleUid, bool canManage)
    {
        if (!TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord))
            return null;

        return shuttleRecord.Crew.Select(member => new ShuttleCrewRecordView
            {
                ProfileId = member.ProfileId,
                StationRecordId = member.StationRecordId,
                Name = member.Name,
                RegisteredAt = member.RegisteredAt,
                ExpiresAt = member.ExpiresAt,
                ArchivedAt = member.ArchivedAt,
                ArchiveReason = member.ArchiveReason,
                ArchivedBy = member.ArchivedBy,
                ArchiveDetails = member.ArchiveDetails,
                OwnerRestoreBlocked = member.OwnerRestoreBlocked,
                Presence = member.Archived ? ShuttleCrewPresence.wtf : GetCrewPresence(member.ProfileId),
                CanManage = canManage,
                IsShuttleOwner = shuttleRecord.OwnerProfileId == member.ProfileId,
                Record = member.Archived ? member.Record : null,
            })
            .ToList();
    }

    /// Checks whether the console is located on a shuttle with an associated shuttle record.
    public bool HasConsoleShuttleRecord(EntityUid consoleUid)
    {
        return TryGetConsoleShuttleRecord(consoleUid, out _);
    }

    /// <summary>
    /// Creates a hash of shuttle crew data that affects the records UI, avoiding unchanged UI state rebuilds.
    /// </summary>
    /// <returns>True if the console has an associated shuttle record; otherwise false.</returns>
    public bool TryGetCrewStateHash(EntityUid consoleUid, out int stateHash)
    {
        stateHash = 0;
        if (!TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord))
            return false;

        var hash = new HashCode();
        hash.Add(shuttleRecord.OwnerProfileId);
        hash.Add(shuttleRecord.CrewRegistrationEnabled);
        foreach (var member in shuttleRecord.Crew)
        {
            hash.Add(member.ProfileId);
            hash.Add(member.StationRecordId);
            hash.Add(member.ExpiresAt);
            hash.Add(member.ArchiveReason);
            hash.Add(member.OwnerRestoreBlocked);
            if (!member.Archived)
                hash.Add(GetCrewPresence(member.ProfileId));
        }

        stateHash = hash.ToHashCode();
        return true;
    }

    /// <summary>
    /// Updates a non-owner crewmember's registration duration after validating the shuttle owner.
    /// Zero minutes represents a permanent registration for the round.
    /// </summary>
    public bool TryUpdateCrewRecord(EntityUid consoleUid, EntityUid user, uint stationRecordId, int minutes)
    {
        if (!IsRegisteredShuttleOwner(consoleUid, user) ||
            !TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) ||
            shuttleRecord.Crew.FirstOrDefault(record => record.StationRecordId == stationRecordId) is not { } member ||
            member.ProfileId == shuttleRecord.OwnerProfileId)
            return false;

        minutes = Math.Clamp(minutes, 0, 1440);
        member.ExpiresAt = minutes == 0 ? null : CurrentRoundTime() + TimeSpan.FromMinutes(minutes);
        TryUpdateRecord(shuttleRecord);
        PlayCrewRecordsSound(consoleUid, true);
        _adminLogger.Add(LogType.ShuttleRecordsUsage, LogImpact.Low, $"{ToPrettyString(user):actor} updated {member.Name}'s shuttle registration duration for {shuttleRecord.Name} {shuttleRecord.Suffix}.");
        return true;
    }

    /// Restores an archived record when the owner is permitted to reinstate it.
    public bool TryRestoreCrewAsOwner(EntityUid consoleUid, EntityUid user, uint stationRecordId)
    {
        if (!IsRegisteredShuttleOwner(consoleUid, user) || !TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) || shuttleRecord.Crew.FirstOrDefault(x => x.StationRecordId == stationRecordId) is not { Archived: true, OwnerRestoreBlocked: false, Record: { } record } member || _station.GetOwningStation(consoleUid) is not { } stationUid)
            return false;

        RestoreCrewStationRecord(shuttleRecord, member, record, stationUid, consoleUid, user);
        return true;
    }

    private ShuttleCrewPresence GetCrewPresence(int profileId)
    {
        if (_cryoProfiles.Contains(profileId))
            return ShuttleCrewPresence.Cryo; // Cryo takes precedence over the body's disconnected state.

        RefreshCrewPresenceCache();
        return _crewPresence.GetValueOrDefault(profileId, ShuttleCrewPresence.wtf);
    }

    /// Builds one shared profile index so presence lookup does not scan entities per crewmember.
    private void RefreshCrewPresenceCache(bool force = false)
    {
        if (!force && _gameTiming.CurTime < _nextCrewPresenceRefresh)
            return;

        _nextCrewPresenceRefresh = _gameTiming.CurTime + TimeSpan.FromSeconds(5);
        _crewPresence.Clear();

        var players = EntityQueryEnumerator<SSDIndicatorComponent>();
        while (players.MoveNext(out var playerUid, out var indicator))
        {
            if (!_idCard.TryFindIdCard(playerUid, out var card) || card.Comp.ProfileId is not { } profileId)
                continue;

            _crewPresence.TryAdd(profileId, indicator.IsSSD ? ShuttleCrewPresence.SSD : ShuttleCrewPresence.Active);
        }

        var sensors = EntityQueryEnumerator<SuitSensorComponent>();
        while (sensors.MoveNext(out _, out var sensor))
        {
            if (sensor.User is not { } user || !_idCard.TryFindIdCard(user, out var card) || card.Comp.ProfileId is not { } profileId)
                continue;

            _crewPresence.TryAdd(profileId, TryComp<SSDIndicatorComponent>(user, out var ssd) && ssd.IsSSD
                ? ShuttleCrewPresence.SSD
                : ShuttleCrewPresence.Active);
        }
    }

    /// Marks the profile associated with a character body as being in cryosleep.
    public void HandleCrewEnteredCryo(EntityUid body)
    {
        if (!_idCard.TryFindIdCard(body, out var card) || card.Comp.ProfileId is not { } profileId)
            return;

        _cryoBodies[body] = profileId;
        _cryoProfiles.Add(profileId);
    }

    /// Clears the cryosleep state previously associated with a character body.
    public void HandleCrewLeftCryo(EntityUid body)
    {
        if (!_cryoBodies.Remove(body, out var profileId))
            return;

        _cryoProfiles.Remove(profileId);
    }

    private TimeSpan CurrentRoundTime() => _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);

    /// Gets the stable profile ID from the ID card currently associated with a user.
    public int? GetUserProfileId(EntityUid user)
    {
        return _idCard.TryFindIdCard(user, out var idCard) ? idCard.Comp.ProfileId : null;
    }

    /// Builds note views for a shuttle and marks entries the supplied profile is permitted to delete.
    public List<ShuttleRecordNoteView> GetShuttleNoteViews(EntityUid consoleUid, int? userProfileId)
    {
        if (!TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord))
            return [];

        var isOwner = userProfileId != null && shuttleRecord.OwnerProfileId == userProfileId;
        var views = shuttleRecord.Notes.Select(note => new ShuttleRecordNoteView(note.Id, note.TargetProfileId, note.Author, note.Title, note.Description, note.CreatedAt, isOwner || userProfileId == note.AuthorProfileId)).ToList();
        return views;
    }

    /// Gets note IDs deletable by a viewer.
    public HashSet<int> GetDeletableShuttleNoteIds(EntityUid consoleUid, int? userProfileId)
    {
        if (userProfileId == null || !TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord))
            return [];

        var isOwner = shuttleRecord.OwnerProfileId == userProfileId;
        return shuttleRecord.Notes
            .Where(note => isOwner || note.AuthorProfileId == userProfileId)
            .Select(note => note.Id)
            .ToHashSet();
    }

    /// Adds a validated note to an active or archived crewmember record.
    public bool TryAddShuttleNote(EntityUid consoleUid, EntityUid user, int targetProfileId, string title, string description)
    {
        title = title.Trim();
        description = description.Trim();
        if (title.Length is < 1 or > 100 ||
            description.Length is < 1 or > MaxNoteDescriptionLength ||
            !TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) ||
            shuttleRecord.Crew.All(member => member.ProfileId != targetProfileId) ||
            shuttleRecord.Notes.Count(note => note.TargetProfileId == targetProfileId) >= MaxNotesPerCrewmember)
            return false;

        var profileId = GetUserProfileId(user);
        var author = _idCard.TryFindIdCard(user, out var idCard) ? idCard.Comp.FullName ?? Name(user) : Name(user);
        shuttleRecord.Notes.Add(new ShuttleRecordNote(shuttleRecord.NextNoteId++, targetProfileId, profileId, author, title, description, CurrentRoundTime()));
        TryUpdateRecord(shuttleRecord);
        _adminLogger.Add(LogType.ShuttleRecordsUsage, LogImpact.Low, $"{ToPrettyString(user):actor} added note '{title}' to profile {targetProfileId} in shuttle records for {shuttleRecord.Name} {shuttleRecord.Suffix}.");
        PlayCrewRecordsSound(consoleUid, true);
        return true;
    }

    /// Deletes a note after verifying the user is its author or the shuttle owner.
    public bool TryDeleteShuttleNote(EntityUid consoleUid, EntityUid user, int noteId)
    {
        if (GetUserProfileId(user) is not { } profileId || !TryGetConsoleShuttleRecord(consoleUid, out var shuttleRecord) || shuttleRecord.Notes.FirstOrDefault(recordNote => recordNote.Id == noteId) is not { } note || profileId != note.AuthorProfileId && profileId != shuttleRecord.OwnerProfileId)
            return false;

        shuttleRecord.Notes.Remove(note);
        TryUpdateRecord(shuttleRecord);
        _adminLogger.Add(LogType.ShuttleRecordsUsage, LogImpact.Low, $"{ToPrettyString(user):actor} deleted a note by {note.Author} from shuttle records for {shuttleRecord.Name} {shuttleRecord.Suffix}.");
        PlayCrewRecordsSound(consoleUid, true);
        return true;
    }

    private bool TryGetConsoleShuttleRecord(EntityUid consoleUid, [NotNullWhen(true)] out ShuttleRecord? shuttleRecord)
    {
        shuttleRecord = null;
        if (Transform(consoleUid).GridUid is not { } shuttleGrid || !TryGetShuttleRecordsDataComponent(out var records))
            return false;

        var shuttleNet = GetNetEntity(shuttleGrid);
        shuttleRecord = records.ShuttleRecords.Values.FirstOrDefault(record => record.EntityUid == shuttleNet);
        return shuttleRecord != null;
    }

    private void ArchiveCrewStationRecord(ShuttleRecord shuttleRecord, ShuttleCrewRecord member, EntityUid stationUid, EntityUid consoleUid, EntityUid user, ShuttleCrewArchiveReason reason, string? details = null)
    {
        if (_stationRecords.TryGetRecord<GeneralStationRecord>(new StationRecordKey(member.StationRecordId, stationUid), out var record))
            member.Record = record with { };
        _stationRecords.RemoveRecord(new StationRecordKey(member.StationRecordId, stationUid));
        member.ArchiveReason = reason;
        member.ArchivedAt = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
        member.ArchivedBy = Name(user);
        member.ArchiveDetails = details;
        member.OwnerRestoreBlocked = reason == ShuttleCrewArchiveReason.SelfRemoved;
        TryUpdateRecord(shuttleRecord);
        _popup.PopupEntity(Loc.GetString("general-station-record-console-shuttle-crew-removed", ("name", member.Name)), consoleUid, user);
        PlayCrewRecordsSound(consoleUid, true);
        _adminLogger.Add(LogType.ShuttleRecordsUsage, LogImpact.Low, $"{ToPrettyString(user):actor} removed {member.Name} from shuttle station records for {shuttleRecord.Name} {shuttleRecord.Suffix}. Reason: {details ?? reason.ToString()}.");
    }

    private void RestoreCrewStationRecord(ShuttleRecord shuttleRecord, ShuttleCrewRecord member, GeneralStationRecord source, EntityUid stationUid, EntityUid consoleUid, EntityUid user)
    {
        var newKey = _stationRecords.AddRecordEntry(stationUid, source with { });
        if (!newKey.IsValid())
            return;
        member.StationRecordId = newKey.Id;
        member.Record = source with { };
        member.RegisteredAt = CurrentRoundTime();
        member.ExpiresAt = null;
        member.ArchivedAt = null;
        member.ArchiveReason = null;
        member.ArchivedBy = null;
        member.ArchiveDetails = null;
        member.OwnerRestoreBlocked = false;
        TryUpdateRecord(shuttleRecord);
        _stationRecords.Synchronize(newKey);
        PlayCrewRecordsSound(consoleUid, true);
        _adminLogger.Add(LogType.ShuttleRecordsUsage, LogImpact.Low, $"{ToPrettyString(user):actor} restored {member.Name} to shuttle station records for {shuttleRecord.Name} {shuttleRecord.Suffix}.");
    }

    /// Archives expired registrations and non-owner profiles whose character entities can no longer be found.
    private void ExpireTemporaryCrew()
    {
        // Make sure the world fucking exists before we do literally anything or you will get fourteen gabzillion dillion null references
        if (!_sectorService.GetServiceEntity().Valid)
            return;

        foreach (var (body, profileId) in _cryoBodies.Where(entry => Deleted(entry.Key)).ToList())
        {
            _cryoBodies.Remove(body);
            if (_cryoBodies.All(entry => entry.Value != profileId))
                _cryoProfiles.Remove(profileId);
        }

        if (!TryGetShuttleRecordsDataComponent(out var records))
            return;
        RefreshCrewPresenceCache(true);
        var now = CurrentRoundTime();
        foreach (var shuttleRecord in records.ShuttleRecords.Values)
        {
            if (!TryGetEntity(shuttleRecord.EntityUid, out var shuttleUid) ||
                _station.GetOwningStation(shuttleUid) is not { } stationUid)
                continue;
            var changed = false;
            foreach (var member in shuttleRecord.Crew.Where(x => x.ProfileId != shuttleRecord.OwnerProfileId && !x.Archived && x.ExpiresAt is { } expiry && expiry <= now))
            {
                _stationRecords.RemoveRecord(new StationRecordKey(member.StationRecordId, stationUid));
                member.ArchiveReason = ShuttleCrewArchiveReason.Expired;
                member.ArchivedAt = now;
                member.ArchivedBy = Loc.GetString("general-station-record-console-archive-automatic-expiration");
                member.OwnerRestoreBlocked = false;
                changed = true;
            }

            foreach (var member in shuttleRecord.Crew.Where(member => member.ProfileId != shuttleRecord.OwnerProfileId && !member.Archived && GetCrewPresence(member.ProfileId) == ShuttleCrewPresence.wtf))
            {
                // An active profile that no longer resolves to a character is assumed deleted.
                if (_stationRecords.TryGetRecord<GeneralStationRecord>(new StationRecordKey(member.StationRecordId, stationUid), out var record))
                    member.Record = record with { };
                _stationRecords.RemoveRecord(new StationRecordKey(member.StationRecordId, stationUid));
                member.ArchiveReason = ShuttleCrewArchiveReason.CharacterDeleted;
                member.ArchivedAt = now;
                member.ArchivedBy = Loc.GetString("general-station-record-console-archive-system");
                member.ArchiveDetails = Loc.GetString("general-station-record-console-archive-character-not-found");
                member.OwnerRestoreBlocked = true;
                changed = true;
                _adminLogger.Add(LogType.ShuttleRecordsUsage, LogImpact.Low, $"Automatically archived {member.Name} from shuttle station records for {shuttleRecord.Name} {shuttleRecord.Suffix} because their character entity could not be found.");
            }
            if (changed)
                TryUpdateRecord(shuttleRecord);
        }
    }

    private void PlayCrewRecordsSound(EntityUid consoleUid, bool success)
    {
        _audioSystem.PlayPvs(success ? CrewRecordsConfirmSound : CrewRecordsErrorSound, consoleUid, AudioParams.Default.WithMaxDistance(8f));
    }
}
