namespace NightmareRebalance;

internal enum HunterRank { Unranked = 0, Bronze = 1, Silver = 2, Gold = 3, Master = 4 }

internal enum ArmyStance { Balanced = 0, AllOutAssault = 1, IronPhalanx = 2, SwiftMarch = 3 }

internal enum WarMutator { Standard = 0, BloodMoon = 1, IronAttrition = 2 }

internal enum ApexAffix { None = 0, Frenzied = 1, Armored = 2, WardBlessed = 3, Golden = 4 }

internal enum FieldBountyKind { Combos = 0, SRanks = 1, ApexPacks = 2, BestiaryEntries = 3, OverdrivesOrSurges = 4 }

internal sealed record CastleUpgradeSpec(
    string Id,
    string Name,
    int CostCommendations,
    string Description);

internal sealed record FieldBountySpec(
    FieldBountyKind Kind,
    string Title,
    int Target,
    string Description);

internal sealed record SkirmishSpec(
    string Id,
    string Name,
    string Subtitle,
    string Description,
    byte BaseWarNumber,
    int EnemyAtkBonus,
    int EnemyDefBonus,
    bool GrantEnemyCritical,
    int BasePotch,
    int Commendations);

internal sealed record QuartermasterOffer(
    string ItemId,
    string Name,
    int CostCommendations,
    string Category,
    string Description);

internal readonly record struct WeaponPerkBonus(int WeaponLevel, string TierName, double AttackMultiplier, double PhysicalDamageTakenMultiplier);

internal readonly record struct CombatComboResult(string Name, int BonusDamage);

internal static class Reforged2Rules
{
    internal static readonly SkirmishSpec[] Skirmishes =
    {
        new("crimson-vanguard", "Skirmish I: Crimson", "Custom War · Map 1",
            "Custom tactical clash against a reinforced Highland vanguard. No turn-3 cutoff; rout the enemy commander.",
            BaseWarNumber: 1, EnemyAtkBonus: 2, EnemyDefBonus: 1, GrantEnemyCritical: false, BasePotch: 1800, Commendations: 2),
        new("ironclad-siege", "Skirmish II: Siege", "Custom War · Fort Map",
            "Command all four allied squads against armored siege forces. Hold the fort or rout the enemy commander.",
            BaseWarNumber: 2, EnemyAtkBonus: 1, EnemyDefBonus: 2, GrantEnemyCritical: false, BasePotch: 2200, Commendations: 3),
        new("highland-grand-army", "Skirmish III: Grand", "Custom War · Map 9",
            "Elite Highland Grand Army showdown with Veteran tactics, boosted enemy commanders, and high Commendation spoils.",
            BaseWarNumber: 3, EnemyAtkBonus: 2, EnemyDefBonus: 2, GrantEnemyCritical: true, BasePotch: 2800, Commendations: 4),
        new("cavalry-blitz", "Skirmish IV: Blitz", "Custom War · Map 11",
            "High-tempo clash against Kiba's reinforced strike force (+3 ATK, Critical). Outmaneuver and break their line.",
            BaseWarNumber: 6, EnemyAtkBonus: 3, EnemyDefBonus: 1, GrantEnemyCritical: true, BasePotch: 3200, Commendations: 4),
        new("nightmare-front", "Skirmish V: Nightmare", "Custom War · Map 8",
            "Endgame Rockaxe Front gauntlet against elite Highland forces (+3 ATK / +3 DEF, Critical). Maximum spoils.",
            BaseWarNumber: 13, EnemyAtkBonus: 3, EnemyDefBonus: 3, GrantEnemyCritical: true, BasePotch: 4000, Commendations: 5),
        new("iron-vanguard", "Skirmish VI: Iron", "Custom War · Map 1",
            "Armored vanguard clash (+3 ATK / +2 DEF, Critical). Break through fortified frontline commanders.",
            BaseWarNumber: 1, EnemyAtkBonus: 3, EnemyDefBonus: 2, GrantEnemyCritical: true, BasePotch: 3400, Commendations: 4),
        new("fortress-gauntlet", "Skirmish VII: Bastion", "Custom War · Fort Map",
            "Heavy siege endurance test (+2 ATK / +4 DEF, Critical). Outlast and dismantle the armored assault.",
            BaseWarNumber: 2, EnemyAtkBonus: 2, EnemyDefBonus: 4, GrantEnemyCritical: true, BasePotch: 3800, Commendations: 5),
        new("imperial-apex", "Skirmish VIII: Apex", "Custom War · Map 8",
            "Ultimate endgame Highland Imperial Army showdown (+4 ATK / +4 DEF, Critical). Highest Commendation payout.",
            BaseWarNumber: 13, EnemyAtkBonus: 4, EnemyDefBonus: 4, GrantEnemyCritical: true, BasePotch: 5000, Commendations: 6),
        new("twin-blades", "Skirmish IX: Blades", "Custom War · Map 11",
            "Relentless Kiba elite strike force (+4 ATK / +3 DEF, Critical). Survive the charge and shatter their flank.",
            BaseWarNumber: 6, EnemyAtkBonus: 4, EnemyDefBonus: 3, GrantEnemyCritical: true, BasePotch: 4600, Commendations: 6),
        new("dunan-finale", "Skirmish X: Finale", "Custom War · Map 9",
            "Grand Championship showdown on Muse Plain (+5 ATK / +5 DEF, Critical). Ultimate test for Marshal-rank armies.",
            BaseWarNumber: 3, EnemyAtkBonus: 5, EnemyDefBonus: 5, GrantEnemyCritical: true, BasePotch: 6000, Commendations: 8)
    };

