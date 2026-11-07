using HarmonyLib;
using UnityEngine;
namespace NightmareRebalance;
// Read-only, gated by the existing optional witness. No chapter requests or field writes.
internal static class NativeWarBoundary
{
 internal static long Epoch;internal static WarFieldLease? Field,Entry;internal static string Status="Observer disabled";
 internal static int Captures,Invalidations,OriginalRequests;static int frame=-1;
 internal static void Invalidate(string reason){Epoch++;Field=null;Entry=null;frame=-1;Invalidations++;Status=reason;}
 internal static void LeavingMap(){if(!NativeWarWitness.Live)return;Field=null;frame=-1;Status="Original field chapter ended; resource retention and return still unproven";}
 internal static void MapTick(MapChapter chapter)
 {
  if(!NativeWarWitness.Live)return;
  try{Field=Capture(chapter);frame=Time.frameCount;if(Field!=null){Captures=Math.Min(1000000,Captures+1);Status=Entry==null?"Genuine idle field observed":WarBoundaryRules.Compare(Entry,Field);}else Status="Field prerequisites unavailable";}
  catch{Field=null;frame=-1;Status="Field capture failed closed";}
 }
 internal static bool OwnersMatch(GAME_WORK? g,MACHICON? m,EVENTCON? e)=>g?.sys_work!=null&&g.party_data!=null&&g.game_data!=null&&m!=null&&e!=null&&m.mdat!=null&&g.sys_work.mcon?.Pointer==m.Pointer&&m.econ?.Pointer==e.Pointer&&e.mcon?.Pointer==m.Pointer&&e.mdat?.Pointer==m.mdat.Pointer;
 internal static WarFieldLease? Capture(MapChapter chapter)
 {
  var g=Framework.Singleton<GAME_WORK>.instance;var sys=g?.sys_work;var m=sys?.mcon;var e=m?.econ;var manager=GRChapterManager.GRInstance;
  if(g==null||sys==null||m==null||e==null||g.party_data==null||g.game_data==null||m.mdat==null||manager==null||
   !OwnersMatch(g,m,e)||manager.activeChapter?.Pointer!=chapter.Pointer||manager.RequestType!=null||e.mcon?.Pointer!=m.Pointer||e.mdat?.Pointer!=m.mdat.Pointer||
   e.mstep!=2||m.ctyp!=0||EVENTCON.noScriptRunning||e.coml==null||e.coml.Any(c=>c!=null&&c.flg!=0)||sys.now_save!=0||sys.g_bInBattle||
   Combat.State!=null||WarRuntime.Active||war_h.g_w_war_no!=0||HuntGuildRuntime.PartyMenuOpen||CastleGuildRuntime.Modal||
   !DifficultyProvenance.Difficulty(out int difficulty,out _)||difficulty!=0||WindowManager.instance==null||
   WindowManager.instance.IsUseMessageWindow||WindowManager.instance.IsOpenCommonDialogWindow())return null;
  var hash=NativeWarWitness.Capture()?.FlagsHash;if(hash==null)return null;
  return new(Epoch,g.Pointer.ToInt64(),sys.Pointer.ToInt64(),g.party_data.Pointer.ToInt64(),g.game_data.Pointer.ToInt64(),m.Pointer.ToInt64(),e.Pointer.ToInt64(),m.mdat.Pointer.ToInt64(),chapter.Pointer.ToInt64(),m.ano,m.vno,m.mno,e.px,e.py,hash);
 }
 internal static void EventBefore(EVENTCON e)
 {try{EventBeforeCore(e);}catch{Entry=null;Status="Native request witness failed closed";}}
 static void EventBeforeCore(EVENTCON e)
 {
  if(!NativeWarWitness.Live)return;
  // This sees a legitimate native command only. Never consume/advance its bytecode.
  bool recent=frame>=0&&Time.frameCount>=frame&&Time.frameCount-frame<=2;
  if(!recent||Field==null||Field.Epoch!=Epoch||e.Pointer.ToInt64()!=Field.Event){Entry=null;Status="Original request has no recent matching idle-field lease";return;}
  var g=Framework.Singleton<GAME_WORK>.instance;
  if(!OwnersMatch(g,e.mcon,e)||g!.Pointer.ToInt64()!=Field.Game||g.sys_work.Pointer.ToInt64()!=Field.System||g.party_data.Pointer.ToInt64()!=Field.Party||g.game_data.Pointer.ToInt64()!=Field.Data||NativeWarWitness.Capture()?.FlagsHash!=Field.Flags){Entry=null;Status="Request owner changed since idle field observation";return;}
  Entry=Field;OriginalRequests++;Status="Original campaign request witnessed; NOT an independent skirmish";
 }
 internal static object Snapshot()=>new{Enabled=NativeWarWitness.Live,Epoch,Status,Captures,Invalidations,OriginalRequests,Field,Entry,
  LauncherEnabled=WarBoundaryRules.CanLaunch,CampaignIsolationImplemented=false,ReturnControlProven=false,EarnedSkirmishCompletions=0,CommanderXpGranted=0,
  Scope="Read-only session pointers, five flag arrays and idle field ownership; not complete save isolation or stable campaign identity"};
}
