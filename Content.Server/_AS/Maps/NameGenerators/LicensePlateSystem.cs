namespace Content.Server.Maps;

public sealed partial class LicensePlateSystem
{
    // These are the parameters that generates the license plate numbers. Changing these will cause all license plates to change.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // Exclude I and O to avoid confusion with 0 and 1
    private const uint Seed = 0x8008;

    public static string GetLicensePlate(int id)
    {
        return id < 0 ? "Unknown" : Encode(Feistel((uint)id));
    }

    /// <summary>
    /// DANGER: MATH AHEAD
    /// This is an implementation of an unbalanced Feistel cipher. This is a bit beyond what can be easily explained here.
    /// Takes the randomness of Mix32 and applies it to the domain of the number of different license plates (32^length).
    /// </summary>
    private static uint Feistel(uint x)
    {
        const int bits = 19; // 2 alpha + 3 decimal = 24^2 * 10^3 = 576000. Use the highest power of 2 that does not exceed expressible set.
        // splits the bits into two halves (called l and r)
        const int wideHalf = (bits + 1) / 2;
        const int narrowHalf = bits / 2;
        int leftWidth = wideHalf, rightWidth = narrowHalf;
        uint l = x & ((1u << leftWidth) - 1);
        // ReSharper disable once IntVariableOverflowInUncheckedContext
        uint r = (x >> leftWidth) & ((1u << rightWidth) - 1);

        // four shuffles for complex math reasons.
        for (uint i = 0; i < 4; i++)
        {
            // f is the function that determines the distribution of the bijection over the domain. Mix32 ensures that it's a random looking shuffle.
            uint f = Mix32(r ^ Seed) & ((1u << leftWidth) - 1);
            // Magic math shit
            (l, r) = (r, l ^ f);
            // flip widths for next iteration
            (leftWidth, rightWidth) = (rightWidth, leftWidth);
        }
        // recombine into one uint
        return (r << leftWidth) | l;
    }

    /// <summary>
    /// Projects the number into AADDD form.
    /// </summary>
    private static string Encode(uint input)
    {
        if (input >= 24*24*10*10*10)
            return "NULL";
        var decimalPart = (input % 1000).ToString("D3");
        input /= 1000;

        var alphaPart = $"{Alphabet[(int)input / 24]}{Alphabet[(int)input % 24]}";

        return alphaPart+decimalPart;
    }

    /// <summary>
    /// murmur3 fmix32. Constants are fine-tuned to get a very random looking shuffle.
    /// </summary>
    private static uint Mix32(uint x)
    {
        unchecked
        {
            x ^= x >> 16;
            x *= 0x85ebca6b;
            x ^= x >> 13;
            x *= 0xc2b2ae35;
            x ^= x >> 16;
            return x;
        }
    }
}
