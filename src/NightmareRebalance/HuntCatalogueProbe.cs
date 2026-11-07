using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
namespace NightmareRebalance;
// Original setup, selectors and reward calculation on disposable native state.
// Walking provenance / victory inputs are fixtures; no battle scene or story callback runs.
internal static class HuntCatalogueProbe
{
 internal static int Assertions,Setups,Selections,Differences;internal static readonly List<object> Rows=new();
 internal static readonly HashSet<string> Covered=new();
 static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool value,string label){Assertions++;if(!value)throw new InvalidOperationException("Hunt catalogue: "+label);}
 static bool Same(UnityEngine.Random.State a,UnityEngine.Random.State b)=>a.s0==b.s0&&a.s1==b.s1&&a.s2==b.s2&&a.s3==b.s3;
 internal static void Table(BATTLE_DATA table)
 {
  var definition=HuntRuntime.Definitions.SingleOrDefault(d=>d.Audit.TableId==table.id);if(definition==null)return;
  var game=Framework.Singleton<GAME_WORK>.instance;var work=Framework.Singleton<BATTLE_WORK>.instance;var state=Combat.State;var rng=UnityEngine.Random.state;
  var last=CombatEvolutionRuntime.LastCopy;var formation=CombatEvolutionRuntime.LastFormation;var selected=HuntRuntime.Catalogue.SelectedHuntId;
  bool enabled=HuntRuntime.Config.EnableExperimentalHunt,strict=DifficultyProvenance.StrictProbe,ai=CombatReforgedRuntime.Config.EnableEnemyAi;
  var setting=HuntRuntime.Settings(definition);bool admitted=setting.Enabled;
  var copies=new List<MONSTER_DATA>();var parties=new List<MONSTER_PARTY_DATA>();
  try{
   HuntRuntime.Catalogue.SelectedHuntId=definition.Id;setting.Enabled=true;HuntRuntime.Config.EnableExperimentalHunt=true;DifficultyProvenance.StrictProbe=true;
   var scratch=Alloc<GAME_WORK>();scratch.sys_work=Alloc<SYS_WORK>();scratch.sys_work.battle_data=table;scratch.sys_work.yobi=new Il2CppStructArray<byte>(8);scratch.game_data=Alloc<GAME_DATA>();scratch.party_data=Alloc<PARTY_DATA>();scratch.party_data.party_cha_no=new Il2CppStructArray<byte>(6);Framework.Singleton<GAME_WORK>.instance=scratch;
   scratch.chara_data=Alloc<CHARA_DATA>();scratch.chara_data.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(1);scratch.chara_data.c_kotei_dat=new Il2CppReferenceArray<C_KOTEI_DAT>(new[]{new C_KOTEI_DAT{seicyo_type=new Il2CppStructArray<byte>(4),monsyo_seigen=new Il2CppStructArray<byte>(4),mon_zoku_aisyo=new Il2CppStructArray<byte>(4),mon_kosu_type=new Il2CppStructArray<byte>(3)}});
   var manager=Alloc<BattleManager>();var native=manager.Cast<IBattleManager>();
   BATTLE_WORK Setup(bool armed,bool walking=true){var w=Alloc<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=w;Combat.State=null;HuntAccess.Ticket.Cancel();if(armed)HuntAccess.Ticket.Toggle(true);if(walking)HuntLaunchProof.FixtureRequest(table);else HuntLaunchProof.Clear();UnityEngine.Random.InitState(17017);manager.BattleDataSetup();Setups++;copies.AddRange(w.monster_data.Where(d=>d!=null));if(!table.monster_party.Any(d=>d.Pointer==w.monster_party_data.Pointer))parties.Add(w.monster_party_data);return w;}
   Check(HuntAccess.SourceMatches(table,definition),"Exact complete source party / monster signatures "+definition.Id);
   var source=table.monster_party[definition.Audit.Indices.Single()];var oldCodes=source.monster_code.ToArray();
   var baseline=Setup(false);var nativeRng=UnityEngine.Random.state;Check(HuntRuntime.Active==null&&HuntAccess.SelectedWork==IntPtr.Zero,"Unarmed keeps ordinary native/remixed selection");
   setting.Enabled=false;Setup(true);Check(HuntRuntime.Active==null&&HuntAccess.SelectedWork==IntPtr.Zero,"Independent hunt opt-out");setting.Enabled=true;
   scratch.game_data.difficulty=2;Setup(true);Check(HuntRuntime.Active==null&&HuntAccess.SelectedWork==IntPtr.Zero,"Hard request excluded");scratch.game_data.difficulty=0;
   Setup(true,false);Check(HuntRuntime.Active==null&&HuntAccess.SelectedWork==IntPtr.Zero,"Missing walking provenance excluded");
   bool module=CombatReforgedRuntime.Config.Enabled;CombatReforgedRuntime.Config.Enabled=false;
   try{Setup(true);Check(HuntRuntime.Active==null&&HuntAccess.SelectedWork==IntPtr.Zero&&!HuntAccess.Ticket.Armed,"Required combat module disabled: no partial hunt selection");}
   finally{CombatReforgedRuntime.Config.Enabled=module;}
   var w=Setup(true);Check(Same(nativeRng,UnityEngine.Random.state),"Post-original selection RNG unchanged");
   Check(HuntRuntime.Active==definition&&w.monster_party_index==definition.Audit.Indices.Single()+1&&w.monster_party_data.monster_code.Select(x=>(int)x).SequenceEqual(definition.Audit.NewCodes),"Requested hunt wins precedence over ordinary formation remix");
   Check(!HuntAccess.Ticket.Armed&&source.monster_code.SequenceEqual(oldCodes),"One-shot request / source not mutated");
   Check(CombatEvolutionRuntime.Hooks(w.monster_party_data.data).SequenceEqual(definition.Audit.Hooks),"All original callbacks preserved");
   Check(w.monster_party_data.monster_id.Select(x=>(int)x).SequenceEqual(CombatEvolutionRules.Types(definition.Audit.NewCodes)),"Preload type list matches local native roster");
   Combat.Begin(manager);Check(HuntRuntime.Active==definition&&HuntRuntime.Work==w.Pointer,"Battle begin preserves requested definition");
   var roster=definition.Audit.NewCodes.Where(x=>x>0).ToArray();int total=6+roster.Length;
   w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.p_status_data=new Il2CppReferenceArray<PLAYER_STATUS_DATA>(12);for(int i=0;i<12;i++)w.p_status_data[i]=new();w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.pm_attack_data=new Il2CppReferenceArray<PM_ATTACK_DATA>(12);w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);w.MokuhyoKaisu=new Il2CppStructArray<byte>(12);w.b_magic_status=new();w.player_kazu=6;w.monster_kazu=(byte)roster.Length;w.pm_kazu=(byte)total;w.front_player=3;w.back_player=3;
   manager.parties=new Il2CppReferenceArray<BattleParty>(new[]{new BattleParty(0,native),new BattleParty(1,native)});
   for(int i=0;i<total;i++){PM_DATA u=i<6?Alloc<BattlePlayerCharacter>():Alloc<BattleMonsterCharacter>();u.manager=native;u.chara_no=i;int code=i<6?0:roster[i-6];u.chara_id=(byte)code;u.position=(byte)(i%6);u.hp=600;u.max_hp=600;u.status=0;u.calc_para=new Il2CppStructArray<int>(new[]{120,70,60,50,80,75,60,100});if(i>=6){u.hp=(short)w.monster_data[code].hp;u.max_hp=w.monster_data[code].hp;for(int j=0;j<7;j++)u.calc_para[j]=w.monster_data[code].para[j];}w.pm_data[i]=u;w.b_chara[i]=Alloc<BATTLE_CHARA>();w.b_chara[i].chara_id=code;w.pm_attack_data[i]=new(){curent_use_cnt=new Il2CppStructArray<byte>(new byte[]{255,255,255,255})};w.c_varia_dat[i]=new(){mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),mp=new Il2CppStructArray<byte>(4),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3)};for(int j=0;j<3;j++)w.c_varia_dat[i].item_eqp[j]=new();}
   scratch.chara_data.c_varia_dat[0]=w.c_varia_dat[0];CombatReforgedRuntime.Probe=true;CombatReforgedRuntime.ProbeWork=w;
   var s=new BattleState(manager){Ready=true,Allowed=true,StatsAllowed=true};Combat.State=s;CombatReforgedRuntime.Ready(s,w,true);HuntRuntime.Ready(s,w);
   Check(HuntRuntime.ActiveRoster&&CombatReforgedRuntime.Plans.Count==roster.Length,"Production admission binds full exact roster including duplicates");
   Check(HuntDebug.Scope(w),"Integrated debug scope admits only the exact bound catalogue roster");
   var actors=new List<object>();
   foreach(var actor in w.pm_data.Where(u=>u!=null&&!u.IsPlayer).Select(u=>u.Cast<BattleMonsterCharacter>())){
    int before=actor.max_hp;var rule=HuntCatalogueRules.Unit(definition,setting,HuntRuntime.Config,actor.monster_data.id);Combat.ScaleEnemy(actor);int after=actor.max_hp;double factor=CombatReforgedRuntime.Factor(s,actor,1,true);Check(after==Rules.Scale(before,factor,32767),"Production factor applied");Combat.ScaleEnemy(actor);Check(actor.max_hp==after,"HP scaled exactly once");
    int differences=0;for(int seed=0;seed<16;seed++){UnityEngine.Random.InitState(seed);CombatReforgedRuntime.Config.EnableEnemyAi=false;int original=actor.SelectAttackMode();var afterOriginal=UnityEngine.Random.state;UnityEngine.Random.InitState(seed);CombatReforgedRuntime.Config.EnableEnemyAi=true;int result=actor.SelectAttackMode();Check(Same(afterOriginal,UnityEngine.Random.state),"Selector RNG preserved");Check(result==original||(result is >=0 and <4&&actor.monster_data.attack_data[result].attack_prob>0&&actor.CheckMonsterAttackCnt(result)&&actor.AttackModeKyoriCheck(result)),"Changed selection remains native legal");Selections++;if(result!=original){differences++;Differences++;}}
    actors.Add(new{Id=actor.monster_data.id,NativeHp=before,FinalHp=after,rule.Behavior,NativeSelectionDifferences=differences,RenderedActions=false});
   }
   if(definition.Id=="dreamers-circuit")Check(PM_DATA.status_henkan_table[10]==b_game_h.STATUS_SLEEP&&w.monster_data[2].attack_data[1].status==10,"Native Nightmare sleep mapping and move");
   if(definition.Id=="thornwake-patrol")Check(PM_DATA.status_henkan_table[1]==b_game_h.STATUS_FUSEN&&w.monster_data[4].attack_data[0].status==1,"Native Holly Elf balloon mapping and move");
   // Native reward calculation against a synthetic defeated roster; no victory or payout is fabricated.
   w.exit_work=new EXIT_WORK{monster_item=new PARTY_ITEM()};w.dead_monster=(byte)roster.Length;w.dead_monster_data=new Il2CppReferenceArray<MONSTER_DEAD_DATA>(roster.Select(i=>new MONSTER_DEAD_DATA{chara_id=(byte)i,player_no=-1}).ToArray());
   int raw=roster.Sum(i=>w.monster_data[i].mochi_kin*((w.monster_data[i].seishitsu_flg&2)!=0?10:1));
   int rewardSeed=0;for(;rewardSeed<1024;rewardSeed++){UnityEngine.Random.InitState(rewardSeed);if(roster.All(i=>OldSrcBase.get_rand(254)>=w.monster_data[i].item_prob.Sum(x=>(int)x)))break;}Check(rewardSeed<1024,"No-drop seed found without inventory mutation");
   int Calculate(bool victory){s.SnapshotTaken=victory;CombatReforgedDiagnostics.Outcome=victory?"VictorySignal":"ClosedWithoutVictorySignal";UnityEngine.Random.InitState(rewardSeed);manager.get_monster_okane(w);return w.exit_work.monster_okane;}
   int scaled=Rules.Scale(raw,Plugin.Options.PotchMultiplier.Value,int.MaxValue);Check(Calculate(false)==scaled,"Escape / no victory excludes bonus");int prepared=Calculate(true);Check(prepared==scaled+HuntCatalogueRules.Bonus(definition,setting,HuntRuntime.Config),"Original native calculation plus configured bonus");Check(Calculate(true)==prepared,"Repeated native calculation does not compound");
   Rows.Add(new{definition.Id,definition.Field,Table=table.id,Actors=actors.ToArray(),NativeBasePotch=raw,ConfiguredBasePotch=scaled,PreparedReward=prepared,OriginalSetup=true,WalkingRequestSynthetic=true,VictoryInputSynthetic=true,RenderedBattle=false,ActualPayout=false});
   foreach(int code in roster.Distinct()){var opd=table.monster_data[code].opd_data;Check(opd!=null&&opd.anime_kazu>0&&opd.pattern_kazu>0&&opd.anime_table.Count>0&&opd.pattern_table.Count>0,"Original animation / pattern sources loaded");}
   GuildNativeProbe.Hunt(manager,w,s,rewardSeed);
   var repeat=Setup(true);Check(HuntRuntime.Active==definition&&HuntAccess.SelectedWork==repeat.Pointer,"Repeat request without campaign flags or completion cap");
   HuntRuntime.Detach();Check(!HuntRuntime.ActiveRoster&&!HuntDebug.Scope(repeat),"Cleanup removes reward / debug scope");Covered.Add(definition.Id);
  }finally{
   HuntRuntime.Catalogue.SelectedHuntId=selected;setting.Enabled=admitted;HuntRuntime.Config.EnableExperimentalHunt=enabled;HuntRuntime.Reset();HuntAccess.Reset();HuntLaunchProof.Clear();DifficultyProvenance.StrictProbe=strict;CombatReforgedRuntime.Config.EnableEnemyAi=ai;CombatReforgedRuntime.Probe=false;CombatReforgedRuntime.ProbeWork=null;CombatReforgedRuntime.Clear();Combat.State=state;Framework.Singleton<GAME_WORK>.instance=game;Framework.Singleton<BATTLE_WORK>.instance=work;UnityEngine.Random.state=rng;CombatEvolutionRuntime.LastCopy=last;CombatEvolutionRuntime.LastFormation=formation;foreach(var c in copies)UnityEngine.Object.Destroy(c);foreach(var c in parties)UnityEngine.Object.Destroy(c);
  }
  Check(Framework.Singleton<GAME_WORK>.instance?.Pointer==game?.Pointer&&Framework.Singleton<BATTLE_WORK>.instance?.Pointer==work?.Pointer,"Globals restored");
  Plugin.Logger.LogInfo($"Hunt catalogue native component PASS: {definition.Id}; cumulative assertions={Assertions}; selected differences={Differences}; rendered battle=False.");
 }
 internal static object Snapshot()=>new{Assertions,Setups,Selections,Differences,Covered=Covered.ToArray(),Rows=Rows.ToArray(),RenderedBattle=false,ActualVictory=false,ActualPayout=false};
}
