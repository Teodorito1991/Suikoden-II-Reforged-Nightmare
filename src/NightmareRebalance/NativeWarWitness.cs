using HarmonyLib;
using System.Security.Cryptography;
using System.Text.Json;
namespace NightmareRebalance;
// Observes original campaign calls only. No launch, callback suppression, save edits or XP grants.
internal static class NativeWarWitness
{
 internal static NativeWarWitnessFile Config=new(){Enabled=false};internal static NativeWarEvidence? Current,Last;
 static long requestOwner;static int requestWar,requestFrame;static bool warned;
 internal static bool Live=>Config.Enabled&&!UnityEngine.Application.isBatchMode&&!Plugin.Options.RunNativeWarSelfTest.Value;
 internal static void Initialize(){try{Config=NativeWarEvidenceRules.Parse(OverhaulRuntime.Load("native-war-witness.json"));}catch(Exception e){Config.Enabled=false;Plugin.Logger.LogWarning("Native war witness disabled: "+e.Message);}}
 internal static NativeWarFrame? Capture()
 {
  try{var g=Framework.Singleton<GAME_WORK>.instance;if(g==null)return null;var arrays=new[]{g.event_flag,g.chara_flag,g.town_flag,g.scpoi_flag,g.t_box_flag};
   bool valid=arrays.All(a=>a!=null&&a.Length<=4096);int total=valid?arrays.Sum(a=>a.Length):0;string? hash=null;
   if(valid){using var stream=new MemoryStream();using(var writer=new BinaryWriter(stream,System.Text.Encoding.UTF8,true))foreach(var a in arrays){writer.Write(a.Length);writer.Write(a.ToArray());}hash=Convert.ToHexString(SHA256.HashData(stream.ToArray()));}
   return new(g.Pointer.ToInt64(),hash,total,g.game_data?.area_no,g.game_data?.town_no,g.game_data?.map_no);
  }catch{return null;}
 }
 static void Safe(Action action){try{action();}catch(Exception e){if(!warned){warned=true;Plugin.Logger.LogWarning("Read-only war witness unavailable: "+e.Message);}}}
 internal static void EventRequest(){if(!Live)return;Safe(()=>{var g=Framework.Singleton<GAME_WORK>.instance;if(g?.sys_work is not {} sys||sys.war_no is <1 or >16)return;requestOwner=g.Pointer.ToInt64();requestWar=sys.war_no;requestFrame=UnityEngine.Time.frameCount;});}
 internal static void Start(){if(!Live)return;Safe(()=>{var f=Capture();int war=Framework.Singleton<GAME_WORK>.instance?.sys_work?.war_no??0;Current=new(){War=war,Entry=f,OriginalEventObserved=NativeWarEvidence.RecentRequest(requestOwner,requestWar,requestFrame,f?.Owner??0,war,UnityEngine.Time.frameCount)};Current.Event("OriginalWarChapterStart",f,f);requestOwner=0;});}
 internal static NativeWarFrame? Before()=>Live&&Current is{ChapterDestroyed:false}?Capture():null;
 internal static void Observe(string stage,NativeWarFrame? before,int? value=null){if(!Live||Current==null)return;Safe(()=>Current.Event(stage,before,Capture(),value));}
 internal static void BattleInit(){if(!Live||Current==null)return;Current.War=war_h.g_w_war_no;Current.BattleInitialized=true;}
 internal static void Result(int result){if(!Live||Current==null||result!=-1||Current.Ending!=null)return;Safe(()=>{Current.Ending=war_sys.end_event_no;Current.Event("NativeEndingSelected",Capture(),Capture(),Current.Ending);});}
 internal static void EndingStart(ushort number){if(Live&&Current?.Ending==number)Current.EndingStarted=true;}
 internal static void EndingFinish(int result){if(Live&&Current is{EndingStarted:true}&&result==-1)Current.EndingFinished=true;}
 internal static void Finish(NativeWarFrame? before,int war){if(!Live||Current?.War!=war)return;Current.FinishCallbackReturned=true;Observe("OriginalFinishCallbackReturned",before,war);}
 internal static void Destroy(){if(!Live||Current==null)return;Safe(()=>{Current.ChapterDestroyed=true;Current.Event("OriginalWarChapterDestroyed",Current.Entry,Capture());Last=Current;Persist("ChapterDestroyed");});}
 internal static void MapTick(){if(!Live||Current is not{ChapterDestroyed:true,Invalidated:null,MapUpdates:<30})return;Safe(()=>{if(Current.MapTick(Capture())){Current.Event("ThirtyMapUpdates",Current.Entry,Capture());Persist("MapUpdatesObserved");}});}
 internal static void SaveRequest(){if(Live&&Current is{} c&&c.Invalidated==null&&c.MapUpdates<30&&c.SaveRequests<256)c.SaveRequests++;}
 internal static void Invalidate(){requestOwner=0;if(!Live||Current==null)return;Safe(()=>{Current.Invalidated="OriginalSaveLoadStarted";Current.Event("OriginalSaveLoadStarted",Current.Entry,Capture());Last=Current;Persist("InvalidatedByLoad");Current=null;});}
 static void Persist(string kind){if(Current==null||!Live)return;string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","native-war");Directory.CreateDirectory(dir);string record=JsonSerializer.Serialize(new{Version="1.17.0-native-skirmish-session.1",Utc=DateTime.UtcNow,SnapshotKind=kind,Evidence=Current});DiagnosticHistoryStore.Append(Path.Combine(dir,"war-lifecycle-history.json"),record);}
 internal static object Snapshot()=>new{Config.Enabled,Current,Last,IndependentSkirmishImplemented=false,SkirmishLaunchExposed=false,TrainingProgressionImplemented=false,TrainingAwardsIssued=0,SaveFilesWritten=false,History="12 bounded stage snapshots; no training receipts",HashScope="Five flag arrays, not all campaign fields; map updates do not prove field control"};
}
[HarmonyPatch(typeof(EVENTCON),"ECWarEventGo")]
internal static class WarWitnessEvent{[HarmonyPrefix]static void Prefix(EVENTCON __instance)=>NativeWarBoundary.EventBefore(__instance);[HarmonyPostfix]static void Postfix()=>NativeWarWitness.EventRequest();}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.Start))]
internal static class WarWitnessStart{[HarmonyPrefix]static void Prefix()=>NativeWarWitness.Start();}
[HarmonyPatch(typeof(w_battle),nameof(w_battle.InitBattle))]
internal static class WarWitnessInit{[HarmonyPostfix]static void Postfix()=>NativeWarWitness.BattleInit();}
// Ending observation is invoked by TotalWarOutcome AFTER its final result
// adjustment; an independent same-priority postfix could see the earlier result.
[HarmonyPatch(typeof(w_demo),nameof(w_demo.InitWarDemo))]
internal static class WarWitnessEndingStart{[HarmonyPostfix]static void Postfix(ushort event_no)=>NativeWarWitness.EndingStart(event_no);}
[HarmonyPatch(typeof(w_demo),nameof(w_demo.DriveWarDemo))]
internal static class WarWitnessEndingEnd{[HarmonyPostfix]static void Postfix(int __result)=>NativeWarWitness.EndingFinish(__result);}
[HarmonyPatch(typeof(w_setflg),nameof(w_setflg.w_foock_finish_work))]
internal static class WarWitnessFinish
{[HarmonyPrefix]static void Prefix(out NativeWarFrame? __state)=>__state=NativeWarWitness.Before();[HarmonyPostfix]static void Postfix(int war_no,NativeWarFrame? __state)=>NativeWarWitness.Finish(__state,war_no);}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.Update))]
internal static class WarWitnessCompletion
{[HarmonyPrefix]static void Prefix(WarChapter __instance,out NativeWarFrame? __state)=>__state=NativeWarWitness.Live&&__instance.isResLoaded&&__instance._warinit==null?NativeWarWitness.Before():null;[HarmonyPostfix]static void Postfix(int __result,NativeWarFrame? __state){if(__state!=null)NativeWarWitness.Observe("OriginalChapterCompletionBranch",__state,__result);}}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.OnDestroy))]
internal static class WarWitnessDestroy{[HarmonyPostfix]static void Postfix()=>NativeWarWitness.Destroy();}
[HarmonyPatch(typeof(MapChapter),nameof(MapChapter.Update))]
internal static class WarWitnessMap{[HarmonyPostfix]static void Postfix(MapChapter __instance){NativeWarWitness.MapTick();NativeWarBoundary.MapTick(__instance);}}
[HarmonyPatch(typeof(GSD2SaveData),nameof(GSD2SaveData.LoadDataCore))]
internal static class WarWitnessLoad{[HarmonyPrefix]static void Prefix(){NativeWarBoundary.Invalidate("Original load started");NativeWarWitness.Invalidate();}}
[HarmonyPatch(typeof(GSD2SaveData),nameof(GSD2SaveData.SaveFile))]
internal static class WarWitnessSave{[HarmonyPrefix]static void Prefix()=>NativeWarWitness.SaveRequest();}

[HarmonyPatch(typeof(MapChapter),nameof(MapChapter.OnDestroy))]
internal static class WarBoundaryMapEnd{[HarmonyPrefix]static void Prefix(){NativeWarBoundary.LeavingMap();}}
