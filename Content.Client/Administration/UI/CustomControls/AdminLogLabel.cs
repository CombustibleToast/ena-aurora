using Content.Shared.Administration.Logs;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.CustomControls;

// Aurora's Song - Make partial
public sealed partial class AdminLogLabel : RichTextLabel
{
    public AdminLogLabel(ref SharedAdminLog log, HSeparator separator)
    {
        Log = log;
        Separator = separator;

        SetMessage($"{log.Date:HH:mm:ss}: {log.Message}");
        OnVisibilityChanged += VisibilityChanged;
    }

    public new SharedAdminLog Log { get; private set; } // Aurora's Song - Sensitive admin log revealing

    public HSeparator Separator { get; }

    private void VisibilityChanged(Control control)
    {
        Separator.Visible = Visible;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        OnVisibilityChanged -= VisibilityChanged;
    }
}
