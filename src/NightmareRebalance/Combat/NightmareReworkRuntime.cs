namespace NightmareRebalance;

internal sealed class EnemyTacticalState
{
    internal readonly ShieldReactionState Reaction = new();
    internal int MaxShields { get => Reaction.MaxShields; set => Reaction.MaxShields = value; }
    internal int Shields { get => Reaction.Shields; set => Reaction.Shields = value; }
    internal int StaggeredUntilTurn { get => Reaction.StaggeredUntilTurn; set => Reaction.StaggeredUntilTurn = value; }
    internal EnemyChargeTracker Charge => Reaction.Charge;
    internal bool Charging => Charge.IsChargingForUi(Framework.Singleton<BATTLE_WORK>.instance?.battle_kaisuu ?? 0);
    internal bool IsBoss { get => Reaction.IsBoss; set => Reaction.IsBoss = value; }
    internal bool IsElite { get => Reaction.IsElite; set => Reaction.IsElite = value; }
}

// Reforged 7.0: True Nightmare Combat Rework runtime.
// Strictly separates pure damage calculation (CalcPlayerDamage / CalcMonsterDamage) from
// committed native action/reaction effects (PM_DATA.DamageSetReActionData / SetMagicDamageChara).
internal static class NightmareReworkRuntime
{
    static readonly Dictionary<IntPtr, EnemyTacticalState> states = new();
    static readonly Dictionary<(IntPtr Actor, IntPtr Target), bool> pendingComboFlags = new();
    static readonly System.Random rng = new();
    static int reactionCommitDepth;
    internal static PackTrait ActivePackTrait { get; private set; }
    internal static ZoneHazard ActiveZoneHazard { get; private set; }
    internal static int PartyAvgLevel { get; private set; }
    static int lastFatigueAnnouncedTurn = -1;

    internal static bool Enabled => Plugin.Options?.EnableTrueNightmareRework.Value == true;
    internal static bool AntiOverlevelEnabled => Plugin.Options?.EnableAntiOverlevelScaling.Value == true;

    internal static bool TryEnterReactionCommit() => ++reactionCommitDepth == 1;
    internal static void ExitReactionCommit() { if (reactionCommitDepth > 0) reactionCommitDepth--; }

    internal static void ClearBattle()
    {
        states.Clear();
        pendingComboFlags.Clear();
        reactionCommitDepth = 0;
        ActivePackTrait = PackTrait.None;
        ActiveZoneHazard = ZoneHazard.Calm;
        PartyAvgLevel = 0;
        lastFatigueAnnouncedTurn = -1;
    }

    internal static void ReadyBattle(BattleState s, BATTLE_WORK w)
    {
        ClearBattle();
        if (!Enabled || !s.Ready || w.pm_data == null) return;

        // Compute live party average level for anti-overleveling scaling.
        int levelSum = 0, levelCount = 0;
        for (int i = 0; i < w.pm_data.Length && i < 6; i++)
        {
            var p = w.pm_data[i]?.TryCast<BattlePlayerCharacter>();
            if (p?.c_varia_dat is { level: > 0 } d) { levelSum += d.level; levelCount++; }
        }
        PartyAvgLevel = levelCount > 0 ? levelSum / levelCount : 0;

        ActiveZoneHazard = NightmareReworkRules.RollZoneHazard(w.battle_data?.id);
        bool hasBoss = w.pm_data.Any(u => u != null && !u.IsPlayer && u.max_hp >= 1200);
        bool isEliteBattle = Reforged2Runtime.ApexBattleActive || Reforged2Runtime.NemesisBattleActive ||
            HuntRuntime.Active != null || TowerOfDunanRuntime.BattleInProgress || SuperbossRuntime.BattleInProgress || CirculatingPathsRuntime.BattleInProgress;

        if (!hasBoss && !isEliteBattle && s.Allowed && s.StatsAllowed)
            ActivePackTrait = NightmareReworkRules.RollPackTrait(rng.Next(100));

        int customShields = SuperbossRuntime.InitialShieldsForSuperboss();
        if (customShields <= 0) customShields = CirculatingPathsRuntime.InitialShieldsForGuardian();
        foreach (var unit in w.pm_data)
        {
            if (unit == null || unit.IsPlayer || unit.hp <= 0 || unit.IsDead) continue;
            bool isBoss = unit.max_hp >= 1200 || SuperbossRuntime.BattleInProgress || CirculatingPathsRuntime.BattleInProgress;
            int shields = customShields > 0 ? customShields : NightmareReworkRules.InitialShields(isBoss, isEliteBattle, ActivePackTrait);
            states[unit.Pointer] = new EnemyTacticalState
            {
                MaxShields = shields,
                Shields = shields,
                IsBoss = isBoss,
                IsElite = isEliteBattle
            };
        }

        if (ActivePackTrait != PackTrait.None)
            Reforged2Runtime.FlashBanner($"⚔ PACK TRAIT: {NightmareReworkRules.PackTraitTitle(ActivePackTrait)}", 3800, "alert");
        else if (ActiveZoneHazard != ZoneHazard.Calm)
            Reforged2Runtime.FlashBanner($"☁ ZONE: {NightmareReworkRules.ZoneHazardTitle(ActiveZoneHazard)}", 3400, "gold");
    }

