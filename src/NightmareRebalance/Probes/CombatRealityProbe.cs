using BepInEx;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace NightmareRebalance;
internal static class CombatRealityProbe
{
 internal static int Assertions,CadenceDifferences,FormationChanges,RuneChanges;internal static bool Passed;internal static readonly List<object> Results=new();
 static T Allocate<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool value,string name){Assertions++;if(!value)throw new InvalidOperationException($"Reality native assertion {Assertions}: {name}");}
 internal static void Run()
 {
  if(!Plugin.Options.RunCombatSelfTest.Value)return;
  if(CombatRealityRuntime.Config.Formations.Length!=CombatRealityRuntime.Audit.Formations.Length){Plugin.Logger.LogWarning("Combat Reality probe not run: unavailable configuration.");return;}
  if(Combat.State!=null||WarRuntime.Active||WarTacticsRuntime.Armed)throw new InvalidOperationException("Reality probe requires unloaded startup.");
  var saved=Framework.Singleton<BATTLE_WORK>.instance;var savedGame=Framework.Singleton<GAME_WORK>.instance;var rng=UnityEngine.Random.state;var objects=new List<UnityEngine.Object>();
  var manager=Allocate<BattleManager>();var w=Allocate<BATTLE_WORK>();var nativeManager=manager.Cast<IBattleManager>();
  bool enabled=CombatRealityRuntime.Config.Enabled,ai=CombatReforgedRuntime.Config.EnableEnemyAi;int fcount=CombatEvolutionRuntime.FormationApplications,ucount=CombatEvolutionRuntime.UniteApplications,bcount=CombatEvolutionRuntime.AdditionalBossAdmissions;
  var ownCounters=(CombatRealityRuntime.FormationApplications,CombatRealityRuntime.RuneApplications);
  var counters=(CombatMasteryRuntime.FormationApplications,CombatMasteryRuntime.RuneApplications,CombatMasteryRuntime.UniteApplications,CombatMasteryRuntime.CadenceSelections);
  var previousCopy=CombatEvolutionRuntime.LastCopy;var previousFormation=CombatEvolutionRuntime.LastFormation;
  try
  {
   Framework.Singleton<BATTLE_WORK>.instance=w;var scratch=Allocate<GAME_WORK>();scratch.chara_data=Allocate<CHARA_DATA>();scratch.chara_data.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(1);Framework.Singleton<GAME_WORK>.instance=scratch;
   w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.pm_attack_data=new Il2CppReferenceArray<PM_ATTACK_DATA>(12);w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);w.MokuhyoKaisu=new Il2CppStructArray<byte>(12);w.b_magic_status=new();w.player_kazu=6;w.monster_kazu=1;w.pm_kazu=7;w.front_player=3;w.back_player=3;
   manager.parties=new Il2CppReferenceArray<BattleParty>(2);manager.parties[0]=new(0,nativeManager);manager.parties[1]=new(1,nativeManager);
   for(int i=0;i<12;i++)
   {
    PM_DATA u=i<6?Allocate<BattlePlayerCharacter>():Allocate<BattleMonsterCharacter>();u.manager=nativeManager;u.chara_no=i;u.chara_id=0;u.position=(byte)(i<6?i:0);u.hp=400;u.max_hp=400;u.calc_para=new Il2CppStructArray<int>(new[]{120,70,60,50,80,75,60,100});if(i>6)u.dead_flg=1;w.pm_data[i]=u;w.b_chara[i]=Allocate<BATTLE_CHARA>();w.b_chara[i].chara_id=0;w.pm_attack_data[i]=new(){curent_use_cnt=new Il2CppStructArray<byte>(new byte[]{255,255,255,255})};
    w.c_varia_dat[i]=new(){mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),mp=new Il2CppStructArray<byte>(new byte[]{0x77,0x55,0x33,0x11}),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3)};for(int j=0;j<3;j++)w.c_varia_dat[i].item_eqp[j]=new();
   }
   scratch.chara_data.c_varia_dat[0]=w.c_varia_dat[0];
   CombatRealityRuntime.Config.Enabled=true;CombatRealityRuntime.ProbeScores=true;CombatRealityRuntime.CandidateScores.Clear();CombatReforgedRuntime.Probe=true;CombatReforgedRuntime.ProbeWork=w;CombatReforgedRuntime.Work=w.Pointer;CombatReforgedRuntime.Config.EnableEnemyAi=true;
   foreach(var a in CombatRealityRuntime.Audit.Formations)
   {
    var table=UnityEngine.ScriptableObject.CreateInstance<BATTLE_DATA>();objects.Add(table);table.id=a.TableId;table.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(a.Monsters.Max(x=>x.Index)+1);
    foreach(var m in a.Monsters){var d=CombatReforgedProbe.Template(m.Id,m.Signature);objects.Add(d);d.attack_data_num=0;table.monster_data[m.Index]=d;}
    var source=UnityEngine.ScriptableObject.CreateInstance<MONSTER_PARTY_DATA>();objects.Add(source);source.id=a.PartyId;source.party_flg=a.Flags;source.item_no=a.Item;source.monster_kazu=a.OldCodes.Count(c=>c>0);source.monster_id=new Il2CppStructArray<sbyte>(a.OldIds.Select(x=>(sbyte)x).ToArray());source.monster_code=new Il2CppStructArray<sbyte>(a.OldCodes.Select(x=>(sbyte)x).ToArray());source.data=new(){monster_position=new Il2CppStructArray<short>(a.Positions.Select(x=>(short)x).ToArray()),battle_start_func_index=a.Hooks[0],be_data=new(){command_num=0,com_data=new Il2CppReferenceArray<BE_COM_DATA>(0)}};
    table.monster_party=new Il2CppReferenceArray<MONSTER_PARTY_DATA>(a.Indices.Max()+1);foreach(int i in a.Indices)table.monster_party[i]=source;w.battle_data=table;w.monster_party_data=source;w.monster_party_index=a.Indices[0]+1;
    var toggle=CombatRealityRuntime.Config.Formations.Single(x=>x.PartyId==a.PartyId);bool oldToggle=toggle.Enabled;toggle.Enabled=false;Check(!CombatRealityRuntime.ApplyFormation(w,true),"formation per-entry opt-out");toggle.Enabled=oldToggle;
    bool forms=CombatRealityRuntime.Config.EnableFormations;CombatRealityRuntime.Config.EnableFormations=false;Check(!CombatRealityRuntime.ApplyFormation(w,true),"formation feature opt-out");CombatRealityRuntime.Config.EnableFormations=forms;
    Check(!CombatRealityRuntime.ApplyFormation(w,false),"compatibility opt-out");Check(CombatRealityRuntime.ApplyFormation(w,true),a.PartyId+" production formation");FormationChanges++;
    var copy=w.monster_party_data;Check(copy.Pointer!=source.Pointer,"disposable party");Check(copy.monster_code.Select(x=>(int)x).SequenceEqual(a.NewCodes),"new codes");Check(source.monster_code.Select(x=>(int)x).SequenceEqual(a.OldCodes),"source codes intact");Check(copy.monster_id.Select(x=>(int)x).SequenceEqual(CombatEvolutionRules.Types(a.NewCodes)),"native preload type list");Check(copy.monster_kazu==source.monster_kazu&&copy.item_no==source.item_no&&copy.party_flg==source.party_flg,"count/rewards/flags");Check(CombatEvolutionRuntime.Hooks(copy.data).SequenceEqual(a.Hooks),"callbacks intact");Check(copy.data.Pointer!=source.data.Pointer&&copy.data.monster_position.Pointer!=source.data.monster_position.Pointer,"deep party data");Check(!CombatRealityRuntime.ApplyFormation(w,true),"copy not mistaken for source");
    w.monster_party_data=source;source.data.battle_end_func_index=1;Check(!CombatRealityRuntime.ApplyFormation(w,true),"unexpected story callback excluded");source.data.battle_end_func_index=0;
    Results.Add(new{Kind="ProductionFormationCopy",a.PartyId,a.OldHp,a.NewHp,CallbacksExecuted=false});
   }
   var actor=w.pm_data[6].Cast<BattleMonsterCharacter>();for(int i=7;i<12;i++)w.pm_data[i]=null!;
   foreach(var b in CombatReforgedRuntime.Audit.Bosses.Concat(CombatEvolutionRuntime.Audit.Bosses).Where(b=>b.TableId=="boss_rat"))
   {
    var d=CombatReforgedProbe.Template(b.EnemyId,b.Signature);objects.Add(d);d.attack_data_num=0;w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(new[]{d});actor.hp=(short)d.hp;actor.max_hp=d.hp;
    var table=UnityEngine.ScriptableObject.CreateInstance<BATTLE_DATA>();objects.Add(table);table.id=b.TableId;w.battle_data=table;
    var party=UnityEngine.ScriptableObject.CreateInstance<MONSTER_PARTY_DATA>();objects.Add(party);party.id=b.PartyId;party.monster_kazu=1;party.party_flg=b.Flags;party.monster_id=new Il2CppStructArray<sbyte>(new sbyte[]{1,0,0,0,0,0});party.monster_code=new Il2CppStructArray<sbyte>(new sbyte[]{0,1,0,0,0,0});party.data=new(){battle_start_func_index=b.Hooks[0],phase_start_func_index=b.Hooks[1],phase_end_func_index=b.Hooks[2],battle_end_func_index=b.Hooks[3],command_func_index=b.Hooks[4],battle_main_func_index=b.Hooks[5],be_data=new()};w.monster_party_data=party;w.monster_party_index=1;
    w.monster_be_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>0);w.monster_bm_func=b.Hooks[5]==0?null:(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>0);
    var state=new BattleState(manager){Ready=true,Allowed=false};Combat.State=state;CombatReforgedRuntime.Ready(state,w,true);CombatEvolutionRuntime.Ready(state,w,true);
    Check(CombatReforgedRuntime.Plans.ContainsKey(actor.Pointer),"boss admission "+b.TableId);Check(CombatEvolutionRuntime.BossTables.ContainsKey(actor.Pointer),"phase binding");
    actor.calc_para=new Il2CppStructArray<int>(CombatRealityRuntime.Audit.RatParameters.Concat(new[]{100}).ToArray());
    int differences=0;
    foreach(var build in new[]{(Name:"Balanced",Defense:60,MagicDefense:50),(Name:"Armored",Defense:140,MagicDefense:35),(Name:"Warded",Defense:45,MagicDefense:140)})foreach(int hp in new[]{(int)d.hp,Math.Max(1,d.hp/3)})for(int seed=0;seed<16;seed++)
    {
     CombatRealityRuntime.ProbeBuild=build.Name;CombatRealityRuntime.ProbeScores=seed==0;for(int i=0;i<6;i++){w.pm_data[i].calc_para[2]=build.Defense;w.pm_data[i].calc_para[3]=build.MagicDefense;}
     actor.hp=(short)hp;
     int[] before=new int[4],after=new int[4];
     CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();CombatRealityRuntime.Config.Enabled=false;UnityEngine.Random.InitState(seed);
     for(int n=0;n<4;n++)before[n]=actor.SelectAttackMode();
     CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();CombatRealityRuntime.Config.Enabled=true;UnityEngine.Random.InitState(seed);
     for(int n=0;n<4;n++){after[n]=actor.SelectAttackMode();Check(after[n] is >=0 and <4&&d.attack_data[after[n]].attack_prob>0&&actor.CheckMonsterAttackCnt(after[n])&&actor.AttackModeKyoriCheck(after[n]),"native cadence selection legal");if(before[n]!=after[n])differences++;}
     if(seed<2)Results.Add(new{Kind="ActualNativeRatSequence",Build=build.Name,Hp=hp,Before=before,After=after,SelectorCallsAreNotTurns=true});

    }
    CadenceDifferences+=differences;Results.Add(new{Kind="ActualNativeBossCadenceSelection",b.TableId,Differences=differences,CallbacksExecuted=false});
    Combat.State=null;
   }
   RuneComponents(w,actor,objects);
   Check(CadenceDifferences>0,"Pest Rat actual native choices changed");
  }
  finally
  {
   (CombatMasteryRuntime.FormationApplications,CombatMasteryRuntime.RuneApplications,CombatMasteryRuntime.UniteApplications,CombatMasteryRuntime.CadenceSelections)=counters;
   CombatMasteryRuntime.Recovery.Clear();
   (CombatRealityRuntime.FormationApplications,CombatRealityRuntime.RuneApplications)=ownCounters;
   CombatRealityRuntime.ProbeScores=false;
   CombatRealityRuntime.Config.Enabled=enabled;CombatReforgedRuntime.Config.EnableEnemyAi=ai;CombatReforgedRuntime.Probe=false;CombatReforgedRuntime.ProbeWork=null;CombatReforgedRuntime.Clear();CombatEvolutionRuntime.BossTables.Clear();Combat.State=null;Framework.Singleton<BATTLE_WORK>.instance=saved;Framework.Singleton<GAME_WORK>.instance=savedGame;UnityEngine.Random.state=rng;
   if(CombatEvolutionRuntime.LastCopy!=null&&CombatEvolutionRuntime.LastCopy!=previousCopy)UnityEngine.Object.Destroy(CombatEvolutionRuntime.LastCopy);CombatEvolutionRuntime.LastCopy=previousCopy;CombatEvolutionRuntime.LastFormation=previousFormation;CombatEvolutionRuntime.FormationApplications=fcount;CombatEvolutionRuntime.UniteApplications=ucount;CombatEvolutionRuntime.AdditionalBossAdmissions=bcount;
   foreach(var o in objects)UnityEngine.Object.Destroy(o);
  }
  Check(Framework.Singleton<BATTLE_WORK>.instance?.Pointer==saved?.Pointer&&Framework.Singleton<GAME_WORK>.instance?.Pointer==savedGame?.Pointer,"globals restored");var r=UnityEngine.Random.state;Check(r.s0==rng.s0&&r.s1==rng.s1&&r.s2==rng.s2&&r.s3==rng.s3,"RNG restored");
  Passed=true;string dir=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"native-reality-probe.json"),JsonSerializer.Serialize(Snapshot(),new JsonSerializerOptions{WriteIndented=true}));Plugin.Logger.LogInfo($"Combat Reality probe PASS: {Assertions} assertions; formations={FormationChanges}; native cadence differences={CadenceDifferences}; Runes={RuneChanges}. No rendered battle/story/save executed.");
 }
 internal static object Snapshot()=>new{Ran=Assertions>0,Passed,Assertions,CadenceDifferences,FormationChanges,RuneChanges,Results=Results.ToArray(),PestRatCandidateScores=CombatRealityRuntime.CandidateScores.ToArray(),RenderedBattlePlayed=false,StoryCallbacksInvoked=false};

 static void RuneComponents(BATTLE_WORK w,BattleMonsterCharacter actor,List<UnityEngine.Object> objects)
 {
  var sample=CombatReforgedRuntime.Audit.Enemies.First();var d=CombatReforgedProbe.Template(sample.Id,sample.Signature);objects.Add(d);d.aisyo=new Il2CppStructArray<byte>(11);d.taisei=new Il2CppStructArray<byte>(4);w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(new[]{d});actor.status=0;actor.dead_flg=0;var player=w.pm_data[0].Cast<BattlePlayerCharacter>();
  foreach(var a in CombatRealityRuntime.Audit.Runes)
  {
   var x=a.Signature;var magic=new MAGI_DATA{name=x[0],text=x[1],atrb=(byte)x[2],eff_val=(short)x[3],uni_no=(byte)x[4],uni_magic=(byte)x[5],info=new Il2CppStructArray<byte>(x.Skip(6).Take(2).Select(v=>(byte)v).ToArray()),status=new Il2CppStructArray<byte>(x.Skip(8).Select(v=>(byte)v).ToArray())};
   Check(CombatRealityRuntime.Spell(a.Id,magic)&&magic.eff_val==a.Effect,"guarded new physical rune");RuneChanges++;
   for(int resistance=0;resistance<4;resistance++)for(int seed=0;seed<16;seed++){d.aisyo[10]=(byte)resistance;UnityEngine.Random.InitState(seed);int baseline=player.CalcPlayerDamage(6);UnityEngine.Random.InitState(seed);Check(player.PlayerCalcSpcDamage(magic,6)==CombatMasteryRules.PhysicalRuneDamage(baseline,resistance,a.Effect),"actual native new rune/resistance");}
   Check(!CombatRealityRuntime.Spell(a.Id,magic),"new rune idempotence");magic.text++;Check(!CombatRealityRuntime.Spell(a.Id,magic),"new rune metadata mismatch");
  }
 }
}
