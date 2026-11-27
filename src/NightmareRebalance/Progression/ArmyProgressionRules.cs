using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NightmareRebalance;

internal enum ArmyRank { Recruit = 0, Lieutenant = 1, Captain = 2, General = 3, Marshal = 4 }

internal enum ArmyDoctrine { None = 0, Vanguard = 1, Bulwark = 2, Tactician = 3 }

internal enum DrillDirective { None = 0, IronWall = 1, NoMedicine = 2, ForcedMarch = 3 }

internal enum HuntRiskModifier { None = 0, FrenziedQuarry = 1, IroncladQuarry = 2 }

internal sealed class HallBossRecord
{
    public bool OriginalCleared { get; set; }
    public bool NightmareCleared { get; set; }
    public int BestOriginalTurns { get; set; }
    public int BestNightmareTurns { get; set; }
    public bool NoKoOriginal { get; set; }
    public bool NoKoNightmare { get; set; }

    internal HallBossRecord Clone() => new()
    {
        OriginalCleared = OriginalCleared,
        NightmareCleared = NightmareCleared,
        BestOriginalTurns = BestOriginalTurns,
        BestNightmareTurns = BestNightmareTurns,
        NoKoOriginal = NoKoOriginal,
        NoKoNightmare = NoKoNightmare
    };
}

internal sealed record WarSynergyPair(int FirstNameId, int SecondNameId, string Title, int AtkBonus, int DefBonus, int AssistAtkBonus, int AssistDefBonus, bool BoostActiveUse);

internal sealed class CommanderProgress
{
    public int NameId { get; set; }
    public string Name { get; set; } = "";
    public int Xp { get; set; }
    public ArmyDoctrine Doctrine { get; set; } = ArmyDoctrine.None;
    public int DrillsWon { get; set; }
    public int StoryWarsWon { get; set; }
    public int NightmareClears { get; set; }

    internal CommanderProgress Clone() => new()
    {
        NameId = NameId,
        Name = Name,
        Xp = Xp,
        Doctrine = Doctrine,
        DrillsWon = DrillsWon,
        StoryWarsWon = StoryWarsWon,
        NightmareClears = NightmareClears
    };
}

internal sealed class ArmyLedgerState
{
    public int SchemaVersion { get; set; } = 1;
    public Dictionary<int, CommanderProgress> Commanders { get; set; } = new();
    public Dictionary<string, HallBossRecord> HallRecords { get; set; } = new(StringComparer.Ordinal);
    public int TotalDrillsWon { get; set; }
    public int TotalVeteranDrillsWon { get; set; }
    public int TotalStoryWarsWon { get; set; }
    public int HallNightmareClears { get; set; }
    public int MoraleBonusCharges { get; set; }
    public long UpdatedUtcTicks { get; set; }

    internal ArmyLedgerState Clone()
    {
        var copy = new ArmyLedgerState
        {
            SchemaVersion = SchemaVersion,
            TotalDrillsWon = TotalDrillsWon,
            TotalVeteranDrillsWon = TotalVeteranDrillsWon,
            TotalStoryWarsWon = TotalStoryWarsWon,
            HallNightmareClears = HallNightmareClears,
            MoraleBonusCharges = MoraleBonusCharges,
            UpdatedUtcTicks = UpdatedUtcTicks
        };
        foreach (var kv in Commanders)
            copy.Commanders[kv.Key] = kv.Value.Clone();
        if (HallRecords != null)
            foreach (var kv in HallRecords)
                copy.HallRecords[kv.Key] = kv.Value.Clone();
        return copy;
    }
}

