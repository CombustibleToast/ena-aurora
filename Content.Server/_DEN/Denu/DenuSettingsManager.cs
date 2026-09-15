using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._DEN.Denu;
using Robust.Shared.IoC;
using Robust.Shared.Player;

namespace Content.Server._DEN.Denu;

public sealed partial class DenuSettingsManager : IDenuSettingsManager, IPostInjectInit
{
    [Dependency] private IServerDbManager _db = default!;
    [Dependency] private UserDbDataManager _userDb = default!;

    private readonly Dictionary<Guid, DenuSettingsRoot> _cache = new();

    public void PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(OnClientDisconnected);
    }

    public async Task<DenuSettingsRoot> LoadSettingsAsync(Guid userId)
    {
        if (_cache.TryGetValue(userId, out DenuSettingsRoot? cachedSettings))
            return cachedSettings;

        string json = await _db.LoadDenuSettingsJsonAsync(userId);

        try
        {
            DenuSettingsRoot? settings = JsonSerializer.Deserialize<DenuSettingsRoot>(json);
            DenuSettingsRoot result = settings ?? new DenuSettingsRoot();
            _cache[userId] = result;
            return result;
        }
        catch (JsonException)
        {
            DenuSettingsRoot result = new DenuSettingsRoot();
            _cache[userId] = result;
            return result;
        }
    }

    public async Task SaveSettingsAsync(Guid userId, DenuSettingsRoot settings)
    {
        string json = JsonSerializer.Serialize(settings);
        await _db.SaveDenuSettingsJsonAsync(userId, json);
        _cache[userId] = settings;
    }

    private async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        await LoadSettingsAsync(session.UserId.UserId);
        cancel.ThrowIfCancellationRequested();
    }

    private void OnClientDisconnected(ICommonSession session)
    {
        _cache.Remove(session.UserId.UserId);
    }
}
