namespace NightmareRebalance;

internal sealed record HallTrialDefinition(string Id, string Name, string TableId,
    string PartyId, int Hp, string? PackId, string SceneId, string Description, string Rewards);

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
            "Native Potch · No native item drop")
    };
}
