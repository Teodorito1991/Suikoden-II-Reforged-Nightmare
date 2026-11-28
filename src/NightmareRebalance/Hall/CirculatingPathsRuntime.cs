namespace NightmareRebalance;

// Runtime controller for the restored cut Sajah Village Dungeon ("Tower of Circulating Paths").
// Tracks current floor (1..6), collected Red Orbs (0..8), searched alcoves, defeated stairwell
// guardians, and launches verified Hall battles cleanly without soft-locks or save corruption.
internal static class CirculatingPathsRuntime
{
    internal static bool RunActive { get; private set; }
    internal static bool BattleInProgress { get; private set; }
    internal static int CurrentFloor { get; private set; } = 1;
    internal static int RedOrbsCollected { get; private set; }
    internal static bool AlcoveSearchedThisFloor { get; private set; }
    internal static bool GuardianDefeatedThisFloor { get; private set; }
    internal static bool SummitCrystalReady { get; private set; }

    internal static int CycleRank =>
        ArmyProgressionRuntime.Enabled ? ArmyProgressionRuntime.Current.CirculatingCycleRank : 0;

    internal static double ActiveHpMultiplier =>
        BattleInProgress
            ? CirculatingPathsRules.Floor(CurrentFloor).HpMultiplier * CirculatingPathsRules.GuardianCycleHpScale(CycleRank)
            : 1.0;

    internal static double ActiveDamageMultiplier =>
        BattleInProgress
            ? CirculatingPathsRules.Floor(CurrentFloor).DamageMultiplier * (1.0 + Math.Min(CycleRank, 5) * 0.06)
            : 1.0;

    internal static int InitialShieldsForGuardian() =>
        BattleInProgress ? CirculatingPathsRules.Floor(CurrentFloor).Shields : 0;

    internal static void EnterOrResume()
    {
        if (!RunActive)
        {
            RunActive = true;
            CurrentFloor = 1;
            RedOrbsCollected = 0;
            AlcoveSearchedThisFloor = false;
            GuardianDefeatedThisFloor = false;
            SummitCrystalReady = false;
        }
    }

    internal static string SearchCurrentAlcove()
    {
        EnterOrResume();
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        if (AlcoveSearchedThisFloor)
            return "You have already searched this floor's Sindar alcove.";
        AlcoveSearchedThisFloor = true;
        if (spec.OrbsOnFloor > 0)
            RedOrbsCollected = Math.Min(CirculatingPathsRules.TotalRedOrbs, RedOrbsCollected + spec.OrbsOnFloor);
        if (Framework.Singleton<GAME_WORK>.instance?.party_data != null && spec.ChestPotch > 0)
            G2_SYS.G2_party_gold(1, spec.ChestPotch);
        if (spec.ChestCommendations > 0)
            ArmyProgressionRuntime.AwardCommendations(spec.ChestCommendations);
        return $"{spec.AlcovePrompt}  (+{spec.ChestPotch:N0} Potch, +{spec.ChestCommendations} Commendations · Red Orbs: {RedOrbsCollected}/{CirculatingPathsRules.TotalRedOrbs})";
    }

    internal static bool LaunchFloorGuardian(out string feedback)
    {
        EnterOrResume();
        if (!HallFirstContact.Available)
        {
            feedback = "Enable Hall of Echoes in settings to challenge the Stairwell Guardians.";
            return false;
        }
        if (GuardianDefeatedThisFloor)
        {
            feedback = "This floor's Stairwell Guardian is already defeated! Ascend to the next floor.";
            return false;
        }
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        if (CurrentFloor == CirculatingPathsRules.TotalFloors && RedOrbsCollected < CirculatingPathsRules.TotalRedOrbs)
        {
            feedback = $"The Sanctum Door requires all {CirculatingPathsRules.TotalRedOrbs} Red Orbs (you have {RedOrbsCollected}).";
            return false;
        }
        BattleInProgress = true;
        if (!HallFirstContact.RequestSuperboss(spec.ToGuardianTrial()))
        {
            BattleInProgress = false;
            feedback = $"{spec.GuardianName} could not be engaged right now.";
            return false;
        }
        feedback = $"★ TOWER OF CIRCULATING PATHS (Floor {CurrentFloor}/6): {spec.GuardianName} blocks the stairs!";
        return true;
    }

