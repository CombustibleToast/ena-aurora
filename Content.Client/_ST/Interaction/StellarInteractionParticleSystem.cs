// SPDX-FileCopyrightText: 2026 Janet Blackquill <uhhadd@gmail.com>
// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared._AS.CCVar; // Aurora's Song
using Content.Shared._ST.Interaction;
using Content.Shared.Humanoid; // Aurora's Song
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration; // Aurora's Song
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client._ST.Interaction;

public sealed partial class StellarInteractionParticleSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = null!;
    [Dependency] private SpriteSystem _sprite = null!;
    [Dependency] private AnimationPlayerSystem _animation = null!;
    [Dependency] private IConfigurationManager _configuration = null!; // Aurora's Song - Interaction particle toggle

    private const string AnimateKey = "particle-animation";
    private const string DefaultHandState = "hand"; // Aurora's Song - Species hand sprites

    private static readonly EntProtoId InteractionParticleId = "StellarInteractionParticle";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<StellarInteractionParticleEvent>(OnInteractionParticle);
    }

    private void OnInteractionParticle(StellarInteractionParticleEvent ev)
    {
        // Aurora's Song - Interaction particle toggle
        if (_configuration.GetCVar(ASCCVars.DisableStellarInteractionParticles))
            return;

        var performer = GetEntity(ev.Performer);
        var used = GetEntity(ev.Used);
        var target = GetEntity(ev.Target);

        if (!Exists(performer) || !Exists(target))
            return;

        var performerXform = Transform(performer);
        var targetXform = Transform(target);
        if (performerXform.MapID == MapId.Nullspace || targetXform.MapID == MapId.Nullspace)
            return;

        if (performerXform.ParentUid != targetXform.ParentUid)
            return;

        var performerTargetDelta = targetXform.LocalPosition - performerXform.LocalPosition;
        var particle = Spawn(InteractionParticleId, performerXform.Coordinates);

        // Aurora's Song - Use the species defined hand if it exists
        var handState = DefaultHandState;
        var particleSprite = Comp<SpriteComponent>(particle);
        if (TryComp<HumanoidProfileComponent>(performer, out var profile))
        {
            var species = profile.Species;
            var speciesState = $"{species.Id.ToLowerInvariant()}-hand";
            var rsi = _sprite.LayerGetEffectiveRsi((particle, particleSprite), 0);
            if (rsi?.TryGetState(speciesState, out _) == true)
                handState = speciesState;
        }
        _sprite.LayerSetRsiState(particle, 0, handState);
        // Aurora's Song end

        if (used is { } usedEntity && Exists(usedEntity) && TryComp<SpriteComponent>(usedEntity, out var usedSprite))
        {
            _sprite.CopySprite((usedEntity, usedSprite), particle);
            // ES START
            _sprite.SetDrawDepth(particle, (int) Shared.DrawDepth.DrawDepth.Effects);
            // ES END
        }

        var spriteColor = Comp<SpriteComponent>(particle).Color;
        _animation.Play(particle, GetAnimation(performerTargetDelta, spriteColor), AnimateKey);
    }

    private Animation GetAnimation(Vector2 endOffset, Color color)
    {
        var startRotation = _random.NextAngle(Angle.FromDegrees(-80), Angle.FromDegrees(80));
        var endRotation = Angle.Zero;
        var startScale = new Vector2(0.3f, 0.3f);
        var endScale = new Vector2(1f, 1f);
        var rotationLength = TimeSpan.FromMilliseconds(600);

        var startOffset = new Vector2();
        var offsetLength = TimeSpan.FromMilliseconds(200);

        var startColor = color.WithAlpha(color.A * 0.9f);
        var endColor = color.WithAlpha(0f);
        var colorLength = rotationLength + offsetLength;

        return new Animation()
        {
            Length = colorLength,

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startRotation, 0f),
                        new AnimationTrackProperty.KeyFrame(endRotation, (float)rotationLength.TotalSeconds, Easings.OutBounce),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startScale, 0f),
                        new AnimationTrackProperty.KeyFrame(endScale, (float)rotationLength.TotalSeconds, Easings.OutBounce),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startOffset, 0f),
                        new AnimationTrackProperty.KeyFrame(endOffset, (float)offsetLength.TotalSeconds, Easings.OutBounce),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startColor, 0f),
                        new AnimationTrackProperty.KeyFrame(startColor, (float)rotationLength.TotalSeconds),
                        new AnimationTrackProperty.KeyFrame(endColor, (float)offsetLength.TotalSeconds, Easings.InOutCirc),
                    },
                },
            },
        };
    }
}
