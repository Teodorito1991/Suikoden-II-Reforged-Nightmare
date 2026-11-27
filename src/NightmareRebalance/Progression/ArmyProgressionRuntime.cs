using BepInEx;

namespace NightmareRebalance;

// Content-addressed save-bound army progression ledger.
// Native gsd2/DataXX save files are never modified; snapshots are keyed by the
// SHA256 fingerprint of the exact native campaign records at save/load time.
internal static class ArmyProgressionRuntime
{
    static readonly object Sync = new();
    static ArmyLedgerIndex index = new();
    static IntPtr boundCampaignOwner = IntPtr.Zero;
    internal static ArmyLedgerState Current { get; private set; } = new();
    internal static DrillDirective ActiveDirective { get; set; } = DrillDirective.None;
    internal static HuntRiskModifier ActiveHuntModifier { get; set; } = HuntRiskModifier.None;
    internal static ArmyStance ActiveStance { get; set; } = ArmyStance.Balanced;
    internal static WarMutator ActiveMutator { get; set; } = WarMutator.Standard;
    internal static string LastAwardNotice { get; private set; } = "";
    static string LedgerPath => Path.Combine(Paths.BepInExRootPath, "NightmareRebalance", "army-ledger.json");

    internal static bool Enabled => Plugin.Options?.EnableArmyProgression.Value == true;
    internal static bool DirectivesEnabled => Plugin.Options?.EnableDrillDirectives.Value == true;
    internal static bool SynergyEnabled => Plugin.Options?.EnableCrossTheaterSynergy.Value == true;
    internal static bool HuntModifiersEnabled => Plugin.Options?.EnableHuntModifiers.Value == true;

    internal static void Initialize()
    {
        lock (Sync)
        {
            Current = new ArmyLedgerState();
            boundCampaignOwner = IntPtr.Zero;
            ActiveDirective = DrillDirective.None;
            LastAwardNotice = "";
            try
            {
                if (File.Exists(LedgerPath))
                    index = ArmyProgressionRules.ParseIndex(File.ReadAllText(LedgerPath));
                else
                    index = new ArmyLedgerIndex();
            }
            catch (Exception ex)
            {
                index = new ArmyLedgerIndex();
                Plugin.Logger.LogWarning("Army progression ledger could not be read; starting empty: " + ex.Message);
            }
        }
    }

    internal static void Clear()
    {
        lock (Sync)
        {
            Current = new ArmyLedgerState();
            boundCampaignOwner = IntPtr.Zero;
            ActiveDirective = DrillDirective.None;
        }
    }

    static string? ComputeLiveFingerprint(GAME_WORK? game)
    {
        if (game?.game_data == null || game.war_data == null || game.chara_data == null || game.party_data == null)
            return null;
        string[] records =
        {
            LitJson.JsonMapper.ToJson(game.game_data),
            LitJson.JsonMapper.ToJson(game.war_data),
            LitJson.JsonMapper.ToJson(game.chara_data),
            LitJson.JsonMapper.ToJson(game.party_data)
        };
        byte[][] flags =
        {
            game.chara_flag?.ToArray() ?? Array.Empty<byte>(),
            game.town_flag?.ToArray() ?? Array.Empty<byte>(),
            game.event_flag?.ToArray() ?? Array.Empty<byte>(),
            game.scpoi_flag?.ToArray() ?? Array.Empty<byte>(),
            game.t_box_flag?.ToArray() ?? Array.Empty<byte>()
        };
        return ArmyProgressionRules.ComputeFingerprint(records, flags);
    }

    static void PersistIndexLocked()
    {
        try
        {
            string dir = Path.GetDirectoryName(LedgerPath)!;
            Directory.CreateDirectory(dir);
            string tmp = LedgerPath + ".tmp";
            File.WriteAllText(tmp, ArmyProgressionRules.SerializeIndex(index));
            File.Move(tmp, LedgerPath, overwrite: true);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning("Army progression ledger write failed: " + ex.Message);
        }
    }

