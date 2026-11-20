using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
namespace NightmareRebalance;
internal static class HallLucaHpProbe
{
    static T Alloc<T>()where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
    internal static void Inspect(BATTLE_DATA table)
    {
        if(!ProductionAssetAudit.Requested||!Environment.GetCommandLineArgs().Contains("--reforged-private-luca-hp-view"))return;
        if(UnityEngine.Application.isBatchMode||!HallFirstContact.EnabledAtStartup)throw new InvalidOperationException("Native scoped getter audit needs the normal Hall patch set.");
        var before=Framework.Singleton<GAME_WORK>.instance;var oldWork=Framework.Singleton<BATTLE_WORK>.instance;
        var rng=UnityEngine.Random.state;
        var g=Alloc<GAME_WORK>();g.game_data=new GAME_DATA();g.game_data.luka_hp=1234;
        g.sys_work=new SYS_WORK();g.party_data=new PARTY_DATA();g.chara_data=new CHARA_DATA();
        g.event_flag=new Il2CppStructArray<byte>(256);g.chara_flag=new Il2CppStructArray<byte>(256);
        g.town_flag=new Il2CppStructArray<byte>(256);g.scpoi_flag=new Il2CppStructArray<byte>(256);g.t_box_flag=new Il2CppStructArray<byte>(256);
        var w=Alloc<BATTLE_WORK>();w.battle_data=table;
        try
        {
            Framework.Singleton<GAME_WORK>.instance=g;Framework.Singleton<BATTLE_WORK>.instance=w;
            string original=LitJson.JsonMapper.ToJson(g.game_data);
            using(var scope=HallLucaHpView.Audit(g,w,4321))
            {
                var native=OldSrcBase.game_work;
                if(native.Pointer!=scope.View.Pointer||native.game_data.luka_hp!=4321||g.game_data.luka_hp!=1234||
                   Framework.Singleton<GAME_WORK>.instance.Pointer!=g.Pointer)throw new InvalidOperationException("Native HP getter escaped its private read view.");
                scope.View.game_data.luka_hp=2100;
                if(OldSrcBase.game_work.game_data.luka_hp!=2100)throw new InvalidOperationException("Private carry read did not update.");
            }
            if(OldSrcBase.game_work.Pointer!=g.Pointer||original!=LitJson.JsonMapper.ToJson(g.game_data)||HallLucaHpView.Current!=null)
                throw new InvalidOperationException("Luca HP getter cleanup changed original data.");
            ProductionAssetAudit.Rows.Add(new{Kind="LucaPrivateHpGetter",table.id,NativeGetterCalls=3,OriginalHp=1234,PrivateHp=2100,
                CampaignObjectNeverReplaced=true,OriginalDataUnchanged=true,ScopeReleased=true,NativeOpeningExecuted=false,ActualBattle=false});
        }
        finally{HallLucaHpView.Current?.Dispose();Framework.Singleton<BATTLE_WORK>.instance=oldWork;Framework.Singleton<GAME_WORK>.instance=before;UnityEngine.Random.state=rng;}
    }
}