    internal static readonly QuartermasterOffer[] QuartermasterStock =
    {
        new("mega-medicine", "Mega Medicine (x3)", 1, "Consumable", "Restores 500 HP per use. Reliable emergency healing for Nightmare runs."),
        new("sacrificial-jizo", "Sacrificial Jizo", 2, "Accessory", "Automatically revives the holder once upon KO in battle."),
        new("power-stone", "Stone of Power", 3, "Growth Stone", "Permanently increases a character's Strength."),
        new("stone-of-magic", "Stone of Magic", 3, "Growth Stone", "Permanently increases a character's Magic."),
        new("magic-protection-stone", "Stone of Magic Prot.", 3, "Growth Stone", "Permanently increases a character's Magic Defense."),
        new("stone-of-speed", "Stone of Speed", 3, "Growth Stone", "Permanently increases a character's Speed."),
        new("earth-orb", "Earth Orb", 4, "Rune Orb", "Classic defensive Earth magic rune."),
        new("flowing-orb", "Flowing Orb", 6, "Rare Rune Orb", "High-tier Water healing & revival rune."),
        new("cyclone-orb", "Cyclone Orb", 6, "Rare Rune Orb", "High-tier Wind storm & healing rune."),
        new("mother-earth-orb", "Mother Earth Orb", 6, "Rare Rune Orb", "High-tier Earth barrier & offensive rune.")
    };

    internal static CommanderProfile ApplySkirmishEnemyBoost(CommanderProfile source, SkirmishSpec spec)
    {
        var abilities = new List<string>(source.Abilities);
        var uses = new List<byte>(source.Uses);
        if (spec.GrantEnemyCritical && !abilities.Contains("SP_CRITICAL") && abilities.Count < 3)
        {
            abilities.Add("SP_CRITICAL");
            uses.Add(0);
        }
        return new CommanderProfile
        {
            NameId = source.NameId,
            Name = source.Name,
            Branch = source.Branch,
            Attack = (byte)Math.Clamp(source.Attack + spec.EnemyAtkBonus, 0, 20),
            Defense = (byte)Math.Clamp(source.Defense + spec.EnemyDefBonus, 0, 20),
            AssistAttack = source.AssistAttack,
            AssistDefense = source.AssistDefense,
            Abilities = abilities.ToArray(),
            Uses = uses.ToArray()
        };
    }

    internal static HunterRank HunterRankForClears(int huntsCompleted) => huntsCompleted switch
    {
        >= 15 => HunterRank.Master,
        >= 8 => HunterRank.Gold,
        >= 4 => HunterRank.Silver,
        >= 1 => HunterRank.Bronze,
        _ => HunterRank.Unranked
    };

