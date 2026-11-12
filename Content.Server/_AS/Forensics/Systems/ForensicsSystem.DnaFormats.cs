using System.Linq;
using Content.Shared._AS.Forensics.Components;

namespace Content.Server.Forensics;

public sealed partial class ForensicsSystem
{
    private string FormatDna(string dna, DnaFormat format)
    {
        return format switch
        {
            DnaFormat.MacAddress => GenerateMacAddress(),
            _ => dna,
        };
    }

    // Makes a MAC styled DNA for inorganic machine species, local/unicast so it doesn't use a real manufacturer prefix.
    private string GenerateMacAddress()
    {
        var address = new byte[6];
        _random.NextBytes(address);

        address[0] = (byte) ((address[0] | 0x02) & 0xFE);
        return string.Join(':', address.Select(value => value.ToString("X2")));
    }
}
