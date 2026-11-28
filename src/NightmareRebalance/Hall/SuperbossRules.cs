namespace NightmareRebalance;

internal sealed record SuperbossSpec(
    string Id,
    string Name,
    string Subtitle,
    string TableId,
    string PartyId,
    int BaseHp,
    string? PackId,
    string SceneId,
    bool RetainCombatMain,
    bool RetainPhaseCallbacks,
    bool RetainCommand,
    double HpMultiplier,
    double BaseDamageMultiplier,
    int InitialShields,
    string TrophyName,
    string TrophySummary,
    int FirstClearPotch,
    int FirstClearCommendations,
    string SealWarning,
    string Phase2Callout,
    string Phase3Callout)
{
    internal HallTrialDefinition ToTrialDefinition() => new(
        "superboss-" + Id, Name, TableId, PartyId, BaseHp, PackId, SceneId,
        Subtitle, $"{FirstClearPotch:N0} Potch · +{FirstClearCommendations} Commendations · {TrophyName}",
        RetainCombatMain, RetainPhaseCallbacks, RetainCommand);
}

// Pure rules for the 4 Secret Superbosses (True Luca, Omega Beast Rune, Eternal Neclord, Awakened Bone Dragon).
internal static class SuperbossRules
{
    internal static readonly SuperbossSpec[] All =
    {
        new("true-luca", "True Luca Blight, Awakened", "The Sovereign of Ruin · 3-Phase Superboss",
            "boss_luc3", "boss_luc3_00", 6500, "luc", "bgd6", false, false, false,
            HpMultiplier: 4.50, BaseDamageMultiplier: 1.35, InitialShields: 7,
            TrophyName: "Crown of the Mad Prince",
            TrophySummary: "+10% Party Phys & Combo DMG, +15% Momentum Gain",
            FirstClearPotch: 50000, FirstClearCommendations: 10,
            SealWarning: "Beyond this seal waits True Luca Blight at full wrath—3 phases, Flame Barrier, Blood Mist, and Crimson Cataclysm. Break his Guard Shields or perish. Break the seal?",
            Phase2Callout: "🔥 TRUE LUCA: \"Die, pig!!\" [PHASE 2: BLOOD OF HIGHLAND — Shields Refilled!]",
            Phase3Callout: "☠ TRUE LUCA: \"THE UNBROKEN WOLF AWAKENS!!\" [PHASE 3: CRIMSON CATACLYSM!]"),

        new("omega-beast", "Omega Beast Rune", "Incarnation of the 27th True Rune · 5-Part Superboss",
            "boss_las", "boss_las_00", 5700, "las", "bgx0", true, true, true,
            HpMultiplier: 3.60, BaseDamageMultiplier: 1.30, InitialShields: 5,
            TrophyName: "Echo of the 27th Rune",
            TrophySummary: "+12% Party Magic/Rune DMG, -10% All DMG Taken",
            FirstClearPotch: 60000, FirstClearCommendations: 12,
            SealWarning: "The 27th True Rune unchained—five coordinated limbs, each fortified with Guard Shields and Arcane Storm surges. Shatter its parts before Silver Wolf's Ruin falls. Break the seal?",
            Phase2Callout: "⚡ OMEGA BEAST RUNE: \"THE 27TH SEAL SHATTERS!\" [PHASE 2: ARCANE STORM +20%!]",
            Phase3Callout: "☠ OMEGA BEAST RUNE: \"SILVER WOLF'S RUIN!\" [PHASE 3: APOCALYPSE SURGE!]"),

        new("eternal-neclord", "Neclord, Lord of Eternal Night", "Shadow of the Sindar · Trial I",
            "boss_nec3", "boss_nec3_00", 4500, "nec3", "bgg9", true, false, false,
            HpMultiplier: 5.00, BaseDamageMultiplier: 1.30, InitialShields: 6,
            TrophyName: "Chalice of Eternal Night",
            TrophySummary: "+8% Party Lifesteal on Attacks",
            FirstClearPotch: 35000, FirstClearCommendations: 8,
            SealWarning: "The undying vampire lord of the Sindar ruins—draining 12% life with every blow while his Shadow Shields endure. Shatter his guard to stop the feast. Break the seal?",
            Phase2Callout: "🌙 ETERNAL NECLORD: \"BOW TO THE CRIMSON MOON!\" [PHASE 2: SHADOW SHIELDS REFILLED!]",
            Phase3Callout: ""),

        new("awakened-dragon", "Awakened Bone Dragon", "Shadow of the Sindar · Trial II",
            "boss_dor", "boss_dor_00", 8900, "dor", "bge2", true, false, false,
            HpMultiplier: 3.40, BaseDamageMultiplier: 1.35, InitialShields: 7,
            TrophyName: "Fang of the Sindar",
            TrophySummary: "+12% All Party DMG & +1 Shield Break on Combos",
            FirstClearPotch: 45000, FirstClearCommendations: 10,
            SealWarning: "An ancient Sindar colossus wrapped in Bone Carapace (-15% damage taken while shielded) and lethal Ancient Breath below half HP. Break the seal?",
            Phase2Callout: "🦴 AWAKENED BONE DRAGON: \"ANCIENT BREATH IGNITES!\" [PHASE 2: CARAPACE REFILLED!]",
            Phase3Callout: "")
    };

    internal static SuperbossSpec? Find(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < All.Length; i++)
            if (All[i].Id == id) return All[i];
        return null;
    }

    internal static int EvaluatePhase(SuperbossSpec spec, int currentHp, int maxHp)
    {
        if (maxHp <= 0) return 1;
        double ratio = (double)currentHp / maxHp;
        bool hasPhase3 = !string.IsNullOrEmpty(spec.Phase3Callout);
        if (hasPhase3)
        {
            if (ratio <= 0.30) return 3;
            if (ratio <= 0.65) return 2;
            return 1;
        }
        return ratio <= 0.50 ? 2 : 1;
    }

    internal static double PhaseDamageMultiplier(SuperbossSpec spec, int phase, int turn)
    {
        double mult = spec.BaseDamageMultiplier;
        if (phase >= 2) mult *= 1.15;
        if (phase >= 3) mult *= 1.15;
        bool ultimateTurn = (phase >= 3 && turn > 0 && turn % 3 == 0) ||
            (string.IsNullOrEmpty(spec.Phase3Callout) && phase >= 2 && turn > 0 && turn % 3 == 0);
        if (ultimateTurn) mult *= 1.25;
        return mult;
    }

    internal static bool AllCleared(HashSet<string>? defeated) =>
        defeated != null && All.All(s => defeated.Contains(s.Id));
}
