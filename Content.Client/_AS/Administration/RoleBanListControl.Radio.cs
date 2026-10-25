namespace Content.Client.Administration.UI.BanList.RoleBans;

/// <summary>
/// Radio Ban UI
/// </summary>
public sealed partial class RoleBanListControl
{
    public void SetScopeHeader(string text)
    {
        BansHeader.ScopeHeader.Text = text;
    }
}