    internal static string AscendToNextFloor()
    {
        if (!RunActive || !GuardianDefeatedThisFloor)
            return "Defeat the Stairwell Guardian first to unlock the stairs.";
        if (CurrentFloor >= CirculatingPathsRules.TotalFloors)
        {
            SummitCrystalReady = true;
            return "The violet Rune of Circulating Paths hovers before you!";
        }
        if (CurrentFloor == CirculatingPathsRules.TotalFloors - 1 && RedOrbsCollected < CirculatingPathsRules.TotalRedOrbs)
            return $"You need all {CirculatingPathsRules.TotalRedOrbs} Red Orbs to unseal Floor VI! Search this floor's alcove first ({RedOrbsCollected}/{CirculatingPathsRules.TotalRedOrbs}).";

        CurrentFloor++;
        AlcoveSearchedThisFloor = false;
        GuardianDefeatedThisFloor = false;
        return $"Ascended to {CirculatingPathsRules.Floor(CurrentFloor).Name}!";
    }

    internal static string ClaimCycleBlessing(bool newCycle)
    {
        if (!SummitCrystalReady)
            return "Reach the Sanctum on Floor VI and defeat the Bone Dragon of the Cycle first.";
        int potch = newCycle ? 100000 : 75000;
        int comm = newCycle ? 25 : 15;
        if (Framework.Singleton<GAME_WORK>.instance?.party_data != null)
            G2_SYS.G2_party_gold(1, potch);
        ArmyProgressionRuntime.AwardCommendations(comm);
        int rank = ArmyProgressionRuntime.RecordCirculatingPathsClear(newCycle);
        ResetRun();
        return newCycle
            ? $"★ RUNE OF CIRCULATING PATHS ACCEPTED! Cycle Rank {rank} (+{rank * 5}% Party DMG, +{rank * 15}% Potch), +{potch:N0} Potch & +{comm} Commendations!"
            : $"★ SINDAR VAULT CLAIMED! +{potch:N0} Potch & +{comm} Commendations!";
    }

    internal static void OnBattleStart()
    {
        if (!BattleInProgress) return;
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        Reforged2Runtime.FlashBanner($"🏛 SAJAH TOWER FLOOR {CurrentFloor}/6: {spec.GuardianName}!", 4000, "gold");
    }

    internal static void OnHallBattleFinished(bool won)
    {
        if (!BattleInProgress) return;
        BattleInProgress = false;
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        if (!won)
        {
            CastleGuildRuntime.TrainingFeedback($"Forced back on {spec.Name}. Your Red Orbs ({RedOrbsCollected}/{CirculatingPathsRules.TotalRedOrbs}) remain intact—regroup and return!");
            return;
        }
        GuardianDefeatedThisFloor = true;
        if (CurrentFloor >= CirculatingPathsRules.TotalFloors)
        {
            SummitCrystalReady = true;
            CastleGuildRuntime.TrainingFeedback($"★ {spec.GuardianName} VANQUISHED! Speak with the Hooded Stranger to touch the Rune of Circulating Paths!");
        }
        else
        {
            CastleGuildRuntime.TrainingFeedback($"★ {spec.GuardianName} defeated! Speak with the Hooded Stranger to ascend to Floor {CurrentFloor + 1}/6.");
        }
    }

    internal static void ResetRun()
    {
        RunActive = false;
        BattleInProgress = false;
        CurrentFloor = 1;
        RedOrbsCollected = 0;
        AlcoveSearchedThisFloor = false;
        GuardianDefeatedThisFloor = false;
        SummitCrystalReady = false;
    }
}
