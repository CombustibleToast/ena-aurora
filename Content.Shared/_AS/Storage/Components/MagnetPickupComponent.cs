using Content.Shared.DeviceLinking;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared._AS.Storage.Components;

/// <summary>
/// Applies an ongoing pickup area around the attached entity.
/// Requires:
///     TimerTriggerComponent
///     ContainerContainer OR StorageComponent
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[AutoGenerateComponentPause]
public sealed partial class MagnetPickupComponent : Component
{
    /// <summary>
    /// What Container should the magnet try and insert into?
    /// Requires ContainerContainer Component
    /// Default: ""
    /// </summary>
    [DataField("containerFocus")]
    public string ContainerFocus = "";

    /// <summary>
    /// The sound that plays when a magnet picks something up
    /// Default: "storageRustle"
    /// </summary>
    [DataField("pickupSound")]
    public SoundSpecifier? PickupSound = new SoundCollectionSpecifier("storageRustle");

    // Frontier: togglable magnets
    /// <summary>
    /// Is the magnet currently enabled?
    /// Default: true
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite), DataField]
    public bool MagnetEnabled = true;

    [DataField]
    [AutoPausedField]
    [AutoNetworkedField]
    public TimeSpan NextScan = TimeSpan.Zero;

    /// <summary>
    /// Max range that the magnet can reach
    /// Default: 1f
    /// </summary>
    [DataField]
    public float Range = 1f;

    /// <summary>
    /// Does this magnet need power to be active?
    /// Default: False
    /// </summary>
    [DataField("requirePower")]
    public bool RequirePower = false;

    /// <summary>
    /// Whether to rely on the ItemToggle Component or not
    /// Default: false
    /// </summary>
    [DataField("useItemToggle")]
    public bool UseItemToggle = false;

    /// <summary>
    /// Remembers if the magnet was on when power was cut and
    /// determines if it should turn on when power returns
    /// </summary>
    public bool TurnOnWhenPowered = false;

    // Signal Ports

    /// <summary>
    /// Port for receiving an ON signal
    /// Default: "On"
    /// </summary>
    [DataField("onPort")]
    public ProtoId<SinkPortPrototype> OnPort = "On";

    /// <summary>
    /// Port for receiving an OFF signal
    /// Default: "Off"
    /// </summary>
    [DataField("offPort")]
    public ProtoId<SinkPortPrototype> OffPort = "Off";

    /// <summary>
    /// Port for receiving a TOGGLE signal
    /// Default: "Toggle"
    /// </summary>
    [DataField("togglePort")]
    public ProtoId<SinkPortPrototype> TogglePort = "Toggle";

    /// <summary>
    /// Port for receiving a PULL signal
    /// Default: "Pull"
    /// </summary>
    [DataField("pullPort")]
    public ProtoId<SinkPortPrototype> PullPort = "Trigger";

}

/// <summary>
/// Called by MagnetPickupSystem when a magnet is toggles
/// Used for things such as visuals, reactions, etc.
/// </summary>
/// <param name="Toggled">If the magnet is on or off</param>>
[ByRefEvent]
public record struct MagnetToggleEvent(bool Toggled);

/// <summary>
/// Called by MagnetPickupSystem when a magnet picks up an item
/// This event calls for every item
/// </summary>
/// <param name="Pickup">The item picked up</param>
[ByRefEvent]
public record struct MagnetPickupEvent(EntityUid Pickup);

/// <summary>
/// Called by MagnetPickupSystem when a magnet is finished picking up a haul
/// </summary>
/// <param name="Quantity">The amount of items inserted into storage</param>
/// <param name="Collected">The list of all inserted items</param>
[ByRefEvent]
public record struct FinishMagnetPickupEvent(int Quantity, HashSet<EntityUid> Collected);
