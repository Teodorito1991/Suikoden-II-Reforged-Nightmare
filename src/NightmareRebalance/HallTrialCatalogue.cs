namespace NightmareRebalance;

internal sealed record HallTrialDefinition(string Id, string Name, string TableId,
    string PartyId, int Hp, string? PackId, string SceneId, string Description, string Rewards,
    bool RetainCombatMain = false, bool RetainPhaseCallbacks = false, bool RetainCommand = false);

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
            "Native Potch · No native item drop", true),
        new("bone-dragon", "Bone Dragon", "boss_dor", "boss_dor_00", 8900, "dor", "bge2",
            "Level 56 foe · Heavy native attacks. Bring a durable front line and healing.", "Native Potch · Original loot", true),
        new("celestial-sword", "Celestial Sword", "boss_sei", "boss_sei_00", 2500, "sei", "bgd3",
            "Level 32 foe · A focused duel with the original animated sword. Protect fragile allies.", "Practice trial · No native Potch or item drop", true),
        new("neclord-final", "Neclord · Final Encounter", "boss_nec3", "boss_nec3_00", 4500, "nec3", "bgg9",
            "Level 50 foe · The vulnerable final encounter. Prepare magical defense and recovery.", "Native Potch · Original loot", true),
        new("lucia-retinue", "Lucia's Retinue", "boss_lus1", "boss_lus1_00", 4700, null, "bge5",
            "Lucia and three Karayan soldiers. Control the supporting fighters before focusing Lucia.", "Native Potch · Original loot", RetainCommand:true),
        new("lucia-highland", "Lucia · Highland", "boss_lus2", "boss_lus2_00", 6500, null, "bgj2",
            "Level 58 foe · Physical pressure and limited native magic. Keep recovery in reserve.", "Native Potch · Original loot", RetainCommand:true),
        new("gorudo", "Gorudo's Guard", "boss_gor", "boss_gor_00", 4500, null, "bgi4",
            "Gorudo and five Matilda knights. Area attacks and a resilient party help break the guard.", "Native Potch · Original loot", RetainCommand:true),
        new("seed-culgan", "Seed & Culgan", "boss_kul_sid", "boss_kul_sid_00", 4200, "kul_sid", "bgj5",
            "Two level 65 commanders. Manage magical and physical pressure; choose your first target.", "Native Potch · Original loot"),
        new("double-head", "Double Head", "boss_ryo", "boss_ryo_00", 2500, "ryo", "bgc5",
            "Level 25 foe · Original alternating battle-local phases. Keep the party healthy.", "Native Potch · Original loot", true, true),
        new("mist-shade", "Mist Shade", "boss_ki1", "boss_ki1_00", 800, "ki1", "bgb6",
            "Level 10 foe · Retains the original changing forms. A short early-game memory.", "Native Potch · Original loot", true, true),
        new("lucia-castle", "Lucia · Castle", "boss_lus3", "boss_lus3_00", 1500, null, "bgk1",
            "Level 49 foe · A distinct, shorter duel with a single limited magic attack.", "Native Potch · Original loot", RetainCommand:true),
        new("rowd-patrol", "Rowd's Patrol", "boss_rau1", "boss_rau1_00", 3000, null, "bga3",
            "Rowd and five Highland soldiers. An early bonus memory; break the formation.", "Native Potch · Original loot"),
        new("worm-moth", "Worm & Poison Moth", "boss_imo2", "boss_imo2_00", 6800, "imo2", "bgf1",
            "Original Worm-to-Moth transformation. Respect native phase timing and limited moves.",
            "Native rewards · One conditional Nightmare stone offer", true, true, true),
        new("beast-rune", "Beast Rune Incarnation", "boss_las", "boss_las_00", 5700, "las", "bgx0",
            "Five coordinated body parts. Native barriers, phase changes and targeting remain active.",
            "Native rewards · One conditional Nightmare stone offer", true, true, true)
    };

}
