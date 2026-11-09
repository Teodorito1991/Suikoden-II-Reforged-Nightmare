using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace NightmareRebalance;
// Read-only assets and private ScriptableObject clones; never EventInit/EventMain/flags/save.
internal static class CastleGuildNativeProbe
{
 static AsyncOperationHandle<MACHIDAT>? pending;static readonly List<AsyncOperationHandle<MACHIDAT>> retained=new();static readonly string[] maps={"vk01","vk31","vk04","vk08"};static int index;static AsyncOperationHandle<UnityEngine.U2D.SpriteAtlas>? atlas;
 internal static bool Passed;internal static int Assertions;internal static readonly List<object> Rows=new();
 static void Check(bool x,string why){Assertions++;if(!x)throw new InvalidOperationException("Castle Guild native: "+why);}
 internal static bool Step()
 {
  if(index==maps.Length){
   if(atlas==null){atlas=Addressables.LoadAssetAsync<UnityEngine.U2D.SpriteAtlas>((Il2CppSystem.String)"gsd2-classic-opd-person-vb10_06_atlas");return false;}
   if(!atlas.IsDone)return false;Check(atlas.Status==AsyncOperationStatus.Succeeded&&atlas.Result!=null,"Original attendant atlas loads");
   var sprites=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Sprite>(atlas.Result!.spriteCount);int count=atlas.Result.GetSprites(sprites);Check(count>0&&count==atlas.Result!.spriteCount&&sprites.All(s=>s!=null&&s.texture!=null),"Original attendant sprites/textures retrieved");Rows.Add(new{AttendantAtlas="gsd2-classic-opd-person-vb10_06_atlas",Sprites=count,RenderedNpc=false});Passed=true;return true;}

  if(pending==null){pending=Addressables.LoadAssetAsync<MACHIDAT>((Il2CppSystem.String)("gsd2-classic-machi-"+maps[index]+"-"+maps[index]));return false;}
  if(!pending.IsDone)return false;Check(pending.Status==AsyncOperationStatus.Succeeded&&pending.Result!=null,"Original castle asset loads");var source=pending.Result!;
  Check(source.eventdata?.mapeventdat!=null&&source.epersondat!=null,"Original event/person tables exist");
  Rows.Add(new{Map=maps[index],Name=source.name,Variants=source.eventdata!.mapeventdat.Count,Persons=source.epersondat!.Length,NativeFieldEntered=false});
  if(maps[index]==CastleGuildRuntime.Audit.MapId){
   Check(CastleGuildRuntime.Matches(source),"Full original target-room source fingerprint");
   var opd=source.epersondat.Single(x=>x.fp_no==CastleGuildRuntime.Audit.Sprite).hito_opd_table;
   Check(opd!=null&&opd.anime_table!=null&&opd.anime_table.Count>0&&opd.pattern_table!=null&&opd.pattern_table.Count>0,"Existing attendant OPD has native animation/pattern records");
   Rows.Add(new{Sprite=CastleGuildRuntime.Audit.Sprite,opd!.id,opd.path,opd.anime_kazu,opd.pattern_kazu,ActorConstructed=false,SpriteRendered=false});
   var runtime=UnityEngine.Object.Instantiate(source);
   try{
    var owner=Alloc<MACHICON>();owner.mdat=runtime;owner.mno=CastleGuildRuntime.Audit.SupportedIndices[0];
    var info=new MemoryInfo.MemoryData{obj=runtime,originalAsset=source,path=CastleGuildRuntime.Audit.MapId};
    Check(!CastleGuildRuntime.Matches(runtime),"Native runtime clone name differs from canonical source");
    Check(CastleGuildRuntime.RuntimeMatches(owner,info),"Game-cache owned clone with exact original signatures admitted");
    owner.memtyp=1;var overlay=new MemoryInfo.MemoryData{obj=Alloc<MACHICON>(),path="OverlayClass 29 "};
    Check(CastleGuildRuntime.FindRuntimeInfo(owner,new[]{overlay,info})?.Pointer==info.Pointer,"Overlay in slot1 cannot mask exact owned room in slot2");
    Check(CastleGuildRuntime.FindRuntimeInfo(owner,new[]{info,overlay})?.Pointer==info.Pointer,"Exact owned room in slot1 supported independently of memtyp");
    Check(CastleGuildRuntime.FindRuntimeInfo(owner,new[]{overlay})==null,"No matching owned room rejected");
    Check(CastleGuildRuntime.FindRuntimeInfo(owner,new[]{info,info})==null,"Ambiguous duplicate ownership rejected");
    var prepared=CastleGuildRuntime.PrepareRuntime(owner,info);UnityEngine.Object.Destroy(prepared);
    info.path="foreign";Check(!CastleGuildRuntime.RuntimeMatches(owner,info),"Wrong cache provenance path rejected");info.path=CastleGuildRuntime.Audit.MapId;
    info.obj=source;Check(!CastleGuildRuntime.RuntimeMatches(owner,info),"Foreign runtime pointer rejected");info.obj=runtime;
    info.originalAsset=null;Check(!CastleGuildRuntime.RuntimeMatches(owner,info),"Missing original source rejected");info.originalAsset=source;
    var map=runtime.eventdata.mapeventdat[0]; // Never mutate the shallow clone's shared source arrays.
    var altered=CastleGuildRuntime.Prepare(source,owner.mno);owner.mdat=altered;info.obj=altered;
    try{Check(!CastleGuildRuntime.RuntimeMatches(owner,info),"Changed roster or script data rejected despite cache ownership");}finally{UnityEngine.Object.Destroy(altered);}
    Rows.Add(new{OriginalName=source.name,RuntimeCloneName=runtime.name,CacheProvenanceFixture=true,LiveRoomEntered=false});
   }finally{UnityEngine.Object.Destroy(runtime);}
   foreach(int target in CastleGuildRuntime.Audit.SupportedIndices){
    var rng=UnityEngine.Random.state;var copy=CastleGuildRuntime.Prepare(source,target);
    try{
     Check(copy.Pointer!=source.Pointer&&copy.eventdata.Pointer!=source.eventdata.Pointer,"Private source clone");
     for(int i=0;i<source.eventdata.mapeventdat.Count;i++){
      var before=source.eventdata.mapeventdat[i];var after=copy.eventdata.mapeventdat[i];
      Check(after.Pointer!=before.Pointer&&after.eventobj.Pointer!=before.eventobj.Pointer&&after.eventdat.Pointer!=before.eventdat.Pointer,"Room arrays privately owned");
      Check(after.evkaz==before.evkaz&&after.eventdat.Length==before.eventdat.Length,"No event script inserted");
      for(int j=0;j<before.eventdat.Length;j++){var a=before.eventdat[j];var b=after.eventdat[j];Check(a.etyp==b.etyp&&a.eventCom.SequenceEqual(b.eventCom)&&a.eventForm.SequenceEqual(b.eventForm)&&a.eventCom.Pointer!=b.eventCom.Pointer&&a.eventForm.Pointer!=b.eventForm.Pointer,"Original script bytes preserved, not aliased");}
      Check(after.okaz==before.okaz+(i==target?1:0)&&after.eventobj.Length==before.eventobj.Length+(i==target?1:0),"Exactly one extra actor in selected variant");
      for(int j=0;j<before.eventobj.Length;j++){var a=before.eventobj[j];var b=after.eventobj[j];Check(a.Pointer!=b.Pointer&&a.otyp==b.otyp&&a.disp==b.disp&&a.spd==b.spd&&a.wt==b.wt&&a.ano==b.ano&&a.ityp==b.ityp&&a.fpno==b.fpno&&a.ozok==b.ozok&&a.pri==b.pri&&a.x==b.x&&a.y==b.y&&a.w==b.w&&a.h==b.h,"Every original resident/prop preserved");}
     }
     var room=copy.eventdata.mapeventdat[target];var actor=room.eventobj[room.eventobj.Length-1];Check(actor.fpno==CastleGuildRuntime.Audit.Sprite&&actor.otyp==1&&actor.disp==1&&actor.spd==4&&actor.wt==255&&actor.ozok==0,"Stationary non-blocking actor definition");
     Check(copy.epersondat.Single(x=>x.fp_no==actor.fpno).hito_opd_table.Pointer==opd.Pointer,"Original loaded sprite resource reused");
     copy.eventdata.mapeventdat[0].eventobj[0].x++;copy.eventdata.mapeventdat[0].eventdat[0].eventCom[0]++;
     Check(CastleGuildRuntime.Matches(source),"Cloned object/script mutations cannot change original asset");Check(!CastleGuildRuntime.Matches(copy),"Unexplained source mutation rejected");
     Check(UnityEngine.Random.state.Equals(rng),"Source preparation preserves native RNG");
    }finally{UnityEngine.Object.Destroy(copy);}
   }
   foreach(int excluded in new[]{-1,0,2,3,4,5}){bool rejected=false;try{var c=CastleGuildRuntime.Prepare(source,excluded);UnityEngine.Object.Destroy(c);}catch(InvalidOperationException){rejected=true;}Check(rejected,"Unsupported castle variants rejected");}
  }
  retained.Add(pending);pending=null;index++;return false;
 }
 static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),Il2CppInterop.Runtime.IL2CPP.il2cpp_object_new(Il2CppInterop.Runtime.Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 internal static MACHIDAT BindWalkingFixture(EVENTCON e,BATTLE_DATA table){
  if(!UnityEngine.Application.isBatchMode||!Plugin.Options.RunCombatSelfTest.Value)throw new InvalidOperationException("Walking scene fixture is batch-test only");
  var m=Alloc<MACHICON>();var data=UnityEngine.ScriptableObject.CreateInstance<MACHIDAT>();data.battle_dat=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<BATTLE_DATA>(new[]{table});var location=HuntFieldSourceProbe.FindLocation(table.id.Split('_')[0])??throw new InvalidOperationException("Missing unique native map ID");m.ano=location.Area;m.vno=location.Town;m.mdat=data;m.econ=e;e.mcon=m;e.mdat=data;return data;
 }
 // Drives the actual request observer, followed by BattleDataSetup in the caller.
 // The walking scope is synthetic and the original EVENTCON methods are not run.
 internal static void RequestCatalogueSetup(BATTLE_DATA table,HuntDefinition hunt,GAME_WORK game)
 {
  var quest=CastleGuildRuntime.Quest;
  var config=CastleGuildRuntime.Config;
  var owner=CastleGuildRuntime.ContractParty;
  var selected=HuntGuildRuntime.SessionSelection;
  var checking=HuntLaunchProof.Checking;
  MACHIDAT? field=null;
  try
  {
   var caller=Alloc<EVENTCON>();
   field=BindWalkingFixture(caller,table);
   CastleGuildRuntime.Config=new(){Enabled=true,AutoRequestAcceptedContract=true};
   CastleGuildRuntime.Quest=new();
   Check(CastleGuildRuntime.Quest.Accept(hunt.Id),"Fixture accepts selected contract");
   CastleGuildRuntime.ContractParty=game.party_data.Pointer;
   HuntGuildRuntime.SessionSelection=hunt.Id;
   HuntLaunchProof.ClearRequest();
   HuntLaunchProof.Checking=caller.Pointer;
   HuntLaunchProof.Requested(caller,0);
   Check(HuntAccess.Ticket.Armed&&HuntLaunchProof.PendingWalkingRequest,
       "Production request observer arms accepted catalogue contract");
  }
  finally
  {
   HuntLaunchProof.Checking=checking;
   HuntGuildRuntime.SessionSelection=selected;
   CastleGuildRuntime.ContractParty=owner;
   CastleGuildRuntime.Config=config;
   CastleGuildRuntime.Quest=quest;
   if(field!=null)UnityEngine.Object.Destroy(field);
  }
 }

 internal static void HuntTable(BATTLE_DATA table)
 {
  var hunt=HuntRuntime.Definitions.SingleOrDefault(d=>d.Audit.TableId==table.id);
  if(hunt==null)return;
  string id=hunt.Id;
  var setting=HuntRuntime.Settings(hunt);
  bool enabled=setting.Enabled;
  var original=Framework.Singleton<GAME_WORK>.instance;var config=CastleGuildRuntime.Config;var quest=CastleGuildRuntime.Quest;var owner=CastleGuildRuntime.ContractParty;int requests=CastleGuildRuntime.AutoRequests;var selected=HuntGuildRuntime.SessionSelection;bool huntEnabled=HuntRuntime.Config.EnableExperimentalHunt,core=CombatReforgedRuntime.Config.Enabled;string status=CastleGuildRuntime.Status;
  MACHIDAT? fieldData=null;
  try{
   var game=Alloc<GAME_WORK>();game.game_data=Alloc<GAME_DATA>();game.party_data=Alloc<PARTY_DATA>();game.party_data.gold=500;Framework.Singleton<GAME_WORK>.instance=game;
   CastleGuildRuntime.Config=new(){Enabled=true,AutoRequestAcceptedContract=true};CastleGuildRuntime.Quest=new();CastleGuildRuntime.Quest.Accept(id);long count=CastleGuildRuntime.Completions;CastleGuildRuntime.ContractParty=game.party_data.Pointer;HuntGuildRuntime.SessionSelection=id;HuntRuntime.Config.EnableExperimentalHunt=true;CombatReforgedRuntime.Config.Enabled=true;setting.Enabled=true;HuntAccess.Ticket.Cancel();var e=Alloc<EVENTCON>();fieldData=BindWalkingFixture(e,table);
   int area=e.mcon.ano,town=e.mcon.vno;
   Check(MachiLoader.GetMachiID(area,town)==table.id.Split('_')[0],"Exact native map identifier");
   e.mcon.ano=10;e.mcon.vno=6;CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Correct battle table in wrong native field cannot arm quest");e.mcon.ano=area;e.mcon.vno=town;
   e.mcon.econ=Alloc<EVENTCON>();CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Foreign field owner cannot arm quest");e.mcon.econ=e;
   var data=e.mdat;e.mdat=Alloc<MACHIDAT>();CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Detached field data cannot arm quest");e.mdat=data;
   CastleGuildRuntime.WalkingRequest(e,table,false);Check(!HuntAccess.Ticket.Armed,"Scripted/non-walking request cannot auto-arm");
   Check(CastleGuildRuntime.InteractionDifficultySupported,"Normal-mode castle interaction admitted");game.game_data.difficulty=1;Check(!CastleGuildRuntime.InteractionDifficultySupported,"Unexplained difficulty rejected for castle interaction");
   game.game_data.difficulty=2;Check(CastleGuildRuntime.InteractionDifficultySupported,"Hard-mode castle interaction admitted independently of combat eligibility");CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Hard context cannot auto-arm");game.game_data.difficulty=0;
   CastleGuildRuntime.ContractParty=IntPtr.Zero;CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Foreign campaign owner cannot auto-arm");CastleGuildRuntime.ContractParty=game.party_data.Pointer;
   HuntGuildRuntime.SessionSelection=HuntRuntime.Definitions.First(x=>x.Id!=id).Id;CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Different selected contract cannot auto-arm");HuntGuildRuntime.SessionSelection=id;
   var wrong=Alloc<BATTLE_DATA>();wrong.id="unrelated";CastleGuildRuntime.WalkingRequest(e,wrong,true);Check(!HuntAccess.Ticket.Armed,"Unrelated source table cannot auto-arm");
   HuntRuntime.Config.EnableExperimentalHunt=false;CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Explicit global hunt opt-out retained");HuntRuntime.Config.EnableExperimentalHunt=true;
   setting.Enabled=false;CastleGuildRuntime.WalkingRequest(e,table,true);Check(!HuntAccess.Ticket.Armed,"Per-contract opt-out retained");setting.Enabled=true;
   var foreign=new HuntRunProgress{HuntId=HuntRuntime.Definitions.First(x=>x.Id!=id).Id};CastleGuildRuntime.Started(foreign);foreign.Victory();foreign.Finish("VictoryRewardsCompleted",0);CastleGuildRuntime.Finished(foreign);
   Check(CastleGuildRuntime.RunningReceipt==null&&CastleGuildRuntime.Completions==count,"Foreign contract cannot claim victory");
   Check(CastleGuildRuntime.HuntEntryCompatible,"Production walking-hook compatibility is intact");Check(!WarRuntime.Active,"War session is inactive in walking fixture");Check(CastleGuildRuntime.WalkingSceneMatches(e,table,HuntRuntime.Definitions.Single(x=>x.Id==id)),"Exact native field owner/table fixture is valid");
   CastleGuildRuntime.WalkingRequest(e,table,true);Check(HuntAccess.Ticket.Armed,"Accepted contract arms exact original source within walking witness fixture");int once=CastleGuildRuntime.AutoRequests;CastleGuildRuntime.WalkingRequest(e,table,true);Check(CastleGuildRuntime.AutoRequests==once,"Already armed request not duplicated");HuntAccess.Ticket.Cancel();
   var escape=new HuntRunProgress{HuntId=id};CastleGuildRuntime.Started(escape);escape.Finish("ClosedWithoutVictorySignal",0);CastleGuildRuntime.Finished(escape);Check(CastleGuildRuntime.Completions==count&&CastleGuildRuntime.Contract==id,"Escape keeps contract retryable without completion");
   var win=new HuntRunProgress{HuntId=id};CastleGuildRuntime.Started(win);win.Victory();win.Finish("VictoryRewardsCompleted",0);CastleGuildRuntime.Finished(win);CastleGuildRuntime.Finished(win);Check(CastleGuildRuntime.Completions==count+1&&CastleGuildRuntime.Contract==id&&CastleGuildRuntime.Quest.State==GuildQuestState.AwaitingReport,"Owning victory receipt completes once and awaits report");
   Check(!CastleGuildRuntime.Quest.Report(),"Cannot report before native field-resume witness");win.FieldResume();CastleGuildRuntime.FieldReturned(win);Check(CastleGuildRuntime.Quest.Report(),"Synthetic field-resume permits one report without grant");Check(!CastleGuildRuntime.Quest.Report(),"Duplicate report rejected");Check(CastleGuildRuntime.Quest.Accept(id),"Reported contract can be accepted again");CastleGuildRuntime.WalkingRequest(e,table,true);Check(HuntAccess.Ticket.Armed,"Repeat accepted contract can request next eligible native walking selection");Check(game.party_data.gold==500,"Quest completion never adds a second reward");
   CastleGuildRuntime.ClearContract();Check(CastleGuildRuntime.Contract==null&&!HuntAccess.Ticket.Armed,"Withdraw/load invalidates only mod contract and pending request");
   Rows.Add(new{HuntId=id,QuestProductionMethodsExecuted=true,WalkingWitnessSynthetic=true,VictoryReceiptSynthetic=true,ActualQuestCompleted=false,NativeBattleLaunched=false,ExtraPayout=0,HeadquartersMapFromProtectedSave=MachiLoader.GetMachiID(10,6)});
  }finally{setting.Enabled=enabled;if(fieldData!=null)UnityEngine.Object.Destroy(fieldData);Framework.Singleton<GAME_WORK>.instance=original;CastleGuildRuntime.Config=config;CastleGuildRuntime.Quest=quest;CastleGuildRuntime.ContractParty=owner;CastleGuildRuntime.AutoRequests=requests;HuntGuildRuntime.SessionSelection=selected;HuntRuntime.Config.EnableExperimentalHunt=huntEnabled;CombatReforgedRuntime.Config.Enabled=core;CastleGuildRuntime.Status=status;HuntAccess.Ticket.Cancel();}
 }
 internal static object Snapshot()=>new{Passed,Assertions,Rows=Rows.ToArray(),NativeActorConstruction=false,RenderedCastle=false,DialogAccepted=false,RoomReentryAccepted=false};
}
