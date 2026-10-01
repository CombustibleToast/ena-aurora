using Content.Shared._AS.Storage.Components;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._AS.Storage.EntitySystems;

/// <summary>
/// <see cref="MagnetPickupComponent"/>
/// </summary>
public sealed partial class MagnetPickupSystem : EntitySystem
{
    /// <summary>
    /// Attaches the text that pops up when a magnet is toggled
    /// </summary>
    /// <param name="uid">The magnet entity</param>
    /// <param name="component">The magnet attached to the entity</param>
    /// <param name="args">The even args</param>
    private void AddToggleMagnetVerb(EntityUid uid, MagnetPickupComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (component.UseItemToggle)
            return;

        if (!HasComp<HandsComponent>(args.User))
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                ToggleMagnet(uid, component);
            },
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png")),
            Text = Loc.GetString("magnet-pickup-component-toggle-verb"),
            //Priority = component.MagnetTogglePriority // Frontier: 3 < component.MagnetTogglePriority
        };

        args.Verbs.Add(verb);
    }

    /// <summary>
    /// Sorts out the text for the examine
    /// </summary>
    /// <param name="uid">The entity being examned</param>
    /// <param name="component">The Magnet attached to the entity</param>
    /// <param name="args">The event args</param>
    private void OnExamined(EntityUid uid, MagnetPickupComponent component, ExaminedEvent args)
    {
        if (component.UseItemToggle)
            return;

        args.PushMarkup(Loc.GetString("magnet-pickup-component-on-examine-main",
                        ("stateText", Loc.GetString(component.MagnetEnabled
                        ? "magnet-pickup-component-magnet-on"
                        : "magnet-pickup-component-magnet-off"))));
    }
}
