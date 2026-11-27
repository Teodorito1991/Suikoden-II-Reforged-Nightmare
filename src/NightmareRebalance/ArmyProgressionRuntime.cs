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
    internal static string LastAwardNotice { get; private set; } = "";
    static string LedgerPath => Path.Combine(Paths.BepInExRootPath, "NightmareRebalance", "army-ledger.json");

    internal static bool Enabled => Plugin.Options?.EnableArmyProgression.Value == true;
    internal static bool DirectivesEnabled => Plugin.Options?.EnableDrillDirectives.Value == true;
    internal static bool SynergyEnabled => Plugin.Options?.EnableCrossTheaterSynergy.Value == true;

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

    internal static CommanderProfile? Enhance(WAR_CHARA_TYPE original, CommanderProfile? existingProfile, bool isEnemy)
    {
        bool training = TrainingWar.Active;
        bool applyDirective = training && DirectivesEnabled && ActiveDirective != DrillDirective.None;
        bool applyProg = Enabled && !isEnemy && Current.Commanders.TryGetValue(original.name, out var progress) && ArmyProgressionRules.RankForXp(progress.Xp) > ArmyRank.Recruit;
        if (!applyDirective && !applyProg) return existingProfile;

        var profile = existingProfile ?? BuildBaseProfile(original);
        if (applyProg && Current.Commanders.TryGetValue(original.name, out var prog))
            profile = ArmyProgressionRules.ApplyProgression(profile, prog);
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

    internal static DrillDirective CycleDirective()
    {
        ActiveDirective = (DrillDirective)(((int)ActiveDirective + 1) % 4);
        return ActiveDirective;
    }
}
