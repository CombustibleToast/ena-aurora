using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Administration;
using Content.Shared._AS.DeviceLinking;
using Content.Shared._AS.DeviceLinking.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.DeviceLinking.Systems;

public sealed partial class SignalConsoleSystem : EntitySystem
{
    private static readonly ProtoId<SourcePortPrototype>[] Ports =
    [
        "Console1", "Console2", "Console3", "Console4", "Console5", "Console6", "Console7", "Console8", "Console9", "Console10", "Console11", "Console12",
    ];

    [Dependency] private DeviceLinkSystem _link = null!;
    [Dependency] private SharedUserInterfaceSystem _ui = null!;
    [Dependency] private IAdminManager _admin = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SignalConsoleComponent, ActivatableUIOpenAttemptEvent>(OnUiOpenAttempt, after: [typeof(ActivatableUIRequiresPowerSystem)]);
        SubscribeLocalEvent<SignalConsoleComponent, AfterActivatableUIOpenEvent>(OnUiOpen);
        SubscribeLocalEvent<SignalConsoleComponent, SignalConsoleToggleMessage>(OnToggle);
        SubscribeLocalEvent<SignalConsoleComponent, SignalConsolePulseMessage>(OnPulse);
        SubscribeLocalEvent<SignalConsoleComponent, SignalConsoleRenameMessage>(OnRename);
        SubscribeLocalEvent<SignalConsoleComponent, SignalConsoleSetModeMessage>(OnSetMode);
        SubscribeLocalEvent<SignalConsoleComponent, LinkAttemptEvent>(OnLinkAttempt);
    }

    private void OnUiOpenAttempt(Entity<SignalConsoleComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        // make sure mappers can open the UI even if the console is unpowered, so they can configure the labels and modes of the channels.
        if (!args.Cancelled || LifeStage(ent.Owner) >= EntityLifeStage.MapInitialized || !_admin.HasAdminFlag(args.User, AdminFlags.Mapping))
            return;

        // i know what im doing
        args.Uncancel();
    }

    private void OnUiOpen(Entity<SignalConsoleComponent> ent, ref AfterActivatableUIOpenEvent args)
    {
        UpdateUi(ent);
    }

    private void OnToggle(Entity<SignalConsoleComponent> ent, ref SignalConsoleToggleMessage args)
    {
        Normalize(ent.Comp);
        if (!ValidChannel(args.Channel) || ent.Comp.Modes[args.Channel] != SignalConsoleChannelMode.Toggle)
            return;

        ent.Comp.States[args.Channel] = !ent.Comp.States[args.Channel];
        _link.SendSignal(ent.Owner, Ports[args.Channel], ent.Comp.States[args.Channel]);
        Dirty(ent);
        UpdateUi(ent);
    }

    private void OnPulse(Entity<SignalConsoleComponent> ent, ref SignalConsolePulseMessage args)
    {
        Normalize(ent.Comp);
        if (!ValidChannel(args.Channel) || ent.Comp.Modes[args.Channel] != SignalConsoleChannelMode.Pulse)
            return;

        _link.InvokePort(ent.Owner, Ports[args.Channel]);
    }

    private void OnRename(Entity<SignalConsoleComponent> ent, ref SignalConsoleRenameMessage args)
    {
        if (!ValidChannel(args.Channel))
            return;

        Normalize(ent.Comp);
        var label = args.Label.Trim();
        ent.Comp.Labels[args.Channel] = string.IsNullOrEmpty(label) ? $"Channel {args.Channel + 1}" : label[..Math.Min(label.Length, SignalConsoleComponent.MaximumLabelLength)];
        Dirty(ent);
        UpdateUi(ent);
    }

    private void OnSetMode(Entity<SignalConsoleComponent> ent, ref SignalConsoleSetModeMessage args)
    {
        if (!ValidChannel(args.Channel) || !Enum.IsDefined(args.Mode))
            return;

        Normalize(ent.Comp);
        if (ent.Comp.Modes[args.Channel] == args.Mode)
            return;

        // A pulse
        if (ent.Comp.States[args.Channel])
        {
            ent.Comp.States[args.Channel] = false;
            _link.SendSignal(ent.Owner, Ports[args.Channel], false);
        }

        ent.Comp.Modes[args.Channel] = args.Mode;
        Dirty(ent);
        UpdateUi(ent);
    }

    private void OnLinkAttempt(EntityUid uid, SignalConsoleComponent _, LinkAttemptEvent args)
    {
        if (!TryComp<DeviceLinkSourceComponent>(uid, out var source))
            return;

        // haha who let the sink in
        var sinkLinks = _link.GetLinks(uid, args.Sink, source);
        if (sinkLinks.Any(link => link.source == args.SourcePort && link.sink == args.SinkPort))
            return;

        var linkedSinks = new HashSet<EntityUid>();
        foreach (var port in Ports)
        {
            linkedSinks.UnionWith(_link.GetLinkedSinks((uid, source), port));
        }

        var linkCount = linkedSinks.Sum(sink => _link.GetLinks(uid, sink, source).Count);
        if (linkCount >= SignalConsoleComponent.MaximumChannels)
            args.Cancel();
    }

    private void UpdateUi(Entity<SignalConsoleComponent> ent)
    {
        Normalize(ent.Comp);
        var source = Comp<DeviceLinkSourceComponent>(ent.Owner);
        var linked = Ports
            .Select(port => _link.GetLinkedSinks((ent.Owner, source), port).Count > 0)
            .ToList();
        _ui.SetUiState(ent.Owner,
            SignalConsoleUiKey.Key,
            new SignalConsoleBoundUserInterfaceState(
                [..ent.Comp.Labels],
                [..ent.Comp.States],
                [..ent.Comp.Modes],
                linked));
    }

    private static void Normalize(SignalConsoleComponent component)
    {
        while (component.Labels.Count < SignalConsoleComponent.MaximumChannels)
            component.Labels.Add($"Channel {component.Labels.Count + 1}");
        while (component.States.Count < SignalConsoleComponent.MaximumChannels)
            component.States.Add(false);
        while (component.Modes.Count < SignalConsoleComponent.MaximumChannels)
            component.Modes.Add(SignalConsoleChannelMode.Pulse);

        if (component.Labels.Count > SignalConsoleComponent.MaximumChannels)
            component.Labels.RemoveRange(SignalConsoleComponent.MaximumChannels, component.Labels.Count - SignalConsoleComponent.MaximumChannels);
        if (component.States.Count > SignalConsoleComponent.MaximumChannels)
            component.States.RemoveRange(SignalConsoleComponent.MaximumChannels, component.States.Count - SignalConsoleComponent.MaximumChannels);
        if (component.Modes.Count > SignalConsoleComponent.MaximumChannels)
            component.Modes.RemoveRange(SignalConsoleComponent.MaximumChannels, component.Modes.Count - SignalConsoleComponent.MaximumChannels);
    }

    private static bool ValidChannel(int channel)
    {
        return channel is >= 0 and < SignalConsoleComponent.MaximumChannels;
    }
}
