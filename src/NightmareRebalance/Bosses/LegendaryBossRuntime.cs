using HarmonyLib;

namespace NightmareRebalance;

internal static class LegendaryBossRuntime
{
    internal static readonly BossSourceAudit[] Audit =
        ContentRules.Read<BossSourceAudit[]>(ContentRuntime.Resource("legendary-boss-audit.json"));
    static LegendaryBossOptions? options;
    static Binding? binding;
    [ThreadStatic] internal static Dispatch? Current;
    internal static int Changes { get; private set; }

    internal sealed record Binding(BattleState State, BATTLE_WORK Work, BattleMonsterCharacter Actor,
        BATTLE_DATA Table, MONSTER_PARTY_DATA Party, FormationAudit Source, int Thread,
        BattleOverlayDelegate?[] Callbacks);
    internal sealed class Dispatch(Binding owner, int entryCommand, bool firstAction)
    {
        internal readonly Binding Owner = owner;
        internal readonly int EntryCommand = entryCommand;
        internal readonly bool FirstAction = firstAction;
        internal bool SawSelector;
    }

    internal static void Initialize()
    {
        try { options = ContentRules.Read<LegendaryBossOptions>(OverhaulRuntime.Load("legendary-bosses.json")); }
        catch (Exception ex) { Plugin.Logger.LogWarning("Legendary boss tactics disabled: " + ex.Message); }
    }

    internal static void Clear()
    {
        binding = null;
        Changes = 0;
    }

    static BattleOverlayDelegate?[] Callbacks(BATTLE_WORK work) => new[]
    {
        work.monster_single_func, work.monster_ps_func, work.monster_pe_func,
        work.monster_be_func, work.monster_cm_func, work.monster_bm_func
    };

    static bool SameMethod(BattleOverlayDelegate? first, BattleOverlayDelegate? second)
    {
        if (first?.Pointer == second?.Pointer) return true;
        return first?.Method is {} a && second?.Method is {} b &&
            a.Pointer == b.Pointer && first.Target?.Pointer == second.Target?.Pointer;
    }

    internal static bool Ready(BattleState state, BATTLE_WORK work, bool compatible,
        FormationAudit? soloFixture = null)
    {
        Clear();
        try
        {
            if (!compatible || !CombatReforgedRuntime.AiActive(work) ||
                !CombatReforgedRuntime.Config.EnableBossEncounters ||
                !DifficultyProvenance.AllowedForFeature(out _, out _)) return false;
            if (work.battle_data is not {} table || work.monster_party_data is not {} party)
                return false;
            // Solo Luca has no live opt-in until a legitimate encounter witnesses this scope.
            var source = CombatReforgedRuntime.Probe && soloFixture?.TableId == "boss_luc3"
                ? soloFixture : Audit.FirstOrDefault(x => x.Data.TableId == table.id &&
                    options?.Enabled(x.Data.TableId) == true)?.Data;
            if (source == null || !BossTacticsRuntime.Source(work, source)) return false;
            var callbacks = Callbacks(work);
            if (callbacks.Where((callback, i) => (callback != null) != (source.Hooks[i] != 0) || !SameMethod(callback,
                source.Hooks[i] == 0 ? null : area_func.GetBattleOverlayFunction(source.Hooks[i]))).Any())
                return false;
            int index = LegendaryBossRules.ActorIndex(source.TableId);
            var actor = work.pm_data.FirstOrDefault(u => u != null && !u.IsPlayer &&
                u.chara_id == index)?.TryCast<BattleMonsterCharacter>();
            if (actor == null || !OwnsActor(state, work, actor) ||
                CombatReforgedRuntime.Plans.ContainsKey(actor.Pointer) || BossTacticsRuntime.Bound(actor)) return false;
            binding = new(state, work, actor, table, party, source, Environment.CurrentManagedThreadId, callbacks);
            BossTacticsRuntime.Record(new { Kind = "LegendaryBossAdmitted", source.TableId });
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Debug("Legendary boss admission retained native AI: " + ex.Message);
            return false;
        }
    }

    static bool OwnsActor(BattleState state, BATTLE_WORK work, BattleMonsterCharacter actor) =>
        ReferenceEquals(Combat.State, state) && actor.manager?.Pointer == state.Manager &&
        actor.manager?.battle_work?.Pointer == work.Pointer &&
        CombatReforgedRuntime.ActiveWork?.Pointer == work.Pointer &&
        (CombatReforgedRuntime.Probe || BattleManager.Instance?.Pointer == state.Manager) &&
        actor.chara_no >= 0 && work.pm_data != null && actor.chara_no < work.pm_data.Length &&
        work.pm_data[actor.chara_no]?.Pointer == actor.Pointer;

    static bool SameCallbacks(Binding owner)
    {
        var current = Callbacks(owner.Work);
        // Opening scripts clear their delegate when done. While a transformation
        // runs, its phase-end delegate points at intermediate functions. The
        // native final step reinstalls the original phase-end method.
        if (current[0] != null && !SameMethod(current[0], owner.Callbacks[0])) return false;
        for (int i = 1; i < 5; i++)
            if (!SameMethod(current[i], owner.Callbacks[i])) return false;
        if (SameMethod(current[5], owner.Callbacks[5])) return true;
        // These two main scripts replace their initial delegate with the same
        // class's steady-state function after the opening sequence.
        if (owner.Source.TableId is "boss_ki1" or "boss_imo2")
        {
            var initial = owner.Callbacks[5]?.Method;
            return current[5]?.Method is { Name: "boss_func03", IsStatic: true } live &&
                initial != null && live.DeclaringType?.Pointer == initial.DeclaringType?.Pointer &&
                current[5]?.Target == null;
        }
        return false;
    }

