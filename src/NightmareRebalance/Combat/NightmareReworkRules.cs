namespace NightmareRebalance;

internal enum PackTrait { None = 0, Thorned = 1, Vampiric = 2, Shielded = 3, Berserk = 4 }

internal enum ZoneHazard { Calm = 0, ArcaneStorm = 1, BloodMist = 2, HighlandAlert = 3 }

internal static class NightmareReworkRules
{
    internal const double ShieldedDamageTakenMultiplier = 0.65;
    internal const double StaggeredDamageTakenMultiplier = 1.50;
    internal const double StaggeredDamageDealtMultiplier = 0.60;
    internal const double ChargedAttackMultiplier = 1.80;
    internal const double DefendVsChargedMultiplier = 0.35;
    internal const double ThornedReflectFraction = 0.12;
    internal const double VampiricLeechFraction = 0.35;

    internal static int InitialShields(bool isBoss, bool isEliteOrApex, PackTrait trait)
    {
        if (isBoss) return 5;
        if (isEliteOrApex) return 3;
        if (trait == PackTrait.Shielded) return 2;
        return 0;
    }

    internal static int ShieldBreakAmount(bool hitWeakness, bool comboProcced) =>
        hitWeakness || comboProcced ? 2 : 1;

    internal static PackTrait RollPackTrait(int roll0To99) => roll0To99 switch
    {
        >= 0 and < 13 => PackTrait.Thorned,
        >= 13 and < 26 => PackTrait.Vampiric,
        >= 26 and < 39 => PackTrait.Shielded,
        >= 39 and < 52 => PackTrait.Berserk,
        _ => PackTrait.None
    };

    internal static string PackTraitTitle(PackTrait trait) => trait switch
    {
        PackTrait.Thorned => "THORNED (12% Phys Reflect)",
        PackTrait.Vampiric => "VAMPIRIC (35% HP Leech)",
        PackTrait.Shielded => "SHIELDED (Guard Pips)",
        PackTrait.Berserk => "BERSERK (+35% DMG / +15% Taken)",
        _ => ""
    };

    internal static string PackTraitShort(PackTrait trait) => trait switch
    {
        PackTrait.Thorned => "THORNED",
        PackTrait.Vampiric => "VAMPIRIC",
        PackTrait.Shielded => "SHIELDED",
        PackTrait.Berserk => "BERSERK",
        _ => ""
    };

    internal static ZoneHazard RollZoneHazard(string? tableId)
    {
        if (string.IsNullOrWhiteSpace(tableId)) return ZoneHazard.Calm;
        int hash = 0;
        foreach (char c in tableId) hash = hash * 31 + c;
        return (Math.Abs(hash) % 4) switch
        {
            0 => ZoneHazard.ArcaneStorm,
            1 => ZoneHazard.BloodMist,
            2 => ZoneHazard.HighlandAlert,
            _ => ZoneHazard.Calm
        };
    }

    internal static string ZoneHazardTitle(ZoneHazard hazard) => hazard switch
    {
        ZoneHazard.ArcaneStorm => "Arcane Storm (+30% All Magic DMG)",
        ZoneHazard.BloodMist => "Blood Mist (+15% Enemy DMG · Fast Fatigue)",
        ZoneHazard.HighlandAlert => "Highland Alert (+15% Enemy Stats · +25% Potch)",
        _ => ""
    };

    // Anti-overleveling scaling: if party avg level exceeds enemy level by 3+, scale enemy up.
    internal static double AntiOverlevelHpMultiplier(int partyAvgLevel, int enemyLevel)
    {
        if (partyAvgLevel <= 0 || enemyLevel <= 0) return 1.0;
        int excess = partyAvgLevel - enemyLevel - 2;
        if (excess <= 0) return 1.0;
        return Math.Min(2.20, 1.0 + excess * 0.06);
    }

    internal static double AntiOverlevelDamageMultiplier(int partyAvgLevel, int enemyLevel)
    {
        if (partyAvgLevel <= 0 || enemyLevel <= 0) return 1.0;
        int excess = partyAvgLevel - enemyLevel - 2;
        if (excess <= 0) return 1.0;
        return Math.Min(1.65, 1.0 + excess * 0.04);
    }

    internal static double BattleFatigueMultiplier(int turn, ZoneHazard hazard)
    {
        int startTurn = hazard == ZoneHazard.BloodMist ? 4 : 6;
        if (turn < startTurn) return 1.0;
        return Math.Min(1.60, 1.0 + (turn - startTurn + 1) * 0.06);
    }

    internal static int ApplyShieldAndStaggerDamage(int damage, int shields, bool staggered)
    {
        if (damage <= 0) return damage;
        if (staggered) return Math.Max(damage + 1, (int)Math.Round(damage * StaggeredDamageTakenMultiplier, MidpointRounding.AwayFromZero));
        if (shields > 0) return Math.Max(1, (int)Math.Round(damage * ShieldedDamageTakenMultiplier, MidpointRounding.AwayFromZero));
        return damage;
    }