    internal static void ObserveCampaignSave(string path)
    {
        if (!Enabled && !SynergyEnabled) return;
        if (!TrainingPersistenceRules.IsCampaignPath(path)) return;
        var game = Framework.Singleton<GAME_WORK>.instance;
        string? hash = ComputeLiveFingerprint(game);
        if (hash == null) return;
        lock (Sync)
        {
            if (boundCampaignOwner != IntPtr.Zero && game!.game_data.Pointer != boundCampaignOwner)
                return;
            boundCampaignOwner = game!.game_data.Pointer;
            ArmyProgressionRules.BindSnapshot(index, hash, Current, DateTime.UtcNow.Ticks);
            PersistIndexLocked();
        }
        Plugin.Debug($"Army progression ledger bound to save fingerprint {hash[..12]}...");
    }

    internal static void ObserveCampaignLoad()
    {
        var game = Framework.Singleton<GAME_WORK>.instance;
        string? hash = ComputeLiveFingerprint(game);
        lock (Sync)
        {
            boundCampaignOwner = game?.game_data?.Pointer ?? IntPtr.Zero;
            Current = ArmyProgressionRules.ResolveSnapshot(index, hash);
            LastAwardNotice = "";
        }
        if (hash != null && (Enabled || SynergyEnabled))
            Plugin.Debug($"Army progression loaded for {hash[..12]}...: {Current.Commanders.Count} commanders, {Current.TotalDrillsWon} drills won.");
    }

    internal static CommanderProfile BuildBaseProfile(WAR_CHARA_TYPE original)
    {
        var abilities = new List<string>(3);
        var uses = new List<byte>(3);
        if (original.nouryoku != null)
        {
            for (int i = 0; i < original.nouryoku.Length && i < 3; i++)
            {
                int id = (int)original.nouryoku[i];
                if (id <= 0 || id >= OverhaulRules.Abilities.Length) continue;
                abilities.Add(OverhaulRules.Abilities[id]);
                byte u = original.kaisu_max != null && i < original.kaisu_max.Length ? original.kaisu_max[i] : (byte)0;
                uses.Add(u);
            }
        }
        string branch = original.force_type == war_data_h.INFANTRY ? "Infantry"
            : original.force_type == war_data_h.ARCHER ? "Archer"
            : original.force_type == war_data_h.MAGICIANS ? "Magicians" : "Unknown";
        return new CommanderProfile
        {
            NameId = original.name,
            Name = OverhaulRuntime.Commanders.Profiles.FirstOrDefault(p => p.NameId == original.name)?.Name ?? ("Commander #" + original.name),
            Branch = branch,
            Attack = original.attack,
            Defense = original.defense,
            AssistAttack = original.assist_attack,
            AssistDefense = original.assist_defense,
            Abilities = abilities.ToArray(),
            Uses = uses.ToArray()
        };
    }

    static WarSynergyPair? ActiveUnitSynergy(int nameId)
    {
        var units = w_unit.unit;
        var chars = TotalWarRuntime.Characters(war_h.g_w_war_no);
        if (units is not { Length: 2 } || units[0] is not { Length: 15 } allies || chars == null) return null;
        for (int i = 0; i < allies.Length; i++)
        {
            var u = allies[i];
            if (u == null || u.flag != 1) continue;
            var members = new List<int>(3);
            for (int slot = 0; slot < 3; slot++)
            {
                int idx = w_unit.GetUnitChara(0, i, slot);
                if (idx < 0 || idx >= chars.Length) continue;
                var ch = chars[idx];
                if (ch != null && ch.name > 0) members.Add(ch.name);
            }
            if (members.Contains(nameId))
                return ArmyProgressionRules.FindWarSynergy(nameId, members);
        }
        return null;
    }

