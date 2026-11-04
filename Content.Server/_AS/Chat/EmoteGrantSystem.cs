using Content.Server.Emoting.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Server.Speech.Muting;
using Content.Shared._AS.Chat;
using Content.Shared.Body;
using Content.Shared.Chat;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Chat;

public sealed partial class EmoteGrantSystem : EntitySystem
{
    [Dependency] private SharedChatSystem _chat = null!;
    [Dependency] private IPrototypeManager ProtoMan = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmoteGrantComponent, EmoteEvent>(OnEmote,
            before: [typeof(VocalSystem), typeof(BodyEmotesSystem)],
            after: [typeof(MutingSystem)]);
        SubscribeLocalEvent<VisualBodyComponent, EmoteEvent>(OnMarkingEmote,
            before: [typeof(VocalSystem), typeof(BodyEmotesSystem)],
            after: [typeof(MutingSystem)]);
    }

    private void OnEmote(Entity<EmoteGrantComponent> ent, ref EmoteEvent args)
    {
        TryPlayGrantedSound(ent.Owner, ref args);
    }

    private void OnMarkingEmote(Entity<VisualBodyComponent> ent, ref EmoteEvent args)
    {
        if (!HasComp<EmoteGrantComponent>(ent.Owner))
            TryPlayGrantedSound(ent.Owner, ref args);
    }

    private void TryPlayGrantedSound(EntityUid source, ref EmoteEvent args)
    {
        if (args.Handled)
            return;

        if (!_chat.TryGetGrantedEmote(source, args.Emote, out var soundOverride) || soundOverride is null)
            return;
        if (_chat.AllowedToUseEmote(source, args.Emote) && ProtoMan.Resolve(soundOverride, out var sounds))
            args.Handled = _chat.TryPlayEmoteSound(source, sounds, args.Emote);
    }
}
