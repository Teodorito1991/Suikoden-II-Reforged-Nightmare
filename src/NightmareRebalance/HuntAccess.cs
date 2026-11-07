using HarmonyLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NightmareRebalance;
// An explicit session-only request, not a native scene launcher or a campaign flag.
internal static class HuntAccess
{
 internal static readonly HuntRequest Ticket=new();
 [ThreadStatic]internal static BATTLE_WORK? SetupWork;
 internal static IntPtr SelectedWork;
 internal static HuntDefinition? Selected;
 internal static HuntDefinition? RequestedHunt=>HuntRuntime.Definitions.FirstOrDefault(x=>x.Id==HuntGuildRuntime.SelectedId&&HuntRuntime.Catalogue.Hunts.Any(c=>c.Id==x.Id&&c.Enabled));
 internal static int OriginalSelection=-1,Selections;
 internal static string Status="Not requested";
 internal static void LeavingField(){HuntFieldContext.Window.Clear();if(!HuntLaunchProof.PendingWalkingRequest)Ticket.Cancel();}
 internal static void BeginSetup(){SelectedWork=IntPtr.Zero;Selected=null;OriginalSelection=-1;}
 internal static bool Authorized(BATTLE_WORK w)=>SelectedWork==w.Pointer||(HuntLaunchProof.FixtureProof&&DifficultyProvenance.StrictProbe&&UnityEngine.Application.isBatchMode);
 internal static bool Toggle()
 {
  var game=Framework.Singleton<GAME_WORK>.instance;
  bool available=HuntRuntime.Config.EnableExperimentalHunt&&CombatReforgedRuntime.Config.Enabled&&RequestedHunt!=null&&HuntFieldContext.Active&&!HuntGuildRuntime.PartyMenuOpen&&Combat.State==null&&!WarRuntime.Active&&game?.party_data!=null&&DifficultyProvenance.Difficulty(out int d,out _)&&d==0;
  bool result=Ticket.Toggle(available);Status=available?(result?"ARMED: "+RequestedHunt!.Name+" in "+RequestedHunt.Field+"; F6 cancels":"Cancelled"):"Unavailable: enable experiment and enter a Normal field and close the party menu";
  if(!CombatReforgedRuntime.Config.Enabled)Status="Unavailable: required combat-reforged module is disabled; ordinary encounters retained";
  Plugin.Logger.LogInfo("Experimental hunt: "+Status);return result;
 }
 internal static void Requested(BATTLE_DATA? table,bool walking)
 {if(Ticket.Armed&&(!walking||table?.id!=RequestedHunt?.Audit.TableId)){Ticket.Cancel();Status="Cancelled: unrelated or scripted encounter";}}
 internal static bool SourceMatches(BATTLE_DATA? table)=>RequestedHunt is {} hunt&&SourceMatches(table,hunt);
 internal static bool SourceMatches(BATTLE_DATA? table,HuntDefinition hunt)
 {
  if(table?.id!=hunt.Audit.TableId||table.monster_party==null||table.monster_party.Length!=hunt.PartyCount||table.monster_data==null)return false;
  var a=hunt.Audit;int index=a.Indices.Single();var s=table.monster_party[index];var d=s?.data;
  return d?.be_data?.com_data is {} commands&&commands.Length==0&&s!.monster_id!=null&&s.monster_code!=null&&d.monster_position!=null&&
   CombatEvolutionRules.FormationMatches(a,table.id,s.id,index,s.party_flg,s.item_no,s.monster_kazu,s.monster_id.Select(x=>(int)x).ToArray(),s.monster_code.Select(x=>(int)x).ToArray(),CombatEvolutionRuntime.Hooks(d),d.be_data.command_num,d.monster_position.Select(x=>(int)x).ToArray())&&
   a.Monsters.All(m=>m.Index<table.monster_data.Length&&table.monster_data[m.Index] is {} native&&native.id==m.Id&&CombatReforgedRuntime.Signature(native).SequenceEqual(m.Signature));
 }
 internal static void Select(int maximum,ref int result)
 {
  if(SetupWork is not {} w||!Ticket.Armed)return;
  var source=Framework.Singleton<GAME_WORK>.instance?.sys_work?.battle_data;
  var hunt=RequestedHunt;bool valid=hunt!=null&&HuntRuntime.Config.EnableExperimentalHunt&&CombatReforgedRuntime.Config.Enabled&&maximum==hunt.PartyCount-1&&HuntLaunchProof.MatchesSource(w,source)&&SourceMatches(source,hunt!)&&Plugin.CheckCompatibility()&&DifficultyProvenance.Difficulty(out int d,out _)&&d==0;
  int before=result;
  if(!Ticket.Consume(valid)){Status="Cancelled: native source/provenance guard rejected request";return;}
  // Original native RNG call already ran. Replace its choice only; setup adds one
  // to store the one-based party number; source uses its audited zero-based slot.
  result=hunt!.Audit.Indices.Single();Selected=hunt;OriginalSelection=before;SelectedWork=w.Pointer;Selections++;Status="Selected "+hunt.Name+"; native setup continues";
 }
 internal static void EndSetup(BATTLE_WORK w,Exception? error)
 {if(Ticket.Armed){Ticket.Cancel();Status="Cancelled: setup did not use eligible random selection";}if(error!=null){SelectedWork=IntPtr.Zero;Selected=null;Status="Cancelled: native setup failed";}}
 internal static void Reset(){Ticket.Cancel();SetupWork=null;SelectedWork=IntPtr.Zero;Selected=null;OriginalSelection=-1;Status="Not requested";}
 internal static object Snapshot()=>new{RequestedHuntId=RequestedHunt?.Id,RequestedField=RequestedHunt?.Field,SelectedHuntId=Selected?.Id,Ticket.Armed,FieldChapterCurrent=HuntFieldContext.Active,Selected=SelectedWork!=IntPtr.Zero,OriginalSelection,Selections,Status,NativeMenuImplemented=false,DirectLaunchImplemented=false,SelectionIsNotRenderedEntryProof=true};
}
[HarmonyPatch(typeof(OldSrcBase),nameof(OldSrcBase.get_rand_maxlimit))]
internal static class HuntOriginalPartySelection
{
 [HarmonyPostfix]static void Postfix(int nMax,ref int __result){try{HuntAccess.Select(nMax,ref __result);}catch(Exception e){HuntAccess.Ticket.Cancel();HuntAccess.Status="Cancelled: "+e.GetType().Name;}}
}
public sealed class HuntAccessControl:UnityEngine.MonoBehaviour
{
 readonly StableHotkeyEdge key=new(),guildKey=new();readonly Stopwatch timer=Stopwatch.StartNew();IntPtr party;
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 public HuntAccessControl(IntPtr p):base(p){}
 public void Update()
 {
  if(!HuntRuntime.Config.EnableExperimentalHunt&&!HuntGuildRuntime.Config.EnableSelector)return;
  try{
   var now=Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer??IntPtr.Zero;
   if(party!=now){CastleGuildRuntime.ObserveQuestOwner(now);HuntAccess.Ticket.Cancel();HuntGuildRuntime.Clear();party=now;}
   GetWindowThreadProcessId(GetForegroundWindow(),out uint foreground);short value=GetAsyncKeyState(0x75);
   bool control=(GetAsyncKeyState(0x11)&0x8000)!=0;bool focused=foreground==(uint)Environment.ProcessId;
   short guildValue=GetAsyncKeyState(0x79);
   if(guildKey.Poll(focused&&HuntGuildRuntime.Config.EnableSelector&&Plugin.Options.EnableDeveloperTools.Value,(guildValue&0x8000)!=0,(guildValue&1)!=0,timer.ElapsedMilliseconds))HuntGuildRuntime.Toggle();
   if(key.Poll(focused&&!control&&!HuntGuildRuntime.Visible&&HuntRuntime.Config.EnableExperimentalHunt,(value&0x8000)!=0,(value&1)!=0,timer.ElapsedMilliseconds))HuntAccess.Toggle();
  }catch(Exception e){HuntAccess.Ticket.Cancel();Plugin.Debug("Hunt request unavailable: "+e.Message);}
 }
}
[HarmonyPatch(typeof(UISaveLoad2),nameof(UISaveLoad2.Create))]
internal static class HuntCancelAtSaveLoadMenu
{
 [HarmonyPrefix]static void Prefix(){CastleGuildRuntime.Close(false);HuntGuildRuntime.Clear();if(HuntAccess.Ticket.Armed){HuntAccess.Ticket.Cancel();HuntAccess.Status="Cancelled: save/load menu opened";}}
}
