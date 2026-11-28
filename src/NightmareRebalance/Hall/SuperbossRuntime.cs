namespace NightmareRebalance;

// Runtime controller for Secret Superboss launches, multi-phase choreography, and permanent Trophy passives.
internal static class SuperbossRuntime
{
    static OptionalEncounterLease? activeLease;
    internal static SuperbossSpec? Active { get; private set; }
    internal static bool BattleInProgress => activeLease is { BattleActive: true } && Active != null;
    internal static int CurrentPhase { get; private set; } = 1;

    internal static bool HasTrophy(string id) =>
        ArmyProgressionRuntime.Enabled && ArmyProgressionRuntime.Current.DefeatedSuperbosses != null &&
        ArmyProgressionRuntime.Current.DefeatedSuperbosses.Contains(id);

    internal static double ActiveHpMultiplier =>
        BattleInProgress && Active != null ? Active.HpMultiplier : 1.0;

    internal static double ActiveDamageMultiplier(BATTLE_WORK? w)
    {
        if (!BattleInProgress || Active == null) return 1.0;
        int turn = w?.battle_kaisuu ?? 0;
        return SuperbossRules.PhaseDamageMultiplier(Active, CurrentPhase, turn);
    }

    internal static bool Launch(string id, out string feedback)
    {
        var spec = SuperbossRules.Find(id);
        if (spec == null)
        {
            feedback = "Unknown secret superboss.";
            return false;
        }
        if (!HallFirstContact.Available)
        {
            feedback = "Enable Hall of Echoes in settings to break a Superboss Seal.";
            return false;
        }
        if (TowerOfDunanRuntime.BattleInProgress)
        {
            feedback = "Finish your active Tower battle first.";
            return false;
        }
        Active = spec;
        CurrentPhase = 1;
        var lease = new OptionalEncounterLease(OptionalEncounterKind.SecretSuperboss, spec.Id);
        activeLease = lease;
        if (!HallFirstContact.RequestSuperboss(spec.ToTrialDefinition(), lease))
        {
            ClearBattle();
            feedback = $"{spec.Name} could not be summoned right now.";
            return false;
        }
        feedback = $"★ SEAL BROKEN: {spec.Name} awakens!";
        return true;
    }

    internal static void OnBattleStart()
    {
        if (!BattleInProgress || Active == null) return;
        CurrentPhase = 1;
        Reforged2Runtime.FlashBanner($"★ SECRET SUPERBOSS: {Active.Name}!", 4200, "alert");
    }

    internal static int InitialShieldsForSuperboss() =>
        BattleInProgress && Active != null ? Active.InitialShields : 0;

    internal static int ApplySuperbossIncomingModifiers(BATTLE_WORK w, BattlePlayerCharacter actor, PM_DATA? target, int damage, bool isMagic)
    {
        if (!BattleInProgress || Active == null || damage <= 0 || target == null) return damage;
        int result = damage;

        var st = NightmareReworkRuntime.StateFor(target);
        bool shielded = st != null && st.Shields > 0;

        // Awakened Bone Dragon: Sindar Carapace (-15% incoming while shields intact)
        if (Active.Id == "awakened-dragon" && shielded)
            result = Math.Max(1, (int)Math.Round(result * 0.85, MidpointRounding.AwayFromZero));

        // Omega Beast Rune Phase 2+: Arcane Storm boosts magic for both sides (+20%)
        if (Active.Id == "omega-beast" && CurrentPhase >= 2 && isMagic)
            result = Math.Max(result + 1, (int)Math.Round(result * 1.20, MidpointRounding.AwayFromZero));

        // True Luca Phase 1: Flame Barrier reflects 8% of physical damage while shields are intact
        if (Active.Id == "true-luca" && CurrentPhase == 1 && !isMagic && shielded && actor.hp > 1)
        {
            int reflect = Math.Max(1, (int)Math.Round(result * 0.08, MidpointRounding.AwayFromZero));
            actor.hp = (short)Math.Max(1, actor.hp - Math.Min(reflect, actor.hp - 1));
            Reforged2Runtime.FlashBanner($"🔥 Flame Barrier! (-{reflect} HP)", 1600, "alert");
        }

        // Check Phase Transitions based on boss HP after this hit (or overall remaining HP for multi-part Omega Beast)
        int curHp = Math.Max(0, target.hp - result);
        int maxHp = Math.Max(1, (int)target.max_hp);
        if (Active.Id == "omega-beast" && w.pm_data != null)
        {
            int totalCur = 0, totalMax = 0;
            foreach (var u in w.pm_data)
            {
                if (u == null || u.IsPlayer) continue;
                totalMax += u.max_hp;
                totalCur += Math.Max(0, u.Pointer == target.Pointer ? u.hp - result : u.hp);
            }
            if (totalMax > 0) { curHp = totalCur; maxHp = totalMax; }
        }

        int nextPhase = SuperbossRules.EvaluatePhase(Active, curHp, maxHp);
        if (nextPhase > CurrentPhase)
        {
            CurrentPhase = nextPhase;
            string callout = nextPhase >= 3 && !string.IsNullOrEmpty(Active.Phase3Callout)
                ? Active.Phase3Callout
                : Active.Phase2Callout;
            if (!string.IsNullOrEmpty(callout))
                Reforged2Runtime.FlashBanner(callout, 4600, "alert");

            // Refill Guard Shields on Phase Shift!
            if (st != null && st.MaxShields > 0)
            {
                st.MaxShields = Active.InitialShields + (nextPhase >= 2 ? 1 : 0);
                st.Shields = st.MaxShields;
                st.StaggeredUntilTurn = -1;
            }
        }
        return result;
    }

