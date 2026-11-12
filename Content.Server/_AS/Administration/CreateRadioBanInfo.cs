using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Managers;

[Access(typeof(BanManager), Other = AccessPermissions.Execute)]
public sealed class CreateRadioBanInfo : CreateBanInfo
{
    internal readonly HashSet<ProtoId<RadioChannelPrototype>> Channels = [];

    public CreateRadioBanInfo(string reason) : base(reason)
    {
    }

    public CreateRadioBanInfo AddChannel(ProtoId<RadioChannelPrototype> channel)
    {
        Channels.Add(channel);
        return this;
    }
}
