using Content.Shared._AS.ChameleonDecorations;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._AS.ChameleonDecorations;

public sealed partial class ChameleonDecorationSystem : SharedChameleonDecorationSystem
{
    [Dependency] private SpriteSystem _sprite = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChameleonDecorationComponent, AfterAutoHandleStateEvent>(OnStateHandled);
    }

    private void OnStateHandled(Entity<ChameleonDecorationComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ApplyAppearance(ent);
    }

    protected override void ApplySprite(EntityUid uid, EntityPrototype prototype)
    {
        if (TryComp(uid, out SpriteComponent? sprite) && prototype.TryGetComponent(out SpriteComponent? targetSprite, Factory))
        {
            _sprite.CopySprite((targetSprite.Owner, targetSprite), (uid, sprite));
            // CopySprite does not copy this setting. Leaving it enabled on a
            // multi-directional target causes SpriteSystem bounds assertions.
            // if we dont snap cardinals it crashes sometimes
            _sprite.SetSnapCardinals((uid, sprite), targetSprite.SnapCardinals);
        }
    }
}
