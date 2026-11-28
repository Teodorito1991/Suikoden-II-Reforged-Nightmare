namespace NightmareRebalance;

// Endless "Tower of Dunan" Roguelite Gauntlet built cleanly atop verified
// Hall of Echoes encounters with between-floor Relic Drafts and Cash-Out spoils.
internal static class TowerOfDunanRuntime
{
    static readonly int[] VerifiedSinglePartyTrials = { 0, 1, 2, 3, 5, 6, 7, 8, 9, 10 };
    static readonly System.Random rng = new();

    internal static bool RunActive { get; private set; }
    static OptionalEncounterLease? activeLease;
    static bool activeLeaseStartedNewRun;
    internal static bool BattleInProgress => activeLease is { BattleActive: true };
    internal static bool BloodOathActive { get; private set; }
    internal static int CurrentFloor { get; private set; }
    internal static int FloorsCleared { get; private set; }
    internal static readonly HashSet<string> ActiveRelics = new(StringComparer.Ordinal);
    internal static TowerRelicSpec[] DraftChoices { get; private set; } = Array.Empty<TowerRelicSpec>();

    internal static bool HasRelic(string id) => RunActive && BattleInProgress && ActiveRelics.Contains(id);
    internal static bool HasOwnedRelic(string id) => RunActive && ActiveRelics.Contains(id);
    internal static bool PendingRelicDraft => RunActive && DraftChoices.Length > 0;
    internal static void ToggleBloodOath() { if (!RunActive) BloodOathActive = !BloodOathActive; }

    internal static double ActiveHpMultiplier =>
        BattleInProgress ? Reforged5Rules.TowerFloorHpMultiplier(CurrentFloor) : 1.0;

    internal static double ActiveDamageMultiplier =>
        BattleInProgress ? Reforged5Rules.TowerFloorDamageMultiplier(CurrentFloor) * (BloodOathActive ? 1.15 : 1.0) : 1.0;

    internal static bool LaunchNextFloor(out string feedback)
    {
        if (!HallFirstContact.Available)
        {
            feedback = "Enable Hall of Echoes in settings to enter the Tower of Dunan.";
            return false;
        }
        if (PendingRelicDraft)
        {
            feedback = "Draft a Relic first before ascending to the next floor.";
            return false;
        }
        var unlocked = VerifiedSinglePartyTrials.Where(HallAvailability.CanSelect).ToArray();
        if (unlocked.Length == 0)
        {
            feedback = "Unlock at least one Hall of Echoes trial (or enable Spoiler Mode) to enter the Tower.";
            return false;
        }

        bool startingNew = !RunActive;
        int targetFloor = startingNew ? 1 : FloorsCleared + 1;
        int trialIndex = unlocked[(targetFloor - 1) % unlocked.Length];
        bool nightmare = targetFloor >= 5 && HallNightmareRuntime.CanSelect(trialIndex);

        if (startingNew)
        {
            RunActive = true;
            FloorsCleared = 0;
            ActiveRelics.Clear();
            DraftChoices = Array.Empty<TowerRelicSpec>();
        }
        CurrentFloor = targetFloor;
        var lease = new OptionalEncounterLease(OptionalEncounterKind.TowerOfDunan, "tower-f" + targetFloor);
        activeLease = lease;
        activeLeaseStartedNewRun = startingNew;
        if (!HallFirstContact.Request(trialIndex, nightmare, lease))
        {
            activeLease = null;
            if (startingNew) ResetRun();
            feedback = "Tower floor could not launch right now.";
            return false;
        }
        feedback = $"Ascending Tower of Dunan — Floor {CurrentFloor}!";
        return true;
    }

    internal static void OnLeaseCancelled(long token)
    {
        if (activeLease?.Token != token) return;
        bool wasNew = activeLeaseStartedNewRun;
        activeLease = null;
        activeLeaseStartedNewRun = false;
        if (wasNew && FloorsCleared == 0) ResetRun();
        else CurrentFloor = FloorsCleared;
    }

    internal static void OnAuthoritativeNonVictory(long token)
    {
        if (activeLease?.Token != token) return;
        activeLease = null;
        activeLeaseStartedNewRun = false;
        int lostFloor = CurrentFloor;
        ResetRun();
        CastleGuildRuntime.TrainingFeedback($"Tower Run Ended on Floor {lostFloor}. Better luck next ascent!");
    }

    internal static void OnAuthoritativeVictory(long token)
    {
        if (activeLease?.Token != token) return;
        activeLease = null;
        activeLeaseStartedNewRun = false;

        FloorsCleared++;
        ArmyProgressionRuntime.RecordTowerFloor(FloorsCleared);
        DraftChoices = Reforged5Rules.DraftRelicsForFloor(FloorsCleared, ActiveRelics, rng.Next());
        if (DraftChoices.Length == 0)
        {
            ArmyProgressionRuntime.AwardCommendations(2);
            CastleGuildRuntime.TrainingFeedback($"Tower Floor {FloorsCleared} Cleared! All Relics Mastered (+2 Bonus Commendations)!");
        }
        else
        {
            string wardenTag = Reforged5Rules.IsWardenFloor(FloorsCleared) ? " ★ LEGENDARY Relic Draft ready!" : " Return to the Guild to draft a Relic or Cash Out.";
            CastleGuildRuntime.TrainingFeedback($"Tower Floor {FloorsCleared} Cleared!{wardenTag}");
        }
    }

    internal static bool PickRelic(int index, out string feedback)
    {
        if (!PendingRelicDraft || index < 0 || index >= DraftChoices.Length)
        {
            feedback = "No Relic draft is active.";
            return false;
        }
        var chosen = DraftChoices[index];
        ActiveRelics.Add(chosen.Id);
        DraftChoices = Array.Empty<TowerRelicSpec>();
        feedback = $"Relic Acquired: {chosen.Name}! Ascend to Floor {FloorsCleared + 1} or Cash Out.";
        return true;
    }

    internal static bool CashOut(out string feedback)
    {
        if (!RunActive || FloorsCleared <= 0)
        {
            ResetRun();
            feedback = "Tower run abandoned with 0 floors cleared.";
            return true;
        }
        bool chalice = HasOwnedRelic("golden-chalice");
        int potch = Reforged5Rules.TowerCashOutPotch(FloorsCleared, chalice, BloodOathActive);
        int comm = Reforged5Rules.TowerCashOutCommendations(FloorsCleared, chalice, BloodOathActive);
        int cleared = FloorsCleared;

        if (Framework.Singleton<GAME_WORK>.instance?.party_data != null && potch > 0)
            G2_SYS.G2_party_gold(1, potch);
        if (comm > 0) ArmyProgressionRuntime.AwardCommendations(comm);

        ResetRun();
        feedback = $"Tower Cash-Out (Floor {cleared}): +{potch:N0} Potch & +{comm} Dunan Commendation(s)!";
        return true;
    }

    internal static void ResetRun()
    {
        RunActive = false;
        activeLease?.CancelOrTeardown();
        activeLease = null;
        activeLeaseStartedNewRun = false;
        CurrentFloor = 0;
        FloorsCleared = 0;
        ActiveRelics.Clear();
        DraftChoices = Array.Empty<TowerRelicSpec>();
    }
}
