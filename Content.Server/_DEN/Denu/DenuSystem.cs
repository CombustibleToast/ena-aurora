using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared._DEN.Denu;
using Content.Shared.Preferences;
using Robust.Shared.IoC;
using Robust.Shared.Player;

namespace Content.Server._DEN.Denu;

public sealed partial class DenuSystem : EntitySystem
{
    [Dependency] private IDenuSettingsManager _settingsManager = default!;
    [Dependency] private IServerPreferencesManager _prefsManager = default!;
    [Dependency] private IServerDbManager _db = default!;
    [Dependency] private UserDbDataManager _userDb = default!;

    public override void Initialize()
    {
        base.Initialize();

        _userDb.AddOnFinishLoad(OnUserDataLoaded);
        SubscribeNetworkEvent<DenuSettingsSaveRequest>(OnSaveRequest);
    }

    private async void OnUserDataLoaded(ICommonSession session)
    {
        try
        {
            Guid userId = session.UserId.UserId;
            DenuSettingsRoot settings = await _settingsManager.LoadSettingsAsync(userId);
            (int currentProfileId, HashSet<int> validProfileIds) = await GetProfileContext(userId);

            settings.EnsureValid(validProfileIds);

            DenuSettingsSyncEvent syncEvent = new DenuSettingsSyncEvent
            {
                Settings = settings,
                CurrentProfileId = currentProfileId
            };

            RaiseNetworkEvent(syncEvent, session);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load Denu settings for player {session.UserId}: {ex}");
        }
    }

    private async void OnSaveRequest(DenuSettingsSaveRequest ev, EntitySessionEventArgs args)
    {
        Guid userId = args.SenderSession.UserId.UserId;

        try
        {
            (int currentProfileId, HashSet<int> validProfileIds) = await GetProfileContext(userId);

            ev.Settings.EnsureValid(validProfileIds);
            await _settingsManager.SaveSettingsAsync(userId, ev.Settings);

            DenuSettingsSaveResponse response = new DenuSettingsSaveResponse
            {
                Success = true,
                Settings = ev.Settings,
                CurrentProfileId = currentProfileId
            };

            RaiseNetworkEvent(response, args.SenderSession);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to save Denu settings for user {userId}: {ex}");

            DenuSettingsSaveResponse response = new DenuSettingsSaveResponse
            {
                Success = false,
                ErrorMessage = "Failed to save settings"
            };

            RaiseNetworkEvent(response, args.SenderSession);
        }
    }

    private async Task<(int currentProfileId, HashSet<int> validProfileIds)> GetProfileContext(Guid userId)
    {
        Dictionary<int, int> slotToProfileId = await _db.GetProfileSlotToProfileIdMapAsync(userId);
        HashSet<int> validProfileIds = new(slotToProfileId.Values);

        if (!_prefsManager.TryGetCachedPreferences(new(userId), out PlayerPreferences? prefs))
        {
            Log.Warning($"Denu sync attempted for user {userId} but preferences not loaded yet");
            return (0, validProfileIds);
        }

        int selectedSlot = prefs.SelectedCharacterIndex;

        int currentProfileId = slotToProfileId.TryGetValue(selectedSlot, out int profId)
            ? profId
            : 0;

        return (currentProfileId, validProfileIds);
    }
}
