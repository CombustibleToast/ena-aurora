using Content.Shared.Chat.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingPrototype
{
    /// <summary>
    /// Emotes available to entities with this marking.
    /// </summary>
    [DataField]
    public List<MarkingGrantedEmote> EmoteGrants = [];
}

[DataDefinition]
public sealed partial class MarkingGrantedEmote
{
    [DataField(required: true)]
    public ProtoId<EmotePrototype> Emote;

    /// <summary>
    /// Eligible species. Empty allows any species using this marking.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> Species = [];

    /// <summary>
    /// Optional sound override. Omitted uses the entity's set sound.
    /// </summary>
    [DataField]
    public ProtoId<EmoteSoundsPrototype>? Sounds;
}
