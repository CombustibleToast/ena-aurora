using System.Numerics;
using Content.Shared._AS.CCVar;
using Content.Shared._AS.StatusEffects;
using Content.Shared.Body.Components;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.StatusEffectNew;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._AS.StatusEffects.Mnemolith;

/// <summary>
/// NIGHTMARE MNEMOLITH SHADER HAHAHAHHA
/// Controls the shader and allthat for the eyes and everythingggg
/// </summary>
public sealed partial class MnemolithOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> MnemolithShader = "Mnemolith";
    private static readonly ReagentId Mnemolith = new("MnemolithReagent", null);

    [Dependency] private IEntityManager _entityManager = null!;
    [Dependency] private IPlayerManager _playerManager = null!;
    [Dependency] private IPrototypeManager _prototypeManager = null!;
    [Dependency] private IConfigurationManager _configuration = null!;
    [Dependency] private IResourceCache _resourceCache = null!;
    [Dependency] private IRobustRandom _random = null!;

    private readonly SharedSolutionContainerSystem _solutionContainer;
    private readonly StatusEffectsSystem _statusEffects;
    private readonly ShaderInstance _shader;
    private readonly Texture[] _openingFrames;
    private readonly Texture[] _idleFrames;
    private readonly List<EdgeEye> _eyes = new();

    private float _currentIntensity;
    private float _targetStage;
    private float _mnemolithAmount;
    private float _nextEye;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace | OverlaySpace.ScreenSpace;

    private const float EyeFrameTime = 0.08f;
    private float OpeningDuration => _openingFrames.Length * EyeFrameTime;

    public MnemolithOverlay()
    {
        IoCManager.InjectDependencies(this);
        _solutionContainer = _entityManager.System<SharedSolutionContainerSystem>();
        _statusEffects = _entityManager.System<StatusEffectsSystem>();
        _shader = _prototypeManager.Index(MnemolithShader).InstanceUnique();
        var eyeRsi = _resourceCache.GetResource<RSIResource>(new ResPath("/Textures/_AS/Effects/eye.rsi")).RSI;
        _openingFrames = eyeRsi["open"].GetFrames(RsiDirection.South);
        _idleFrames = eyeRsi["idle"].GetFrames(RsiDirection.South);
        ZIndex = 8;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _mnemolithAmount = GetMnemolithAmount();
        // mnemolith ranges
        _targetStage = _mnemolithAmount switch
        {
            >= 44f => 3f, >= 29f => 2f, >= 14f => 1f, _ => 0f,
        };

        var hasLingeringEffect = _statusEffects.HasEffectComp<MnemolithStatusEffectComponent>(_playerManager.LocalEntity);
        var target = _mnemolithAmount > 0f ? DoseToIntensity(_mnemolithAmount) : hasLingeringEffect ? 0.025f : 0f;

        var speed = target > _currentIntensity
            ? 1.6f
            : target <= 0f ? 0.9f : 2.8f;
        _currentIntensity = MathHelper.Lerp(_currentIntensity, target, Math.Clamp(args.DeltaSeconds * speed, 0f, 1f));

        if (_currentIntensity < 0.001f)
            _currentIntensity = 0f;

        UpdateEyes(args.DeltaSeconds);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var player = _playerManager.LocalEntity;
        return player != null && _currentIntensity > 0.001f && _entityManager.TryGetComponent<EyeComponent>(player.Value, out var eye) && args.Viewport.Eye == eye.Eye;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Space == OverlaySpace.ScreenSpace)
        {
            DrawEyes(args);
            return;
        }

        if (ScreenTexture == null)
            return;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("Intensity", _currentIntensity);
        _shader.SetParameter("Stage", _targetStage);
        _shader.SetParameter("ReducedMotion", _configuration.GetCVar(CCVars.ReducedMotion) ? 1f : 0f);

        var handle = args.WorldHandle;
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    private void UpdateEyes(float frameTime)
    {
        if (_configuration.GetCVar(ASCCVars.Scopophobia))
        {
            _eyes.Clear();
            _nextEye = 0f;
            return;
        }

        for (var i = _eyes.Count - 1; i >= 0; i--)
        {
            var eye = _eyes[i];
            eye.Elapsed += frameTime;

            // If the dose drops below the overdose threshold, every open eye closes immediately.
            if (_mnemolithAmount < 14f && eye.Elapsed < OpeningDuration + eye.IdleDuration)
                eye.Elapsed = OpeningDuration + eye.IdleDuration;

            if (eye.Elapsed >= OpeningDuration * 2f + eye.IdleDuration)
                _eyes.RemoveAt(i);
        }

        if (_mnemolithAmount < 14f)
        {
            _nextEye = 0f;
            return;
        }

        _nextEye -= frameTime;
        var maxEyes = _targetStage switch
        {
            >= 3f => 20, >= 2f => 12, _ => 6,
        };

        if (_nextEye > 0f || _eyes.Count >= maxEyes)
            return;

        SpawnEye();
        _nextEye = _targetStage switch
        {
            >= 3f => _random.NextFloat(0.10f, 0.25f),
            >= 2f => _random.NextFloat(0.20f, 0.42f),
            _ => _random.NextFloat(0.35f, 0.70f),
        };
    }

    private void SpawnEye()
    {
        // Try several positions so eyes scatter around without overlapping.
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var side = _random.Next(4);
            var alongEdge = _random.NextFloat(0.035f, 0.965f);
            var inset = _random.NextFloat(0.055f, 0.22f);
            var position = side switch
            {
                0 => new Vector2(alongEdge, inset),
                1 => new Vector2(1f - inset, alongEdge),
                2 => new Vector2(alongEdge, 1f - inset),
                _ => new Vector2(inset, alongEdge),
            };

            var intensityScale = 0.90f + _currentIntensity * 0.45f;
            var scale = _random.NextFloat(0.95f, 1.40f) * intensityScale;
            if (!CanPlaceEye(position, scale))
                continue;

            _eyes.Add(new EdgeEye
            {
                Position = position,
                Scale = scale,
                // Every eye has a short life of its own: open, stare/jitter, then close.
                IdleDuration = _targetStage >= 3f ? _random.NextFloat(0.65f, 2.7f) : _random.NextFloat(0.45f, 1.25f),
            });
            return;
        }

        // Skip this spawn rather than allowing an overlap.
    }

    private bool CanPlaceEye(Vector2 position, float scale)
    {
        foreach (var eye in _eyes)
        {
            var minimumDistance = 0.05f * (scale + eye.Scale);
            if (Vector2.DistanceSquared(position, eye.Position) < minimumDistance * minimumDistance)
                return false;
        }

        return true;
    }

    private void DrawEyes(in OverlayDrawArgs args)
    {
        var width = args.ViewportBounds.Width;
        var height = args.ViewportBounds.Height;
        var screen = args.ScreenHandle;

        foreach (var eye in _eyes)
        {
            Texture frame;
            if (eye.Elapsed < OpeningDuration)
            {
                var index = Math.Min((int) (eye.Elapsed / EyeFrameTime), _openingFrames.Length - 1);
                frame = _openingFrames[index];
            }
            else if (eye.Elapsed < OpeningDuration + eye.IdleDuration)
            {
                var idleTime = eye.Elapsed - OpeningDuration;
                var index = (int) (idleTime / EyeFrameTime) % _idleFrames.Length;
                frame = _idleFrames[index];
            }
            else
            {
                var closingTime = eye.Elapsed - OpeningDuration - eye.IdleDuration;
                var index = _openingFrames.Length - 1 - Math.Min((int) (closingTime / EyeFrameTime), _openingFrames.Length - 1);
                frame = _openingFrames[index];
            }

            var size = frame.Size * eye.Scale;
            var center = new Vector2(eye.Position.X * width, eye.Position.Y * height);
            var glowColor = Color.FromHex("#a94dff55");
            var innerGlowColor = Color.FromHex("#c77dff88");
            var outerGlowSize = size * 1.55f;
            var innerGlowSize = size * 1.24f;
            screen.DrawTextureRect(frame, UIBox2.FromDimensions(center - outerGlowSize / 2f, outerGlowSize), glowColor);
            screen.DrawTextureRect(frame, UIBox2.FromDimensions(center - innerGlowSize / 2f, innerGlowSize), innerGlowColor);
            screen.DrawTextureRect(frame, UIBox2.FromDimensions(center - size / 2f, size));
        }
    }

    private float GetMnemolithAmount()
    {
        var player = _playerManager.LocalEntity;
        if (player == null || !_entityManager.TryGetComponent<BloodstreamComponent>(player.Value, out var bloodstream))
            return 0f;

        return _solutionContainer.TryGetSolution(player.Value, bloodstream.BloodSolutionName, out _, out var solution) ? solution.GetReagentQuantity(Mnemolith).Float() : 0f;
    }

    private static float DoseToIntensity(float amount)
    {
        if (amount <= 0f)
            return 0f;

        return amount switch
        {
            < 14f => MathHelper.Lerp(0.025f, 0.20f, amount / 14f),
            < 29f => MathHelper.Lerp(0.28f, 0.48f, (amount - 14f) / 15f),
            < 44f => MathHelper.Lerp(0.56f, 0.74f, (amount - 29f) / 15f),
            _ => MathHelper.Lerp(0.82f, 1f, Math.Clamp((amount - 44f) / 15f, 0f, 1f)),
        };
    }

    private sealed class EdgeEye
    {
        public Vector2 Position;
        public float Scale;
        public float IdleDuration;
        public float Elapsed;
    }

}
