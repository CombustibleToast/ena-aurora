using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._AS.StatusEffects.Mnemolith;

public sealed partial class MnemolithOverlaySystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayManager = null!;
    [Dependency] private IPlayerManager _playerManager = null!;

    private MnemolithOverlay _overlay = null!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new MnemolithOverlay();
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(_ => AddOverlay());
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(_ => RemoveOverlay());

        if (_playerManager.LocalEntity != null)
            AddOverlay();
    }

    public override void Shutdown()
    {
        RemoveOverlay();
        base.Shutdown();
    }

    private void AddOverlay()
    {
        if (!_overlayManager.HasOverlay<MnemolithOverlay>())
            _overlayManager.AddOverlay(_overlay);
    }

    private void RemoveOverlay()
    {
        _overlayManager.RemoveOverlay(_overlay);
    }
}