    internal static CommanderProfile? Enhance(WAR_CHARA_TYPE original, CommanderProfile? existingProfile, bool isEnemy)
    {
        bool training = TrainingWar.Active;
        var skirmish = TrainingWar.ActiveSkirmish;
        bool applySkirmish = training && isEnemy && skirmish != null;
        bool applyDirective = training && DirectivesEnabled && ActiveDirective != DrillDirective.None;
        bool applyStance = !isEnemy && ActiveStance != ArmyStance.Balanced;
        bool applyMutator = training && ActiveMutator != WarMutator.Standard;
        bool applyProg = Enabled && !isEnemy && Current.Commanders.TryGetValue(original.name, out var progress) && ArmyProgressionRules.RankForXp(progress.Xp) > ArmyRank.Recruit;
        var synergy = Enabled && !isEnemy ? ActiveUnitSynergy(original.name) : null;
        if (!applySkirmish && !applyDirective && !applyStance && !applyMutator && !applyProg && synergy == null) return existingProfile;

        var profile = existingProfile ?? BuildBaseProfile(original);
        if (applyProg && Current.Commanders.TryGetValue(original.name, out var prog))
        {
            profile = ArmyProgressionRules.ApplyProgression(profile, prog);
            if (training && prog.BranchOverride != WarBranchOverride.Default)
                profile = Reforged5Rules.ApplyBranchOverride(profile, prog.BranchOverride);
        }
        if (synergy != null)
            profile = ArmyProgressionRules.ApplyWarSynergy(profile, synergy);
        if (applyStance)
            profile = Reforged2Rules.ApplyStance(profile, ActiveStance);
        if (applySkirmish)
            profile = Reforged2Rules.ApplySkirmishEnemyBoost(profile, skirmish!);
        if (applyMutator)
            profile = Reforged2Rules.ApplyMutator(profile, ActiveMutator);
        if (applyDirective)
            profile = ArmyProgressionRules.ApplyDirective(profile, ActiveDirective, isEnemy);
        return profile;
    }

    internal static List<CommanderParticipation> CaptureAlliedParticipants()
    {
        var result = new List<CommanderParticipation>();
        var units = w_unit.unit;
        if (units is not { Length: 2 } || units[0] is not { Length: 15 } allies) return result;
        for (int i = 0; i < allies.Length; i++)
        {
            var u = allies[i];
            if (u == null || u.flag == 0) continue;
            bool survived = u.flag == 1;
            for (int slot = 0; slot < 3; slot++)
            {
                int charaIndex = w_unit.GetUnitChara(0, i, slot);
                if (charaIndex < 0) continue;
                var ch = w_chara.chara(charaIndex);
                if (ch == null || ch.name <= 0) continue;
                string name = OverhaulRuntime.Commanders.Profiles.FirstOrDefault(p => p.NameId == ch.name)?.Name ?? ("Commander #" + ch.name);
                result.Add(new CommanderParticipation(ch.name, name, slot == 0, survived));
            }
        }
        return result;
    }

    internal static XpAwardSummary RecordTrainingVictory(IReadOnlyList<CommanderParticipation> participants, bool veteran, DrillDirective directive, long turnCount)
    {
        if (!Enabled && !SynergyEnabled) return default;
        lock (Sync)
        {
            var game = Framework.Singleton<GAME_WORK>.instance;
            if (game?.game_data != null) boundCampaignOwner = game.game_data.Pointer;
            var summary = ArmyProgressionRules.AwardTrainingXp(Current, participants, veteran, directive, turnCount, SynergyEnabled);
            if (Current.CastleUpgrades?.Contains("war-college") == true && summary.LeaderXp > 0)
            {
                int bonus = Math.Max(5, summary.LeaderXp / 4);
                foreach (var p in participants)
                    if (Current.Commanders.TryGetValue(p.NameId, out var c)) c.Xp += bonus;
            }
            EvaluateHonorsLocked();
            LastAwardNotice = summary.CommandersUpdated > 0
                ? $"Commanders earned +{summary.LeaderXp} XP" + (summary.RanksGained > 0 ? $" ({summary.RanksGained} promoted!)" : "") + "."
                : "";
            Plugin.Logger.LogInfo($"Army Progression: training victory recorded; commanders={summary.CommandersUpdated}, leaderXp={summary.LeaderXp}, promotions={summary.RanksGained}, directive={directive}, fulfilled={summary.DirectiveFulfilled}, moraleCharges={Current.MoraleBonusCharges}.");
            return summary;
        }
    }

    internal static XpAwardSummary RecordStoryWarVictory()
    {
        if (!Enabled || TrainingWar.Active) return default;
        var participants = CaptureAlliedParticipants();
        if (participants.Count == 0) return default;
        lock (Sync)
        {
            var game = Framework.Singleton<GAME_WORK>.instance;
            if (game?.game_data != null) boundCampaignOwner = game.game_data.Pointer;
            var summary = ArmyProgressionRules.AwardStoryWarXp(Current, participants);
            Plugin.Logger.LogInfo($"Army Progression: story war victory recorded; commanders={summary.CommandersUpdated}, promotions={summary.RanksGained}.");
            return summary;
        }
    }

