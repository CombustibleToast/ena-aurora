using Content.Shared._Starfall.Traits.Assorted;
using Content.Shared.Bed.Sleep;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.SSDIndicator;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._AS.Traits;

public sealed partial class MetabolicCrashesSchedulerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = null!;
    [Dependency] private INetManager _net = null!;
    [Dependency] private IRobustRandom _random = null!;
    [Dependency] private HungerSystem _hunger = null!;
    [Dependency] private MobStateSystem _mobState = null!;
    [Dependency] private StatusEffectsSystem _statusEffects = null!;

    // ᓚᘏᗢ <(I realize now that using the term crash as a variable might not be ideal and get across the wrong idea but uhhhh mraowwww
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<MetabolicCrashesSchedulerComponent>();
        while (query.MoveNext(out var uid, out var crashes))
        {
            // If we are dead or SSD don't do anything
            if (_mobState.IsDead(uid) || TryComp<SSDIndicatorComponent>(uid, out var ssd) && ssd.IsSSD)
                continue;

            if (TryComp<HungerComponent>(uid, out var hunger))
                UpdateCrash(uid, crashes, _hunger.GetHunger(hunger), crashes.HungerCrashThreshold, ref crashes.NextHungerCrash, nameof(MetabolicCrashesSchedulerComponent.NextHungerCrash));

            // If both needs are depleted, both remain active and the entity experiences incidents more frequently.
            if (TryComp<ThirstComponent>(uid, out var thirst))
                UpdateCrash(uid, crashes, thirst.CurrentThirst, crashes.ThirstCrashThreshold, ref crashes.NextThirstCrash, nameof(MetabolicCrashesSchedulerComponent.NextThirstCrash));
        }
    }

    private void UpdateCrash(EntityUid uid, MetabolicCrashesSchedulerComponent crashes, float value, float threshold, ref TimeSpan nextCrash, string fieldName)
    {
        if (value > threshold)
        {
            ResetTimer(uid, crashes, ref nextCrash, fieldName);
            return;
        }

        if (nextCrash == TimeSpan.Zero)
        {
            ScheduleTimer(uid, crashes, out nextCrash, fieldName, crashes.MinTimeBetweenCrashes, crashes.MaxTimeBetweenCrashes);
            return;
        }

        if (nextCrash > _timing.CurTime)
            return;

        TriggerCrash(uid, crashes);
        ScheduleTimer(uid, crashes, out nextCrash, fieldName, crashes.MinTimeBetweenCrashes, crashes.MaxTimeBetweenCrashes);
    }

    private void TriggerCrash(EntityUid uid, MetabolicCrashesSchedulerComponent crashes)
    {
        // 50/50 chance you get a migraine or pass out
        if (_random.Prob(0.5f))
        {
            // Do not overlap an active sleep status.
            if (_statusEffects.HasStatusEffect(uid, SleepingSystem.StatusEffectForcedSleeping))
                return;

            _statusEffects.TryAddStatusEffectDuration(uid, SleepingSystem.StatusEffectForcedSleeping, _random.Next(crashes.MinSleepDuration, crashes.MaxSleepDuration));

            return;
        }

        // Do not overlap an active chronic migraine.
        if (_statusEffects.HasStatusEffect(uid, MigraineEffectComponent.Prototype))
            return;

        _statusEffects.TrySetStatusEffectDuration(uid, MigraineEffectComponent.Prototype, _random.Next(crashes.MinMigraineDuration, crashes.MaxMigraineDuration));
    }

    private void ScheduleTimer(EntityUid uid, MetabolicCrashesSchedulerComponent crashes, out TimeSpan timer, string fieldName, TimeSpan minimum, TimeSpan maximum)
    {
        timer = _timing.CurTime + _random.Next(minimum, maximum);
        DirtyField(uid, crashes, fieldName);
    }

    private void ResetTimer(EntityUid uid, MetabolicCrashesSchedulerComponent crashes, ref TimeSpan timer, string fieldName)
    {
        if (timer == TimeSpan.Zero)
            return;

        timer = TimeSpan.Zero;
        DirtyField(uid, crashes, fieldName);
    }
}
