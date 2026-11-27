namespace NightmareRebalance;

internal enum CharacterTalent { None = 0, Berserker = 1, Arcanist = 2, Bulwark = 3, Tactician = 4 }

internal enum TalentSetBonus { None = 0, BalancedVanguard = 1, WarbandFury = 2, ArcaneConclave = 3, IronCitadel = 4 }

internal enum WarBranchOverride { Default = 0, Infantry = 1, Archer = 2, Magicians = 3 }

internal sealed class CharacterMastery
{
    public int CharaId { get; set; }
    public string Name { get; set; } = "";
    public int Xp { get; set; }
    public CharacterTalent Talent { get; set; } = CharacterTalent.None;

    internal CharacterMastery Clone() => new()
    {
        CharaId = CharaId,
        Name = Name,
        Xp = Xp,
        Talent = Talent
    };
}

internal sealed class NemesisRecord
{
    public string SpeciesId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public int Tier { get; set; } = 1;
    public ApexAffix Affix { get; set; } = ApexAffix.Frenzied;
    public int EncountersSurvived { get; set; } = 1;

    internal NemesisRecord Clone() => new()
    {
        SpeciesId = SpeciesId,
        Name = Name,
        Title = Title,
        Tier = Tier,
        Affix = Affix,
        EncountersSurvived = EncountersSurvived
    };
}

internal sealed record TowerRelicSpec(string Id, string Name, string Summary);

internal static class Reforged5Rules
{
    internal static readonly int[] MasteryThresholds = { 0, 100, 250, 500, 900 };

    internal static int MasteryRankForXp(int xp)
    {
        if (xp >= MasteryThresholds[4]) return 5;
        if (xp >= MasteryThresholds[3]) return 4;
        if (xp >= MasteryThresholds[2]) return 3;
        if (xp >= MasteryThresholds[1]) return 2;
        return 1;
    }

    internal static string TalentTitle(CharacterTalent t) => t switch
    {
        CharacterTalent.Berserker => "Berserker",
        CharacterTalent.Arcanist => "Arcanist",
        CharacterTalent.Bulwark => "Bulwark",
        CharacterTalent.Tactician => "Tactician",
        _ => "Unassigned"
    };

    internal static string TalentDescription(CharacterTalent t) => t switch
    {
        CharacterTalent.Berserker => "+8% physical DMG (+14% at Rank III+) and 5% HP Lifesteal on physical attacks.",
        CharacterTalent.Arcanist => "+10% magic DMG (+16% at Rank III+) and bonus Rune Charge recovery synergy.",
        CharacterTalent.Bulwark => "-8% all damage taken (-14% at Rank III+) and Riposte counters deal +30% instead of +20%.",
        CharacterTalent.Tactician => "Applies 2 stacks of Armor Break per hit and generates +50% extra Momentum.",
        _ => "No Talent assigned. Confirm to cycle Berserker / Arcanist / Bulwark / Tactician."
    };

    internal static int ApplyTalentPhysical(int damage, CharacterTalent talent, int masteryRank, out int lifestealHp)
    {
        lifestealHp = 0;
        if (damage <= 0 || talent != CharacterTalent.Berserker) return damage;
        double mult = masteryRank >= 3 ? 1.14 : 1.08;
        int boosted = Math.Max(damage + 1, (int)Math.Round(damage * mult, MidpointRounding.AwayFromZero));
        lifestealHp = Math.Max(1, (int)Math.Round(boosted * 0.05, MidpointRounding.AwayFromZero));
        return boosted;
    }

    internal static int ApplyTalentMagic(int damage, CharacterTalent talent, int masteryRank)
    {
        if (damage <= 0 || talent != CharacterTalent.Arcanist) return damage;
        double mult = masteryRank >= 3 ? 1.16 : 1.10;
        return Math.Max(damage + 1, (int)Math.Round(damage * mult, MidpointRounding.AwayFromZero));
    }

