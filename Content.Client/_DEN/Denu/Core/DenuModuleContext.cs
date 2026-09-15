using Content.Shared._DEN.Denu;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using System.Threading;
using RTimer = Robust.Shared.Timing.Timer;

namespace Content.Client._DEN.Denu.Core;

public sealed partial class DenuModuleContext
{
    [Dependency] private IEntityManager _entityManager = default!;

    private DenuSettingsRoot? _settingsRoot;
    private int _currentProfileId;
    private readonly Dictionary<string, CancellationTokenSource> _debouncedSaves = new();

    public DenuSettingsRoot SettingsRoot => _settingsRoot ??= new DenuSettingsRoot();

    public int CurrentProfileId => _currentProfileId;

    public bool IsEditingGlobal { get; private set; } = true;

    public event Action? OnDataChanged;
    public event Action? OnEditStateChanged;

    public DenuModuleContext()
    {
        IoCManager.InjectDependencies(this);
    }

    public void SetScope(bool global)
    {
        if (IsEditingGlobal == global)
            return;

        IsEditingGlobal = global;
        OnEditStateChanged?.Invoke();
    }

    public void ApplySync(DenuSettingsRoot settingsRoot, int currentProfileId)
    {
        bool profileChanged = _currentProfileId != currentProfileId;

        _settingsRoot = settingsRoot;
        _currentProfileId = currentProfileId;

        OnDataChanged?.Invoke();

        if (profileChanged || _firstSyncPending)
        {
            _firstSyncPending = false;
            OnEditStateChanged?.Invoke();
        }
    }

    public void ApplySaveEcho(DenuSettingsRoot settingsRoot, int currentProfileId)
    {
        _settingsRoot = settingsRoot;
        _currentProfileId = currentProfileId;
        OnDataChanged?.Invoke();
    }

    public void SaveNow()
    {
        DenuSettingsSaveRequest request = new DenuSettingsSaveRequest
        {
            Settings = SettingsRoot
        };

        _entityManager.EntityNetManager.SendSystemNetworkMessage(request);
    }

    public void SaveDebounced(string key, TimeSpan delay)
    {
        if (_debouncedSaves.TryGetValue(key, out CancellationTokenSource? existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        CancellationTokenSource cts = new();
        _debouncedSaves[key] = cts;

        RTimer.Spawn(delay, () =>
        {
            if (cts.IsCancellationRequested)
                return;

            if (_debouncedSaves.TryGetValue(key, out CancellationTokenSource? current) && ReferenceEquals(current, cts))
                _debouncedSaves.Remove(key);

            SaveNow();
            cts.Dispose();
        }, cts.Token);
    }

    private bool _firstSyncPending = true;
}
