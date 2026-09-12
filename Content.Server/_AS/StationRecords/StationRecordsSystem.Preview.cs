// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clothing;
using Content.Shared.Preferences;
using Content.Shared.StationRecords;

namespace Content.Server.StationRecords.Systems;

// Store the player's selected loadout for the records portrait.
public sealed partial class StationRecordsSystem
{
    private static void PopulateRecordPreview(HumanoidCharacterProfile profile, string jobId, GeneralStationRecord record)
    {
        record.ProfileId = profile.ProfileId;
        profile.Loadouts.TryGetValue(LoadoutSystem.GetJobPrototype(jobId), out var selectedLoadout);
        record.Appearance = profile.Appearance.Clone();
        record.Sex = profile.Sex;
        record.SelectedLoadout = selectedLoadout?.Clone();
    }
}
