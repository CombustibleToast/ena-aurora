using System.Linq;
using Content.Client._Starfall.UserInterface.Lobby.Traits; // Aurora's Song
using Content.Shared.Traits;
using Robust.Shared.Utility;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{

    /// <summary>
    /// Refreshes traits selector
    /// </summary>
    public void RefreshTraits()
    {
        TraitsEditor.RefreshRequested ??= RefreshTraits; // Aurora's Song
        TraitsEditor.Clear(); // Aurora's Song

        var traits = TraitsEditor.FilterSearch(_prototypeManager.EnumeratePrototypes<TraitPrototype>()) // Aurora's Song
            .ToList();
        TabContainer.SetTabTitle(2, Loc.GetString("humanoid-profile-editor-traits-tab")); // Aurora's Song - 3>2

        if (traits.Count < 1)
        {
            TraitsEditor.ShowNoTraits(); // Aurora's Song
            return;
        }

        // Setup model
        Dictionary<string, List<string>> traitGroups = new();
        List<string> defaultTraits = new();
        traitGroups.Add(TraitCategoryPrototype.Default, defaultTraits);

        foreach (var trait in traits)
        {
            // Begin Misfit - Add trait hiding
            if (Profile?.Species is { } selectedSpecies &&
                (trait.SpeciesBlacklist.Contains(selectedSpecies) ||
                 trait.SpeciesWhitelist.Length > 0 &&
                 !trait.SpeciesWhitelist.Contains(selectedSpecies)))
            {
                Profile = Profile?.WithoutTraitPreference(trait.ID, _prototypeManager);
                continue;
            }
            // End Misfit

            if (trait.Category == null)
            {
                defaultTraits.Add(trait.ID);
                continue;
            }

            if (!_prototypeManager.HasIndex(trait.Category))
                continue;

            var group = traitGroups.GetOrNew(trait.Category);
            group.Add(trait.ID);
        }

        // Create UI view from model
        foreach (var (categoryId, categoryTraits) in traitGroups)
        {
            TraitCategoryPrototype? category = null;

            if (categoryId != TraitCategoryPrototype.Default)
                category = _prototypeManager.Index<TraitCategoryPrototype>(categoryId);

            var traitGrid = TraitsEditor.AddCategory(category); // Aurora's Song
            List<StarfallTraitCard?> selectors = new(); // Aurora's Song
            var selectionCount = 0;

            foreach (var traitProto in categoryTraits)
            {
                var trait = _prototypeManager.Index<TraitPrototype>(traitProto);
                var selector = new StarfallTraitCard(trait); // Aurora's Song

                selector.Preference = Profile?.TraitPreferences.Contains(trait.ID) == true;
                if (selector.Preference)
                    selectionCount += trait.Cost;

                selector.PreferenceChanged += preference =>
                {
                    if (preference)
                    {
                        Profile = Profile?.WithTraitPreference(trait.ID, _prototypeManager);
                    }
                    else
                    {
                        Profile = Profile?.WithoutTraitPreference(trait.ID, _prototypeManager);
                    }

                    SetDirty();
                    RefreshTraits(); // If too many traits are selected, they will be reset to the real value.
                };
                selectors.Add(selector);
            }

            // Selection counter
            if (category is { MaxTraitPoints: >= 0 })
            {
                TraitsEditor.AddPointCounter(selectionCount, category.MaxTraitPoints.Value); // Aurora's Song
            }

            TraitsEditor.AddTraitGrid(traitGrid); // Aurora's Song

            foreach (var selector in selectors)
            {
                if (selector == null)
                    continue;

                if (category is { MaxTraitPoints: >= 0 } &&
                    !selector.Preference && // Aurora's Song
                    selector.Cost + selectionCount > category.MaxTraitPoints)
                    selector.SetUnavailable(true); // Aurora's Song

                traitGrid.AddChild(selector); // Aurora's Song
            }
        }
    }
}
