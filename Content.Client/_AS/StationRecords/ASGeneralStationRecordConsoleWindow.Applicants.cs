// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._AS.StationRecords;

// I just straight up recoded the applicant panel
public sealed partial class ASGeneralStationRecordConsoleWindow
{
    private const int AdvertisementCharacterLimit = 500;

    public event Action<ProtoId<JobPrototype>>? OnJobAdd;
    public event Action<ProtoId<JobPrototype>>? OnJobSubtract;
    public event Action<string>? OnAdvertisementChanged;

    private string _serverAdvertisement = string.Empty;
    private bool _advertisementIsDirty;

    private void InitializeApplicantsPanel()
    {
        AdTextBox.Placeholder = new Rope.Leaf(Loc.GetString(
            "general-station-record-console-ad-default-text",
            ("size", AdvertisementCharacterLimit)));
        AdTextBox.OnTextChanged += _ => SetAdvertisementDirty(ReadAdvertisement() != _serverAdvertisement);
        AdSubmitButton.OnPressed += _ => SubmitAdvertisement();
    }

    private void RefreshApplicantsPanel(GeneralStationRecordConsoleState state)
    {
        JobListing.Visible = state.JobList != null;
        if (state.JobList != null)
            RebuildApplicantRows(state.JobList);

        AdContainer.Visible = state.Advertisement != null;
        if (state.Advertisement == null)
            return;

        _serverAdvertisement = state.Advertisement;
        if (!_advertisementIsDirty && !AdTextBox.HasKeyboardFocus())
            AdTextBox.TextRope = new Rope.Leaf(_serverAdvertisement);
    }

    private void RebuildApplicantRows(IReadOnlyDictionary<ProtoId<JobPrototype>, int?> capacities)
    {
        JobListing.RemoveAllChildren();
        foreach (var (jobId, capacity) in capacities)
        {
            if (capacity is null or < 0)
                continue;

            var displayedName = _prototypeManager.TryIndex(jobId, out var prototype)
                ? prototype.LocalizedName
                : jobId.Id;
            JobListing.AddChild(CreateApplicantRow(jobId, displayedName, capacity.Value));
        }
    }

    private Control CreateApplicantRow(ProtoId<JobPrototype> jobId, string displayedName, int capacity)
    {
        var row = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, HorizontalExpand = true };
        row.AddChild(new Label { Text = displayedName, MinWidth = 210 });

        var decrease = new Button { Text = "-" };
        decrease.OnPressed += _ => OnJobSubtract?.Invoke(jobId);
        row.AddChild(decrease);
        row.AddChild(new Label { Text = capacity.ToString(), MinWidth = 22, Margin = new Thickness(6, 0, 0, 0) });

        var increase = new Button { Text = "+" };
        increase.OnPressed += _ => OnJobAdd?.Invoke(jobId);
        row.AddChild(increase);
        return row;
    }

    private void SubmitAdvertisement()
    {
        var proposed = ReadAdvertisement();
        if (proposed != _serverAdvertisement)
        {
            _serverAdvertisement = proposed;
            OnAdvertisementChanged?.Invoke(proposed);
        }

        SetAdvertisementDirty(false);
    }

    private string ReadAdvertisement()
    {
        var text = Rope.Collapse(AdTextBox.TextRope);
        return text.Length <= AdvertisementCharacterLimit
            ? text
            : text[..AdvertisementCharacterLimit];
    }

    private void SetAdvertisementDirty(bool dirty)
    {
        _advertisementIsDirty = dirty;
        AdUnsavedChanges.Visible = dirty;
        AdSubmitButton.Disabled = !dirty;
    }
}
