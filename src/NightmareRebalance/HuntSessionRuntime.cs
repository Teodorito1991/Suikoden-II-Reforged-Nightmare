using System.Text.Json;
namespace NightmareRebalance;
internal static class HuntSessionRuntime
{
 internal static HuntRunProgress? Current,Last;
 internal static bool RealSession=>!UnityEngine.Application.isBatchMode&&!CombatReforgedRuntime.Probe&&!DifficultyProvenance.StrictProbe&&!Plugin.Options.RunCombatSelfTest.Value;
 internal static void Begin()
 {
  if(!RealSession)return;
  try{if(Current is{Finished:false})Finish("ReplacedWithoutCompletion",0);Persist(Last);Current=new(){Build="1.13.0-native-war-foundation.1",Entry=HuntAccess.Selections,HuntId=HuntRuntime.Active?.Id??"",TableId=HuntRuntime.Active?.Audit.TableId??""};Current.DebugArmed(HuntDebug.Enabled&&HuntDebug.Armed);CastleGuildRuntime.Started(Current);}catch(Exception e){Warn(e);}
 }
 internal static void Finish(string outcome,int charges)
 {if(!RealSession||Current==null)return;Current.Finish(outcome,charges);CastleGuildRuntime.Finished(Current);Last=Current;Current=null;}
 internal static void FieldUpdate()
 {
  if(!RealSession||Combat.State!=null||Last==null)return;
  try{if(Last.FieldResume()){CastleGuildRuntime.FieldReturned(Last);Plugin.Logger.LogInfo("Hunt field chapter resumed: "+Last.RunId+"; native map update observed, player controls not inferred.");Persist(Last);}}catch(Exception e){Warn(e);}
 }
 static void Persist(HuntRunProgress? run)
 {
  if(run==null||run.JournalWritten||!RealSession)return;
  try{string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);DiagnosticHistoryStore.Append(Path.Combine(dir,"hunt-history.json"),JsonSerializer.Serialize(run));run.JournalWritten=true;}catch(Exception e){Warn(e);}
 }
 static void Warn(Exception e)=>Plugin.Logger.LogWarning("Hunt session observation unavailable: "+e.Message);
 internal static object Snapshot()=>new{Current,Last,HistoricalAcceptance=new{Build="1.5.2-hunt-debug.2",Sha256="C56E5695FB15F4372A83817BB9EB8C5F3DB92F84F5AC3CD8E1EA86585CDC84A8",CompletedVictories=1,Entries=2,ActualPotchCredit=3478,UserConfirmedFieldControl=true,DebugArmed=true,Source="V152-MANUAL-HUNT-MILESTONE.md",HistoricalEvidenceIsNotCurrentBuildVerification=true},Journal="combat/hunt-history.json; last12 closed hunts; field tick is not proof of player control"};
}
