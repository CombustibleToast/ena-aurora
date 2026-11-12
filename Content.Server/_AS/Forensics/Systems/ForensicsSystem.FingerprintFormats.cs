using System.Linq;
using Content.Shared._AS.Forensics.Components;

namespace Content.Server.Forensics;

public sealed partial class ForensicsSystem
{
    private static string FormatFingerprint(string fingerprint, FingerprintFormat format)
    {
        return format switch
        {
            FingerprintFormat.HardwareId => FormatHardwareId(fingerprint),
            _ => fingerprint,
        };
    }

    // Makes a hardware ID styled fingerprint for inorganic machine species, no this is not the actual player's HWID.
    private static string FormatHardwareId(string fingerprint)
    {
        return $"{string.Join('-', Enumerable.Range(0, 4)
            .Select(i => fingerprint.Substring(i * 4, 4)))}";
    }
}
