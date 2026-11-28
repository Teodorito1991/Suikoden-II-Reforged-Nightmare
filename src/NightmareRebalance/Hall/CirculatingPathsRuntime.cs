namespace NightmareRebalance;

// Runtime controller for the restored cut Sajah Village Dungeon ("Tower of Circulating Paths").
// Tracks current floor (1..6), collected Red Orbs (0..8), solved puzzles, visited shrines,
// active Spire Blessings, unique Stairwell Guardian combat mechanics, and the Rune of Circulating Paths summit.
internal static class CirculatingPathsRuntime
{
    static OptionalEncounterLease? activeLease;
    internal static bool RunActive { get; private set; }
    internal static bool BattleInProgress => activeLease is { BattleActive: true };
    internal static int CurrentFloor { get; private set; } = 1;
    internal static int RedOrbsCollected { get; private set; }
    internal static bool AlcoveSearchedThisFloor { get; private set; }
    internal static bool ShrineVisitedThisFloor { get; private set; }
    internal static bool GuardianDefeatedThisFloor { get; private set; }
    internal static bool SummitCrystalReady { get; private set; }
    internal static bool GuardianPhase2Triggered { get; private set; }
    internal static readonly HashSet<string> ActiveBlessings = new(StringComparer.Ordinal);

    internal static bool HasBlessing(string id) => BattleInProgress && ActiveBlessings.Contains(id);

    internal static int CycleRank =>
        ArmyProgressionRuntime.Enabled ? ArmyProgressionRuntime.Current.CirculatingCycleRank : 0;

    internal static double ActiveHpMultiplier =>
        BattleInProgress
            ? CirculatingPathsRules.Floor(CurrentFloor).HpMultiplier * CirculatingPathsRules.GuardianCycleHpScale(CycleRank)
            : 1.0;