    internal static EnemyTacticalState? StateFor(PM_DATA? enemy) =>
        Enabled && enemy != null && states.TryGetValue(enemy.Pointer, out var st) ? st : null;

    internal static bool IsStaggered(EnemyTacticalState st, int currentTurn)
    {
        if (st.StaggeredUntilTurn < 0) return false;
        if (currentTurn > st.StaggeredUntilTurn)
        {
            st.StaggeredUntilTurn = -1;
            if (st.MaxShields > 0) st.Shields = st.MaxShields;
            return false;
        }
        return true;
    }

    internal static bool HasAnyActiveMechanics =>
        Enabled && (ActivePackTrait != PackTrait.None || ActiveZoneHazard != ZoneHazard.Calm ||
            states.Values.Any(st => st.MaxShields > 0 || st.Charging));

    internal static double EnemyHpScalingFor(PM_DATA unit)
    {
        double mult = 1.0;
        if (AntiOverlevelEnabled)
        {
            int enemyLv = unit.monster_data?.level ?? 0;
            mult *= NightmareReworkRules.AntiOverlevelHpMultiplier(PartyAvgLevel, enemyLv);
        }
        if (Enabled && ActiveZoneHazard == ZoneHazard.HighlandAlert) mult *= 1.15;
        return mult;
    }

    internal static double EnemyDamageScalingFor(BATTLE_WORK? w, PM_DATA attacker, bool isMagic)
    {
        int turn = w?.battle_kaisuu ?? 0;
        double mult = AntiOverlevelEnabled
            ? NightmareReworkRules.AntiOverlevelDamageMultiplier(PartyAvgLevel, attacker.monster_data?.level ?? 0)
            : 1.0;
        if (!Enabled) return mult;
        if (ActivePackTrait == PackTrait.Berserk) mult *= 1.35;
        if (ActiveZoneHazard == ZoneHazard.HighlandAlert || ActiveZoneHazard == ZoneHazard.BloodMist) mult *= 1.15;
        if (isMagic && ActiveZoneHazard == ZoneHazard.ArcaneStorm) mult *= 1.30;

        double fatigue = NightmareReworkRules.BattleFatigueMultiplier(turn, ActiveZoneHazard);
        if (fatigue > 1.0 && turn != lastFatigueAnnouncedTurn)
        {
            lastFatigueAnnouncedTurn = turn;
            Reforged2Runtime.FlashBanner($"⚠ Battle Fatigue: Enemy DMG +{(int)Math.Round((fatigue - 1.0) * 100)}%!", 2400, "alert");
        }
        mult *= fatigue;

        if (StateFor(attacker) is {} st)
        {
            bool staggered = IsStaggered(st, turn);
            mult *= st.Charge.EvaluateDamageMultiplier(turn, st.IsBoss, st.IsElite, staggered, out bool beganTelegraph, out bool beganUnleash);
            if (beganUnleash)
                Reforged2Runtime.FlashBanner("☠ CHARGED DEVASTATION UNLEASHED!", 2800, "alert");
            else if (beganTelegraph)
            {
                string name = Reforged2Runtime.ResolveEnemyDisplayName(attacker, out _);
                Reforged2Runtime.FlashBanner($"⚠ {name} CHARGING DEVASTATION — BREAK SHIELD OR DEFEND NEXT TURN!", 3800, "alert");
            }
        }
        return mult;
    }

    // PURE damage calculation only: never mutates shields, stagger, actor HP, or Momentum.
    // Repeated calculations of the same hit return identical damage.
    internal static int ApplyPlayerAttackOnEnemy(BATTLE_WORK w, BattlePlayerCharacter actor, PM_DATA? enemy, int damage, bool isMagic, bool comboProcced)
    {
        if (!Enabled || damage <= 0) return damage;
        int result = damage;
        if (isMagic && ActiveZoneHazard == ZoneHazard.ArcaneStorm)
            result = Math.Max(result + 1, (int)Math.Round(result * 1.30, MidpointRounding.AwayFromZero));
        if (ActivePackTrait == PackTrait.Berserk)
            result = Math.Max(result + 1, (int)Math.Round(result * 1.15, MidpointRounding.AwayFromZero));

        if (enemy != null)
        {
            pendingComboFlags[(actor.Pointer, enemy.Pointer)] = comboProcced;
            if (StateFor(enemy) is {} st)
                result = st.Reaction.CalculatePlayerDamage(result, w.battle_kaisuu);
        }
        if (SuperbossRuntime.BattleInProgress)
            result = SuperbossRuntime.ApplySuperbossIncomingModifiers(w, actor, enemy, result, isMagic);
        if (CirculatingPathsRuntime.BattleInProgress)
            result = CirculatingPathsRuntime.ApplyGuardianIncomingModifiers(w, actor, enemy, result, isMagic);
        return result;
    }

