using Content.Shared.Radio.Components;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._EE.Radio;

public sealed class IntrinsicRadioKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EncryptionChannelsChangedEvent>(OnTransmitterChannelsChanged);
        SubscribeLocalEvent<ActiveRadioComponent, EncryptionChannelsChangedEvent>(OnReceiverChannelsChanged);
    }

    private void OnTransmitterChannelsChanged(EntityUid uid, IntrinsicRadioTransmitterComponent component, EncryptionChannelsChangedEvent args)
    {
        // Aurora's Song Start - Intrinsic readonly channels
        component.ReadOnlyChannels.Clear();
        component.ReadOnlyChannels.UnionWith(args.Component.ReadOnlyChannels);
        // Aurora's Song End
        UpdateChannels(uid, args.Component, ref component.Channels);
    }

    private void OnReceiverChannelsChanged(EntityUid uid, ActiveRadioComponent component, EncryptionChannelsChangedEvent args)
    {
        // Aurora's Song Start - Intrinsic disabled channels
        component.DisabledChannels.Clear();
        component.DisabledChannels.UnionWith(args.Component.DisabledChannels);
        // Aurora's Song End
        UpdateChannels(uid, args.Component, ref component.Channels);
    }

    private void UpdateChannels(EntityUid _, EncryptionKeyHolderComponent keyHolderComp, ref HashSet<ProtoId<RadioChannelPrototype>> channels)
    {
        channels.Clear();
        channels.UnionWith(keyHolderComp.Channels);
    }
}
