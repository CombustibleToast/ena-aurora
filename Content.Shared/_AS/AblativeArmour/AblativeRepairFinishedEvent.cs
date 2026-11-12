using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Armor;

[Serializable, NetSerializable]
public sealed partial class AblativeRepairFinishedEvent : SimpleDoAfterEvent
{
    public float Delay;

}