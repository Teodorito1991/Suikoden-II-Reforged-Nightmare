using HarmonyLib;

namespace NightmareRebalance;

internal static class StarBondRuntime
{
    internal static int[] FieldParty()
    {
        var slots = Framework.Singleton<GAME_WORK>.instance?.party_data?.party_cha_no;
        return slots == null ? Array.Empty<int>() : slots.Take(6).Select(x => (int)x).ToArray();
    }

    internal static bool Available(BATTLE_WORK w, int actor, int unite, bool requireUniteLegal = false)
    {
        if (!Plugin.Options.EnableStarBonds.Value || Combat.State is not { Bonds: true } state ||
            state.Work != w.Pointer || w.pm_data is not {} units || actor < 0 || actor >= units.Length ||
            units[actor] is not { IsPlayer: true } owner || !Able(owner, state.Manager)) return false;

        var bond = StarBondRules.Catalogue.Single(b => b.Unite == unite);
        if (!bond.Members.Contains(owner.chara_id) || w.unite_data_table is not {} table || unite >= table.Length || w.GetUniteData(unite) is not {} native ||
            native.func_no != bond.Function || native.player_id == null || native.player_kazu != bond.Members.Length ||
            !native.player_id.Select(id => (int)id).SequenceEqual(bond.Members)) return false;

        foreach (int id in bond.Members)
        {
            var unit = units.FirstOrDefault(u => u != null && u.IsPlayer && u.chara_id == id);
            if (unit == null || !Able(unit, state.Manager) || requireUniteLegal && !unit.UniteCanCheck()) return false;
        }
        return true;
    }

    static bool Able(PM_DATA u, IntPtr manager) => u.hp > 0 && !u.IsDead &&
        u.manager?.Pointer == manager && u.ActionCanCheck();

    internal static bool Eligible(BATTLE_WORK w) => w.monster_party_data != null &&
        w.monster_bm_func == null && w.monster_be_func == null && w.monster_cm_func == null &&
        DifficultyProvenance.Difficulty(out int difficulty, out _) && difficulty == 0;

    internal static PM_DATA? Enemy(BATTLE_WORK? w, int target) =>
        w?.pm_data is {} slots && target >= 0 && target < slots.Length &&
        slots[target] is { IsPlayer: false, hp: > 0, IsDead: false } enemy ? enemy : null;

    internal static void Guard(BATTLE_WORK w, PM_DATA defender, MONSTER_ATTACK_DATA attack, ref int damage)
    {
        if (defender.chara_id == 7 && (attack.attack_zokusei & 0x10) != 0 && attack.attack_type is 1 or 2 &&
            (defender.status & b_game_h.STATUS_GUARD) != 0 &&
            (defender.status & b_game_h.STATUS_NO_GUARD) == 0 &&
            StarBondRules.Wounded(defender.hp, defender.max_hp) && Available(w, defender.chara_no, 30))
            damage -= StarBondRules.DamageBonus(damage, 8, 20);
    }

    [ThreadStatic] internal static IntPtr AttackOwner;
    [ThreadStatic] internal static IntPtr FireOwner;
    internal sealed record UniteScope(IntPtr Actor, IntPtr Action, int Target);
    [ThreadStatic] internal static UniteScope? Bandit;
}

[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.PlayerAttackDamageSet))]
internal static class StarBondAttackScope
{
    [HarmonyPrefix] static void Prefix(BattlePlayerCharacter __instance, out IntPtr __state)
    {
        __state = StarBondRuntime.AttackOwner;
        StarBondRuntime.AttackOwner = __instance.Pointer;
    }
    [HarmonyFinalizer] static Exception? Finalizer(IntPtr __state, Exception? __exception)
    { StarBondRuntime.AttackOwner = __state; return __exception; }
}

[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.CalcPlayerDamage))]
internal static class StarBondAttackPatch
{
    [HarmonyPostfix] static void Postfix(BattlePlayerCharacter __instance, int mokuhyo, ref int __result)
    {
        if (__instance.Pointer != StarBondRuntime.AttackOwner || __instance.chara_id != 30 ||
            __instance.c_koudou != battle_h.CMD_ATTACK) return;
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        if (StarBondRuntime.Enemy(w, mokuhyo) is {} enemy && (enemy.status & b_game_h.STATUS_POISON) != 0 &&
            StarBondRuntime.Available(w!, __instance.chara_no, 7))
            __result += StarBondRules.DamageBonus(__result, 8, 15);
    }
}

[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.NormalMagicDamageFunc))]
internal static class StarBondFireScope
{
    [HarmonyPrefix] static void Prefix(BattlePlayerCharacter __instance, byte magi_no, out IntPtr __state)
    {
        __state = StarBondRuntime.FireOwner;
        StarBondRuntime.FireOwner = magi_no == 2 ? __instance.Pointer : IntPtr.Zero;
    }
    [HarmonyFinalizer] static Exception? Finalizer(IntPtr __state, Exception? __exception)
    { StarBondRuntime.FireOwner = __state; return __exception; }
}

[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.PlayerCalcMagicDamage))]
internal static class StarBondFirePatch
{
    [HarmonyPostfix] static void Postfix(BattlePlayerCharacter __instance, int mokuhyo, ref int __result)
    {
        if (__instance.Pointer != StarBondRuntime.FireOwner || __instance.chara_id != 33 ||
            __instance.c_koudou != battle_h.CMD_MAGIC) return;
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        if (StarBondRuntime.Enemy(w, mokuhyo)?.calc_para is { Length: > 3 } stats &&
            stats[2] > stats[3] && StarBondRuntime.Available(w!, __instance.chara_no, 2))
            __result += StarBondRules.DamageBonus(__result, 8, 18);
    }
}

[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.UniteAttackDataSet))]
internal static class StarBondUniteScope
{
    [HarmonyPrefix] static void Prefix(BattlePlayerCharacter __instance, int unite_no,
        ACTION_DATA action_data, int mokuhyo, out StarBondRuntime.UniteScope? __state)
    {
        __state = StarBondRuntime.Bandit;
        StarBondRuntime.Bandit = unite_no == 42 && action_data != null &&
            __instance.c_koudou == battle_h.CMD_UNITE ? new(__instance.Pointer, action_data.Pointer, mokuhyo) : null;
    }

    [HarmonyFinalizer] static Exception? Finalizer(StarBondRuntime.UniteScope? __state, Exception? __exception)
    { StarBondRuntime.Bandit = __state; return __exception; }
}

[HarmonyPatch(typeof(PM_DATA), nameof(PM_DATA.DamageSetReActionData))]
internal static class StarBondUniteDamagePatch
{
    [HarmonyPrefix] static void Prefix(PM_DATA __instance, ACTION_DATA action_data, int mokuhyo, ref short damage)
    {
        if (StarBondRuntime.Bandit is not {} scope || __instance.Pointer != scope.Actor ||
            action_data?.Pointer != scope.Action || mokuhyo != scope.Target) return;
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        if (StarBondRuntime.Enemy(w, mokuhyo) is {} enemy && StarBondRules.Wounded(enemy.hp, enemy.max_hp) &&
            StarBondRuntime.Available(w!, __instance.chara_no, 42, true))
            damage = (short)Math.Min(short.MaxValue, damage + StarBondRules.DamageBonus(damage, 8, 24));
    }
}