internal sealed class ArmyLedgerIndex
{
    public int SchemaVersion { get; set; } = 1;
    public Dictionary<string, ArmyLedgerState> Snapshots { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal readonly record struct CommanderParticipation(int NameId, string Name, bool Leader, bool Survived);

internal readonly record struct XpAwardSummary(int CommandersUpdated, int RanksGained, int LeaderXp, bool DirectiveFulfilled);

internal static class ArmyProgressionRules
{
    internal const int MaxLedgerEntries = 256;
    internal const int ForcedMarchMaxTurns = 6;
    internal static readonly int[] RankThresholds = { 0, 100, 250, 500, 900 };

    internal static ArmyRank RankForXp(int xp)
    {
        if (xp >= RankThresholds[4]) return ArmyRank.Marshal;
        if (xp >= RankThresholds[3]) return ArmyRank.General;
        if (xp >= RankThresholds[2]) return ArmyRank.Captain;
        if (xp >= RankThresholds[1]) return ArmyRank.Lieutenant;
        return ArmyRank.Recruit;
    }

    internal static int NextRankXp(ArmyRank rank) => rank switch
    {
        ArmyRank.Recruit => RankThresholds[1],
        ArmyRank.Lieutenant => RankThresholds[2],
        ArmyRank.Captain => RankThresholds[3],
        ArmyRank.General => RankThresholds[4],
        _ => RankThresholds[4]
    };

    internal static string RankTitle(ArmyRank rank) => rank switch
    {
        ArmyRank.Lieutenant => "Lieutenant",
        ArmyRank.Captain => "Captain",
        ArmyRank.General => "General",
        ArmyRank.Marshal => "Marshal",
        _ => "Recruit"
    };

    internal static string DoctrineTitle(ArmyDoctrine doctrine) => doctrine switch
    {
        ArmyDoctrine.Vanguard => "Vanguard",
        ArmyDoctrine.Bulwark => "Bulwark",
        ArmyDoctrine.Tactician => "Tactician",
        _ => "Unassigned"
    };

    internal static string DoctrineDescription(ArmyDoctrine doctrine) => doctrine switch
    {
        ArmyDoctrine.Vanguard => "Captain: SP_MOUNT/SP_CRITICAL. General: +1 Assist ATK & offense ability.",
        ArmyDoctrine.Bulwark => "Captain: SP_HP_PLUS. General: +1 Assist DEF & SP_BODY_GUARD.",
        ArmyDoctrine.Tactician => "Captain: SP_SCOUT or +1 active use. General: +1 Assist DEF & SP_SEE_THROUGH.",
        _ => "Balanced rank stats only. Choose a Doctrine at Captain or higher."
    };

    internal static string DirectiveTitle(DrillDirective directive) => directive switch
    {
        DrillDirective.IronWall => "Iron Wall",
        DrillDirective.NoMedicine => "No Field Medicine",
        DrillDirective.ForcedMarch => "Forced March",
        _ => "Standard Rules"
    };

    internal static string DirectiveDescription(DrillDirective directive) => directive switch
    {
        DrillDirective.IronWall => "Enemy war units gain +2 DEF. Awards 1.35x Commander XP and 1.25x Potch.",
        DrillDirective.NoMedicine => "Allied SP_MEDICAL abilities disabled. Awards 1.40x Commander XP and 1.25x Potch.",
        DrillDirective.ForcedMarch => $"Rout the enemy commander within {ForcedMarchMaxTurns} turns. Awards 1.50x XP and 1.35x Potch.",
        _ => "No extra tactical restriction. Standard Commander XP and Potch."
    };

    internal static double DirectiveXpMultiplier(DrillDirective directive) => directive switch
    {
        DrillDirective.IronWall => 1.35,
        DrillDirective.NoMedicine => 1.40,
        DrillDirective.ForcedMarch => 1.50,
        _ => 1.0
    };

    internal static double DirectivePotchMultiplier(DrillDirective directive) => directive switch
    {
        DrillDirective.IronWall => 1.25,
        DrillDirective.NoMedicine => 1.25,
        DrillDirective.ForcedMarch => 1.35,
        _ => 1.0
    };

    internal static bool IsDirectiveFulfilled(DrillDirective directive, long turnCount) => directive switch
    {
        DrillDirective.ForcedMarch => turnCount > 0 && turnCount <= ForcedMarchMaxTurns,
        _ => true
    };

    internal static int AdjustedPotch(int basePotch, DrillDirective directive, long turnCount)
    {
        if (basePotch <= 0 || directive == DrillDirective.None || !IsDirectiveFulfilled(directive, turnCount))
            return basePotch;
        return (int)Math.Round(basePotch * DirectivePotchMultiplier(directive), MidpointRounding.AwayFromZero);
    }

    internal static string ComputeFingerprint(IReadOnlyList<string> serializedRecords, IReadOnlyList<byte[]> flagArrays)
    {
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        byte[] sep = { (byte)'|' };
        for (int i = 0; i < serializedRecords.Count; i++)
        {
            if (i > 0) sha.AppendData(sep);
            sha.AppendData(Encoding.UTF8.GetBytes(serializedRecords[i] ?? ""));
        }
        for (int i = 0; i < flagArrays.Count; i++)
        {
            sha.AppendData(sep);
            if (flagArrays[i] != null) sha.AppendData(flagArrays[i]);
        }
        return Convert.ToHexString(sha.GetHashAndReset());
    }

    internal static void BindSnapshot(ArmyLedgerIndex index, string saveHash, ArmyLedgerState state, long utcTicks)
    {
        if (string.IsNullOrWhiteSpace(saveHash)) return;
        var clone = state.Clone();
        clone.UpdatedUtcTicks = utcTicks;
        index.Snapshots[saveHash] = clone;
        if (index.Snapshots.Count > MaxLedgerEntries)
        {
            var oldest = index.Snapshots
                .OrderBy(kv => kv.Value.UpdatedUtcTicks)
                .Take(index.Snapshots.Count - MaxLedgerEntries)
                .Select(kv => kv.Key)
                .ToArray();
            foreach (var key in oldest) index.Snapshots.Remove(key);
        }
    }

    internal static ArmyLedgerState ResolveSnapshot(ArmyLedgerIndex index, string? saveHash)
    {
        if (!string.IsNullOrWhiteSpace(saveHash) && index.Snapshots.TryGetValue(saveHash, out var found) && found != null)
            return found.Clone();
        return new ArmyLedgerState();
    }

    internal static ArmyLedgerIndex ParseIndex(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Length > 2 * 1024 * 1024)
            return new ArmyLedgerIndex();
        var parsed = JsonSerializer.Deserialize<ArmyLedgerIndex>(json);
        if (parsed == null || parsed.SchemaVersion != 1 || parsed.Snapshots == null)
            return new ArmyLedgerIndex();
        return parsed;
    }

    internal static string SerializeIndex(ArmyLedgerIndex index) =>
        JsonSerializer.Serialize(index, new JsonSerializerOptions { WriteIndented = false });

    internal static XpAwardSummary AwardTrainingXp(
        ArmyLedgerState state,
        IReadOnlyList<CommanderParticipation> participants,
        bool veteran,
        DrillDirective directive,
        long turnCount,
        bool enableCrossTheaterSynergy)
    {
        bool fulfilled = IsDirectiveFulfilled(directive, turnCount);
        double mult = fulfilled ? DirectiveXpMultiplier(directive) : 1.0;
        int baseXp = veteran ? 90 : 50;
        int updated = 0, ranksGained = 0, leaderSample = 0;
        var seen = new HashSet<int>();

        foreach (var p in participants)
        {
            if (p.NameId <= 0 || !seen.Add(p.NameId)) continue;
            int raw = baseXp + (p.Survived ? 20 : 0);
            int awarded = (int)Math.Round(raw * mult * (p.Leader ? 1.0 : 0.75), MidpointRounding.AwayFromZero);
            if (p.Leader && leaderSample == 0) leaderSample = awarded;

            if (!state.Commanders.TryGetValue(p.NameId, out var entry))
            {
                entry = new CommanderProgress { NameId = p.NameId, Name = p.Name };
                state.Commanders[p.NameId] = entry;
            }
            else if (string.IsNullOrEmpty(entry.Name) && !string.IsNullOrEmpty(p.Name))
            {
                entry.Name = p.Name;
            }

            var before = RankForXp(entry.Xp);
            entry.Xp = Math.Min(99999, entry.Xp + Math.Max(0, awarded));
            entry.DrillsWon++;
            if (RankForXp(entry.Xp) > before) ranksGained++;
            updated++;
        }

        state.TotalDrillsWon++;
        if (veteran)
        {
            state.TotalVeteranDrillsWon++;
            if (enableCrossTheaterSynergy)
                state.MoraleBonusCharges = Math.Min(3, state.MoraleBonusCharges + 1);
        }

        return new XpAwardSummary(updated, ranksGained, leaderSample, fulfilled);
    }

    internal static XpAwardSummary AwardStoryWarXp(ArmyLedgerState state, IReadOnlyList<CommanderParticipation> participants)
    {
        int updated = 0, ranksGained = 0;
        var seen = new HashSet<int>();
        foreach (var p in participants)
        {
            if (p.NameId <= 0 || !seen.Add(p.NameId)) continue;
            int raw = 60 + (p.Survived ? 20 : 0);
            int awarded = (int)Math.Round(raw * (p.Leader ? 1.0 : 0.75), MidpointRounding.AwayFromZero);
            if (!state.Commanders.TryGetValue(p.NameId, out var entry))
            {
                entry = new CommanderProgress { NameId = p.NameId, Name = p.Name };
                state.Commanders[p.NameId] = entry;
            }
            var before = RankForXp(entry.Xp);
            entry.Xp = Math.Min(99999, entry.Xp + awarded);
            entry.StoryWarsWon++;
            if (RankForXp(entry.Xp) > before) ranksGained++;
            updated++;
        }
        state.TotalStoryWarsWon++;
        return new XpAwardSummary(updated, ranksGained, 80, true);
    }

    internal static XpAwardSummary AwardHallNightmareXp(ArmyLedgerState state, IReadOnlyList<(int NameId, string Name)> partyCommanders)
    {
        int updated = 0, ranksGained = 0;
        var seen = new HashSet<int>();
        foreach (var (nameId, name) in partyCommanders)
        {
            if (nameId <= 0 || !seen.Add(nameId)) continue;
            if (!state.Commanders.TryGetValue(nameId, out var entry))
            {
                entry = new CommanderProgress { NameId = nameId, Name = name };
                state.Commanders[nameId] = entry;
            }
            var before = RankForXp(entry.Xp);
            entry.Xp = Math.Min(99999, entry.Xp + 35);
            entry.NightmareClears++;
            if (RankForXp(entry.Xp) > before) ranksGained++;
            updated++;
        }
        state.HallNightmareClears++;
        return new XpAwardSummary(updated, ranksGained, 35, true);
    }

    internal static CommanderProfile ApplyProgression(CommanderProfile source, CommanderProgress? progress)
    {
        if (progress == null) return source;
        var rank = RankForXp(progress.Xp);
        if (rank == ArmyRank.Recruit) return source;

        int atk = source.Attack;
        int def = source.Defense;
        int assistAtk = source.AssistAttack;
        int assistDef = source.AssistDefense;
        var abilities = new List<string>(source.Abilities);
        var uses = new List<byte>(source.Uses);

        // Rank stat bonuses (strictly bounded within OverhaulRules 0..20 / 0..5).
        if (rank >= ArmyRank.Lieutenant) atk++;
        if (rank >= ArmyRank.Captain) def++;
        if (rank >= ArmyRank.General) atk++;
        if (rank >= ArmyRank.Marshal) { atk++; def++; }

        static bool TryAddAbility(List<string> list, List<byte> useList, string ability, byte useCount)
        {
            int id = Array.IndexOf(OverhaulRules.Abilities, ability);
            if (!OverhaulRules.SafeAbility(id) || list.Contains(ability) || list.Count >= 3) return false;
            byte boundedUses = OverhaulRules.ActiveAbility(id) ? (byte)Math.Clamp((int)useCount, 1, 3) : (byte)0;
            list.Add(ability);
            useList.Add(boundedUses);
            return true;
        }

        static bool TryBoostActiveUse(List<string> list, List<byte> useList)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int id = Array.IndexOf(OverhaulRules.Abilities, list[i]);
                if (OverhaulRules.ActiveAbility(id) && useList[i] < 3)
                {
                    useList[i]++;
                    return true;
                }
            }
            return false;
        }

