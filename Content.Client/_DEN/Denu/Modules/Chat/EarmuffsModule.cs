using Content.Client._DEN.Denu.Core;
using Content.Client._DEN.Earmuffs;
using Content.Shared._DEN.Denu.Chat;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using UiControl = Robust.Client.UserInterface.Control;

namespace Content.Client._DEN.Denu.Modules.Chat;

public sealed class EarmuffsDenuModule : DenuExclusiveModule<EarmuffRangeSettings>
{
    private EarmuffsSystem? _earmuffsSystem;

    public override string Id => "earmuffs";

    public override string DisplayName => "Hearing Range";

    public override int TabOrder => 1;

    private EarmuffsTab? _tab;

    public float CurrentRange { get; private set; } = 10.0f;
    public float EditRange { get; private set; } = 10.0f;

    public event Action<float>? OnRangeChanged;

    private EarmuffsSystem? EarmuffsSystem
    {
        get
        {
            if (_earmuffsSystem == null)
            {
                try
                {
                    IEntitySystemManager? sysMan = IoCManager.Resolve<IEntitySystemManager>();
                    sysMan.TryGetEntitySystem(out _earmuffsSystem);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            }

            return _earmuffsSystem;
        }
    }

    protected override List<EarmuffRangeSettings> Items => Context.SettingsRoot.Modules.Chat.EarmuffRange.Items;

    protected override EarmuffRangeSettings CreateDefaultSettings()
    {
        return new EarmuffRangeSettings();
    }

    protected override void CopySettings(EarmuffRangeSettings source, EarmuffRangeSettings target)
    {
        target.Value = source.Value;
    }

    protected override void ApplyEffectiveSettings(EarmuffRangeSettings settings)
    {
        CurrentRange = settings.Value;
        EarmuffsSystem?.UpdateEarmuffs(CurrentRange);
        OnRangeChanged?.Invoke(CurrentRange);
    }

    protected override void OnEditSettingsUpdated(EarmuffRangeSettings settings)
    {
        EditRange = settings.Value;
        _tab?.RefreshFromSettings();
    }

    public override UiControl CreateTabContent()
    {
        _tab = new EarmuffsTab(this);
        return _tab;
    }

    public void SetRange(float range, bool persist)
    {
        if (IsEditingEffectiveItem())
        {
            CurrentRange = range;
            EarmuffsSystem?.UpdateEarmuffs(range);
            OnRangeChanged?.Invoke(range);
        }

        EditRange = range;

        if (!persist)
            return;

        if (!TryGetOrCreateEditSettings(out EarmuffRangeSettings settings))
            return;

        if (Math.Abs(settings.Value - range) <= 0.0001f)
            return;

        settings.Value = range;
        SaveNow();
    }
}
