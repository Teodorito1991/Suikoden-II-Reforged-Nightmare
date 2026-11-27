using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
using UnityEngine;
internal static class HuntDebugProbe
{
 static T Alloc<T>() where T:Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 internal static void Run()
 {
  var oldDefinition=NightmareRebalance.HuntRuntime.Active;var prior=Framework.Singleton<BATTLE_WORK>.instance;var priorGame=Framework.Singleton<GAME_WORK>.instance;var oldWork=HuntDebug.Work.GetValue(null);var oldParty=HuntDebug.Party.GetValue(null);var units=(Dictionary<IntPtr,string>)HuntDebug.Units.GetValue(null)!;var oldUnits=units.ToArray();var rng=UnityEngine.Random.state;
  var table=ScriptableObject.CreateInstance<BATTLE_DATA>();var party=ScriptableObject.CreateInstance<MONSTER_PARTY_DATA>();var checks=new List<string>();string? error=null;
  void Check(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);}
  try{
   HuntDebug.FixtureEnabled=true;NightmareRebalance.HuntRuntime.Active=NightmareRebalance.HuntRuntime.Definitions.Single(x=>x.Id=="sun-kings-retinue");
   var game=Alloc<GAME_WORK>();game.game_data=Alloc<GAME_DATA>();game.party_data=Alloc<PARTY_DATA>();game.party_data.party_cha_no=new Il2CppStructArray<byte>(6);game.chara_data=Alloc<CHARA_DATA>();game.chara_data.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(1);game.chara_data.c_kotei_dat=new Il2CppReferenceArray<C_KOTEI_DAT>(new[]{new C_KOTEI_DAT{seicyo_type=new Il2CppStructArray<byte>(4),monsyo_seigen=new Il2CppStructArray<byte>(4),mon_zoku_aisyo=new Il2CppStructArray<byte>(4),mon_kosu_type=new Il2CppStructArray<byte>(3)}});Framework.Singleton<GAME_WORK>.instance=game;var manager=Alloc<BattleManager>();var w=Alloc<BATTLE_WORK>();var native=manager.Cast<IBattleManager>();Framework.Singleton<BATTLE_WORK>.instance=w;table.id="vd06_00";w.battle_data=table;w.monster_party_data=party;w.monster_party_index=4;
   w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.p_status_data=new Il2CppReferenceArray<PLAYER_STATUS_DATA>(12);for(int i=0;i<12;i++)w.p_status_data[i]=new();w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.pm_attack_data=new Il2CppReferenceArray<PM_ATTACK_DATA>(12);w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);w.MokuhyoKaisu=new Il2CppStructArray<byte>(12);w.b_magic_status=new();w.player_kazu=6;w.monster_kazu=2;w.pm_kazu=8;w.front_player=3;w.back_player=3;
   manager.parties=new Il2CppReferenceArray<BattleParty>(2);manager.parties[0]=new(0,native);manager.parties[1]=new(1,native);
   for(int i=0;i<8;i++){PM_DATA u=i<6?Alloc<BattlePlayerCharacter>():Alloc<BattleMonsterCharacter>();u.manager=native;u.chara_no=i;u.chara_id=0;u.position=(byte)(i%6);u.hp=600;u.max_hp=600;u.status=0;u.calc_para=new Il2CppStructArray<int>(new[]{120,70,60,50,80,75,60,100});w.pm_data[i]=u;w.b_chara[i]=Alloc<BATTLE_CHARA>();w.b_chara[i].chara_id=0;w.pm_attack_data[i]=new(){curent_use_cnt=new Il2CppStructArray<byte>(new byte[]{255,255,255,255})};w.c_varia_dat[i]=new(){mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),mp=new Il2CppStructArray<byte>(4),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3)};for(int j=0;j<3;j++)w.c_varia_dat[i].item_eqp[j]=new();}
   game.chara_data.c_varia_dat[0]=w.c_varia_dat[0];HuntDebug.Work.SetValue(null,w.Pointer);HuntDebug.Party.SetValue(null,party.Pointer);units.Clear();units[w.pm_data[6].Pointer]="vd06_00_02";units[w.pm_data[7].Pointer]="vd06_00_03";
   Check((bool)HarmonyLib.AccessTools.Method(typeof(NightmareRebalance.Plugin),"CheckCompatibility").Invoke(null,null)!,"Production compatibility guard accepts integrated debug patch owner");Check(HuntDebug.Scope(w),"Exact synthetic admitted hunt scope accepted");var actor=w.pm_data[6];var player=w.pm_data[0];
   void Hit(int target){var action=new ACTION_DATA{chara_no=6,action_type=0};action.CreateReActionData(1);actor.DamageSetReActionData(action,target,25,0,0);}
   HuntDebug.Armed=false;Hit(0);Check(player.hp==575,"OFF preserves actual native25 HP loss");player.hp=600;
   HuntDebug.FixtureEnabled=false;HuntDebug.Armed=true;short disabledHit=25;HuntDamageProtection.Prefix(0,ref disabledHit);Check(!HuntDebug.Enabled&&disabledHit==25,"Default disabled debug ignores armed intent");HuntDebug.FixtureEnabled=true;
   HuntDebug.Armed=true;Hit(0);Check(player.hp==600&&HuntDebug.Blocked==1,"ON prevents native incoming25 HP loss");
   short enemyHit=25;HuntDamageProtection.Prefix(7,ref enemyHit);Check(enemyHit==25,"Production damage prefix preserves enemy-target argument (component check)");
   w.monster_party_index=3;Hit(0);Check(player.hp==575,"Other encounter untouched while armed");player.hp=600;w.monster_party_index=4;
   HuntDebug.Work.SetValue(null,IntPtr.Zero);Hit(0);Check(player.hp==575,"Unadmitted hunt-shaped encounter untouched");HuntDebug.Work.SetValue(null,w.Pointer);player.hp=600;
   short negative=-10;HuntDamageProtection.Prefix(0,ref negative);Check(negative==-10,"Negative damage/heal input preserved");short positive=25;HuntDamageProtection.Prefix(99,ref positive);Check(positive==25,"Invalid target preserved");
   NightmareRebalance.HuntRuntime.Detach();Check(!HuntDebug.Scope(w)&&units.Count==0,"Production cleanup detaches hunt and debug scope");
   Check(HarmonyLib.Harmony.GetPatchInfo(HarmonyLib.AccessTools.Method(typeof(PM_DATA),nameof(PM_DATA.DamageSetReActionData)))?.Owners.Contains(HuntDebug.Id)==true,"Native damage method patched by addon");
  }catch(Exception e){error=e.ToString();HuntDebug.Log.LogError(e);}finally{NightmareRebalance.HuntRuntime.Active=oldDefinition;HuntDebug.FixtureEnabled=false;HuntDebug.Armed=false;HuntDebug.Work.SetValue(null,oldWork);HuntDebug.Party.SetValue(null,oldParty);units.Clear();foreach(var pair in oldUnits)units[pair.Key]=pair.Value;Framework.Singleton<BATTLE_WORK>.instance=prior;Framework.Singleton<GAME_WORK>.instance=priorGame;UnityEngine.Random.state=rng;UnityEngine.Object.Destroy(table);UnityEngine.Object.Destroy(party);}
  string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","developer");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"hunt-debug-probe.json"),JsonSerializer.Serialize(new{Passed=error==null,Assertions=checks.Count,Checks=checks,Error=error,RenderedCombat=false,SaveWritten=false,SingletonRestored=Framework.Singleton<BATTLE_WORK>.instance?.Pointer==prior?.Pointer}));HuntDebug.Blocked=0;HuntDebug.Prevented=0;HuntDebug.Events.Clear();HuntDebug.Dirty=false;HuntDebug.Log.LogInfo($"Hunt debug native probe passed={error==null}; assertions={checks.Count}");
 }
}
