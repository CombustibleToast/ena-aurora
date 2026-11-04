using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat.Prototypes;

public sealed partial class EmotePrototype
{
    [DataField]
    public List<MarkingEmoteGrant> MarkingGrants = [];
}

[DataDefinition]
public sealed partial class MarkingEmoteGrant
{
    /// <summary>
    /// Eligible species. Empty allows any species.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> Species = [];

    /// <summary>
    /// Matches any marking in these body part categories, or any specific marking below.
    /// Empty category and marking lists match nothing.
    /// </summary>
    [DataField]
    public HashSet<HumanoidVisualLayers> Categories = [];

    [DataField]
    public HashSet<ProtoId<MarkingPrototype>> Markings = [];

    /// <summary>
    /// Optional sound set for this grant. Optional uses the entity's normal emote sound.
    /// </summary>
    [DataField]
    public ProtoId<EmoteSoundsPrototype>? Sounds;
}
