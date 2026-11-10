using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Security.Cryptography;
using System.Text.Json;
namespace NightmareRebalance;
internal sealed class WarReturnEvidence
{
    public int War {get;set;}
    public int? NativeEnding {get;set;}
    public string Outcome {get;set;}="None";
    public bool EndingStarted {get;set;}
    public bool EndingFinished {get;set;}
    public bool FinishCallbackReturned {get;set;}
    public bool ChapterDestroyed {get;set;}
    public int MapUpdatesAfterEnding {get;set;}
    public bool SaveReloadVerified=>false;
}
internal static class WarTacticsRuntime
{
    internal static WarTacticsFile Config=new();
    internal static readonly HashSet<int> Verified=new(),Routed=new();
    internal static readonly Dictionary<int,int> Focus=new(),Commitments=new();
    internal static readonly Dictionary<int,(int x,int y,int priorX,int priorY)> History=new();
    internal static readonly List<object> ScoreTrace=new(),CommandTrace=new();
    internal static bool Armed,Ready,Probe,AiFaulted;
    internal static int War,ScoreChanges,SelectedChanges,Generation;
    internal static long PhaseTurn=-1;
    internal static WarReturnEvidence Evidence=new();
    internal static void Initialize()
    {
        try
        {
            Config=WarTacticsRules.Parse(OverhaulRuntime.Load("war-tactics.json"));
            foreach(int n in WarTacticsRules.Supported)if(Audit(n))Verified.Add(n);
            Plugin.Logger.LogInfo("War Tactics audited native definitions: "+string.Join(",",Verified.OrderBy(n=>n))+". Staged war 10/15 outcomes and controls remain native.");
        }
        catch(Exception e){Config=new();Verified.Clear();Plugin.Logger.LogError("War Tactics disabled: "+e.Message);}
    }
    internal static bool Audit(int war)
    {
        using var d=JsonDocument.Parse(ContentRuntime.Resource("tactics-audit.json"));
        if(!d.RootElement.TryGetProperty(war.ToString(),out var row)||war_h.war2map[war]!=row.GetProperty("Map").GetInt32())return false;
        var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+war)!;
        foreach(var kv in row.GetProperty("Scripts").EnumerateObject())
        {
            var b=type.GetProperty(kv.Name)?.GetValue(null) as Il2CppStructArray<byte>;
            if(b==null||Convert.ToHexString(SHA256.HashData(b.ToArray()))!=kv.Value.GetString())return false;
        }
        var units=(Il2CppReferenceArray<Il2CppReferenceArray<Il2CppStructArray<sbyte>>>)type.GetProperty("unit_init_data_"+war)!.GetValue(null)!;
        return units.Length==2&&units.All(s=>s.Length==15)&&Convert.ToHexString(SHA256.HashData(units.SelectMany(s=>s.SelectMany(u=>u.Select(v=>unchecked((byte)v)))).ToArray()))==row.GetProperty("Roster").GetString();
    }
    internal static void Arm(){Clear();if(TrainingWar.Active)return;Armed=true;}
    internal static void Begin()
    {
        Ready=false;AiFaulted=false;War=war_h.g_w_war_no;Generation++;Routed.Clear();Focus.Clear();Commitments.Clear();History.Clear();PhaseTurn=-1;ScoreChanges=SelectedChanges=0;ScoreTrace.Clear();CommandTrace.Clear();
        if(!Armed||!Verified.Contains(War)||!Audit(War)||war_h.g_w_map_no!=war_h.war2map[War])return;
        var rows=w_unit.unit;var source=TotalWarRuntime.Characters(War);
        if(rows?.Length!=2||rows[0]?.Length!=15||rows[1]?.Length!=15||source==null)return;
        var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+War)!;
        var start=(Il2CppReferenceArray<Il2CppReferenceArray<Il2CppStructArray<sbyte>>>)type.GetProperty("unit_init_data_"+War)!.GetValue(null)!;
        if(w_unit.GetUnitChara(1,0,0)!=start[1][0][3])return;
        Ready=true;Evidence=new(){War=War};
        if(!Probe)Plugin.Logger.LogInfo($"War Tactics {War} READY: native map {war_h.g_w_map_no}; tactical AI={AiAllowed(War)}; no launch/story edits.");
    }
    internal static bool AiAllowed(int war)=>Ready&&Armed&&!AiFaulted&&War==war&&Config.Enabled&&Config.EnabledWars.Contains(war)&&Plugin.Options.EnableWarTactics.Value&&Plugin.Options.EnableNativeWarAI.Value&&(war==2?War2Runtime.Policy is {EnemyAi:true}:war is 3 or 6 or 13?TotalWarRuntime.Rules.Scenarios.Any(p=>p.WarNumber==war&&p.EnemyAi):true);
    internal static void Clear(){if(Armed)Evidence.ChapterDestroyed=true;Armed=Ready=false;Routed.Clear();Focus.Clear();Commitments.Clear();History.Clear();PhaseTurn=-1;}
    internal static void Capture()
    {
        if(!Ready||War!=war_h.g_w_war_no)return;
        int max=War==3?7:War==6?11:War==13?14:0;
        for(int i=1;i<=max;i++)
        {
            var u=w_unit.unit[1][i];if(u is not {flag:2})continue;
            int leader=w_unit.GetUnitChara(1,i,0);if(leader>=0&&leader<(TotalWarRuntime.Characters(War)?.Length??0)&&w_chara.GetCharaZenmetu(leader)==0)Routed.Add(i);
        }
    }
    internal static bool TacticalObjective()
    {
        var p=Config.Objectives.FirstOrDefault(p=>p.WarNumber==War);
        if(!Ready||!Config.Enabled||!Plugin.Options.EnableWarTactics.Value||p==null||!TotalWarRuntime.Ready||TotalWarRuntime.Policy is not {Enabled:true}||!Plugin.Options.EnableNativeWarRemix.Value)return false;
        Capture();var hero=w_unit.unit[0][0];var enemy=w_unit.unit[1][0];
        return WarTacticsRules.Objective(p,War,war_sys.turn,Routed.Count,hero.flag==1,enemy.flag==1,hero.x,hero.y);
    }
    internal static int Score(int slot,TACTICS_RESULT_TYPE c,int before,int originalNative,WarAiRuntime.Scope scope)
    {
        if(AiFaulted)return before;
        try{return ScoreCore(slot,c,before,originalNative,scope);}
        catch(Exception e){AiFaulted=true;Plugin.Logger.LogWarning("War Tactics AI disabled for this battle after inspection failure: "+e.Message);return before;}
    }
    static int ScoreCore(int slot,TACTICS_RESULT_TYPE c,int before,int originalNative,WarAiRuntime.Scope scope)
    {
        if(!AiAllowed(war_h.g_w_war_no)||c.act>2||!NativeTacticsBoard.Cell(c.tx,c.ty)||w_unit.unit[1][slot] is not {flag:1} u||u.chara==null||u.chara.Length!=3||u.chara[0]<0)return before;
        if(PhaseTurn!=war_sys.turn){PhaseTurn=war_sys.turn;Focus.Clear();Commitments.Clear();}
        scope.Board??=NativeTacticsBoard.Build(slot);var b=scope.Board;
        bool repeated=History.TryGetValue(slot,out var last)&&c.tx==last.priorX&&c.ty==last.priorY;
        int focus=c.act==0?Focus.GetValueOrDefault(c.target):0;
        int chance=100;
        if(c.act==0){if(c.target>=15||w_unit.unit[0][c.target] is not {flag:1})return before;chance=w_kekka.CalcAttackHitRate(1,(byte)slot,0,c.target);}
        if(c.act==2&&(c.tokusyu_chara>=3||c.tokusyu_nou>=3||w_unit.GetUnitTokusyuKaisu(1,slot,c.tokusyu_chara,c.tokusyu_nou)<=0))return before;
        var facts=b.Facts(c,focus,repeated);
        int after=WarTacticsRules.Score(before,Config,b.Actor,c.act,chance,facts);
        int from=b.GoalCosts[b.Actor.Y*16+b.Actor.X],to=b.GoalCosts[c.ty*16+c.tx];
        if(c.act==1&&from<10000&&to<10000)after=(int)Math.Clamp((long)after+Math.Clamp(from-to,-3,3)*Config.FocusWeight,-30000,30000);
        var evidence=new CandidateEvidence(c.Pointer.ToInt64(),c.act,c.tx,c.ty,c.target,before,after);
        if(scope.TacticalCandidates.Count<2048)scope.TacticalCandidates.Add(evidence);else scope.Overflow=true;
        int existing=scope.Candidates.FindIndex(e=>e.Pointer==evidence.Pointer);
        var native=evidence with{Before=originalNative};
        if(existing>=0)scope.Candidates[existing]=native;else if(scope.Candidates.Count<2048)scope.Candidates.Add(native);else scope.Overflow=true;
        if(after!=before&&!Probe)
        {ScoreChanges++;if(ScoreTrace.Count<16)ScoreTrace.Add(new{Turn=war_sys.turn,Unit=slot,Action=c.act,X=c.tx,Y=c.ty,Before=before,After=after,Threat=facts.Threat,Support=facts.CoveredSupport,Focus=facts.Focus,NativePathFrom=from,NativePathTo=to});}
        return after;
    }
    internal static void Selected(WarAiRuntime.Scope? scope)
    {
        if(scope==null||!AiAllowed(war_h.g_w_war_no)||w_tact.selected_result==null)return;
        var c=w_tact.selected_result;
        if(!scope.Overflow&&scope.TacticalCandidates.Count>0)
        {
            var chosen=scope.TacticalCandidates.FirstOrDefault(e=>e.Pointer==c.Pointer.ToInt64());
            if(chosen!=null&&chosen.Before<scope.TacticalCandidates.Max(e=>e.Before)&&!Probe)
            {SelectedChanges++;if(CommandTrace.Count<32)CommandTrace.Add(new{Turn=war_sys.turn,Unit=scope.Unit,Chosen=chosen,BestPreviousScore=scope.TacticalCandidates.Max(e=>e.Before),StrictSelectionChanged=true});}
        }
        if(Commitments.Remove(scope.Unit,out int previous))Focus[previous]=Math.Max(0,Focus.GetValueOrDefault(previous)-1);
        if(c.act==0&&c.target<15){Focus[c.target]=Focus.GetValueOrDefault(c.target)+1;Commitments[scope.Unit]=c.target;}
        var u=w_unit.unit[1][scope.Unit];var old=History.GetValueOrDefault(scope.Unit,(u.x,u.y,u.x,u.y));
        History[scope.Unit]=(c.tx,c.ty,old.x,old.y);
    }
    internal static void ObserveResult(int result)
    {
        if(!Ready||result!=-1||Evidence.NativeEnding!=null)return;
        Evidence.NativeEnding=war_sys.end_event_no;Evidence.Outcome=War==2?War2Runtime.Outcome.ToString():TotalWarRuntime.Result;
    }
    internal static object Snapshot()=>new{Ready,War,Generation,ScoreChanges,SelectedChanges,EligibleRouts=Routed.OrderBy(n=>n).ToArray(),Objective=Config.Objectives.FirstOrDefault(p=>p.WarNumber==War),ScoreTrace=ScoreTrace.ToArray(),CommandTrace=CommandTrace.ToArray(),Evidence,VerifiedWars=Verified.OrderBy(n=>n).ToArray()};
}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.Start))]
internal static class WarTacticsArm{[HarmonyPrefix]static void Prefix()=>WarTacticsRuntime.Arm();}
[HarmonyPatch(typeof(w_battle),nameof(w_battle.InitBattle))]
internal static class WarTacticsBegin{[HarmonyPostfix]static void Postfix()=>WarTacticsRuntime.Begin();}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.OnDestroy))]
internal static class WarTacticsExit{[HarmonyPostfix]static void Postfix()=>WarTacticsRuntime.Clear();}
[HarmonyPatch(typeof(w_demo),nameof(w_demo.InitWarDemo))]
internal static class TacticsEndingStart{[HarmonyPostfix]static void Postfix(ushort event_no){if(WarTacticsRuntime.Armed&&WarTacticsRuntime.Evidence.NativeEnding==event_no)WarTacticsRuntime.Evidence.EndingStarted=true;}}
[HarmonyPatch(typeof(w_demo),nameof(w_demo.DriveWarDemo))]
internal static class TacticsEndingPlayback{[HarmonyPostfix]static void Postfix(int __result){if(__result==-1&&WarTacticsRuntime.Evidence.EndingStarted)WarTacticsRuntime.Evidence.EndingFinished=true;}}
[HarmonyPatch(typeof(w_setflg),nameof(w_setflg.w_foock_finish_work))]
internal static class TacticsFinishCallback{[HarmonyPostfix]static void Postfix(int war_no){if(WarTacticsRuntime.Evidence.War==war_no&&WarTacticsRuntime.Evidence.EndingFinished)WarTacticsRuntime.Evidence.FinishCallbackReturned=true;}}
[HarmonyPatch(typeof(MapChapter),nameof(MapChapter.Update))]
internal static class TacticsMapEvidence{[HarmonyPostfix]static void Postfix(){var e=WarTacticsRuntime.Evidence;if(e.ChapterDestroyed&&e.EndingFinished&&e.FinishCallbackReturned&&e.MapUpdatesAfterEnding<30)e.MapUpdatesAfterEnding++;}}
