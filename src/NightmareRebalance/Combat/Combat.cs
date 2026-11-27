using HarmonyLib;

namespace NightmareRebalance;

internal sealed class BattleState
{
    internal readonly IntPtr Manager, Work;
    internal bool Ready, Allowed, StatsAllowed, SnapshotTaken, ReportWritten;
    internal TeamworkBinding? Teamwork;
    internal string? TeamworkProfile;
    internal bool Bonds;
    internal CoverageReport? Report;
    internal EncounterIdentity? Identity;
    internal readonly Dictionary<IntPtr, BossResolution> BossPlans = new();
    internal readonly Dictionary<IntPtr, EnemyProfile> EnemyPlans = new();
    internal readonly VictoryGate Victory = new();
    internal readonly EnemyInstanceLedger ScaledEnemies = new();
    internal readonly List<C_VARIA_DAT> Eligible = new();
    internal BattleState(BattleManager manager) { Manager = manager.Pointer; Work = manager.battle_work.Pointer; }
}

internal static class Combat
{
    internal static BattleState? State;
    internal static void Clear(bool detachHunt=true)
    {
        CoverageRuntime.Emit(State?.Report, "closed-or-replaced");
        CombatReforgedDiagnostics.Finish(State);
        if(detachHunt)HuntRuntime.Detach();
        CombatReforgedRuntime.Clear();
        CombatEvolutionRuntime.BossTables.Clear();
        Reforged2Runtime.ClearBattle();
        State = null;
    }

    internal static bool Matches(BATTLE_WORK? work) => State is { Ready: true, Allowed: true } s && work != null && s.Work == work.Pointer;
    internal static bool Matches(BattleManager manager) => State?.Manager == manager.Pointer && Matches(manager.battle_work);
    internal static bool StatsMatch(BATTLE_WORK? work) => State is { Ready: true, StatsAllowed: true } s && work != null && s.Work == work.Pointer;
    internal static CoverageReport? Observe(string hook, BATTLE_WORK? work)
    {
        var report = work != null && State?.Work == work.Pointer ? State.Report : null;
        report?.Hook(hook);
        return report;
    }

    internal static void Begin(BattleManager manager)
    {
        // Formation was selected before BattleMainInit. Preserve that new
        // preparation while retiring any previous combat diagnostics.
        Clear(false);
        if (manager.battle_work == null) return;
        State = new BattleState(manager);
    }

