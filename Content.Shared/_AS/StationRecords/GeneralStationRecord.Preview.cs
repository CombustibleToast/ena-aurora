// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Humanoid;
using Content.Shared.Preferences.Loadouts;

// Intentional to extend its partial type.
namespace Content.Shared.StationRecords;

public sealed partial record GeneralStationRecord
{
    [DataField]
    public int? ProfileId;

    [DataField]
    public HumanoidCharacterAppearance? Appearance;

    // Sex.
    [DataField]
    public Sex Sex;

    [DataField]
    public RoleLoadout? SelectedLoadout;
}
