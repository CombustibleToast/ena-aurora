using UiControl = Robust.Client.UserInterface.Control;

namespace Content.Client._DEN.Denu.Core;

public interface IDenuModule : IDisposable
{
    string Id { get; }

    string DisplayName { get; }

    int TabOrder { get; }

    void Initialize(DenuModuleContext context);

    UiControl CreateTabContent();
}
