using BepInEx;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace NightmareRebalance;
// Explicit startup-only resource audit. It never starts a fight or touches a save.
public sealed class ProductionAssetAudit : MonoBehaviour
{
 internal static bool Requested=>Plugin.Options.RunCombatSelfTest.Value&&Environment.GetCommandLineArgs().Contains("--reforged-production-audit");
 internal static bool Passed,Finished,Scheduled;internal static int Assertions;internal static readonly List<object> Rows=new();
 static readonly BossSourceAudit[] bossSources=ContentRules.Read<BossSourceAudit[]>(ContentRuntime.Resource("boss-resource-audit.json"));
 static readonly BossSourceAudit[] lucaSources=ContentRules.Read<BossSourceAudit[]>(ContentRuntime.Resource("luca-source-audit.json"));
 string[]? tables;int quitAt;int index;float started;bool pending,previousBackground;readonly List<AsyncOperationHandle<BATTLE_DATA>> retained=new();AsyncOperationHandle<BATTLE_DATA> handle=null!;
 public ProductionAssetAudit(IntPtr p):base(p){}
 static T Allocate<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 static void Check(bool ok,string label){Assertions++;if(!ok)throw new InvalidOperationException(label);}
 public void Update()
 {
  if(Finished){if(quitAt>0&&Time.frameCount>=quitAt){quitAt=Time.frameCount+60;Application.Quit();}return;}
  if(!Requested)return;
  try
  {
   if(Combat.State!=null||WarRuntime.Active||WarTacticsRuntime.Armed)throw new InvalidOperationException("Audit requires unloaded launcher.");
   if(tables==null){previousBackground=Application.runInBackground;Application.runInBackground=true;tables=CombatEvolutionRuntime.Audit.Formations.Concat(CombatMasteryRuntime.Audit.Formations).Concat(CombatRealityRuntime.Audit.Formations).Select(x=>x.TableId).Concat(bossSources.Concat(lucaSources).Select(x=>x.Data.TableId)).Concat(BossTacticsRuntime.RevolutionAudit.Select(a=>a.TableId)).Concat(CombatReforgedRuntime.Audit.Enemies.Select(x=>x.Id.Substring(0,x.Id.LastIndexOf('_')))).Distinct().ToArray();started=Time.realtimeSinceStartup;}
   if(Time.realtimeSinceStartup-started>40)throw new TimeoutException("Production resource audit timed out before resource completion.");
   // HD UI font/material bundles require the graphics player; never load in -nographics.
   // The explicit unloaded render smoke verifies them separately.
   if(!GuildNativeProbe.Resources())return;
   if(Time.realtimeSinceStartup-started>40)throw new TimeoutException("Production resource audit timed out.");
   if(!pending)
   {if(index==tables.Length){if(!HuntResourceProbe.Step())return;if(!HuntFieldSourceProbe.Step())return;if(!CastleGuildNativeProbe.Step())return;MaintenanceNativeProbe.Finish();ExpansionNativeProbe.Finish();GuildNativeProbe.Menu();Check(BossTacticsProbe.LucaRows.Count==3,"All three original Luca selector baselines");Check(BossTacticsProbe.Covered.Count==BossTacticsRuntime.Audit.Length+BossTacticsRuntime.RevolutionAudit.Length,"All audited boss action fixtures");Check(BossTacticsProbe.Differences>0,"Actual native boss selections changed");Check(GuildNativeProbe.Hunts.Count==3,"Guild rewards tested for three hunts");Check(HuntCatalogueProbe.Covered.Count==3,"All three catalogue native component fixtures executed");Passed=true;Finish();return;}Plugin.Logger.LogInfo("Production asset request: "+tables[index]);handle=Addressables.LoadAssetAsync<BATTLE_DATA>((Il2CppSystem.String)("gsd2-classic-battle-battle-"+tables[index]));pending=true;return;}
   if(!handle.IsDone)return;
   try{Check(handle.Status==AsyncOperationStatus.Succeeded,"Native asset load failed: "+tables[index]);Inspect(handle.Result);}
   finally{retained.Add(handle);pending=false;}
   index++;Checkpoint();Plugin.Logger.LogInfo("Production asset completed: "+tables[index-1]);
  }
  catch(Exception e){Plugin.Logger.LogError("Production asset audit failed: "+e);Finish();}
 }
 static void Inspect(BATTLE_DATA table)
 {
  if(table==null||table.monster_data==null||table.monster_party==null)throw new InvalidOperationException("Native battle table arrays");
  CastleGuildNativeProbe.HuntTable(table);
  BossTacticsProbe.Table(table);
  if (lucaSources.SingleOrDefault(x => x.Data.TableId == table.id) is {} luca)
  {
   InspectBoss(table, new[] { luca });
   BossTacticsProbe.Table(table, luca.Data);
   return;
  }
  MaintenanceNativeProbe.Table(table);
  ExpansionNativeProbe.Table(table);
  HuntLifecycleProbe.Table(table);
  HuntCatalogueProbe.Table(table);
  var configs=CombatEvolutionRuntime.Audit.Formations.Concat(CombatMasteryRuntime.Audit.Formations).Concat(CombatRealityRuntime.Audit.Formations).Where(a=>a.TableId==table.id).ToArray();if(configs.Length==0){if(bossSources.Any(x=>x.Data.TableId==table.id))InspectBoss(table,bossSources.Where(x=>x.Data.TableId==table.id).ToArray());return;}Check(configs.Length>0,"Original table ID");
  MeasureDamage(table);
  foreach(var a in configs)
  {
   var w=Allocate<BATTLE_WORK>();w.battle_data=table;w.monster_party_index=a.Indices[0]+1;w.monster_party_data=table.monster_party[w.monster_party_index-1];var source=w.monster_party_data;
   foreach(var m in a.Monsters){var d=table.monster_data[m.Index];if(d==null)throw new InvalidOperationException("Missing monster");Check(d.id==m.Id&&CombatReforgedRuntime.Signature(d).SequenceEqual(m.Signature),"Loaded enemy signature: "+m.Id);Rows.Add(new{Kind="LoadedMonster",d.id,d.attack_data_num,Slots=d.attack_data.Length,Hp=d.hp,MatchesAudit=true});}
   var previous=CombatEvolutionRuntime.LastCopy;var last=CombatEvolutionRuntime.LastFormation;int count=CombatEvolutionRuntime.FormationApplications;
   // Directly exercise the same guarded copy implementation, using genuine source assets.
   try{Check(CombatEvolutionRuntime.ApplyAuditedFormation(w,true,new[]{a},new[]{new EvolutionSwitch{PartyId=a.PartyId}}),"Loaded formation rejected: "+a.PartyId);Check(source.monster_code.Select(c=>(int)c).SequenceEqual(a.OldCodes),"Source unchanged");Check(w.monster_party_data.monster_code.Select(c=>(int)c).SequenceEqual(a.NewCodes),"Loaded-source production copy");}
   finally{if(CombatEvolutionRuntime.LastCopy!=null&&CombatEvolutionRuntime.LastCopy!=previous)UnityEngine.Object.Destroy(CombatEvolutionRuntime.LastCopy);CombatEvolutionRuntime.LastCopy=previous;CombatEvolutionRuntime.LastFormation=last;CombatEvolutionRuntime.FormationApplications=count;}
   Rows.Add(new{Kind="LoadedFormationProductionPath",a.TableId,a.PartyId,Passed=true,RenderedFight=false});
  }
 }
 static void InspectBoss(BATTLE_DATA table, BossSourceAudit[] expected)
 {
  Check(expected.Length>0,"Audited original boss table ID");
  foreach(var record in expected)
  {
   var a=record.Data;var party=table.monster_party[a.Indices[0]];if(party==null)throw new InvalidOperationException("Missing original boss party");Check(party.id==a.PartyId&&party.party_flg==a.Flags&&party.item_no==a.Item,"Original boss identity/flags/item");
   Check(party.monster_code.Select(x=>(int)x).SequenceEqual(a.OldCodes)&&party.monster_id.Select(x=>(int)x).SequenceEqual(a.OldIds),"Original boss roster/preload");
   Check(CombatEvolutionRuntime.Hooks(party.data).SequenceEqual(a.Hooks),"Original boss callback tuple");Check(party.data.be_data.command_num==record.CommandCount&&party.data.be_data.com_data.Length==record.CommandArrayLength,"Original boss BE commands");
   foreach(var m in a.Monsters){var native=table.monster_data[m.Index];Check(native!=null&&native.id==m.Id&&CombatReforgedRuntime.Signature(native).SequenceEqual(m.Signature),"Original loaded boss attack signature");}
   bool admitted=CombatReforgedRuntime.Audit.Bosses.Concat(CombatEvolutionRuntime.Audit.Bosses).Any(x=>x.TableId==table.id);
   Rows.Add(new{Kind="LoadedBossSourceAudit",a.TableId,a.PartyId,a.Flags,a.Hooks,a.HookNames,MonsterIds=a.Monsters.Select(m=>m.Id).ToArray(),Hp=a.Monsters.Select(m=>m.Signature[0]).ToArray(),ExistingAdmissionFamily=admitted,ModifiedByAudit=false,CallbacksExecuted=false,EndingVerified=false});
  }
 }
 static void MeasureDamage(BATTLE_DATA table)
 {
  var saved=Framework.Singleton<BATTLE_WORK>.instance;var rng=UnityEngine.Random.state;
  try
  {
   var manager=Allocate<BattleManager>();var w=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=w;var native=manager.Cast<IBattleManager>();
   w.player_kazu=6;w.monster_kazu=1;w.pm_kazu=7;w.battle_data=table;w.monster_data=table.monster_data;w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);
   var player=Allocate<BattlePlayerCharacter>();player.manager=native;player.chara_no=0;player.chara_id=0;player.hp=600;player.max_hp=600;player.calc_para=new Il2CppStructArray<int>(8);w.pm_data[0]=player;
   var actor=Allocate<BattleMonsterCharacter>();actor.manager=native;actor.chara_no=6;actor.calc_para=new Il2CppStructArray<int>(8);actor.hp=600;actor.max_hp=600;w.pm_data[6]=actor;w.b_chara[6]=Allocate<BATTLE_CHARA>();
   w.c_varia_dat[0]=new(){mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3),mp=new Il2CppStructArray<byte>(4)};
   for(int index=1;index<table.monster_data.Length;index++)
   {
    var d=table.monster_data[index];if(d==null||d.para?.Length!=7)continue;actor.chara_id=(byte)index;w.b_chara[6].chara_id=index;for(int k=0;k<7;k++)actor.calc_para[k]=d.para[k];
    int tier=d.level<=15?0:d.level<=35?1:2;int baseDefense=new[]{24,68,130}[tier],baseMagicDefense=new[]{22,65,120}[tier];
    var profile=OverhaulRuntime.Enemies.GetValueOrDefault(d.id);var pacing=CombatReforgedRuntime.Config.Enabled&&CombatReforgedRuntime.Config.EnablePacing?CombatReforgedRuntime.Config.RolePacing.FirstOrDefault(x=>x.Role==profile?.Role):null;double factor=pacing?.DamageMultiplier??profile?.DamageMultiplier??Plugin.Options.EnemyDamageMultiplier.Value;
    foreach(var budget in new[]{(Name:"Balanced",Defense:baseDefense,MagicDefense:baseMagicDefense),(Name:"Armored",Defense:baseDefense+35,MagicDefense:Math.Max(0,baseMagicDefense-15)),(Name:"Warded",Defense:Math.Max(0,baseDefense-15),MagicDefense:baseMagicDefense+35)})
    {
     player.calc_para[2]=budget.Defense;player.calc_para[3]=budget.MagicDefense;
     for(int mode=0;mode<4;mode++)
     {
      var a=d.attack_data[mode];if(a.attack_prob==0||a.attack==0||a.attack_type is not(1 or 2))continue;int total=0,max=0;
      for(int seed=0;seed<8;seed++){UnityEngine.Random.InitState(1700+seed);int raw=actor.CalcMonsterDamage(0,mode);Check(raw>=0,"Native raw damage must be nonnegative in status-free scratch fixture");total+=raw;max=Math.Max(max,raw);}
      Rows.Add(new{Kind="LoadedEnemyNativeDamageBudget",Table=table.id,Enemy=d.id,Level=d.level,Budget=budget.Name,budget.Defense,budget.MagicDefense,Mode=mode,ProbabilityWeight=a.attack_prob,Physical=(a.attack_zokusei&0x10)!=0,Multi=(a.attack_seishitsu&0xf00) is 0x200 or 0x400 or 0x800,Status=a.status,MeanRaw=total/8.0,MaxRaw=max,DamageFactor=factor,ProjectedModifiedMax=Rules.Scale(max,factor,9999),IllustrativePartyHp=new[]{115,350,570}[tier],NativeRawFormulaExecuted=true,LiveDamageHookAcceptance=false,FullBattlePlayed=false});
     }
    }
   }
  }
  finally{Framework.Singleton<BATTLE_WORK>.instance=saved;UnityEngine.Random.state=rng;}
  Check(Framework.Singleton<BATTLE_WORK>.instance?.Pointer==saved?.Pointer,"Damage budget singleton restored");
 }

 void Finish()
 {
  // Keep original resource handles until the isolated process exits. Do not unload
  // source assets while native references from the audit may still exist.
  if(tables!=null&&!(Application.isBatchMode&&Environment.GetCommandLineArgs().Contains("--reforged-test-exit")))Application.runInBackground=previousBackground;
  Finished=true;string dir=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"production-asset-audit.json"),JsonSerializer.Serialize(Snapshot(),new JsonSerializerOptions{WriteIndented=true}));Plugin.Logger.LogInfo($"Production asset audit {(Passed?"PASS":"FAILED")}: {Assertions} assertions; no battle, story callback or save executed.");
  if(Application.isBatchMode&&Environment.GetCommandLineArgs().Contains("--reforged-test-exit")){quitAt=Time.frameCount+60;Plugin.Logger.LogInfo("Isolated audit scheduled normal exit after startup settles; no campaign loaded.");}
 }
 void Checkpoint(){string dir=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"production-asset-audit.json"),JsonSerializer.Serialize(Snapshot(),new JsonSerializerOptions{WriteIndented=true}));}
 internal static object Snapshot()=>new{GuildVisual=GuildVisualNativeProbe.Snapshot(),CastleGuild=CastleGuildNativeProbe.Snapshot(),Requested,Finished,Passed,Assertions,Rows=Rows.ToArray(),Maintenance=MaintenanceNativeProbe.Snapshot(),Expansion=ExpansionNativeProbe.Snapshot(),Guild=GuildNativeProbe.Snapshot(),BossTactics=BossTacticsProbe.Snapshot(),LucaSelectors=BossTacticsProbe.LucaRows.ToArray(),RewardAvailability=HuntRewardAvailabilityProbe.Snapshot(),HuntFieldSources=HuntFieldSourceProbe.Snapshot(),HuntCatalogue=HuntCatalogueProbe.Snapshot(),HuntLifecycle=HuntLifecycleProbe.Snapshot(),HuntResources=HuntResourceProbe.Snapshot(),HuntRewards=HuntRewardProbe.Snapshot(),FullBattlePlayed=false,SaveWritten=false};
}