    internal static int ApplyTalentDefense(int damage, CharacterTalent talent, int masteryRank)
    {
        if (damage <= 1 || talent != CharacterTalent.Bulwark) return damage;
        double mult = masteryRank >= 3 ? 0.86 : 0.92;
        return Math.Max(1, (int)Math.Round(damage * mult, MidpointRounding.AwayFromZero));
    }

    internal static int MomentumBonusForTalent(int baseDelta, CharacterTalent talent) =>
        talent == CharacterTalent.Tactician ? (int)Math.Round(baseDelta * 1.5, MidpointRounding.AwayFromZero) : baseDelta;

    internal static string BranchOverrideTitle(WarBranchOverride b) => b switch
    {
        WarBranchOverride.Infantry => "Infantry (Override)",
        WarBranchOverride.Archer => "Archer (Override)",
        WarBranchOverride.Magicians => "Magicians (Override)",
        _ => "Default Branch"
    };

    internal static CommanderProfile ApplyBranchOverride(CommanderProfile source, WarBranchOverride branch)
    {
        if (branch == WarBranchOverride.Default) return source;
        string bName = branch switch
        {
            WarBranchOverride.Infantry => "Infantry",
            WarBranchOverride.Archer => "Archer",
            WarBranchOverride.Magicians => "Magicians",
            _ => source.Branch
        };
        return new CommanderProfile
        {
            NameId = source.NameId,
            Name = source.Name,
            Branch = bName,
            Attack = source.Attack,
            Defense = source.Defense,
            AssistAttack = source.AssistAttack,
            AssistDefense = source.AssistDefense,
            Abilities = source.Abilities.ToArray(),
            Uses = source.Uses.ToArray()
        };
    }

    static readonly string[] NemesisTitles =
    {
        "the Unbroken", "the Bloodied", "the Relentless", "the Grim", "the Executioner", "the Undying"
    };

    internal static NemesisRecord PromoteNemesis(NemesisRecord? existing, string speciesId, string name, int rngSeed)
    {
        if (existing != null && existing.SpeciesId == speciesId)
        {
            var upgraded = existing.Clone();
            upgraded.Tier = Math.Min(5, upgraded.Tier + 1);
            upgraded.EncountersSurvived++;
            return upgraded;
        }
        return new NemesisRecord
        {
            SpeciesId = speciesId,
            Name = name,
            Title = NemesisTitles[Math.Abs(rngSeed) % NemesisTitles.Length],
            Tier = 1,
            Affix = Reforged2Rules.RollApexAffix(rngSeed),
            EncountersSurvived = 1
        };
    }

    internal static double NemesisHpMultiplier(int tier) => 1.0 + Math.Clamp(tier, 1, 5) * 0.20;
    internal static double NemesisDamageMultiplier(int tier) => 1.0 + Math.Clamp(tier, 1, 5) * 0.12;
    internal static int NemesisCommendationReward(int tier) => Math.Clamp(tier + 1, 2, 6);
    internal static bool ShouldAmbushWithNemesis(int roll0To99) => roll0To99 >= 0 && roll0To99 < 20;

    internal static readonly TowerRelicSpec[] TowerRelics =
    {
        new("vampiric-fang", "Vampiric Fang", "+10% HP Lifesteal on all physical attacks during this Tower run."),
        new("arcane-battery", "Arcane Battery", "+25% magic damage during this Tower run."),
        new("overdrive-core", "Overdrive Core", "Start every Tower floor with 60% Party Momentum."),
        new("shatter-gauntlet", "Shatter Gauntlet", "+30% bonus damage on all Combat Combos."),
        new("aegis-crest", "Aegis Crest", "-15% all incoming damage during this Tower run."),
        new("executioner-seal", "Executioner's Seal", "Execute Finisher procs at <= 35% enemy HP."),
        new("golden-chalice", "Golden Chalice", "+50% Potch and +1 bonus Commendation on Tower Cash-Out."),
        new("stormcaller-sigil", "Stormcaller Sigil", "+20% First Strike and Spellblade Resonance damage."),
        new("iron-riposte", "Iron Riposte", "+25% Riposte and Desperation Surge damage."),
        new("blood-pact", "Blood Pact", "+25% all party damage dealt, +10% damage taken."),
        new("tactician-tome", "Tactician's Tome", "Living allies recover 20% max HP after every Tower floor."),
        new("titan-heart", "Titan's Heart", "+15% all damage dealt while at full Momentum / Overdrive.")
    };

