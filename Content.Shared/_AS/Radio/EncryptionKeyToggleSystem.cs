using System.Linq;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Radio;

/// <summary>
/// This shit makes encryption keys awesome
/// </summary>
public sealed partial class EncryptionKeyToggleSystem : EntitySystem
{
    [Dependency] private IPrototypeManager ProtoMan = null!;
    [Dependency] private SharedPopupSystem _popup = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EncryptionKeyHolderComponent, GetVerbsEvent<InnateVerb>>(OnToggleKeyIntrinsic);
        SubscribeLocalEvent<EncryptionKeyHolderComponent, InventoryRelayedEvent<GetVerbsEvent<InnateVerb>>>(OnToggleKey);
    }

    private void OnToggleKeyIntrinsic(Entity<EncryptionKeyHolderComponent> ent, ref GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanComplexInteract)
            return;

        var user = args.User;

        // Make sure only the wearer/owner can change it
        var intrinsic = HasComp<IntrinsicRadioReceiverComponent>(args.Target) ? user : ent.Owner;

        if (intrinsic != args.Target)
            return;

        var priority = 0;

        // We need to put these in backwards
        var verbs = ent.Comp.Channels.Reverse()
            .Select(channel => ProtoMan.Index(channel))
            .Select(channelProto => new InnateVerb
            {
                Priority = priority++,
                Category = VerbCategory.ToggleChannel,
                Text = Loc.GetString("encryption-keys-toggle-button", ("color", channelProto.Color), ("channel", channelProto.LocalizedName), ("enabled", !ent.Comp.DisabledChannels.Contains(channelProto.ID))),
                Act = () =>
                {
                    var added = ent.Comp.DisabledChannels.Add(channelProto.ID);

                    if (!added)
                        ent.Comp.DisabledChannels.Remove(channelProto.ID);

                    var ev = new EncryptionChannelsChangedEvent(ent.Comp);
                    RaiseLocalEvent(ent, ev);
                    _popup.PopupClient(Loc.GetString("encryption-keys-popup-toggle", ("enabled", !added), ("channel", channelProto.LocalizedName)), ent, user);
                },
            })
            .ToArray();

        var disableAll = new InnateVerb
        {
            Priority = priority++,
            Category = VerbCategory.ToggleChannel,
            Text = Loc.GetString("encryption-keys-toggle-disable-all"),
            Act = () =>
            {
                ent.Comp.DisabledChannels.Clear();
                ent.Comp.DisabledChannels.UnionWith(ent.Comp.Channels);
                var ev = new EncryptionChannelsChangedEvent(ent.Comp);
                RaiseLocalEvent(ent, ev);
                _popup.PopupClient(Loc.GetString("encryption-keys-popup-toggle-all", ("enabled", false)), ent, user);
            },
        };
        var enableAll = new InnateVerb
        {
            Priority = priority + 1,
            Category = VerbCategory.ToggleChannel,
            Text = Loc.GetString("encryption-keys-toggle-enable-all"),
            Act = () =>
            {
                ent.Comp.DisabledChannels.Clear();
                var ev = new EncryptionChannelsChangedEvent(ent.Comp);
                RaiseLocalEvent(ent, ev);
                _popup.PopupClient(Loc.GetString("encryption-keys-popup-toggle-all", ("enabled", true)), ent, user);
            },
        };

        foreach (var verb in verbs)
        {
            args.Verbs.Add(verb);
        }

        args.Verbs.Add(disableAll);
        args.Verbs.Add(enableAll);
    }

    private void OnToggleKey(Entity<EncryptionKeyHolderComponent> ent, ref InventoryRelayedEvent<GetVerbsEvent<InnateVerb>> args)
    {
        // Make sure only the wearer/owner can change it
        if (args.Owner != args.Args.User)
            return;

        OnToggleKeyIntrinsic(ent, ref args.Args);
    }
}
