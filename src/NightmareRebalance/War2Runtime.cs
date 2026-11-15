using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Security.Cryptography;

namespace NightmareRebalance;

internal static class War2Runtime
{
    internal static WarPolicy? Policy;
    internal static bool Armed,Ready,DefinitionsVerified,Probe;
    internal static int Generation,PlayerUnits,ContinueChecks,OutcomeChecks,AiChanges;
    internal static War2Outcome Outcome;
    internal static object? LastResult;
    internal static readonly List<object> AiTrace=new();
    internal static bool Active=>Ready&&!TrainingWar.Active&&Policy is {Enabled:true}&&Plugin.Options.EnableNativeWarRemix.Value;
    internal static void Initialize()
    {
        try{Policy=WarPolicyRules.Parse(OverhaulRuntime.Load("native-war-rules.json")).Wars.Single();DefinitionsVerified=Audit();
            if(!DefinitionsVerified)Plugin.Logger.LogWarning("War 2 policy disabled: native script/map fingerprint mismatch.");}
        catch(Exception e){Plugin.Logger.LogError("War 2 rules disabled: "+e.Message);}
    }
    internal static bool Audit()
    {
        bool Hash(Il2CppStructArray<byte> a,string h)=>a!=null&&Convert.ToHexString(SHA256.HashData(a.ToArray()))==h;
        return war_h.war2map[2]==1
            &&Hash(w_dat_2.event_op_0,"F5A1F374293F5A0122BDB879CBE8F29109E66B48D59FDAFC2A14E37DEF68B534")
            &&Hash(w_dat_2.event_end_0,"7C3A4F0BE9EDD5086B2CAD0EF60F8A8D279114347B4C0CE300CBA6EB78308225")
            &&Hash(w_dat_2.event_end_1,"B150D28158FA9612164DE003EFFE43C865F1355A57C363716D503CBD9EC97855")
            &&Hash(w_dat_2.event_end_2,"1920BE5905B41E9320C5E87CBA2622A357F4C096D19358B90A5535B13F3FD4A4")
            &&Hash(w_dat_2.event_4,"3D61EB9E6EEE1359D7D253966BAD72CCCC46C158BA621F90D0FA82239851C1D2")
            &&w_dat_2.event_data_2[1]?.Pointer==w_dat_2.event_end_0.Pointer
            &&w_dat_2.event_data_2[4]?.Pointer==w_dat_2.event_end_2.Pointer;
    }
    internal static void Arm(){Clear();if(TrainingWar.Active&&!TrainingWar.FortDefenseActive)return;Armed=true;}
    internal static void Begin()
    {
        Ready=false;Generation++;PlayerUnits=ContinueChecks=OutcomeChecks=AiChanges=0;Outcome=War2Outcome.Vanilla;AiTrace.Clear();
        if(!Armed||Policy==null||!DefinitionsVerified||war_h.g_w_war_no!=2||!Audit()||!Plugin.CheckCompatibility())return;
        var rows=w_unit.unit;
        if(rows==null||rows.Length!=2||rows[0]?.Length!=15||rows[1]?.Length!=15)return;
        int[] leaders={0,1,9,20};
        for(int i=0;i<4;i++)if(rows[0][i]==null||w_unit.GetUnitChara(0,i,0)!=leaders[i])return;
        if(w_unit.GetUnitChara(1,0,0)!=61||w_chara.GetCharaZenmetu(61)!=0)return;
        Ready=true;
        if(TrainingWar.FortDefenseActive||(Active&&Policy.PlayerCommands))
            for(int i=0;i<4;i++){w_unit.SetUnitControl(0,i,war_h.CONTROL_PLAYER);PlayerUnits++;}
        if(!Probe)Plugin.Logger.LogInfo($"War 2 READY: remix={Active}, player squads={PlayerUnits}, native AI scoring={!TrainingWar.Active&&Plugin.Options.EnableNativeWarAI.Value&&Policy.EnemyAi}; fallback turn={Policy.MaximumTurn}. Native fort-breach/ending callbacks retained.");
    }
    internal static List<WarUnitState> States()
    {
        var result=new List<WarUnitState>();var rows=w_unit.unit;
        if(rows==null||rows.Length!=2)return result;
        for(int side=0;side<2;side++)
        {if(rows[side]?.Length!=15)return new();for(int i=0;i<15;i++){var u=rows[side][i];if(u==null)return new();result.Add(new(side,i,u.flag,u.x,u.y,u.damage));}}
        return result;
    }
    internal static bool Check(ref int result)
    {
        bool training = Ready && TrainingWar.FortDefenseActive;
        if (!(Active || Probe || training) || Policy == null || war_h.g_w_war_no != 2)
            return true;
        var decision=WarPolicyRules.Decide(Policy,true,true,war_h.g_w_war_no,war_sys.turn,States());
        if (decision == War2Outcome.Vanilla || (training && decision != War2Outcome.TacticalVictory)) return true;
        if(Outcome is War2Outcome.TacticalVictory or War2Outcome.ProtectedAreaBreached or War2Outcome.AlliedDefeat or War2Outcome.OrderlyWithdrawal)
        {result=-1;return false;}
        Outcome=decision;
        if(decision==War2Outcome.Continue){ContinueChecks++;result=0;return false;}
        // War 2 differs from war 1: native all-allies-lost selects ending 1 WITHOUT game-over.
        // All other exits use the original fort evacuation (ending 4), including a tactical victory.
        war_sys.end_event_no=decision==War2Outcome.AlliedDefeat?(byte)1:(byte)4;
        result=-1;OutcomeChecks++;
        if(!Probe)Plugin.Logger.LogInfo($"War 2 OUTCOME: {decision}; turn={war_sys.turn}; native ending={war_sys.end_event_no}; campaign evacuation callbacks retained.");
        return false;
    }
    internal static object Snapshot()=>new{Ready,Active,DefinitionsVerified,Generation,PlayerUnits,ContinueChecks,OutcomeChecks,Outcome=Outcome.ToString(),AiChanges,Policy,AiTrace=AiTrace.ToArray(),Units=States(),LastResult};
    internal static void Clear(){if(Ready)LastResult=new{Outcome=Outcome.ToString(),ContinueChecks,AiChanges};Armed=Ready=false;Outcome=War2Outcome.Vanilla;}
}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.Start))]
internal static class War2ChapterArm{[HarmonyPrefix]static void Prefix()=>War2Runtime.Arm();}
[HarmonyPatch(typeof(w_battle),nameof(w_battle.InitBattle))]
internal static class War2BattleBegin{[HarmonyPostfix]static void Postfix()=>War2Runtime.Begin();}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.OnDestroy))]
internal static class War2ChapterExit{[HarmonyPostfix]static void Postfix()=>War2Runtime.Clear();}
[HarmonyPatch(typeof(w_syouha),nameof(w_syouha.CheckSyouhai))]
internal static class War2OutcomePatch{[HarmonyPrefix]static bool Prefix(ref int __result)=>War2Runtime.Check(ref __result);}

