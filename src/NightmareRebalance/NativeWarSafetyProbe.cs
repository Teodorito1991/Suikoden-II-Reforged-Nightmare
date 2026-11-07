using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace NightmareRebalance;
internal static class NativeWarSafetyProbe
{
 internal static int Assertions;internal static bool Passed;internal static object? ArmyEffect;
 static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool x,string label){Assertions++;if(!x)throw new InvalidOperationException("Native war safety: "+label);}
 internal static void Run()
 {
  if(!Plugin.Options.RunNativeWarSelfTest.Value)return;if(war_h.g_w_war_no!=0||WarRuntime.Active||War2Runtime.Armed||Combat.State!=null)throw new InvalidOperationException("War safety fixture requires unloaded startup.");
  var game=Framework.Singleton<GAME_WORK>.instance;var units=w_unit.unit;var chars=w_dat_2.chara_2;int war=war_h.g_w_war_no,map=war_h.g_w_map_no;var rng=UnityEngine.Random.state;
  try{
   var g=Alloc<GAME_WORK>();g.game_data=Alloc<GAME_DATA>();g.event_flag=new Il2CppStructArray<byte>(new byte[]{1,2,3});g.chara_flag=new Il2CppStructArray<byte>(2);g.town_flag=new Il2CppStructArray<byte>(2);g.scpoi_flag=new Il2CppStructArray<byte>(2);g.t_box_flag=new Il2CppStructArray<byte>(2);Framework.Singleton<GAME_WORK>.instance=g;
   var before=NativeWarWitness.Capture();var after=NativeWarWitness.Capture();Check(before?.FlagsHash!=null&&before==after,"Actual interop fields captured consistently");Check(g.event_flag.ToArray().SequenceEqual(new byte[]{1,2,3}),"Capture never writes flags");
   g.event_flag[0]=4;Check(NativeWarWitness.Capture()?.FlagsHash!=before!.FlagsHash,"Fingerprint sees changed fixture input without calling campaign flags");g.event_flag[0]=1;
   g.event_flag=null;Check(NativeWarWitness.Capture()?.FlagsHash==null,"Missing data never counts as clean state");
   Check(!NativeWarWitness.Live,"Batch fixtures excluded from genuine lifecycle journal");
   foreach(var method in new[]{AccessTools.Method(typeof(EVENTCON),"ECWarEventGo"),AccessTools.Method(typeof(WarChapter),nameof(WarChapter.Start)),AccessTools.Method(typeof(WarChapter),nameof(WarChapter.Update)),AccessTools.Method(typeof(WarChapter),nameof(WarChapter.OnDestroy)),AccessTools.Method(typeof(w_setflg),nameof(w_setflg.w_foock_finish_work)),AccessTools.Method(typeof(GSD2SaveData),nameof(GSD2SaveData.LoadDataCore)),AccessTools.Method(typeof(GSD2SaveData),nameof(GSD2SaveData.SaveFile))})Check(method!=null&&Harmony.GetPatchInfo(method)?.Owners.Contains(Plugin.Id)==true,"Read-only patch owner "+method?.Name);
   Framework.Singleton<GAME_WORK>.instance=game;
   // init only assigns the original static character array. No InitUnit, event interpreter,
   // WarChapter, scene, ending or campaign-finish routine is called.
   w_dat_2.init();var source=w_dat_2.chara_2;w_dat_2.chara_2=new Il2CppReferenceArray<WAR_CHARA_TYPE>(source.ToArray());
   war_h.g_w_war_no=2;war_h.g_w_map_no=war_h.war2map[2];var rows=new Il2CppReferenceArray<Il2CppReferenceArray<UNIT_TYPE>>(2);
   for(int s=0;s<2;s++){rows[s]=new Il2CppReferenceArray<UNIT_TYPE>(15);for(int i=0;i<15;i++)rows[s][i]=new(){chara=new Il2CppStructArray<sbyte>(new sbyte[]{-1,-1,-1})};}
   w_unit.unit=rows;rows[0][0].flag=1;rows[0][0].x=4;rows[0][0].y=4;rows[0][0].chara[0]=0;rows[1][0].flag=1;rows[1][0].x=5;rows[1][0].y=4;rows[1][0].chara[0]=0;
   var original=source[0];var copied=WarRuntime.Copy(original,new CommanderProfile{Attack=original.attack,Defense=original.defense,AssistAttack=original.assist_attack,AssistDefense=original.assist_defense,Abilities=Array.Empty<string>(),Uses=Array.Empty<byte>()});
   copied.nouryoku=new Il2CppStructArray<war_data_h.tagSPECIAL_ABILITY>(original.nouryoku.ToArray());copied.kaisu=new Il2CppStructArray<byte>(original.kaisu.ToArray());copied.kaisu_max=new Il2CppStructArray<byte>(original.kaisu_max.ToArray());
   Check(copied.Pointer!=original.Pointer&&copied.nouryoku.Pointer!=original.nouryoku.Pointer,"Deep disposable native copy");int attack=original.attack,defense=original.defense;Check(attack<255&&defense<255,"Single byte training increment capacity");
   int power=w_unit.GetUnitPower(0,0),toughness=w_unit.GetUnitToughness(0,0);w_dat_2.chara_2[0]=copied;Check(w_unit.GetUnitPower(0,0)==power&&w_unit.GetUnitToughness(0,0)==toughness,"Same template same real getters");
   copied.attack++;Check(w_unit.GetUnitPower(0,0)==power+1,"Real native unit power reads copied attack increment");copied.defense++;Check(w_unit.GetUnitToughness(0,0)==toughness+1,"Real native toughness reads copied defence increment");copied.defense=(byte)defense;
   int foeIndex=-1,oldChance=-1,newChance=-1;
   for(int i=1;i<Math.Min(108,source.Length);i++)if(source[i]!=null){rows[1][0].chara[0]=(sbyte)i;copied.attack=(byte)attack;int first=w_kekka.CalcAttackHitRate(0,0,1,0);copied.attack=(byte)(attack+1);int next=w_kekka.CalcAttackHitRate(0,0,1,0);Check(first is >=0 and <=100&&next is >=0 and <=100,"Native hit-rate bounds");if(next!=first){foeIndex=i;oldChance=first;newChance=next;break;}}
   Check(original.attack==attack&&original.defense==defense,"Shared original template never trained");Check(copied.status_flag==original.status_flag&&copied.sibou_serifu==original.sibou_serifu&&copied.yarare_serifu==original.yarare_serifu,"Story and casualty fields retained");
   ArmyEffect=new{SourceNameId=original.name,BaseAttack=attack,FixtureAttack=attack+1,NativePowerBefore=power,NativePowerAfter=w_unit.GetUnitPower(0,0),NativeToughnessBase=toughness,OpponentSourceIndex=foeIndex,NativeHitRateBefore=oldChance,NativeHitRateAfter=newChance,HitRateChanged=foeIndex>=0,FixtureOnly=true,TrainingEarned=false,SourceTemplateUnchanged=true,CampaignProgressionApplied=false};
  }finally{Framework.Singleton<GAME_WORK>.instance=game;w_unit.unit=units;w_dat_2.chara_2=chars;war_h.g_w_war_no=war;war_h.g_w_map_no=map;UnityEngine.Random.state=rng;}
  Check(Framework.Singleton<GAME_WORK>.instance?.Pointer==game?.Pointer&&w_unit.unit?.Pointer==units?.Pointer&&w_dat_2.chara_2?.Pointer==chars?.Pointer&&war_h.g_w_war_no==war&&war_h.g_w_map_no==map,"All native references restored");var end=UnityEngine.Random.state;Check(end.s0==rng.s0&&end.s1==rng.s1&&end.s2==rng.s2&&end.s3==rng.s3,"RNG restored");
  Passed=true;string dir=Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","native-war");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"native-safety-probe.json"),JsonSerializer.Serialize(Snapshot(),new JsonSerializerOptions{WriteIndented=true}));Plugin.Logger.LogInfo($"Native war safety probe PASS: {Assertions} assertions; commander effects native; skirmish/story functions not invoked.");
 }
 internal static object Snapshot()=>new{Passed,Assertions,ArmyEffect,SkirmishLaunched=false,CampaignCallbacksInvoked=false,SaveWritten=false,ProgressionAwarded=false};
}
