using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Security.Cryptography;
using System.Text.Json;
namespace NightmareRebalance;

internal static class TotalWarRuntime
{
    internal static CampaignWarFile Rules=new();
    internal static CommanderFile Specializations=new();
    internal static readonly HashSet<int> Verified=new();
    internal static bool Armed,Ready,Probe;
    internal static int Generation,ActiveWar,AiChanges,SelectedChanges;
    internal static string Result="Vanilla",Transition="No chapter";
    internal static int? Ending;
    internal static object? LastBattle;
    internal static readonly List<object> AiTrace=new();
    internal static CampaignWarPolicy? Policy=>Rules.Scenarios.FirstOrDefault(r=>r.WarNumber==war_h.g_w_war_no);
    internal static bool AiAllowed(int war)=>war==2?War2Runtime.Ready&&War2Runtime.Policy is {EnemyAi:true}:Ready&&ActiveWar==war&&Policy is {EnemyAi:true};
    internal static void Initialize()
    {
        try
        {
            Rules=TotalWarRules.Parse(OverhaulRuntime.Load("total-war-rules.json"));
            foreach(var a in TotalWarRules.Audits)if(Audit(a.War))Verified.Add(a.War);
        }
        catch(Exception e){Rules=new();Verified.Clear();Plugin.Logger.LogError("Total War scenario/AI rules disabled: "+e.Message);}
        // An optional balance-file error must not disable verified scenario rules.
        if(Plugin.Options.EnableWarSpecializations.Value)
            try
            {
                var baseline=ContentRules.Read<CommanderFile>(ContentRuntime.Resource("war-specializations.json"));
                Specializations=OverhaulRules.Commanders(OverhaulRuntime.Load("war-specializations.json"),baseline);
            }
            catch(Exception e){Specializations=new();Plugin.Logger.LogError("War specializations disabled: "+e.Message);}
        Plugin.Logger.LogInfo($"Total War definitions verified: {string.Join(",",Verified.OrderBy(n=>n))}; {Specializations.Profiles.Length} scoped specialization profiles. No later-war launch or story callback invoked.");
    }
    internal static bool Audit(int war)
    {
        var a=TotalWarRules.Audits.SingleOrDefault(a=>a.War==war);if(a.War==0||war_h.war2map[war]!=a.Map)return false;
        using var doc=JsonDocument.Parse(ContentRuntime.Resource("native-war-audit.json"));
        var entry=doc.RootElement.GetProperty(war.ToString());var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+war)!;
        foreach(var item in entry.GetProperty("Scripts").EnumerateObject())
        {
            var bytes=type.GetProperty(item.Name)?.GetValue(null) as Il2CppStructArray<byte>;
            if(bytes==null||Convert.ToHexString(SHA256.HashData(bytes.ToArray()))!=item.Value.GetString())return false;
        }
        var rows=(Il2CppReferenceArray<Il2CppReferenceArray<Il2CppStructArray<sbyte>>>)type.GetProperty("unit_init_data_"+war)!.GetValue(null)!;
        if(rows.Length!=2||rows.Any(r=>r.Length!=15)||rows.Any(r=>r.Any(u=>u.Length!=7)))return false;
        var flat=rows.SelectMany(r=>r.SelectMany(u=>u.Select(v=>unchecked((byte)v)))).ToArray();
        if(Convert.ToHexString(SHA256.HashData(flat))!=entry.GetProperty("Roster").GetString())return false;
        var table=(Il2CppReferenceArray<Il2CppStructArray<byte>>)type.GetProperty("event_data_"+war)!.GetValue(null)!;
        var ending=(Il2CppStructArray<byte>)type.GetProperty(a.TimerEnding==4?"event_end_2":"event_end_0")!.GetValue(null)!;
        return table[a.TimerEnding]?.Pointer==ending.Pointer;
    }
    internal static Il2CppReferenceArray<WAR_CHARA_TYPE>? Characters(int war)=>typeof(w_dat_2).Assembly.GetType("w_dat_"+war)?.GetProperty("chara_"+war)?.GetValue(null) as Il2CppReferenceArray<WAR_CHARA_TYPE>;
    internal static void Arm(){Clear();if(TrainingWar.Active)return;Armed=true;Transition="Chapter started";}
    internal static void Begin()
    {
        Ready=false;Generation++;ActiveWar=war_h.g_w_war_no;Result="Vanilla";Ending=null;AiChanges=SelectedChanges=0;AiTrace.Clear();
        var a=TotalWarRules.Audits.SingleOrDefault(a=>a.War==ActiveWar);
        if(!Armed||a.War==0||!Verified.Contains(a.War)||!Audit(a.War)||!Plugin.CheckCompatibility())return;
        var rows=w_unit.unit;var characters=Characters(a.War);
        if(rows?.Length!=2||rows[0]?.Length!=15||rows[1]?.Length!=15||characters==null||characters.Length<=a.Commander||w_unit.GetUnitChara(1,0,0)!=a.Commander)return;
        var command=characters[a.Commander];
        if(command==null||command.name!=a.CommanderName||((command.status_flag&2)==0)!=a.Routable)return;
        Ready=true;Transition="Native battlefield initialized";
        if(!Probe)Plugin.Logger.LogInfo($"Total War {a.War} READY: map {a.Map}; objective {Policy?.Objective}; deadline {Policy?.MaximumTurn}; commander NameId {a.CommanderName}, routable={a.Routable}.");
    }
    internal static void Clear()
    {
        if(Ready||(Armed&&ActiveWar==2&&Ending!=null))LastBattle=new{War=ActiveWar,Result,Ending,Transition,AiChanges,SelectedChanges};
        Armed=Ready=false;ActiveWar=0;Transition="Chapter destroyed; campaign return not inferred";
    }
    internal static void ApplyResult(ref int result,byte previousEnd,sbyte previousGameOver)
    {
        if(Armed&&War2Runtime.Ready&&war_h.g_w_war_no==2&&result==-1){ActiveWar=2;Observe(War2Runtime.Outcome.ToString());return;}
        var r=Policy;if(!Ready||r==null||war_h.g_w_war_no!=ActiveWar)return;
        var a=TotalWarRules.Audits.Single(a=>a.War==ActiveWar);
        var decision=TotalWarRules.Decide(r,Plugin.Options.EnableNativeWarRemix.Value,true,ActiveWar,war_sys.turn,War2Runtime.States());
        if(decision!=CampaignDecision.Vanilla&&WarTacticsRuntime.TacticalObjective()&&(result==0||war_sys.end_event_no==a.TimerEnding))
        {result=-1;war_sys.end_event_no=(byte)a.TimerEnding;Observe(ActiveWar==3?"Breakthrough":"TacticalThreatReduction");return;}
        // Intercept only the audited late timer exit. Native command/loss/event exits win.
        if(decision==CampaignDecision.Continue&&result==-1&&war_sys.end_event_no==a.TimerEnding&&war_sys.turn>=a.TimerTurn)
        {result=0;war_sys.end_event_no=previousEnd;war_sys.game_over_flag=previousGameOver;Result="Continue";return;}
        if(decision==CampaignDecision.Continue&&result==0){Result="Continue";return;}
        if(decision is CampaignDecision.EligibleElimination or CampaignDecision.HoldCompleted or CampaignDecision.Withdrawal)
        {
            // No shortening below the original deadline or selecting a new ending.
            if(result==0||war_sys.end_event_no==a.TimerEnding){result=-1;war_sys.end_event_no=(byte)a.TimerEnding;Observe(decision.ToString());return;}
        }
        if(result==-1)Observe(w_unit.GetUnitFlag(1,0)==2?"NativeCommanderDefeat":"NativeOutcome");
    }
    static void Observe(string reason)
    {
        if(Ending!=null)return;Ending=war_sys.end_event_no;Result=reason;Transition="Ending selected; playback pending";
        if(!Probe)Plugin.Logger.LogInfo($"Total War {ActiveWar} OUTCOME: {reason}; turn={war_sys.turn}; native ending={Ending}; original interpreter/campaign callbacks retained.");
    }
    internal static CommanderProfile? Specialize(WAR_CHARA_TYPE original)
    {
        int war=war_h.g_w_war_no;
        if(!Armed||!Plugin.Options.EnableWarSpecializations.Value||!(Verified.Contains(war)||(war==2&&War2Runtime.DefinitionsVerified)))return null;
        var chars=Characters(war);if(chars==null)return null;
        // Source pointer must belong to an audited enemy roster member. Never rebalance
        // an allied copy of Kiba or a saved normal character that shares a name.
        var type=typeof(w_dat_2).Assembly.GetType("w_dat_"+war)!;
        var rows=(Il2CppReferenceArray<Il2CppReferenceArray<Il2CppStructArray<sbyte>>>)type.GetProperty("unit_init_data_"+war)!.GetValue(null)!;
        bool enemy=false;
        foreach(var u in rows[1])for(int i=3;i<6;i++)if(u[i]>=0&&u[i]<chars.Length&&chars[u[i]]?.Pointer==original.Pointer)enemy=true;
        if(!enemy)return null;
        string branch=original.force_type==0?"Infantry":original.force_type==1?"Archer":original.force_type==2?"Magicians":"Unknown";
        return OverhaulRules.Select(Specializations,original.name,branch);
    }
    internal static object Snapshot()=>new{Armed,Ready,Generation,ActiveWar,CurrentTurn=Ready?war_sys.turn:0,Policy,Result,Ending,Transition,AiChanges,SelectedChanges,AiTrace=AiTrace.ToArray(),LastBattle,VerifiedWars=Verified.OrderBy(n=>n).ToArray(),Units=Ready?War2Runtime.States():new()};
}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.Start))]
internal static class TotalWarChapterStart{[HarmonyPrefix]static void Prefix()=>TotalWarRuntime.Arm();}
[HarmonyPatch(typeof(w_battle),nameof(w_battle.InitBattle))]
internal static class TotalWarBattleStart{[HarmonyPostfix]static void Postfix()=>TotalWarRuntime.Begin();}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.OnDestroy))]
internal static class TotalWarChapterEnd{[HarmonyPostfix]static void Postfix()=>TotalWarRuntime.Clear();}
[HarmonyPatch(typeof(w_syouha),nameof(w_syouha.CheckSyouhai))]
internal static class TotalWarOutcome
{
    internal readonly record struct Before(byte End,sbyte GameOver);
    [HarmonyPrefix]static void Prefix(out Before __state){WarTacticsRuntime.Capture();__state=new(war_sys.end_event_no,war_sys.game_over_flag);}
    [HarmonyPostfix]static void Postfix(Before __state,ref int __result){TotalWarRuntime.ApplyResult(ref __result,__state.End,__state.GameOver);WarTacticsRuntime.ObserveResult(__result);NativeWarWitness.Result(__result);}
}
[HarmonyPatch(typeof(w_demo),nameof(w_demo.InitWarDemo))]
internal static class TotalWarEndingTrace
{[HarmonyPostfix]static void Postfix(ushort event_no){if((TotalWarRuntime.Armed||War2Runtime.Armed)&&event_no==war_sys.end_event_no)TotalWarRuntime.Transition="Native ending demo initialized ("+event_no+")";}}
[HarmonyPatch(typeof(w_setflg),nameof(w_setflg.w_foock_finish_work))]
internal static class TotalWarCallbackTrace
{[HarmonyPostfix]static void Postfix(int war_no){if(TotalWarRuntime.Armed&&war_no==war_h.g_w_war_no)TotalWarRuntime.Transition="Native finish callback returned; field control not inferred";}}
