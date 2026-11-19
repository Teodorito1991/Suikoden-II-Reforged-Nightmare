namespace NightmareRebalance;

internal sealed record HallTrialDefinition(string Id, string Name, string TableId,
    string PartyId, int Hp, string? PackId, string SceneId, string Description, string Rewards,
    bool RetainCombatMain = false);

internal static class HallTrialCatalogue
{
    // Stable IDs and original source data; no campaign boss records are modified.
    internal static readonly HallTrialDefinition[] Entries =
    {
        new("rat", "Pest Rat", "boss_rat", "boss_rat_00", 5200, "rat", "bgh3",
            "Level 40 foe · Challenge Pest Rat at its original HP.", "Native Potch · Possible Poison Orb"),
        new("harpy", "Harpy", "boss_hap1", "boss_hap1_00", 7400, "hap", "bgi6",
            "Level 45 foe · Prepare for physical pressure and original status attacks.",
            "Native Potch · No native item drop"),
        new("golden-wolf", "Golden Wolf", "boss_gwo1", "boss_gwo1_00", 3500, null, "bgc8",
            "Level 49 foe · A swift single-opponent trial. Bring healing.",
            "Native Potch · No native item drop"),
        new("sierra", "Sierra", "boss_sie1", "boss_sie1_00", 2800, "sie", "bgg8",
            "Level 47 foe · Prepare magical protection and recovery.",
            "Native Potch · No native item drop"),
        new("harpy-second", "Harpy · Second Encounter", "boss_hap2", "boss_hap2_00", 12000, "hap", "bgi6",
            "Level 60 foe · Prepare status protection and sustained healing.",
            "Native Potch · No native item drop"),
        new("stone-golem", "Stone Golem", "boss_gol", "boss_gol_00", 7500, "gol", "bgg7",
            "Level 55 foe · Prepare for heavy physical blows and varied original attacks.",
            "Native Potch · No native item drop", true),
        new("abomination", "Abomination", "boss_nik", "boss_nik_00", 4500, "nik", "bgd5",
            "Level 34 foe · Face a mixture of physical and magical pressure. Keep healing ready.",
            "Native Potch · No native item drop", true),
        new("worm", "Worm", "boss_imo1", "boss_imo1_00", 4200, "imo1", "bgf4",
            "Level 44 foe · Watch for the original limited-use attack. Prepare steady recovery.",
            "Native Potch · No native item drop", true)
    };

}
