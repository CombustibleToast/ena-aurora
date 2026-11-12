using System.Numerics;
using Content.Shared._Starfall.Particles;
using Content.Shared._Starfall.Particles.Visuals;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._Starfall.Particles.Visuals;

/// <summary>
/// Handles <see cref="ColdBreathParticleEvent"/> and displays cold breath particles.
/// </summary>
public sealed partial class ColdBreathParticleSystem : EntitySystem
{
    private static readonly ProtoId<ParticleEffectPrototype> ColdBreathEffect = "SfColdBreath";

    [Dependency] private ParticleSystem _particles = null!;
    [Dependency] private SpriteSystem _sprites = null!;
    [Dependency] private IEyeManager _eye = null!;
    [Dependency] private SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ColdBreathParticleEvent>(OnColdBreath);
    }

    private void OnColdBreath(ColdBreathParticleEvent ev)
    {
        var entity = GetEntity(ev.Entity);
        if (!Exists(entity))
        {
            _particles.SpawnEffect(ColdBreathEffect, ev.Coords);
            return;
        }

        var spawnCoords = ev.Coords;
        ParticleRuntimeOverrides? overrides = null;
        if (TryComp<SpriteComponent>(entity, out var sprite))
        {
            var bounds = _sprites.LayerMapTryGet((entity, sprite), HumanoidVisualLayers.Head, out var headLayer, false) &&
                         sprite[headLayer] is SpriteComponent.Layer layer
                ? _sprites.GetLocalBounds(layer)
                : _sprites.GetLocalBounds((entity, sprite));

            var eyeAngle = (float)_eye.CurrentEye.Rotation;
            var worldFacing = _transform.GetWorldRotation(entity).RotateVec(-Vector2.UnitY);
            var screenFacing = Rotate(worldFacing, eyeAngle);
            var screenHeadOffset = new Vector2(0f, bounds.Height * 0.20f);
            var worldHeadOffset = Rotate(screenHeadOffset, -eyeAngle);
            var mouthDistance = MathF.Max(0.18f, bounds.Width * 0.24f);
            if (screenFacing.Y < -0.5f)
                mouthDistance *= 0.4f;
            var worldMouthOffset = worldHeadOffset + worldFacing * mouthDistance;

            var entityCoords = _transform.GetMapCoordinates(entity);
            spawnCoords = new MapCoordinates(entityCoords.Position + worldMouthOffset, entityCoords.MapId);
            overrides = new ParticleRuntimeOverrides
            {
                SpawnOffset = Vector2.Zero,
                EmitAngle = new Angle(MathF.Atan2(screenFacing.X, screenFacing.Y)),
            };
        }

        _particles.CreateParticle(ColdBreathEffect, spawnCoords, overrides: overrides);
    }

    private static Vector2 Rotate(Vector2 vector, float radians)
    {
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        return new Vector2(vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos);
    }
}
