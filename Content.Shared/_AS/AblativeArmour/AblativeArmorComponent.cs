using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;
using Content.Shared.Tools;

namespace Content.Shared._AS.Armor;

/// <summary>
/// Used for clothing that reduces damage when worn, and can take damage and have its resistances reduced.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedAblativeArmorSystem)), AutoGenerateComponentState]
public sealed partial class AblativeArmorComponent : Component
{
    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;

    /// <summary>
    /// A multiplier applied to the calculated point value
    /// to determine the monetary value of the armor
    /// </summary>
    [DataField]
    public float PriceMultiplier = 1;

    /// <summary>
    /// If true, you can examine the armor to see the protection. If false, the verb won't appear.
    /// </summary>
    [DataField]
    public bool ShowArmorOnExamine = true;

    /// <summary>
    /// The current damage the armour has taken, used to determine the efficacy of its resistances
    /// </summary>
    [DataField, AutoNetworkedField]
    public float IntegrityDamage = 0;

    /// <summary>
    /// The health pool of the ablative armour, used to determine the efficacy of its resistances
    /// </summary>
    [DataField]
    public float MaxIntegrity = 100;

    /// <summary>
    /// What percentage of the 'coefficients' does the ablative armour apply while at minimum integrity, scales linearly from 100% effectiveness to this number
    /// </summary>
    [DataField]
    public float MinIntegrityCoefficientModifier = 0.5F;

    /// <summary>
    /// What percentage of the 'FlatReduction' does the ablative armour apply while at minimum integrity, scales linearly from 100% effectiveness to this number
    /// </summary>
    [DataField]
    public float MinIntegrityFlatReductionModifier = 0.5F;

    /// <summary>
    /// Whether the armour can be repaired, while it is being worn, by the person wearing it
    /// </summary>
    [DataField]
    public bool AllowSelfRepair = true;

    /// <summary>
    /// If self repair is allowed, multiply the do-after time by this number
    /// </summary>
    [DataField]
    public float SelfRepairPenalty = 1;

    /// <summary>
    /// Whether the armour can be repaired, while it is being worn, by someone who is not repairing it
    /// </summary>
    [DataField]
    public bool AllowOtherRepair = false;

    /// <summary>
    /// How long the do-after time of a single repair cycle takes, in seconds
    /// </summary>
    [DataField]
    public float DoAfterDelay = 5;

    /// <summary>
    /// How much integrity should be restored per repair cycle
    /// </summary>
    [DataField]
    public float RepairAmount = 50;

    /// <summary>
    /// If not null, the material needed to repair the armour
    /// </summary>
    // currently irrepairable if null
    [DataField]
    public ProtoId<StackPrototype>? MaterialProto;
}

/// <summary>
/// Event raised on an armor entity to get additional examine text relating to its armor.
/// </summary>
/// <param name="Msg"></param>
[ByRefEvent]
public record struct ArmorExamineEvent(FormattedMessage Msg);

/// <summary>
/// A Relayed inventory event, gets the total Armor for all Inventory slots defined by the Slotflags in TargetSlots
/// </summary>
public sealed class CoefficientQueryEvent : EntityEventArgs, IInventoryRelayEvent
{
    /// <summary>
    /// All slots to relay to
    /// </summary>
    public SlotFlags TargetSlots { get; set; }

    /// <summary>
    /// The Total of all Coefficients.
    /// </summary>
    public DamageModifierSet DamageModifiers { get; set; } = new DamageModifierSet();

    public CoefficientQueryEvent(SlotFlags slots)
    {
        TargetSlots = slots;
    }
}