    // Committed native reaction hit (PM_DATA.DamageSetReActionData / SetMagicDamageChara).
    // Fires once per actual landed hit — preserves multihits, counters, AoE, and two distinct same-turn attacks,
    // while repeated calculations or cancelled/missed hits never mutate state.
    internal static void CommitPlayerHit(BATTLE_WORK? w, BattlePlayerCharacter actor, PM_DATA? enemy, int committedDamage, bool isMagic)
    {
        if (!Enabled || w == null || enemy == null || committedDamage <= 0) return;
        int turn = w.battle_kaisuu;
        bool comboProcced = pendingComboFlags.Remove((actor.Pointer, enemy.Pointer), out bool c) && c;

        if (StateFor(enemy) is {} st)
        {
            string weak = enemy.calc_para is { Length: > 3 } ? Reforged2Rules.TacticalWeaknessHint(enemy.calc_para[2], enemy.calc_para[3]) : "Balanced";
            bool hitWeak = (isMagic && weak == "Weak: Magic") || (!isMagic && weak == "Weak: Phys");
            bool broke = st.Reaction.CommitPlayerHit(turn, hitWeak, comboProcced, SuperbossRuntime.HasTrophy("awakened-dragon"), out bool interrupted);
            if (broke)
            {
                string eName = Reforged2Runtime.ResolveEnemyDisplayName(enemy, out _);
                Reforged2Runtime.FlashBanner(
                    interrupted ? $"★ {eName} STAGGERED & CHARGE INTERRUPTED! (+50% DMG Taken)" : $"💥 {eName} SHIELD BROKEN — STAGGERED! (+50% DMG Taken)",
                    3400, "success");
                Reforged2Runtime.GrantBonusMomentum(25, w);
            }
        }

        if (!isMagic && ActivePackTrait == PackTrait.Thorned && actor.hp > 1)
        {
            int reflect = NightmareReworkRules.ComputeThornedReflect(committedDamage, actor.hp);
            if (reflect > 0)
            {
                actor.hp = (short)Math.Max(1, actor.hp - reflect);
                Reforged2Runtime.FlashBanner($"🌵 Thorned Reflect! (-{reflect} HP)", 1800, "alert");
            }
        }
    }

    // PURE damage mitigation only: Defend vs Charged Devastation reduction. Never mutates HP or Momentum.
    internal static int ApplyDefendAndEnemyTraits(BATTLE_WORK? w, PM_DATA? attacker, int defenderSlot, int incomingDamage)
    {
        if (!Enabled || incomingDamage <= 0 || w?.pm_data == null || defenderSlot < 0 || defenderSlot >= w.pm_data.Length)
            return incomingDamage;
        var defender = w.pm_data[defenderSlot];
        if (defender == null || !defender.IsPlayer || defender.IsDead) return incomingDamage;

        int result = incomingDamage;
        int turn = w.battle_kaisuu;
        bool guarding = (defender.status & b_game_h.STATUS_GUARD) != 0 && (defender.status & b_game_h.STATUS_NO_GUARD) == 0;
        if (guarding && StateFor(attacker) is {} st && st.Charge.IsUnleashing(turn))
            result = Math.Max(1, (int)Math.Round(result * NightmareReworkRules.DefendVsChargedMultiplier, MidpointRounding.AwayFromZero));
        return result;
    }

    // Committed native enemy hit (PM_DATA.DamageSetReActionData / SetMagicDamageChara).
    internal static void CommitEnemyHit(BATTLE_WORK? w, PM_DATA? attacker, int defenderSlot, int committedDamage)
    {
        if (committedDamage <= 0 || w?.pm_data == null || defenderSlot < 0 || defenderSlot >= w.pm_data.Length)
            return;
        var defender = w.pm_data[defenderSlot];
        if (defender == null || !defender.IsPlayer || defender.IsDead) return;

        if (Enabled)
        {
            bool guarding = (defender.status & b_game_h.STATUS_GUARD) != 0 && (defender.status & b_game_h.STATUS_NO_GUARD) == 0;
            if (guarding)
            {
                int heal = NightmareReworkRules.ComputeDefendHeal(defender.max_hp);
                if (heal > 0 && defender.hp > 0 && defender.hp < defender.max_hp)
                    defender.hp = (short)Math.Min(defender.max_hp, defender.hp + heal);
                Reforged2Runtime.GrantBonusMomentum(20, w);
            }
            if (ActivePackTrait == PackTrait.Vampiric && attacker != null && attacker.hp > 0 && attacker.hp < attacker.max_hp)
            {
                int leech = NightmareReworkRules.ComputeVampiricHeal(committedDamage);
                if (leech > 0)
                    attacker.hp = (short)Math.Min(attacker.max_hp, attacker.hp + leech);
            }
        }
        SuperbossRuntime.OnSuperbossDealtDamage(attacker, committedDamage);
        CirculatingPathsRuntime.OnGuardianDealtDamage(attacker, committedDamage);
        if (attacker != null)
            Reforged2Runtime.ObservePlayerHit(w, attacker.chara_no, defenderSlot, committedDamage);
    }

    internal static double ZonePotchMultiplier =>
        Enabled && ActiveZoneHazard == ZoneHazard.HighlandAlert ? 1.25 : 1.0;
}
