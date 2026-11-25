using BepInEx;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace NightmareRebalance;
internal static class CombatMasteryProbe
{
 internal static int Assertions,CadenceDifferences,FormationChanges,UniteChanges,RuneChanges;internal static bool Passed;internal static readonly List<object> Results=new();
 static T Allocate<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool value,string name){Assertions++;if(!value)throw new InvalidOperationException($"Mastery native assertion {Assertions}: {name}");}
 internal static void Run()
 {
  if(!Plugin.Options.RunCombatSelfTest.Value)return;
  if(Combat.State!=null||WarRuntime.Active||WarTacticsRuntime.Armed)throw new InvalidOperationException("Mastery probe requires unloaded startup.");
  var saved=Framework.Singleton<BATTLE_WORK>.instance;var savedGame=Framework.Singleton<GAME_WORK>.instance;var rng=UnityEngine.Random.state;var objects=new List<UnityEngine.Object>();
  var manager=Allocate<BattleManager>();var w=Allocate<BATTLE_WORK>();var nativeManager=manager.Cast<IBattleManager>();
  bool enabled=CombatMasteryRuntime.Config.Enabled,ai=CombatReforgedRuntime.Config.EnableEnemyAi;int fcount=CombatEvolutionRuntime.FormationApplications,ucount=CombatEvolutionRuntime.UniteApplications,bcount=CombatEvolutionRuntime.AdditionalBossAdmissions;
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
   CombatMasteryRuntime.Config.Enabled=true;CombatReforgedRuntime.Probe=true;CombatReforgedRuntime.ProbeWork=w;CombatReforgedRuntime.Work=w.Pointer;CombatReforgedRuntime.Config.EnableEnemyAi=true;
   foreach(var a in CombatMasteryRuntime.Audit.Formations)
   {
    var table=UnityEngine.ScriptableObject.CreateInstance<BATTLE_DATA>();objects.Add(table);table.id=a.TableId;table.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(a.Monsters.Max(x=>x.Index)+1);
    foreach(var m in a.Monsters){var d=CombatReforgedProbe.Template(m.Id,m.Signature);objects.Add(d);d.attack_data_num=0;table.monster_data[m.Index]=d;}
    var source=UnityEngine.ScriptableObject.CreateInstance<MONSTER_PARTY_DATA>();objects.Add(source);source.id=a.PartyId;source.party_flg=a.Flags;source.item_no=a.Item;source.monster_kazu=a.OldCodes.Count(c=>c>0);source.monster_id=new Il2CppStructArray<sbyte>(a.OldIds.Select(x=>(sbyte)x).ToArray());source.monster_code=new Il2CppStructArray<sbyte>(a.OldCodes.Select(x=>(sbyte)x).ToArray());source.data=new(){monster_position=new Il2CppStructArray<short>(a.Positions.Select(x=>(short)x).ToArray()),battle_start_func_index=a.Hooks[0],be_data=new(){command_num=0,com_data=new Il2CppReferenceArray<BE_COM_DATA>(0)}};
    table.monster_party=new Il2CppReferenceArray<MONSTER_PARTY_DATA>(a.Indices.Max()+1);foreach(int i in a.Indices)table.monster_party[i]=source;w.battle_data=table;w.monster_party_data=source;w.monster_party_index=a.Indices[0]+1;
    var toggle=CombatMasteryRuntime.Config.Formations.Single(x=>x.PartyId==a.PartyId);bool oldToggle=toggle.Enabled;toggle.Enabled=false;Check(!CombatMasteryRuntime.ApplyFormation(w,true),"formation per-entry opt-out");toggle.Enabled=oldToggle;
    bool forms=CombatMasteryRuntime.Config.EnableFormations;CombatMasteryRuntime.Config.EnableFormations=false;Check(!CombatMasteryRuntime.ApplyFormation(w,true),"formation feature opt-out");CombatMasteryRuntime.Config.EnableFormations=forms;
    Check(!CombatMasteryRuntime.ApplyFormation(w,false),"compatibility opt-out");Check(CombatMasteryRuntime.ApplyFormation(w,true),a.PartyId+" production formation");FormationChanges++;
    var copy=w.monster_party_data;Check(copy.Pointer!=source.Pointer,"disposable party");Check(copy.monster_code.Select(x=>(int)x).SequenceEqual(a.NewCodes),"new codes");Check(source.monster_code.Select(x=>(int)x).SequenceEqual(a.OldCodes),"source codes intact");Check(copy.monster_id.Select(x=>(int)x).SequenceEqual(CombatEvolutionRules.Types(a.NewCodes)),"native preload type list");Check(copy.monster_kazu==source.monster_kazu&&copy.item_no==source.item_no&&copy.party_flg==source.party_flg,"count/rewards/flags");Check(CombatEvolutionRuntime.Hooks(copy.data).SequenceEqual(a.Hooks),"callbacks intact");Check(copy.data.Pointer!=source.data.Pointer&&copy.data.monster_position.Pointer!=source.data.monster_position.Pointer,"deep party data");Check(!CombatMasteryRuntime.ApplyFormation(w,true),"copy not mistaken for source");
    w.monster_party_data=source;source.data.battle_end_func_index=1;Check(!CombatMasteryRuntime.ApplyFormation(w,true),"unexpected story callback excluded");source.data.battle_end_func_index=0;
    Results.Add(new{Kind="ProductionFormationCopy",a.PartyId,a.OldHp,a.NewHp,CallbacksExecuted=false});
   }
   var actor=w.pm_data[6].Cast<BattleMonsterCharacter>();for(int i=7;i<12;i++)w.pm_data[i]=null!;
   foreach(var b in CombatReforgedRuntime.Audit.Bosses.Concat(CombatEvolutionRuntime.Audit.Bosses))
   {
    var d=CombatReforgedProbe.Template(b.EnemyId,b.Signature);objects.Add(d);d.attack_data_num=0;w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(new[]{d});actor.hp=(short)d.hp;actor.max_hp=d.hp;
    var table=UnityEngine.ScriptableObject.CreateInstance<BATTLE_DATA>();objects.Add(table);table.id=b.TableId;w.battle_data=table;
    var party=UnityEngine.ScriptableObject.CreateInstance<MONSTER_PARTY_DATA>();objects.Add(party);party.id=b.PartyId;party.monster_kazu=1;party.party_flg=b.Flags;party.monster_id=new Il2CppStructArray<sbyte>(new sbyte[]{1,0,0,0,0,0});party.monster_code=new Il2CppStructArray<sbyte>(new sbyte[]{0,1,0,0,0,0});party.data=new(){battle_start_func_index=b.Hooks[0],phase_start_func_index=b.Hooks[1],phase_end_func_index=b.Hooks[2],battle_end_func_index=b.Hooks[3],command_func_index=b.Hooks[4],battle_main_func_index=b.Hooks[5],be_data=new()};w.monster_party_data=party;w.monster_party_index=1;
    w.monster_be_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>0);w.monster_bm_func=b.Hooks[5]==0?null:(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>0);
    var state=new BattleState(manager){Ready=true,Allowed=false};Combat.State=state;CombatReforgedRuntime.Ready(state,w,true);CombatEvolutionRuntime.Ready(state,w,true);
    Check(CombatReforgedRuntime.Plans.ContainsKey(actor.Pointer),"boss admission "+b.TableId);Check(CombatEvolutionRuntime.BossTables.ContainsKey(actor.Pointer),"phase binding");
    int differences=0;
    foreach(int hp in new[]{(int)d.hp,Math.Max(1,d.hp/3)})for(int seed=0;seed<16;seed++)
    {
     actor.hp=(short)hp;
     int[] before=new int[4],after=new int[4];
     CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();CombatMasteryRuntime.Config.Enabled=false;UnityEngine.Random.InitState(seed);
     for(int n=0;n<4;n++)before[n]=actor.SelectAttackMode();
     CombatReforgedRuntime.Previous.Clear();CombatMasteryRuntime.Recovery.Clear();CombatMasteryRuntime.Config.Enabled=true;UnityEngine.Random.InitState(seed);
     for(int n=0;n<4;n++){after[n]=actor.SelectAttackMode();Check(after[n] is >=0 and <4&&d.attack_data[after[n]].attack_prob>0&&actor.CheckMonsterAttackCnt(after[n])&&actor.AttackModeKyoriCheck(after[n]),"native cadence selection legal");if(before[n]!=after[n])differences++;}

    }
    CadenceDifferences+=differences;Results.Add(new{Kind="ActualNativeBossCadenceSelection",b.TableId,Differences=differences,CallbacksExecuted=false});
    Combat.State=null;
   }
   var player=w.pm_data[0].Cast<BattlePlayerCharacter>();
   w.unite_data_table=new Il2CppReferenceArray<UNITE_DATA>(51);
   foreach(var a in CombatMasteryRuntime.Audit.Unites){var x=a.Signature;w.unite_data_table[a.Id]=new(){name=x[0],text=x[1],info=(byte)x[2],atrb=(byte)x[3],player_kazu=(byte)x[4],func_no=(byte)x[5],hosei=x[6],m_status=(byte)x[7],m_status_prob=(byte)x[8],p_status=(byte)x[9],p_status_prob=(byte)x[10],player_id=new Il2CppStructArray<byte>(x.Skip(11).Select(n=>(byte)n).ToArray())};}
   var originals=w.unite_data_table;Check(CombatMasteryRuntime.ApplyUnites(w)==3,"three additional production Unite copies");UniteChanges=3;
   foreach(var a in CombatMasteryRuntime.Audit.Unites){var c=w.GetUniteData(a.Id);Check(c.hosei==a.Effect&&c.Pointer!=originals[a.Id].Pointer,"actual native lookup reads copy");Check(CombatEvolutionRuntime.UniteSignature(originals[a.Id]).SequenceEqual(a.Signature),"original Unite intact");Check(c.player_id.Pointer!=originals[a.Id].player_id.Pointer,"deep Unite membership");Check(c.p_status==originals[a.Id].p_status&&c.m_status==originals[a.Id].m_status&&c.func_no==originals[a.Id].func_no,"Unite statuses/function preserved");}
   Check(CombatMasteryRuntime.ApplyUnites(w)==0,"Unite idempotence");
   bool uniteEnabled=CombatMasteryRuntime.Config.EnableUniteOptions;CombatMasteryRuntime.Config.EnableUniteOptions=false;w.unite_data_table=originals;Check(CombatMasteryRuntime.ApplyUnites(w)==0,"Unite feature opt-out");CombatMasteryRuntime.Config.EnableUniteOptions=uniteEnabled;
   var first=CombatMasteryRuntime.Audit.Unites[0];int oldValue=originals[first.Id].hosei;originals[first.Id].hosei=oldValue+1;Check(CombatMasteryRuntime.ApplyUnites(w)==2,"custom Unite value preserved");Check(w.GetUniteData(first.Id).hosei==oldValue+1&&w.GetUniteData(first.Id).Pointer==originals[first.Id].Pointer,"no overwrite of custom Unite");originals[first.Id].hosei=oldValue;
   Components(w,manager,player,actor,objects);
   Check(CadenceDifferences>0,"actual native choices changed");
  }
  finally
  {
   (CombatMasteryRuntime.FormationApplications,CombatMasteryRuntime.RuneApplications,CombatMasteryRuntime.UniteApplications,CombatMasteryRuntime.CadenceSelections)=counters;
   CombatMasteryRuntime.Recovery.Clear();
   CombatMasteryRuntime.Config.Enabled=enabled;CombatReforgedRuntime.Config.EnableEnemyAi=ai;CombatReforgedRuntime.Probe=false;CombatReforgedRuntime.ProbeWork=null;CombatReforgedRuntime.Clear();CombatEvolutionRuntime.BossTables.Clear();Combat.State=null;Framework.Singleton<BATTLE_WORK>.instance=saved;Framework.Singleton<GAME_WORK>.instance=savedGame;UnityEngine.Random.state=rng;
   if(CombatEvolutionRuntime.LastCopy!=null&&CombatEvolutionRuntime.LastCopy!=previousCopy)UnityEngine.Object.Destroy(CombatEvolutionRuntime.LastCopy);CombatEvolutionRuntime.LastCopy=previousCopy;CombatEvolutionRuntime.LastFormation=previousFormation;CombatEvolutionRuntime.FormationApplications=fcount;CombatEvolutionRuntime.UniteApplications=ucount;CombatEvolutionRuntime.AdditionalBossAdmissions=bcount;
   foreach(var o in objects)UnityEngine.Object.Destroy(o);
  }
  Check(Framework.Singleton<BATTLE_WORK>.instance?.Pointer==saved?.Pointer&&Framework.Singleton<GAME_WORK>.instance?.Pointer==savedGame?.Pointer,"globals restored");var r=UnityEngine.Random.state;Check(r.s0==rng.s0&&r.s1==rng.s1&&r.s2==rng.s2&&r.s3==rng.s3,"RNG restored");
  Passed=true;string dir=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"native-mastery-probe.json"),JsonSerializer.Serialize(Snapshot(),new JsonSerializerOptions{WriteIndented=true}));Plugin.Logger.LogInfo($"Combat Mastery probe PASS: {Assertions} assertions; formations={FormationChanges}; native cadence differences={CadenceDifferences}; Unites={UniteChanges}. No rendered battle/story/save executed.");
 }
 internal static object Snapshot()=>new{Ran=Assertions>0,Passed,Assertions,CadenceDifferences,FormationChanges,UniteChanges,RuneChanges,Results=Results.ToArray(),RenderedBattlePlayed=false,StoryCallbacksInvoked=false};
 static void Components(BATTLE_WORK w,BattleManager manager,BattlePlayerCharacter player,BattleMonsterCharacter actor,List<UnityEngine.Object> objects)
 {
  Combat.State=null;CombatMasteryRuntime.Recovery.Clear();
  var sample=CombatReforgedRuntime.Audit.Enemies.First();var d=CombatReforgedProbe.Template(sample.Id,sample.Signature);objects.Add(d);d.aisyo=new Il2CppStructArray<byte>(16);d.taisei=new Il2CppStructArray<byte>(4);w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(new[]{d});actor.chara_id=0;actor.status=0;player.status=0;
  for(int seed=0;seed<32;seed++)
  {
   w.c_varia_dat[0].mon_eqp[0]=0;w.c_varia_dat[0].mon_eqp[1]=0;UnityEngine.Random.InitState(seed);int normal=player.CalcPlayerDamage(6);
   w.c_varia_dat[0].mon_eqp[0]=82;UnityEngine.Random.InitState(seed);int one=player.CalcPlayerDamage(6);Check(one==normal*2,"Double-Edged native outgoing tradeoff");
   w.c_varia_dat[0].mon_eqp[1]=82;Check(player.EquipEmblCheck(82,0)==2,"native equipment counts duplicate emblems");UnityEngine.Random.InitState(seed);Check(player.CalcPlayerDamage(6)==one,"damage branch treats duplicate passive presence as boolean");
  }
  w.c_varia_dat[0].mon_eqp[0]=0;w.c_varia_dat[0].mon_eqp[1]=0;
  foreach(var a in CombatMasteryRuntime.Audit.Runes.Concat(RpgEvolutionRuntime.Options.PhysicalRunes?RpgEvolutionRuntime.Audit.Runes:Array.Empty<PhysicalRuneAudit>()))
  {
   var x=a.Signature;var magic=new MAGI_DATA{name=x[0],text=x[1],atrb=(byte)x[2],eff_val=(short)x[3],uni_no=(byte)x[4],uni_magic=(byte)x[5],info=new Il2CppStructArray<byte>(x.Skip(6).Take(2).Select(v=>(byte)v).ToArray()),status=new Il2CppStructArray<byte>(x.Skip(8).Select(v=>(byte)v).ToArray())};
   bool Apply()=>CombatMasteryRuntime.Spell(a.Id,magic)||RpgEvolutionRuntime.Spell(a.Id,magic);
   Check(Apply()&&magic.eff_val==a.Effect,"production guarded physical rune");RuneChanges++;Check(!Apply(),"physical rune idempotence");
   for(int resistance=0;resistance<4;resistance++)for(int seed=0;seed<16;seed++)
   {d.aisyo[10]=(byte)resistance;UnityEngine.Random.InitState(seed);int physical=player.CalcPlayerDamage(6);UnityEngine.Random.InitState(seed);int actual=player.PlayerCalcSpcDamage(magic,6);Check(actual==CombatMasteryRules.PhysicalRuneDamage(physical,resistance,a.Effect),"actual native physical rune/resistance/rounding");}
   magic.eff_val=(short)(a.Effect+1);Check(!Apply()&&magic.eff_val==a.Effect+1,"custom physical rune value preserved");magic.eff_val=(short)x[3];magic.text++;Check(!Apply(),"physical rune metadata mismatch");
  }
  d.aisyo[10]=0;
  foreach(var a in CombatMasteryRuntime.Audit.Unites.Concat(RpgEvolutionRuntime.Options.Unites?RpgEvolutionRuntime.Audit.Unites:Array.Empty<UniteAudit>()))
  {
   var x=a.Signature;w.unite_data_table[a.Id]=new(){name=x[0],text=x[1],info=(byte)x[2],atrb=(byte)x[3],player_kazu=(byte)x[4],func_no=(byte)x[5],hosei=a.Effect,m_status=(byte)x[7],m_status_prob=(byte)x[8],p_status=(byte)x[9],p_status_prob=(byte)x[10],player_id=new Il2CppStructArray<byte>(x.Skip(11).Select(n=>(byte)n).ToArray())};
   for(int seed=0;seed<16;seed++)
   {
    actor.hp=10000;actor.max_hp=10000;actor.status=0;actor.dead_flg=0;UnityEngine.Random.InitState(seed);int physical=player.CalcPlayerDamage(6);var action=new ACTION_DATA{chara_no=0};action.CreateReActionData(1);UnityEngine.Random.InitState(seed);player.UniteAttackDataSet(a.Id,action,6,0,100);Check(action.GetReActionData().damage==physical*a.Effect/100,"actual native Unite damage helper reads new coefficient");
   }
  }
  var game=Framework.Singleton<GAME_WORK>.instance;game.chara_data.c_kotei_dat=new Il2CppReferenceArray<C_KOTEI_DAT>(1);game.chara_data.c_kotei_dat[0]=new(){mon_zoku_aisyo=new Il2CppStructArray<byte>(4)};
  var spell=new MAGI_DATA{atrb=1,eff_val=80};
  foreach(int power in new[]{0,80,180})foreach(int magic in new[]{0,70,150})foreach(int defense in new[]{0,50,200})
  {
   spell.eff_val=(short)power;player.calc_para[1]=magic;actor.calc_para[3]=defense;actor.status=0;player.status=0;int modified=player.CalcMagicDamageAtrbHosei(1,power);int expected=Math.Max(0,modified+magic-defense);
   Check(player.PlayerCalcMagicDamage(spell,6)==expected,"native spell power plus Magic minus MagicDefense");actor.status=0x40000000;Check(player.PlayerCalcMagicDamage(spell,6)==expected/3,"native guarding magic reduction");actor.status=0;
   Check(player.PlayerClacMagicKaifuku(spell)==modified+magic,"native healing spell power plus Magic");
  }
  BalanceAuditProbe.PlayerDamage(player,actor);
  foreach(var a in CombatReforgedRuntime.Audit.Enemies)
  {
   var native=CombatReforgedProbe.Template(a.Id,a.Signature);objects.Add(native);native.attack_data_num=0;w.monster_data[0]=native;actor.hp=400;actor.max_hp=400;actor.dead_flg=0;actor.status=0;
   Check(CombatReforgedRuntime.Signature(native).SequenceEqual(a.Signature),"original serialized count0 signature "+a.Id);
   foreach(int invalid in new[]{-1,1,2,3,5}){native.attack_data_num=invalid;Check(CombatReforgedRuntime.Signature(native).Length==0,"unsupported count rejected");}native.attack_data_num=0;
   Combat.State=new BattleState(manager){Ready=true,Allowed=true};CombatReforgedRuntime.Ready(Combat.State,w,true);Check(CombatReforgedRuntime.Plans.ContainsKey(actor.Pointer),"production regular admission accepts serialized count0 "+a.Id);
  }
  Combat.State=null;
  Results.Add(new{Kind="SerializedActionCountRegression",Enemies=CombatReforgedRuntime.Audit.Enemies.Length,DeclaredCount=0,Slots=4,FullAttackSignaturesRequired=true,ActualSceneLoaded=false});
  bool require=Plugin.Options.RequireSurvivingCharacter.Value,regen=Plugin.Options.EnableRuneRegeneration.Value;int amount=Plugin.Options.RuneChargesPerVictory.Value;var priority=Plugin.Options.RuneRecoveryPriority.Value;int maxLevel=Plugin.Options.RuneRecoveryMaxLevel.Value;int restored=CombatReforgedDiagnostics.RuneRestored;string outcome=CombatReforgedDiagnostics.Outcome;
  try
  {
   Plugin.Options.RequireSurvivingCharacter.Value=true;Plugin.Options.EnableRuneRegeneration.Value=true;Plugin.Options.RuneChargesPerVictory.Value=1;Plugin.Options.RuneRecoveryPriority.Value=RecoveryPriority.LowestDepletedLevel;
   Plugin.Options.RuneRecoveryMaxLevel.Value=4;
   for(int i=0;i<6;i++){w.pm_data[i].hp=400;w.pm_data[i].dead_flg=0;w.c_varia_dat[i].mp=new Il2CppStructArray<byte>(new byte[]{0x37,0x25,0x13,0x01});}
   w.pm_data[1].hp=0;w.pm_data[1].dead_flg=1;w.c_varia_dat[2]=w.c_varia_dat[0];w.c_varia_dat[3].mp=new Il2CppStructArray<byte>(new byte[]{0x77,0x55,0x33,0x11});w.c_varia_dat[4].mp[0]=0x77;
   Combat.State=new BattleState(manager){Ready=true,Allowed=true};Combat.Win(manager);Combat.Win(manager);Check(Combat.State.Eligible.Count==4,"victory component survivor snapshot and native data-pointer deduplication");w.pm_data[1].hp=400;w.pm_data[1].dead_flg=0;
   manager.battle_exit99();Check(w.c_varia_dat[0].mp[0]==0x47,"actual native completion stage triggers production one-charge recovery");Check(w.c_varia_dat[1].mp[0]==0x37,"defeated at victory excluded even if later revived");Check(w.c_varia_dat[3].mp[0]==0x77&&w.c_varia_dat[3].mp[1]==0x55,"full charges preserved");Check(w.c_varia_dat[4].mp[0]==0x77&&w.c_varia_dat[4].mp[1]==0x35,"recovery advances to next depleted level");
   manager.battle_exit99();Combat.Win(manager);manager.battle_exit99();Check(w.c_varia_dat[0].mp[0]==0x47,"repeated native completion and late victory do not restore twice");
   Combat.State=new BattleState(manager){Ready=true,Allowed=true};Combat.State.Victory.MarkNonVictory();Combat.Win(manager);manager.battle_exit99();Check(w.c_varia_dat[0].mp[0]==0x47,"non-victory component gate cannot be rearmed");
   Combat.State=new BattleState(manager){Ready=true,Allowed=true};manager.battle_exit99();Check(w.c_varia_dat[0].mp[0]==0x47,"completion without victory restores nothing");
   Combat.State=new BattleState(manager){Ready=true,Allowed=true};CombatReforgedDiagnostics.Outcome="InProgress";
   Combat.NonVictory(manager);Combat.Win(manager);
   Check(CombatReforgedDiagnostics.Outcome=="NonVictorySignal"&&!Combat.State.SnapshotTaken,"late win cannot reopen non-victory diagnostics or hunt snapshot");
   Check(!Combat.State.Victory.TryConsume(),"late win cannot reopen non-victory recovery");
   Combat.State=new BattleState(manager){Ready=true,Allowed=true,ReportWritten=true};CombatReforgedDiagnostics.Outcome="VictoryRewardsCompleted";Combat.Win(manager);Combat.NonVictory(manager);
   Check(CombatReforgedDiagnostics.Outcome=="VictoryRewardsCompleted"&&!Combat.State.SnapshotTaken,"closed report cannot be rewritten by outcome callbacks");
   var foreign=Allocate<BattleManager>();Combat.State=new BattleState(manager){Ready=true,Allowed=true};CombatReforgedDiagnostics.Outcome="InProgress";Combat.Win(foreign);Combat.NonVictory(foreign);
   Check(CombatReforgedDiagnostics.Outcome=="InProgress"&&!Combat.State.SnapshotTaken,"foreign manager cannot change active battle outcome");
   Combat.Win(manager);Check(CombatReforgedDiagnostics.Outcome=="VictorySignal"&&Combat.State.SnapshotTaken,"legitimate production victory still snapshots survivors");

   Plugin.Options.RuneRecoveryMaxLevel.Value=2;
   for(int i=0;i<6;i++)w.c_varia_dat[i].mp=new Il2CppStructArray<byte>(new byte[]{0x77,0x55,0x23,0x01});
   Combat.State=new BattleState(manager){Ready=true,Allowed=true};Combat.Win(manager);manager.battle_exit99();
   Check(w.c_varia_dat[0].mp.SequenceEqual(new byte[]{0x77,0x55,0x23,0x01}),"native completion respects Full-profile high-tier recovery boundary");
   w.c_varia_dat[0].mp[1]=0x45;
   Combat.State=new BattleState(manager){Ready=true,Allowed=true};Combat.Win(manager);manager.battle_exit99();
   Check(w.c_varia_dat[0].mp.SequenceEqual(new byte[]{0x77,0x55,0x23,0x01}),"native completion still restores low-tier sustain");
   Results.Add(new{Kind="SyntheticVictoryComponentAndActualNativeExitStage",RecoveryTierBoundaryVerified=true,EarnedVictory=false,RewardsExecuted=false,SceneLoaded=false,StoryCallbacksExecuted=false,SaveWritten=false});
  }
  finally{Combat.State=null;Plugin.Options.RequireSurvivingCharacter.Value=require;Plugin.Options.EnableRuneRegeneration.Value=regen;Plugin.Options.RuneChargesPerVictory.Value=amount;Plugin.Options.RuneRecoveryPriority.Value=priority;Plugin.Options.RuneRecoveryMaxLevel.Value=maxLevel;CombatReforgedDiagnostics.RuneRestored=restored;CombatReforgedDiagnostics.Outcome=outcome;}
 }

}


