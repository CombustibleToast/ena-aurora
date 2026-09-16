using Content.Shared._Funkystation.Traits;
using Content.Shared.Nutrition.Components;

namespace Content.Shared.Nutrition.EntitySystems;

public sealed partial class HungerSystem
{
    private bool IsHungerAlertSuppressed(EntityUid uid, HungerThreshold threshold)
    {
        return threshold == HungerThreshold.Peckish && HasComp<SuppressedBodilyCuesComponent>(uid);
    }
}