        if (rank >= ArmyRank.Captain)
        {
            switch (progress.Doctrine)
            {
                case ArmyDoctrine.Vanguard:
                    if (!TryAddAbility(abilities, uses, "SP_MOUNT", 0))
                        TryAddAbility(abilities, uses, "SP_CRITICAL", 0);
                    break;
                case ArmyDoctrine.Bulwark:
                    if (!TryAddAbility(abilities, uses, "SP_HP_PLUS", 0))
                        def++;
                    break;
                case ArmyDoctrine.Tactician:
                    if (!TryBoostActiveUse(abilities, uses))
                        TryAddAbility(abilities, uses, "SP_SCOUT", 1);
                    break;
            }
        }

        if (rank >= ArmyRank.General)
        {
            switch (progress.Doctrine)
            {
                case ArmyDoctrine.Vanguard:
                    assistAtk++;
                    if (!TryAddAbility(abilities, uses, "SP_CRITICAL", 0))
                        atk++;
                    break;
                case ArmyDoctrine.Bulwark:
                    assistDef++;
                    if (!TryAddAbility(abilities, uses, "SP_BODY_GUARD", 0))
                        def++;
                    break;
                case ArmyDoctrine.Tactician:
                    assistDef++;
                    if (!TryAddAbility(abilities, uses, "SP_SEE_THROUGH", 0))
                        TryBoostActiveUse(abilities, uses);
                    break;
            }
        }

