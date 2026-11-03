using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Verbs;

namespace Content.Shared.Interaction;

public abstract partial class SharedInteractionSystem
{
    /// <summary>
    /// Handles toggling toggleable clothing via alt-click
    /// </summary>
    private bool TryHandleToggleableClothingAltClick(EntityUid user, EntityUid item)
    {
        if (!HasComp<ToggleableClothingComponent>(item))
            return false;

        var verbs = _verbSystem.GetLocalVerbs(item, user, typeof(EquipmentVerb));
        foreach (var verb in verbs)
        {
            var togglesOwnSuit = verb.EventTarget == item && verb.ExecutionEventArgs is ToggleClothingEvent;
            var togglesOtherWearer = verb.Act != null && !verb.EventTarget.IsValid() && verb.ExecutionEventArgs == null;
            if (!togglesOwnSuit && !togglesOtherWearer)
                continue;

            _verbSystem.ExecuteVerb(verb, user, item);
            return true;
        }

        return false;
    }
}
