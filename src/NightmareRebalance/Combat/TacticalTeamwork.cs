namespace NightmareRebalance;

internal enum TeamworkStyle { Pack, Disruption, Escort, Mixed }

internal static class TacticalTeamwork
{
    // Keys refer to existing exact-source formation audits, including the native slot layouts.
    internal static readonly IReadOnlyDictionary<string, TeamworkStyle> Formations = new Dictionary<string, TeamworkStyle>
    {
        ["wa_00_02"] = TeamworkStyle.Pack,
        ["wa_00_04"] = TeamworkStyle.Disruption,
        ["vb07_00_06"] = TeamworkStyle.Disruption,
        ["vc12_00_02"] = TeamworkStyle.Disruption,
        ["vb21_00_03"] = TeamworkStyle.Mixed,
        ["vd06_00_02"] = TeamworkStyle.Escort,
        ["vd06_00_04"] = TeamworkStyle.Escort,
        ["vh04_00_05"] = TeamworkStyle.Disruption,
        ["ve03_00_05"] = TeamworkStyle.Disruption,
        ["vf06_00_04"] = TeamworkStyle.Pack,
        ["ve08_00_03"] = TeamworkStyle.Disruption,
        ["ve08_00_06"] = TeamworkStyle.Escort,
    };

    internal static readonly IReadOnlyDictionary<string, TeamworkStyle> Hunts = new Dictionary<string, TeamworkStyle>
    {
        ["bramble-road"] = TeamworkStyle.Disruption,
        ["matilda-choir"] = TeamworkStyle.Disruption,
        ["twin-head-pack"] = TeamworkStyle.Mixed,
        ["undying-watch"] = TeamworkStyle.Mixed,
        ["tenzan-trial"] = TeamworkStyle.Escort
    };

    internal static int ArcaneGuard(int damage) => damage - damage / 10;

    internal static bool CanFinish(CombatAction action, CombatTarget target, int magic) =>
        !action.Multi && action.Status == 0 &&
        CombatReforgedRules.BaseDamage(action.Power, magic, target.Defense, target.MagicDefense, action.Physical, 1)
            >= target.Hp + Math.Max(8, target.MaxHp / 10);

    internal static int ActionBonus(TeamworkStyle style, CombatAction action, IReadOnlyList<CombatTarget> targets,
        int magic, bool hasCaster, bool allyCritical, bool reservedStatus)
    {
        if (action.Type is not (1 or 2) || targets.Count == 0) return 0;
        bool finish = targets.Any(t => t.Focus == 0 && !t.Guarding && CanFinish(action, t, magic));
        return style switch
        {
            TeamworkStyle.Pack => finish ? 24 : action.Multi && allyCritical ? -12 : 0,
            TeamworkStyle.Disruption when action.Status != 0 =>
                targets.All(t => !t.StatusUseful) ? -32 : reservedStatus ? -30 :
                action.Multi && targets.Count(t => t.StatusUseful) * 2 < targets.Count ? -25 : 10,
            TeamworkStyle.Escort => hasCaster && action.Physical && !action.Multi ? 18 : finish ? 20 : 0,
            TeamworkStyle.Mixed => MixedBonus(action, targets, magic),
            _ => 0
        };
    }

    static int MixedBonus(CombatAction action, IReadOnlyList<CombatTarget> targets, int magic)
    {
        int matchup = targets.Max(t => action.Physical ? t.MagicDefense - t.Defense : t.Defense - t.MagicDefense);
        return Math.Clamp(matchup / 3, -16, 24)
            + (targets.Any(t => !t.Guarding && CanFinish(action, t, magic)) ? 10 : 0);
    }

    internal static int TargetBonus(TeamworkStyle style, CombatAction action, CombatTarget target,
        int magic, bool hasCaster, int alliedSelections)
    {
        int finish = !target.Guarding && CanFinish(action, target, magic) ? 32 : 0;
        int bonus = style switch
        {
            TeamworkStyle.Pack => finish - alliedSelections * 35,
            TeamworkStyle.Disruption => action.Status != 0
                ? (target.StatusUseful ? 30 : -65) - alliedSelections * 30
                : (action.Physical && target.Asleep && finish == 0 ? -50 : finish) - alliedSelections * 20,
            TeamworkStyle.Escort => (hasCaster && action.Physical ? Math.Min(40, target.Magic / 2) : finish)
                - alliedSelections * 30,
            TeamworkStyle.Mixed => Math.Clamp((action.Physical ? target.MagicDefense - target.Defense
                : target.Defense - target.MagicDefense) / 2, -30, 30) + finish - alliedSelections * 25,
            _ => 0
        };
        return bonus - (target.Guarding ? 12 : 0);
    }
}

internal sealed class TeamworkBinding
{
    internal readonly string Id;
    internal readonly TeamworkStyle Style;
    internal readonly IntPtr Table, Party, Source;
    internal readonly int Index;
    internal readonly Dictionary<int, IntPtr> Roster;
    readonly Dictionary<int, (int Status, bool Area)> actions = new();
    readonly Dictionary<int, int> targets = new();
    int phase = int.MinValue;

    internal TeamworkBinding(string id, TeamworkStyle style, BATTLE_WORK work, MONSTER_PARTY_DATA source,
        Dictionary<int, IntPtr> roster)
    {
        Id = id;
        Style = style;
        Table = work.battle_data.Pointer;
        Party = work.monster_party_data.Pointer;
        Source = source.Pointer;
        Index = work.monster_party_index;
        Roster = roster;
    }

    internal void ResetChoices() { actions.Clear(); targets.Clear(); phase = int.MinValue; }

