using HarmonyLib;
using UnityEngine;

namespace NightmareRebalance;

// Reforged 2.0 live RPG combat overhaul: Weapon Milestone Perks, Elemental/Status
// Combos, Boss Phase Telegraphs, Toggleable Enemy Tactical Scanner (F4), and
// Castle Quartermaster Commendation Shop delivery.
internal static class Reforged2Runtime
{
    internal static bool ScannerVisible;
    internal static string BannerText = "";
    static long bannerUntil;
    static int lastTurn = -1;
    static int spellsThisTurn;
    static readonly Dictionary<IntPtr, int> bossPhases = new();

    internal static bool BannerVisible => Environment.TickCount64 < bannerUntil && !string.IsNullOrEmpty(BannerText);
    internal static bool CombosEnabled => Plugin.Options?.EnableCombatCombos.Value == true;
    internal static bool PerksEnabled => Plugin.Options?.EnableWeaponMilestonePerks.Value == true;
    internal static bool TelegraphsEnabled => Plugin.Options?.EnableBossTelegraphs.Value == true;

    internal static void ClearBattle()
    {
        lastTurn = -1;
        spellsThisTurn = 0;
        bossPhases.Clear();
        BannerText = "";
        bannerUntil = 0;
    }

    internal static void FlashBanner(string text, int durationMs = 3400)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        BannerText = text;
        bannerUntil = Environment.TickCount64 + durationMs;
    }

    internal static void ToggleScanner()
    {
        ScannerVisible = !ScannerVisible;
        if (ScannerVisible) FlashBanner("Tactical Scanner ON (F4 to hide)", 1800);
    }

    internal static string ResolveEnemyDisplayName(PM_DATA unit, out string role)
    {
        string id = unit.monster_data?.id ?? "";
        if (OverhaulRuntime.Enemies.TryGetValue(id, out var prof) && !string.IsNullOrWhiteSpace(prof.Name))
        {
            role = prof.Role;
            return prof.Name;
        }
        role = unit.max_hp >= 1200 ? "Boss" : "Enemy";
        try
        {
            if (unit.monster_data != null && unit.monster_data.name_id > 0)
            {
                string nativeName = G2_SYS.G2_name_get(unit.monster_data.name_id);
                if (!string.IsNullOrWhiteSpace(nativeName)) return nativeName.Trim();
            }
        }
        catch { }
        return string.IsNullOrWhiteSpace(id) ? "Enemy" : id;
    }

    internal static void CheckBossTelegraphs(BATTLE_WORK? w)
    {
        if (!TelegraphsEnabled || w?.pm_data == null) return;
        foreach (var unit in w.pm_data)
        {
            if (unit == null || unit.IsPlayer || unit.hp <= 0 || unit.IsDead || unit.max_hp < 1200) continue;
            int prev = bossPhases.GetValueOrDefault(unit.Pointer, 0);
            string name = ResolveEnemyDisplayName(unit, out _);
            string msg = Reforged2Rules.BossPhaseBanner(name, unit.hp, unit.max_hp, prev, out int next);
            if (next != prev)
            {
                bossPhases[unit.Pointer] = next;
                if (msg.Length > 0) FlashBanner(msg, 3800);
            }
        }
    }

    internal static int ApplyPlayerPhysical(BattlePlayerCharacter attacker, int mokuhyo, int damage)
    {
        if (damage <= 0 || Combat.State is not { Ready: true }) return damage;
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        if (w == null) return damage;
        int result = damage;

        if (PerksEnabled && attacker.c_varia_dat is {} data)
            result = Reforged2Rules.ApplyWeaponAttackPerk(result, data.buki_lv);

        if (CombosEnabled && StarBondRuntime.Enemy(w, mokuhyo) is {} enemy)
        {
            bool sleeping = (enemy.status & b_game_h.STATUS_SLEEP) != 0;
            bool poisoned = (enemy.status & b_game_h.STATUS_POISON) != 0;
            var combo = Reforged2Rules.EvaluatePhysicalCombo(result, sleeping, false, poisoned);
            if (combo.BonusDamage > 0)
            {
                result += combo.BonusDamage;
                FlashBanner($"★ {combo.Name}! (+{combo.BonusDamage})", 2200);
            }
        }
        return result;
    }

    internal static int ApplyPlayerMagic(BattlePlayerCharacter caster, int damage)
    {
        if (damage <= 0 || !CombosEnabled || Combat.State is not { Ready: true }) return damage;
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        if (w == null) return damage;
        if (w.battle_kaisuu != lastTurn)
        {
            lastTurn = w.battle_kaisuu;
            spellsThisTurn = 0;
        }
        spellsThisTurn++;
        var combo = Reforged2Rules.EvaluateArcaneChain(damage, spellsThisTurn);
        if (combo.BonusDamage > 0)
        {
            FlashBanner($"★ {combo.Name}! (+{combo.BonusDamage})", 2200);
            return damage + combo.BonusDamage;
        }
        return damage;
    }

    internal static int ApplyDefenderWeaponPerk(BATTLE_WORK? w, int defenderSlot, int damage)
    {
        if (damage <= 1 || !PerksEnabled || w?.pm_data == null || defenderSlot < 0 || defenderSlot >= w.pm_data.Length)
            return damage;
        var player = w.pm_data[defenderSlot]?.TryCast<BattlePlayerCharacter>();
        if (player?.c_varia_dat is not {} data) return damage;
        return Reforged2Rules.ApplyWeaponDefensePerk(damage, data.buki_lv);
    }

    internal static bool TryPurchaseQuartermaster(int offerIndex, out string feedback)
    {
        if (offerIndex < 0 || offerIndex >= Reforged2Rules.QuartermasterStock.Length)
        {
            feedback = "Invalid Quartermaster item.";
            return false;
        }
        var offer = Reforged2Rules.QuartermasterStock[offerIndex];
        if (ArmyProgressionRuntime.Current.Commendations < offer.CostCommendations)
        {
            feedback = $"Need {offer.CostCommendations} Commendation(s) (you have {ArmyProgressionRuntime.Current.Commendations}).";
            return false;
        }
        var game = Framework.Singleton<GAME_WORK>.instance;
        var bag = game?.party_data?.party_item;
        if (game == null || bag == null || bag.Length != 30)
        {
            feedback = "Party inventory is unavailable right now.";
            return false;
        }
        var audit = HuntGuildRuntime.Items.FirstOrDefault(x => x.Id == offer.ItemId);
        if (audit == null)
        {
            feedback = "Audited item definition is unavailable.";
            return false;
        }
        int emptySlot = -1;
        for (int i = 0; i < bag.Length; i++)
        {
            if (bag[i] != null && bag[i].item_no == 0) { emptySlot = i; break; }
        }
        if (emptySlot < 0)
        {
            feedback = "Your party bag is full (30/30). Make room first.";
            return false;
        }
        var nativeData = G2_SYS.G2_data_get(game.ovit_data, audit.Kind, audit.Number);
        if (nativeData == null)
        {
            feedback = "Native item template unavailable on this map.";
            return false;
        }
        if (G2_SYS.G2_item_search(audit.Kind, audit.Number, 0) < 0)
            G2_SYS.G2_have_data_put(audit.Kind, audit.Number, nativeData);

        bag[emptySlot].item_no = (byte)audit.Number;
        bag[emptySlot].use_cnt = (byte)audit.UseCount;
        ArmyProgressionRuntime.SpendCommendations(offer.CostCommendations);
        feedback = $"Acquired {offer.Name}! (-{offer.CostCommendations} Commendations; save to persist ledger.)";
        Plugin.Logger.LogInfo($"Quartermaster: granted {offer.Name} (kind={audit.Kind}, no={audit.Number}) for {offer.CostCommendations} Commendations.");
        return true;
    }
}

[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.CalcPlayerDamage))]
internal static class Reforged2PhysicalPatch
{
    [HarmonyPostfix]
    static void Postfix(BattlePlayerCharacter __instance, int mokuhyo, ref int __result)
    {
        if (__instance.c_koudou != battle_h.CMD_ATTACK) return;
        __result = Reforged2Runtime.ApplyPlayerPhysical(__instance, mokuhyo, __result);
        Reforged2Runtime.CheckBossTelegraphs(Framework.Singleton<BATTLE_WORK>.instance);
    }
}

[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.PlayerCalcMagicDamage))]
internal static class Reforged2MagicPatch
{
    [HarmonyPostfix]
    static void Postfix(BattlePlayerCharacter __instance, ref int __result)
    {
        if (__instance.c_koudou != battle_h.CMD_MAGIC) return;
        __result = Reforged2Runtime.ApplyPlayerMagic(__instance, __result);
        Reforged2Runtime.CheckBossTelegraphs(Framework.Singleton<BATTLE_WORK>.instance);
    }
}
