namespace Content.Shared.Chat;

public static class ChatChannelExtensions
{
    public static Color TextColor(this ChatChannel channel)
    {
        return channel switch
        {
            ChatChannel.Server => Color.Orange,
            ChatChannel.Radio => Color.LimeGreen,
            ChatChannel.LOOC => Color.MediumTurquoise,
            ChatChannel.OOC => Color.LightSkyBlue,
            ChatChannel.Dead => Color.MediumPurple,
            ChatChannel.Admin => Color.Red,
            ChatChannel.AdminAlert => Color.Red,
            ChatChannel.AdminChat => Color.HotPink,
            ChatChannel.Whisper => Color.DarkGray,
            ChatChannel.Subtle => Color.FromHex("#FFBBC0"), // DEN - Add subtle/subtle OOC // Aurora's Song - Changed for accessibility
            ChatChannel.SubtleOOC => Color.FromHex("#FF7782"), // DEN - Add subtle/subtle OOC // Aurora's Song - Changed for accessibility
            _ => Color.LightGray
        };
    }
}