    internal static void Ready(BattleManager manager)
    {
        var s = State;
        if (s == null || s.Manager != manager.Pointer) return;
        var w = manager.battle_work;
        // These resolved delegates are assigned by BattleMainInit from MONSTER_PARTY_SUB_DATA.
        // Preserve custom story/main/ending logic. Do not infer meaning from party IDs or flag bits.
        bool compatible = Plugin.CheckCompatibility();
        // Default path keeps v0.1's exact reads and predicate; optional diagnostics cannot block it.
        s.Allowed = compatible && w.monster_party_data != null && w.monster_bm_func == null &&
            w.monster_be_func == null && w.player_kazu > 0 && w.monster_kazu > 0;
        s.StatsAllowed = s.Allowed;
        if (Plugin.Options.EnableBossDiagnostics.Value || Plugin.Options.EnableScriptedBossOverrides.Value ||
            Plugin.Options.EnableBossProfiles.Value || Plugin.Options.EnableBossProfileDiagnostics.Value)
        {
            try
            {
                var identity = CoverageRuntime.Capture(w);
                s.Identity = identity;
                var decision = EncounterCoverage.Decide(identity, compatible, Plugin.Options.EnableScriptedBossOverrides.Value, CoverageRuntime.Selected);
                s.StatsAllowed = decision.StatScaling;
                s.Report = CoverageRuntime.Start(identity, decision, w);
            }
            catch (Exception ex)
            {
                // No optional field/symbol/diagnostic failure may widen scope or interrupt a battle.
                s.StatsAllowed = s.Allowed;
                CoverageRuntime.WarnInspection($"Encounter inspection unavailable; preserving v0.1 scope: {ex.Message}");
            }
        }
        CombatReforgedRuntime.Ready(s,w,compatible);
        HuntRuntime.Ready(s,w);
        try { CombatEvolutionRuntime.Ready(s,w,compatible);
        if(compatible&&(s.Allowed||s.StatsAllowed))CombatMasteryRuntime.ApplyUnites(w);
        if(compatible&&s.Allowed)RpgEvolutionRuntime.ApplyUnites(w); } catch(Exception e) { Plugin.Logger.LogWarning("Combat Evolution retained prior plans: "+e.Message); }
        BossTacticsRuntime.Ready(w,compatible);
        s.Ready = true;
        s.Bonds = s.Allowed && StarBondRuntime.Eligible(w);
        LegendaryBossRuntime.Ready(s,w,compatible);
        TeamworkRuntime.Ready(s,w);
        try{CombatRealityRuntime.Ready(s,w);}catch(Exception e){CombatRealityRuntime.Event("Diagnostics","InspectionUnavailable","battle",e.Message);}
        CombatReforgedDiagnostics.Begin(s,w);
        if ((Plugin.Options.EnableBossProfiles.Value || Plugin.Options.EnableBossProfileDiagnostics.Value) && w.pm_data != null)
            foreach (var unit in w.pm_data)
            {
                if (unit == null) continue;
                try { BossRuntime.Register(s, unit); }
                catch (Exception ex) { CoverageRuntime.WarnInspection($"Boss inspection unavailable; Stage A factors retained: {ex.Message}"); }
            }
        Plugin.Debug($"Battle initialized: party={w.monster_party_data?.id}, allowed={s.Allowed}, players={w.player_kazu}, enemies={w.monster_kazu}");
        Reforged2Runtime.TryArmApexBattle(s, w);
        if (s.StatsAllowed && w.pm_data != null) foreach (var unit in w.pm_data) ScaleEnemy(unit);
        MaintenanceRuntime.BaselineHp(w);
        CoverageRuntime.Emit(s.Report, "initialized");
    }

    internal static void ScaleEnemy(PM_DATA? unit)
    {
        var s = State;
        if (s is not { Ready: true, StatsAllowed: true } || unit == null || unit.IsPlayer ||
            unit.TryCast<BattleMonsterCharacter>() == null || unit.hp <= 0 || unit.max_hp == 0 || !s.ScaledEnemies.Add(unit.Pointer.ToInt64())) return;
        if (HallFirstContact.UsesOriginalHp())
            return;
        // Mutate live instances only. MONSTER_DATA templates remain vanilla, including across encounters.
        int beforeHp = unit.hp, beforeMax = unit.max_hp;
        double factor = OverhaulRuntime.Register(s, unit)?.HpMultiplier ?? BossRuntime.Plan(s, unit)?.HpMultiplier ?? Plugin.Options.EnemyHpMultiplier.Value;
        factor = CombatReforgedRuntime.Factor(s,unit,factor,true);
        if (Reforged2Runtime.ApexBattleActive) factor *= Reforged2Rules.ApexHpMultiplier;
        unit.max_hp = (ushort)Math.Max(1, Rules.Scale(unit.max_hp, factor, short.MaxValue));
        unit.hp = (short)Math.Clamp(Rules.Scale(unit.hp, factor, short.MaxValue), 1, unit.max_hp);
        s.Report?.Applied("EnemyMaxHp", unit.chara_no, beforeMax, unit.max_hp, factor);
        s.Report?.Applied("EnemyCurrentHp", unit.chara_no, beforeHp, unit.hp, factor);
        CombatRealityRuntime.Event("EnemyHp","Applied",unit.monster_data?.id??"unknown",new{Before=beforeMax,After=unit.max_hp,Factor=factor});
    }

