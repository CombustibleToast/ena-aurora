// Aurora Song - AS Bluespace Bounty System
// Based on New Frontier Station 14's Bluespace Error System
// Original implementation: https://github.com/new-frontiers-14/frontier-station-14
// This component adds bounty checking functionality to bluespace error events

using Content.Server._AS.StationEvents.Events;
using Content.Server._NF.StationEvents.Components;
using Content.Server.StationEvents.Components;
using Content.Shared._NF.Bank.Components;
using Content.Shared.Radio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.StationEvents.Components;

/// <summary>
/// Aurora Song - Bounty variant of New Frontier's BluespaceErrorRuleComponent
/// Checks for specific mob prototypes and their cuff states for bonus rewards
/// </summary>
[RegisterComponent, Access(typeof(BluespaceErrorBountyRule))]
public sealed partial class BluespaceErrorBountyRuleComponent : Component
{
    // Fields adapted from Frontier's BluespaceErrorRuleComponent
    [DataField("groups")]
    public Dictionary<string, IBluespaceSpawnGroup> Groups = new();

    [DataField("rewardAccounts")]
    public Dictionary<SectorBankAccount, float> RewardAccounts = new();

    [DataField]
    public List<EntityUid> GridsUid = new();

    [DataField]
    public List<MapId> MapsUid = new();

    [DataField]
    public bool AnchorAfterWarp = false;

    [DataField]
    public bool DeleteGridsOnEnd = true;

    [DataField]
    public double StartingValue = 0;

    /// <summary>
    /// Whether to include grid value (salvage) as part of the reward.
    /// If true, the grid's appraised value is calculated and awarded.
    /// If false, only bounty objective rewards/penalties are given.
    /// </summary>
    [DataField]
    public bool IncludeGridValue = true;

    // Aurora Song - Bounty-specific fields

    /// <summary>
    /// Capture targets: Mobs that must be alive AND cuffed
    /// Key: Prototype ID (e.g., "MobPirateCaptain")
    /// Value: Tuple of (expectedCount, rewardPerTarget, penaltyPerMissing)
    /// </summary>
    [DataField]
    public Dictionary<string, BountyCaptureTarget> CaptureTargets = new();

    /// <summary>
    /// Elimination targets: Mobs that must be dead
    /// Key: Prototype ID (e.g., "MobHostileSpider")
    /// Value: Tuple of (expectedCount, rewardPerTarget, penaltyPerSurvivor)
    /// </summary>
    [DataField]
    public Dictionary<string, BountyEliminationTarget> EliminationTargets = new();

    /// <summary>
    /// Removal targets: Items/entities that must be removed from the grid
    /// Key: Prototype ID (e.g., "CrateContraband")
    /// Value: Tuple of (expectedCount, rewardPerRemoved, penaltyPerRemaining)
    /// </summary>
    [DataField]
    public Dictionary<string, BountyRemovalTarget> RemovalTargets = new();

    /// <summary>
    /// Rescue targets: Mobs that must be alive and NOT cuffed
    /// Key: Prototype ID (e.g., "MobHumanScientist")
    /// Value: Tuple of (expectedCount, rewardPerTarget, penaltyPerMissing)
    /// </summary>
    [DataField]
    public Dictionary<string, BountyRescueTarget> RescueTargets = new();

    /// <summary>
    /// Radio channels for bounty result announcements
    /// </summary>
    [DataField]
    public List<ProtoId<RadioChannelPrototype>> AnnouncementChannels = new();
}

/// <summary>
/// Capture target definition - must be alive AND cuffed
/// </summary>
[DataDefinition]
public sealed partial class BountyCaptureTarget
{
    [DataField("count")]
    public int ExpectedCount;

    [DataField("reward")]
    public int RewardPerTarget;

    [DataField("penalty")]
    public int PenaltyPerMissing;
}

/// <summary>
/// Elimination target definition - must be dead
/// </summary>
[DataDefinition]
public sealed partial class BountyEliminationTarget
{
    [DataField("count")]
    public int ExpectedCount;

    [DataField("reward")]
    public int RewardPerTarget;

    [DataField("penalty")]
    public int PenaltyPerSurvivor;
}

/// <summary>
/// Removal target definition - entity must not be on grid
/// </summary>
[DataDefinition]
public sealed partial class BountyRemovalTarget
{
    [DataField("count")]
    public int ExpectedCount;

    [DataField("reward")]
    public int RewardPerRemoved;

    [DataField("penalty")]
    public int PenaltyPerRemaining;
}

/// <summary>
/// Rescue target definition - must be alive and NOT cuffed
/// </summary>
[DataDefinition]
public sealed partial class BountyRescueTarget
{
    [DataField("count")]
    public int ExpectedCount;

    [DataField("reward")]
    public int RewardPerTarget;

    [DataField("penalty")]
    public int PenaltyPerMissing;
}
