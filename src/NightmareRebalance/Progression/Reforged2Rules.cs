namespace NightmareRebalance;

internal enum HunterRank { Unranked = 0, Bronze = 1, Silver = 2, Gold = 3, Master = 4 }

internal enum ArmyStance { Balanced = 0, AllOutAssault = 1, IronPhalanx = 2, SwiftMarch = 3 }

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
            BaseWarNumber: 13, EnemyAtkBonus: 3, EnemyDefBonus: 3, GrantEnemyCritical: true, BasePotch: 4000, Commendations: 5)
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
        return new("Arcane Chain", Math.Clamp((int)Math.Round(baseDamage * 0.18, MidpointRounding.AwayFromZero), 5, 90));
    }

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
}
