using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Humanoid;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Humanoid;

/// <summary>
/// Unary coloration strategy that returns sheleg skin tones, with 0 being lightest and 100 being darkest
///
/// A lot of these functions are from NF
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class ShelegTonedSkinColoration : ISkinColorationStrategy
{
    [DataField]
    public Color ValidHumanSkinTone = Color.FromHsv(new Vector4(210f / 360f, 0.5f, 0.8f, 1f));

    public SkinColorationStrategyInput InputType => SkinColorationStrategyInput.Unary;

    public bool VerifySkinColor(Color color, [NotNullWhen(false)] out string? reason)
    {
        reason = null;

        var colorValues = Color.ToHsv(color);

        var hue = Math.Round(colorValues.X * 360f);
        var sat = Math.Round(colorValues.Y * 100f);
        var val = Math.Round(colorValues.Z * 100f);
        // rangeOffset makes it so that this value
        // is 210 <= hue <= 220
        if (hue < 210 || hue > 220)
        {
            reason = $"Hue {hue} is outside of expected ranges 210 and 220.";
            return false;
        }

        // rangeOffset makes it so that these two values
        // are 50 <= sat <= 100 and 20 <= val <= 100
        // where saturation increases to 100 and value decreases to 20
        if (sat < 50 || val < 20)
        {
            reason = "Saturation or value are below expected number of 50 or 20 respectively.";
            return false;
        }

        return true;
    }

    public Color ClosestSkinColor(Color color)
    {
        return ValidHumanSkinTone;
    }

    public Color FromUnary(float color)
    {
        // 0 - 100, 0 being light blue and 100 being dark blue
        // HSV based
        //
        // 0 - 20 changes the hue
        // 20 - 100 changes the value
        // 0 is 220 - 50 - 100
        // 20 is 210 - 50 - 100
        // 100 is 210 - 100 - 20

        var tone = Math.Clamp(color, 0, 100);

        var rangeOffset = tone - 20;

        float hue = 210;
        float sat = 50;
        float val = 100;

        if (rangeOffset <= 0)
        {
            hue += Math.Abs(rangeOffset) / 2; // Slight hue shift for lighter tones
        }
        else
        {
            sat += rangeOffset / 2;
            val -= rangeOffset;
        }

        return Color.FromHsv(new Vector4(hue / 360, sat / 100, val / 100, 1.0f));
    }

    public float ToUnary(Color color)
    {
        var hsv = Color.ToHsv(color);
        // check for hue/value first, if hue is lower than this percentage
        // and value is 1.0
        // then it'll be hue
        if (Math.Clamp(hsv.X, 210f / 360f, 220f / 360f) > 210f / 360f
            && hsv.Z == 1.0)
        {
            return Math.Abs(220 - (hsv.X * 360));
        }
        // otherwise it'll directly be the saturation
        else
        {
            return hsv.Y * 100;
        }
    }
}
