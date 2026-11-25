using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace NightmareRebalance;
internal static class HallNightmareProbe
{
    static bool rewards;
    static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("Nightmare native fixture: "+label);}
    static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
    internal static void Inspect(BATTLE_DATA original)
    {
        if(!ProductionAssetAudit.Requested)return;
        var definition=HallTrialCatalogue.AllSources.FirstOrDefault(x=>x.TableId==original.id);
        if(definition==null||HallNightmareRules.Find(definition.Id) is not {} profile)return;
        var audit=HallTrialSources.Get(original.id);var source=original.monster_data[audit.Monsters.OrderByDescending(m=>m.Signature[0]).First().Index];
        Check(source.level==profile.Level,"original foe level identity");
        var signature=CombatReforgedRuntime.Signature(source);
        for(int cycle=0;cycle<2;cycle++)
        {
            var copy=HallNightmareRuntime.Copy(original,definition,audit.Monsters.OrderByDescending(m=>m.Signature[0]).First().Index);
            var setupCopy=copy.ShallowCopy();
            try
            {
                Check(copy.Pointer!=source.Pointer&&setupCopy.Pointer!=copy.Pointer,"distinct owned native clones");
                Check(copy.hp==HallNightmareRules.Hp(source.hp,profile)&&setupCopy.hp==copy.hp,"native subsequent copy preserves once-scaled HP");
                Check(CombatReforgedRuntime.Signature(copy).Skip(1).SequenceEqual(signature.Skip(1)),"all attacks/stats except intended HP unchanged");
                Check(signature.SequenceEqual(CombatReforgedRuntime.Signature(source)),"original source unchanged");
            }
            finally{UnityEngine.Object.Destroy(setupCopy);UnityEngine.Object.Destroy(copy);}
        }
        ProductionAssetAudit.Rows.Add(new{Kind="HallNightmareNativeCopies",definition.Id,SourceHp=source.hp,NightmareHp=HallNightmareRules.Hp(source.hp,profile),profile.Damage,Cycles=2,SourceUntouched=true,Rendered=false});
        if(!rewards){Rewards();rewards=true;}
    }
    static void Rewards()
    {
        HallPartyEquipmentProbe.Run();
        var old=Framework.Singleton<GAME_WORK>.instance;var rng=UnityEngine.Random.state;
        try
        {
            var g=Alloc<GAME_WORK>();g.party_data=Alloc<PARTY_DATA>();g.party_data.party_item=new Il2CppReferenceArray<PARTY_ITEM>(30);
            for(int i=0;i<30;i++)g.party_data.party_item[i]=new PARTY_ITEM();
            g.ovit_data=GuildNativeProbe.Data!.ovit_data;g.have_data=new Il2CppReferenceArray<HAVE_DATA>(176);
            Framework.Singleton<GAME_WORK>.instance=g;
            var exit=new EXIT_WORK{monster_item=new PARTY_ITEM()};
            int seed=0;for(;seed<1000;seed++){UnityEngine.Random.InitState(seed);if(UnityEngine.Random.Range(0,100)<25)break;}
            Check(seed<1000,"known native RNG winning seed");
            var ledger=new HallStoneReward();UnityEngine.Random.InitState(seed);
            ledger.Prepare(g,exit,false);Check(!ledger.Attempted&&exit.monster_item.item_no==0,"nonvictory has no roll or offer");
            ledger.Prepare(g,exit,true);Check(ledger.Prepared&&exit.monster_item.item_no==21&&exit.monster_item.use_cnt==1,"production prepares audited native stone");
            var after=UnityEngine.Random.state;ledger.Prepare(g,exit,true);
            Check(after.Equals(UnityEngine.Random.state),"duplicate callback neither rerolls nor grants");
            exit.monster_item.item_no=0;ledger.Prepare(g,exit,true);Check(exit.monster_item.item_no==21&&after.Equals(UnityEngine.Random.state),"recalculation retains one pending offer");
            ledger.PayoutStarted=true;exit.monster_item.item_no=0;ledger.Prepare(g,exit,true);Check(exit.monster_item.item_no==0,"handoff seals ledger");
            var native=G2_SYS.G2_data_get(g.ovit_data,0,21);
            int inserted=G2_SYS.G2_min_item_move(g.ovit_data,native,0,0,0,21,0,0);
            Check(g.party_data.party_item.Count(x=>x.item_no==21)==1,"native stone insertion");
            var orb=G2_SYS.G2_data_get(g.ovit_data,2,4);
            foreach(var slot in g.party_data.party_item){slot.item_no=4;slot.use_cnt=32;}
            int full=G2_SYS.G2_min_item_move(g.ovit_data,native,0,0,0,21,0,0);
            Check(g.party_data.party_item.All(x=>x.item_no==4&&x.use_cnt==32),"native capacity preserves full inventory");
            var priority=new HallStoneReward();exit.monster_item.item_no=4;priority.Prepare(g,exit,true);Check(!priority.Attempted&&exit.monster_item.item_no==4,"rolled loot priority");
            g.sys_work=new SYS_WORK{status_para=new Il2CppStructArray<ushort>(7)};g.game_data=new GAME_DATA{difficulty=0};
            g.chara_data=new CHARA_DATA{ckd_info=GuildNativeProbe.Data.all_data_table.ckd_info,c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(GuildNativeProbe.Data.all_data_table.c_kotei_dat.Length),c_kotei_dat=GuildNativeProbe.Data.all_data_table.c_kotei_dat};
            for(int i=0;i<g.chara_data.c_varia_dat.Length;i++)g.chara_data.c_varia_dat[i]=new C_VARIA_DAT{para=new Il2CppStructArray<byte>(7)};
            var character=new C_VARIA_DAT{para=new Il2CppStructArray<byte>(new byte[]{100,100,100,100,100,100,100})};g.chara_data.c_varia_dat[0]=character;
            G2_SYS.G2_chara_para(1,0,3,0);Check(character.para[0]==103,"native permanent strength increase");
            character.para[0]=254;G2_SYS.G2_chara_para(1,0,3,0);G2_SYS.G2_chara_para(1,0,3,0);
            Check(character.para[0]==255,"repeated native strength increase saturates at 255");
            ProductionAssetAudit.Rows.Add(new{Kind="HallNightmareNativeReward",Prepared=true,NativeInserted=true,NativeFullInventoryPreserved=true,DuplicateRollPrevented=true,OriginalLootPriority=true,NativeStrengthCap=255,NativeInsertReturn=inserted,NativeFullReturn=full,VictoryInputSynthetic=true,RenderedReceipt=false});
            foreach(string reward in new[]{"power-stone","stone-of-magic","stone-of-speed","magic-protection-stone"})
            {
                foreach(var slot in g.party_data.party_item){slot.item_no=0;slot.use_cnt=0;}
                var offer=new EXIT_WORK{monster_item=new PARTY_ITEM()};var receipt=new HallStoneReward();
                UnityEngine.Random.InitState(seed);receipt.Prepare(g,offer,true,reward);
                var item=HuntGuildRuntime.Items.Single(x=>x.Id==reward);
                Check(receipt.Prepared&&offer.monster_item.item_no==item.Number&&offer.monster_item.use_cnt==item.UseCount,
                    "per-tier audited growth stone offer");
                var rngAfter=UnityEngine.Random.state;byte pending=offer.monster_item.item_no;
                receipt.Prepare(g,offer,true,reward=="power-stone"?"stone-of-magic":"power-stone");
                Check(offer.monster_item.item_no==pending&&rngAfter.Equals(UnityEngine.Random.state),"pending reward cannot change identity or reroll");
                var data=G2_SYS.G2_data_get(g.ovit_data,item.Kind,item.Number);
                G2_SYS.G2_min_item_move(g.ovit_data,data,0,0,item.Kind,item.Number,0,0);
                Check(g.party_data.party_item.Count(x=>x.item_no==item.Number)==1,"native capacity-controlled growth stone receipt");
                ProductionAssetAudit.Rows.Add(new{Kind="HallGrowthReward",Id=reward,item.Number,NativeOffer=true,NativeInsertion=true,
                    DuplicateIdentityProtected=true,VictoryInputSynthetic=true,RenderedReceipt=false});
            }

        }
        finally{Framework.Singleton<GAME_WORK>.instance=old;UnityEngine.Random.state=rng;}
    }
}