    internal static void Win(BattleManager manager)
    {
        // The outcome, hunt journal and survivor snapshot must share the same
        // terminal gate. Late/foreign callbacks cannot reclassify an escape or
        // reopen a completed reward report.
        if (State is not {Ready:true,ReportWritten:false} s || s.Manager!=manager.Pointer ||
            manager.battle_work?.Pointer!=s.Work || !s.Victory.TryMarkVictory()) return;
        s.Teamwork=null;
        s.Bonds=false;
        MaintenanceRuntime.Charges("VictoryBeforeRewards",manager.battle_work);
        CombatReforgedDiagnostics.Outcome="VictorySignal";
        var report = Observe(nameof(BattleManager.WinEnd), manager.battle_work);
        if (report != null) report.OutcomeSignal = "WinEnd-observed";
        if (!Matches(manager) || State == null) return;
        HuntSessionRuntime.Current?.Victory();
        // Snapshot survival before result processing can revive/normalize status.
        if (State.SnapshotTaken) return;
        State.SnapshotTaken = true;
        var units = manager.battle_work.pm_data;
        if (units == null) return;
        var seen = new HashSet<IntPtr>();
        foreach (var unit in units)
        {
            if (unit == null || !unit.IsPlayer) continue;
            var player = unit.TryCast<BattlePlayerCharacter>();
            var data = player?.c_varia_dat;
            if (data == null || !seen.Add(data.Pointer)) continue;
            if (Plugin.Options.RequireSurvivingCharacter.Value && (unit.hp <= 0 || unit.IsDead)) continue;
            State.Eligible.Add(data);
        }
        State.Victory.MarkVictory();
    }

    internal static void NonVictory(BattleManager manager)
    {
        if (State is not {Ready:true,ReportWritten:false} s || s.Manager!=manager.Pointer ||
            manager.battle_work?.Pointer!=s.Work) return;
        s.Teamwork=null;
        s.Bonds=false;
        s.Victory.MarkNonVictory();
        CombatReforgedDiagnostics.Outcome="NonVictorySignal";
        var report=Observe(nameof(BattleManager.NotWinEnd),manager.battle_work);
        if(report!=null)report.OutcomeSignal="NotWinEnd-observed";
        CoverageRuntime.Emit(report,"non-victory-path");
    }

    internal static void Complete(BattleManager manager)
    {
        var report = Observe(nameof(BattleManager.battle_exit99), manager.battle_work);
        bool publish = report is { RewardCompletionObserved: false };
        if (publish) report!.RewardCompletionObserved = true;
        if (!Matches(manager) || State == null || !State.Victory.TryConsume() || !Plugin.Options.EnableRuneRegeneration.Value)
        {
            if (publish) CoverageRuntime.Emit(report, "reward-pipeline-completed");
            return;
        }
        MaintenanceRuntime.Charges("BeforeRegeneration",manager.battle_work);
        int chargeBudget = Plugin.Options.RuneChargesPerVictory.Value + (ArmyProgressionRuntime.ConsumeMoraleBonus() ? 1 : 0);
        int totalRestored = 0;
        Span<byte> values = stackalloc byte[4];
        foreach (var data in State.Eligible)
        {
            var mp = data.mp;
            if (mp == null || mp.Length != 4) continue;
            for (int i = 0; i < 4; i++) values[i] = mp[i];
            int restored = Rules.Recover(values, chargeBudget, Plugin.Options.RuneRecoveryPriority.Value, Plugin.Options.RuneRecoveryMaxLevel.Value);
            if (restored == 0) continue;
            totalRestored += restored;
            CombatReforgedDiagnostics.RuneRestored+=restored;
            CombatRealityRuntime.Event("RuneRegeneration","Applied","victory",new{Restored=restored,Priority=Plugin.Options.RuneRecoveryPriority.Value.ToString(),MaximumLevel=Plugin.Options.RuneRecoveryMaxLevel.Value});
            for (int i = 0; i < 4; i++) mp[i] = values[i];
            State.Report?.Applied("RuneChargesRestored", -1, 0, restored, 1);
            Plugin.Debug($"Victory charge recovery: character={data.Pointer}, restored={restored}");
        }
        Reforged2Runtime.OnBattleVictory(totalRestored);
        MaintenanceRuntime.Charges("AfterRegeneration",manager.battle_work);
        if (publish) CoverageRuntime.Emit(report, "reward-pipeline-completed");
    }

