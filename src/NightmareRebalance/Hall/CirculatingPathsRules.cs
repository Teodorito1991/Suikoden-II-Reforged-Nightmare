namespace NightmareRebalance;

internal sealed record CirculatingFloorSpec(
    int Floor,
    string Name,
    string Atmosphere,
    string AlcovePrompt,
    string InscriptionLore,
    int OrbsOnFloor,
    string GuardianId,
    string GuardianName,
    string TableId,
    string PartyId,
    int BaseHp,
    string? PackId,
    string SceneId,
    bool RetainCombatMain,
    bool RetainPhaseCallbacks,
    double HpMultiplier,
    double DamageMultiplier,
    int Shields,
    int ChestPotch,
    int ChestCommendations)
{
    internal HallTrialDefinition ToGuardianTrial() => new(
        "sajah-floor-" + Floor, GuardianName, TableId, PartyId, BaseHp, PackId, SceneId,
        $"Tower of Circulating Paths · Floor {Floor}/6",
        "Unlocks the stairwell to the next floor",
        RetainCombatMain, RetainPhaseCallbacks, false);
}

// Pure rules for the restored cut Sajah Village late-game dungeon:
// "The Tower of Circulating Paths" (6 floors, 8 Red Orbs, 6 Stairwell Guardians,
// and the Rune of Circulating Paths summit crystal).
internal static class CirculatingPathsRules
{
    internal const int TotalFloors = 6;
    internal const int TotalRedOrbs = 8;
    internal const int MaxCycleRank = 5;

    internal static readonly CirculatingFloorSpec[] Floors =
    {
        new(1, "Floor I — Beneath the Sajah Boulder",
            "Cold Sindar stonework stretches upward behind the sealed boulder of Sajah Village. Faint crimson lights pulse in the alcoves.",
            "You pry open a weathered serpent niche and find two glowing Red Orbs pulsing with Sindar warmth!",
            "Inscription I: \"Behind the quiet village of Sajah, the Sindar built a spire that bends time back upon itself.\"",
            OrbsOnFloor: 2, GuardianId: "sajah-rat", GuardianName: "Sindar Plague King",
            TableId: "boss_rat", PartyId: "boss_rat_00", BaseHp: 5200, PackId: "rat", SceneId: "bgh3",
            RetainCombatMain: false, RetainPhaseCallbacks: false,
            HpMultiplier: 2.20, DamageMultiplier: 1.15, Shields: 4, ChestPotch: 8000, ChestCommendations: 2),

        new(2, "Floor II — Corridor of the Serpent",
            "Massive twin-serpent pillars line the second tier. Ancient machinery hums softly behind the cracked walls.",
            "Hidden behind a rotating serpent statue lie two more Red Orbs—and a cache of Sajah silver!",
            "Inscription II: \"Eight crimson orbs seal the summit door. Only those who endure all six guardians may gaze upon the Cycle.\"",
            OrbsOnFloor: 2, GuardianId: "sajah-golem", GuardianName: "Sindar Sentinel Golem",
            TableId: "boss_gol", PartyId: "boss_gol_00", BaseHp: 7500, PackId: "gol", SceneId: "bgg7",
            RetainCombatMain: true, RetainPhaseCallbacks: false,
            HpMultiplier: 2.10, DamageMultiplier: 1.20, Shields: 5, ChestPotch: 10000, ChestCommendations: 2),

        new(3, "Floor III — Hall of Broken Mirrors",
            "Polished obsidian mirrors reflect battles that have already happened—and paths never taken.",
            "Beneath a shattered obsidian mirror, you recover the fifth Red Orb!",
            "Inscription III: \"The shops of Sajah still stock relics for travelers who return from the summit, though few ever remember why.\"",
            OrbsOnFloor: 1, GuardianId: "sajah-chimera", GuardianName: "Chimera of the Mirrors",
            TableId: "boss_ryo", PartyId: "boss_ryo_00", BaseHp: 2500, PackId: "ryo", SceneId: "bgc5",
            RetainCombatMain: true, RetainPhaseCallbacks: true,
            HpMultiplier: 4.80, DamageMultiplier: 1.22, Shields: 5, ChestPotch: 12000, ChestCommendations: 3),

        new(4, "Floor IV — Chamber of Endless Night",
            "No torches burn here, yet the violet glow of the summit filters down through the ceiling grates.",
            "Two Red Orbs rest upon twin altars in the dark. As you lift them, something stirs near the stairwell...",
            "Inscription IV: \"To walk a new path is to surrender comfort, yet carry the strength forged in fire.\"",
            OrbsOnFloor: 2, GuardianId: "sajah-abom", GuardianName: "Abomination of the Deep",
            TableId: "boss_nik", PartyId: "boss_nik_00", BaseHp: 4500, PackId: "nik", SceneId: "bgd5",
            RetainCombatMain: true, RetainPhaseCallbacks: false,
            HpMultiplier: 3.40, DamageMultiplier: 1.26, Shields: 6, ChestPotch: 15000, ChestCommendations: 3),

        new(5, "Floor V — The Sealed Vault",
            "A monumental bronze door with eight circular sockets dominates the upper landing. Only one orb remains.",
            "Inside the vault's final reliquary shines the eighth and final Red Orb!",
            "Inscription V: \"Place the eight orbs into the door of fate. Beyond lies the Rune of Circulating Paths.\"",
            OrbsOnFloor: 1, GuardianId: "sajah-harpy", GuardianName: "Winged Warden of Sajah",
            TableId: "boss_hap2", PartyId: "boss_hap2_00", BaseHp: 12000, PackId: "hap", SceneId: "bgi6",
            RetainCombatMain: false, RetainPhaseCallbacks: false,
            HpMultiplier: 2.00, DamageMultiplier: 1.30, Shields: 6, ChestPotch: 20000, ChestCommendations: 4),

        new(6, "Floor VI — Sanctum of Circulating Paths",
            "The eight Red Orbs flare crimson and the sealed door grinds open. Ahead stands the final Guardian—and a violet crystal floating in midair.",
            "All eight Red Orbs are already set into the Sanctum Door. Only the Keeper of the Cycle stands between you and the Crystal.",
            "The Rune of Circulating Paths hums: \"You who have conquered the six halls... Will you claim the blessing of a New Cycle?\"",
            OrbsOnFloor: 0, GuardianId: "sajah-dragon", GuardianName: "Bone Dragon of the Cycle",
            TableId: "boss_dor", PartyId: "boss_dor_00", BaseHp: 8900, PackId: "dor", SceneId: "bge2",
            RetainCombatMain: true, RetainPhaseCallbacks: false,
            HpMultiplier: 2.90, DamageMultiplier: 1.35, Shields: 7, ChestPotch: 25000, ChestCommendations: 5)
    };

    internal static CirculatingFloorSpec Floor(int floorNumber) =>
        Floors[Math.Clamp(floorNumber - 1, 0, Floors.Length - 1)];

    internal static string OrbProgressBar(int orbs)
    {
        int clamped = Math.Clamp(orbs, 0, TotalRedOrbs);
        return new string('●', clamped) + new string('○', TotalRedOrbs - clamped);
    }

    internal static double CycleDamageBonus(int cycleRank) =>
        1.0 + Math.Clamp(cycleRank, 0, MaxCycleRank) * 0.05;

    internal static double CyclePotchMultiplier(int cycleRank) =>
        1.0 + Math.Clamp(cycleRank, 0, MaxCycleRank) * 0.15;

    internal static double GuardianCycleHpScale(int cycleRank) =>
        1.0 + Math.Clamp(cycleRank, 0, MaxCycleRank) * 0.12;
}