    internal static double ActiveDamageMultiplier(BATTLE_WORK? w, PM_DATA? attacker)
    {
        if (!BattleInProgress) return 1.0;
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        double mult = spec.DamageMultiplier * (1.0 + Math.Min(CycleRank, 5) * 0.06);
        int turn = w?.battle_kaisuu ?? 0;

        // Floor 1 Toxic Miasma: +20% DMG below 40% HP
        if (CurrentFloor == 1 && attacker != null && attacker.max_hp > 0 && attacker.hp * 100 / attacker.max_hp <= 40)
            mult *= 1.20;
        // Floor 5 Gale Tempest: +30% DMG every 3rd turn
        if (CurrentFloor == 5 && turn > 0 && turn % 3 == 0)
            mult *= 1.30;
        // Floor 6 Chrono Cataclysm Phase 2: +20% DMG
        if (CurrentFloor == 6 && GuardianPhase2Triggered)
            mult *= 1.20;
        // Mirror Ward Spire Blessing: -15% Incoming DMG
        if (HasBlessing("mirror-ward"))
            mult *= 0.85;
        return mult;
    }

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
            ShrineVisitedThisFloor = false;
            GuardianDefeatedThisFloor = false;
            SummitCrystalReady = false;
            GuardianPhase2Triggered = false;
            ActiveBlessings.Clear();
        }
    }

    internal static string ResolvePuzzle(bool safeChoice)
    {
        EnterOrResume();
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        if (AlcoveSearchedThisFloor)
            return "You have already solved this floor's Sindar puzzle.";
        AlcoveSearchedThisFloor = true;
        if (spec.OrbsOnFloor > 0)
            RedOrbsCollected = Math.Min(CirculatingPathsRules.TotalRedOrbs, RedOrbsCollected + spec.OrbsOnFloor);

        int bonusPotch = safeChoice ? 0 : 2000 + CurrentFloor * 1000;
        int totalPotch = spec.ChestPotch + bonusPotch;
        if (Framework.Singleton<GAME_WORK>.instance?.party_data != null && totalPotch > 0)
            G2_SYS.G2_party_gold(1, totalPotch);
        if (spec.ChestCommendations > 0)
            ArmyProgressionRuntime.AwardCommendations(spec.ChestCommendations);
        if (!safeChoice) DamageFieldPartyPercent(0.15);

        string outcome = safeChoice ? spec.PuzzleSafeResult : spec.PuzzleRiskyResult;
        return $"{outcome}  (+{totalPotch:N0} Potch, +{spec.ChestCommendations} Commendations · Red Orbs: {RedOrbsCollected}/{CirculatingPathsRules.TotalRedOrbs})";
    }

    internal static string ActivateFloorShrine()
    {
        EnterOrResume();
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        if (ShrineVisitedThisFloor)
            return $"You have already used the {spec.ShrineTitle} on this floor.";

        var party = Framework.Singleton<GAME_WORK>.instance?.party_data;
        if (spec.ShrinePotchCost > 0)
        {
            if (party == null || party.gold < spec.ShrinePotchCost)
                return $"Need {spec.ShrinePotchCost:N0} Potch to activate the {spec.ShrineTitle}.";
            G2_SYS.G2_party_gold(2, spec.ShrinePotchCost);
        }
        if (spec.ShrineCommCost > 0)
        {
            if (ArmyProgressionRuntime.Current.Commendations < spec.ShrineCommCost)
                return $"Need {spec.ShrineCommCost} Commendation(s) to activate the {spec.ShrineTitle}.";
            ArmyProgressionRuntime.SpendCommendations(spec.ShrineCommCost);
        }

        ShrineVisitedThisFloor = true;
        var notes = new List<string>(2);
        if (!string.IsNullOrEmpty(spec.BlessingId) && CirculatingPathsRules.FindBlessing(spec.BlessingId) is {} b)
        {
            ActiveBlessings.Add(b.Id);
            notes.Add($"Spire Blessing Acquired: {b.Name} ({b.Summary})!");
        }
        if (spec.ShrineHealsParty)
        {
            HealFieldPartyFull();
            notes.Add("Party HP & Rune charges fully restored!");
        }
        return notes.Count > 0 ? string.Join("  ", notes) : $"{spec.ShrineTitle} activated.";
    }

    static void HealFieldPartyFull()
    {
        try
        {
            var g = Framework.Singleton<GAME_WORK>.instance;
            var pd = g?.party_data;
            var list = g?.chara_data?.c_varia_dat;
            if (list == null || pd?.party_cha_no == null) return;
            for (int i = 0; i < pd.party_cha_no.Length && i < 6; i++)
            {
                int cid = pd.party_cha_no[i];
                if (cid <= 0 || cid >= list.Length) continue;
                var ch = list[cid];
                if (ch == null) continue;
                if (ch.max_hp > 0) ch.now_hp = ch.max_hp;
                if (ch.mp != null)
                {
                    for (int m = 0; m < ch.mp.Length && m < 4; m++)
                    {
                        byte max = (byte)(ch.mp[m] & 0x0F);
                        ch.mp[m] = (byte)((max << 4) | max);
                    }
                }
            }
        }
        catch { }
    }

    static void DamageFieldPartyPercent(double fraction)
    {
        try
        {
            var g = Framework.Singleton<GAME_WORK>.instance;
            var pd = g?.party_data;
            var list = g?.chara_data?.c_varia_dat;
            if (list == null || pd?.party_cha_no == null) return;
            for (int i = 0; i < pd.party_cha_no.Length && i < 6; i++)
            {
                int cid = pd.party_cha_no[i];
                if (cid <= 0 || cid >= list.Length) continue;
                var ch = list[cid];
                if (ch == null || ch.now_hp <= 1 || ch.max_hp <= 0) continue;
                int dmg = Math.Max(1, (int)Math.Round(ch.max_hp * fraction, MidpointRounding.AwayFromZero));
                ch.now_hp = (ushort)Math.Max(1, ch.now_hp - dmg);
            }
        }
        catch { }
    }

    internal static int ApplyGuardianIncomingModifiers(BATTLE_WORK w, BattlePlayerCharacter actor, PM_DATA? target, int damage, bool isMagic)
    {
        if (!BattleInProgress || damage <= 0 || target == null) return damage;
        int result = damage;
        var st = NightmareReworkRuntime.StateFor(target);
        bool shielded = st != null && st.Shields > 0;

        // Floor 2 Stone Aegis: -15% physical taken while shielded
        if (CurrentFloor == 2 && !isMagic && shielded)
            result = Math.Max(1, (int)Math.Round(result * 0.85, MidpointRounding.AwayFromZero));

        // Floor 3 Mirror Reflection: reflects 8% magic while shielded
        if (CurrentFloor == 3 && isMagic && shielded && actor.hp > 1)
        {
            int reflect = Math.Max(1, (int)Math.Round(result * 0.08, MidpointRounding.AwayFromZero));
            actor.hp = (short)Math.Max(1, actor.hp - Math.Min(reflect, actor.hp - 1));
            Reforged2Runtime.FlashBanner($"🪞 Mirror Reflection! (-{reflect} HP)", 1600, "alert");
        }

        // Floor 6 Bone Dragon of the Cycle: Chrono Cataclysm phase shift at <=50% HP
        if (CurrentFloor == 6 && !GuardianPhase2Triggered && target.max_hp > 0)
        {
            int afterHp = Math.Max(0, target.hp - result);
            if (afterHp * 100 / target.max_hp <= 50)
            {
                GuardianPhase2Triggered = true;
                Reforged2Runtime.FlashBanner("⏳ BONE DRAGON OF THE CYCLE: \"CHRONO CATACLYSM!\" [SHIELDS REFILLED +20% DMG]", 4200, "alert");
                if (st != null && st.MaxShields > 0)
                {
                    st.Shields = st.MaxShields;
                    st.StaggeredUntilTurn = -1;
                }
            }
        }
        return result;
    }

    internal static void OnGuardianDealtDamage(PM_DATA? attacker, int dealtDamage)
    {
        if (!BattleInProgress || CurrentFloor != 4 || attacker == null || dealtDamage <= 0 || attacker.hp <= 0 || attacker.hp >= attacker.max_hp)
            return;
        var st = NightmareReworkRuntime.StateFor(attacker);
        if (st == null || st.Shields <= 0) return;
        // Floor 4 Abyssal Drain: heals 10% of damage dealt while shielded
        int heal = Math.Max(1, (int)Math.Round(dealtDamage * 0.10, MidpointRounding.AwayFromZero));
        attacker.hp = (short)Math.Min(attacker.max_hp, attacker.hp + heal);
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
        GuardianPhase2Triggered = false;
        var lease = new OptionalEncounterLease(OptionalEncounterKind.CirculatingPaths, "sajah-f" + CurrentFloor);
        activeLease = lease;
        if (!HallFirstContact.RequestSuperboss(spec.ToGuardianTrial(), lease))
        {
            activeLease = null;
            feedback = $"{spec.GuardianName} could not be engaged right now.";
            return false;
        }
        feedback = $"★ TOWER OF CIRCULATING PATHS (Floor {CurrentFloor}/6): {spec.GuardianName} ({spec.GuardianMechanicName}) blocks the stairs!";
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
            return $"You need all {CirculatingPathsRules.TotalRedOrbs} Red Orbs to unseal Floor VI! Solve this floor's Sindar puzzle first ({RedOrbsCollected}/{CirculatingPathsRules.TotalRedOrbs}).";

        CurrentFloor++;
        AlcoveSearchedThisFloor = false;
        ShrineVisitedThisFloor = false;
        GuardianDefeatedThisFloor = false;
        GuardianPhase2Triggered = false;
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
        HealFieldPartyFull();
        int rank = ArmyProgressionRuntime.RecordCirculatingPathsClear(newCycle);
        ResetRun();
        return newCycle
            ? $"★ RUNE OF CIRCULATING PATHS ACCEPTED! Cycle Rank {rank} (+{rank * 5}% Party DMG, +{rank * 15}% Potch), +{potch:N0} Potch & +{comm} Commendations!"
            : $"★ SINDAR VAULT CLAIMED! +{potch:N0} Potch & +{comm} Commendations!";
    }

    internal static void OnBattleStart()
    {
        if (!BattleInProgress) return;
        GuardianPhase2Triggered = false;
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        Reforged2Runtime.FlashBanner($"🏛 SAJAH FLOOR {CurrentFloor}/6: {spec.GuardianName} [{spec.GuardianMechanicName}]!", 4200, "gold");
    }

    internal static void OnLeaseCancelled(long token)
    {
        if (activeLease?.Token != token) return;
        activeLease = null;
        GuardianPhase2Triggered = false;
    }

    internal static void OnAuthoritativeNonVictory(long token)
    {
        if (activeLease?.Token != token) return;
        activeLease = null;
        GuardianPhase2Triggered = false;
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        CastleGuildRuntime.TrainingFeedback($"Forced back on {spec.Name}. Your Red Orbs ({RedOrbsCollected}/{CirculatingPathsRules.TotalRedOrbs}) & Spire Blessings remain intact!");
    }

    internal static void OnAuthoritativeVictory(long token)
    {
        if (activeLease?.Token != token) return;
        activeLease = null;
        GuardianPhase2Triggered = false;
        var spec = CirculatingPathsRules.Floor(CurrentFloor);
        GuardianDefeatedThisFloor = true;
        WalkableDungeonRuntime.RefreshActorVisibility();
        if (CurrentFloor >= CirculatingPathsRules.TotalFloors)
        {
            SummitCrystalReady = true;
            CastleGuildRuntime.TrainingFeedback($"★ {spec.GuardianName} VANQUISHED! Approach the Violet Crystal (or Hooded Stranger) for the Rune of Circulating Paths!");
        }
        else
        {
            CastleGuildRuntime.TrainingFeedback($"★ {spec.GuardianName} defeated! Ascend to Floor {CurrentFloor + 1}/6.");
        }
    }

    internal static void ResetRun()
    {
        RunActive = false;
        activeLease?.CancelOrTeardown();
        activeLease = null;
        CurrentFloor = 1;
        RedOrbsCollected = 0;
        AlcoveSearchedThisFloor = false;
        ShrineVisitedThisFloor = false;
        GuardianDefeatedThisFloor = false;
        SummitCrystalReady = false;
        GuardianPhase2Triggered = false;
        ActiveBlessings.Clear();
    }
}