    internal static string HunterRankTitle(HunterRank rank) => rank switch
    {
        HunterRank.Master => "Master Hunter",
        HunterRank.Gold => "Gold Hunter",
        HunterRank.Silver => "Silver Hunter",
        HunterRank.Bronze => "Bronze Hunter",
        _ => "Unranked"
    };

    internal static double HunterPotchBonusMultiplier(HunterRank rank) => rank switch
    {
        HunterRank.Master => 1.25,
        HunterRank.Gold => 1.15,
        HunterRank.Silver => 1.10,
        HunterRank.Bronze => 1.05,
        _ => 1.00
    };

    internal static WeaponPerkBonus WeaponPerk(int weaponLevel) => weaponLevel switch
    {
        >= 16 => new(weaponLevel, "Masterwork", 1.14, 0.90),
        >= 12 => new(weaponLevel, "Battle Tempo", 1.10, 0.94),
        >= 8 => new(weaponLevel, "Keen Edge", 1.06, 1.00),
        _ => new(weaponLevel, "", 1.00, 1.00)
    };

    internal static int ApplyWeaponAttackPerk(int damage, int weaponLevel)
    {
        if (damage <= 0) return damage;
        var perk = WeaponPerk(weaponLevel);
        if (perk.AttackMultiplier <= 1.0) return damage;
        return Math.Max(damage + 1, (int)Math.Round(damage * perk.AttackMultiplier, MidpointRounding.AwayFromZero));
    }

    internal static int ApplyWeaponDefensePerk(int damage, int weaponLevel)
    {
        if (damage <= 1) return damage;
        var perk = WeaponPerk(weaponLevel);
        if (perk.PhysicalDamageTakenMultiplier >= 1.0) return damage;
        return Math.Max(1, (int)Math.Round(damage * perk.PhysicalDamageTakenMultiplier, MidpointRounding.AwayFromZero));
    }

    internal static CombatComboResult EvaluatePhysicalCombo(int baseDamage, bool targetSleeping, bool targetBalloon, bool targetPoisoned)
    {
        if (baseDamage <= 0) return default;
        if (targetSleeping || targetBalloon)
            return new("Shatter Strike", Math.Clamp((int)Math.Round(baseDamage * 0.20, MidpointRounding.AwayFromZero), 4, 80));
        if (targetPoisoned)
            return new("Venom Rend", Math.Clamp((int)Math.Round(baseDamage * 0.15, MidpointRounding.AwayFromZero), 3, 60));
        return default;
    }

    internal static CombatComboResult EvaluateArcaneChain(int baseDamage, int spellsCastThisTurn)
    {
        if (baseDamage <= 0 || spellsCastThisTurn < 2) return default;
        if (spellsCastThisTurn >= 3)
            return new("Elemental Overload", Math.Clamp((int)Math.Round(baseDamage * 0.30, MidpointRounding.AwayFromZero), 10, 140));
        return new("Arcane Chain", Math.Clamp((int)Math.Round(baseDamage * 0.18, MidpointRounding.AwayFromZero), 5, 90));
    }

    internal static CombatComboResult EvaluateFirstStrike(int baseDamage, int turn, bool firstActionUsed)
    {
        if (baseDamage <= 0 || firstActionUsed || turn > 1) return default;
        return new("First Strike", Math.Clamp((int)Math.Round(baseDamage * 0.12, MidpointRounding.AwayFromZero), 4, 65));
    }

    internal const double BossEnrageDamageMultiplier = 1.10;
    internal const double BossEnragePotchMultiplier = 1.15;

    internal static string BossPhaseBanner(string bossName, int hp, int maxHp, int previousPhase, out int newPhase)
    {
        newPhase = previousPhase;
        if (maxHp <= 0 || hp <= 0) return "";
        double frac = (double)hp / maxHp;
        if (frac <= 0.25 && previousPhase < 2)
        {
            newPhase = 2;
            return $"{bossName} enters Desperation Phase — Guard!";
        }
        if (frac <= 0.50 && previousPhase < 1)
        {
            newPhase = 1;
            return $"{bossName}'s fury rises — High Pressure!";
        }
        return "";
    }

