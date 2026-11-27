using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Il2CppInterop.Runtime;
using System.Runtime.CompilerServices;
namespace NightmareRebalance;
internal static class HallContentProbe
{
    static T Allocate<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;
    }
    internal static void Inspect(BATTLE_DATA table)
    {
        var definition=HallTrialCatalogue.AllSources.Single(x=>x.TableId==table.id);
        var audit=HallTrialSources.Get(table.id);
        var source=HallEchoesProbe.ValidateSource(table,audit);
        var boss=Array.IndexOf(BattleLoader.btl_bos_data.ToArray(),table.id);
        if(boss<0 || BattleLoader.GetBossResID(boss)!=definition.PackId ||
           table.battle_field_data?.hd_field_id!=definition.SceneId ||
           audit.Monsters.Max(m=>(int)table.monster_data[m.Index].hp)!=definition.Hp)
            throw new InvalidOperationException("Hall definition differs from original resources: "+definition.Id);
        var owned=HallTrialSetup.Copy(source,audit,definition);
        var clones=new List<MONSTER_DATA>();
        try
        {
            if(owned.Pointer==source.Pointer || owned.data.Pointer==source.data.Pointer ||
               owned.party_flg!=0 || owned.item_no!=0 || owned.data.battle_end_func_index!=0 ||
               owned.data.battle_start_func_index!=audit.Hooks[0] ||
               owned.data.phase_start_func_index!=(definition.RetainPhaseCallbacks?audit.Hooks[1]:0) ||
               owned.data.phase_end_func_index!=(definition.RetainPhaseCallbacks?audit.Hooks[2]:0) ||
               owned.data.command_func_index!=(definition.RetainCommand?audit.Hooks[4]:0) ||
               owned.data.battle_main_func_index!=(definition.RetainCombatMain?audit.Hooks[5]:0) ||
               !owned.monster_code.Select(x=>(int)x).SequenceEqual(audit.OldCodes))
                throw new InvalidOperationException("Hall owned callback/formation copy failed.");
            foreach(var monster in audit.Monsters)
            {
                var copy=HallNightmareRuntime.Copy(table,definition,monster.Index);clones.Add(copy);
                if(copy.Pointer==table.monster_data[monster.Index].Pointer ||
                   copy.hp!=HallNightmareRules.Hp(monster.Signature[0],HallNightmareRules.Find(definition.Id)!))
                    throw new InvalidOperationException("Hall group Nightmare copy failed.");
            }
            var previousGame=Framework.Singleton<GAME_WORK>.instance;
            var previousWork=Framework.Singleton<BATTLE_WORK>.instance;
            var rng=UnityEngine.Random.state;
            var local=ScriptableObject.CreateInstance<BATTLE_DATA>();
            BATTLE_WORK? work=null;
            try
            {
                var game=Allocate<GAME_WORK>();game.sys_work=Allocate<SYS_WORK>();
                game.game_data=Allocate<GAME_DATA>();game.game_data.difficulty=0;
                game.party_data=Allocate<PARTY_DATA>();
                game.sys_work.battle_no=-1;game.sys_work.yobi=new Il2CppStructArray<byte>(8);
                local.id="echo_trial_"+definition.Id;local.battle_field_data=table.battle_field_data;
                local.is_battle_field_data=table.is_battle_field_data;local.monster_party_num=1;
                local.monster_party=new Il2CppReferenceArray<MONSTER_PARTY_DATA>(new[]{owned});local.monster_data=table.monster_data;
                game.sys_work.battle_data=local;game.sys_work.yobi[0]=1;
                Framework.Singleton<GAME_WORK>.instance=game;
                work=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=work;
                var manager=Allocate<BattleManager>();manager.BattleDataSetup();
                if(work.monster_party_data?.Pointer!=owned.Pointer || work.monster_party_index!=1 ||
                   work.battle_data?.Pointer!=local.Pointer || game.sys_work.battle_no!=-1)
                    throw new InvalidOperationException("Original manager rejected Hall formation selection.");
            }
            finally
            {
                Framework.Singleton<BATTLE_WORK>.instance=previousWork;Framework.Singleton<GAME_WORK>.instance=previousGame;
                UnityEngine.Random.state=rng;
                if(work?.monster_data!=null)
                    foreach(var monster in work.monster_data.Where(m=>m!=null).GroupBy(m=>m.Pointer).Select(g=>g.First()))
                        if(!table.monster_data.Any(m=>m?.Pointer==monster.Pointer))UnityEngine.Object.Destroy(monster);
                UnityEngine.Object.Destroy(local);
            }
            HallEchoesProbe.ValidateSource(table,audit);
            ProductionAssetAudit.Rows.Add(new{Kind="HallContentPreparation",definition.Id,OriginalMonsters=audit.Monsters.Length,
                ActiveActors=owned.monster_kazu,OriginalSourceUnchanged=true,OwnedFormation=true,
                definition.RetainCombatMain,definition.RetainPhaseCallbacks,definition.RetainCommand,
                NativeEndingDetached=true,NativeBattleDataSetup=true,AllNightmareClones=true,CallbacksExecuted=false,BattleRendered=false});
        }
        finally{foreach(var copy in clones)UnityEngine.Object.Destroy(copy);UnityEngine.Object.Destroy(owned);}
    }
}
