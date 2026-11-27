using HarmonyLib;
using UnityEngine;

namespace NightmareRebalance;

internal readonly record struct CombatFeedEntry(string Text, long ExpiresAt, string Tone);

// Reforged 2.5 live RPG combat overhaul: Party Momentum & Dunan Overdrive,
// Weapon Milestone Perks, Elemental/Status/Execute Combos, Riposte & Desperation
// Surges, Boss Phase Telegraphs, Apex Packs with Elite Affixes, Battle Grade &
// MVP, Victory Streak, Hunter's Bestiary, Field Bounties, and Quartermaster
// Permanent Castle Upgrades + Item Shop.
internal static class Reforged2Runtime
{
    internal static bool ScannerVisible;
    internal static bool HelpCardVisible;
    internal static int BestiaryViewMode; // 0 = Off, 1 = Bestiary, 2 = Dunan Honors
    internal static bool BestiaryVisible => BestiaryViewMode > 0;
    internal static bool CleanHudMode;
    internal static bool ApexBattleActive;
    internal static ApexAffix ActiveApexAffix;
    internal static int Momentum;
    internal static int OverdriveUntilTurn = -1;
    internal static int WinStreak;

    static readonly List<CombatFeedEntry> feed = new(4);
    static int lastTurn = -1;
    static int spellsThisTurn;
    static bool firstActionUsed;
    static bool bossEnragedInBattle;
    static int battleCombos, battleSurges, battleOverdrives;
    static readonly Dictionary<IntPtr, int> bossPhases = new();
    static readonly Dictionary<IntPtr, int> armorBreaks = new();
    static readonly HashSet<IntPtr> surgedPlayers = new();
    static readonly HashSet<IntPtr> ripostePlayers = new();
    static readonly Dictionary<int, int> battleDamageByChara = new();
    static readonly System.Random rng = new();

    internal static bool CombosEnabled => Plugin.Options?.EnableCombatCombos.Value == true;
    internal static bool PerksEnabled => Plugin.Options?.EnableWeaponMilestonePerks.Value == true;
    internal static bool TelegraphsEnabled => Plugin.Options?.EnableBossTelegraphs.Value == true;
    internal static bool ApexEnabled => Plugin.Options?.EnableApexEncounters.Value == true;
    internal static bool SurgeEnabled => Plugin.Options?.EnableDesperationSurge.Value == true;
    internal static bool MomentumEnabled => Plugin.Options?.EnableMomentumOverdrive.Value == true;
    internal static bool BountiesEnabled => Plugin.Options?.EnableFieldBounties.Value == true;

    internal static bool OverdriveActive(BATTLE_WORK? w = null)
    {
        if (!MomentumEnabled || OverdriveUntilTurn < 0) return false;
        int turn = (w ?? Framework.Singleton<BATTLE_WORK>.instance)?.battle_kaisuu ?? 0;
        if (turn > OverdriveUntilTurn)
        {
            OverdriveUntilTurn = -1;
            Momentum = 0;
            return false;
        }
        return true;
    }

    internal static CombatFeedEntry[] ActiveFeed()
    {
        long now = Environment.TickCount64;
        for (int i = feed.Count - 1; i >= 0; i--)
            if (feed[i].ExpiresAt <= now) feed.RemoveAt(i);
        return feed.Count == 0 ? Array.Empty<CombatFeedEntry>() : feed.ToArray();
    }

    internal static bool BannerVisible => ActiveFeed().Length > 0;

