using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace NightmareRebalance;
// Only unloaded native fixtures may temporarily replace GAME_WORK. Never a live sandbox.
internal static class NativeWarFoundationProbe
{
 internal static int Assertions;internal static bool Passed;static bool owned;
 static void Check(bool ok,string label){Assertions++;if(!ok)throw new InvalidOperationException("War foundation: "+label);}
 static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase {RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static GAME_WORK Fork(GAME_WORK source)
 {
  var arrays=new[]{source.event_flag,source.chara_flag,source.town_flag,source.scpoi_flag,source.t_box_flag};
  if(arrays.Any(a=>a==null||a.Length==0||a.Length>4096))throw new InvalidOperationException("Incomplete campaign flag fork");
  var copy=Alloc<GAME_WORK>();copy.event_flag=new Il2CppStructArray<byte>(source.event_flag.ToArray());copy.chara_flag=new Il2CppStructArray<byte>(source.chara_flag.ToArray());
  copy.town_flag=new Il2CppStructArray<byte>(source.town_flag.ToArray());copy.scpoi_flag=new Il2CppStructArray<byte>(source.scpoi_flag.ToArray());copy.t_box_flag=new Il2CppStructArray<byte>(source.t_box_flag.ToArray());return copy;
 }
 static void InFork(GAME_WORK source,Action<GAME_WORK> action)
 {
  if(owned||!UnityEngine.Application.isBatchMode||!Plugin.Options.RunNativeWarSelfTest.Value||!Environment.GetCommandLineArgs().Contains("--reforged-test-exit")||
    Combat.State!=null||WarRuntime.Active||war_h.g_w_war_no!=0)throw new InvalidOperationException("Flag fork is unloaded probe-only");
  var original=Framework.Singleton<GAME_WORK>.instance;var rng=UnityEngine.Random.state;
  var copy=Fork(source);owned=true;
  try{Framework.Singleton<GAME_WORK>.instance=copy;action(copy);}
  finally{Framework.Singleton<GAME_WORK>.instance=original;UnityEngine.Random.state=rng;owned=false;}
 }
 internal static void Run()
 {
  if(!Plugin.Options.RunNativeWarSelfTest.Value)return;
  if(!UnityEngine.Application.isBatchMode||!Environment.GetCommandLineArgs().Contains("--reforged-test-exit"))return;
  var original=Framework.Singleton<GAME_WORK>.instance;var originalFlags=NativeWarWitness.Capture();var rng=UnityEngine.Random.state;
  foreach(var method in new[]{AccessTools.Method(typeof(w_setflg),nameof(w_setflg.w_foock_finish_work)),AccessTools.Method(typeof(WarChapter),"w_foock_finish_work"),AccessTools.Method(typeof(G2_SYS),nameof(G2_SYS.G2_cha_flag))})
   Check(method!=null&&Harmony.GetPatchInfo(method)?.Owners.All(o=>o==Plugin.Id)!=false,"Known primitive has no foreign patch owner");
  var source=Alloc<GAME_WORK>();source.event_flag=new Il2CppStructArray<byte>(256);source.chara_flag=new Il2CppStructArray<byte>(256);source.town_flag=new Il2CppStructArray<byte>(256);source.scpoi_flag=new Il2CppStructArray<byte>(256);source.t_box_flag=new Il2CppStructArray<byte>(256);
  source.event_flag[0xA9]=0x02;source.chara_flag[121]=0x80;
  InFork(source,copy=>{
   Check(copy.Pointer!=source.Pointer,"Separate GAME_WORK shell");var a=new[]{copy.event_flag,copy.chara_flag,copy.town_flag,copy.scpoi_flag,copy.t_box_flag};var b=new[]{source.event_flag,source.chara_flag,source.town_flag,source.scpoi_flag,source.t_box_flag};
   for(int i=0;i<5;i++){Check(a[i].Pointer!=b[i].Pointer&&a[i].ToArray().SequenceEqual(b[i].ToArray()),"Independent flag storage "+i);}
   w_setflg.w_foock_finish_work(1);Check(copy.event_flag[0xA9]==0x42&&source.event_flag[0xA9]==0x02,"Original War1 finish primitive isolated");
   copy.event_flag[0xA9]=0x02;var chapter=Alloc<WarChapter>();chapter.w_foock_finish_work(1);
   Check(copy.event_flag[0xA9]==0x42&&source.event_flag[0xA9]==0x02,"Separate original chapter finish primitive isolated");
   G2_SYS.G2_cha_flag(30,121);Check(copy.chara_flag[121]==0x81&&source.chara_flag[121]==0x80,"Original interpreter status primitive mode30 isolated");
   G2_SYS.G2_cha_flag(31,121);Check(copy.chara_flag[121]==0xC7&&source.chara_flag[121]==0x80,"Original interpreter status primitive mode31 isolated");
   // DriveUnitDestroy has two verified native calls with mode 33. Rehearse
   // ONLY its flag primitive, not casualty selection, dialogue or combat.
   copy.chara_flag[121]=0x27; G2_SYS.G2_cha_flag(33,121);
   Check(copy.chara_flag[121]==0x95&&source.chara_flag[121]==0x80,"Casualty-reachable mode33 OR90/ANDDD primitive isolated");
   Check(a[0].ToArray().Where((v,i)=>i!=0xA9).All(v=>v==0),"No unexpected event flag writes in tested primitives");
   bool nested=false;try{InFork(source,_=>{});}catch(InvalidOperationException){nested=true;}Check(nested,"Nested singleton ownership rejected");
  });
  Check(Framework.Singleton<GAME_WORK>.instance?.Pointer==original?.Pointer,"Singleton restored after success");
  bool fault=false;try{InFork(source,_=>throw new ApplicationException("Injected fixture failure"));}catch(ApplicationException){fault=true;}
  Check(fault&&Framework.Singleton<GAME_WORK>.instance?.Pointer==original?.Pointer,"Singleton restored after exception");
  var damaged=Fork(source);damaged.chara_flag=null;bool rejected=false;try{InFork(damaged,_=>{});}catch(InvalidOperationException){rejected=true;}Check(rejected,"Missing flag storage rejected before publication");
  var g=Alloc<GAME_WORK>();g.sys_work=Alloc<SYS_WORK>();g.party_data=Alloc<PARTY_DATA>();g.game_data=Alloc<GAME_DATA>();var m=Alloc<MACHICON>();var e=Alloc<EVENTCON>();var data=UnityEngine.ScriptableObject.CreateInstance<MACHIDAT>();
  try{g.sys_work.mcon=m;m.econ=e;e.mcon=m;m.mdat=data;e.mdat=data;
   Check(NativeWarBoundary.OwnersMatch(g,m,e),"Production field ownership helper accepts exact disposable owner graph");
   g.sys_work.mcon=Alloc<MACHICON>();Check(!NativeWarBoundary.OwnersMatch(g,m,e),"Detached system field rejected");g.sys_work.mcon=m;
   m.econ=Alloc<EVENTCON>();Check(!NativeWarBoundary.OwnersMatch(g,m,e),"Foreign event owner rejected");m.econ=e;
   e.mcon=Alloc<MACHICON>();Check(!NativeWarBoundary.OwnersMatch(g,m,e),"Back pointer mismatch rejected");e.mcon=m;
   e.mdat=null;Check(!NativeWarBoundary.OwnersMatch(g,m,e),"Missing field data rejected");e.mdat=data;
   g.party_data=null;Check(!NativeWarBoundary.OwnersMatch(g,m,e),"Missing live party owner rejected");
  }finally{UnityEngine.Object.Destroy(data);}
  Check(originalFlags==NativeWarWitness.Capture(),"Original startup flags unchanged");var end=UnityEngine.Random.state;Check(end.s0==rng.s0&&end.s1==rng.s1&&end.s2==rng.s2&&end.s3==rng.s3,"Native RNG unchanged");
  Check(!WarBoundaryRules.CanLaunch,"No launcher available from passing fixture");Check(!NativeWarWitness.Live,"Fixtures cannot create genuine lifecycle receipts");
  Passed=true;string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","native-war");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"foundation-probe.json"),JsonSerializer.Serialize(Snapshot(),new JsonSerializerOptions{WriteIndented=true}));
  Plugin.Logger.LogInfo($"Native war foundation probe PASS: {Assertions} assertions; actual War1 flag primitives on disposable fork; NO chapter transition, earned outcome, casualty or save invoked.");
 }
 internal static object Snapshot()=>new{Passed,Assertions,Scope="Actual native flag-write primitives on disposable arrays; not a full campaign sandbox",SkirmishLaunched=false,FieldReturnTested=false,RealVictory=false,SaveWritten=false,CommanderXpGranted=0};
}
