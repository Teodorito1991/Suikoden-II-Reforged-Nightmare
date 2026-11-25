using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
namespace NightmareRebalance;
internal static class HallPartyEquipmentProbe
{
    static T Alloc<T>()where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
    internal static void Run()
    {
        var prior=Framework.Singleton<GAME_WORK>.instance;var rng=UnityEngine.Random.state;
        try
        {
            var g=Alloc<GAME_WORK>();g.party_data=new PARTY_DATA();g.party_data.party_cha_no[0]=5;
            g.party_data.party_item=new Il2CppReferenceArray<PARTY_ITEM>(30);for(int i=0;i<30;i++)g.party_data.party_item[i]=new PARTY_ITEM();
            g.chara_data=new CHARA_DATA();g.chara_data.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(82);
            g.chara_data.c_varia_dat[5]=new C_VARIA_DAT{bogu_eqp=new Il2CppStructArray<byte>(new byte[]{8,23,39}),
                mon_eqp=new Il2CppStructArray<byte>(3),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3)};
            for(int i=0;i<3;i++)g.chara_data.c_varia_dat[5].item_eqp[i]=new PARTY_ITEM();
            g.have_data=new Il2CppReferenceArray<HAVE_DATA>(176);for(int i=0;i<176;i++)g.have_data[i]=new HAVE_DATA();
            var original=g.have_data;Framework.Singleton<GAME_WORK>.instance=g;
            if(G2_SYS.G2_item_search(1,23,0)!=-1)throw new InvalidOperationException("Empty native equipment cache not empty.");
            var lease=new HallPartyEquipment(g,GuildNativeProbe.Data!.ovit_data,new[]{5});
            var player=Alloc<BattlePlayerCharacter>();player.party_player_no=0;player.chara_id=5;
            int reads=0;for(int a=0;a<=8;a++){player.G2_def_atrb(a);reads++;}
            if(g.have_data.Pointer==original.Pointer||original.Any(x=>x.no!=0))throw new InvalidOperationException("Stage registration mutated borrowed cache.");
            lease.Restore();lease.Restore();
            if(g.have_data.Pointer!=original.Pointer)throw new InvalidOperationException("Original equipment cache not restored.");
            ProductionAssetAudit.Rows.Add(new{Kind="HallPartyEquipmentNative",NativeAttributeReads=reads,OriginalTemplates=true,EmptyCache=true,
                OriginalCachePreserved=true,Restored=true,PartyRecordsSynthetic=true,ActualBattle=false});
        }
        finally{Framework.Singleton<GAME_WORK>.instance=prior;UnityEngine.Random.state=rng;}
    }
}
