using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace NightmareRebalance;
internal static class BossTacticsProbe
{
 internal static int Assertions,Selections,Differences;internal static readonly List<object> Rows=new();internal static readonly HashSet<string> Covered=new();
 static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool x,string message){Assertions++;if(!x)throw new InvalidOperationException("Boss tactics: "+message);}
 static bool Same(UnityEngine.Random.State a,UnityEngine.Random.State b)=>a.s0==b.s0&&a.s1==b.s1&&a.s2==b.s2&&a.s3==b.s3;
 internal static void Table(BATTLE_DATA table, FormationAudit? luca = null, FormationAudit? legendary = null)
 {
  var a=luca??legendary??BossTacticsRuntime.Audit.Concat(BossTacticsRuntime.RevolutionAudit).FirstOrDefault(x=>x.TableId==table.id);if(a==null)return;
  var game=Framework.Singleton<GAME_WORK>.instance;var work=Framework.Singleton<BATTLE_WORK>.instance;var state=Combat.State;var rng=UnityEngine.Random.state;bool quality=BossTacticsRuntime.Config.EnableCombatQuality;bool strict=DifficultyProvenance.StrictProbe,ai=CombatReforgedRuntime.Config.EnableEnemyAi,suppress=BossTacticsRuntime.Suppress;
  var copies=new List<MONSTER_DATA>();bool majorScope=false;StoryBossSelectionScope? outerScope=null;
  try{
   var g=Alloc<GAME_WORK>();g.game_data=Alloc<GAME_DATA>();g.chara_data=Alloc<CHARA_DATA>();g.chara_data.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(1);Framework.Singleton<GAME_WORK>.instance=g;
   var manager=Alloc<BattleManager>();var native=manager.Cast<IBattleManager>();var w=Alloc<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=w;
   w.battle_data=table;w.monster_party_index=a.Indices[0]+1;w.monster_party_data=table.monster_party[a.Indices[0]];w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(table.monster_data.ToArray());
   DifficultyProvenance.StrictProbe=true;var previous=DifficultyProvenance.Start(w);try{foreach(var m in a.Monsters){var local=table.monster_data[m.Index].ShallowCopy();copies.Add(local);w.monster_data[m.Index]=local;}}finally{DifficultyProvenance.End(previous);}
   int[] roster=a.OldCodes.Where(x=>x>0).ToArray();w.player_kazu=6;w.monster_kazu=roster.Length;w.pm_kazu=6+roster.Length;w.front_player=3;w.back_player=3;
   w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.pm_attack_data=new Il2CppReferenceArray<PM_ATTACK_DATA>(12);w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);w.MokuhyoKaisu=new Il2CppStructArray<byte>(12);w.b_magic_status=new();
   manager.parties=new Il2CppReferenceArray<BattleParty>(new[]{new BattleParty(0,native),new BattleParty(1,native)});
   for(int i=0;i<w.pm_kazu;i++){
    PM_DATA u=i<6?Alloc<BattlePlayerCharacter>():Alloc<BattleMonsterCharacter>();u.manager=native;u.chara_no=i;u.chara_id=(byte)(i<6?0:roster[i-6]);u.position=(byte)(i<6?i:luca==null&&legendary==null?Array.IndexOf(a.OldCodes,roster[i-6]):Enumerable.Range(0,a.OldCodes.Length).Where(slot=>a.OldCodes[slot]>0).ElementAt(i-6));u.hp=600;u.max_hp=600;u.calc_para=new Il2CppStructArray<int>(new[]{120,70,60,50,80,75,60,100});
    if(i>=6){u.hp=(short)w.monster_data[u.chara_id].hp;u.max_hp=(ushort)u.hp;for(int k=0;k<7;k++)u.calc_para[k]=w.monster_data[u.chara_id].para[k];}
    w.pm_data[i]=u;w.b_chara[i]=Alloc<BATTLE_CHARA>();w.b_chara[i].chara_id=u.chara_id;w.pm_attack_data[i]=new(){curent_use_cnt=new Il2CppStructArray<byte>(new byte[]{255,255,255,255})};w.c_varia_dat[i]=new(){mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),mp=new Il2CppStructArray<byte>(4),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3)};for(int j=0;j<3;j++)w.c_varia_dat[i].item_eqp[j]=new();
   }
   g.chara_data.c_varia_dat[0]=w.c_varia_dat[0];
   // Fixtures mark presence only; no real or synthetic callback is ever invoked.
   w.monster_be_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>throw new Exception("Forbidden story callback"));
   if(a.Hooks[5]!=0)w.monster_bm_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>throw new Exception("Forbidden main callback"));
   if(a.Hooks[4]!=0)w.monster_cm_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>throw new Exception("Forbidden command callback"));
   if(BossTacticsRules.Major(a.TableId))UseOriginalBindings(w,a);
   CombatReforgedRuntime.Probe=true;CombatReforgedRuntime.ProbeWork=w;var s=new BattleState(manager){Ready=true};Combat.State=s;
   CombatReforgedRuntime.Ready(s,w,true);CombatEvolutionRuntime.Ready(s,w,true);CombatReforgedRuntime.Config.EnableEnemyAi=true;
   BossTacticsRuntime.Suppress=false;BossTacticsRuntime.Ready(w,true);
   if (legendary != null)
   {
    ObserveLegendary(w, s, legendary);
    return;
   }
   if (luca != null)
   {
    ObserveLuca(manager, w, luca);
    return;
   }
   var pattern=BossTacticsRuntime.Config.Patterns.Concat(BossTacticsRuntime.RevolutionConfig.Patterns).Single(x=>x.TableId==table.id);var actor=w.pm_data.First(x=>x!=null&&!x.IsPlayer&&x.monster_data?.id==pattern.EnemyId).Cast<BattleMonsterCharacter>();
   Check(BossTacticsRuntime.Bound(actor),"Production admission "+table.id);Covered.Add(table.id);bool stat=s.StatsAllowed;int max=actor.max_hp;var hooks=CombatEvolutionRuntime.Hooks(w.monster_party_data.data);var ending=w.monster_be_func.Pointer;var main=w.monster_bm_func?.Pointer;
   Check(BossTacticsRuntime.Source(w,a),"Source and observed ShallowCopy provenance");actor.monster_data.hp++;Check(!BossTacticsRuntime.Source(w,a)&&!BossTacticsRuntime.Bound(actor),"Unexplained or post-admission data mutation rejected");actor.monster_data.hp--;
   var endCallback=w.monster_be_func;w.monster_be_func=null;Check(!BossTacticsRuntime.Source(w,a),"Missing resolved ending excluded");w.monster_be_func=endCallback;
   w.monster_kazu++;Check(!BossTacticsRuntime.Source(w,a),"Unexpected roster count excluded");w.monster_kazu--;
   int localIndex=actor.chara_id;var observed=w.monster_data[localIndex];var unobserved=table.monster_data[localIndex].ShallowCopy();copies.Add(unobserved);w.monster_data[localIndex]=unobserved;Check(!BossTacticsRuntime.Source(w,a),"Unobserved identical copy rejected");w.monster_data[localIndex]=observed;
   BossTacticsRuntime.Ready(w,false);Check(!BossTacticsRuntime.Bound(actor),"Compatibility exclusion");g.game_data.difficulty=2;BossTacticsRuntime.Ready(w,true);Check(!BossTacticsRuntime.Bound(actor),"Hard major/tactics excluded without weakening regular admission");g.game_data.difficulty=0;
   bool on=pattern.Enabled;pattern.Enabled=false;BossTacticsRuntime.Ready(w,true);Check(!BossTacticsRuntime.Bound(actor),"Per-boss opt-out");pattern.Enabled=on;BossTacticsRuntime.Ready(w,true);
   if(BossTacticsRules.Major(table.id))
   {
    VerifyMajorCommandScope(w,actor);
    StoryBossDispatchGuard.Prefix(actor,out outerScope);majorScope=true;
    Check(StoryBossSelectionScope.Allows(actor),"Major discretionary production scope admitted");
   }
   bool revolution=BossRevolutionRules.Supports(table.id);
   if(revolution)
   {
    var uses=w.pm_attack_data[actor.chara_no].curent_use_cnt;
    for(int i=0;i<4;i++)uses[i]=actor.monster_data.attack_data[i].kaisu>0
        ? (byte)actor.monster_data.attack_data[i].kaisu : byte.MaxValue;
    var beforeUses=uses.ToArray();
    Check(BossTacticsRuntime.UsesOriginalTargets(actor),"Original targets bypass resampling");
    BossTacticsRuntime.Suppress=true;UnityEngine.Random.InitState(301);
    int baseline=manager.parties[1].MonsterFindKojinMokuhyo(actor.chara_no);
    var baselineRng=UnityEngine.Random.state;
    BossTacticsRuntime.Suppress=false;UnityEngine.Random.InitState(301);
    int actual=manager.parties[1].MonsterFindKojinMokuhyo(actor.chara_no);
    Check(actual>=0&&actual<w.player_kazu&&actual==baseline&&Same(baselineRng,UnityEngine.Random.state),"Original legal target and RNG preserved");
    Check(beforeUses.SequenceEqual(uses.ToArray()),"Target selection preserves native limited uses");
   }
   int changed=0;var windows=new Dictionary<string,int>();
   foreach(int hp in new[]{max,Math.Max(1,max/2)})foreach(byte counter in new byte[]{0,1,2,3,4,5})for(int seed=0;seed<(revolution?3:12);seed++){
    actor.hp=(short)hp;w.battle_kaisuu=counter;CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();BossTacticsRuntime.Suppress=true;UnityEngine.Random.InitState(seed);int before=actor.SelectAttackMode();var afterNative=UnityEngine.Random.state;
    CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();BossTacticsRuntime.Suppress=false;UnityEngine.Random.InitState(seed);int after=actor.SelectAttackMode();
    Check(Same(afterNative,UnityEngine.Random.state),"Native RNG unchanged");Check(after==before||after is >=0 and <4&&actor.monster_data.attack_data[after].attack_prob>0&&actor.CheckMonsterAttackCnt(after)&&actor.AttackModeKyoriCheck(after),"Changed choice is native legal");
    Check(w.battle_kaisuu==counter,"Selectors never advance native counter");Selections++;if(before!=after){changed++;Differences++;}string win=BossTacticsRules.Window(pattern,counter,hp,max);windows[win+"/"+after]=windows.GetValueOrDefault(win+"/"+after)+1;
   }
   if(revolution)
   {
    Check(changed>0,"New policy changes an original selection");
    Check(!CombatReforgedRuntime.Plans.ContainsKey(actor.Pointer),"No new stat plan");
    var marker=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>throw new Exception("Forbidden scripted command"));
    w.monster_cm_func=marker;
    Check(!BossTacticsRuntime.Bound(actor),"New scripted command excluded");
    w.monster_cm_func=null;
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
   EffectiveStoryStats(w,s);
   Rows.Add(new{a.TableId,NativeSelectorDifferences=changed,Selections=windows.Values.Sum(),Windows=windows,ArmoredSelection=builds[0],WardedSelection=builds[1],BuildChangedSelection=builds[0]!=builds[1],ExhaustedSelection=depleted,NativeRawDamageBudgets=budgets,AdmissionProductionPath=true,StoryCallbacksInvoked=false,RenderedBossPlayed=false});
  }finally{if(majorScope)StoryBossDispatchGuard.Finalizer(outerScope,null);BossTacticsRuntime.Config.EnableCombatQuality=quality;DifficultyProvenance.StrictProbe=strict;CombatReforgedRuntime.Config.EnableEnemyAi=ai;BossTacticsRuntime.Suppress=suppress;CombatReforgedRuntime.Probe=false;CombatReforgedRuntime.ProbeWork=null;CombatReforgedRuntime.Clear();Combat.State=state;Framework.Singleton<GAME_WORK>.instance=game;Framework.Singleton<BATTLE_WORK>.instance=work;UnityEngine.Random.state=rng;foreach(var copy in copies)UnityEngine.Object.Destroy(copy);}
 }
 static void EffectiveStoryStats(BATTLE_WORK work,BattleState state)
 {
  foreach(var actor in work.pm_data.Where(x=>x!=null&&!x.IsPlayer).Select(x=>x.Cast<BattleMonsterCharacter>()))
  {
   int beforeHp=actor.max_hp;
   Combat.ScaleEnemy(actor);int finalHp=actor.max_hp;Combat.ScaleEnemy(actor);
   Check(actor.max_hp==finalHp,"Story HP applies once per actor");
   double hpFactor=state.StatsAllowed?CombatReforgedRuntime.Factor(state,actor,Plugin.Options.EnemyHpMultiplier.Value,true):1;
   Check(finalHp==Rules.Scale(beforeHp,hpFactor,32767),"Actual story HP path matches selected replacement factor");
   int mode=Enumerable.Range(0,4).First(i=>actor.monster_data.attack_data[i].attack>0&&actor.monster_data.attack_data[i].attack_type is 1 or 2);
   bool allowed=state.StatsAllowed;int raw,modified;
   try
   {
    state.StatsAllowed=false;UnityEngine.Random.InitState(6221);raw=actor.CalcMonsterDamage(0,mode);
    state.StatsAllowed=allowed;UnityEngine.Random.InitState(6221);modified=actor.CalcMonsterDamage(0,mode);
   }
   finally{state.StatsAllowed=allowed;}
   double damageFactor=allowed?CombatReforgedRuntime.Factor(state,actor,Plugin.Options.EnemyDamageMultiplier.Value,false):1;
   Check(modified==Rules.Scale(raw,damageFactor,9999),"Actual native story damage uses one factor, not stacked globals");
   Rows.Add(new{Kind="EffectiveStoryStats",Table=work.battle_data.id,Enemy=actor.monster_data.id,
    SourceActorHp=beforeHp,FinalActorHp=finalHp,HpFactor=hpFactor,RawNativeDamage=raw,FinalNativeDamage=modified,
    DamageFactor=damageFactor,StatsEligible=allowed,NativeDamageHookExecuted=true,StoryCallbacksInvoked=false,RenderedBattle=false});
  }
 }

 static void VerifyMajorCommandScope(BATTLE_WORK work,BattleMonsterCharacter actor)
 {
  int command=actor.c_koudou;var ending=work.monster_be_func;
  try
  {
   actor.c_koudou=0;
   Check(!StoryBossSelectionScope.Allows(actor),"Major selector outside native dispatch remains native");
   foreach(sbyte entry in new sbyte[]{0,7,11})
   {
    actor.c_koudou=entry;StoryBossDispatchGuard.Prefix(actor,out var saved);
    try
    {
     actor.c_koudou=0;
     Check(StoryBossSelectionScope.Allows(actor)==(entry==0),"Immutable major entry classification survives fallthrough");
     int native,selected;BossTacticsRuntime.Suppress=true;UnityEngine.Random.InitState(351);native=actor.SelectAttackMode();var rng=UnityEngine.Random.state;
     BossTacticsRuntime.Suppress=false;UnityEngine.Random.InitState(351);selected=actor.SelectAttackMode();
     Check(Same(rng,UnityEngine.Random.state),"Major command fence adds no RNG");
     if(entry!=0)Check(selected==native,"Scripted entry leaves actual native selector unchanged");
     StoryBossDispatchGuard.Prefix(actor,out var nested);
     try{Check(!StoryBossSelectionScope.Allows(actor),"Nested major dispatch cannot reuse owner");}
     finally{StoryBossDispatchGuard.Finalizer(nested,null);}
     Check(StoryBossSelectionScope.Allows(actor)==(entry==0),"Nested cleanup restores prior major owner");
     if(entry==0)
     {
      work.monster_be_func=null;Check(!StoryBossSelectionScope.Allows(actor),"Changed ending callback refuses major selection");work.monster_be_func=ending;
      work.monster_be_func=work.monster_single_func;Check(!StoryBossSelectionScope.Allows(actor),"Wrong non-null ending method refuses major selection");work.monster_be_func=ending;
      var opening=work.monster_single_func;work.monster_single_func=null;Check(StoryBossSelectionScope.Allows(actor),"Native opening completion keeps discretionary choices");work.monster_single_func=opening;
      var main=work.monster_bm_func;work.monster_bm_func=ending;Check(!StoryBossSelectionScope.Allows(actor),"Unknown main callback remains native");work.monster_bm_func=main;
      var state=Combat.State;Combat.State=new BattleState(actor.manager.Cast<BattleManager>()){Ready=true};
      Check(!StoryBossSelectionScope.Allows(actor),"New battle generation rejects stale command owner");Combat.State=state;
     }
    }
    finally{StoryBossDispatchGuard.Finalizer(saved,null);}
   }
   Check(StoryBossSelectionScope.Current==null,"Major finalizer clears scope");
   Rows.Add(new{Kind="StoryBossCommandGuard",Table=work.battle_data.id,Discretionary=true,ScriptedEntriesRetained=true,NestedAndStaleRejected=true,NativeSelectors=true,DispatcherInvoked=false,StoryCallbacksInvoked=false});
  }
  finally{actor.c_koudou=(sbyte)command;work.monster_be_func=ending;}
 }
 internal static readonly List<object> LucaRows = new();
 internal static readonly List<object> LegendaryRows = new();

 static void UseOriginalBindings(BATTLE_WORK work, FormationAudit source)
 {
  BattleOverlayDelegate? At(int i) => source.Hooks[i] == 0 ? null : area_func.GetBattleOverlayFunction(source.Hooks[i]);
  work.monster_single_func = At(0);
  work.monster_ps_func = At(1);
  work.monster_pe_func = At(2);
  work.monster_be_func = At(3);
  work.monster_cm_func = At(4);
  work.monster_bm_func = At(5);
 }

 static void ObserveLegendary(BATTLE_WORK work, BattleState state, FormationAudit source)
 {
  UseOriginalBindings(work, source);
  var ending = work.monster_be_func;
  var command = work.monster_cm_func;
  var phaseEnd = work.monster_pe_func;
  Check(LegendaryBossRuntime.Ready(state, work, true), "Legendary original-source admission " + source.TableId);
  var actor = work.pm_data.First(u => u != null && !u.IsPlayer &&
      u.chara_id == LegendaryBossRules.ActorIndex(source.TableId)).Cast<BattleMonsterCharacter>();
  foreach (var player in work.pm_data.Take(work.player_kazu))
  {
   player.calc_para[2] = 100;
   player.calc_para[3] = 45;
  }
  actor.action_kaisu = 0;
  actor.position = 2;
  EffectiveStoryStats(work,state);
  int changes = 0;
  var phases = new Dictionary<string,int>();
  foreach (var phase in source.Monsters.Where(m => LegendaryBossRules.AcceptsPhase(source.TableId, m.Index)))
  {
   // The scratch actor presents the exact alternate native template. This does
   // not execute the original animation or the phase-transition callbacks.
   actor.chara_id = (byte)phase.Index;
   work.b_chara[actor.chara_no].chara_id = actor.chara_id;
   actor.InitMonsterAttackCnt();
   var uses = work.pm_attack_data[actor.chara_no].curent_use_cnt;
   var beforeUses = uses.ToArray();
   int phaseChanges = 0;
   foreach (byte round in new byte[] { 0, 1, 2, 3, 4, 5 })
   for (int seed = 0; seed < 12; seed++)
   {
    work.battle_kaisuu = round;
    UnityEngine.Random.InitState(seed);
    int original = actor.SelectAttackMode();
    var rng = UnityEngine.Random.state;
    UnityEngine.Random.InitState(seed);
    LegendaryBossDispatchPatch.Prefix(actor, out var prior);
    try
    {
     Check(LegendaryBossRuntime.Current != null, "Legendary dispatcher scope admitted");
     int selected = actor.SelectAttackMode();
     Check(selected == original || selected is >= 0 and < 4 && actor.CheckMonsterAttackCnt(selected) &&
         actor.AttackModeKyoriCheck(selected), "Legendary substitution stays native-legal");
     Check(Same(rng, UnityEngine.Random.state), "Legendary selection adds no RNG draws");
     if (selected != original) phaseChanges++;
    }
    finally { LegendaryBossDispatchPatch.Finalizer(prior, null); }
   }
   Check(uses.SequenceEqual(beforeUses), "Legendary selector never advances uses");
   phases.Add(phase.Id, phaseChanges);
   changes += phaseChanges;
   foreach (int i in Enumerable.Range(0, 4).Where(i => actor.monster_data.attack_data[i].kaisu > 0))
    uses[i] = 0;
   LegendaryBossDispatchPatch.Prefix(actor, out var saved);
   try
   {
    work.battle_kaisuu = 1;
    int depleted = actor.SelectAttackMode();
    Check(depleted < 0 || actor.CheckMonsterAttackCnt(depleted), "Legendary depleted moves stay depleted");
   }
   finally { LegendaryBossDispatchPatch.Finalizer(saved, null); }
  }
  Check(changes > 0, "Legendary policy changes an original decision " + source.TableId);
  Check(work.monster_be_func?.Pointer == ending?.Pointer &&
      work.monster_cm_func?.Pointer == command?.Pointer &&
      work.monster_pe_func?.Pointer == phaseEnd?.Pointer && !state.StatsAllowed,
      "Legendary selector does not touch phase, command, ending or stat eligibility");
  work.monster_be_func = null;
  LegendaryBossDispatchPatch.Prefix(actor, out var mismatch);
  try
  {
   int before = LegendaryBossRuntime.Changes;
   actor.SelectAttackMode();
   Check(LegendaryBossRuntime.Changes == before, "Legendary missing callback rejects selection");
  }
  finally { LegendaryBossDispatchPatch.Finalizer(mismatch, null); }
  work.monster_be_func = ending;
  LegendaryRows.Add(new { source.TableId, PhaseSelectorChanges = phases,
      SelectorChanges = changes, NativeSelectors = true, NativeDispatcher = false, RenderedBattle = false });
 }

 // Baseline only. Counter and HP inputs belong to scratch state, not a story battle.
 static void ObserveLuca(BattleManager manager, BATTLE_WORK work, FormationAudit source)
 {
  Check(BossTacticsRuntime.Source(work, source), "Luca roster, native copies and callback registry");
  var actor = work.pm_data.First(u => u != null && u.monster_data?.id == source.Monsters.Last().Id)
      .Cast<BattleMonsterCharacter>();
  Check(!BossTacticsRuntime.Bound(actor) && !CombatReforgedRuntime.Plans.ContainsKey(actor.Pointer),
      "Luca remains outside the existing tactics and profile admissions");
  var end = work.monster_be_func.Pointer;
  var command = work.monster_cm_func?.Pointer;
  var sequences = new List<object>();
  foreach (int seed in Enumerable.Range(0, 8))
  {
   var modes = new List<int>();
   var targets = new List<int>();
   UnityEngine.Random.State baselineRng = default;
   foreach (bool modAi in new[] { false, true })
   {
    UnityEngine.Random.InitState(seed + 2100);
    CombatReforgedRuntime.Config.EnableEnemyAi = modAi;
    for (byte counter = 0; counter < 6; counter++)
    {
     work.battle_kaisuu = counter;
     actor.hp = (short)(actor.max_hp - counter * 200);
     int mode = actor.SelectAttackMode();
     Check(mode is >= 0 and < 3 && actor.CheckMonsterAttackCnt(mode) &&
         actor.AttackModeKyoriCheck(mode), "Luca original mode is available");
     actor.c_mode = (sbyte)mode;
     int target = manager.parties[1].MonsterFindKojinMokuhyo(actor.chara_no);
     Check(target >= 0 && target < work.player_kazu, "Luca native target is in the player party");
     Check(work.battle_kaisuu == counter, "Luca selectors leave the supplied counter unchanged");
     if (!modAi)
     {
      modes.Add(mode);
      targets.Add(target);
     }
     else
      Check(mode == modes[counter] && target == targets[counter], "Existing AI leaves Luca selectors unchanged");
    }
    if (!modAi) baselineRng = UnityEngine.Random.state;
    else Check(Same(baselineRng, UnityEngine.Random.state), "Luca selector sequences preserve native RNG");
   }
   sequences.Add(new { Modes = modes, TargetSlots = targets });
  }
  Check(work.monster_be_func.Pointer == end && work.monster_cm_func?.Pointer == command,
      "Luca callback bindings left untouched");
  LucaRows.Add(new { source.TableId, Actor = actor.monster_data.id, EnemyCount = work.monster_kazu,
      Sequences = sequences, AreaTargets = source.TableId == "boss_luc3" ? ObserveLucaArea(manager, work, actor) : null, ManagerPrerequisite = source.TableId == "boss_luc3" ? ObserveManagerQueue(manager, work, actor) : null, Dispatcher = source.TableId == "boss_luc3" ? ObserveLucaCommitment(work, actor, source) : null, ExistingAiChanges = 0, StoryCallbacksInvoked = false, TacticalTurnsExecuted = false });
 }

 // Verify the native reader and queue ownership without entering the camera tail of BattleMakeData.
 static object ObserveManagerQueue(BattleManager manager, BATTLE_WORK work, BattleMonsterCharacter actor)
 {
  Check(manager.battle_work?.Pointer == work.Pointer &&
      work.action_top == null && work.action_sp == null && work.b_read_work == null,
      "Fresh scratch manager owns the battle work and its queue");
  var priorReader = Framework.Singleton<b_read>.instance;
  var reader = Alloc<b_read>();
  var readerWork = new B_READ_WORK();
  var rng = UnityEngine.Random.state;
  work.b_read_work = readerWork;
  try
  {
   // This reader is bound to the scratch manager only. The singleton is untouched.
   reader.InitBRead(manager.Cast<IBattleManager>());
   var table = readerWork.read_data_table;
   Check(table != null && table.Length == 18 && table.All(entry => entry == 0),
       "Native reader allocated an empty 18-slot table");
   table![0] = 19;
   table[17] = 23;
   reader.InitBReadTable();
   Check(readerWork.read_data_table.Pointer == table.Pointer && table.All(entry => entry == 0) &&
       readerWork.cd_status == 2 && readerWork.seek_flg == 1,
       "BattleMakeData reader initialization cleared its own table");
   for (int cycle = 0; cycle < 2; cycle++)
   {
    var head = manager.CreateActionData((sbyte)actor.chara_no);
    var tail = actor.GetActionData();
    Check(head.Pointer != tail.Pointer && work.action_top?.Pointer == head.Pointer &&
        work.action_sp?.Pointer == tail.Pointer && head.next_action_data?.Pointer == tail.Pointer &&
        tail.next_action_data == null && head.chara_no == actor.chara_no &&
        tail.chara_no == actor.chara_no && head.data.All(value => value == 0),
        "Manager and actor append to one owned native action chain");
    tail.CreateReActionData(2);
    Check(tail.reaction_num == 2 && head.reaction_data == null,
        "Native reaction allocation belongs to the tail only");
    tail.DeleteActionData();
    Check(tail.reaction_data == null, "Native deletion clears owned reaction references");
    head.next_action_data = null;
    work.action_top = null;
    work.action_sp = null;
   }
   Check(Same(rng, UnityEngine.Random.state) &&
       Framework.Singleton<b_read>.instance?.Pointer == priorReader?.Pointer,
       "Reader and queue operations preserve RNG and the original reader singleton");
   return new { NativeReaderSlots = 18, NativeQueueCycles = 2,
       NativeReactionCleanup = true, BattleMakeDataInvoked = false };
  }
  finally
  {
   work.action_top = null;
   work.action_sp = null;
   work.b_read_work = null;
  }
 }

 // Only populated during one synchronous call from the isolated production audit.
 [ThreadStatic] static DispatchProbe? activeDispatch;
 sealed class DispatchProbe(BATTLE_WORK work, BattleMonsterCharacter actor, int alternative)
 {
  internal readonly BATTLE_WORK Work = work;
  internal readonly BattleMonsterCharacter Actor = actor;
  internal readonly int Alternative = alternative;
  internal readonly List<(int Original, int Selected)> Selections = new();
 }

 internal static void ObserveDispatchSelection(BattleMonsterCharacter actor, ref int selected)
 {
  var probe = activeDispatch;
  if (probe == null || !CombatReforgedRuntime.Probe ||
      CombatReforgedRuntime.ProbeWork?.Pointer != probe.Work.Pointer ||
      probe.Actor.Pointer != actor.Pointer) return;
  var rng = UnityEngine.Random.state;
  int original = selected;
  int alternative = probe.Alternative;
  if (probe.Selections.Count == 0 && selected is >= 0 and < 3 &&
      alternative is >= 0 and < 3 && actor.CheckMonsterAttackCnt(alternative) &&
      actor.AttackModeKyoriCheck(alternative))
  {
   var move = actor.monster_data.attack_data[alternative];
   if (move.attack_prob > 0 &&
       ((move.attack_zokusei & 16) != 0 ? actor.AttackCanCheck() : actor.MagicCanCheck()))
    selected = alternative;
  }
  Check(Same(rng, UnityEngine.Random.state), "Controlled selector adds no RNG draws");
  probe.Selections.Add((original, selected));
 }

 static List<object> ObserveLucaCommitment(BATTLE_WORK work, BattleMonsterCharacter actor, FormationAudit source)
 {
  var rows = new List<object>();
  work.p_status_data = new Il2CppReferenceArray<PLAYER_STATUS_DATA>(12);
  for (int slot = 0; slot < work.pm_kazu; slot++) work.p_status_data[slot] = new();
  var scratch = Framework.Singleton<GAME_WORK>.instance;
  scratch.party_data = new() { party_cha_no = new Il2CppStructArray<byte>(6) };
  scratch.chara_data.c_kotei_dat = new Il2CppReferenceArray<C_KOTEI_DAT>(new[] { new C_KOTEI_DAT() });
  var game = scratch.game_data;
  game.luka_hp = 4321;
  UseOriginalBindings(work, source);
  var ending = work.monster_be_func.Pointer;
  actor.position = 2;
  foreach (var (condition, alternate) in new[]
  {
   ("Full", -1), ("Full", 0), ("Full", 2),
   ("Wounded", 0), ("Wounded", 2), ("Defensive", 2),
   ("Silenced", 2), ("NoTargets", 2), ("Scoped", -1)
  })
  {
   for (int slot = 0; slot < work.player_kazu; slot++)
   {
    var player = work.pm_data[slot];
    player.position = (byte)(slot + 1);
    player.hp = (short)(condition == "NoTargets" ? 0 :
        condition == "Wounded" ? 300 : player.max_hp);
    player.dead_flg = (byte)(condition == "NoTargets" ? 1 : 0);
    player.status = condition == "NoTargets" ? 0x800000u : 0;
    player.calc_para[2] = player.calc_para[3] = condition == "Defensive" ? 160 : 60;
   }
   actor.status = condition == "Silenced" ? 2048u : 0;
   actor.action_num = 3;
   actor.action_kaisu = 0;
   actor.finished_flg = 0;
   actor.c_koudou = 0;
   actor.InitMonsterAttackCnt();
   bool scoped = condition == "Scoped";
   if (scoped)
   {
    work.battle_kaisuu = 1;
    Check(!LegendaryBossRuntime.Ready(Combat.State!, work, true), "Solo Luca has no production opt-in");
    Check(LegendaryBossRuntime.Ready(Combat.State!, work, true, source), "Solo Luca exact scratch binding");
    foreach (sbyte excluded in new sbyte[] { 11, 7 })
    {
     actor.c_koudou = excluded;
     LegendaryBossDispatchPatch.Prefix(actor, out var prior);
     try
     {
      // Simulate fallthrough with command 0 at selection. Admission must
      // still consult the immutable entry-time classification.
      actor.c_koudou = 0;
      int selected = 1;
      LegendaryBossRuntime.Select(actor, ref selected);
      Check(selected == 1, "Luca entry command excludes entire invocation");
     }
     finally { LegendaryBossDispatchPatch.Finalizer(prior, null); }
    }
    LegendaryBossDispatchPatch.Prefix(actor, out var outer);
    try
    {
     var originalScope = LegendaryBossRuntime.Current;
     LegendaryBossDispatchPatch.Prefix(actor, out var inner);
     try
     {
      LegendaryBossDispatchPatch.Prefix(actor, out var third);
      try { Check(LegendaryBossRuntime.Current == null, "Nested Luca dispatch cannot reacquire scope"); }
      finally { LegendaryBossDispatchPatch.Finalizer(third, null); }
     }
     finally { LegendaryBossDispatchPatch.Finalizer(inner, null); }
     Check(ReferenceEquals(LegendaryBossRuntime.Current, originalScope), "Nested dispatcher restores outer scope");
    }
    finally { LegendaryBossDispatchPatch.Finalizer(outer, null); }
   }
   var counts = work.pm_attack_data[actor.chara_no].curent_use_cnt;
   var usesBefore = counts.ToArray();
   byte counter = work.battle_kaisuu;
   work.action_top = null;
   work.action_sp = null;
   for (int i = 0; i < work.MokuhyoKaisu.Length; i++) work.MokuhyoKaisu[i] = 0;
   UnityEngine.Random.InitState(2100);
   var probe = new DispatchProbe(work, actor, alternate);
   int result;
   try
   {
    activeDispatch = probe;
    result = actor.monster_attack_sub();
   }
   finally { activeDispatch = null; }
   var action = work.action_top;
   bool committed = result == 1 && action != null;
   Check(actor.action_kaisu == 1 && work.battle_kaisuu == counter &&
       game.luka_hp == 4321 && work.monster_be_func.Pointer == ending &&
       work.monster_cm_func == null && usesBefore.SequenceEqual(counts),
       "Luca dispatcher preserves handoff, callbacks, round and unlimited uses");
   if (condition == "NoTargets")
   {
    Check(!committed && action == null && probe.Selections.Count == 10,
        "No targets exhaust native retry loop without a committed action");
    rows.Add(new { Condition = condition, Requested = alternate, Result = result,
        Committed = false, SelectorCalls = probe.Selections.Count });
    continue;
   }
   Check(committed && action!.chara_no == actor.chara_no &&
       action.reaction_num > 0 && action.next_action_data == null,
       $"Luca dispatcher produced one owned action (result={result}, mode={actor.c_mode})");
   if (condition == "Silenced")
    Check(actor.c_mode == 0 && probe.Selections.All(x => x.Original == x.Selected),
        "Silence blocks the requested special and preserves the native basic attack");
   else if (alternate >= 0)
    Check(actor.c_mode == alternate, "Controlled legal choice survived native action construction");
   if (scoped)
   {
    var baseline = System.Text.Json.JsonSerializer.SerializeToElement(rows[0]);
    Check(baseline.GetProperty("FinalMode").GetInt32() == 1 && actor.c_mode == 2 &&
        LegendaryBossRuntime.Changes == 1 && LegendaryBossRuntime.Current == null,
        "Scoped selector replaces the original column choice, commits row, and clears scope");
   }
   var data = action!.data.Select(x => (int)x).ToArray();
   if (actor.c_mode is 1 or 2)
    Check(data[0] == actor.c_mode && action.action_type == 10, "Native special action encodes selected move");
   var reactions = Enumerable.Range(0, action.reaction_num).Select(i =>
   {
    var reaction = action.reaction_data[i];
    return new { Slot = reaction.mokuhyo, reaction.reaction_type, reaction.damage };
   }).ToArray();
   Check(reactions.All(x => x.Slot >= 0 && x.Slot < work.player_kazu),
       "Committed reactions resolve only player slots in this fixture");
   Check(reactions.Any(x => x.damage > 0), "Native action recorded positive damage");
   var move = actor.monster_data.attack_data[actor.c_mode];
   rows.Add(new { Condition = condition, Requested = alternate, Result = result, Committed = true,
       FinalMode = actor.c_mode, action.action_type, Data = data, Reactions = reactions,
       Shape = move.attack_seishitsu & 0xf00, DisplayIndex = move.attack_disp_func_index,
       SelectorCalls = probe.Selections.Count,
       Selections = probe.Selections.Select(x => new { x.Original, x.Selected }).ToArray() });
  }
  return rows;
 }

 [UnmanagedFunctionPointer(CallingConvention.Winapi)]
 delegate int NativeAreaSelector(IntPtr party, int actor, ref IntPtr targets, IntPtr methodInfo);

 // Observe the actual target buffers used by the solo encounter's area attacks.
 static List<object> ObserveLucaArea(BattleManager manager, BATTLE_WORK work, BattleMonsterCharacter actor)
 {
  var rows = new List<object>();
  var party = manager.parties[1];
  // This startup-only probe runs after Plugin's exact GameAssembly/metadata check.
  // Use the native by-reference array ABI. The generated wrapper returned the
  // original zero-filled buffer even when the native result reported two slots.
  var module = NativeLibrary.Load("GameAssembly.dll");
  var column = Marshal.GetDelegateForFunctionPointer<NativeAreaSelector>(module + 0x1a75600);
  var row = Marshal.GetDelegateForFunctionPointer<NativeAreaSelector>(module + 0x1a759d0);
  try
  {
   actor.position = 2;
   foreach (bool sparse in new[] { false, true })
   {
    for (int slot = 0; slot < work.player_kazu; slot++)
    {
     var player = work.pm_data[slot];
     bool unavailable = sparse && slot % 2 == 0;
     player.position = (byte)(slot + 1);
     player.hp = (short)(unavailable ? 0 : player.max_hp);
     player.dead_flg = (byte)(unavailable ? 1 : 0);
     player.status = unavailable ? 0x800000u : 0;
    }
    foreach (sbyte mode in new sbyte[] { 1, 2 })
    {
     actor.c_mode = mode;
     var groups = new List<int[]>();
     for (int seed = 0; seed < 8; seed++)
     {
      UnityEngine.Random.InitState(2200 + seed);
      var buffer = new Il2CppStructArray<int>(6);
      IntPtr nativeBuffer = buffer.Pointer;
      int count = (mode == 1 ? column : row)(party.Pointer, actor.chara_no, ref nativeBuffer, IntPtr.Zero);
      GC.KeepAlive(buffer);
      Check(nativeBuffer == buffer.Pointer, "Luca area selector retains caller-owned target buffer");
      Check(count > 0 && count <= buffer.Length, "Luca native area target count");
      var targets = buffer.Take(count).ToArray();
      Check(targets.Distinct().Count() == count && targets.All(slot =>
          slot >= 0 && slot < work.player_kazu && work.pm_data[slot].hp > 0 &&
          work.pm_data[slot].MokuhyoCanCheck()), $"Luca area target mismatch: mode={mode}, sparse={sparse}, slots={string.Join(",", targets)}");
      groups.Add(targets);
     }
     rows.Add(new { Mode = mode, SparseParty = sparse, Groups = groups });
    }
   }
   return rows;
  }
  finally { NativeLibrary.Free(module); }
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
    if(counter==1&&actor.CheckMonsterAttackCnt(pattern.RecoveryMode)&&actor.AttackModeKyoriCheck(pattern.RecoveryMode))Check(revised==pattern.RecoveryMode,"Native recovery selection prefers the configured legal move");
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
   QualityRows.Add(new{pattern.TableId,Party=party,NativeActionComparisons=48,ActionChangesFromQualityDisabled=actionChanged,ActionChangesFromNativeSelector=vanillaChanged,NativeTargetComparisons=6,TargetChangesFromQualityDisabled=targetChanged,SelectionDistribution=distributions,NativeSelectedRawDamage=raw,NativeSelectedDamageWithExistingModifiers=modified,QualityDamageUnchanged=true,IllustrativePartyHp=hp,
    OriginalDamageFormulaExecuted=true,FullPartyBuild=false,ActualHealExecuted=false,NativeAnimationExecuted=false,StoryCallbackExecuted=false,FullFightOutcome="Not simulated or played"});
  }
  Check(totalActions>0&&totalTargets>0,"Quality changes actual native action and target selections for "+pattern.TableId);
 }
 internal static object Snapshot()=>new{Assertions,Selections,Differences,Covered=Covered.ToArray(),Rows=Rows.ToArray(),CombatQuality=QualityRows.ToArray(),RenderedBossPlayed=false,CallbacksInvoked=false,PlayerItemAward=false};
}
