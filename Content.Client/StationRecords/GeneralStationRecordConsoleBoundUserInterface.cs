using Content.Client._AS.StationRecords; // Aurora's Song - Replacement station records UI.
using Content.Shared.StationRecords;
using Robust.Client.UserInterface;
using Content.Shared._NF.StationRecords; // Frontier
using Content.Shared.Roles; // Frontier
using Robust.Shared.Prototypes; // Frontier

namespace Content.Client.StationRecords;

public sealed class GeneralStationRecordConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ASGeneralStationRecordConsoleWindow? _window; // Aurora's Song: Crew records, see ASGeneralStationRecordConsoleWindow.cs
    private ShuttleRecordViewerPermissions? _pendingPermissions; // Aurora's Song - Per-viewer shuttle records permissions.

    public GeneralStationRecordConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ASGeneralStationRecordConsoleWindow>(); // Aurora's Song: Hijack the window to use our custom one for shuttle crew records.
        _window.OnKeySelected += key =>
            SendMessage(new SelectStationRecord(key));
        _window.OnFiltersChanged += (type, filterValue) =>
            SendMessage(new SetStationRecordFilter(type, filterValue));
        _window.OnJobAdd += OnJobsAdd; // Frontier: job modification buttons
        _window.OnJobSubtract += OnJobsSubtract; // Frontier: job modification buttons
        _window.OnDeleted += id => SendMessage(new DeleteStationRecord(id));
        _window.OnAdvertisementChanged += OnAdvertisementChanged; // Frontier: job modification buttons
        // Aurora's Song Start: Shuttle crew records messages.
        _window.OnShuttleNoteAdded += (profileId, title, description) => SendMessage(new AddShuttleRecordNote(profileId, title, description));
        _window.OnShuttleNoteDeleted += (profileId, id) => SendMessage(new DeleteShuttleRecordNote(profileId, id));
        _window.OnShuttleCrewUpdated += (id, minutes) => SendMessage(new UpdateShuttleCrewRecord(id, minutes));
        _window.OnShuttleCrewRestored += id => SendMessage(new RestoreShuttleCrewRecord(id));
        _window.OnShuttleCrewArchived += (id, reason) => SendMessage(new ArchiveShuttleCrewRecord(id, reason));
        // Aurora's Song End

        if (_pendingPermissions != null)
        {
            _window.ApplyViewerPermissions(_pendingPermissions);
            _pendingPermissions = null;
        }
    }

    // Frontier: job modification buttons, ship advertisements
    private void OnJobsAdd(ProtoId<JobPrototype> job)
    {
        SendMessage(new AdjustStationJobMsg(job, 1));
    }
    private void OnJobsSubtract(ProtoId<JobPrototype> job)
    {
        SendMessage(new AdjustStationJobMsg(job, -1));
    }
    private void OnAdvertisementChanged(string text)
    {
        SendMessage(new SetStationAdvertisementMsg(text));
    }
    // End Frontier
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not GeneralStationRecordConsoleState cast)
            return;

        _window?.UpdateState(cast);
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (message is ShuttleRecordViewerPermissions permissions)
        {
            if (_window != null)
                _window.ApplyViewerPermissions(permissions);
            else
                _pendingPermissions = permissions;
        }
    }
}