internal static class WarAiRuntime
{
    internal sealed record Scope(int Force,int Unit)
    { internal readonly List<CandidateEvidence> Candidates=new(),TacticalCandidates=new(); internal bool Overflow; internal NativeTacticsBoard? Board; }
    [ThreadStatic]internal static Scope? Current;
}
[HarmonyPatch(typeof(w_tact),nameof(w_tact.AssessTactics))]
internal static class WarAiScope
{
    [HarmonyPrefix]static void Prefix(int force,int unit_number,out WarAiRuntime.Scope? __state)
    {__state=WarAiRuntime.Current;WarAiRuntime.Current=null;if((TotalWarRuntime.AiAllowed(war_h.g_w_war_no)||WarTacticsRuntime.AiAllowed(war_h.g_w_war_no))&&force==1&&unit_number is >=0 and <15&&Plugin.Options.EnableNativeWarAI.Value)WarAiRuntime.Current=new(force,unit_number);}
    [HarmonyPostfix]static void Postfix(){TotalWarAi.Selected(WarAiRuntime.Current);WarTacticsRuntime.Selected(WarAiRuntime.Current);}
    [HarmonyFinalizer]static Exception? Finalizer(WarAiRuntime.Scope? __state,Exception? __exception){WarAiRuntime.Current=__state;return __exception;}
}
[HarmonyPatch(typeof(w_tach),nameof(w_tach.Hyouka))]
internal static class WarAiScore
{
    [HarmonyPostfix]static void Postfix(int force,int unit_number,TACTICS_RESULT_TYPE result,ref int __result)
    {
        var scope=WarAiRuntime.Current;var r=War2Runtime.Policy;
        if(scope==null||force!=1||scope.Force!=force||scope.Unit!=unit_number||result==null||!Plugin.Options.EnableNativeWarAI.Value)return;
        if(!(TotalWarRuntime.AiAllowed(war_h.g_w_war_no)||War2Runtime.Probe||WarTacticsRuntime.AiAllowed(war_h.g_w_war_no)))return;
        var units=w_unit.unit;if(units==null||units.Length!=2||units[0]?.Length!=15||units[1]?.Length!=15)return;
        int before=__result;
        if(war_h.g_w_war_no==2&&r is {EnemyAi:true}&&result.act==0&&result.target<15)
        {
            var target=units[0][result.target];var attacker=units[1][unit_number];
            if(target==null||attacker==null||target.flag!=1||attacker.flag!=1)return;
            int max=w_unit.GetUnitHP(0,result.target);
            int chance=w_kekka.CalcAttackHitRate(1,(byte)unit_number,0,result.target);
            __result=WarPolicyRules.AiScore(before,result.act,target.flag,target.damage,max,chance,r.WoundedTargetBonus,r.PoorOddsPenalty);
            if(before!=__result&&!War2Runtime.Probe)
            {
                War2Runtime.AiChanges++;
                if(War2Runtime.AiTrace.Count<24)War2Runtime.AiTrace.Add(new{Turn=war_sys.turn,Attacker=unit_number,Target=result.target,Damage=target.damage,MaxHits=max,Chance=chance,Before=before,After=__result});
            }
        }
        if(!War2Runtime.Probe)
        {
            __result=TotalWarAi.Score(force,unit_number,result,__result,before,scope);
            __result=WarTacticsRuntime.Score(unit_number,result,__result,before,scope);
        }
    }
}
