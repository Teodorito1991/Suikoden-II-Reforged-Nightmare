using BepInEx;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Security.Cryptography;
namespace NightmareRebalance;
internal static class ExpansionNativeProbe
{
 internal static int Assertions;internal static bool Passed,HuntPrepared,HistoryRestartVerified;internal static object? Evidence;
 static T Allocate<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool ok,string message){Assertions++;if(!ok)throw new InvalidOperationException("Expansion native "+Assertions+": "+message);}
 internal static void Table(BATTLE_DATA table)
 {
  if(table.id!="vd06_00")return;
  var saved=Framework.Singleton<BATTLE_WORK>.instance;var game=Framework.Singleton<GAME_WORK>.instance;var state=Combat.State;var rng=UnityEngine.Random.state;var last=CombatEvolutionRuntime.LastCopy;var lastFormation=CombatEvolutionRuntime.LastFormation;int formations=CombatEvolutionRuntime.FormationApplications;int apps=HuntRuntime.Applications;int rewards=HuntRuntime.RewardCalculations;bool enabled=HuntRuntime.Config.EnableExperimentalHunt;string outcome=CombatReforgedDiagnostics.Outcome;var objects=new List<UnityEngine.Object>();
  try{
   CombatEvolutionRuntime.LastCopy=null;var manager=Allocate<BattleManager>();var w=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=w;var scratch=Allocate<GAME_WORK>();scratch.game_data=Allocate<GAME_DATA>();Framework.Singleton<GAME_WORK>.instance=scratch;
   w.battle_data=table;w.monster_party_index=4;w.monster_party_data=table.monster_party[3];var original=w.monster_party_data;
   HuntRuntime.Reset();HuntLaunchProof.Clear();HuntRuntime.Config.EnableExperimentalHunt=false;Check(!HuntRuntime.Formation(w,true),"Default-off prototype changes no encounter");HuntRuntime.Config.EnableExperimentalHunt=true;Check(!HuntRuntime.Formation(w,true),"Unproven/scripted launch cannot receive hunt");DifficultyProvenance.StrictProbe=true;HuntLaunchProof.Fixture(w);Check(!HuntRuntime.Formation(w,false),"Compatibility opt-out");scratch.game_data.difficulty=2;Check(!HuntRuntime.Formation(w,true),"Normal foundation only; no hunt normalization for Hard");scratch.game_data.difficulty=0;Check(HuntRuntime.Formation(w,true),"Real source optional formation copy");
   var copy=w.monster_party_data;Check(copy.Pointer!=original.Pointer&&copy.monster_kazu==original.monster_kazu&&copy.party_flg==0,"Disposable original-count party");Check(original.monster_code.Select(x=>(int)x).SequenceEqual(HuntRuntime.Audit.OldCodes),"Source party untouched");Check(copy.monster_code.Select(x=>(int)x).SequenceEqual(HuntRuntime.Audit.NewCodes),"Audited native roster");Check(CombatEvolutionRuntime.Hooks(copy.data).SequenceEqual(HuntRuntime.Audit.Hooks),"Original field/start callbacks");
   w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(table.monster_data.ToArray());w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.player_kazu=6;w.monster_kazu=2;w.pm_kazu=8;
   DifficultyProvenance.StrictProbe=true;CombatReforgedRuntime.Probe=true;CombatReforgedRuntime.ProbeWork=w;
   var previous=DifficultyProvenance.Start(w);try{for(int i=1;i<table.monster_data.Length;i++){if(table.monster_data[i]==null)continue;var c=table.monster_data[i].ShallowCopy();objects.Add(c);c.hp=(ushort)MaintenanceRules.NativeHp(c.hp,ConstShare.GetDifficultyRate(0));w.monster_data[i]=c;}}finally{DifficultyProvenance.End(previous);}
   var native=manager.Cast<IBattleManager>();for(int i=0;i<8;i++){PM_DATA u=i<6?Allocate<BattlePlayerCharacter>():Allocate<BattleMonsterCharacter>();u.manager=native;u.chara_no=i;u.hp=300;u.max_hp=300;u.calc_para=new Il2CppStructArray<int>(8);w.pm_data[i]=u;w.b_chara[i]=Allocate<BATTLE_CHARA>();int id=i<6?0:i-3;u.chara_id=(byte)id;w.b_chara[i].chara_id=id;if(i>=6){u.hp=(short)w.monster_data[id].hp;u.max_hp=w.monster_data[id].hp;}}
   var s=new BattleState(manager){Ready=true,Allowed=true,StatsAllowed=true};Combat.State=s;CombatReforgedRuntime.Ready(s,w,true);Check(CombatReforgedRuntime.Plans.Count==2,"Strict original-engine admissions");HuntRuntime.Ready(s,w);Check(HuntRuntime.Units.Count==2,"Hunt binding after regular source checks");
   foreach(var u in w.pm_data.Where(u=>u!=null&&!u.IsPlayer)){int before=u.max_hp;Combat.ScaleEnemy(u);int after=u.max_hp;Check(after==Rules.Scale(before,u.monster_data.id=="vd06_00_02"?HuntRuntime.Config.SunKingHpMultiplier:HuntRuntime.Config.SorcererHpMultiplier,32767),"Existing scale ledger/hunt replacement factor");Combat.ScaleEnemy(u);Check(u.max_hp==after,"Exactly-once hunt scale");}
   var exit=new EXIT_WORK{monster_okane=1000};CombatReforgedDiagnostics.Outcome="ClosedWithoutVictorySignal";HuntRuntime.Reward(w,exit);Check(exit.monster_okane==1000,"No bonus after non-victory");s.SnapshotTaken=true;CombatReforgedDiagnostics.Outcome="VictorySignal";HuntRuntime.Reward(w,exit);Check(exit.monster_okane==1000+HuntRuntime.Config.VictoryBonusPotch,"Audited reward field preparation");HuntRuntime.Reward(w,exit);Check(exit.monster_okane==1000+HuntRuntime.Config.VictoryBonusPotch,"Repeat callback no stacked reward");
   HuntRewardProbe.Run(manager,w,s);
   Evidence=new{Hunt=HuntRuntime.Snapshot(),Roles=CombatReforgedRuntime.Plans.Values.ToArray(),Hp=w.pm_data.Where(u=>u!=null&&!u.IsPlayer).Select(u=>new{Id=u.monster_data.id,u.max_hp}).ToArray(),RewardField=exit.monster_okane,RandomLaunchProof="Developer-only fixture; actual field launch not executed",NativeRewardPayoutExecuted=false,RenderedBattleLaunched=false,VictoryEarned=false,FieldReturnVerified=false};HuntPrepared=true;
  }finally{HuntLaunchProof.Clear();HuntRuntime.Config.EnableExperimentalHunt=enabled;HuntRuntime.Reset();HuntRuntime.Applications=apps;HuntRuntime.RewardCalculations=rewards;DifficultyProvenance.StrictProbe=false;CombatReforgedRuntime.Probe=false;CombatReforgedRuntime.ProbeWork=null;CombatReforgedRuntime.Clear();Combat.State=state;CombatReforgedDiagnostics.Outcome=outcome;Framework.Singleton<BATTLE_WORK>.instance=saved;Framework.Singleton<GAME_WORK>.instance=game;UnityEngine.Random.state=rng;if(CombatEvolutionRuntime.LastCopy!=null&&CombatEvolutionRuntime.LastCopy!=last)UnityEngine.Object.Destroy(CombatEvolutionRuntime.LastCopy);CombatEvolutionRuntime.LastCopy=last;CombatEvolutionRuntime.LastFormation=lastFormation;CombatEvolutionRuntime.FormationApplications=formations;foreach(var o in objects)UnityEngine.Object.Destroy(o);}
  Check(Framework.Singleton<BATTLE_WORK>.instance?.Pointer==saved?.Pointer&&Framework.Singleton<GAME_WORK>.instance?.Pointer==game?.Pointer,"Globals restored");
 }
 internal static void Finish()
 {
  Check(HuntPrepared,"One audited hunt candidate prepared");string dir=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","developer");Directory.CreateDirectory(dir);string path=Path.Combine(dir,"expansion-history-fixture.json");string hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(Plugin).Assembly.Location)));
  if(File.Exists(path)){using var old=JsonDocument.Parse(File.ReadAllText(path));var rows=old.RootElement.GetProperty("Battles");if(rows.GetArrayLength()==12&&rows[0].GetProperty("PluginHash").GetString()==hash&&rows[0].GetProperty("Process").GetInt32()!=Environment.ProcessId){Check(rows[0].GetProperty("Generation").GetInt32()>=1,"Oldest correctly trimmed");Check(rows[11].GetProperty("Outcome").GetString()=="Escape","Last escape preserved");HistoryRestartVerified=true;}}
  // Dedicated component fixture file only, never the player's battle-history.json.
  if(File.Exists(path))File.Move(path,path+".previous",true);
  for(int i=0;i<13;i++)DiagnosticHistoryStore.Append(path,JsonSerializer.Serialize(new{PluginHash=hash,Process=Environment.ProcessId,Generation=i,Outcome=i==12?"Escape":"Victory",SyntheticComponentFixture=true,ActualGameplay=false}));
  using(var current=JsonDocument.Parse(File.ReadAllText(path)))Check(current.RootElement.GetProperty("Battles").GetArrayLength()==12,"History bounded across writes");
  Passed=true;Plugin.Logger.LogInfo($"Expansion probe PASS: {Assertions} assertions; hunt prepared={HuntPrepared}; cross-process history={HistoryRestartVerified}; playable hunt=False.");
 }
 internal static object Snapshot()=>new{Passed,Assertions,HuntPrepared,HistoryRestartVerified,Evidence,PlayableHunt=false};
}
