// This is intentional to extend the existing AfkManager without changing upstream files
namespace Content.Shared.SSDIndicator;

public sealed partial class SSDIndicatorSystem
{
    /// <summary>
    /// Sets the SSD state.
    /// </summary>
    public void SetSsdState(EntityUid uid, bool isSsd)
    {
        if (!TryComp<SSDIndicatorComponent>(uid, out var component) || component.IsSSD == isSsd)
            return;

        component.IsSSD = isSsd;

        if (_icSsdSleep)
        {
            if (isSsd)
                component.FallAsleepTime = _timing.CurTime + TimeSpan.FromSeconds(_icSsdSleepTime);
            else
            {
                component.FallAsleepTime = TimeSpan.Zero;
                _statusEffects.TryRemoveStatusEffect(uid, StatusEffectSSDSleeping);
            }
        }

        Dirty(uid, component);
    }
}
