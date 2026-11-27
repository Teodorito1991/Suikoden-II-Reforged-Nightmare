using HarmonyLib;

namespace NightmareRebalance;

// Only the three opt-in legacy major policies need this additional command fence.
// No command, queue, callback, counter, stat or outcome is written here.
internal sealed record StoryBossSelectionScope(BattleState State, BossTacticsRuntime.Binding Binding,
    BattleMonsterCharacter Actor, int EntryCommand)
{
    [ThreadStatic] internal static StoryBossSelectionScope? Current;

    internal static bool Allows(PM_DATA actor)
    {
        try
        {
        if(!BossTacticsRuntime.Bindings.TryGetValue(actor.Pointer,out var binding) ||
            !BossTacticsRules.Major(binding.Pattern.TableId))return true;
        var owner=Current;
        return owner!=null && owner.EntryCommand==0 && actor.c_koudou==0 &&
            ReferenceEquals(owner.Binding,binding) && ReferenceEquals(Combat.State,owner.State) &&
            owner.Actor.Pointer==actor.Pointer && Owns(owner.State,binding,owner.Actor) && Callbacks(binding);
        }
        catch(Exception ex){Plugin.Debug("Story boss command check retained native choice: "+ex.Message);return false;}
    }

    internal static StoryBossSelectionScope? Begin(BattleMonsterCharacter actor)
    {
        try
        {
            if(Combat.State is not {} state || !BossTacticsRuntime.Bound(actor) ||
                !BossTacticsRuntime.Bindings.TryGetValue(actor.Pointer,out var binding) ||
                !BossTacticsRules.Major(binding.Pattern.TableId) || !Owns(state,binding,actor) || !Callbacks(binding))return null;
            return new(state,binding,actor,actor.c_koudou);
        }
        catch(Exception ex){Plugin.Debug("Story boss command scope retained native choice: "+ex.Message);return null;}
    }

    static bool Owns(BattleState state,BossTacticsRuntime.Binding binding,BattleMonsterCharacter actor) =>
        state.Ready && state.Work==binding.Work.Pointer && state.Work==CombatReforgedRuntime.ActiveWork?.Pointer &&
        actor.manager?.Pointer==state.Manager && actor.manager?.battle_work?.Pointer==state.Work &&
        (CombatReforgedRuntime.Probe || BattleManager.Instance?.Pointer==state.Manager) &&
        actor.chara_no>=0 && actor.chara_no<binding.Work.pm_data.Length &&
        binding.Work.pm_data[actor.chara_no]?.Pointer==actor.Pointer;

    static bool Callbacks(BossTacticsRuntime.Binding binding)
    {
        var work=binding.Work;
        var source=BossTacticsRuntime.Audit.Single(a=>a.TableId==binding.Pattern.TableId);
        BattleOverlayDelegate?[] live={work.monster_single_func,work.monster_ps_func,work.monster_pe_func,
            work.monster_be_func,work.monster_cm_func,work.monster_bm_func};
        for(int i=0;i<live.Length;i++)
        {
            if(i==0 && live[i]==null)continue; // Native opening may finish and clear itself.
            var expected=source.Hooks[i]==0?null:area_func.GetBattleOverlayFunction(source.Hooks[i]);
            if(live[i]?.Pointer==expected?.Pointer)continue;
            if(live[i]?.Method is not {} actual || expected?.Method is not {} original ||
                actual.Pointer!=original.Pointer || live[i]?.Target?.Pointer!=expected.Target?.Pointer)return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.monster_attack_sub))]
internal static class StoryBossDispatchGuard
{
    [ThreadStatic] static int depth;
    [HarmonyPrefix]
    internal static void Prefix(BattleMonsterCharacter __instance,out StoryBossSelectionScope? __state)
    {
        __state=StoryBossSelectionScope.Current;
        StoryBossSelectionScope.Current=depth++==0?StoryBossSelectionScope.Begin(__instance):null;
    }
    [HarmonyFinalizer]
    internal static Exception? Finalizer(StoryBossSelectionScope? __state,Exception? __exception)
    {
        StoryBossSelectionScope.Current=__state;depth=Math.Max(0,depth-1);return __exception;
    }
}
