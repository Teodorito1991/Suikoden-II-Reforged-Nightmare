using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;
// Original reward calculation, real source loot records and native equipment checks.
// Defeated-enemy and victory inputs are synthetic component fixtures, never earned wins.
internal static class HuntRewardProbe
{
 internal static int Assertions;internal static bool Passed;internal static readonly List<object> Rows=new();
 static void Check(bool x,string why){Assertions++;if(!x)throw new InvalidOperationException("Hunt reward: "+why);}
 internal static void Run(BattleManager manager,BATTLE_WORK w,BattleState s)
 {
  var exit=w.exit_work;var magic=w.b_magic_status;var dead=w.dead_monster_data;byte count=w.dead_monster;var variations=w.c_varia_dat;string outcome=CombatReforgedDiagnostics.Outcome;bool snapshot=s.SnapshotTaken;var rng=UnityEngine.Random.state;bool enabled=HuntRuntime.Config.EnableExperimentalHunt;
  try{
   w.exit_work=new EXIT_WORK{monster_item=new PARTY_ITEM()};w.b_magic_status=new BATTLE_MAGIC_STATUS();
   w.dead_monster_data=new Il2CppReferenceArray<MONSTER_DEAD_DATA>(new[]{new MONSTER_DEAD_DATA{chara_id=3,player_no=-1},new MONSTER_DEAD_DATA{chara_id=4,player_no=-1}});w.dead_monster=2;
   w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);for(int i=0;i<6;i++)w.c_varia_dat[i]=new C_VARIA_DAT{mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3),mp=new Il2CppStructArray<byte>(4)};
   // Select a native RNG seed yielding no item candidate. Original item arrays are
   // untouched; this isolates money without inventing inventory/food flag context.
   int seed=0;for(;seed<128;seed++){UnityEngine.Random.InitState(seed);if(OldSrcBase.get_rand(254)>=20&&OldSrcBase.get_rand(254)>=20)break;}Check(seed<128,"Native no-item seed found");
   int raw=w.monster_data[3].mochi_kin*((w.monster_data[3].seishitsu_flg&2)!=0?10:1)+w.monster_data[4].mochi_kin*((w.monster_data[4].seishitsu_flg&2)!=0?10:1);
   int Calculate(string label,bool victory,bool bonus,bool prosperity){
    HuntRuntime.Config.EnableExperimentalHunt=bonus;s.SnapshotTaken=victory;CombatReforgedDiagnostics.Outcome=victory?"VictorySignal":"NonVictorySignal";w.c_varia_dat[0].mon_eqp[0]=(byte)(prosperity?63:0);w.b_magic_status.player_get_money=37;UnityEngine.Random.InitState(seed);
    manager.get_monster_okane(w);int result=w.exit_work.monster_okane;int native=raw*(prosperity?2:1);int expected=Rules.Scale(native,Plugin.Options.PotchMultiplier.Value,int.MaxValue)+(victory&&bonus?HuntRuntime.Config.VictoryBonusPotch:0);
    Check(result==expected,label+" amount from original function and production modifier/bonus");Check(w.exit_work.monster_item.item_no==0,"No item candidate in controlled native seed");Check(w.b_magic_status.player_get_money==(prosperity?74:37),"Original separate acquired-money semantics");
    Rows.Add(new{Case=label,NativeBase=raw,Prosperity=prosperity,ExpectedNativeTotal=native,ConfiguredFactor=Plugin.Options.PotchMultiplier.Value,Prepared=result,SeparateMoney=w.b_magic_status.player_get_money,OriginalRewardCalculationExecuted=true,VictoryInputSynthetic=true,ActualVictory=false,ActualPayout=false});return result;
   }
   Calculate("No hunt bonus",true,false,false);Calculate("Non-victory bonus excluded",false,true,false);int first=Calculate("Victory component bonus",true,true,false);Check(Calculate("Repeated native recalculation",true,true,false)==first,"Original recalculation never compounds bonus");Calculate("Prosperity precedes flat bonus",true,true,true);HuntRuntime.RecalculateReward();Check(Calculate("Independent repeat victory component",true,true,false)==first,"New victory starts a fresh flat bonus gate");
   Passed=true;Plugin.Logger.LogInfo($"Hunt reward component probe PASS: {Assertions} assertions; original calculation/equipment/modifier/bonus; earned payout=False.");
  }finally{w.exit_work=exit;w.b_magic_status=magic;w.dead_monster_data=dead;w.dead_monster=count;w.c_varia_dat=variations;HuntRuntime.Config.EnableExperimentalHunt=enabled;s.SnapshotTaken=snapshot;CombatReforgedDiagnostics.Outcome=outcome;UnityEngine.Random.state=rng;}
 }
 internal static object Snapshot()=>new{Passed,Assertions,Rows=Rows.ToArray(),OriginalRewardCalculationExecuted=Passed,NativeRewardUiExecuted=false,ActualVictory=false,ActualPayout=false,ActualDropAward=false};
}
