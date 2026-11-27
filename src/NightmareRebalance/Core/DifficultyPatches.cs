using HarmonyLib;

namespace NightmareRebalance;

internal static class DamageScope
{
    internal static bool Enter() => DamageScaling.Enter();
    internal static Exception? Exit(Exception? exception) => DamageScaling.Exit(exception);
    internal static void Apply(bool outer, bool eligible, BATTLE_WORK? work, int attacker, int target, ref int result)
    {
        if (outer && eligible && result > 0)
        {
            int before = result;
            var state = Combat.State;
            double factor = Plugin.Options.EnemyDamageMultiplier.Value;
            if (state != null && work?.pm_data is { } units && attacker >= 0 && attacker < units.Length && units[attacker] is { } unit)
                factor = state.EnemyPlans.GetValueOrDefault(unit.Pointer)?.DamageMultiplier ?? BossRuntime.Plan(state, unit)?.DamageMultiplier ?? factor;
            if (state != null && work?.pm_data is {} live && attacker>=0 && attacker<live.Length && live[attacker] is {} acting) factor=CombatReforgedRuntime.Factor(state,acting,factor,false);
            if (Reforged2Runtime.ApexBattleActive) factor *= Reforged2Rules.ApexDamageMultiplier;
            if (DamageScaling.Apply(outer, eligible, ref result, factor))
            {
                CombatReforgedDiagnostics.Calculated(attacker,target,before,result,factor);
                MaintenanceRuntime.Scaled(attacker,target,before,result,factor);
                CombatRealityRuntime.Event("EnemyDamage","Applied",attacker.ToString(),new{Before=before,After=result,Factor=factor,Target=target});
                state?.Report?.Applied("EnemyDamage", attacker, before, result, factor);
            }
        }
    }
}

[HarmonyPatch(typeof(BattleMonsterCharacter), nameof(BattleMonsterCharacter.CalcMonsterDamage))]
internal static class EnemyDamagePatch
{
    [HarmonyPrefix] internal static void Prefix(out bool __state) => __state = DamageScope.Enter();
    [HarmonyPostfix] internal static void Postfix(BattleMonsterCharacter __instance, int mokuhyo, int attack_mode, bool __state, System.Reflection.MethodBase __originalMethod, ref int __result)
    {
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        Combat.Observe(__originalMethod.Name, w);
        MaintenanceRuntime.Raw(__instance.chara_no,mokuhyo,__result,__originalMethod.Name);
        bool eligible=w!=null&&Combat.EnemyToPlayer(w,__instance.chara_no,mokuhyo);
        DamageScope.Apply(__state,eligible,w,__instance.chara_no,mokuhyo,ref __result);
        if (__state && eligible && __result > 1 && __instance.monster_data?.attack_data is {} perkMoves &&
            attack_mode >= 0 && attack_mode < perkMoves.Length && perkMoves[attack_mode] is { attack_type: 1 })
            __result = Reforged2Runtime.ApplyDefenderWeaponPerk(w, mokuhyo, __result);
        if (__state && eligible && __result > 0 && w!.pm_data[mokuhyo] is {} defender &&
            __instance.monster_data?.attack_data is {} moves && attack_mode >= 0 && attack_mode < moves.Length &&
            moves[attack_mode] is {} move)
            StarBondRuntime.Guard(w,defender,move,ref __result);
        if (__state && eligible && __result >= 10 && CombatReforgedRuntime.Config.Enabled &&
            CombatReforgedRuntime.Config.EnableArcaneGuard && w!.pm_data[mokuhyo] is { hp: > 0, IsDead: false } player &&
            (player.status & b_game_h.STATUS_GUARD) != 0 && (player.status & b_game_h.STATUS_NO_GUARD) == 0 &&
            __instance.monster_data?.attack_data is {} attacks && attack_mode >= 0 && attack_mode < attacks.Length &&
            attacks[attack_mode] is { attack_type: 2, attack: > 0 } action && (action.attack_zokusei & 0x10) == 0 &&
            TeamworkRuntime.Bound(w) is {} team && team.Roster.ContainsValue(__instance.Pointer))
        {
            int before=__result;
            __result=TacticalTeamwork.ArcaneGuard(before);
            MaintenanceRuntime.Ai("ArcaneGuard",__instance.chara_no,new{Target=mokuhyo,Before=before,After=__result});
        }
        if (__state && eligible && __result > 0) Reforged2Runtime.ObservePlayerHit(w, mokuhyo, __result);
    }
    [HarmonyFinalizer] internal static Exception? Finalizer(Exception? __exception) => DamageScope.Exit(__exception);
}

