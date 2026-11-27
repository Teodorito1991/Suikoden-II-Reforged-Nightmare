using BepInEx;
using System.Text.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NightmareRebalance;

public sealed class DeveloperOverlay : MonoBehaviour
{
    bool visible;
    long nextRefresh;
    bool faulted;
    readonly StableHotkeyEdge toggleKey=new(),exportKey=new();
    readonly Stopwatch timer=Stopwatch.StartNew();
    GUIStyle? textStyle;
    static readonly uint processId=(uint)Environment.ProcessId;
    [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
    string summary="Reforged developer tools ready.";
    internal static int ExportCount;
    public DeveloperOverlay(IntPtr pointer):base(pointer) { }
    public void Update()
    {
        if(faulted||!Plugin.Options.EnableDeveloperTools.Value)return;
        try
        {
            GetWindowThreadProcessId(GetForegroundWindow(),out uint foreground);
            bool focused=foreground==processId;
            short toggle=GetAsyncKeyState(0x77),export=GetAsyncKeyState(0x78);
            if(toggleKey.Poll(focused,(toggle&0x8000)!=0,(toggle&1)!=0,timer.ElapsedMilliseconds)){visible=!visible;nextRefresh=0;}
            if(exportKey.Poll(focused,(export&0x8000)!=0,(export&1)!=0,timer.ElapsedMilliseconds))Export();
            if(visible&&timer.ElapsedMilliseconds>=nextRefresh)
            {nextRefresh=timer.ElapsedMilliseconds+500;try{summary=Summary();}catch(Exception e){summary="Inspection unavailable: "+e.Message;}}
            NativeInputAudit.Sample("Update");
        }
        catch(Exception e){faulted=true;visible=false;Plugin.Logger.LogError("Developer inspector disabled after input failure: "+e);}
    }
    public void LateUpdate(){if(Plugin.Options.EnableDeveloperTools.Value)NativeInputAudit.Sample("LateUpdate");}
    public void OnGUI()
    {
        if(faulted||!visible||!Plugin.Options.EnableDeveloperTools.Value)return;
        try
        {
            if(textStyle==null)
            {
                textStyle=new GUIStyle {font=Font.CreateDynamicFontFromOSFont("Consolas",18),fontSize=18,wordWrap=true,alignment=TextAnchor.UpperLeft};
                textStyle.normal.textColor=Color.white;
            }
            var matrix=GUI.matrix;var color=GUI.color;
            try
            {
                GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;
                GUI.DrawTexture(new Rect(16,16,760,580),Texture2D.blackTexture,ScaleMode.StretchToFill,false);
                GUI.Label(new Rect(30,30,730,36),"Reforged v1.11 - Combat / War inspector (F8 closes)",textStyle);
                GUI.Label(new Rect(30,72,730,510),summary,textStyle);
            }
            finally{GUI.matrix=matrix;GUI.color=color;}
        }
        catch(Exception e){faulted=true;visible=false;Plugin.Logger.LogError("Developer inspector disabled after rendering failure: "+e);}
    }
    static string Summary()
    {
        string body=Combat.State is {Ready:true}?CombatReforgedDiagnostics.Summary():WarInspector.Summary();
        return HuntRuntime.Config.EnableExperimentalHunt?"HUNT GUILD (party menu: F10)\nEXPERIMENTAL HUNT: "+(HuntAccess.RequestedHunt?.Name??"selection disabled")+"\n"+(HuntAccess.RequestedHunt?.Field??"")+"\nF6 request/cancel: "+HuntAccess.Status+"\n"+body:body;
    }
    internal static void Export()
    {
        try
        {
            var s=Combat.State;var manager=BattleManager.Instance;
            var units=new List<object>();
            if(s is {Ready:true}&&manager!=null&&s.Manager==manager.Pointer&&manager.battle_work?.pm_data is {} roster)
                foreach(var unit in roster)
                {
                    if(unit==null||units.Count>=64)continue;
                    s.EnemyPlans.TryGetValue(unit.Pointer,out var profile);
                    units.Add(new { Slot=unit.chara_no,Player=unit.IsPlayer,Hp=unit.hp,MaxHp=unit.max_hp,EnemyId=unit.IsPlayer?null:unit.monster_data?.id,Role=profile?.Role,HpFactor=profile?.HpMultiplier,DamageFactor=profile?.DamageMultiplier });
                }
            string directory=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","developer");
            Directory.CreateDirectory(directory);
            // Bounded to 16 files written by this process, no deletion of the user's previous evidence.
            if(ExportCount>=16){Plugin.Logger.LogWarning("Developer snapshot limit reached for this session (16).");return;}
            string path=Path.Combine(directory,$"snapshot-{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}.json");
            var report=new {CastleGuild=CastleGuildRuntime.Snapshot(),NativeWarSafety=NativeWarWitness.Snapshot(),NativeWarSafetyProbe=NativeWarSafetyProbe.Snapshot(),NativeWarBoundary=NativeWarBoundary.Snapshot(),NativeWarFoundationProbe=NativeWarFoundationProbe.Snapshot(),HuntGuild=HuntGuildRuntime.Snapshot(),Version=Plugin.BuildLabel,HuntDebug=new{HuntDebug.Enabled,HuntDebug.Armed,HuntDebug.Blocked,HuntDebug.Prevented,HuntDebug.Faulted,DebugArmedIsNotBalanceProof=true,Assisted=HuntSessionRuntime.Current?.DebugArmedDuringRun??HuntSessionRuntime.Last?.DebugArmedDuringRun??false},NativeInput=NativeInputDiagnostics.Snapshot(),InputAudit=NativeInputAudit.Snapshot(),CombatReforged=CombatReforgedRuntime.Snapshot(),CombatEvolution=CombatEvolutionRuntime.Snapshot(),CombatMastery=CombatMasteryRuntime.Snapshot(),CombatReality=CombatRealityRuntime.Snapshot(),Maintenance=MaintenanceRuntime.Snapshot(),Expansion=HuntRuntime.Snapshot(),RealityProbe=CombatRealityProbe.Snapshot(),ProductionAssets=ProductionAssetAudit.Snapshot(),MasteryProbe=CombatMasteryProbe.Snapshot(),EvolutionProbe=CombatEvolutionProbe.Snapshot(),CombatMetrics=CombatReforgedDiagnostics.Snapshot(),CombatProbe=CombatReforgedProbe.Snapshot(),Utc=DateTime.UtcNow,ReadOnly=true,CombatTestedByExporter=false,StandaloneArenaImplemented=false,
                EnemyProfiles=OverhaulRuntime.Enemies.Count,CommanderProfiles=OverhaulRuntime.Commanders.Profiles.Length,OverhaulRuntime.EnemyApplications,OverhaulRuntime.CommanderApplications,
                RegularScope=s?.Allowed??false,WarActive=WarRuntime.Active,WarNumber=WarInspector.CurrentWar,NativeRemix=NativeWarRemix.Snapshot(),War2=War2Runtime.Snapshot(),WarInspector=WarInspector.Snapshot(),TacticsProbe=new{Ran=WarTacticsProbe.Assertions>0,WarTacticsProbe.Assertions,WarTacticsProbe.Passed,WarTacticsProbe.SelectionDifferences,WarTacticsProbe.LegalCandidates},TotalWarProbe=new{Ran=TotalWarProbe.Assertions>0,TotalWarProbe.Assertions,TotalWarProbe.Passed,TotalWarProbe.DecisionDifferences},RevolutionProbe=new{Ran=War2Probe.Assertions>0,War2Probe.Assertions,War2Probe.Passed},TargetingChanges=TargetingRuntime.Changes,Units=units};
            File.WriteAllText(path,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));ExportCount++;
            Plugin.Logger.LogInfo("Developer snapshot: "+path);
        }
        catch(Exception e){Plugin.Logger.LogWarning("Developer snapshot unavailable: "+e.Message);}
    }
}

