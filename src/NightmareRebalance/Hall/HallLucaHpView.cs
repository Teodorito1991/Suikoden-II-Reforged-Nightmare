using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
namespace NightmareRebalance;

// A read view for the original HP getter. It never replaces the campaign singleton
// or its GAME_DATA. Other members are borrowed read dependencies, not owned trees.
internal sealed class HallLucaHpView : IDisposable
{
    [ThreadStatic]internal static HallLucaHpView? Current;
    internal readonly GAME_WORK Original,View;
    readonly BATTLE_WORK work;readonly int thread;readonly IntPtr dataOwner;readonly long epoch;
    readonly Func<bool> owns;
    HallLucaHpView(GAME_WORK game,BATTLE_WORK battle,int hp,Func<bool> owner)
    {
        if(Current!=null||hp is <0 or >32767||!owner())throw new InvalidOperationException("Unowned/nested Luca HP scope.");
        Original=game;work=battle;owns=owner;thread=Environment.CurrentManagedThreadId;dataOwner=game.game_data.Pointer;epoch=NativeWarBoundary.Epoch;
        View=Alloc<GAME_WORK>();
        View.game_data=LitJson.JsonMapper.ToObject(LitJson.JsonMapper.ToJson(game.game_data),Il2CppType.Of<GAME_DATA>()).Cast<GAME_DATA>();
        if(View.game_data.Pointer==game.game_data.Pointer)throw new InvalidOperationException("HP view aliases campaign data.");
        View.game_data.luka_hp=(short)hp;
        View.sys_work=game.sys_work;View.party_data=game.party_data;View.chara_data=game.chara_data;
        View.g_hd_mode=game.g_hd_mode;View.have_data=game.have_data;View.arms_data=game.arms_data;View.magi_data=game.magi_data;View.ovit_data=game.ovit_data;
        View.event_flag=new Il2CppStructArray<byte>(game.event_flag.ToArray());
        View.chara_flag=new Il2CppStructArray<byte>(game.chara_flag.ToArray());
        View.town_flag=new Il2CppStructArray<byte>(game.town_flag.ToArray());
        View.scpoi_flag=new Il2CppStructArray<byte>(game.scpoi_flag.ToArray());
        View.t_box_flag=new Il2CppStructArray<byte>(game.t_box_flag.ToArray());
        Current=this;
    }
    static T Alloc<T>()where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
    internal bool Valid=>Current==this&&Environment.CurrentManagedThreadId==thread&&owns()&&NativeWarBoundary.Epoch==epoch&&Original.game_data.Pointer==dataOwner&&
        Framework.Singleton<GAME_WORK>.instance?.Pointer==Original.Pointer&&Framework.Singleton<BATTLE_WORK>.instance?.Pointer==work.Pointer;
    internal static HallLucaHpView OpenForOwnedStage(BATTLE_WORK work,int carryHp)
    {
        var game=HallFirstContact.GameForOwnedLucaStage(work)??throw new InvalidOperationException("No admitted Luca stage owner.");
        return new(game,work,carryHp,()=>HallFirstContact.GameForOwnedLucaStage(work)?.Pointer==game.Pointer);
    }
    internal static HallLucaHpView Audit(GAME_WORK g,BATTLE_WORK w,int hp)
    {
        if(!ProductionAssetAudit.Requested||!Environment.GetCommandLineArgs().Contains("--reforged-private-luca-hp-view")||
           g.sys_work?.mcon!=null||UnityEngine.Application.isBatchMode)throw new InvalidOperationException("Private unloaded native getter audit only.");
        return new(g,w,hp,()=>g.sys_work?.mcon==null&&w.battle_data?.id is "boss_luc1" or "boss_luc2" or "boss_luc3");
    }
    public void Dispose(){if(Current!=this)throw new InvalidOperationException("Luca getter scope ownership lost.");Current=null;}
}

[HarmonyPatch(typeof(OldSrcBase),"get_game_work")]
[HallTrialPatch]
internal static class HallLucaScopedGetter
{
    static bool Prefix(ref GAME_WORK __result)
    {
        if(HallLucaHpView.Current is not {} scope)return true;
        if(!scope.Valid)throw new InvalidOperationException("Invalid Luca HP read owner.");
        __result=scope.View;return false;
    }
}