        return new CommanderProfile
        {
            NameId = source.NameId,
            Name = source.Name,
            Branch = source.Branch,
            Attack = (byte)Math.Clamp(atk, 0, 20),
            Defense = (byte)Math.Clamp(def, 0, 20),
            AssistAttack = (byte)Math.Clamp(assistAtk, 0, 5),
            AssistDefense = (byte)Math.Clamp(assistDef, 0, 5),
            Abilities = abilities.ToArray(),
            Uses = uses.ToArray()
        };
    }

    internal static CommanderProfile ApplyDirective(CommanderProfile source, DrillDirective directive, bool isEnemy)
    {
        if (directive == DrillDirective.None) return source;
        if (directive == DrillDirective.IronWall && isEnemy)
        {
            return new CommanderProfile
            {
                NameId = source.NameId,
                Name = source.Name,
                Branch = source.Branch,
                Attack = source.Attack,
                Defense = (byte)Math.Clamp(source.Defense + 2, 0, 20),
                AssistAttack = source.AssistAttack,
                AssistDefense = source.AssistDefense,
                Abilities = source.Abilities.ToArray(),
                Uses = source.Uses.ToArray()
            };
        }
        if (directive == DrillDirective.NoMedicine && !isEnemy)
        {
            var uses = source.Uses.ToArray();
            for (int i = 0; i < source.Abilities.Length && i < uses.Length; i++)
            {
                if (source.Abilities[i] is "SP_MEDICAL1" or "SP_MEDICAL2")
                    uses[i] = 0;
            }
            return new CommanderProfile
            {
                NameId = source.NameId,
                Name = source.Name,
                Branch = source.Branch,
                Attack = source.Attack,
                Defense = source.Defense,
                AssistAttack = source.AssistAttack,
                AssistDefense = source.AssistDefense,
                Abilities = source.Abilities.ToArray(),
                Uses = uses
            };
        }
        return source;
    }

    internal static readonly WarSynergyPair[] WarSynergies =
    {
        new(3, 4, "Mercenary Command", AtkBonus: 1, DefBonus: 0, AssistAtkBonus: 1, AssistDefBonus: 0, BoostActiveUse: false),
        new(90, 86, "Strategist Council", AtkBonus: 0, DefBonus: 1, AssistAtkBonus: 0, AssistDefBonus: 1, BoostActiveUse: false),
        new(54, 62, "Arcane Barrage", AtkBonus: 1, DefBonus: 0, AssistAtkBonus: 0, AssistDefBonus: 0, BoostActiveUse: true),
        new(11, 12, "Iron Vanguard", AtkBonus: 1, DefBonus: 1, AssistAtkBonus: 0, AssistDefBonus: 0, BoostActiveUse: false)
    };

    internal static WarSynergyPair? FindWarSynergy(int nameId, IReadOnlyCollection<int> unitMemberNameIds)
    {
        foreach (var pair in WarSynergies)
        {
            if (pair.FirstNameId == nameId && unitMemberNameIds.Contains(pair.SecondNameId)) return pair;
            if (pair.SecondNameId == nameId && unitMemberNameIds.Contains(pair.FirstNameId)) return pair;
        }
        return null;
    }

    internal static CommanderProfile ApplyWarSynergy(CommanderProfile source, WarSynergyPair? synergy)
    {
        if (synergy == null) return source;
        var uses = source.Uses.ToArray();
        if (synergy.BoostActiveUse)
        {
            for (int i = 0; i < source.Abilities.Length && i < uses.Length; i++)
            {
                int id = Array.IndexOf(OverhaulRules.Abilities, source.Abilities[i]);
                if (OverhaulRules.ActiveAbility(id) && uses[i] < 3)
                {
                    uses[i]++;
                    break;
                }
            }
        }
        return new CommanderProfile
        {
            NameId = source.NameId,
            Name = source.Name,
            Branch = source.Branch,
            Attack = (byte)Math.Clamp(source.Attack + synergy.AtkBonus, 0, 20),
            Defense = (byte)Math.Clamp(source.Defense + synergy.DefBonus, 0, 20),
            AssistAttack = (byte)Math.Clamp(source.AssistAttack + synergy.AssistAtkBonus, 0, 5),
            AssistDefense = (byte)Math.Clamp(source.AssistDefense + synergy.AssistDefBonus, 0, 5),
            Abilities = source.Abilities.ToArray(),
            Uses = uses
        };
    }

    internal static void RecordHallClear(ArmyLedgerState state, string bossId, bool nightmare, int turns, bool noKo)
    {
        if (string.IsNullOrWhiteSpace(bossId)) return;
        state.HallRecords ??= new Dictionary<string, HallBossRecord>(StringComparer.Ordinal);
        if (!state.HallRecords.TryGetValue(bossId, out var rec))
        {
            rec = new HallBossRecord();
            state.HallRecords[bossId] = rec;
        }
        int safeTurns = Math.Max(1, turns);
        if (nightmare)
        {
            rec.NightmareCleared = true;
            if (rec.BestNightmareTurns == 0 || safeTurns < rec.BestNightmareTurns) rec.BestNightmareTurns = safeTurns;
            if (noKo) rec.NoKoNightmare = true;
        }
        else
        {
            rec.OriginalCleared = true;
            if (rec.BestOriginalTurns == 0 || safeTurns < rec.BestOriginalTurns) rec.BestOriginalTurns = safeTurns;
            if (noKo) rec.NoKoOriginal = true;
        }
    }

    internal static string HallMedalTag(HallBossRecord? rec)
    {
        if (rec == null) return "";
        if (rec.NightmareCleared)
            return $"★ NM {rec.BestNightmareTurns}T" + (rec.NoKoNightmare ? " No-KO" : "");
        if (rec.OriginalCleared)
            return $"✓ {rec.BestOriginalTurns}T" + (rec.NoKoOriginal ? " No-KO" : "");
        return "";
    }

    internal static string HuntModifierTitle(HuntRiskModifier mod) => mod switch
    {
        HuntRiskModifier.FrenziedQuarry => "Frenzied Quarry",
        HuntRiskModifier.IroncladQuarry => "Ironclad Quarry",
        _ => "Standard Hunt"
    };

    internal static string HuntModifierDescription(HuntRiskModifier mod) => mod switch
    {
        HuntRiskModifier.FrenziedQuarry => "Quarry deals +20% damage. Victory bonus Potch +35%.",
        HuntRiskModifier.IroncladQuarry => "Quarry gains +25% HP. Victory bonus Potch +35%.",
        _ => "No extra Hunt modifier. Standard contract spoils."
    };

    internal static double HuntModifierStatFactor(HuntRiskModifier mod, bool hp) => mod switch
    {
        HuntRiskModifier.IroncladQuarry when hp => 1.25,
        HuntRiskModifier.FrenziedQuarry when !hp => 1.20,
        _ => 1.0
    };

    internal static int HuntModifierBonusPotch(int baseBonus, HuntRiskModifier mod) =>
        baseBonus <= 0 || mod == HuntRiskModifier.None
            ? baseBonus
            : (int)Math.Round(baseBonus * 1.35, MidpointRounding.AwayFromZero);
}