    internal const double ApexHpMultiplier = 1.30;
    internal const double ApexDamageMultiplier = 1.15;
    internal const double ApexPotchMultiplier = 2.00;
    internal const int ApexChancePercent = 10;

    internal static bool ShouldSpawnApex(int roll0To99, int chancePercent = ApexChancePercent) =>
        roll0To99 >= 0 && roll0To99 < Math.Clamp(chancePercent, 0, 100);

    internal static bool ShouldArmDesperation(int hpAfterHit, int maxHp) =>
        maxHp > 0 && hpAfterHit > 0 && hpAfterHit * 4 <= maxHp;

    internal static int ApplyDesperationSurge(int damage) =>
        damage <= 0 ? damage : Math.Max(damage + 1, (int)Math.Round(damage * 1.25, MidpointRounding.AwayFromZero));

    internal static string TacticalWeaknessHint(int pdf, int mdf)
    {
        if (pdf >= mdf + 15) return "Weak: Magic";
        if (mdf >= pdf + 15) return "Weak: Phys";
        return "Balanced";
    }

    internal static string StanceTitle(ArmyStance stance) => stance switch
    {
        ArmyStance.AllOutAssault => "All-Out Assault",
        ArmyStance.IronPhalanx => "Iron Phalanx",
        ArmyStance.SwiftMarch => "Swift March",
        _ => "Balanced Formation"
    };

    internal static string StanceDescription(ArmyStance stance) => stance switch
    {
        ArmyStance.AllOutAssault => "Allied war units gain +1 ATK and -1 DEF.",
        ArmyStance.IronPhalanx => "Allied war units gain +1 DEF and -1 ATK.",
        ArmyStance.SwiftMarch => "Allied war units gain +1 active ability use (max 3).",
        _ => "Standard allied war formation with no trade-offs."
    };

    internal static CommanderProfile ApplyStance(CommanderProfile source, ArmyStance stance)
    {
        if (stance == ArmyStance.Balanced) return source;
        int atk = source.Attack, def = source.Defense;
        var uses = source.Uses.ToArray();
        switch (stance)
        {
            case ArmyStance.AllOutAssault:
                atk++; def--;
                break;
            case ArmyStance.IronPhalanx:
                def++; atk--;
                break;
            case ArmyStance.SwiftMarch:
                for (int i = 0; i < source.Abilities.Length && i < uses.Length; i++)
                {
                    int id = Array.IndexOf(OverhaulRules.Abilities, source.Abilities[i]);
                    if (OverhaulRules.ActiveAbility(id) && uses[i] < 3) { uses[i]++; break; }
                }
                break;
        }
        return new CommanderProfile
        {
            NameId = source.NameId,
            Name = source.Name,
            Branch = source.Branch,
            Attack = (byte)Math.Clamp(atk, 0, 20),
            Defense = (byte)Math.Clamp(def, 0, 20),
            AssistAttack = source.AssistAttack,
            AssistDefense = source.AssistDefense,
            Abilities = source.Abilities.ToArray(),
            Uses = uses
        };
    }

    internal static readonly CastleUpgradeSpec[] CastleUpgrades =
    {
        new("field-medic-corps", "Castle: Field Medic Corps", 4,
            "Permanent Upgrade · Living party members recover 10% max HP after every battle victory."),
        new("master-blacksmiths", "Castle: Master Blacksmiths", 5,
            "Permanent Upgrade · Weapon Milestone Perks unlock 2 levels earlier (Lv 6 / 10 / 14)."),
        new("scout-network", "Castle: Scout Network", 4,
            "Permanent Upgrade · Raises Apex Pack spawn rate (10% -> 18%) and auto-opens F4 Scanner on bosses."),
        new("tactical-reserve", "Castle: Tactical Reserve", 6,
            "Permanent Upgrade · Start every battle with 35% Party Momentum pre-charged."),
        new("war-college", "Castle: War College", 7,
            "Permanent Upgrade · Commanders earn +25% bonus War XP on every Drill, Skirmish, and Story War victory."),
        new("imperial-treasury", "Castle: Imperial Treasury", 7,
            "Permanent Upgrade · Permanently increases all RPG battle Potch by +15% (stacks with S-Rank, Streak, and Apex)."),
        new("arcane-academy", "Castle: Arcane Academy", 8,
            "Permanent Upgrade · Recovers +1 extra Rune Charge per surviving ally on every S-Rank victory."),
        new("grand-armory", "Castle: Grand Armory", 8,
            "Permanent Upgrade · Permanently grants +5% bonus physical and magic damage to all party members.")
    };

