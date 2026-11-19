using HarmonyLib;
using UnityEngine;
namespace NightmareRebalance;
internal static class HallNightmareRuntime
{
    internal static bool CanSelect(int index)=>index>=0&&index<HallTrialCatalogue.Entries.Length&&
        Plugin.Options.EnableNightmareEchoes.Value&&HallAvailability.CanSelect(index)&&
        HallNightmareRules.Find(HallTrialCatalogue.Entries[index].Id)!=null;
    internal static MONSTER_DATA Copy(BATTLE_DATA original,HallTrialDefinition definition,int monsterIndex = -1)
    {
        var audit=HallTrialSources.Get(definition.TableId);
        HallEchoesProbe.ValidateSource(original,audit);
        var source=original.monster_data[monsterIndex<0?audit.Monsters[0].Index:monsterIndex];
        var profile=HallNightmareRules.Find(definition.Id)??throw new InvalidOperationException("No Nightmare profile.");
        var signature=CombatReforgedRuntime.Signature(source);
        var copy=source.ShallowCopy();
        if(copy.Pointer==source.Pointer)throw new InvalidOperationException("Nightmare requires an owned native clone.");
        copy.hp=(ushort)HallNightmareRules.Hp(source.hp,profile);
        if(!signature.SequenceEqual(CombatReforgedRuntime.Signature(source)))throw new InvalidOperationException("Original boss template changed.");
        Plugin.Logger.LogInfo($"HallNightmare: owned {definition.Id}; HP {source.hp}->{copy.hp}; damage relative to configured Original={profile.Damage}; source unchanged=True.");
        return copy;
    }
    internal static CombatUnitPlan? Plan(BATTLE_WORK w,PM_DATA actor)=>
        HallFirstContact.NightmareId(w,actor) is {} id?new(actor.monster_data.id,"Adaptive",true,null,HallNightmareRules.Find(id)!.Damage):null;
    internal static int Target(IReadOnlyList<CombatTarget> legal,int original,int round)
    {
        // Only candidates returned by the existing native sampler; avoid repeated wounded focus.
        if(legal.Count==0)return original;
        var ordered=legal.OrderBy(t=>t.Focus).ThenByDescending(t=>t.Hp*100/Math.Max(1,t.MaxHp)).ThenBy(t=>t.Slot).ToArray();
        return ordered[round%Math.Min(2,ordered.Length)].Slot;
    }
    static int damageLogged;
    internal static void Reaction(PM_DATA actor,ACTION_DATA action,int target,ref short damage)
    {
        var w=Framework.Singleton<BATTLE_WORK>.instance;
        if(damage<=0||action==null||action.chara_no!=actor.chara_no||
            HallFirstContact.NightmareId(w,actor) is not {} id||w?.pm_data is not {} units||
            target<0||target>=units.Length||units[target] is not {IsPlayer:true,IsDead:false} player||player.hp<=0)return;
        int before=damage;
        double factor=HallNightmareRules.Find(id)!.Damage;
        damage=(short)HallNightmareRules.DamageAmount(before,factor);
        if(damageLogged++<48)Plugin.Logger.LogInfo($"HallNightmare: {id} native reaction damage={before}->{damage}; Nightmare factor={factor}.");
    }
    static int logged;
    internal static void Selected(BATTLE_WORK w,PM_DATA actor,int before,int after)
    {
        if(HallFirstContact.NightmareId(w,actor) is {} id&&logged++<64)
            Plugin.Logger.LogInfo($"HallNightmare: {id} native round={w.battle_kaisuu} legal mode={before}->{after}.");
    }
}

// Same audited reaction boundary already used by diagnostics, bonds and hunt protection.
// No calculation-hook increment remains: normal and special attacks meet here once.
[HarmonyPatch(typeof(PM_DATA),nameof(PM_DATA.DamageSetReActionData))]
[HallTrialPatch]
internal static class HallNightmareReaction
{
    [ThreadStatic]static int depth;
    [HarmonyPrefix]static void Prefix(PM_DATA __instance,ACTION_DATA action_data,int mokuhyo,ref short damage)
    {
        bool outer=depth++==0;
        if(!outer)return;
        try{HallNightmareRuntime.Reaction(__instance,action_data,mokuhyo,ref damage);}
        catch(Exception e){Plugin.Logger.LogWarning("Nightmare reaction kept native damage: "+e.Message);}
    }
    [HarmonyFinalizer]static Exception? Finalizer(Exception? __exception){depth=Math.Max(0,depth-1);return __exception;}
}