    internal static int ComputeThornedReflect(int physicalDamageDealt, int attackerCurrentHp)
    {
        if (physicalDamageDealt <= 0 || attackerCurrentHp <= 1) return 0;
        int reflect = Math.Max(1, (int)Math.Round(physicalDamageDealt * ThornedReflectFraction, MidpointRounding.AwayFromZero));
        return Math.Min(attackerCurrentHp - 1, reflect);
    }

    internal static int ComputeVampiricHeal(int damageDealt) =>
        damageDealt <= 0 ? 0 : Math.Max(1, (int)Math.Round(damageDealt * VampiricLeechFraction, MidpointRounding.AwayFromZero));

    internal static int ComputeDefendHeal(int maxHp) =>
        maxHp <= 0 ? 0 : Math.Max(1, (int)Math.Round(maxHp * 0.10, MidpointRounding.AwayFromZero));

    internal static bool ShouldChargeThisTurn(int turn, bool isBoss, bool isElite, bool staggered)
    {
        if (staggered || (!isBoss && !isElite)) return false;
        int cadence = isBoss ? 3 : 4;
        return turn >= 2 && (turn - 2) % cadence == 0;
    }
}

// Turn-scoped charge state machine: prevents an AoE from charging on target 1 and
// unleashing on target 2, and keeps the unleashing state readable for Defend across all targets.
internal sealed class EnemyChargeTracker
{
    internal int ChargedOnTurn { get; private set; } = -1;
    internal int UnleashingOnTurn { get; private set; } = -1;

    internal bool IsChargingForUi(int currentTurn) =>
        (ChargedOnTurn >= 0 && currentTurn >= ChargedOnTurn && UnleashingOnTurn < 0) || UnleashingOnTurn == currentTurn;

    internal bool IsUnleashing(int currentTurn) => UnleashingOnTurn == currentTurn;

    internal void Interrupt()
    {
        ChargedOnTurn = -1;
        UnleashingOnTurn = -1;
    }

    internal double EvaluateDamageMultiplier(int currentTurn, bool isBoss, bool isElite, bool staggered, out bool beganTelegraph, out bool beganUnleash)
    {
        beganTelegraph = false;
        beganUnleash = false;
        if (staggered)
        {
            Interrupt();
            return NightmareReworkRules.StaggeredDamageDealtMultiplier;
        }
        if (UnleashingOnTurn >= 0 && currentTurn > UnleashingOnTurn)
            UnleashingOnTurn = -1;
        if (UnleashingOnTurn == currentTurn)
            return NightmareReworkRules.ChargedAttackMultiplier;
        if (ChargedOnTurn >= 0 && currentTurn > ChargedOnTurn)
        {
            ChargedOnTurn = -1;
            UnleashingOnTurn = currentTurn;
            beganUnleash = true;
            return NightmareReworkRules.ChargedAttackMultiplier;
        }
        if (ChargedOnTurn == currentTurn)
            return 1.0;
        if (NightmareReworkRules.ShouldChargeThisTurn(currentTurn, isBoss, isElite, false))
        {
            ChargedOnTurn = currentTurn;
            beganTelegraph = true;
        }
        return 1.0;
    }
}

// Pure shield/reaction commit state machine separating pure damage queries from committed native hits.
internal sealed class ShieldReactionState
{
    internal int MaxShields;
    internal int Shields;
    internal int StaggeredUntilTurn = -1;
    internal readonly EnemyChargeTracker Charge = new();
    internal bool IsBoss;
    internal bool IsElite;

    internal bool IsStaggered(int currentTurn)
    {
        if (StaggeredUntilTurn < 0) return false;
        if (currentTurn > StaggeredUntilTurn)
        {
            StaggeredUntilTurn = -1;
            if (MaxShields > 0) Shields = MaxShields;
            return false;
        }
        return true;
    }

    // Pure damage calculation: never mutates shields, stagger, or charge state.
    internal int CalculatePlayerDamage(int baseDamage, int currentTurn)
    {
        if (baseDamage <= 0) return baseDamage;
        bool staggered = IsStaggered(currentTurn);
        return NightmareReworkRules.ApplyShieldAndStaggerDamage(baseDamage, Shields, staggered);
    }

    // Committed native reaction hit: mutates shields/stagger/interrupt once per actual committed hit.
    internal bool CommitPlayerHit(int currentTurn, bool hitWeak, bool comboProcced, bool hasDragonTrophy, out bool interruptedCharge)
    {
        interruptedCharge = false;
        if (IsStaggered(currentTurn) || Shields <= 0) return false;
        int breakAmt = NightmareReworkRules.ShieldBreakAmount(hitWeak, comboProcced);
        if (comboProcced && hasDragonTrophy) breakAmt++;
        Shields = Math.Max(0, Shields - breakAmt);
        if (Shields == 0)
        {
            StaggeredUntilTurn = currentTurn + 1;
            interruptedCharge = Charge.IsChargingForUi(currentTurn);
            Charge.Interrupt();
            return true;
        }
        return false;
    }
}
