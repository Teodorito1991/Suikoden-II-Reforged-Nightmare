using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
namespace NightmareRebalance;
// Calls original setup only with disposable campaign/work objects. No scene or outcome is launched.
internal static class HuntLifecycleProbe
{
 internal static int Assertions,Setups,RemixSetups,VoluntarySelections;internal static bool PurseComponentPassed;internal static readonly HashSet<string> RemixIds=new();internal static readonly List<object> Rows=new();
 static T Allocate<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool x,string why){Assertions++;if(!x)throw new InvalidOperationException("Hunt lifecycle: "+why);}
 internal static void Table(BATTLE_DATA table)
 {
  var audits=CombatEvolutionRuntime.Audit.Formations.Concat(CombatMasteryRuntime.Audit.Formations).Concat(CombatRealityRuntime.Audit.Formations).Concat(RpgEvolutionRuntime.Audit.Formations).Where(a=>a.TableId==table.id).ToArray();
  if(table.id!="vd06_00"&&audits.Length==0)return;
  var oldGame=Framework.Singleton<GAME_WORK>.instance;var oldWork=Framework.Singleton<BATTLE_WORK>.instance;var rng=UnityEngine.Random.state;var state=Combat.State;var last=CombatEvolutionRuntime.LastCopy;var lastFormation=CombatEvolutionRuntime.LastFormation;int apps=CombatEvolutionRuntime.FormationApplications;bool enabled=HuntRuntime.Config.EnableExperimentalHunt;
  int mastery=CombatMasteryRuntime.FormationApplications,reality=CombatRealityRuntime.FormationApplications,hunts=HuntRuntime.Applications,selections=HuntAccess.Selections;bool strict=DifficultyProvenance.StrictProbe;
  var castleQuest=CastleGuildRuntime.Quest;var castleConfig=CastleGuildRuntime.Config;var castleOwner=CastleGuildRuntime.ContractParty;int castleRequests=CastleGuildRuntime.AutoRequests;string castleStatus=CastleGuildRuntime.Status;var guildSelection=HuntGuildRuntime.SessionSelection;var checking=HuntLaunchProof.Checking;
  var fields=new List<MACHIDAT>();var copies=new List<MONSTER_DATA>();var parties=new List<MONSTER_PARTY_DATA>();
  try{
   var game=Allocate<GAME_WORK>();game.sys_work=Allocate<SYS_WORK>();game.sys_work.battle_data=table;game.sys_work.yobi=new Il2CppStructArray<byte>(8);game.game_data=Allocate<GAME_DATA>();game.party_data=Allocate<PARTY_DATA>();Framework.Singleton<GAME_WORK>.instance=game;
   var manager=Allocate<BattleManager>();HuntRuntime.Config.EnableExperimentalHunt=false;
   void Remember(BATTLE_WORK w){copies.AddRange(w.monster_data.Where(x=>x!=null));if(w.monster_party_data.Pointer!=table.monster_party[w.monster_party_index-1].Pointer)parties.Add(w.monster_party_data);}
   foreach(int i in (table.id=="vd06_00"?Enumerable.Range(0,table.monster_party.Length):audits.SelectMany(a=>a.Indices)).Distinct()){
    var w=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=w;game.sys_work.yobi[0]=(byte)(i+1);Combat.State=null;
    manager.BattleDataSetup();Setups++;
    Check(w.monster_party_index==i+1,"Actual original setup stores one-based party number");
    Check(w.battle_data.Pointer==table.Pointer,"Original source table bound");
    Check(w.monster_party_data.id==table.monster_party[i].id,"Original one-based selection maps to zero-based array");
    Remember(w);foreach(var d in w.monster_data.Where(x=>x!=null)){var source=table.monster_data.First(x=>x!=null&&x.id==d.id);Check(d.Pointer!=source.Pointer&&d.hp==source.hp,"Normal disposable copies from actual setup");Check(DifficultyProvenance.Matches(w,d,CombatReforgedRuntime.Signature(source)),"Production provenance from original complete setup");}
    var a=audits.FirstOrDefault(a=>a.Indices.Contains(i));if(a!=null){Check(w.monster_party_data.monster_code.Select(x=>(int)x).SequenceEqual(a.NewCodes),"Retained formation applied after actual native setup");Check(CombatEvolutionRuntime.Hooks(w.monster_party_data.data).SequenceEqual(a.Hooks),"Original callbacks preserved without execution");RemixSetups++;var selected=w.monster_party_data;w.monster_party_data=table.monster_party[i];w.monster_party_index=0;Check(!CombatEvolutionRuntime.ApplyAuditedFormation(w,true,new[]{a},new[]{new EvolutionSwitch{PartyId=a.PartyId}}),"Invalid zero native party number rejected");w.monster_party_index=i+1;w.monster_party_data=selected;}
    Rows.Add(new{Table=table.id,ArrayIndex=i,NativePartyNumber=w.monster_party_index,Party=w.monster_party_data.id,RemixApplied=a!=null,OriginalSetupExecuted=true,RenderedBattle=false});
   }
   foreach(var a in audits)RemixIds.Add(a.PartyId);
   if(table.id=="vd06_00"){
   foreach(int m in new[]{3,4}){var d=table.monster_data[m];var opd=d.opd_data??throw new InvalidOperationException(nameof(d.opd_data));Check(opd.anime_kazu>0&&opd.pattern_kazu>0&&opd.anime_table.Count>0&&opd.pattern_table.Count>0,"Original native animation/pattern dependencies loaded");Rows.Add(new{Kind="NativeAnimationSource",d.id,opd.anime_kazu,opd.pattern_kazu,PlaybackExecuted=false});}
   // A synthetic walking request supplies context; original setup and RNG selector execute.
   DifficultyProvenance.StrictProbe=true;HuntRuntime.Config.EnableExperimentalHunt=true;game.sys_work.yobi[0]=0;
   Check(!HuntFieldContext.Active&&!HuntAccess.Toggle()&&!HuntAccess.Ticket.Armed,"Production F6 request denied without a current original field chapter");
   int exactSeed=0;for(;exactSeed<128;exactSeed++){UnityEngine.Random.InitState(exactSeed);if(OldSrcBase.get_rand_maxlimit(8)==3)break;}Check(exactSeed<128,"Original native RNG can select audited source slot");
   var unarmed=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=unarmed;UnityEngine.Random.InitState(exactSeed);HuntLaunchProof.FixtureRequest(table);manager.BattleDataSetup();Setups++;Remember(unarmed);
   Check(unarmed.monster_party_index==4&&unarmed.monster_party_data.Pointer==table.monster_party[3].Pointer&&unarmed.monster_party_data.monster_code.Select(x=>(int)x).SequenceEqual(HuntRuntime.Audit.OldCodes),"Exact native hunt source stays ordinary when enabled but not armed");
   Rows.Add(new{Kind="UnarmedExactSource",NativePartyNumber=4,SourceArrayIndex=3,OriginalPartyPreserved=true,OriginalSetupExecuted=true,WalkingRequestSynthetic=true,RenderedBattle=false});
   var baseline=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=baseline;UnityEngine.Random.InitState(5101);HuntLaunchProof.FixtureRequest(table);manager.BattleDataSetup();Setups++;Remember(baseline);var nativeRng=UnityEngine.Random.state;
   Check(!HuntAccess.Ticket.Armed&&HuntAccess.SelectedWork==IntPtr.Zero,"Enabled option alone never requests a hunt");
   var selectedWork=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=selectedWork;UnityEngine.Random.InitState(5101);HuntAccess.Ticket.Toggle(true);HuntLaunchProof.FixtureRequest(table);HuntAccess.LeavingField();Check(HuntAccess.Ticket.Armed,"Proven pending walking request survives map-to-battle handoff");manager.BattleDataSetup();Setups++;Remember(selectedWork);var selectedRng=UnityEngine.Random.state;
   Check(selectedWork.monster_party_index==4&&HuntAccess.SelectedWork==selectedWork.Pointer,"Production hook selects native party number four after explicit request");
   Check(selectedWork.monster_party_data.monster_code.Select(x=>(int)x).SequenceEqual(HuntRuntime.Audit.NewCodes),"Hunt copied by actual setup postfix");
   Combat.Begin(manager);Check(HuntRuntime.Work==selectedWork.Pointer&&HuntRuntime.Party==selectedWork.monster_party_data.Pointer,"Production BattleMainInit prefix preserves freshly prepared hunt context");
   Check(!HuntAccess.Ticket.Armed,"Request consumed once");Check(nativeRng.s0==selectedRng.s0&&nativeRng.s1==selectedRng.s1&&nativeRng.s2==selectedRng.s2&&nativeRng.s3==selectedRng.s3,"Original selector RNG call preserved");VoluntarySelections++;
   Rows.Add(new{Kind="NativeSelection",Before=HuntAccess.OriginalSelection,After=3,NativePartyNumber=4,OneShot=true,OriginalSetupExecuted=true,WalkingRequestSynthetic=true,RenderedBattle=false});
   // Production walking-request helper + ORIGINAL native setup, with a synthetic walking witness.
   // No field/event initializer, scene launch, victory callback or game-save write.
   CastleGuildRuntime.Config=new(){Enabled=true,AutoRequestAcceptedContract=true};CastleGuildRuntime.Quest=new();CastleGuildRuntime.Quest.Accept("sun-kings-retinue");CastleGuildRuntime.ContractParty=game.party_data.Pointer;HuntGuildRuntime.SessionSelection="sun-kings-retinue";
   var walker=Allocate<EVENTCON>();fields.Add(CastleGuildNativeProbe.BindWalkingFixture(walker,table));HuntLaunchProof.Clear();HuntLaunchProof.Checking=walker.Pointer;
   HuntLaunchProof.Requested(walker,1);Check(!HuntAccess.Ticket.Armed,"Castle contract does not arm from scripted native-request type");
   HuntLaunchProof.Requested(walker,0);HuntLaunchProof.Checking=checking;Check(HuntAccess.Ticket.Armed,"Accepted castle contract auto-arms through production walking-request helper without F6");
   HuntAccess.LeavingField();Check(HuntAccess.Ticket.Armed,"Castle request survives original field-to-battle handoff witness");
   var autoWork=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=autoWork;UnityEngine.Random.InitState(5101);manager.BattleDataSetup();Setups++;Remember(autoWork);var autoRng=UnityEngine.Random.state;
   Check(HuntAccess.SelectedWork==autoWork.Pointer&&autoWork.monster_party_index==4&&HuntRuntime.Active?.Id=="sun-kings-retinue","Castle request reaches actual production native setup and audited party");
   Check(!HuntAccess.Ticket.Armed&&nativeRng.Equals(autoRng),"Castle selection consumes once and preserves original native RNG call");
   Rows.Add(new{Kind="CastleAutoWalkingNativeSetup",NoF6Called=true,WalkingWitnessSynthetic=true,OriginalSetupExecuted=true,NativePartyNumber=autoWork.monster_party_index,RenderedBattle=false,ActualVictory=false});
   // Another attempt can be requested; no daily or persistent completion gate.
   var repeated=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=repeated;HuntAccess.Ticket.Toggle(true);HuntLaunchProof.FixtureRequest(table);manager.BattleDataSetup();Setups++;Remember(repeated);Check(HuntAccess.SelectedWork==repeated.Pointer,"Repeat native selection without campaign flags");VoluntarySelections++;
   var excluded=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=excluded;game.game_data.difficulty=2;HuntAccess.Ticket.Toggle(true);HuntLaunchProof.FixtureRequest(table);manager.BattleDataSetup();Setups++;Remember(excluded);Check(HuntAccess.SelectedWork==IntPtr.Zero&&!HuntAccess.Ticket.Armed,"Hard guard rejects request in original setup");game.game_data.difficulty=0;
   var scripted=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=scripted;HuntAccess.Ticket.Toggle(true);HuntLaunchProof.Clear();manager.BattleDataSetup();Setups++;Remember(scripted);Check(HuntAccess.SelectedWork==IntPtr.Zero&&!HuntAccess.Ticket.Armed,"No walking provenance leaves native choice untouched");
   HuntAccess.Ticket.Toggle(true);HuntAccess.Requested(table,false);Check(!HuntAccess.Ticket.Armed,"Scripted request cancels ticket");
   HuntLaunchProof.Clear();HuntAccess.Ticket.Toggle(true);HuntAccess.LeavingField();Check(!HuntAccess.Ticket.Armed,"Unrelated field exit cancels uncommitted request");
   game.party_data.gold=1000;Check(G2_SYS.G2_party_gold(1,3500)==4500&&game.party_data.gold==4500,"Original native purse credit on scratch party");
   Check(HuntRewardObservation.Credits.Count==0,"Unrelated purse credit is not attributed to a hunt");
   HuntRewardObservation.InNativeReward=true;game.party_data.gold=1000;G2_SYS.G2_party_gold(1,3500);HuntRewardObservation.InNativeReward=false;Check(HuntRewardObservation.Credits.Count==1,"Production observer captures native purse mutation in synthetic payout scope");
   game.party_data.gold=999000;Check(G2_SYS.G2_party_gold(1,3500)==999999&&game.party_data.gold==999999,"Original native purse cap, no plugin cap");
   Check(G2_SYS.G2_party_gold(0,0)==999999,"Original read mode does not credit twice");
   PurseComponentPassed=true;
   }
  }finally{
   CastleGuildRuntime.Quest=castleQuest;CastleGuildRuntime.Config=castleConfig;CastleGuildRuntime.ContractParty=castleOwner;CastleGuildRuntime.AutoRequests=castleRequests;CastleGuildRuntime.Status=castleStatus;HuntGuildRuntime.SessionSelection=guildSelection;HuntLaunchProof.Checking=checking;
   HuntRuntime.Config.EnableExperimentalHunt=enabled;HuntRuntime.Reset();HuntLaunchProof.Clear();HuntAccess.Reset();HuntAccess.Selections=selections;HuntRuntime.Applications=hunts;HuntRewardObservation.InNativeReward=false;DifficultyProvenance.StrictProbe=strict;Combat.State=state;Framework.Singleton<GAME_WORK>.instance=oldGame;Framework.Singleton<BATTLE_WORK>.instance=oldWork;UnityEngine.Random.state=rng;
   CombatEvolutionRuntime.LastCopy=last;CombatEvolutionRuntime.LastFormation=lastFormation;CombatEvolutionRuntime.FormationApplications=apps;CombatMasteryRuntime.FormationApplications=mastery;CombatRealityRuntime.FormationApplications=reality;foreach(var f in fields)UnityEngine.Object.Destroy(f);foreach(var c in copies)UnityEngine.Object.Destroy(c);foreach(var c in parties)UnityEngine.Object.Destroy(c);
  }
  Check(Framework.Singleton<GAME_WORK>.instance?.Pointer==oldGame?.Pointer&&Framework.Singleton<BATTLE_WORK>.instance?.Pointer==oldWork?.Pointer,"Original globals restored");
  Plugin.Logger.LogInfo($"Hunt lifecycle component probe PASS: {Assertions} assertions; original setup calls={Setups}; native scratch purse credit; rendered hunt=False.");
 }
 internal static object Snapshot()=>new{Assertions,Setups,RemixSetups,UniqueRemixIdentities=RemixIds.Count,VoluntarySelections,Rows=Rows.ToArray(),NativeSetupExecuted=Setups>0,NativePurseComponentExecuted=PurseComponentPassed,RenderedHunt=false,ActualVictory=false,ActualPayout=false,FieldReturn=false};
}
