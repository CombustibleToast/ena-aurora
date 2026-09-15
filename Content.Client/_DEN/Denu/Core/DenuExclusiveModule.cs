using Content.Shared._DEN.Denu;

namespace Content.Client._DEN.Denu.Core;

public abstract class DenuExclusiveModule<TSettings> : DenuModuleBase
    where TSettings : class, IScopedItem
{
    protected TSettings? EffectiveSettingsRef { get; private set; }
    protected TSettings? EditSettingsRef { get; private set; }

    protected abstract List<TSettings> Items { get; }
    protected abstract TSettings CreateDefaultSettings();
    protected abstract void ApplyEffectiveSettings(TSettings settings);
    protected abstract void OnEditSettingsUpdated(TSettings settings);

    protected virtual void CopySettings(TSettings source, TSettings target)
    {
    }

    protected override void OnContextDataChanged()
    {
        EffectiveSettingsRef = ScopeResolver.ResolveExclusive(Items, Context.CurrentProfileId);
        TSettings effective = EffectiveSettingsRef ?? CreateDefaultSettings();
        ApplyEffectiveSettings(effective);
    }

    protected override void OnContextEditStateChanged()
    {
        EditSettingsRef = ResolveEditSettingsRef();
        TSettings edit = EditSettingsRef ?? CreateDefaultSettings();
        OnEditSettingsUpdated(edit);
    }

    protected bool IsEditingEffectiveItem()
    {
        return EditSettingsRef != null && ReferenceEquals(EditSettingsRef, EffectiveSettingsRef);
    }

    protected bool TryGetOrCreateEditSettings(out TSettings settings)
    {
        if (Context.IsEditingGlobal)
        {
            settings = ScopeResolver.GetOrCreateGlobal(Items, CreateDefaultSettings);
            EditSettingsRef = settings;
            return true;
        }

        if (Context.CurrentProfileId <= 0)
        {
            settings = null!;
            return false;
        }

        TSettings? existing = ScopeResolver.FindExact(Items, Context.CurrentProfileId);
        if (existing != null)
        {
            settings = existing;
            EditSettingsRef = settings;
            return true;
        }

        settings = CreateDefaultSettings();
        TSettings? source = EditSettingsRef ?? EffectiveSettingsRef ?? ScopeResolver.FindGlobal(Items);
        if (source != null)
            CopySettings(source, settings);

        settings.ProfileIds.Clear();
        settings.ProfileIds.Add(Context.CurrentProfileId);
        Items.Add(settings);

        EditSettingsRef = settings;
        EffectiveSettingsRef = settings;
        return true;
    }

    private TSettings? ResolveEditSettingsRef()
    {
        if (Context.IsEditingGlobal)
            return ScopeResolver.FindGlobal(Items);

        if (Context.CurrentProfileId <= 0)
            return null;

        return ScopeResolver.FindExact(Items, Context.CurrentProfileId) ?? ScopeResolver.FindGlobal(Items);
    }
}