    internal static void FlashBanner(string text, int durationMs = 3200, string tone = "gold")
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        long now = Environment.TickCount64;
        for (int i = feed.Count - 1; i >= 0; i--)
            if (feed[i].ExpiresAt <= now) feed.RemoveAt(i);
        if (feed.Count >= 3) feed.RemoveAt(0);
        feed.Add(new CombatFeedEntry(text, now + durationMs, tone));
    }

    internal static void ClearBattle()
    {
        lastTurn = -1;
        spellsThisTurn = 0;
        firstActionUsed = false;
        bossEnragedInBattle = false;
        battleCombos = 0;
        battleSurges = 0;
        battleOverdrives = 0;
        ApexBattleActive = false;
        ActiveApexAffix = ApexAffix.None;
        Momentum = 0;
        OverdriveUntilTurn = -1;
        bossPhases.Clear();
        armorBreaks.Clear();
        surgedPlayers.Clear();
        ripostePlayers.Clear();
        battleDamageByChara.Clear();
        feed.Clear();
    }

    internal static void TryArmApexBattle(BattleState s, BATTLE_WORK w)
    {
        ApexBattleActive = false;
        ActiveApexAffix = ApexAffix.None;
        OverdriveUntilTurn = -1;

        if (MomentumEnabled)
        {
            int startMom = (ArmyProgressionRuntime.HasUpgrade("tactical-reserve") ? 35 : 0)
                + Reforged2Rules.StreakStartingMomentum(WinStreak);
            Momentum = Math.Clamp(startMom, 0, Reforged2Rules.OverdriveThreshold - 1);
        }

        bool hasBoss = w.pm_data != null && w.pm_data.Any(u => u != null && !u.IsPlayer && u.max_hp >= 1200);
        if (hasBoss && ArmyProgressionRuntime.HasUpgrade("scout-network") && !ScannerVisible)
        {
            ScannerVisible = true;
            FlashBanner("Scout Network: Tactical Scanner auto-engaged!", 2600, "success");
        }

        if (!ApexEnabled || !s.Allowed || !s.StatsAllowed || HuntRuntime.Active != null || HallFirstContact.UsesOriginalHp() || hasBoss)
            return;
        int chance = ArmyProgressionRuntime.HasUpgrade("scout-network") ? 18 : Reforged2Rules.ApexChancePercent;
        if (Reforged2Rules.ShouldSpawnApex(rng.Next(100), chance))
        {
            ApexBattleActive = true;
            ActiveApexAffix = Reforged2Rules.RollApexAffix(rng.Next(4));
            string affixLabel = Reforged2Rules.ApexAffixTitle(ActiveApexAffix);
            FlashBanner($"⚠ APEX PACK · {affixLabel} (+1 Commendation)", 4400, "alert");
            Plugin.Logger.LogInfo($"Reforged 2.5: Apex Pack ({ActiveApexAffix}) spawned for ordinary encounter.");
        }
    }

    static void GainMomentum(int delta, BATTLE_WORK? w)
    {
        if (!MomentumEnabled || delta <= 0 || OverdriveActive(w)) return;
        Momentum = Reforged2Rules.AddMomentum(Momentum, delta, out bool triggered);
        if (triggered)
        {
            int turn = w?.battle_kaisuu ?? 0;
            OverdriveUntilTurn = turn + 1;
            battleOverdrives++;
            FlashBanner("★ DUNAN OVERDRIVE! (+20% DMG · -15% Taken)", 3800, "gold");
            if (ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.OverdrivesOrSurges) is {} msg)
                FlashBanner("★ " + msg, 3600, "success");
        }
    }

    internal static int ApplyOverdriveDefense(int incomingDamage) =>
        OverdriveActive() ? Reforged2Rules.ApplyOverdriveDefense(incomingDamage) : incomingDamage;

    internal static void ObservePlayerHit(BATTLE_WORK? w, int attackerSlot, int defenderSlot, int incomingDamage)
    {
        if (incomingDamage <= 0 || w?.pm_data == null || defenderSlot < 0 || defenderSlot >= w.pm_data.Length)
            return;
        var player = w.pm_data[defenderSlot];
        if (player == null || !player.IsPlayer || player.IsDead) return;
        int projectedHp = player.hp - incomingDamage;

        if (attackerSlot >= 0 && attackerSlot < w.pm_data.Length && w.pm_data[attackerSlot] is { max_hp: >= 1200 })
            GainMomentum(10, w);

        if (!SurgeEnabled) return;
        if (Reforged2Rules.ShouldArmDesperation(projectedHp, player.max_hp) && surgedPlayers.Add(player.Pointer))
        {
            string pName = player.chara_id > 0 ? G2_SYS.G2_name_get(player.chara_id) : "Ally";
            FlashBanner($"★ {pName}: Desperation Surge Ready! (+25% DMG)", 2800, "gold");
        }
        else if (Reforged2Rules.ShouldArmRiposte(incomingDamage, projectedHp, player.max_hp) && ripostePlayers.Add(player.Pointer))
        {
            string pName = player.chara_id > 0 ? G2_SYS.G2_name_get(player.chara_id) : "Ally";
            FlashBanner($"⚔ {pName}: Riposte Armed! (+20% Next Strike)", 2400, "success");
        }
    }

    internal static int ArmorBreakStacksFor(PM_DATA? enemy) =>
        enemy != null && armorBreaks.TryGetValue(enemy.Pointer, out int s) ? s : 0;

    internal static (int FrontAlive, int BackAlive) FormationCounts(BATTLE_WORK? w)
    {
        int front = 0, back = 0;
        if (w?.pm_data == null) return (0, 0);
        for (int i = 0; i < w.pm_data.Length && i < 6; i++)
        {
            var u = w.pm_data[i];
            if (u == null || !u.IsPlayer || u.hp <= 0 || u.IsDead) continue;
            if (i < 3) front++; else back++;
        }
        return (front, back);
    }

    internal static bool IsSRankVictory(BATTLE_WORK? w)
    {
        if (w?.pm_data == null) return false;
        int kos = w.pm_data.Count(u => u != null && u.IsPlayer && (u.hp <= 0 || u.IsDead));
        return Reforged2Rules.BattleGrade(kos, w.battle_kaisuu, battleCombos, battleSurges) == "S";
    }

    internal static int ExtraRuneChargesForVictory(BATTLE_WORK? w) =>
        ArmyProgressionRuntime.HasUpgrade("arcane-academy") && IsSRankVictory(w) ? 1 : 0;

    internal static bool IsBossEnraged(PM_DATA? unit) =>
        TelegraphsEnabled && unit != null && bossPhases.GetValueOrDefault(unit.Pointer, 0) >= 2;

    internal static double VictoryPotchMultiplier(BATTLE_WORK? w)
    {
        double mult = Reforged2Rules.StreakPotchMultiplier(WinStreak + 1);
        if (ArmyProgressionRuntime.HasUpgrade("imperial-treasury")) mult *= 1.15;
        if (bossEnragedInBattle) mult *= Reforged2Rules.BossEnragePotchMultiplier;
        if (w?.pm_data != null)
        {
            int kos = w.pm_data.Count(u => u != null && u.IsPlayer && (u.hp <= 0 || u.IsDead));
            string grade = Reforged2Rules.BattleGrade(kos, w.battle_kaisuu, battleCombos, battleSurges);
            if (grade == "S") mult *= 1.15;
        }
        return mult;
    }

    internal static void OnBattleNonVictory()
    {
        WinStreak = 0;
    }

    internal static void OnBattleVictory(BATTLE_WORK? w, int restoredCharges)
    {
        WinStreak++;
        bool wasApex = ApexBattleActive;
        if (wasApex)
        {
            ArmyProgressionRuntime.AwardCommendations(1);
            ArmyProgressionRuntime.RecordApexClear();
        }

        int kos = 0;
        int healedAllies = 0;
        int newSpecies = 0;
        int milestoneComm = 0;
        if (w?.pm_data != null)
        {
            bool fieldMedic = ArmyProgressionRuntime.HasUpgrade("field-medic-corps");
            foreach (var unit in w.pm_data)
            {
                if (unit == null) continue;
                if (unit.IsPlayer)
                {
                    if (unit.hp <= 0 || unit.IsDead) { kos++; continue; }
                    if (fieldMedic && unit.TryCast<BattlePlayerCharacter>()?.c_varia_dat is {} data && data.max_hp > 0 && data.now_hp > 0 && data.now_hp < data.max_hp)
                    {
                        int heal = Math.Max(1, (int)Math.Round(data.max_hp * 0.10, MidpointRounding.AwayFromZero));
                        data.now_hp = (ushort)Math.Min(data.max_hp, data.now_hp + heal);
                        healedAllies++;
                    }
                }
                else
                {
                    string key = unit.monster_data?.id ?? "";
                    string name = ResolveEnemyDisplayName(unit, out _);
                    if (string.IsNullOrWhiteSpace(key)) key = name;
                    int lv = unit.monster_data?.level ?? 0;
                    string weak = unit.calc_para is { Length: > 3 } ? Reforged2Rules.TacticalWeaknessHint(unit.calc_para[2], unit.calc_para[3]) : "Balanced";
                    var (isNew, mDelta) = ArmyProgressionRuntime.RecordBestiaryKill(key, name, lv, weak, wasApex);
                    if (isNew) newSpecies++;
                    milestoneComm += mDelta;
                }
            }
        }

        string grade = Reforged2Rules.BattleGrade(kos, w?.battle_kaisuu ?? 1, battleCombos, battleSurges);
        ArmyProgressionRuntime.RecordBattleStats(battleCombos, battleOverdrives, grade == "S");
        string mvpText = "";
        if (battleDamageByChara.Count > 0)
        {
            var top = battleDamageByChara.OrderByDescending(kv => kv.Value).First();
            string mvpName = top.Key > 0 ? G2_SYS.G2_name_get(top.Key) : "";
            if (!string.IsNullOrWhiteSpace(mvpName)) mvpText = $"MVP: {mvpName.Trim()} ({top.Value:N0})";
        }

        var bountyNotices = new List<string>(2);
        if (wasApex && ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.ApexPacks) is {} b1) bountyNotices.Add(b1);
        if (grade == "S" && ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.SRanks) is {} b2) bountyNotices.Add(b2);
        if (newSpecies > 0 && ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.BestiaryEntries, newSpecies) is {} b3) bountyNotices.Add(b3);
        var newHonors = ArmyProgressionRuntime.CheckHonors(WinStreak);

        var parts = new List<string>(8) { $"Grade {grade}" };
        if (mvpText.Length > 0) parts.Add(mvpText);
        if (WinStreak >= 3) parts.Add($"Streak x{WinStreak}");
        if (wasApex) parts.Add("Apex +1 Comm.");
        if (milestoneComm > 0) parts.Add($"Bestiary +{milestoneComm} Comm.");
        if (healedAllies > 0) parts.Add("Medic +10% HP");
        if (restoredCharges > 0) parts.Add($"+{restoredCharges} Rune");
        foreach (var h in newHonors) parts.Add($"Honor: {h.Title} (+2 Comm.)");
        foreach (var bn in bountyNotices) parts.Add(bn);

        CastleGuildRuntime.TrainingFeedback("Victory · " + string.Join(" · ", parts));
    }

    internal static void ToggleHelpCard() => HelpCardVisible = !HelpCardVisible;
    internal static void ToggleBestiary() => BestiaryViewMode = (BestiaryViewMode + 1) % 3;
    internal static void ToggleCleanHud()
    {
        CleanHudMode = !CleanHudMode;
        FlashBanner(CleanHudMode ? "Clean HUD Mode ON (F2 to restore)" : "Tactical HUD Restored (F2)", 1800, "muted");
    }

    internal static void ToggleScanner()
    {
        ScannerVisible = !ScannerVisible;
        if (ScannerVisible) FlashBanner("Tactical Scanner ON (F4 to hide)", 1800, "muted");
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
                if (next >= 2)
                {
                    bossEnragedInBattle = true;
                    GainMomentum(25, w);
                }
                if (msg.Length > 0) FlashBanner(msg + (next >= 2 ? " (+10% Boss DMG · +15% Potch)" : ""), 3800, "alert");
            }
        }
    }

    static void RecordDamage(BattlePlayerCharacter actor, int amount)
    {
        if (amount <= 0 || actor.chara_id <= 0) return;
        battleDamageByChara[actor.chara_id] = battleDamageByChara.GetValueOrDefault(actor.chara_id) + amount;
    }

    internal static int ApplyPlayerPhysical(BattlePlayerCharacter attacker, int mokuhyo, int damage)
    {
        if (damage <= 0 || Combat.State is not { Ready: true }) return damage;
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        if (w == null) return damage;
        int result = damage;

        if (PerksEnabled && attacker.c_varia_dat is {} data)
            result = Reforged2Rules.ApplyWeaponAttackPerk(result, data.buki_lv, ArmyProgressionRuntime.HasUpgrade("master-blacksmiths"));

        result = Reforged2Rules.ApplyGrandArmory(result, ArmyProgressionRuntime.HasUpgrade("grand-armory"));

        if (attacker.chara_no is >= 3 and < 6)
        {
            var (_, backAlive) = FormationCounts(w);
            result = Reforged2Rules.ApplyCrossfireAttack(result, backAlive);
        }

        if (ApexBattleActive && ActiveApexAffix == ApexAffix.Armored)
            result = Reforged2Rules.ApplyApexIncomingReduction(result, ActiveApexAffix, isMagic: false);

        if (OverdriveActive(w))
            result = Reforged2Rules.ApplyOverdriveAttack(result);

        if (CombosEnabled && !firstActionUsed)
        {
            var fs = Reforged2Rules.EvaluateFirstStrike(result, w.battle_kaisuu, firstActionUsed);
            firstActionUsed = true;
            if (fs.BonusDamage > 0)
            {
                result += fs.BonusDamage;
                FlashBanner($"⚡ {fs.Name}! (+{fs.BonusDamage})", 2000, "gold");
                GainMomentum(10, w);
            }
        }

        if (SurgeEnabled && surgedPlayers.Remove(attacker.Pointer))
        {
            int before = result;
            result = Reforged2Rules.ApplyDesperationSurge(result);
            battleSurges++;
            FlashBanner($"★ Desperation Surge! (+{result - before})", 2400, "gold");
            GainMomentum(30, w);
            if (ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.OverdrivesOrSurges) is {} bm)
                FlashBanner("★ " + bm, 3200, "success");
        }
        else if (SurgeEnabled && ripostePlayers.Remove(attacker.Pointer))
        {
            int before = result;
            result = Reforged2Rules.ApplyRiposte(result);
            battleSurges++;
            FlashBanner($"⚔ Riposte Counter! (+{result - before})", 2300, "success");
            GainMomentum(15, w);
        }

        if (CombosEnabled && StarBondRuntime.Enemy(w, mokuhyo) is {} enemy)
        {
            int stacks = armorBreaks.GetValueOrDefault(enemy.Pointer, 0);
            if (stacks > 0) result = Reforged2Rules.ApplyArmorBreak(result, stacks);
            if (stacks < Reforged2Rules.MaxArmorBreakStacks)
            {
                armorBreaks[enemy.Pointer] = stacks + 1;
                if (stacks + 1 == Reforged2Rules.MaxArmorBreakStacks)
                    FlashBanner("⚔ Armor Shattered! (Max Break +15% DMG)", 2100, "alert");
            }
            bool sleeping = (enemy.status & b_game_h.STATUS_SLEEP) != 0;
            bool poisoned = (enemy.status & b_game_h.STATUS_POISON) != 0;
            var combo = Reforged2Rules.EvaluatePhysicalCombo(result, sleeping, false, poisoned);
            if (combo.BonusDamage == 0)
                combo = Reforged2Rules.EvaluateExecuteFinisher(result, enemy.hp, enemy.max_hp);
            if (combo.BonusDamage > 0)
            {
                result += combo.BonusDamage;
                battleCombos++;
                FlashBanner($"★ {combo.Name}! (+{combo.BonusDamage})", 2200, "gold");
                GainMomentum(25, w);
                if (ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.Combos) is {} bm)
                    FlashBanner("★ " + bm, 3200, "success");
            }
        }

        RecordDamage(attacker, result);
        return result;
    }

    internal static int ApplyPlayerMagic(BattlePlayerCharacter caster, int damage)
    {
        if (damage <= 0 || Combat.State is not { Ready: true }) return damage;
        var w = Framework.Singleton<BATTLE_WORK>.instance;
        if (w == null) return damage;
        int result = damage;

        result = Reforged2Rules.ApplyGrandArmory(result, ArmyProgressionRuntime.HasUpgrade("grand-armory"));
        if (caster.chara_no is >= 3 and < 6)
        {
            var (_, backAlive) = FormationCounts(w);
            result = Reforged2Rules.ApplyCrossfireAttack(result, backAlive);
        }

        if (ApexBattleActive && ActiveApexAffix == ApexAffix.WardBlessed)
            result = Reforged2Rules.ApplyApexIncomingReduction(result, ActiveApexAffix, isMagic: true);

        if (OverdriveActive(w))
            result = Reforged2Rules.ApplyOverdriveAttack(result);

        if (CombosEnabled && !firstActionUsed)
        {
            var fs = Reforged2Rules.EvaluateFirstStrike(result, w.battle_kaisuu, firstActionUsed);
            firstActionUsed = true;
            if (fs.BonusDamage > 0)
            {
                result += fs.BonusDamage;
                FlashBanner($"⚡ {fs.Name}! (+{fs.BonusDamage})", 2000, "gold");
                GainMomentum(10, w);
            }
        }

        if (SurgeEnabled && surgedPlayers.Remove(caster.Pointer))
        {
            int before = result;
            result = Reforged2Rules.ApplyDesperationSurge(result);
            battleSurges++;
            FlashBanner($"★ Desperation Surge! (+{result - before})", 2400, "gold");
            GainMomentum(30, w);
            if (ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.OverdrivesOrSurges) is {} bm)
                FlashBanner("★ " + bm, 3200, "success");
        }
        if (CombosEnabled)
        {
            if (w.battle_kaisuu != lastTurn)
            {
                lastTurn = w.battle_kaisuu;
                spellsThisTurn = 0;
            }
            spellsThisTurn++;
            var combo = Reforged2Rules.EvaluateArcaneChain(result, spellsThisTurn);
            if (combo.BonusDamage > 0)
            {
                battleCombos++;
                result += combo.BonusDamage;
                FlashBanner($"★ {combo.Name}! (+{combo.BonusDamage})", 2200, "gold");
                GainMomentum(25, w);
                if (ArmyProgressionRuntime.AdvanceBounty(FieldBountyKind.Combos) is {} bm)
                    FlashBanner("★ " + bm, 3200, "success");
            }
        }

        RecordDamage(caster, result);
        return result;
    }

    internal static int ApplyDefenderWeaponPerk(BATTLE_WORK? w, int defenderSlot, int damage)
    {
        if (damage <= 1 || !PerksEnabled || w?.pm_data == null || defenderSlot < 0 || defenderSlot >= w.pm_data.Length)
            return damage;
        var player = w.pm_data[defenderSlot]?.TryCast<BattlePlayerCharacter>();
        if (player?.c_varia_dat is not {} data) return damage;
        int result = Reforged2Rules.ApplyWeaponDefensePerk(damage, data.buki_lv, ArmyProgressionRuntime.HasUpgrade("master-blacksmiths"));
        if (defenderSlot < 3)
        {
            var (frontAlive, _) = FormationCounts(w);
            result = Reforged2Rules.ApplyPhalanxDefense(result, frontAlive);
        }
        return result;
    }

    internal static bool TryPurchaseQuartermaster(int rowIndex, out string feedback)
    {
        int upgradeCount = Reforged2Rules.CastleUpgrades.Length;
        if (rowIndex >= 0 && rowIndex < upgradeCount)
            return ArmyProgressionRuntime.TryUnlockUpgrade(Reforged2Rules.CastleUpgrades[rowIndex], out feedback);

        int offerIndex = rowIndex - upgradeCount;
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
