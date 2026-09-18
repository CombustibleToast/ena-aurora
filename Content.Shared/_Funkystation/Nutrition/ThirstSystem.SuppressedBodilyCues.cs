using Content.Shared._Funkystation.Traits;
using Content.Shared.Nutrition.Components;

namespace Content.Shared.Nutrition.EntitySystems;

public sealed partial class ThirstSystem
{
    private bool IsThirstAlertSuppressed(EntityUid uid, ThirstThreshold threshold)
    {
        return threshold == ThirstThreshold.Thirsty && HasComp<SuppressedBodilyCuesComponent>(uid);
    }
}