    internal sealed record HonorSpec(string Id, string Title, string Requirement);

    internal static readonly HonorSpec[] Honors =
    {
        new("first-blood", "First Blood", "Win 1 Army Training Drill or Skirmish."),
        new("seasoned-tactician", "Seasoned Tactician", "Win 5 Army Drills or Skirmishes."),
        new("apex-slayer", "Apex Slayer", "Defeat 3 Apex Pack encounters."),
        new("master-naturalist", "Master Naturalist", "Catalogue 15 unique Bestiary species."),
        new("castle-architect", "Castle Architect", "Unlock 4 Permanent Castle Upgrades."),
        new("bounty-champion", "Bounty Champion", "Complete 5 rotating Field Bounties."),
        new("unstoppable-force", "Unstoppable Force", "Reach a 10-battle Victory Streak."),
        new("grand-hunter", "Grand Hunter", "Reach Gold Hunter Rank (8+ Hunts)."),
        new("echo-conqueror", "Echo Conqueror", "Clear 3 Hall of Echoes Nightmare battles."),
        new("marshal-of-dunan", "Marshal of Dunan", "Promote any Commander to Marshal (900+ XP)."),
        new("combo-master", "Combo Master", "Land 25 total Combat Combos."),
        new("overdrive-legend", "Overdrive Legend", "Trigger 10 Dunan Overdrives."),
        new("s-rank-virtuoso", "S-Rank Virtuoso", "Achieve 15 S-Rank Battle Victories."),
        new("grand-architect", "Grand Architect", "Unlock all 8 Permanent Castle Upgrades."),
        new("bestiary-scholar", "Bestiary Scholar", "Catalogue 30 unique Bestiary species.")
    };

    internal static List<HonorSpec> EvaluateNewHonors(ArmyLedgerState ledger, int currentWinStreak)
    {
        var newlyUnlocked = new List<HonorSpec>();
        ledger.UnlockedHonors ??= new HashSet<string>(StringComparer.Ordinal);
        bool IsMet(string id) => id switch
        {
            "first-blood" => ledger.TotalDrillsWon >= 1,
            "seasoned-tactician" => ledger.TotalDrillsWon >= 5,
            "apex-slayer" => ledger.ApexPacksCleared >= 3,
            "master-naturalist" => (ledger.Bestiary?.Count ?? 0) >= 15,
            "castle-architect" => (ledger.CastleUpgrades?.Count ?? 0) >= 4,
            "bounty-champion" => ledger.BountiesCompleted >= 5,
            "unstoppable-force" => currentWinStreak >= 10,
            "grand-hunter" => ledger.HuntsCompleted >= 8,
            "echo-conqueror" => ledger.HallNightmareClears >= 3,
            "marshal-of-dunan" => ledger.Commanders.Values.Any(c => ArmyProgressionRules.RankForXp(c.Xp) == ArmyRank.Marshal),
            "combo-master" => ledger.TotalCombosLanded >= 25,
            "overdrive-legend" => ledger.TotalOverdrivesTriggered >= 10,
            "s-rank-virtuoso" => ledger.TotalSRanks >= 15,
            "grand-architect" => (ledger.CastleUpgrades?.Count ?? 0) >= CastleUpgrades.Length,
            "bestiary-scholar" => (ledger.Bestiary?.Count ?? 0) >= 30,
            _ => false
        };
        foreach (var h in Honors)
        {
            if (!ledger.UnlockedHonors.Contains(h.Id) && IsMet(h.Id))
            {
                ledger.UnlockedHonors.Add(h.Id);
                ledger.Commendations = Math.Min(999, ledger.Commendations + 2);
                newlyUnlocked.Add(h);
            }
        }
        return newlyUnlocked;
    }

    internal const int MaxArmorBreakStacks = 3;