    internal static double TowerFloorHpMultiplier(int floor) => 1.0 + Math.Max(0, floor - 1) * 0.08;
    internal static double TowerFloorDamageMultiplier(int floor) => 1.0 + Math.Max(0, floor - 1) * 0.05;
    internal static int TowerCashOutPotch(int floorsCleared, bool goldenChalice)
    {
        int raw = Math.Max(0, floorsCleared) * 500;
        return goldenChalice ? (int)Math.Round(raw * 1.5, MidpointRounding.AwayFromZero) : raw;
    }
    internal static int TowerCashOutCommendations(int floorsCleared, bool goldenChalice) =>
        Math.Max(0, floorsCleared) + (goldenChalice && floorsCleared > 0 ? 1 : 0);

    internal static TowerRelicSpec[] DraftRelics(IReadOnlyCollection<string> ownedIds, int seed)
    {
        var pool = TowerRelics.Where(r => !ownedIds.Contains(r.Id)).ToList();
        if (pool.Count == 0) return Array.Empty<TowerRelicSpec>();
        var rng = new Random(seed);
        var result = new List<TowerRelicSpec>(3);
        while (result.Count < 3 && pool.Count > 0)
        {
            int idx = rng.Next(pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result.ToArray();
    }

    internal static readonly TowerRelicSpec[] LegendaryRelics =
    {
        new("bright-shield-echo", "★ Bright Shield Echo", "LEGENDARY · -20% all incoming damage & +15% post-floor party HP heal."),
        new("black-sword-echo", "★ Black Sword Echo", "LEGENDARY · +30% all party physical and magic damage."),
        new("true-rune-surge", "★ True Rune Surge", "LEGENDARY · Start every Tower floor at 100% Momentum (Dunan Overdrive!).")
    };

    internal static bool IsWardenFloor(int floor) => floor > 0 && floor % 5 == 0;

    internal static TowerRelicSpec[] DraftRelicsForFloor(int clearedFloor, IReadOnlyCollection<string> ownedIds, int seed)
    {
        if (IsWardenFloor(clearedFloor))
        {
            var legPool = LegendaryRelics.Where(r => !ownedIds.Contains(r.Id)).ToArray();
            if (legPool.Length > 0) return legPool;
        }
        return DraftRelics(ownedIds, seed);
    }

    internal static int TowerCashOutPotch(int floorsCleared, bool goldenChalice, bool bloodOath)
    {
        int raw = TowerCashOutPotch(floorsCleared, goldenChalice);
        return bloodOath ? (int)Math.Round(raw * 1.5, MidpointRounding.AwayFromZero) : raw;
    }

    internal static int TowerCashOutCommendations(int floorsCleared, bool goldenChalice, bool bloodOath) =>
        TowerCashOutCommendations(floorsCleared, goldenChalice) + (bloodOath && floorsCleared > 0 ? 2 : 0);

    internal static TalentSetBonus EvaluateTalentSet(IReadOnlyList<CharacterTalent> partyTalents)
    {
        int zerk = 0, arc = 0, bul = 0, tac = 0;
        for (int i = 0; i < partyTalents.Count; i++)
        {
            switch (partyTalents[i])
            {
                case CharacterTalent.Berserker: zerk++; break;
                case CharacterTalent.Arcanist: arc++; break;
                case CharacterTalent.Bulwark: bul++; break;
                case CharacterTalent.Tactician: tac++; break;
            }
        }
        if (zerk >= 1 && arc >= 1 && bul >= 1 && tac >= 1) return TalentSetBonus.BalancedVanguard;
        if (zerk >= 3) return TalentSetBonus.WarbandFury;
        if (arc >= 3) return TalentSetBonus.ArcaneConclave;
        if (bul >= 3) return TalentSetBonus.IronCitadel;
        return TalentSetBonus.None;
    }

    internal static string TalentSetTitle(TalentSetBonus set) => set switch
    {
        TalentSetBonus.BalancedVanguard => "Balanced Vanguard (+8% DMG · +15 Mom)",
        TalentSetBonus.WarbandFury => "Warband Fury (+10% Phys · +3% Leech)",
        TalentSetBonus.ArcaneConclave => "Arcane Conclave (+12% Magic · +8 Mom)",
        TalentSetBonus.IronCitadel => "Iron Citadel (-10% All DMG Taken)",
        _ => ""
    };

    internal static int NemesisAmbushChance(int battlesSinceSeen) =>
        Math.Clamp(20 + Math.Max(0, battlesSinceSeen) * 10, 20, 80);

    internal static string BuildChronicleMarkdown(ArmyLedgerState ledger)
    {
        var sb = new System.Text.StringBuilder(1024);
        sb.AppendLine("# Suikoden II: Reforged Nightmare — Campaign Chronicle");
        sb.AppendLine();
        sb.AppendLine($"- **Dunan Commendations:** {ledger.Commendations}");
        sb.AppendLine($"- **Highest Tower of Dunan Floor:** {ledger.HighestTowerFloor}");
        sb.AppendLine($"- **Dunan Honors Unlocked:** {ledger.UnlockedHonors?.Count ?? 0} / {Reforged2Rules.Honors.Length}");
        sb.AppendLine($"- **Castle Upgrades Unlocked:** {ledger.CastleUpgrades?.Count ?? 0} / {Reforged2Rules.CastleUpgrades.Length}");
        sb.AppendLine($"- **Army Drills Won:** {ledger.TotalDrillsWon} ({ledger.TotalVeteranDrillsWon} Veteran) · Story Wars: {ledger.TotalStoryWarsWon}");
        sb.AppendLine($"- **Hunter's Bestiary:** {ledger.Bestiary?.Count ?? 0} species · Apex Packs Cleared: {ledger.ApexPacksCleared} · Nemeses Slain: {ledger.NemesesSlain}");
        sb.AppendLine($"- **Combat Feats:** {ledger.TotalCombosLanded} Combos · {ledger.TotalOverdrivesTriggered} Overdrives · {ledger.TotalSRanks} S-Ranks");
        if (ledger.ActiveNemesis is {} nem)
            sb.AppendLine($"- **Active Nemesis:** {nem.Name} {nem.Title} (Tier {nem.Tier}, {Reforged2Rules.ApexAffixShort(nem.Affix)})");
        sb.AppendLine();
        sb.AppendLine("## Top Army Commanders");
        foreach (var c in ledger.Commanders.Values.OrderByDescending(x => x.Xp).Take(8))
            sb.AppendLine($"- **{c.Name}** — {ArmyProgressionRules.RankTitle(ArmyProgressionRules.RankForXp(c.Xp))} ({c.Xp} XP, {ArmyProgressionRules.DoctrineTitle(c.Doctrine)}, {BranchOverrideTitle(c.BranchOverride)})");
        if (ledger.PartyMastery != null && ledger.PartyMastery.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Party Character Talents");
            foreach (var m in ledger.PartyMastery.Values.OrderByDescending(x => x.Xp).Take(10))
                sb.AppendLine($"- **{m.Name}** — Mastery Rank {MasteryRankForXp(m.Xp)} ({m.Xp} XP, {TalentTitle(m.Talent)})");
        }
        return sb.ToString();
    }
}
