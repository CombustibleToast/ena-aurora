using Content.Shared._Floof.Consent;
using Content.Shared.EntityEffects;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Consent.EntityEffects;

public sealed partial class Consent : EntityEffectCondition
{

    [DataField]
    public List<ProtoId<ConsentTogglePrototype>> EffectTypes = new();

    public override bool Condition(EntityEffectBaseArgs args)
    {
        PlayerConsentSettings? settings = null;

        if (args.EntityManager.System<SharedMindSystem>().TryGetMind(args.TargetEntity, out _, out var mind)
            && mind.Session is {} session)
            args.EntityManager.System<SharedConsentSystem>().TryGetConsent(session.UserId, out settings);

        foreach (var effect in EffectTypes)
        {
            if(!args.EntityManager.System<SharedConsentSystem>().HasConsent(settings, effect))
               return false;
        }
        return true;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
