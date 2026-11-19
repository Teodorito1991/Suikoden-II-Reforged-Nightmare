using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.Json;
using UnityEngine;

internal static class HuntDebug
{
 public const string Id=NightmareRebalance.Plugin.Id;
 internal static BepInEx.Logging.ManualLogSource Log=null!;
 internal static bool Armed,Faulted,Dirty;
 internal static bool FixtureEnabled;
 internal static bool Enabled=>NightmareRebalance.Plugin.Options.EnableHuntDebugTools.Value||(FixtureEnabled&&Application.isBatchMode&&Environment.GetCommandLineArgs().Contains("--reforged-hunt-debug-selftest"));
 internal static int Blocked;
 internal static long Prevented;
 internal static FieldInfo Work=null!,Party=null!,Units=null!;
 internal static readonly List<object> Events=new();
 internal static void Initialize(NightmareRebalance.Plugin host)
 {
  Log=NightmareRebalance.Plugin.Logger;
  try{
   var type=AppDomain.CurrentDomain.GetAssemblies().First(a=>a.GetName().Name=="ReforgedNightmare").GetType("NightmareRebalance.HuntRuntime")??throw new Exception("Required hunt module missing");
   Work=AccessTools.Field(type,"Work")??throw new Exception("Hunt Work missing");Party=AccessTools.Field(type,"Party")??throw new Exception("Hunt Party missing");Units=AccessTools.Field(type,"Units")??throw new Exception("Hunt Units missing");
   if(Enabled)host.AddComponent<HuntDebugOverlay>();
   Log.LogInfo("Hunt debug available="+Enabled+"; OFF each launch. F7 toggles incoming-damage protection ONLY for the admitted hunt. No level/save edits.");
   
  }catch(Exception e){Faulted=true;Armed=false;Log.LogError(e);}
 }
 internal static bool Scope(BATTLE_WORK? w)
 {
  var a=NightmareRebalance.HuntRuntime.Active;
  if(a==null||w==null||Faulted||w.Pointer!=(IntPtr)Work.GetValue(null)!||w.monster_party_data?.Pointer!=(IntPtr)Party.GetValue(null)!||w.battle_data?.id!=a.Audit.TableId||w.monster_party_index!=a.Audit.Indices.Single()+1||w.pm_data==null)return false;
  if(Units.GetValue(null) is not Dictionary<IntPtr,string> units||!NightmareRebalance.HuntCatalogueRules.RosterMatches(a,units.Values))return false;
  var enemies=w.pm_data.Where(u=>u!=null&&!u.IsPlayer).ToArray();return enemies.Length==a.Roster.Length&&enemies.All(u=>units.ContainsKey(u.Pointer));
 }
 internal static void Record(object value){if(Events.Count<128)Events.Add(value);Dirty=true;}
 internal static void Write()
 {
  try{string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","developer");Directory.CreateDirectory(dir);string path=Path.Combine(dir,"hunt-debug.json");File.WriteAllText(path+".tmp",JsonSerializer.Serialize(new{Version=NightmareRebalance.Plugin.BuildLabel,Utc=DateTime.UtcNow,Armed,Faulted,Blocked,Prevented,Scope="Admitted native catalogue hunt only",Events=Events.ToArray(),LevelsChanged=false,SaveFilesWritten=false,StatusImmunity=false}));File.Move(path+".tmp",path,true);}catch(Exception e){Log.LogWarning("Debug report unavailable: "+e.Message);}Dirty=false;
 }
}
[HarmonyPatch(typeof(PM_DATA),nameof(PM_DATA.DamageSetReActionData))]
internal static class HuntDamageProtection
{
 [HarmonyPrefix,HarmonyPriority(Priority.First)]internal static void Prefix(int mokuhyo,ref short damage)
 {
  if(!HuntDebug.Enabled||!HuntDebug.Armed||HuntDebug.Faulted||damage<=0)return;
  try{var w=Framework.Singleton<BATTLE_WORK>.instance;if(!HuntDebug.Scope(w)||mokuhyo<0||mokuhyo>=w!.pm_data.Length||w.pm_data[mokuhyo] is not {IsPlayer:true,IsDead:false} target)return;
   int original=damage;damage=0;HuntDebug.Blocked++;HuntDebug.Prevented+=original;NightmareRebalance.HuntSessionRuntime.Current?.Block(original);HuntDebug.Record(new{Kind="PreventedIncomingDamage",Slot=mokuhyo,Character=target.chara_id,Damage=original,Hp=target.hp});
  }catch(Exception e){HuntDebug.Armed=false;HuntDebug.Faulted=true;HuntDebug.Log.LogError("Debug protection disabled: "+e.Message);}
 }
}
public class HuntDebugOverlay:MonoBehaviour
{
 readonly Stopwatch timer=Stopwatch.StartNew();bool held;long last=-1000,nextWrite;GUIStyle? style;
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 public HuntDebugOverlay(IntPtr p):base(p){}
 public void Update()
 {
  GetWindowThreadProcessId(GetForegroundWindow(),out uint foreground);short key=GetAsyncKeyState(0x76);bool down=(key&0x8000)!=0;
  if(foreground==(uint)Environment.ProcessId&&!held&&(down||(key&1)!=0)&&timer.ElapsedMilliseconds-last>300&&!HuntDebug.Faulted){last=timer.ElapsedMilliseconds;HuntDebug.Armed=!HuntDebug.Armed;NightmareRebalance.HuntSessionRuntime.Current?.DebugArmed(HuntDebug.Armed);HuntDebug.Log.LogInfo("HUNT DEBUG protection "+(HuntDebug.Armed?"ARMED":"OFF"));HuntDebug.Record(new{Kind="Toggle",HuntDebug.Armed});}
  held=down;
  if(HuntDebug.Dirty&&timer.ElapsedMilliseconds>=nextWrite){nextWrite=timer.ElapsedMilliseconds+1000;HuntDebug.Write();}
 }
 public void OnGUI()
 {
  if(!HuntDebug.Enabled||!HuntDebug.Armed||HuntDebug.Faulted)return;
  try{style??=new GUIStyle{font=Font.CreateDynamicFontFromOSFont("Consolas",18),fontSize=18};style.normal.textColor=Color.yellow;var matrix=GUI.matrix;var color=GUI.color;
   try{GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;float y=Math.Max(16,Screen.height-80);GUI.DrawTexture(new Rect(16,y,850,60),Texture2D.blackTexture);GUI.Label(new Rect(26,y+8,830,44),"HUNT DEBUG ON (F7 off) - incoming hit damage blocked in hunt only\nNo level edits / no status immunity - test copy; do not save",style);}finally{GUI.matrix=matrix;GUI.color=color;}
  }catch(Exception e){HuntDebug.Armed=false;HuntDebug.Faulted=true;HuntDebug.Log.LogError(e);}
 }
}
public class HuntDebugExit:MonoBehaviour
{int frames;public HuntDebugExit(IntPtr p):base(p){}public void Update(){if(++frames==60)Application.Quit();}}