[HarmonyPatch(typeof(boss_las), nameof(boss_las.CalcMonsterDamageLast))]
internal static class FinalBossDamagePatch
{
    [HarmonyPrefix] internal static void Prefix(out bool __state) => __state = DamageScope.Enter();
    [HarmonyPostfix] static void Postfix(BATTLE_WORK battle_work, int chara_no, int mokuhyo, bool __state, ref int __result)
    {
        Combat.Observe(nameof(boss_las.CalcMonsterDamageLast), battle_work);
        DamageScope.Apply(__state, Combat.EnemyToPlayer(battle_work, chara_no, mokuhyo), battle_work, chara_no, mokuhyo, ref __result);
    }
    [HarmonyFinalizer] internal static Exception? Finalizer(Exception? __exception) => DamageScope.Exit(__exception);
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.calc_exp))]
internal static class ExperiencePatch
{
    [HarmonyPostfix] static void Postfix(BATTLE_WORK battle_work)
    {
        Combat.Observe(nameof(BattleManager.calc_exp), battle_work);
        if (!Combat.Matches(battle_work) || Plugin.Options.ExperienceMultiplier.Value == 1) return;
        var exp = battle_work.exit_work?.exp;
        if (exp == null) return;
        // Vanilla recalculates this table from base rewards on each invocation.
        for (int i = 0; i < exp.Length; i++)
        {
            int before = exp[i];
            exp[i] = Rules.Scale(exp[i], Plugin.Options.ExperienceMultiplier.Value, int.MaxValue);
            Combat.State?.Report?.Applied("Experience", i, before, exp[i], Plugin.Options.ExperienceMultiplier.Value);
        }
    }
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.get_monster_okane))]
internal static class PotchPatch
{
    [HarmonyPrefix]static void Prefix()=>HuntRuntime.RecalculateReward();
    [HarmonyPostfix] static void Postfix(BATTLE_WORK battle_work)
    {
        HallFirstContact.PrepareStone(battle_work,battle_work.exit_work);
        Combat.Observe(nameof(BattleManager.get_monster_okane), battle_work);
        if (!Combat.Matches(battle_work)) return;
        var exit = battle_work.exit_work;
        // Return value is not assumed to be potch. The verified reward field is monster_okane.
        if (exit != null)
        {
            int before = exit.monster_okane;
            double potchFactor = Plugin.Options.PotchMultiplier.Value * (Reforged2Runtime.ApexBattleActive ? Reforged2Rules.ApexPotchMultiplier : 1.0);
            exit.monster_okane = Rules.Scale(exit.monster_okane, potchFactor, int.MaxValue);
            int configured=exit.monster_okane;
            HuntRuntime.Reward(battle_work,exit);
            try{HuntRuntime.ObserveCalculation(battle_work,before,configured);}catch(Exception e){Plugin.Debug("Hunt reward observation unavailable: "+e.Message);}
            Combat.State?.Report?.Applied("Potch", -1, before, exit.monster_okane, Plugin.Options.PotchMultiplier.Value);
        }
    }
}

[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.CalcMonsterHangekiDamage))]
internal static class EnemyCounterDamagePatch
{
 [HarmonyPrefix]static void Prefix(out bool __state)=>EnemyDamagePatch.Prefix(out __state);
 [HarmonyPostfix]static void Postfix(BattleMonsterCharacter __instance,int mokuhyo,bool __state,System.Reflection.MethodBase __originalMethod,ref int __result)=>EnemyDamagePatch.Postfix(__instance,mokuhyo,-1,__state,__originalMethod,ref __result);
 [HarmonyFinalizer]static Exception? Finalizer(Exception? __exception)=>EnemyDamagePatch.Finalizer(__exception);
}