    internal static Dispatch? Begin(BattleMonsterCharacter actor)
    {
        var owner = binding;
        if (owner == null || Current != null || owner.Actor.Pointer != actor.Pointer ||
            owner.Thread != Environment.CurrentManagedThreadId) return null;
        try
        {
            if (!ReferenceEquals(Combat.State, owner.State) || !OwnsActor(owner.State, owner.Work, actor))
                return null;
            // Capture before native code increments the action counter or handles command 11.
            return new(owner, actor.c_koudou, actor.action_kaisu == 0);
        }
        catch (Exception ex)
        {
            Plugin.Debug("Legendary boss dispatcher retained native AI: " + ex.Message);
            return null;
        }
    }

    internal static void Select(BattleMonsterCharacter actor, ref int selected)
    {
        var dispatch = Current;
        if (dispatch == null || dispatch.SawSelector) return;
        dispatch.SawSelector = true;
        var owner = dispatch.Owner;
        var work = owner.Work;
        if (!ReferenceEquals(binding, owner) || owner.Actor.Pointer != actor.Pointer ||
            !LegendaryBossRules.Discretionary(dispatch.EntryCommand) || selected is < 0 or > 3) return;
        try
        {
            if (!CombatReforgedRuntime.AiActive(work) || !OwnsActor(owner.State, work, actor) ||
                !LegendaryBossRules.Discretionary(actor.c_koudou) || actor.IsDead ||
                work.battle_data?.Pointer != owner.Table.Pointer ||
                work.monster_party_data?.Pointer != owner.Party.Pointer ||
                owner.Table.id != owner.Source.TableId || owner.Party.id != owner.Source.PartyId ||
                work.monster_party_index != owner.Source.Indices[0] + 1 ||
                work.battle_data.monster_party[work.monster_party_index - 1]?.Pointer != work.monster_party_data.Pointer ||
                !CombatEvolutionRuntime.Hooks(work.monster_party_data.data).SequenceEqual(owner.Source.Hooks) ||
                work.monster_party_data.data.be_data.command_num != 0 ||
                work.monster_party_data.data.be_data.com_data.Length != 0 ||
                !SameCallbacks(owner)) return;

            int phase = actor.chara_id;
            if (!LegendaryBossRules.AcceptsPhase(owner.Source.TableId, phase)) return;
            var data = actor.monster_data;
            var source = owner.Source.Monsters.Single(m => m.Index == phase);
            if (data == null || data.id != source.Id || work.monster_data[phase]?.Pointer != data.Pointer ||
                !DifficultyProvenance.Matches(work, data, source.Signature)) return;

            var targets = CombatReforgedRuntime.Targets(work, true,
                owner.Source.TableId == "boss_imo2" && phase == 2 ? 4 : 0);
            int mode = LegendaryBossRules.Preferred(owner.Source.TableId, phase, work.battle_kaisuu,
                dispatch.FirstAction, targets, targets.Count(t => t.StatusUseful));
            if (mode < 0 || mode == selected || work.b_magic_status == null ||
                work.pm_attack_data[actor.chara_no]?.curent_use_cnt?.Length != 4) return;

            var move = data.attack_data[mode];
            if (move == null || move.attack_prob == 0 || move.attack <= 0 ||
                move.attack_type is not (1 or 2) || !actor.CheckMonsterAttackCnt(mode) ||
                !actor.AttackModeKyoriCheck(mode) ||
                ((move.attack_zokusei & 0x10) != 0 ? !actor.AttackCanCheck() : !actor.MagicCanCheck()) ||
                ((move.attack_zokusei & 0x20) != 0 && work.b_magic_status.water_silent_cnt > 0)) return;

            BossTacticsRuntime.Record(new { Kind = "LegendaryBossSelectorChanged", owner.Source.TableId,
                Actor = data.id, Round = work.battle_kaisuu, Before = selected, After = mode });
            selected = mode;
            Changes++;
        }
        catch (Exception ex) { Plugin.Debug("Legendary boss selector retained native AI: " + ex.Message); }
    }
}

[HarmonyPatch(typeof(BattleMonsterCharacter), nameof(BattleMonsterCharacter.monster_attack_sub))]
internal static class LegendaryBossDispatchPatch
{
    [ThreadStatic] static int depth;

    [HarmonyPrefix]
    internal static void Prefix(BattleMonsterCharacter __instance, out LegendaryBossRuntime.Dispatch? __state)
    {
        __state = LegendaryBossRuntime.Current;
        LegendaryBossRuntime.Current = depth++ == 0 ? LegendaryBossRuntime.Begin(__instance) : null;
    }

    [HarmonyFinalizer]
    internal static Exception? Finalizer(LegendaryBossRuntime.Dispatch? __state, Exception? __exception)
    {
        LegendaryBossRuntime.Current = __state;
        depth--;
        return __exception;
    }
}