    internal static int ApplyArmorBreak(int damage, int stacks)
    {
        if (damage <= 0 || stacks <= 0) return damage;
        int clamped = Math.Clamp(stacks, 0, MaxArmorBreakStacks);
        return Math.Max(damage + 1, (int)Math.Round(damage * (1.0 + clamped * 0.05), MidpointRounding.AwayFromZero));
    }

    internal static int ApplyPhalanxDefense(int damage, int livingFrontRowAllies) =>
        damage <= 1 || livingFrontRowAllies < 3 ? damage : Math.Max(1, (int)Math.Round(damage * 0.94, MidpointRounding.AwayFromZero));

    internal static int ApplyCrossfireAttack(int damage, int livingBackRowAllies) =>
        damage <= 0 || livingBackRowAllies < 3 ? damage : Math.Max(damage + 1, (int)Math.Round(damage * 1.06, MidpointRounding.AwayFromZero));

    internal static int ApplyGrandArmory(int damage, bool hasGrandArmory) =>
        damage <= 0 || !hasGrandArmory ? damage : Math.Max(damage + 1, (int)Math.Round(damage * 1.05, MidpointRounding.AwayFromZero));

    internal static readonly FieldBountySpec[] FieldBounties =
    {
        new(FieldBountyKind.Combos, "Combo Tactician", 5, "Land 5 Combat Combos (Shatter, Venom, Arcane, Execute)."),
        new(FieldBountyKind.SRanks, "Flawless Commander", 3, "Achieve 3 S-Rank Battle Victories."),
        new(FieldBountyKind.ApexPacks, "Apex Hunter", 1, "Defeat 1 Apex Pack encounter."),
        new(FieldBountyKind.BestiaryEntries, "Field Naturalist", 3, "Catalogue 3 new enemy species in the Bestiary."),
        new(FieldBountyKind.OverdrivesOrSurges, "Unstoppable Spirit", 2, "Trigger 2 Dunan Overdrives or Desperation Surges.")
    };

    internal static FieldBountySpec ActiveBounty(int index) =>
        FieldBounties[Math.Abs(index) % FieldBounties.Length];

    internal static WeaponPerkBonus WeaponPerk(int weaponLevel, bool masterBlacksmiths) =>
        WeaponPerk(masterBlacksmiths ? weaponLevel + 2 : weaponLevel);

    internal static int ApplyWeaponAttackPerk(int damage, int weaponLevel, bool masterBlacksmiths) =>
        ApplyWeaponAttackPerk(damage, masterBlacksmiths ? weaponLevel + 2 : weaponLevel);

    internal static int ApplyWeaponDefensePerk(int damage, int weaponLevel, bool masterBlacksmiths) =>
        ApplyWeaponDefensePerk(damage, masterBlacksmiths ? weaponLevel + 2 : weaponLevel);

    internal static CombatComboResult EvaluateExecuteFinisher(int baseDamage, int targetHp, int targetMaxHp)
    {
        if (baseDamage <= 0 || targetMaxHp <= 0 || targetHp <= 0 || targetHp * 5 > targetMaxHp)
            return default;
        return new("Execute Finisher", Math.Clamp((int)Math.Round(baseDamage * 0.15, MidpointRounding.AwayFromZero), 5, 75));
    }

    internal static bool ShouldArmRiposte(int incomingDamage, int hpAfterHit, int maxHp) =>
        maxHp > 0 && hpAfterHit > 0 && incomingDamage * 5 >= maxHp;

    internal static int ApplyRiposte(int damage) =>
        damage <= 0 ? damage : Math.Max(damage + 1, (int)Math.Round(damage * 1.20, MidpointRounding.AwayFromZero));

    internal const int OverdriveThreshold = 100;

    internal static int AddMomentum(int current, int delta, out bool triggeredOverdrive)
    {
        triggeredOverdrive = false;
        if (delta <= 0) return Math.Clamp(current, 0, OverdriveThreshold);
        int next = Math.Clamp(current + delta, 0, OverdriveThreshold);
        if (next >= OverdriveThreshold)
        {
            triggeredOverdrive = true;
            return OverdriveThreshold;
        }
        return next;
    }

    internal static int ApplyOverdriveAttack(int damage) =>
        damage <= 0 ? damage : Math.Max(damage + 1, (int)Math.Round(damage * 1.20, MidpointRounding.AwayFromZero));

