using Content.Shared._AS.Chat;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat;

public abstract partial class SharedChatSystem
{
    [Dependency] private MarkingEmoteCacheSystem _markingEmoteCache = null!;

    private bool HasGrantedEmote(EntityUid source, EmotePrototype emote)
    {
        return TryGetGrantedEmote(source, emote, out _);
    }

    /// <summary>
    /// Checks all grant sources for an emote and its sound.
    /// </summary>
    public bool TryGetGrantedEmote(EntityUid source, EmotePrototype emote, out ProtoId<EmoteSoundsPrototype>? sounds)
    {
        // Component grants take priority
        if (!TryComp<EmoteGrantComponent>(source, out var grant) || !grant.Emotes.Contains(emote.ID))
            return _markingEmoteCache.TryGetGrant(source, emote.ID, out sounds);
        sounds = grant.Sounds;
        return true;

    }
}
