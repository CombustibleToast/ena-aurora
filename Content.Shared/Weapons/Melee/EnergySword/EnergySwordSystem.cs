using Content.Shared.Interaction;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Toggleable;
using Content.Shared.Tools.Systems;
using Robust.Shared.Random;
using Content.Shared.Emag.Systems; // Aurora's Song - Emag Color Changing

namespace Content.Shared.Weapons.Melee.EnergySword;

public sealed partial class EnergySwordSystem : EntitySystem
{
    [Dependency] private SharedRgbLightControllerSystem _rgbSystem = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedToolSystem _toolSystem = default!;
    [Dependency] private EmagSystem _emag = default!; // Aurora's Song - Emag Color Changing

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnergySwordComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EnergySwordComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<EnergySwordComponent, GotEmaggedEvent>(OnEmagged); // Aurora's Song - Emag Color Changing
    }
    // Used to pick a random color for the blade on map init.
    private void OnMapInit(Entity<EnergySwordComponent> entity, ref MapInitEvent args)
    {
        if (entity.Comp.ColorOptions.Count != 0)
        {
            entity.Comp.ActivatedColor = _random.Pick(entity.Comp.ColorOptions);
            Dirty(entity);
        }

        if (!TryComp(entity, out AppearanceComponent? appearanceComponent))
            return;

        _appearance.SetData(entity, ToggleableVisuals.Color, entity.Comp.ActivatedColor, appearanceComponent);
    }

    // Used to make the blade multicolored when using a multitool on it.
    private void OnInteractUsing(Entity<EnergySwordComponent> entity, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (entity.Comp.BlockHacking || !_toolSystem.HasQuality(args.Used, SharedToolSystem.PulseQuality)) // Frontier: add entity.Comp.BlocksHacking
            return;

        args.Handled = true;
        entity.Comp.Hacked = !entity.Comp.Hacked;

        if (entity.Comp.Hacked)
        {
            var rgb = EnsureComp<RgbLightControllerComponent>(entity);
            _rgbSystem.SetCycleRate(entity, entity.Comp.CycleRate, rgb);
        }
        else
            RemComp<RgbLightControllerComponent>(entity);

        Dirty(entity);
    }

    // Aurora's Song Start - Emag Color Changing
    private void OnEmagged(Entity<EnergySwordComponent> entity, ref GotEmaggedEvent args)
    {
        if (args.Handled)
            return;

        // Prevents color change if the entity is multi-tool hacked, or unhackable
        if (entity.Comp.BlockHacking || entity.Comp.Hacked)
            return;

        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (entity.Comp.ColorOptions.Count != 0)
        {
            entity.Comp.ActivatedColor = _random.Pick(entity.Comp.ColorOptions);
            Dirty(entity);
        }

        if (!TryComp(entity, out AppearanceComponent? appearanceComponent))
            return;

        _appearance.SetData(entity, ToggleableVisuals.Color, entity.Comp.ActivatedColor, appearanceComponent);

        args.Repeatable = true;
        args.Handled = true;
    }
    // Aurora's Song End
}