    internal static XpAwardSummary RecordHallNightmareVictory(BATTLE_WORK? work)
    {
        if (!SynergyEnabled || work?.pm_data == null) return default;
        var commanders = new List<(int NameId, string Name)>();
        foreach (var unit in work.pm_data)
        {
            if (unit == null || !unit.IsPlayer || unit.hp <= 0 || unit.IsDead) continue;
            var profile = OverhaulRuntime.Commanders.Profiles.FirstOrDefault(p => p.NameId == unit.chara_id);
            if (profile != null) commanders.Add((profile.NameId, profile.Name));
        }
        lock (Sync)
        {
            var summary = ArmyProgressionRules.AwardHallNightmareXp(Current, commanders);
            if (summary.CommandersUpdated > 0)
                Plugin.Logger.LogInfo($"Cross-Theater Synergy: Hall Nightmare clear granted +35 War XP to {summary.CommandersUpdated} party commander(s).");
            return summary;
        }
    }

    internal static bool ConsumeMoraleBonus()
    {
        if (!SynergyEnabled) return false;
        lock (Sync)
        {
            if (Current.MoraleBonusCharges <= 0) return false;
            Current.MoraleBonusCharges--;
            return true;
        }
    }

    internal static CommanderProgress[] RosterSnapshot()
    {
        lock (Sync)
        {
            return Current.Commanders.Values
                .OrderByDescending(c => c.Xp)
                .ThenBy(c => c.Name, StringComparer.Ordinal)
                .Select(c => c.Clone())
                .ToArray();
        }
    }

    internal static ArmyDoctrine CycleDoctrine(int nameId)
    {
        lock (Sync)
        {
            if (!Current.Commanders.TryGetValue(nameId, out var entry)) return ArmyDoctrine.None;
            entry.Doctrine = (ArmyDoctrine)(((int)entry.Doctrine + 1) % 4);
            return entry.Doctrine;
        }
    }

    internal static WarBranchOverride CycleBranchOverride(int nameId)
    {
        lock (Sync)
        {
            if (!Current.Commanders.TryGetValue(nameId, out var entry)) return WarBranchOverride.Default;
            entry.BranchOverride = (WarBranchOverride)(((int)entry.BranchOverride + 1) % 4);
            return entry.BranchOverride;
        }
    }

    internal static CharacterMastery[] PartyMasterySnapshot()
    {
        lock (Sync)
        {
            Current.PartyMastery ??= new Dictionary<int, CharacterMastery>();
            int[] party = StarBondRuntime.FieldParty();
            foreach (int id in party)
            {
                if (id <= 0 || Current.PartyMastery.ContainsKey(id)) continue;
                string name = G2_SYS.G2_name_get(id)?.Trim() ?? ("Hero #" + id);
                Current.PartyMastery[id] = new CharacterMastery { CharaId = id, Name = name };
            }
            return Current.PartyMastery.Values
                .OrderByDescending(m => party.Contains(m.CharaId) ? 1 : 0)
                .ThenByDescending(m => m.Xp)
                .ThenBy(m => m.Name, StringComparer.Ordinal)
                .Select(m => m.Clone())
                .ToArray();
        }
    }

    internal static (CharacterTalent Talent, int MasteryRank) GetCharacterTalent(int charaId)
    {
        if (charaId <= 0) return (CharacterTalent.None, 1);
        lock (Sync)
        {
            if (Current.PartyMastery != null && Current.PartyMastery.TryGetValue(charaId, out var m))
                return (m.Talent, Reforged5Rules.MasteryRankForXp(m.Xp));
            return (CharacterTalent.None, 1);
        }
    }

    internal static CharacterTalent CycleCharacterTalent(int charaId, string name)
    {
        lock (Sync)
        {
            Current.PartyMastery ??= new Dictionary<int, CharacterMastery>();
            if (!Current.PartyMastery.TryGetValue(charaId, out var m))
            {
                m = new CharacterMastery { CharaId = charaId, Name = name };
                Current.PartyMastery[charaId] = m;
            }
            m.Talent = (CharacterTalent)(((int)m.Talent + 1) % 5);
            return m.Talent;
        }
    }