    internal static int ApplyOverdriveDefense(int damage) =>
        damage <= 1 ? damage : Math.Max(1, (int)Math.Round(damage * 0.85, MidpointRounding.AwayFromZero));

    internal static ApexAffix RollApexAffix(int roll0To3) => (Math.Abs(roll0To3) % 4) switch
    {
        0 => ApexAffix.Frenzied,
        1 => ApexAffix.Armored,
        2 => ApexAffix.WardBlessed,
        _ => ApexAffix.Golden
    };

    internal static string ApexAffixTitle(ApexAffix affix) => affix switch
    {
        ApexAffix.Frenzied => "Frenzied (+10% DMG)",
        ApexAffix.Armored => "Armored (-10% Phys Taken)",
        ApexAffix.WardBlessed => "Ward-Blessed (-10% Magic Taken)",
        ApexAffix.Golden => "Golden (2.5x Potch)",
        _ => ""
    };

    internal static string ApexAffixShort(ApexAffix affix) => affix switch
    {
        ApexAffix.Frenzied => "FRENZIED",
        ApexAffix.Armored => "ARMORED",
        ApexAffix.WardBlessed => "WARDED",
        ApexAffix.Golden => "GOLDEN",
        _ => ""
    };

    internal static double ApexAffixDamageMultiplier(ApexAffix affix) =>
        affix == ApexAffix.Frenzied ? 1.10 : 1.00;

    internal static double ApexAffixPotchMultiplier(ApexAffix affix) =>
        affix == ApexAffix.Golden ? 2.50 : ApexPotchMultiplier;

    internal static int ApplyApexIncomingReduction(int damage, ApexAffix affix, bool isMagic)
    {
        if (damage <= 1) return damage;
        if ((!isMagic && affix == ApexAffix.Armored) || (isMagic && affix == ApexAffix.WardBlessed))
            return Math.Max(1, (int)Math.Round(damage * 0.90, MidpointRounding.AwayFromZero));
        return damage;
    }

    internal static string BattleGrade(int koCount, int turns, int combos, int surges)
    {
        if (koCount == 0 && (turns <= 3 || combos + surges >= 2)) return "S";
        if (koCount == 0) return "A";
        if (koCount == 1) return "B";
        return "C";
    }

    internal static double StreakPotchMultiplier(int streak) => streak switch
    {
        >= 10 => 1.20,
        >= 5 => 1.10,
        _ => 1.00
    };

    internal static int StreakStartingMomentum(int streak) => streak >= 10 ? 10 : 0;

    internal static string MutatorTitle(WarMutator mutator) => mutator switch
    {
        WarMutator.BloodMoon => "Blood Moon",
        WarMutator.IronAttrition => "Iron Attrition",
        _ => "Standard Battlefield"
    };

    internal static string MutatorDescription(WarMutator mutator) => mutator switch
    {
        WarMutator.BloodMoon => "All war units gain +2 ATK and -1 DEF. Awards +1 bonus Commendation on victory.",
        WarMutator.IronAttrition => "All war units gain +2 DEF for prolonged sieges. Awards +1 bonus Commendation on victory.",
        _ => "Standard war rules with no battlefield mutator."
    };

    internal static CommanderProfile ApplyMutator(CommanderProfile source, WarMutator mutator)
    {
        if (mutator == WarMutator.Standard) return source;
        int atk = source.Attack, def = source.Defense;
        if (mutator == WarMutator.BloodMoon) { atk += 2; def -= 1; }
        else if (mutator == WarMutator.IronAttrition) { def += 2; }
        return new CommanderProfile
        {
            NameId = source.NameId,
            Name = source.Name,
            Branch = source.Branch,
            Attack = (byte)Math.Clamp(atk, 0, 20),
            Defense = (byte)Math.Clamp(def, 0, 20),
            AssistAttack = source.AssistAttack,
            AssistDefense = source.AssistDefense,
            Abilities = source.Abilities.ToArray(),
            Uses = source.Uses.ToArray()
        };
    }

    internal static int BestiaryMilestonesFor(int uniqueSpecies) => Math.Max(0, uniqueSpecies) / 10;
}
