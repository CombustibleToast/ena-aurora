using UiControl = Robust.Client.UserInterface.Control;

namespace Content.Client._DEN.Denu.Core;

public abstract class DenuModuleBase : IDenuModule
{
    protected DenuModuleContext Context { get; private set; } = default!;

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public abstract int TabOrder { get; }

    public void Initialize(DenuModuleContext context)
    {
        Context = context;
        Context.OnDataChanged += OnContextDataChanged;
        Context.OnEditStateChanged += OnContextEditStateChanged;
        OnInitialized();
    }

    protected virtual void OnInitialized()
    {
    }

    protected abstract void OnContextDataChanged();

    protected abstract void OnContextEditStateChanged();

    protected void SaveNow()
    {
        Context.SaveNow();
    }

    protected void SaveDebounced(string key, TimeSpan delay)
    {
        Context.SaveDebounced(key, delay);
    }

    public abstract UiControl CreateTabContent();

    public virtual void Dispose()
    {
        if (Context != null)
        {
            Context.OnDataChanged -= OnContextDataChanged;
            Context.OnEditStateChanged -= OnContextEditStateChanged;
        }
    }
}
