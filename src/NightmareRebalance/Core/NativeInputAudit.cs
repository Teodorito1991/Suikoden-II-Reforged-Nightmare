using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
namespace NightmareRebalance;
// Explicit read-only diagnostic session. No synthetic keys, binding changes or
// controller state injection. Records only the three known confirmation controls.
internal static class NativeInputAudit
{
 internal static bool Requested=>Environment.GetCommandLineArgs().Contains("--reforged-input-audit");
 static readonly Stopwatch clock=Stopwatch.StartNew();static readonly List<object> rows=new();static string previous="";static int samples,dropped;static long nextWrite;static bool fileStarted,writeFaulted;
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 internal static void Sample(string phase)
 {
  if(!Requested||clock.ElapsedMilliseconds>180000||!GRInputManager.IsInstantiated)return;
  try{
   if(clock.ElapsedMilliseconds>=nextWrite){nextWrite=clock.ElapsedMilliseconds+1000;Write();}
   samples++;var keyboard=Keyboard.current;int windows=0,edges=0,held=0,pressed=0;
   var codes=new[]{0x58,0x0D,0x20};var controls=keyboard==null?null:new[]{keyboard.xKey,keyboard.enterKey,keyboard.spaceKey};
   for(int i=0;i<3;i++){short state=GetAsyncKeyState(codes[i]);if((state&0x8000)!=0)windows|=1<<i;if((state&1)!=0)edges|=1<<i;if(controls!=null){if(controls[i].isPressed)held|=1<<i;if(controls[i].wasPressedThisFrame)pressed|=1<<i;}}
   GetWindowThreadProcessId(GetForegroundWindow(),out uint foreground);bool windowFocused=foreground==(uint)Environment.ProcessId;
   var manager=GRInputManager.Instance;bool on=false,press=false;float value=0;
   if(manager.keys!=null&&manager.keys.TryGetValue(GRInputManager.Type.Decide,out var key)&&key!=null){on=key.on;press=key.press;value=key.value;}
   string current=$"{windowFocused}/{keyboard!=null}/{windows}/{edges}/{held}/{pressed}/{on}/{press}/{value}/{UnityEngine.Application.isFocused}/{GRInputManager.Pause}/{manager.mode}/{GRInputManager.LastInputDeviceType}";
   if(current==previous)return;previous=current;
   if(rows.Count>=1024){dropped++;return;}
   rows.Add(new{Milliseconds=clock.ElapsedMilliseconds,Frame=UnityEngine.Time.frameCount,Phase=phase,WindowsForeground=windowFocused,KeyboardPresent=keyboard!=null,KeyboardEnabled=keyboard?.enabled,WindowsHeld=windows,WindowsPressedSinceSample=edges,UnityHeld=held,UnityPressedThisFrame=pressed,NativeDecideOn=on,NativeDecidePress=press,NativeDecideValue=value,Focused=UnityEngine.Application.isFocused,GRInputManager.Pause,Mode=manager.mode.ToString(),Device=GRInputManager.LastInputDeviceType.ToString()});
  }catch(Exception e){if(rows.Count<1024)rows.Add(new{Error=e.GetType().Name,Phase=phase});}
 }
 internal static void Write()
 {
  if(!Requested||writeFaulted)return;
  try{
   string text=System.Text.Json.JsonSerializer.Serialize(new{Version="1.13.0-native-war-foundation.1",Utc=DateTime.UtcNow,Audit=Snapshot()});
   if(System.Text.Encoding.UTF8.GetByteCount(text)>1048576)return;
   string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","developer");Directory.CreateDirectory(dir);string path=Path.Combine(dir,"input-audit.json");
   if(!fileStarted){if(File.Exists(path))File.Move(path,path+".previous",true);fileStarted=true;}
   File.WriteAllText(path+".tmp",text);File.Move(path+".tmp",path,true);
  }catch(Exception e){writeFaulted=true;Plugin.Logger.LogWarning("Input audit file unavailable: "+e.Message);}
 }
 internal static object Routing()
 {
  var deviceRows=new List<object>();var maps=new List<object>();var errors=new List<string>();var supported=new List<string>();string? update=null,background=null;
  try{var all=InputSystem.devices;for(int i=0;i<Math.Min(all.Count,16);i++){var d=all[i];if(d!=null)deviceRows.Add(new{d.name,d.layout,d.enabled,d.deviceId,d.native,d.canRunInBackground});}}catch(Exception e){errors.Add("Devices: "+e.GetType().Name);}
  try{var manager=GRInputManager.Instance;if(manager.keys!=null&&manager.keys.TryGetValue(GRInputManager.Type.Decide,out var key)&&key?.action!=null)for(int i=0;i<Math.Min(key.action.Count,32);i++){var action=key.action[i];if(action==null)continue;var controls=new List<string>();var resolved=action.controls;for(int j=0;j<Math.Min(resolved.Count,32);j++)controls.Add(resolved[j].path);maps.Add(new{action.name,action.enabled,ResolvedControls=controls.ToArray()});}}catch(Exception e){errors.Add("ResolvedControls: "+e.GetType().Name);}
  try{var settings=InputSystem.settings;update=settings.updateMode.ToString();background=settings.backgroundBehavior.ToString();var names=settings.supportedDevices;for(int i=0;i<Math.Min(names.Count,32);i++)supported.Add(names[i]);}catch(Exception e){errors.Add("Settings: "+e.GetType().Name);}
  return new{KeyboardPresent=Keyboard.current!=null,Devices=deviceRows.ToArray(),Maps=maps.ToArray(),SupportedDevices=supported.ToArray(),UpdateMode=update,BackgroundBehavior=background,Errors=errors.ToArray()};
 }
 internal static object Snapshot()=>new{Requested,Samples=samples,Dropped=dropped,MaskLegend="bit0=X, bit1=Enter, bit2=Space",Rows=rows.ToArray(),Routing=Routing(),ReadOnly=true,InputInjected=false};
}
