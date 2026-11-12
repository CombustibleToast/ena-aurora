using Content.Server.Administration;
﻿using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Radio;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Radio;

/// <summary>
/// Radio Ban Command
/// </summary>
[AdminCommand(AdminFlags.Ban)]
public sealed partial class RadioRestrictPlayerCommand : IConsoleCommand
{
    [Dependency] private IPlayerLocator _locator = default!;
    [Dependency] private IBanManager _bans = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private ILogManager _log = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private ISawmill? _sawmill;

    public string Command => "radioban";
    public string Description => Loc.GetString("cmd-radioban-desc");
    public string Help => Loc.GetString("cmd-radioban-help");

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        string target;
        string channel;
        string reason;
        uint minutes;

        if (!Enum.TryParse(_cfg.GetCVar(CCVars.RoleBanDefaultSeverity), out NoteSeverity severity))
        {
            _sawmill ??= _log.GetSawmill("admin.radio_ban");
            _sawmill.Warning("Radio ban severity could not be parsed from config! Defaulting to medium.");
            severity = NoteSeverity.Medium;
        }

        switch (args.Length)
        {
            case 3:
                target = args[0];
                channel = args[1];
                reason = args[2];
                minutes = 0;

                break;
            case 4:
                target = args[0];
                channel = args[1];
                reason = args[2];

                if (!uint.TryParse(args[3], out minutes))
                {
                    shell.WriteError(Loc.GetString("cmd-radioban-minutes-parse", ("time", args[3]), ("help", Help)));

                    return;
                }

                break;
            case 5:
                target = args[0];
                channel = args[1];
                reason = args[2];

                if (!uint.TryParse(args[3], out minutes))
                {
                    shell.WriteError(Loc.GetString("cmd-radioban-minutes-parse", ("time", args[3]), ("help", Help)));

                    return;
                }

                if (!Enum.TryParse(args[4], ignoreCase: true, out severity))
                {
                    shell.WriteLine(Loc.GetString("cmd-radioban-severity-parse", ("severity", args[4]), ("help", Help)));
                    return;
                }

                break;
            default:
                shell.WriteError(Loc.GetString("cmd-radioban-arg-count"));
                shell.WriteLine(Help);

                return;
        }

        var located = await _locator.LookupIdByNameOrIdAsync(target);
        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-radioban-name-parse"));

            return;
        }

        var targetUid = located.UserId;

        var banInfo = new CreateRadioBanInfo(reason);
        if (minutes > 0)
            banInfo.WithMinutes(minutes);
        banInfo.AddUser(targetUid, located.Username);
        banInfo.WithBanningAdmin(shell.Player?.UserId);
        banInfo.WithSeverity(severity);

        if (!_proto.HasIndex<RadioChannelPrototype>(channel))
        {
            shell.WriteError(Loc.GetString("cmd-radioban-channel-parse", ("channel", channel)));
            return;
        }

        banInfo.AddChannel(new ProtoId<RadioChannelPrototype>(channel));

        _bans.CreateRadioBan(banInfo);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var durOpts = new CompletionOption[]
        {
            new("0", Loc.GetString("cmd-roleban-hint-duration-1")),
            new("1440", Loc.GetString("cmd-roleban-hint-duration-2")),
            new("4320", Loc.GetString("cmd-roleban-hint-duration-3")),
            new("10080", Loc.GetString("cmd-roleban-hint-duration-4")),
            new("20160", Loc.GetString("cmd-roleban-hint-duration-5")),
            new("43800", Loc.GetString("cmd-roleban-hint-duration-6")),
        };

        var severities = new CompletionOption[]
        {
            new("none", Loc.GetString("admin-note-editor-severity-none")),
            new("minor", Loc.GetString("admin-note-editor-severity-low")),
            new("medium", Loc.GetString("admin-note-editor-severity-medium")),
            new("high", Loc.GetString("admin-note-editor-severity-high")),
        };

        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-radioban-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<RadioChannelPrototype>(),
                Loc.GetString("cmd-radioban-hint-2")),
            3 => CompletionResult.FromHint(Loc.GetString("cmd-radioban-hint-3")),
            4 => CompletionResult.FromHintOptions(durOpts, Loc.GetString("cmd-radioban-hint-4")),
            5 => CompletionResult.FromHintOptions(severities, Loc.GetString("cmd-radioban-hint-5")),
            _ => CompletionResult.Empty
        };
    }
}
