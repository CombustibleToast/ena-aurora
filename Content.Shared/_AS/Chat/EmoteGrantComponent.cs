using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Chat;

/// <summary>
/// Adds emotes without replacing existing emotes.
/// Removing it revokes what it granted.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmoteGrantComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<EmotePrototype>> Emotes = [];

    /// <summary>
    /// Optional sounds to change an emote.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<EmoteSoundsPrototype>? Sounds;
}
