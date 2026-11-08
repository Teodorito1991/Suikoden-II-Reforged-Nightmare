using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
namespace NightmareRebalance;
internal static class BossTacticsProbe
{
 internal static int Assertions,Selections,Differences;internal static readonly List<object> Rows=new();internal static readonly HashSet<string> Covered=new();
 static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool x,string message){Assertions++;if(!x)throw new InvalidOperationException("Boss tactics: "+message);}
 static bool Same(UnityEngine.Random.State a,UnityEngine.Random.State b)=>a.s0==b.s0&&a.s1==b.s1&&a.s2==b.s2&&a.s3==b.s3;
 internal static void Table(BATTLE_DATA table)
 {
  var a=BossTacticsRuntime.Audit.FirstOrDefault(x=>x.TableId==table.id);if(a==null)return;
  var game=Framework.Singleton<GAME_WORK>.instance;var work=Framework.Singleton<BATTLE_WORK>.instance;var state=Combat.State;var rng=UnityEngine.Random.state;bool quality=BossTacticsRuntime.Config.EnableCombatQuality;bool strict=DifficultyProvenance.StrictProbe,ai=CombatReforgedRuntime.Config.EnableEnemyAi,suppress=BossTacticsRuntime.Suppress;
  var copies=new List<MONSTER_DATA>();
  try{
   var g=Alloc<GAME_WORK>();g.game_data=Alloc<GAME_DATA>();g.chara_data=Alloc<CHARA_DATA>();g.chara_data.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(1);Framework.Singleton<GAME_WORK>.instance=g;
   var manager=Alloc<BattleManager>();var native=manager.Cast<IBattleManager>();var w=Alloc<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=w;
   w.battle_data=table;w.monster_party_index=a.Indices[0]+1;w.monster_party_data=table.monster_party[a.Indices[0]];w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(table.monster_data.ToArray());
   DifficultyProvenance.StrictProbe=true;var previous=DifficultyProvenance.Start(w);try{foreach(var m in a.Monsters){var local=table.monster_data[m.Index].ShallowCopy();copies.Add(local);w.monster_data[m.Index]=local;}}finally{DifficultyProvenance.End(previous);}
   int[] roster=a.OldCodes.Where(x=>x>0).ToArray();w.player_kazu=6;w.monster_kazu=roster.Length;w.pm_kazu=6+roster.Length;w.front_player=3;w.back_player=3;
   w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.pm_attack_data=new Il2CppReferenceArray<PM_ATTACK_DATA>(12);w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);w.MokuhyoKaisu=new Il2CppStructArray<byte>(12);w.b_magic_status=new();
   manager.parties=new Il2CppReferenceArray<BattleParty>(new[]{new BattleParty(0,native),new BattleParty(1,native)});
   for(int i=0;i<w.pm_kazu;i++){
    PM_DATA u=i<6?Alloc<BattlePlayerCharacter>():Alloc<BattleMonsterCharacter>();u.manager=native;u.chara_no=i;u.chara_id=(byte)(i<6?0:roster[i-6]);u.position=(byte)(i<6?i:Array.IndexOf(a.OldCodes,roster[i-6]));u.hp=600;u.max_hp=600;u.calc_para=new Il2CppStructArray<int>(new[]{120,70,60,50,80,75,60,100});
    if(i>=6){u.hp=(short)w.monster_data[u.chara_id].hp;u.max_hp=(ushort)u.hp;for(int k=0;k<7;k++)u.calc_para[k]=w.monster_data[u.chara_id].para[k];}
    w.pm_data[i]=u;w.b_chara[i]=Alloc<BATTLE_CHARA>();w.b_chara[i].chara_id=u.chara_id;w.pm_attack_data[i]=new(){curent_use_cnt=new Il2CppStructArray<byte>(new byte[]{255,255,255,255})};w.c_varia_dat[i]=new(){mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),mp=new Il2CppStructArray<byte>(4),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3)};for(int j=0;j<3;j++)w.c_varia_dat[i].item_eqp[j]=new();
   }
   g.chara_data.c_varia_dat[0]=w.c_varia_dat[0];
   // Fixtures mark presence only; no real or synthetic callback is ever invoked.
   w.monster_be_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>throw new Exception("Forbidden story callback"));
   if(a.Hooks[5]!=0)w.monster_bm_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>throw new Exception("Forbidden main callback"));
   if(a.Hooks[4]!=0)w.monster_cm_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>throw new Exception("Forbidden command callback"));
   CombatReforgedRuntime.Probe=true;CombatReforgedRuntime.ProbeWork=w;var s=new BattleState(manager){Ready=true};Combat.State=s;
   CombatReforgedRuntime.Ready(s,w,true);CombatEvolutionRuntime.Ready(s,w,true);CombatReforgedRuntime.Config.EnableEnemyAi=true;
   BossTacticsRuntime.Suppress=false;BossTacticsRuntime.Ready(w,true);var pattern=BossTacticsRuntime.Config.Patterns.Single(x=>x.TableId==table.id);var actor=w.pm_data.First(x=>x!=null&&!x.IsPlayer&&x.monster_data?.id==pattern.EnemyId).Cast<BattleMonsterCharacter>();
   Check(BossTacticsRuntime.Bound(actor),"Production admission "+table.id);Covered.Add(table.id);bool stat=s.StatsAllowed;int max=actor.max_hp;var hooks=CombatEvolutionRuntime.Hooks(w.monster_party_data.data);var ending=w.monster_be_func.Pointer;var main=w.monster_bm_func?.Pointer;
   Check(BossTacticsRuntime.Source(w,a),"Source and observed ShallowCopy provenance");actor.monster_data.hp++;Check(!BossTacticsRuntime.Source(w,a)&&!BossTacticsRuntime.Bound(actor),"Unexplained or post-admission data mutation rejected");actor.monster_data.hp--;
   var endCallback=w.monster_be_func;w.monster_be_func=null;Check(!BossTacticsRuntime.Source(w,a),"Missing resolved ending excluded");w.monster_be_func=endCallback;
   w.monster_kazu++;Check(!BossTacticsRuntime.Source(w,a),"Unexpected roster count excluded");w.monster_kazu--;
   int localIndex=actor.chara_id;var observed=w.monster_data[localIndex];var unobserved=table.monster_data[localIndex].ShallowCopy();copies.Add(unobserved);w.monster_data[localIndex]=unobserved;Check(!BossTacticsRuntime.Source(w,a),"Unobserved identical copy rejected");w.monster_data[localIndex]=observed;
   BossTacticsRuntime.Ready(w,false);Check(!BossTacticsRuntime.Bound(actor),"Compatibility exclusion");g.game_data.difficulty=2;BossTacticsRuntime.Ready(w,true);Check(!BossTacticsRuntime.Bound(actor),"Hard major/tactics excluded without weakening regular admission");g.game_data.difficulty=0;
   bool on=pattern.Enabled;pattern.Enabled=false;BossTacticsRuntime.Ready(w,true);Check(!BossTacticsRuntime.Bound(actor),"Per-boss opt-out");pattern.Enabled=on;BossTacticsRuntime.Ready(w,true);
   int changed=0;var windows=new Dictionary<string,int>();
   foreach(int hp in new[]{max,Math.Max(1,max/2)})foreach(byte counter in new byte[]{0,1,2,3,4,5})for(int seed=0;seed<12;seed++){
    actor.hp=(short)hp;w.battle_kaisuu=counter;CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();BossTacticsRuntime.Suppress=true;UnityEngine.Random.InitState(seed);int before=actor.SelectAttackMode();var afterNative=UnityEngine.Random.state;
    CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();BossTacticsRuntime.Suppress=false;UnityEngine.Random.InitState(seed);int after=actor.SelectAttackMode();
    Check(Same(afterNative,UnityEngine.Random.state),"Native RNG unchanged");Check(after==before||after is >=0 and <4&&actor.monster_data.attack_data[after].attack_prob>0&&actor.CheckMonsterAttackCnt(after)&&actor.AttackModeKyoriCheck(after),"Changed choice is native legal");
    Check(w.battle_kaisuu==counter,"Selectors never advance native counter");Selections++;if(before!=after){changed++;Differences++;}string win=BossTacticsRules.Window(pattern,counter,hp,max);windows[win+"/"+after]=windows.GetValueOrDefault(win+"/"+after)+1;
   }
   Check(actor.max_hp==max&&s.StatsAllowed==stat,"AI adds no stats or eligibility");Check(CombatEvolutionRuntime.Hooks(w.monster_party_data.data).SequenceEqual(hooks)&&w.monster_be_func.Pointer==ending&&w.monster_bm_func?.Pointer==main,"Callbacks unchanged");
   w.battle_kaisuu=2;actor.hp=(short)max;var builds=new List<int>();
   foreach(bool armored in new[]{true,false}){for(int i=0;i<6;i++){w.pm_data[i].calc_para[2]=armored?200:20;w.pm_data[i].calc_para[3]=armored?20:200;}CombatReforgedRuntime.Previous.Clear();UnityEngine.Random.InitState(991);int mode=actor.SelectAttackMode();builds.Add(mode);Check(mode is >=0 and <4&&actor.monster_data.attack_data[mode].attack_prob>0,"Build-responsive selection legal");}
   if(BossCombatQuality.Supports(table.id))Quality(manager,w,actor,pattern);
   // Exhaust original limited attacks, never replenishing or inventing a use.
   foreach(int i in Enumerable.Range(0,4).Where(i=>actor.monster_data.attack_data[i].kaisu>0))w.pm_attack_data[actor.chara_no].curent_use_cnt[i]=0;
   UnityEngine.Random.InitState(72);int depleted=actor.SelectAttackMode();Check(depleted is >=0 and <4&&actor.CheckMonsterAttackCnt(depleted),"Native use exhaustion respected");
   var budgets=new List<object>();bool statScope=s.StatsAllowed;s.StatsAllowed=false;
   foreach(int mode in Enumerable.Range(0,4).Where(i=>actor.monster_data.attack_data[i].attack_prob>0&&actor.monster_data.attack_data[i].attack>0)){
    var attack=actor.monster_data.attack_data[mode];var values=new List<int>();for(int seed=0;seed<4;seed++){UnityEngine.Random.InitState(seed);int raw=actor.CalcMonsterDamage(0,mode);Check(raw>=0,"Actual native raw damage helper");values.Add(raw);}
    budgets.Add(new{Mode=mode,attack.attack,attack.attack_prob,attack.kaisu,attack.status,Physical=(attack.attack_zokusei&16)!=0,Defense=w.pm_data[0].calc_para[2],MagicDefense=w.pm_data[0].calc_para[3],MeanRaw=values.Average(),MaxRaw=values.Max(),DamageFactorAddedByThisModule=1,ExecutedAction=false});
   }s.StatsAllowed=statScope;
   foreach(var m in a.Monsters)Check(CombatReforgedRuntime.Signature(table.monster_data[m.Index]).SequenceEqual(m.Signature),"Source asset untouched");
   Rows.Add(new{a.TableId,NativeSelectorDifferences=changed,Selections=144,Windows=windows,ArmoredSelection=builds[0],WardedSelection=builds[1],BuildChangedSelection=builds[0]!=builds[1],ExhaustedSelection=depleted,NativeRawDamageBudgets=budgets,AdmissionProductionPath=true,StoryCallbacksInvoked=false,RenderedBossPlayed=false});
  }finally{BossTacticsRuntime.Config.EnableCombatQuality=quality;DifficultyProvenance.StrictProbe=strict;CombatReforgedRuntime.Config.EnableEnemyAi=ai;BossTacticsRuntime.Suppress=suppress;CombatReforgedRuntime.Probe=false;CombatReforgedRuntime.ProbeWork=null;CombatReforgedRuntime.Clear();Combat.State=state;Framework.Singleton<GAME_WORK>.instance=game;Framework.Singleton<BATTLE_WORK>.instance=work;UnityEngine.Random.state=rng;foreach(var copy in copies)UnityEngine.Object.Destroy(copy);}
 }
 internal static readonly List<object> QualityRows=new();
 static void Quality(BattleManager manager,BATTLE_WORK w,BattleMonsterCharacter actor,BossPattern pattern)
 {
  int level=actor.monster_data.level;int max=actor.max_hp;int totalActions=0,totalTargets=0;
  foreach(string party in new[]{"Physical","Magic","Balanced","DefensiveHealing","Underprepared","WoundedBalanced"}){
   int hp=Math.Clamp(level*9+80,180,650);if(party=="Underprepared")hp=hp*4/5;
   for(int i=0;i<6;i++){
    var u=w.pm_data[i];u.max_hp=(ushort)hp;u.hp=(short)(party=="WoundedBalanced"&&i<3?hp/4:hp);
    u.calc_para[0]=level*(party=="Physical"?4:2)+30;u.calc_para[1]=level*(party=="Magic"?3:1)+30;
    u.calc_para[2]=level*(party is "Physical" or "DefensiveHealing"?3:party=="Underprepared"?1:2)+20+i*4;
    u.calc_para[3]=level*(party is "Magic" or "DefensiveHealing"?3:1)+20+i*3;
   }
   var distributions=new Dictionary<string,int>();int actionChanged=0,targetChanged=0,vanillaChanged=0;var raw=new List<int>();var modified=new List<int>();
   foreach(int bossHp in new[]{max,Math.Max(1,max/2)})foreach(byte counter in new byte[]{0,1,2,3})for(int seed=0;seed<6;seed++){
    actor.hp=(short)bossHp;w.battle_kaisuu=counter;
    void Reset(bool quality){BossTacticsRuntime.Config.EnableCombatQuality=quality;CombatReforgedRuntime.Previous.Clear();CombatReforgedRuntime.Focus.Clear();CombatMasteryRuntime.Recovery.Clear();UnityEngine.Random.InitState(400+seed);}
    Reset(false);CombatReforgedRuntime.Config.EnableEnemyAi=false;int vanilla=actor.SelectAttackMode();CombatReforgedRuntime.Config.EnableEnemyAi=true;
    Reset(false);int old=actor.SelectAttackMode();var rng=UnityEngine.Random.state;
    Reset(true);int revised=actor.SelectAttackMode();Check(Same(rng,UnityEngine.Random.state),"Quality selector consumes no extra native RNG");
    Check(revised is >=0 and <4&&actor.CheckMonsterAttackCnt(revised)&&actor.AttackModeKyoriCheck(revised),"Quality choice remains original-legal");
    Check(w.battle_kaisuu==counter,"Quality does not advance the battle counter");
    if(old!=revised)actionChanged++;if(vanilla!=revised)vanillaChanged++;
    string window=BossTacticsRules.Window(pattern,counter,bossHp,max);distributions[window+"/"+revised]=distributions.GetValueOrDefault(window+"/"+revised)+1;
    bool broad=(actor.monster_data.attack_data[revised].attack_seishitsu&0xf00) is 0x200 or 0x400 or 0x800;
    if(counter==1)Check(revised==pattern.RecoveryMode,"Native recovery selection remains basic when legal");
    if(party=="WoundedBalanced")Check(!broad,"Wounded-party fixture avoids broad native attacks table="+pattern.TableId+" counter="+counter+" revised="+revised+" basicRange="+actor.AttackModeKyoriCheck(0)+" basicUses="+actor.CheckMonsterAttackCnt(0)+" canAttack="+actor.AttackCanCheck()+" bound="+BossTacticsRuntime.Bound(actor)+" targets="+CombatReforgedRuntime.Targets(w,true).Count);
    if(seed==0){bool stats=Combat.State!.StatsAllowed;var damageRng=UnityEngine.Random.state;
     try{
      Combat.State.StatsAllowed=false;UnityEngine.Random.state=damageRng;raw.Add(actor.CalcMonsterDamage(0,revised));
      Combat.State.StatsAllowed=true;BossTacticsRuntime.Config.EnableCombatQuality=false;UnityEngine.Random.state=damageRng;int priorDamage=actor.CalcMonsterDamage(0,revised);
      BossTacticsRuntime.Config.EnableCombatQuality=true;UnityEngine.Random.state=damageRng;int qualityDamage=actor.CalcMonsterDamage(0,revised);
      Check(priorDamage==qualityDamage,"Quality adds no damage multiplier to identical native move");modified.Add(qualityDamage);
     }finally{Combat.State.StatsAllowed=stats;UnityEngine.Random.state=damageRng;BossTacticsRuntime.Config.EnableCombatQuality=true;}}

   }
   // Exercise the ACTUAL native single-target evaluator and patched production
   // hook, comparing the previous mod policy with the quality option. Not a fight.
   actor.c_mode=0;w.pm_data[0].hp=(short)(hp/4);w.pm_data[5].calc_para[2]=10;
   for(int seed=0;seed<6;seed++){
    BossTacticsRuntime.Config.EnableCombatQuality=false;CombatReforgedRuntime.Focus.Clear();UnityEngine.Random.InitState(900+seed);int old=manager.parties[1].MonsterFindKojinMokuhyo(actor.chara_no);var rng=UnityEngine.Random.state;
    BossTacticsRuntime.Config.EnableCombatQuality=true;CombatReforgedRuntime.Focus.Clear();UnityEngine.Random.InitState(900+seed);int revised=manager.parties[1].MonsterFindKojinMokuhyo(actor.chara_no);
    Check(Same(rng,UnityEngine.Random.state),"Quality targeting preserves native post-selection RNG");
    Check(CombatReforgedRuntime.LastNativeTargets.Contains(revised)&&revised>=0&&revised<6,"Quality target was returned by native legal discovery");
    if(old!=revised)targetChanged++;
   }
   totalActions+=actionChanged;totalTargets+=targetChanged;
   QualityRows.Add(new{pattern.TableId,Party=party,NativeActionComparisons=48,ActionChangesFromV119=actionChanged,ActionChangesFromNativeSelector=vanillaChanged,NativeTargetComparisons=6,TargetChangesFromV119=targetChanged,SelectionDistribution=distributions,NativeSelectedRawDamage=raw,NativeSelectedDamageWithExistingModifiers=modified,QualityDamageUnchanged=true,IllustrativePartyHp=hp,
    OriginalDamageFormulaExecuted=true,FullPartyBuild=false,ActualHealExecuted=false,NativeAnimationExecuted=false,StoryCallbackExecuted=false,FullFightOutcome="Not simulated or played"});
  }
  Check(totalActions>0&&totalTargets>0,"Quality changes actual native action and target selections for "+pattern.TableId);
 }
 internal static object Snapshot()=>new{Assertions,Selections,Differences,Covered=Covered.ToArray(),Rows=Rows.ToArray(),CombatQuality=QualityRows.ToArray(),RenderedBossPlayed=false,CallbacksInvoked=false,PlayerItemAward=false};
}

