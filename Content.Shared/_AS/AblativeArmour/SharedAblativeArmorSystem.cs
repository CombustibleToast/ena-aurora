using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Stacks;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._AS.Armor;

/// <summary>
///     This handles logic relating to <see cref="AblativeArmorComponent" />
/// </summary>
public abstract partial class SharedAblativeArmorSystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private ClothingSystem _clothing = default!;
    [Dependency] private SharedStackSystem _stacks = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private IGameTiming _gameTiming = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AblativeArmorComponent, InventoryRelayedEvent<CoefficientQueryEvent>>(OnCoefficientQuery);
        SubscribeLocalEvent<AblativeArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
        SubscribeLocalEvent<AblativeArmorComponent, BorgModuleRelayedEvent<DamageModifyEvent>>(OnBorgDamageModify);

        SubscribeLocalEvent<AblativeArmorComponent, GetVerbsEvent<ExamineVerb>>(OnArmorVerbExamine);

        SubscribeLocalEvent<AblativeArmorComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<AblativeArmorComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<AblativeArmorRepairComponent, InteractUsingEvent>(RelayExternalRepair);
        SubscribeLocalEvent<AblativeArmorComponent, InteractUsingEvent>(RelayDirectRepair);
        SubscribeLocalEvent<AblativeArmorComponent, AblativeRepairFinishedEvent>(OnRepairFinished);
    }

    /// <summary>
    /// Get the total Damage reduction value of all equipment caught by the relay.
    /// </summary>
    /// <param name="ent">The item that's being relayed to</param>
    /// <param name="args">The event, contains the running count of armor percentage as a coefficient</param>
    private void OnCoefficientQuery(Entity<AblativeArmorComponent> ent, ref InventoryRelayedEvent<CoefficientQueryEvent> args)
    {
        if (TryComp<MaskComponent>(ent, out var mask) && mask.IsToggled)
            return;

        foreach (var armorCoefficient in ent.Comp.Modifiers.Coefficients)
        {
            args.Args.DamageModifiers.Coefficients[armorCoefficient.Key] = args.Args.DamageModifiers.Coefficients.TryGetValue(armorCoefficient.Key, out var coefficient) ? coefficient * armorCoefficient.Value : armorCoefficient.Value;
        }
    }

    /// <summary>
    /// A rewritten application of a <see cref="DamageSpecifier.ApplyModifierSet"/> to account for mitigation changes from integrity loss 
    /// </summary>
    /// <param name="uid">The Uid of the armour</param>
    /// <param name="component">The ablative armour component of the armour</param>
    /// <param name="args">The <see cref="InventoryRelayedEvent<DamageModifyEvent>"/> containing damage</param>
    private void OnDamageModify(EntityUid uid, AblativeArmorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        // If we are a mask, only mitigate if we are pulled up
        if (TryComp<MaskComponent>(uid, out var mask) && mask.IsToggled)
            return;

        // Calculate what percentage of integrity we have
        var healthRatio = (component.MaxIntegrity - component.IntegrityDamage) / component.MaxIntegrity;

        DamageSpecifier newDamage = new();
        newDamage.DamageDict.EnsureCapacity(args.Args.Damage.DamageDict.Count);

        foreach (var (key, value) in args.Args.Damage.DamageDict)
        {
            if (value == 0)
                continue;

            if (value < 0)
            {
                newDamage.DamageDict[key] = value;
                continue;
            }

            float newValue = value.Float();

            if (component.Modifiers.FlatReduction.TryGetValue(key, out var reduction))
            {
                // Lerp between min% effectiveness and max effectiveness based on integrity %
                reduction = DoMath(component.MinIntegrityFlatReductionModifier, reduction, healthRatio, false);
                newValue = Math.Max(0f, newValue - reduction); // flat reductions can't heal you
            }

            if (component.Modifiers.Coefficients.TryGetValue(key, out var coefficient))
            {
                // Lerp between min% effectiveness and max effectiveness based on integrity %
                coefficient = DoMath(component.MinIntegrityCoefficientModifier, coefficient, healthRatio, true);
                newValue *= coefficient; // coefficients can heal you, e.g. cauterizing bleeding
            }

            if (newValue != 0)
                newDamage.DamageDict[key] = FixedPoint2.New(newValue);
        }

        args.Args.Damage = newDamage;

        // Damage the armour after it mitigates the incoming
        DamageIntegrity(uid, component, args.Args.OriginalDamage);
    }

    private void OnBorgDamageModify(EntityUid uid, AblativeArmorComponent component,
        ref BorgModuleRelayedEvent<DamageModifyEvent> args)
    {
        if (TryComp<MaskComponent>(uid, out var mask) && mask.IsToggled)
            return;

        // Calculate what percentage of integrity we have
        var healthRatio = (component.MaxIntegrity - component.IntegrityDamage) / component.MaxIntegrity;

        DamageSpecifier newDamage = new();
        newDamage.DamageDict.EnsureCapacity(args.Args.Damage.DamageDict.Count);

        foreach (var (key, value) in args.Args.Damage.DamageDict)
        {
            if (value == 0)
                continue;

            if (value < 0)
            {
                newDamage.DamageDict[key] = value;
                continue;
            }

            float newValue = value.Float();

            if (component.Modifiers.FlatReduction.TryGetValue(key, out var reduction))
            {
                // Lerp between min% effectiveness and max effectiveness based on integrity %
                reduction = DoMath(component.MinIntegrityFlatReductionModifier, reduction, healthRatio, false);
                newValue = Math.Max(0f, newValue - reduction); // flat reductions can't heal you
            }

            if (component.Modifiers.Coefficients.TryGetValue(key, out var coefficient))
            {
                // Lerp between min% effectiveness and max effectiveness based on integrity %
                coefficient = DoMath(component.MinIntegrityCoefficientModifier, coefficient, healthRatio, true);
                newValue *= coefficient; // coefficients can heal you, e.g. cauterizing bleeding
            }

            if (newValue != 0)
                newDamage.DamageDict[key] = FixedPoint2.New(newValue);
        }

        args.Args.Damage = newDamage;

        // Damage the armour after it mitigates the incoming
        DamageIntegrity(uid, component, args.Args.OriginalDamage);
    }


    /// <summary>
    /// Creates the armour examine button that appears when you examine the armour
    /// </summary>
    /// <param name="uid">The Uid of the armour</param>
    /// <param name="component">The ablative armour component of the armour</param>
    /// <param name="args">The <see cref="GetVerbsEvent<ExamineVerb>" that is requesting the examine.</param>
    private void OnArmorVerbExamine(EntityUid uid, AblativeArmorComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !component.ShowArmorOnExamine)
            return;

        var examineMarkup = GetArmorExamine(component);

        var ev = new ArmorExamineEvent(examineMarkup);
        RaiseLocalEvent(uid, ref ev);

        _examine.AddDetailedExamineVerb(args, component, examineMarkup,
            Loc.GetString("armor-examinable-verb-text"), "/Textures/Interface/VerbIcons/dot.svg.192dpi.png",
            Loc.GetString("armor-examinable-verb-message"));
    }

    /// <summary>
    /// Formats the damage mitigation values, integrity, and repair information for the Detailed Examine Verb
    /// </summary>
    /// <param name="component">The ablative armor component of the armour that we are sourcing information from</param>
    /// <returns>A formatted message containing armour information</returns>
    private FormattedMessage GetArmorExamine(AblativeArmorComponent component)
    {
        // Calculate what percentage of integrity we have
        var healthRatio = (component.MaxIntegrity - component.IntegrityDamage) / component.MaxIntegrity;

        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("armor-examine"));

        // For each percentile modifier, add a line to the examine text
        foreach (var coefficientArmor in component.Modifiers.Coefficients)
        {
            var newValue = DoMath(component.MinIntegrityCoefficientModifier, coefficientArmor.Value, healthRatio, true);
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + coefficientArmor.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-coefficient-value",
                ("type", armorType),
                ("value", MathF.Round((1f - newValue) * 100, 0).ToString()) // Floats were being evil without .ToString()
            ));
        }

        // For each flat reduction, add a line to the examine text
        foreach (var flatArmor in component.Modifiers.FlatReduction)
        {
            var newValue = DoMath(component.MinIntegrityFlatReductionModifier, flatArmor.Value, healthRatio, false);
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + flatArmor.Key.ToLower());
            msg.AddMarkupOrThrow(Loc.GetString("armor-reduction-value",
                ("type", armorType),
                ("value", MathF.Round(newValue, 1).ToString()) // Ditto
            ));
        }

        // Add additional information about our integrity
        msg.PushNewline();
        msg.AddMarkupOrThrow(Loc.GetString("ablative-armor-integrity",
                ("currentIntegrity", component.MaxIntegrity - component.IntegrityDamage),
                ("maxIntegrity", component.MaxIntegrity)
            ));

        // Add additional information about repair materials
        if (component.MaterialProto != null)
        {
            msg.PushNewline();
            msg.AddMarkupOrThrow(Loc.GetString("ablative-armor-repair-material", ("repairMaterial", component.MaterialProto)));
        }
        return msg;
    }

    /// <summary>
    /// Reduces the ablative armours integrity based on the intensity of incoming damage and nominal resistances
    /// </summary>
    /// <param name="uid">The Uid of the armour being damaged</param>
    /// <param name="component">The ablative armor component of the armour</param>
    /// <param name="damage">The damage specifier detailing incoming damage</param>
    /// <remarks>
    /// This likely could hav been done with a DamageableComponent
    /// </remarks>
    private void DamageIntegrity(EntityUid uid, AblativeArmorComponent component, DamageSpecifier damage)
    {
        var incomingDamage = 0.0F;
        var deflected = false;

        foreach (var (key, value) in damage.DamageDict) // This is just stripped down code from DamageSpecifier.cs
        {

            float newValue = value.Float();

            // We only care about damages that our ablative armour would ablate, I.E. flat resists
            if (component.Modifiers.FlatReduction.TryGetValue(key, out var reduction))
            {
                incomingDamage += Math.Max(0f, newValue - reduction); // Minimum 0
                deflected = true;
            }
        }


        if (incomingDamage > 1)
        {
            component.IntegrityDamage = Math.Min(component.MaxIntegrity, component.IntegrityDamage + incomingDamage);
            Dirty(uid, component);
        }
        else if (deflected)
        {
            // Minimum one damage to the integrity
            component.IntegrityDamage = Math.Min(component.MaxIntegrity, component.IntegrityDamage + 1);
            Dirty(uid, component);
        }
    }

    /// <summary>
    /// Adds a <see cref="AblativeArmorRepairComponent"/> with the Uid and Component of the armour to facilitate repair relaying, if the armour can be externally repaired
    /// </summary>
    /// <param name="entity">The Uid of the armour being equipped</param>
    /// <param name="component">The ablative armor component of the armour</param>
    /// <param name="args"></param>
    private void OnEquipped(EntityUid entity, AblativeArmorComponent component, GotEquippedEvent args)
    {
        if (!_gameTiming.IsFirstTimePredicted)
            return;
        EnsureComp<AblativeArmorRepairComponent>(args.EquipTarget, out var repairable);
        repairable.Recipients.Add(entity); // Add our Uid and component to wearer for tracking and relaying repairs
        Dirty(args.EquipTarget, repairable);

    }

    /// <summary>
    /// Removes the armours entry from the wearers <see cref="AblativeArmorRepairComponent"/>, and removes the component if no entries remain
    /// </summary>
    /// <param name="entity">The Uid of the armour being equipped</param>
    /// <param name="component">The ablative armor component of the armour</param>
    /// <param name="args"></param>
    private void OnUnequipped(EntityUid entity, AblativeArmorComponent component, GotUnequippedEvent args)
    {
        // Clients were predicting this multiple times, causing mid enumeration modification errors
        // They were also predicting both this and OnEquipped multiple times in one go. I have no idea how prediction works but that also got stopped by running this check
        if (!_gameTiming.IsFirstTimePredicted)
            return;

        if (TryComp<AblativeArmorRepairComponent>(args.EquipTarget, out var repairable))
        {

            repairable.Recipients.Remove(entity); // Remove ourselves from the Hashset
            Dirty(args.EquipTarget, repairable);

            if (repairable.Recipients.Count == 0)
                RemComp<AblativeArmorRepairComponent>(args.EquipTarget); // And remove the tracker component if theres nothing left

        }
    }

    /// <summary>
    /// Relays any repair attempts on the user to any armour that is being tracked
    /// </summary>
    /// <param name="uid">The entity wearing the armour</param>
    /// <param name="component">The component keeping track of the entitys worn ablative armours</param>
    /// <param name="args"></param>
    // To-do: Sanity check that the armour we are tracking is actually still being worn by the person
    private void RelayExternalRepair(EntityUid uid, AblativeArmorRepairComponent component, InteractUsingEvent args)
    {
        foreach (var key in component.Recipients)
        {
            if (TryComp<AblativeArmorComponent>(key, out var value))
                Repair(key, value, args, external: true);
        }
    }

    private void RelayDirectRepair(EntityUid uid, AblativeArmorComponent component, InteractUsingEvent args)
    {
        var equipped = false;
        if (TryComp<ClothingComponent>(uid, out var clothing) && clothing.InSlotFlag != null)
            equipped = _clothing.IsEquipped((uid, clothing));
        Repair(uid, component, args, equipped: equipped);

    }

    /// <summary>
    /// Handles checking conditions and creating the do-after event for attempting to repair the armour
    /// </summary>
    /// <param name="uid">The Uid of the armour being repaired</param>
    /// <param name="component">The ablative armor component of the armour</param>
    /// <param name="args"></param>
    // Hell
    private async void Repair(EntityUid uid, AblativeArmorComponent component, InteractUsingEvent args, bool external = false, bool equipped = false)
    {
        if (args.Handled
            || component.IntegrityDamage == 0 // Is there damage to repair?
            || args.User == args.Target && !component.AllowSelfRepair && external // Are we repairing ourselves, and is it allowed?
            || !component.AllowSelfRepair && equipped // Are we trying to repair ourselves by directly clicking on the armour?
            || args.User != args.Target && !component.AllowOtherRepair && external) // Is someone else repairing us, and is it allowed?
            return;

        if (component.MaterialProto == null
            || !TryComp<StackComponent>(args.Used, out var stack)
            || stack == null
            || _prototypeManager.Index<StackPrototype>(component.MaterialProto) != _prototypeManager.Index<StackPrototype>(stack.StackTypeId) // And is it the right stack item?
            || stack.Count < 5) // Do we have enough material?)
            return;

        float delay = args.User == args.Target
            ? component.DoAfterDelay * component.SelfRepairPenalty
            : component.DoAfterDelay;

        var repairEvent = new AblativeRepairFinishedEvent
        {
            Delay = delay
        };

        var doAfterEventArgs =
        new DoAfterArgs(EntityManager,
        args.User,
        TimeSpan.FromSeconds(delay),
        repairEvent,
        uid,
        target: args.Target,
        args.Used)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfterEventArgs);

    }

    /// <summary>
    /// Handles changing the integrity of armour after it has been repaired, informing the client, and repeating the do-after if necessary
    /// </summary>
    /// <param name="uid">The Uid of the armour being repaired</param>
    /// <param name="ablativeComponent">The ablative armour component of the armour</param>
    /// <param name="args"></param>
    private void OnRepairFinished(EntityUid uid, AblativeArmorComponent ablativeComponent, AblativeRepairFinishedEvent args)
    {
        if (args.Cancelled
        || args.Handled
        || ablativeComponent.IntegrityDamage == 0 // Did someone else repair the armour first?
        || TryComp<StackComponent>(args.Used, out var stack) && stack.Count < 5) // Do we still have enough material
            return;

        // Repair a set amount, down to 0
        ablativeComponent.IntegrityDamage = Math.Max(0, ablativeComponent.IntegrityDamage - ablativeComponent.RepairAmount);
        Dirty(uid, ablativeComponent);


        if (args.Used is { } material) // I hate null
        {
            _stacks.ReduceCount((material, stack), 5);

            // If theres damage to repair and enough material to repair it, keep going
            if (stack != null && stack.Count >= 5 && ablativeComponent.IntegrityDamage != 0)
                args.Repeat = true;
        }


        if (!args.Repeat)
        {
            _popupSystem.PopupClient(Loc.GetString("ablative-armor-finished-repairing", ("target", uid)), args.User, args.User);
        }
    }

    // I'm bad with names, okay
    private float DoMath(float modifier, float original, float ratio, bool coefficient)
    {
        var originalModified = original * modifier;

        if (coefficient)
            originalModified = 1 - (1 - original) * modifier;

        return MathHelper.Lerp(originalModified, original, ratio);
    }
}