    internal static void OnSuperbossDealtDamage(PM_DATA? attacker, int dealtDamage)
    {
        if (!BattleInProgress || Active == null || attacker == null || dealtDamage <= 0 || attacker.hp <= 0 || attacker.hp >= attacker.max_hp)
            return;
        var st = NightmareReworkRuntime.StateFor(attacker);
        bool shielded = st != null && st.Shields > 0;
        double leechRate = 0.0;
        if (Active.Id == "eternal-neclord" && shielded) leechRate = 0.12;
        else if (Active.Id == "true-luca" && CurrentPhase >= 2) leechRate = 0.10;
        if (leechRate <= 0.0) return;

        int heal = Math.Max(1, (int)Math.Round(dealtDamage * leechRate, MidpointRounding.AwayFromZero));
        attacker.hp = (short)Math.Min(attacker.max_hp, attacker.hp + heal);
    }

    internal static void OnLeaseCancelled(long token)
    {
        if (activeLease?.Token != token) return;
        ClearBattle();
    }

    internal static void OnAuthoritativeNonVictory(long token)
    {
        if (activeLease?.Token != token || Active == null) return;
        var spec = Active;
        ClearBattle();
        CastleGuildRuntime.TrainingFeedback($"Defeated by {spec.Name}. Regroup, refine your Talents, and break the seal again!");
    }

    internal static void OnAuthoritativeVictory(long token)
    {
        if (activeLease?.Token != token || Active == null) return;
        var spec = Active;
        ClearBattle();

        bool firstClear = ArmyProgressionRuntime.RecordSuperbossVictory(spec.Id, out bool allFourCleared);
        if (firstClear)
        {
            if (Framework.Singleton<GAME_WORK>.instance?.party_data != null && spec.FirstClearPotch > 0)
                G2_SYS.G2_party_gold(1, spec.FirstClearPotch);
            ArmyProgressionRuntime.AwardCommendations(spec.FirstClearCommendations + (allFourCleared ? 20 : 0));
            string allNote = allFourCleared ? " ★ ALL 4 SUPERBOSSES VANQUISHED — SOVEREIGN OF DUNAN (+20 Bonus Commendations)!" : "";
            CastleGuildRuntime.TrainingFeedback(
                $"★ VICTORY OVER {spec.Name.ToUpperInvariant()}! Trophy Earned: {spec.TrophyName} (+{spec.FirstClearPotch:N0} Potch, +{spec.FirstClearCommendations} Commendations)!{allNote}");
        }
        else
        {
            ArmyProgressionRuntime.AwardCommendations(3);
            CastleGuildRuntime.TrainingFeedback($"★ {spec.Name} vanquished again! (+3 Bonus Commendations)");
        }
    }

    internal static void ClearBattle()
    {
        activeLease?.CancelOrTeardown();
        activeLease = null;
        Active = null;
        CurrentPhase = 1;
    }
}