    internal void Phase(int current)
    {
        if (phase == current) return;
        ResetChoices();
        phase = current;
    }

    internal bool AreaReserved(int actor, int status) =>
        actions.Any(entry => entry.Key != actor && entry.Value == (status, true));

    internal int AlliedSelections(int actor, int target) =>
        Math.Min(2, targets.Count(entry => entry.Key != actor && entry.Value == target));

    // Selection intent only. Actual status and guard come from live PM_DATA.
    internal void Selected(int actor, CombatAction action) => actions[actor] = (action.Status, action.Multi);
    internal void Targeted(int actor, int target) => targets[actor] = target;
}

internal static class TeamworkRuntime
{
    static bool Unscripted(BATTLE_WORK w) => w.monster_ps_func == null && w.monster_pe_func == null &&
        w.monster_cm_func == null && w.monster_bm_func == null && w.monster_be_func == null;

    internal static void Ready(BattleState state, BATTLE_WORK w)
    {
        try
        {
            if (!state.Allowed || !Unscripted(w) || w.pm_data == null ||
                !DifficultyProvenance.AllowedForFeature(out _, out _) ||
                w.monster_party_data is not {} party || party.Pointer != CombatEvolutionRuntime.LastCopy?.Pointer ||
                w.battle_data is not {} table || table.monster_party == null ||
                w.monster_party_index < 1 || w.monster_party_index > table.monster_party.Length ||
                table.monster_party[w.monster_party_index - 1] is not {} source) return;

            FormationAudit? audit;
            TeamworkStyle style;
            string id;
            if (HuntRuntime.Active is {} hunt && HuntRuntime.Work == w.Pointer)
            {
                if (!HuntRuntime.ActiveRoster || !TacticalTeamwork.Hunts.TryGetValue(hunt.Id, out style)) return;
                audit = hunt.Audit;
                id = hunt.Id;
            }
            else
            {
                id = party.id;
                if (!TacticalTeamwork.Formations.TryGetValue(id, out style)) return;
                audit = CombatEvolutionRuntime.Audit.Formations.Concat(CombatMasteryRuntime.Audit.Formations)
                    .Concat(CombatRealityRuntime.Audit.Formations).Concat(RpgEvolutionRuntime.Audit.Formations)
                    .SingleOrDefault(a => a.TableId == table.id && a.PartyId == id);
            }

            if (audit == null || audit.TableId != table.id || audit.PartyId != party.id || source.id != party.id ||
                !audit.Indices.Contains(w.monster_party_index - 1) || party.data?.be_data?.command_num != 0 ||
                !CombatEvolutionRuntime.Hooks(party.data).SequenceEqual(audit.Hooks) ||
                !audit.NewCodes.SequenceEqual(party.monster_code.Select(x => (int)x)) ||
                !audit.OldCodes.SequenceEqual(source.monster_code.Select(x => (int)x))) return;

            var roster = new Dictionary<int, IntPtr>();
            var codes = new List<int>();
            for (int slot = 0; slot < w.pm_data.Length; slot++)
            {
                if (w.pm_data[slot] is not {} unit || unit.IsPlayer) continue;
                if (slot != unit.chara_no || !CombatReforgedRuntime.Plans.TryGetValue(unit.Pointer, out var plan) ||
                    plan.Boss || unit.monster_data?.id != plan.Id) return;
                roster.Add(slot, unit.Pointer);
                codes.Add(unit.chara_id);
            }

            if (roster.Count != w.monster_kazu ||
                !codes.OrderBy(x => x).SequenceEqual(audit.NewCodes.Where(x => x > 0).OrderBy(x => x))) return;
            state.Teamwork = new TeamworkBinding(id, style, w, source, roster);
            state.TeamworkProfile = id;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning("Tactical teamwork retained previous combat: " + e.Message);
        }
    }

    internal static TeamworkBinding? Bound(BATTLE_WORK w)
    {
        if (Combat.State is not { Ready: true, Allowed: true, Teamwork: {} team } s || s.Work != w.Pointer) return null;
        var units = w.pm_data;
        if (!Unscripted(w) || units == null || team.Table != w.battle_data?.Pointer ||
            team.Party != w.monster_party_data?.Pointer || team.Index != w.monster_party_index ||
            w.battle_data?.monster_party?[team.Index - 1]?.Pointer != team.Source ||
            units.Count(u => u != null && !u.IsPlayer) != team.Roster.Count ||
            team.Roster.Any(entry => entry.Key >= units.Length || units[entry.Key]?.Pointer != entry.Value))
        {
            s.Teamwork = null;
            return null;
        }
        team.Phase(w.battle_phase_no);
        return team;
    }

    internal static bool LivingCaster(BATTLE_WORK w, TeamworkBinding team, PM_DATA actor) =>
        team.Style == TeamworkStyle.Escort && w.b_magic_status is {} magic && w.pm_attack_data is {} attackWork &&
        team.Roster.Keys.Any(slot => slot < attackWork.Length &&
            w.pm_data[slot] is {} ally && ally.Pointer != actor.Pointer && ally.hp > 0 && !ally.IsDead &&
            ally.MagicCanCheck() && ally.monster_data?.attack_data is {} attacks &&
            Enumerable.Range(0, attacks.Length).Any(i => attacks[i] is { attack_type: 2, attack_prob: > 0, attack: > 0 } a &&
                (a.attack_zokusei & 0x10) == 0 && (magic.water_silent_cnt == 0 || (a.attack_zokusei & 0x20) == 0) &&
                attackWork[slot]?.curent_use_cnt is {} uses && i < uses.Length && uses[i] != 0));
}
