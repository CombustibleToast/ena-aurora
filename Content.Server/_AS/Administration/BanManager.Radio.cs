using System.Globalization;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration.Notes;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Shared.Administration.Notes;
using Content.Shared.Database;
using Content.Shared.Radio;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.Managers;

public interface IRadioBanManager
{
    bool TryGetRadioBanExpiration(ICommonSession player, ProtoId<RadioChannelPrototype> channel, out DateTimeOffset? expires);
}

/// <summary>
/// Caching, logging, and pardoning for radio bans.
/// </summary>
public sealed partial class BanManager : IRadioBanManager
{
    [Dependency] private IAdminNotesManager _radioBanNotes = default!;
    [Dependency] private EuiManager _radioBanEuis = default!;

    public const string DbTypeRadioChannel = "RadioChannel";

    private readonly Dictionary<ICommonSession, List<BanDef>> _cachedRadioBans = new();

    public async void CreateRadioBan(CreateRadioBanInfo banInfo)
    {
        if (banInfo.Channels.Count == 0)
            throw new ArgumentException("Must specify at least one radio channel to ban!");

        var channelDefs = banInfo.Channels.Select(channel =>
        {
            if (!_prototypeManager.HasIndex(channel))
                throw new UnknownPrototypeException(channel, typeof(RadioChannelPrototype));
            return new BanRoleDef(DbTypeRadioChannel, channel);
        }).ToImmutableArray();

        var (banDef, expires) = await CreateBanDef(banInfo, BanType.Radio, channelDefs);
        banDef = await _db.AddBanAsync(banDef);

        foreach (var userId in banDef.UserIds)
        {
            if (_playerManager.TryGetSessionById(userId, out var session) &&
                _cachedRadioBans.TryGetValue(session, out var cached))
                cached.Add(banDef);
        }

        var targetName = banInfo.Users.Count == 0 ? "null" : string.Join(", ", banInfo.Users.Select(user => $"{user.UserName} ({user.UserId})"));
        var length = expires == null ? Loc.GetString("cmd-roleban-inf") : Loc.GetString("cmd-roleban-until", ("expires", expires));
        var logMessage = $"{banInfo.BanningAdmin?.ToString() ?? "SYSTEM"} created radio ban #{banDef.Id} for {targetName} on {string.Join(", ", banInfo.Channels)} ({length}). Severity: {banDef.Severity}. Reason: {banInfo.Reason}";
        _sawmill.Info(logMessage);
        _chat.SendAdminAlert(logMessage);

        try
        {
            await NotifyRadioBanTargets(banInfo, expires);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to notify players about radio ban #{banDef.Id}: {e}");
        }
    }

    private async Task NotifyRadioBanTargets(CreateRadioBanInfo banInfo, DateTimeOffset? expires)
    {
        var channels = string.Join(", ", banInfo.Channels.Select(channel =>
            _prototypeManager.Index(channel).LocalizedName));
        var duration = expires is null
            ? Loc.GetString("radio-ban-notification-permanent")
            : Loc.GetString("radio-ban-notification-until", ("expires", expires.Value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)));
        var message = Loc.GetString("radio-ban-notification",
            ("channels", channels),
            ("duration", duration),
            ("reason", banInfo.Reason));

        foreach (var (userId, _) in banInfo.Users)
        {
            if (banInfo.BanningAdmin is { } adminId &&
                _playerManager.TryGetSessionById(adminId, out var adminSession))
            {
                await _radioBanNotes.AddAdminRemark(adminSession,
                    userId,
                    NoteType.Message,
                    message,
                    null,
                    false,
                    null);
            }
            else
            {
                continue;
            }

            if (!_playerManager.TryGetSessionById(userId, out var targetSession))
                continue;

            var messages = (await _radioBanNotes.GetNewMessages(userId))
                .Where(adminMessage => !adminMessage.Dismissed)
                .OrderBy(adminMessage => adminMessage.CreatedAt)
                .ToArray();
            if (messages.Length > 0)
                _radioBanEuis.OpenEui(new AdminMessageEui(messages), targetSession);
        }
    }

    public HashSet<ProtoId<RadioChannelPrototype>>? GetRadioBans(NetUserId playerUserId)
    {
        if (!_playerManager.TryGetSessionById(playerUserId, out var session) ||
            !_cachedRadioBans.TryGetValue(session, out var bans))
            return null;

        return bans
            .Where(ban => ban.Unban == null && (ban.ExpirationTime == null || ban.ExpirationTime > DateTimeOffset.Now))
            .SelectMany(ban => ban.Roles ?? [])
            .Where(role => role.RoleType == DbTypeRadioChannel)
            .Select(role => new ProtoId<RadioChannelPrototype>(role.RoleId))
            .ToHashSet();
    }

    public bool IsRadioBanned(ICommonSession player, ProtoId<RadioChannelPrototype> channel) =>
        GetRadioBans(player.UserId)?.Contains(channel) == true;

    /// <summary>
    /// Gets the end of all active restrictions on a channel. Null means a permanent ban.
    /// </summary>
    public bool TryGetRadioBanExpiration(ICommonSession player, ProtoId<RadioChannelPrototype> channel, out DateTimeOffset? expires)
    {
        expires = null;
        if (!_cachedRadioBans.TryGetValue(player, out var bans))
            return false;

        var found = false;
        var now = DateTimeOffset.UtcNow;
        foreach (var ban in bans)
        {
            if (ban.Unban != null || ban.ExpirationTime <= now ||
                ban.Roles?.Any(role => role.RoleType == DbTypeRadioChannel && role.RoleId == channel.Id) != true)
                continue;

            found = true;
            if (ban.ExpirationTime is not { } expiration)
            {
                expires = null;
                return true;
            }

            if (expires == null || expiration > expires)
                expires = expiration;
        }

        return found;
    }

}