    internal static bool EnemyToPlayer(BATTLE_WORK w, int attacker, int target)
    {
        if (!StatsMatch(w) || w.pm_data == null || attacker < 0 || target < 0 || attacker >= w.pm_data.Length || target >= w.pm_data.Length) return false;
        var source = w.pm_data[attacker];
        var victim = w.pm_data[target];
        return source != null && !source.IsPlayer && source.TryCast<BattleMonsterCharacter>() != null && victim != null && victim.IsPlayer;
    }
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.BattleMainInit))]
internal static class InitializePatch
{
    [HarmonyPrefix] static void Prefix(BattleManager __instance) => Combat.Begin(__instance);
    [HarmonyPostfix] static void Postfix(BattleManager __instance) => Combat.Ready(__instance);
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.InitNewMonster))]
internal static class ReinforcementPatch
{
    [HarmonyPostfix] static void Postfix(BattleManager __instance, int chara_no)
    {
        var w = __instance.battle_work;
        Combat.Observe(nameof(BattleManager.InitNewMonster), w);
        if (Combat.State is { Ready: true } s && s.Manager == __instance.Pointer && w.pm_data != null && chara_no >= 0 && chara_no < w.pm_data.Length)
        {
            CoverageRuntime.ObserveEnemy(s.Report, w.pm_data[chara_no]);
            if (!s.Allowed && s.StatsAllowed)
            {
                s.StatsAllowed = false;
                CombatReforgedRuntime.Plans.Clear();
                CoverageRuntime.Emit(s.Report, "override-suspended-unexpected-reinforcement");
                return;
            }
            if (Plugin.Options.EnableBossProfiles.Value || Plugin.Options.EnableBossProfileDiagnostics.Value)
            {
                try { BossRuntime.Register(s, w.pm_data[chara_no], reinforcement: true); }
                catch (Exception ex) { CoverageRuntime.WarnInspection($"Reinforcement inspection unavailable; Stage A factors retained: {ex.Message}"); }
            }
            if (Combat.StatsMatch(w)) Combat.ScaleEnemy(w.pm_data[chara_no]);
        }
    }
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.WinEnd))]
internal static class VictoryPatch
{
    [HarmonyPostfix] static void Postfix(BattleManager __instance)
    {
        Combat.Win(__instance);
        HallFirstContact.Outcome(__instance.battle_work, true);
    }
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.NotWinEnd))]
internal static class NonVictoryPatch
{
    [HarmonyPrefix] static void Prefix(BattleManager __instance)
    {
        Combat.NonVictory(__instance);
        HallFirstContact.Outcome(__instance.battle_work, false);
    }
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.battle_exit99))]
internal static class CompletedRewardPatch
{
    // The original clears exit_work and phase_step. Run after a successful original call.
    [HarmonyPostfix] static void Postfix(BattleManager __instance)
    {
        Combat.Complete(__instance);
        if(Combat.Matches(__instance)&&CombatReforgedDiagnostics.Outcome=="VictorySignal")CombatReforgedDiagnostics.Outcome="VictoryRewardsCompleted";
        CombatReforgedDiagnostics.Finish(Combat.State);
    }
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.BattleEndWait))]
internal static class CleanupPatch { [HarmonyPrefix] static void Prefix() => Combat.Clear(); }
