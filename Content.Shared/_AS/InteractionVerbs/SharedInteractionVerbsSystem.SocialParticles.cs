// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Interaction;
using Content.Shared.Verbs;

namespace Content.Shared.InteractionVerbs;

/// <summary>
/// Handling for delaying interaction particles until the interaction succeeds.
/// </summary>
public abstract partial class SharedInteractionVerbsSystem
{
    [Dependency] private SharedInteractionSystem _particleInteractionSystem = null!;

    private void ApplyInteractionParticleTiming(InteractionVerbPrototype proto, Verb verb)
    {
        if (!proto.InteractionParticle || proto.Delay > TimeSpan.Zero)
            verb.DoContactInteraction = false;
    }

    private void OnInteractionVerbPerformed(InteractionVerbPrototype proto, InteractionArgs args)
    {
        if (proto.DoContactInteraction && (!proto.InteractionParticle || proto.Delay > TimeSpan.Zero))
            _particleInteractionSystem.DoContactInteraction(args.User, args.Target, null, false, interactionParticles: proto.InteractionParticle);
    }
}