    internal static void AwardPartyMastery(int charaId, string name, int xp)
    {
        if (charaId <= 0 || xp <= 0) return;
        lock (Sync)
        {
            Current.PartyMastery ??= new Dictionary<int, CharacterMastery>();
            if (!Current.PartyMastery.TryGetValue(charaId, out var m))
            {
                m = new CharacterMastery { CharaId = charaId, Name = name };
                Current.PartyMastery[charaId] = m;
            }
            if (!string.IsNullOrWhiteSpace(name)) m.Name = name;
            m.Xp = Math.Min(9999, m.Xp + xp);
        }
    }

    internal static NemesisRecord PromoteNemesis(string speciesId, string name, int seed)
    {
        lock (Sync)
        {
            Current.ActiveNemesis = Reforged5Rules.PromoteNemesis(Current.ActiveNemesis, speciesId, name, seed);
            return Current.ActiveNemesis.Clone();
        }
    }

    internal static int ClearNemesis()
    {
        lock (Sync)
        {
            if (Current.ActiveNemesis == null) return 0;
            int comm = Reforged5Rules.NemesisCommendationReward(Current.ActiveNemesis.Tier);
            Current.ActiveNemesis = null;
            Current.NemesesSlain++;
            Current.Commendations = Math.Min(999, Current.Commendations + comm);
            return comm;
        }
    }

    internal static void RecordTowerFloor(int floorsCleared)
    {
        lock (Sync)
        {
            if (floorsCleared > Current.HighestTowerFloor)
                Current.HighestTowerFloor = floorsCleared;
        }
    }

    internal static DrillDirective CycleDirective()
    {
        ActiveDirective = (DrillDirective)(((int)ActiveDirective + 1) % 4);
        return ActiveDirective;
    }

    internal static HuntRiskModifier CycleHuntModifier()
    {
        ActiveHuntModifier = (HuntRiskModifier)(((int)ActiveHuntModifier + 1) % 3);
        return ActiveHuntModifier;
    }

    internal static ArmyStance CycleStance()
    {
        ActiveStance = (ArmyStance)(((int)ActiveStance + 1) % 4);
        return ActiveStance;
    }

    internal static WarMutator CycleMutator()
    {
        ActiveMutator = (WarMutator)(((int)ActiveMutator + 1) % 3);
        return ActiveMutator;
    }

    internal static bool HasUpgrade(string upgradeId)
    {
        lock (Sync)
            return Current.CastleUpgrades != null && Current.CastleUpgrades.Contains(upgradeId);
    }

    internal static bool TryUnlockUpgrade(CastleUpgradeSpec spec, out string feedback)
    {
        lock (Sync)
        {
            Current.CastleUpgrades ??= new HashSet<string>(StringComparer.Ordinal);
            if (Current.CastleUpgrades.Contains(spec.Id))
            {
                feedback = $"{spec.Name} is already unlocked.";
                return false;
            }
            if (Current.Commendations < spec.CostCommendations)
            {
                feedback = $"Need {spec.CostCommendations} Commendation(s) (you have {Current.Commendations}).";
                return false;
            }
            Current.Commendations -= spec.CostCommendations;
            Current.CastleUpgrades.Add(spec.Id);
            var honors = EvaluateHonorsLocked();
            var game = Framework.Singleton<GAME_WORK>.instance;
            if (game?.game_data != null) boundCampaignOwner = game.game_data.Pointer;
            feedback = $"Unlocked {spec.Name}!" + (honors.Count > 0 ? $" Honor: {honors[0].Title} (+2 Comm)!" : " Save at an inn or orb to bind.");
            return true;
        }
    }

    internal static (bool IsNewSpecies, int MilestoneCommendations) RecordBestiaryKill(string speciesKey, string displayName, int level, string weaknessHint, bool apex)
    {
        if (string.IsNullOrWhiteSpace(speciesKey)) return (false, 0);
        lock (Sync)
        {
            Current.Bestiary ??= new Dictionary<string, BestiaryEntry>(StringComparer.Ordinal);
            bool isNew = false;
            if (!Current.Bestiary.TryGetValue(speciesKey, out var entry))
            {
                entry = new BestiaryEntry { Name = displayName, WeaknessHint = weaknessHint };
                Current.Bestiary[speciesKey] = entry;
                isNew = true;
            }
            entry.Kills++;
            if (apex) entry.ApexKills++;
            if (level > entry.HighestLevel) entry.HighestLevel = level;
            if (!string.IsNullOrWhiteSpace(weaknessHint)) entry.WeaknessHint = weaknessHint;

            int milestones = Reforged2Rules.BestiaryMilestonesFor(Current.Bestiary.Count);
            int delta = Math.Max(0, milestones - Current.BestiaryMilestonesClaimed);
            if (delta > 0)
            {
                Current.BestiaryMilestonesClaimed = milestones;
                Current.Commendations = Math.Min(999, Current.Commendations + delta);
            }
            return (isNew, delta);
        }
    }

