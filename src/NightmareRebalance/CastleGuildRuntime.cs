using HarmonyLib;
using UnityEngine;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
namespace NightmareRebalance;
internal sealed class CastleGuildOptions { public bool Enabled {get;set;}=false;public bool AutoRequestAcceptedContract {get;set;}=true; }
internal sealed class CastleGuildAudit {public string MapId{get;set;}="";public int Sprite{get;set;} public int ObjectX{get;set;} public int ObjectY{get;set;} public int[] SupportedIndices{get;set;}=Array.Empty<int>();public int[] Persons{get;set;}=Array.Empty<int>();public CastleSubmapAudit[] Submaps{get;set;}=Array.Empty<CastleSubmapAudit>();}
internal sealed class CastleSubmapAudit {public int Index{get;set;} public int MapNo{get;set;} public int Objects{get;set;} public int Scripts{get;set;} public string Fingerprint{get;set;}="";}
// Runtime-owned source clone and modal; no native event bytecode, resident, save or flag edits.
internal static class CastleGuildRuntime
{
 internal static CastleGuildOptions Config=new();internal static CastleGuildAudit Audit=null!;
 internal static MACHICON? Owner;internal static MACHIDAT? Source,Copy;internal static EVENTCON? Field;internal static EVENT_HUMAN? Clerk;
 internal static IntPtr Party;internal static int ObjectIndex=-1,Focus,Insertions,Opens,Accepts,Closes,AutoRequests;internal static bool Modal,ReleaseFence;static bool confirmReleased;static int releaseFrames;static int inputFrame=-1;
 internal static string Status="Castle Guild not visited";internal static string ObservedMap="";internal static int ObservedVariant=-1,ObservedCtyp=-1,RoomInitCalls,EventInitCalls;internal static bool LastSourceMatched;
 internal static void ObserveMap(MACHICON m){if(!Config.Enabled||UnityEngine.Application.isBatchMode)return;ObservedMap=m.GetMapName();ObservedVariant=m.mno;ObservedCtyp=m.ctyp;RoomInitCalls++;}internal static GuildQuest Quest=new();internal static string? Contract=>Quest.ContractId;internal static IntPtr ContractParty;internal static string? RunningReceipt=>Quest.RunningReceipt;internal static long Completions=>Quest.Completions;
 internal static void Initialize(){Audit=ContentRules.Read<CastleGuildAudit>(ContentRuntime.Resource("castle-guild-audit.json"));try{Config=System.Text.Json.JsonSerializer.Deserialize<CastleGuildOptions>(OverhaulRuntime.Load("castle-guild.json"))??new();}catch(Exception e){Config=new();Plugin.Logger.LogWarning("Castle Guild disabled: "+e.Message);}}
 internal static string Fingerprint(MAPEVDAT m)
 {
  var n=new List<int>{m.mapno,m.batlno,m.batltim,m.batlpro,m.mapst,m.okaz,m.evkaz,m.hkaz,m.eventobj?.Length??-1,m.eventdat?.Length??-1};
  if(m.eventobj!=null)foreach(var o in m.eventobj){if(o==null)throw new InvalidOperationException("Null castle object");n.AddRange(new int[]{o.otyp,o.disp,o.spd,o.wt,o.ano,o.ityp,o.fpno,o.ozok,o.pri,o.x,o.y,o.w,o.h});}
  if(m.eventdat!=null)foreach(var e in m.eventdat){if(e?.eventForm==null||e.eventCom==null)throw new InvalidOperationException("Null castle event");n.Add(e.etyp);n.Add(e.eventForm.Length);n.AddRange(e.eventForm.Select(b=>(int)b));n.Add(e.eventCom.Length);n.AddRange(e.eventCom.Select(b=>(int)b));}
  return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(",",n.Select(x=>x.ToString(CultureInfo.InvariantCulture))))));
 }
 internal static bool Matches(MACHIDAT m)=>m!=null&&m.name==Audit.MapId&&MatchesData(m);
 static bool MatchesData(MACHIDAT m)=>m!=null&&m.eventdata?.mapeventdat?.Count==Audit.Submaps.Length&&m.epersondat!=null&&m.epersondat.Select(x=>(int)x.fp_no).SequenceEqual(Audit.Persons)&&m.epersondat.Any(x=>x.fp_no==Audit.Sprite&&x.hito_opd_table!=null&&x.hito_opd_table.id=="vb10_06"&&x.hito_opd_table.path=="person"&&x.hito_opd_table.anime_kazu==4&&x.hito_opd_table.pattern_kazu==12)&&Audit.Submaps.All(a=>Fingerprint(m.eventdata.mapeventdat[a.Index])==a.Fingerprint);
 internal static object? LastSourceEvidence;
 internal static bool RuntimeMatches(MACHICON m,MemoryInfo.MemoryData? info)=>m.mdat!=null&&info?.obj?.TryCast<MACHIDAT>() is {} runtime&&runtime.Pointer==m.mdat.Pointer&&info.path==Audit.MapId&&info.originalAsset?.TryCast<MACHIDAT>() is {} original&&Matches(original)&&MatchesData(runtime);
 internal static MemoryInfo.MemoryData? FindRuntimeInfo(MACHICON m,IEnumerable<MemoryInfo.MemoryData?> candidates){var matches=candidates.Where(x=>RuntimeMatches(m,x)).ToArray();return matches.Length==1?matches[0]:null;}
 internal static MACHIDAT PrepareRuntime(MACHICON m,MemoryInfo.MemoryData? info)=>PrepareCore(m.mdat,m.mno,RuntimeMatches(m,info));
 internal static MACHIDAT Prepare(MACHIDAT source,int index)=>PrepareCore(source,index,Matches(source));
 static MACHIDAT PrepareCore(MACHIDAT source,int index,bool proven)
 {
  if(!proven||!Audit.SupportedIndices.Contains(index))throw new InvalidOperationException("Unverified castle source/variant");
  var copy=UnityEngine.Object.Instantiate(source);copy.name=source.name;
  try{
   var events=new EVENTDAT();events.sce_msg=source.eventdata.sce_msg;events.mapeventdat=new Il2CppSystem.Collections.Generic.List<MAPEVDAT>();
   for(int i=0;i<source.eventdata.mapeventdat.Count;i++){
    var old=source.eventdata.mapeventdat[i];bool add=i==index;
    var map=new MAPEVDAT{mapno=old.mapno,batlno=old.batlno,batltim=old.batltim,batlpro=old.batlpro,mapst=old.mapst,okaz=checked((byte)(old.okaz+(add?1:0))),evkaz=old.evkaz,hkaz=old.hkaz};
    var objects=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<EVENT_OBJ>(old.eventobj.Length+(add?1:0));
    for(int j=0;j<old.eventobj.Length;j++){var o=old.eventobj[j];objects[j]=new EVENT_OBJ{otyp=o.otyp,disp=o.disp,spd=o.spd,wt=o.wt,ano=o.ano,ityp=o.ityp,fpno=o.fpno,ozok=o.ozok,pri=o.pri,x=o.x,y=o.y,w=o.w,h=o.h};}
    if(add)objects[old.eventobj.Length]=new EVENT_OBJ{otyp=1,disp=1,spd=4,wt=255,ano=0,ityp=0,fpno=(ushort)Audit.Sprite,ozok=0,pri=0,x=Audit.ObjectX,y=Audit.ObjectY,w=0,h=0};
    var scripts=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<EVENT_WK>(old.eventdat.Length);for(int j=0;j<old.eventdat.Length;j++){var e=old.eventdat[j];scripts[j]=new EVENT_WK{etyp=e.etyp,eventForm=e.eventForm.ToArray(),eventCom=e.eventCom.ToArray()};}
    map.eventobj=objects;map.eventdat=scripts;events.mapeventdat.Add(map);
   }
   copy.eventdata=events;return copy;
  }catch{UnityEngine.Object.Destroy(copy);throw;}
 }
 internal static bool InteractionDifficultySupported=>DifficultyProvenance.Difficulty(out _,out _);
 internal static bool Compatible=>new[]{AccessTools.Method(typeof(MACHICON),nameof(MACHICON.EventInit)),AccessTools.Method(typeof(EVENTCON),nameof(EVENTCON.EventMain)),AccessTools.Method(typeof(EVENTCON),nameof(EVENTCON.EventEnd))}.All(m=>Harmony.GetPatchInfo(m)?.Owners.All(o=>o==Plugin.Id)!=false);
 internal static void BeforeInit(MACHICON m)
 {
  ObserveQuestOwner(Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer??IntPtr.Zero);
  if(Config.Enabled&&!UnityEngine.Application.isBatchMode){ObserveMap(m);EventInitCalls++;}
  if(!Config.Enabled)return;
  if(!Compatible){Status="Foreign field hooks; insertion skipped";return;}
  if(UnityEngine.Application.isBatchMode){Status="Batch probe; no field insertion";return;}
  if(Combat.State!=null||WarRuntime.Active){Status="Combat or war owner active; insertion skipped";return;}
  if(!InteractionDifficultySupported){Status="Native difficulty provenance unavailable at EventInit";return;}
  if(Owner!=null){Status="Previous native Guild owner has not ended; new insertion skipped";return;}
  if(m?.mdat is not {} source||!m.GetMapName().StartsWith(Audit.MapId+" ",StringComparison.Ordinal)){Status="Outside audited castle tavern";return;}
  var candidates=new[]{MemoryInfo.GetInfo(ADDRESSTYPE.TOWN_1_ADDRESS),MemoryInfo.GetInfo(ADDRESSTYPE.TOWN_2_ADDRESS)};
  var info=FindRuntimeInfo(m,candidates);
  LastSourceMatched=RuntimeMatches(m,info);
  LastSourceEvidence=new{RuntimeName=source.name,CachePath=info?.path,OriginalName=info?.originalAsset?.name,MemoryType=m.memtyp,MatchedTownSlot=info==null?0:Array.FindIndex(candidates,x=>x?.Pointer==info.Pointer)+1,CacheOwnsRuntime=info?.obj?.Pointer==source.Pointer,OriginalMatches=info?.originalAsset?.TryCast<MACHIDAT>() is {} original&&Matches(original),RuntimeDataMatches=MatchesData(source),Variant=m.mno};
  if(EventInitCalls<=32)Plugin.Logger.LogInfo("Castle Guild source: "+System.Text.Json.JsonSerializer.Serialize(LastSourceEvidence));
  if(!Audit.SupportedIndices.Contains(m.mno)||!LastSourceMatched){Status="Castle source or event variant rejected";return;}
  var party=Framework.Singleton<GAME_WORK>.instance?.party_data;if(party==null)return;
  Copy=PrepareRuntime(m,info);Source=source;Owner=m;Party=party.Pointer;ObjectIndex=source.eventdata.mapeventdat[m.mno].okaz;m.mdat=Copy;Status="Castle source cloned; awaiting native actor construction";
 }
 internal static void AfterInit(MACHICON m,EVENTCON e)
 {
  if(Owner?.Pointer!=m.Pointer||Copy==null)return;
  Field=e;if(e?.mdat?.Pointer!=Copy.Pointer||e.mcon?.Pointer!=m.Pointer||e.ehuman==null||ObjectIndex+8>=e.ehuman.Count){Status="Native actor construction not observed; retain clone until EventEnd";return;}
  var h=e.ehuman[ObjectIndex+8];var sprite=Copy.epersondat.Single(x=>x.fp_no==Audit.Sprite).hito_opd_table;
  if(h==null||h.fpno!=Audit.Sprite||h.opd?.Pointer!=sprite.Pointer||h.otyp!=1||h.x.ss.inte!=Audit.ObjectX*8||h.y.ss.inte!=Audit.ObjectY*8){if(h!=null){h.disp=0;h.forceDisable=true;}Status="Native Guild actor signature rejected; retain clone until EventEnd";return;}
  Field=e;Clerk=h;h.disp=1;h.forceHDDisable=false;Insertions++;Status="Guild attendant prepared in castle tavern; rendered visibility unverified";
 }
 internal static bool Owned=>Owner!=null&&Copy!=null&&Field!=null&&Clerk!=null&&Owner.mdat?.Pointer==Copy.Pointer&&Owner.econ?.Pointer==Field.Pointer&&Field.mdat?.Pointer==Copy.Pointer&&Field.mcon?.Pointer==Owner.Pointer&&Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer==Party;
 internal static bool Idle=>Owned&&Config.Enabled&&Compatible&&HuntFieldContext.Active&&Combat.State==null&&!WarRuntime.Active&&Field!.mstep==2&&Owner!.ctyp==0&&!EVENTCON.noScriptRunning&&Field.coml!=null&&Field.coml.All(c=>c==null||c.flg==0)&&!HuntGuildRuntime.PartyMenuOpen&&InteractionDifficultySupported&&WindowManager.instance!=null&&!WindowManager.instance.IsUseMessageWindow&&!WindowManager.instance.IsOpenCommonDialogWindow();
 internal static bool Near
 {
  get{
   if(!Owned||Field!.ehuman.Count==0||Field.ehuman[0] is not {} p||Clerk!.disp!=1)return false;
   int x=p.x.ss.inte,y=p.y.ss.inte;if(Math.Abs(x-Clerk.x.ss.inte)>20||Math.Abs(y-Clerk.y.ss.inte)>20)return false;
   // Do not claim confirm beside another original NPC or native world device.
   for(int i=8;i<Field.ehuman.Count;i++){var h=Field.ehuman[i];if(h==null||h.Pointer==Clerk.Pointer||h.disp!=1||h.forceDisable||(h.otyp!=1&&h.otyp!=3))continue;if(Math.Abs(x-h.x.ss.inte)<=24&&Math.Abs(y-h.y.ss.inte)<=24)return false;}
   return true;
  }
 }
 internal static bool Context=>Modal&&Idle&&Near&&!HuntGuildRuntime.RenderFaulted;
 static bool Press(GRInputManager.Type t)=>GRInputManager.IsInstantiated&&GRInputManager.Instance.keys!=null&&GRInputManager.Instance.keys.TryGetValue(t,out var k)&&k!=null&&k.on;
 static TrainingWar.Drill trainingDrill;
 internal static bool TrainingSelected => Focus==HuntRuntime.Definitions.Length&&Quest.State is not (GuildQuestState.Accepted or GuildQuestState.AwaitingReport)&&TrainingWar.Available;
 internal static TrainingWar.Drill SelectedTraining => trainingDrill switch
 {
  TrainingWar.Drill.FortDefense when TrainingWar.FortDefenseAvailable => trainingDrill,
  TrainingWar.Drill.War3 when TrainingWar.War3Available => trainingDrill,
  TrainingWar.Drill.CommanderRout when TrainingWar.ChallengeAvailable => trainingDrill,
  _ => TrainingWar.Drill.Standard
 };
 static bool Held(GRInputManager.Type t)=>GRInputManager.IsInstantiated&&GRInputManager.Instance.keys!=null&&GRInputManager.Instance.keys.TryGetValue(t,out var k)&&k!=null&&k.press;
 internal static bool Tick(EVENTCON e)
 {
  if(Field?.Pointer!=e.Pointer)return true;
  if(!Idle||!Near){Close(false);return true;}
  if(inputFrame==Time.frameCount&&Modal&&HuntGuildRuntime.RenderReady){e.EventPlayerDisp(false);return false;}inputFrame=Time.frameCount;
  bool decide=Press(GRInputManager.Type.Decide),cancel=Press(GRInputManager.Type.Cancel);
  if(ReleaseFence){if(++releaseFrames>120||(!Held(GRInputManager.Type.Decide)&&!Held(GRInputManager.Type.Cancel))){ReleaseFence=false;return true;}e.EventPlayerDisp(false);return false;}
  if(!Modal){if(!decide)return true;Modal=true;trainingDrill=TrainingWar.Drill.Standard;Focus=Quest.State==GuildQuestState.AwaitingReport?HuntRuntime.Definitions.Length:0;confirmReleased=false;HuntGuildRuntime.Visible=true;HuntGuildRuntime.LastRenderFrame=-1;Opens++;Status="Welcome to the Guild. Which quarry will you pursue?";}
  if(!HuntGuildRuntime.Visible||HuntGuildRuntime.RenderFaulted){Close(false);return true;}
  if(!Held(GRInputManager.Type.Decide))confirmReleased=true;
  Focus=GuildScreenRules.Move(Focus,Press(GRInputManager.Type.Up),Press(GRInputManager.Type.Down),HuntRuntime.Definitions.Length);
  if(TrainingSelected)
  {
   bool left=Press(GRInputManager.Type.Left),right=Press(GRInputManager.Type.Right);
   if(left!=right)trainingDrill=TrainingWar.Cycle(SelectedTraining,right);
  }
  if(cancel){Close(true);e.EventPlayerDisp(false);return false;}
  if(decide&&confirmReleased&&HuntGuildRuntime.RenderReady){
   if(Focus==HuntRuntime.Definitions.Length){
    if(Quest.State==GuildQuestState.AwaitingReport)ReportContract();
    else if(Quest.State==GuildQuestState.Accepted)WithdrawContract();
    else if(TrainingSelected)TrainingWar.Request(SelectedTraining);
    Close(true);e.EventPlayerDisp(false);return false;
   }
   if(AcceptContract(HuntRuntime.Definitions[Focus].Id)){Close(true);e.EventPlayerDisp(false);return false;}
  }
  if(!HuntGuildRuntime.RenderReady)return true; // never invisible input capture
  e.EventPlayerDisp(false);return false;
 }
 internal static void Close(bool fence){if(Modal){Closes++;HuntGuildRuntime.Visible=false;HuntGuildRuntime.LastRenderFrame=-1;}Modal=false;ReleaseFence=fence;releaseFrames=0;}
 internal static void Detach(){Close(false);if(Owner!=null&&Copy!=null&&Owner.mdat?.Pointer==Copy.Pointer)Owner.mdat=Source;if(Copy!=null)UnityEngine.Object.Destroy(Copy);Owner=null;Source=null;Copy=null;Field=null;Clerk=null;Party=IntPtr.Zero;ObjectIndex=-1;}
 internal static string FeedbackText="";static long feedbackUntil;
 internal static bool FeedbackVisible=>Environment.TickCount64<feedbackUntil&&HuntFieldContext.Active&&Combat.State==null&&!WarRuntime.Active&&!HuntGuildRuntime.PartyMenuOpen&&WindowManager.instance!=null&&!WindowManager.instance.IsUseMessageWindow&&!WindowManager.instance.IsOpenCommonDialogWindow();
 static void Notify(string text){if(!HuntSessionRuntime.RealSession)return;FeedbackText=text;feedbackUntil=Environment.TickCount64+6000;}
 internal static string QuestPrompt=>Quest.State switch{GuildQuestState.Accepted=>"Contract: "+ContractName,GuildQuestState.AwaitingReport=>"Hunt complete — report to the Guild",GuildQuestState.Reported=>"Report accepted — contracts available again",_=>"Hunt Guild attendant"};
 internal static string QuestSummary=>Quest.State switch{
  GuildQuestState.Accepted=>"Accepted: "+ContractName+". Seek your quarry in "+GuildScreenRules.Card(Contract??"").Destination+".",
  GuildQuestState.AwaitingReport=>"Completed: "+ContractName+". Return to the Guild to report.",
  GuildQuestState.Reported=>"Well hunted! Your report is recorded. You may take another contract.",
  _=>"No active contract. Choose one to begin."};
 internal static string ContractName=>HuntRuntime.Definitions.FirstOrDefault(x=>x.Id==Contract)?.Name??Contract??"";
 internal static string ActionLabel=>Quest.State==GuildQuestState.AwaitingReport?"Report completion":Quest.State==GuildQuestState.Accepted?"Withdraw contract":TrainingWar.Available?(SelectedTraining switch { TrainingWar.Drill.FortDefense => "Train: Fort Defense", TrainingWar.Drill.War3 => "Train: War 3", TrainingWar.Drill.CommanderRout => "Train: Commander Rout", _ => "Train: War 1" }):"Close";
 internal static bool AcceptContract(string id){
  if(!Context)return false;
  if(Quest.State is GuildQuestState.Accepted or GuildQuestState.AwaitingReport){Status=Quest.State==GuildQuestState.AwaitingReport?"Report the completed hunt first.":"Finish or withdraw your active contract first.";return false;}
  if(!DifficultyProvenance.Difficulty(out int d,out _)||d!=0){Status="Hunt encounters require Normal difficulty. Your difficulty was not changed.";return false;}
  if(!HuntRuntime.Config.EnableExperimentalHunt||!CombatReforgedRuntime.Config.Enabled){Status="Hunt requests are disabled in configuration.";return false;}
  if(!HuntRuntime.Definitions.Any(x=>x.Id==id&&HuntRuntime.Settings(x).Enabled)){Status="This contract is not currently available.";return false;}
  if(!HuntGuildRuntime.Select(id)||!Quest.Accept(id)){Status="The Guild cannot accept this contract right now.";return false;}
  ContractParty=Framework.Singleton<GAME_WORK>.instance!.party_data.Pointer;Accepts++;Status=QuestSummary;Notify(Status);Journal("Accepted");return true;
 }
 internal static bool WithdrawContract(){if(!Context||!Quest.Withdraw())return false;HuntAccess.Ticket.Cancel();Status="Contract withdrawn. Choose another whenever you are ready.";Notify(Status);Journal("Withdrawn");return true;}
 internal static bool ReportContract(){if(!Context||Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer!=ContractParty||!Quest.Report()){Status="Completion is not ready to report.";return false;}HuntAccess.Ticket.Cancel();Status=QuestSummary;Notify(Status);Journal("Reported");return true;}
 internal static void ClearContract(string reason="Contract cleared by native load or session change"){
  bool had=Quest.State!=GuildQuestState.NoContract;Quest=new();ContractParty=IntPtr.Zero;HuntAccess.Ticket.Cancel();
  if(had){Status=reason+". Contracts are session-only.";Notify(Status);Journal("SessionCleared");}
 }
 internal static void ObserveQuestOwner(IntPtr now){if(ContractParty!=IntPtr.Zero&&now!=IntPtr.Zero&&now!=ContractParty)ClearContract("A different party session was loaded");}
 internal static bool HuntEntryCompatible=>new[]{AccessTools.Method(typeof(EVENTCON),nameof(EVENTCON.EventBatlleCheck)),AccessTools.Method(typeof(EVENTCON),nameof(EVENTCON.EventBatlleSet))}.All(m=>Harmony.GetPatchInfo(m)?.Owners.All(o=>o==Plugin.Id)!=false);
 internal static bool WalkingSceneMatches(EVENTCON e,BATTLE_DATA? table,HuntDefinition hunt)=>table!=null&&e?.mcon is {} m&&m.econ?.Pointer==e.Pointer&&e.mdat!=null&&m.mdat?.Pointer==e.mdat.Pointer&&e.mdat.battle_dat!=null&&e.mdat.battle_dat.Any(t=>t?.Pointer==table.Pointer)&&MachiLoader.GetMachiID(m.ano,m.vno)==hunt.Audit.TableId.Split('_')[0];
 internal static int WalkingChecks;internal static object? LastWalkingEvidence;
 internal static void WalkingRequest(EVENTCON e,BATTLE_DATA? table,bool walking)
 {
  if(!walking||Quest.State!=GuildQuestState.Accepted)return;
  WalkingChecks++;
  var hunt=HuntRuntime.Definitions.FirstOrDefault(x=>x.Id==Contract&&HuntRuntime.Settings(x).Enabled);
  bool owner=ContractParty!=IntPtr.Zero&&Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer==ContractParty;
  bool normal=DifficultyProvenance.Difficulty(out int d,out float rate)&&d==0&&rate==1f;
  bool source=hunt!=null&&HuntAccess.SourceMatches(table,hunt);
  bool scene=source&&WalkingSceneMatches(e,table,hunt!);
  bool compatible=HuntEntryCompatible;
  string? nativeField=null;try{if(e.mcon!=null)nativeField=MachiLoader.GetMachiID(e.mcon.ano,e.mcon.vno);}catch{nativeField="Unavailable";}
  string reason=!Config.Enabled||!Config.AutoRequestAcceptedContract?"Automatic contract requests disabled":!owner?"Different live party session":!normal?"Normal difficulty required":HuntGuildRuntime.SelectedId!=Contract?"Selected contract changed":!HuntRuntime.Config.EnableExperimentalHunt||!CombatReforgedRuntime.Config.Enabled?"Required hunt/combat module disabled":!compatible?"Foreign walking hooks":WarRuntime.Active?"War session active":!source?"Unrelated or changed native encounter table":!scene?"Wrong or detached native field owner":"Eligible";
  LastWalkingEvidence=new{Check=WalkingChecks,NativeField=nativeField,NativeTable=table?.id,ExpectedHunt=Contract,Normal=normal,SessionOwner=owner,SourceMatches=source,SceneMatches=scene,HooksCompatible=compatible,Reason=reason};
  if(reason!="Eligible"){Journal("WalkingSkipped");return;}
  if(!HuntAccess.Ticket.Armed){HuntAccess.Ticket.Toggle(true);AutoRequests++;Status="Contract encounter requested through original walking gameplay.";Journal("NativeWalkingRequest");}
 }
 internal static void Started(HuntRunProgress run){if(Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer==ContractParty&&Quest.Started(run.HuntId,run.RunId)){Status="Hunt in progress. Native battle outcomes and rewards remain authoritative.";Journal("NativeHuntStarted");}}
 internal static void Finished(HuntRunProgress run){
  if(Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer!=ContractParty)return;
  bool owned=run.RunId==RunningReceipt;bool completed=Quest.Finished(run);
  if(completed){HuntAccess.Ticket.Cancel();Status=QuestSummary;Journal("VictoryAwaitingReport");}
  else if(owned&&run.Finished){Status="No completed victory. Your contract remains retryable.";Journal("NonVictoryRetryable");}
 }
 internal static void FieldReturned(HuntRunProgress run){if(Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer==ContractParty&&Quest.FieldReturned(run)){Status=QuestSummary;Notify("Quarry defeated! Return to the Guild with your report.");Journal("NativeFieldResumed");}}
 static void Journal(string stage){
  if(!HuntSessionRuntime.RealSession)return;
  try{string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","guild");Directory.CreateDirectory(dir);var row=new{Version="1.13.0-native-war-foundation.1",Utc=DateTime.UtcNow,Stage=stage,State=Quest.State.ToString(),Contract,Quest.RunningReceipt,Quest.CompletedReceipt,Quest.Completions,Quest.Reports,Quest.FieldReturnObserved,AutoRequests,LastWalkingEvidence,Status,DiagnosticsOnly=true,Hydrated=false,CampaignIdentityImplemented=false,RewardsGrantedByReporting=0};DiagnosticHistoryStore.Append(Path.Combine(dir,"castle-quest-history.json"),System.Text.Json.JsonSerializer.Serialize(row));Plugin.Logger.LogInfo("Castle quest: "+stage+"; "+Quest.State+"; "+Contract);}
  catch(Exception e){Plugin.Logger.LogWarning("Castle quest diagnostic unavailable: "+e.Message);}
 }
 internal static object Snapshot()=>new{Config.Enabled,Compatible,HuntEntryCompatible,WalkingChecks,LastWalkingEvidence,NativeDifficulty=Framework.Singleton<GAME_WORK>.instance?.game_data?.difficulty,DifficultyProven=DifficultyProvenance.Difficulty(out _,out _),BattleOwnerPresent=Combat.State!=null,BatchMode=Application.isBatchMode,ObservedMap,ObservedVariant,ObservedCtyp,RoomInitCalls,EventInitCalls,LastSourceMatched,LastSourceEvidence,FieldStep=Field?.mstep,ActorCount=Field?.ehuman?.Count,NativeScriptDispatchDisabled=EVENTCON.noScriptRunning,NativeScriptsIdle=!EVENTCON.noScriptRunning&&Field?.coml!=null&&Field.coml.All(c=>c==null||c.flg==0),ActiveCommands=Field?.coml?.Count(c=>c!=null&&c.flg!=0),Config.AutoRequestAcceptedContract,Owned,Idle,Near,Modal,ReleaseFence,Status,Insertions,Opens,Accepts,Closes,Focus,AutoRequests,QuestState=Quest.State.ToString(),QuestSummary,Contract,RunningReceipt,Quest.CompletedReceipt,Completions,Quest.Reports,Quest.FieldReturnObserved,ObjectIndex,SourceMap=Source?.name,Attendant=Clerk==null?null:new{Clerk.disp,Clerk.forceDisable,Clerk.forceHDDisable,X=Clerk.x.ss.inte,Y=Clerk.y.ss.inte,Sprite=Clerk.fpno},Player=Owned&&Field!.ehuman.Count>0?new{X=Field.ehuman[0].x.ss.inte,Y=Field.ehuman[0].y.ss.inte}:null,NativeObjectConstructionObserved=Clerk!=null,RenderedNpcAccepted=false,FullQuestAccepted=false,QuestPersistence="Session only; actual native load/restart clears contract; saving does not persist it; no per-save identity store",RewardsGrantedByCastleModule=0};
}
[HarmonyPatch(typeof(MACHICON),nameof(MACHICON.EventInit))]
internal static class CastleGuildInit
{
 [HarmonyPrefix]static void Prefix(MACHICON __instance){try{CastleGuildRuntime.BeforeInit(__instance);}catch(Exception e){CastleGuildRuntime.Detach();Plugin.Logger.LogWarning("Castle Guild preparation skipped: "+e);}}
 [HarmonyPostfix]static void Postfix(MACHICON __instance,EVENTCON __result){try{CastleGuildRuntime.AfterInit(__instance,__result);}catch(Exception e){CastleGuildRuntime.Field=__result;CastleGuildRuntime.Clerk=null;CastleGuildRuntime.Close(false);Plugin.Logger.LogWarning("Castle Guild actor rejected; source clone retained until native end: "+e);}}
 [HarmonyFinalizer]static Exception? Finalizer(Exception? __exception){if(__exception!=null)CastleGuildRuntime.Detach();return __exception;}
}
[HarmonyPatch(typeof(EVENTCON),nameof(EVENTCON.EventMain))]
internal static class CastleGuildFieldInput
{
 [HarmonyPrefix]static bool Prefix(EVENTCON __instance){try{return CastleGuildRuntime.Tick(__instance);}catch(Exception e){CastleGuildRuntime.Close(false);Plugin.Logger.LogWarning("Castle Guild released field input: "+e);return true;}}
}
[HarmonyPatch(typeof(EVENTCON),nameof(EVENTCON.EventEnd))]
internal static class CastleGuildFieldEnd{[HarmonyPostfix]static void Postfix(EVENTCON __instance){if(CastleGuildRuntime.Field?.Pointer==__instance.Pointer)CastleGuildRuntime.Detach();}}

[HarmonyPatch(typeof(GSD2SaveData),nameof(GSD2SaveData.LoadDataCore))]
internal static class CastleGuildOriginalLoad{[HarmonyPrefix]static void Prefix(){CastleGuildRuntime.Close(false);CastleGuildRuntime.ClearContract();}}

[HarmonyPatch(typeof(MACHICON),"OnMapInit")]
internal static class CastleGuildRoomWitness{[HarmonyPostfix]static void Postfix(MACHICON __instance){try{CastleGuildRuntime.ObserveMap(__instance);}catch(Exception e){Plugin.Debug("Castle room observation unavailable: "+e.Message);}}}
