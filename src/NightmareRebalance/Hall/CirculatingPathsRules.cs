namespace NightmareRebalance;

internal sealed record SpireBlessingSpec(string Id, string Name, string Summary);

internal sealed record CirculatingFloorSpec(
    int Floor,
    string ShortTag,
    string Name,
    string Atmosphere,
    string PuzzlePrompt,
    string PuzzleChoiceSafe,
    string PuzzleChoiceRisky,
    string PuzzleSafeResult,
    string PuzzleRiskyResult,
    string ShrineTitle,
    string ShrineDescription,
    string? BlessingId,
    int ShrinePotchCost,
    int ShrineCommCost,
    bool ShrineHealsParty,
    string InscriptionLore,
    int OrbsOnFloor,
    string GuardianId,
    string GuardianName,
    string GuardianMechanicName,
    string GuardianMechanicDesc,
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
// "The Tower of Circulating Paths" (6 floors, 8 Red Orbs, Sindar Puzzles,
// Secret Shrines & Spire Blessings, 6 Stairwell Guardians with unique mechanics,
// and the Rune of Circulating Paths summit crystal).
internal static class CirculatingPathsRules
{
    internal const int TotalFloors = 6;
    internal const int TotalRedOrbs = 8;
    internal const int MaxCycleRank = 5;

    internal static readonly SpireBlessingSpec[] Blessings =
    {
        new("serpent-fang", "Serpent's Fang", "+15% Party DMG in Sajah Spire"),
        new("mirror-ward", "Mirror Ward", "-15% Incoming DMG in Sajah Spire"),
        new("vampiric-seal", "Vampiric Seal", "+10% Party Lifesteal in Sajah Spire"),
        new("sindar-pulse", "Sindar Pulse", "+25% Momentum Gain in Sajah Spire")
    };

    internal static SpireBlessingSpec? FindBlessing(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < Blessings.Length; i++)
            if (Blessings[i].Id == id) return Blessings[i];
        return null;
    }

    internal static readonly CirculatingFloorSpec[] Floors =
    {
        new(1, "F1 Boulder", "Floor I — Beneath the Sajah Boulder",
            "Cold Sindar stonework stretches upward behind the sealed boulder of Sajah Village. Faint crimson lights pulse in the alcoves.",
            "A twin-headed serpent dial guards two crimson sockets. Etched below: \"Yield to the quiet river, never the blade.\"",
            "Turn the dial toward the River Sigil", "Force the lock open with your weapon",
            "The stone clicks softly—revealing two glowing Red Orbs and an untouched Sajah silver coffer!",
            "A Sindar needle-trap grazes the party (-15% HP), but you wrench free both Red Orbs and extra silver (+3,000 Potch)!",
            "Faint Blue Crystal", "Restores full party HP & Rune charges for 3,000 Potch.",
            BlessingId: null, ShrinePotchCost: 3000, ShrineCommCost: 0, ShrineHealsParty: true,
            "Inscription I: \"Behind the quiet village of Sajah, the Sindar built a spire that bends time back upon itself.\"",
            OrbsOnFloor: 2, GuardianId: "sajah-rat", GuardianName: "Sindar Plague King",
            GuardianMechanicName: "Toxic Miasma", GuardianMechanicDesc: "+20% DMG below 40% HP",
            TableId: "boss_rat", PartyId: "boss_rat_00", BaseHp: 5200, PackId: "rat", SceneId: "bgh3",
            RetainCombatMain: false, RetainPhaseCallbacks: false,
            HpMultiplier: 2.20, DamageMultiplier: 1.15, Shields: 4, ChestPotch: 8000, ChestCommendations: 2),

        new(2, "F2 Serpent", "Floor II — Corridor of the Serpent",
            "Massive twin-serpent pillars line the second tier. Ancient machinery hums softly behind the cracked walls.",
            "Two bronze serpents hold crimson spheres in their jaws—one faces Dawn, the other Dusk.",
            "Press both Dawn & Dusk fangs together", "Snatch the spheres before the jaws shut",
            "The statues bow in unison, surrendering two Red Orbs and a pouch of Sajah coins!",
            "The bronze jaws snap shut (-15% party HP), yet you rip out both Red Orbs and a hidden gem (+4,000 Potch)!",
            "Shrine of the Twin Serpents", "Offer 1 Commendation to gain [Serpent's Fang]: +15% Party DMG for this spire run.",
            BlessingId: "serpent-fang", ShrinePotchCost: 0, ShrineCommCost: 1, ShrineHealsParty: false,
            "Inscription II: \"Eight crimson orbs seal the summit door. Only those who endure all six guardians may gaze upon the Cycle.\"",
            OrbsOnFloor: 2, GuardianId: "sajah-golem", GuardianName: "Sindar Sentinel Golem",
            GuardianMechanicName: "Stone Aegis", GuardianMechanicDesc: "-15% Phys DMG taken while shielded",
            TableId: "boss_gol", PartyId: "boss_gol_00", BaseHp: 7500, PackId: "gol", SceneId: "bgg7",
            RetainCombatMain: true, RetainPhaseCallbacks: false,
            HpMultiplier: 2.10, DamageMultiplier: 1.20, Shields: 5, ChestPotch: 10000, ChestCommendations: 2),

        new(3, "F3 Mirrors", "Floor III — Hall of Broken Mirrors",
            "Polished obsidian mirrors reflect battles that have already happened—and paths never taken.",
            "Three obsidian mirrors show your party. Only one reflection holds a glowing crimson orb in its left hand.",
            "Reach into the mirror with the crimson reflection", "Shatter all three obsidian mirrors",
            "Your hand passes cleanly through cold glass and withdraws the fifth Red Orb!",
            "Shards of Sindar glass cut the party (-15% HP), uncovering the fifth Red Orb and lost gold (+5,000 Potch)!",
            "Mirror of Clarity", "Offer 1 Commendation to gain [Mirror Ward]: -15% Incoming DMG for this spire run.",
            BlessingId: "mirror-ward", ShrinePotchCost: 0, ShrineCommCost: 1, ShrineHealsParty: false,
            "Inscription III: \"The shops of Sajah still stock relics for travelers who return from the summit, though few ever remember why.\"",
            OrbsOnFloor: 1, GuardianId: "sajah-chimera", GuardianName: "Chimera of the Mirrors",
            GuardianMechanicName: "Mirror Reflection", GuardianMechanicDesc: "Reflects 8% Magic DMG while shielded",
            TableId: "boss_ryo", PartyId: "boss_ryo_00", BaseHp: 2500, PackId: "ryo", SceneId: "bgc5",
            RetainCombatMain: true, RetainPhaseCallbacks: true,
            HpMultiplier: 4.80, DamageMultiplier: 1.22, Shields: 5, ChestPotch: 12000, ChestCommendations: 3),

        new(4, "F4 Night", "Floor IV — Chamber of Endless Night",
            "No torches burn here, yet the violet glow of the summit filters down through the ceiling grates.",
            "Twin altars flicker with violet flame. An inscription warns: \"Extinguish shadow with stillness, not steel.\"",
            "Kneel briefly before the violet flame", "Sweep the orbs off both altars at once",
            "The flames part harmlessly—two Red Orbs and a Sindar reliquary rest in your hands!",
            "Violet fire sears the vanguard (-15% HP), but you seize both Red Orbs and charred gold (+6,000 Potch)!",
            "Altar of the Crimson Moon", "Offer 1 Commendation to gain [Vampiric Seal]: +10% Party Lifesteal for this spire run.",
            BlessingId: "vampiric-seal", ShrinePotchCost: 0, ShrineCommCost: 1, ShrineHealsParty: false,
            "Inscription IV: \"To walk a new path is to surrender comfort, yet carry the strength forged in fire.\"",
            OrbsOnFloor: 2, GuardianId: "sajah-abom", GuardianName: "Abomination of the Deep",
            GuardianMechanicName: "Abyssal Drain", GuardianMechanicDesc: "Heals 10% of DMG dealt while shielded",
            TableId: "boss_nik", PartyId: "boss_nik_00", BaseHp: 4500, PackId: "nik", SceneId: "bgd5",
            RetainCombatMain: true, RetainPhaseCallbacks: false,
            HpMultiplier: 3.40, DamageMultiplier: 1.26, Shields: 6, ChestPotch: 15000, ChestCommendations: 3),

        new(5, "F5 Vault", "Floor V — The Sealed Vault",
            "A monumental bronze door with eight circular sockets dominates the upper landing. Only one orb remains.",
            "The eighth Red Orb levitates inside a spinning ring of Sindar runes.",
            "Match the rune cadence and lift the Orb", "Plunge your hand straight through the ring",
            "The runes chime in harmony—the 8th and final Red Orb is yours! The Sanctum Door trembles!",
            "Arcane static jolts the party (-15% HP), yet you claim the 8th Red Orb and Sajah bullion (+8,000 Potch)!",
            "Sajah Ghost Merchant", "Pay 5,000 Potch for [Sindar Pulse] (+25% Momentum) + Full Party HP/Rune Restore!",
            BlessingId: "sindar-pulse", ShrinePotchCost: 5000, ShrineCommCost: 0, ShrineHealsParty: true,
            "Inscription V: \"Place the eight orbs into the door of fate. Beyond lies the Rune of Circulating Paths.\"",
            OrbsOnFloor: 1, GuardianId: "sajah-harpy", GuardianName: "Winged Warden of Sajah",
            GuardianMechanicName: "Gale Tempest", GuardianMechanicDesc: "+30% DMG burst every 3rd turn",
            TableId: "boss_hap2", PartyId: "boss_hap2_00", BaseHp: 12000, PackId: "hap", SceneId: "bgi6",
            RetainCombatMain: false, RetainPhaseCallbacks: false,
            HpMultiplier: 2.00, DamageMultiplier: 1.30, Shields: 6, ChestPotch: 20000, ChestCommendations: 4),

        new(6, "F6 ★ Sanctum", "Floor VI — Sanctum of Circulating Paths",
            "The eight Red Orbs flare crimson and the sealed door grinds open. Ahead stands the final Guardian—and a violet crystal floating in midair.",
            "All eight Red Orbs are set into the Sanctum Door.", "", "", "", "",
            "Sanctum Crystal", "Pay 5,000 Potch for a full Party HP & Rune Restore before the final Guardian.",
            BlessingId: null, ShrinePotchCost: 5000, ShrineCommCost: 0, ShrineHealsParty: true,
            "The Rune of Circulating Paths hums: \"You who have conquered the six halls... Will you claim the blessing of a New Cycle?\"",
            OrbsOnFloor: 0, GuardianId: "sajah-dragon", GuardianName: "Bone Dragon of the Cycle",
            GuardianMechanicName: "Chrono Cataclysm", GuardianMechanicDesc: "Refills Guard Shields at ≤50% HP!",
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

// Pure rollback + save-fencing coordinator for experimental map warps.
internal readonly record struct WarpCoordinateSnapshot(
    int GameArea, int GameTown, int GameMap,
    int MachiArea, int MachiTown, int MachiMap, int MachiCtyp,
    bool DungeonModeActive, int ReturnAno, int ReturnVno, int ReturnMno);

internal sealed class WarpTransitionGuard
{
    internal bool TransitionPending { get; private set; }

    internal bool TryExecute(
        WarpCoordinateSnapshot before,
        Action applyTargetCoordinates,
        Action invokeNativeTransition,
        Action<WarpCoordinateSnapshot> restoreCoordinates,
        out Exception? failure)
    {
        if (TransitionPending)
        {
            failure = new InvalidOperationException("Warp transition already in progress.");
            return false;
        }
        TransitionPending = true;
        try
        {
            applyTargetCoordinates();
            invokeNativeTransition();
            failure = null;
            return true;
        }
        catch (Exception ex)
        {
            TransitionPending = false;
            try { restoreCoordinates(before); } catch { }
            failure = ex;
            return false;
        }
    }

    internal void MarkTransitionSettled() => TransitionPending = false;
}