    internal static BestiaryEntry[] BestiarySnapshot()
    {
        lock (Sync)
        {
            if (Current.Bestiary == null || Current.Bestiary.Count == 0) return Array.Empty<BestiaryEntry>();
            return Current.Bestiary.Values
                .OrderByDescending(e => e.Kills)
                .ThenBy(e => e.Name, StringComparer.Ordinal)
                .Select(e => e.Clone())
                .ToArray();
        }
    }

    internal static string? AdvanceBounty(FieldBountyKind kind, int amount = 1)
    {
        if (amount <= 0 || Plugin.Options?.EnableFieldBounties.Value != true) return null;
        lock (Sync)
        {
            var spec = Reforged2Rules.ActiveBounty(Current.ActiveBountyIndex);
            if (spec.Kind != kind) return null;
            Current.ActiveBountyProgress += amount;
            if (Current.ActiveBountyProgress < spec.Target) return null;
            Current.BountiesCompleted++;
            Current.Commendations = Math.Min(999, Current.Commendations + 1);
            Current.ActiveBountyIndex = (Current.ActiveBountyIndex + 1) % Reforged2Rules.FieldBounties.Length;
            Current.ActiveBountyProgress = 0;
            EvaluateHonorsLocked();
            return $"Bounty Complete: {spec.Title}! (+1 Commendation)";
        }
    }

    internal static void RecordApexClear()
    {
        lock (Sync)
            Current.ApexPacksCleared++;
    }

    internal static void RecordBattleStats(int combos, int overdrives, bool sRank)
    {
        lock (Sync)
        {
            if (combos > 0) Current.TotalCombosLanded += combos;
            if (overdrives > 0) Current.TotalOverdrivesTriggered += overdrives;
            if (sRank) Current.TotalSRanks++;
        }
    }

    internal static List<Reforged2Rules.HonorSpec> CheckHonors(int winStreak)
    {
        lock (Sync)
            return EvaluateHonorsLocked(winStreak);
    }

    static List<Reforged2Rules.HonorSpec> EvaluateHonorsLocked(int winStreak = 0) =>
        Reforged2Rules.EvaluateNewHonors(Current, Math.Max(winStreak, Reforged2Runtime.WinStreak));

    internal static void RecordHallClear(string bossId, bool nightmare, int turns, bool noKo)
    {
        if (!Enabled && !SynergyEnabled) return;
        lock (Sync)
        {
            var game = Framework.Singleton<GAME_WORK>.instance;
            if (game?.game_data != null) boundCampaignOwner = game.game_data.Pointer;
            ArmyProgressionRules.RecordHallClear(Current, bossId, nightmare, turns, noKo);
            Current.Commendations = Math.Min(999, Current.Commendations + (nightmare ? 2 : 1));
        }
    }

    internal static void AwardCommendations(int count)
    {
        if (count <= 0) return;
        lock (Sync)
        {
            var game = Framework.Singleton<GAME_WORK>.instance;
            if (game?.game_data != null) boundCampaignOwner = game.game_data.Pointer;
            Current.Commendations = Math.Min(999, Current.Commendations + count);
        }
    }

    internal static void SpendCommendations(int count)
    {
        if (count <= 0) return;
        lock (Sync)
        {
            Current.Commendations = Math.Max(0, Current.Commendations - count);
        }
    }

    internal static void RecordHuntCompletion()
    {
        lock (Sync)
        {
            var game = Framework.Singleton<GAME_WORK>.instance;
            if (game?.game_data != null) boundCampaignOwner = game.game_data.Pointer;
            Current.HuntsCompleted++;
            Current.Commendations = Math.Min(999, Current.Commendations + 1);
        }
    }

    internal static string HallMedalFor(string bossId)
    {
        lock (Sync)
        {
            if (Current.HallRecords == null || !Current.HallRecords.TryGetValue(bossId, out var rec)) return "";
            return ArmyProgressionRules.HallMedalTag(rec);
        }
    }
}
