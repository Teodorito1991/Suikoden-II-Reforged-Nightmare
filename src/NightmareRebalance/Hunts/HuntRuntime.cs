namespace NightmareRebalance;
internal static class HuntRuntime
{
 internal static ExpansionFile Config=new();internal static FormationAudit Audit=new();
 internal static HuntDefinition[] Definitions=Array.Empty<HuntDefinition>();internal static HuntCatalogueFile Catalogue=new();internal static HuntDefinition? Active;
 internal static HuntDefinition[] BrowseOrder=Array.Empty<HuntDefinition>();
 internal static HuntOverride Settings(HuntDefinition d)=>Catalogue.Hunts.Single(x=>x.Id==d.Id);
 internal static bool ActiveRoster=>Active is {} a&&HuntCatalogueRules.RosterMatches(a,Units.Values);
 internal static IntPtr Work,Party;internal static readonly Dictionary<IntPtr,string> Units=new();
 static FlatRewardGate reward=new();internal static int Applications,RewardCalculations;internal static object? LastOutcome;
 internal static readonly List<object> Calculations=new();
 internal static void Initialize()
 {
  try
  {
   Config=ExpansionRules.Parse(OverhaulRuntime.Load("first-expansion.json"));
   Audit=ContentRules.Read<FormationAudit>(ContentRuntime.Resource("hunt-audit.json"));
   Definitions=ContentRules.Read<HuntDefinition[]>(ContentRuntime.Resource("hunt-catalogue-audit.json"));
   var defaults=HuntCatalogueRules.Parse(ContentRuntime.Resource("hunt-catalogue.json"),Definitions);
   Catalogue=HuntCatalogueRules.Parse(OverhaulRuntime.Load("hunt-catalogue.json"),Definitions,defaults);
   BrowseOrder=Definitions.OrderBy(d=>GuildScreenRules.Card(d.Id).Tier).ToArray();
   Plugin.Logger.LogInfo($"Expansion: {Definitions.Length} guarded native Hunt contracts; enabled={Config.EnableExperimentalHunt}.");
  }
  catch(Exception e){Definitions=BrowseOrder=Array.Empty<HuntDefinition>();Catalogue=new();Config.EnableExperimentalHunt=false;Plugin.Logger.LogWarning("Hunt catalogue disabled: "+e.Message);}
 }
 internal static void Detach(){Work=Party=IntPtr.Zero;Units.Clear();Active=null;HuntAccess.SelectedWork=IntPtr.Zero;HuntAccess.Selected=null;}
 internal static void Reset(){GuildItemRuntime.Reset();Work=Party=IntPtr.Zero;Units.Clear();Active=null;reward=new();LastOutcome=null;HuntRewardObservation.Credits.Clear();Calculations.Clear();}
 internal static bool Formation(BATTLE_WORK w,bool compatible)
 {
  if(!Config.EnableExperimentalHunt||!compatible||!HuntAccess.Authorized(w)||!HuntLaunchProof.Matches(w)||!DifficultyProvenance.AllowedForFeature(out _,out _))return false;
  var definition=HuntAccess.Selected??(HuntLaunchProof.FixtureProof&&DifficultyProvenance.StrictProbe&&UnityEngine.Application.isBatchMode?Definitions.Single(x=>x.Id=="sun-kings-retinue"):null);
  if(definition==null||!Settings(definition).Enabled)return false;var audit=definition.Audit;
  bool applied=CombatEvolutionRuntime.ApplyAuditedFormation(w,compatible,new[]{audit},new[]{new EvolutionSwitch{PartyId=audit.PartyId,Enabled=true}});
  if(applied){Active=definition;Work=w.Pointer;Party=w.monster_party_data.Pointer;Applications++;}return applied;
 }
 internal static void Ready(BattleState s,BATTLE_WORK w)
 {
  if(!Config.EnableExperimentalHunt||!s.Allowed||w.Pointer!=Work||w.monster_party_data?.Pointer!=Party||Active==null||w.monster_party_index!=Active.Audit.Indices.Single()+1||w.battle_data?.id!=Active.Audit.TableId||w.pm_data==null)return;
  foreach(var u in w.pm_data.Where(u=>u!=null&&!u.IsPlayer)){
   if(!CombatReforgedRuntime.Plans.TryGetValue(u.Pointer,out var p)||!Active.Units.Any(x=>x.Id==p.Id)){Units.Clear();return;}Units[u.Pointer]=p.Id;
  }
  if(!ActiveRoster){Units.Clear();return;}
  foreach(var u in Units)CombatReforgedRuntime.Plans[u.Key]=CombatReforgedRuntime.Plans[u.Key] with{Behavior=HuntCatalogueRules.Unit(Active,Settings(Active),Config,u.Value).Behavior};
  HuntSessionRuntime.Begin();
  CombatRealityRuntime.Event("HuntPrototype","Admitted",Active.Id,new{Experimental=true,Meaning="Guarded admission observed; rendered completion requires lifecycle evidence"});
 }
 internal static double? Factor(PM_DATA u,bool hp)
 {
  if(!Config.EnableExperimentalHunt||Active is not {} a||Combat.State?.Work!=Work||!Units.TryGetValue(u.Pointer,out var id))return null;
  double baseFactor=hp?HuntCatalogueRules.Unit(a,Settings(a),Config,id).HpMultiplier:HuntCatalogueRules.Damage(a,Settings(a),Config);
  if(ArmyProgressionRuntime.HuntModifiersEnabled&&ArmyProgressionRuntime.ActiveHuntModifier!=HuntRiskModifier.None)
   baseFactor*=ArmyProgressionRules.HuntModifierStatFactor(ArmyProgressionRuntime.ActiveHuntModifier,hp);
  return baseFactor;
 }
 internal static void Reward(BATTLE_WORK w,EXIT_WORK exit)
 {
  if(!Config.EnableExperimentalHunt||Work!=w.Pointer||Party!=w.monster_party_data?.Pointer||!ActiveRoster||Combat.State is not{Allowed:true,SnapshotTaken:true}||Combat.State.Work!=Work||CombatReforgedDiagnostics.Outcome!="VictorySignal")return;
  int bonus=HuntCatalogueRules.Bonus(Active!,Settings(Active!),Config);
  var hRank=Reforged2Rules.HunterRankForClears(ArmyProgressionRuntime.Current.HuntsCompleted);
  bonus=(int)Math.Round(bonus*Reforged2Rules.HunterPotchBonusMultiplier(hRank),MidpointRounding.AwayFromZero);
  if(ArmyProgressionRuntime.HuntModifiersEnabled)
   bonus=ArmyProgressionRules.HuntModifierBonusPotch(bonus,ArmyProgressionRuntime.ActiveHuntModifier);
  int before=exit.monster_okane;exit.monster_okane=reward.Apply(before,bonus);RewardCalculations++;HuntSessionRuntime.Current?.Prepare(exit.monster_okane);
  GuildItemRuntime.Prepare(w,exit);
  LastOutcome=new{Before=before,After=exit.monster_okane,HuntId=Active?.Id,VictoryBonusPotch=bonus,Meaning="Native reward field prepared; receipt/inventory and field return not inferred"};
 }
 internal static void RecalculateReward()=>reward=new();
 internal static void ObserveCalculation(BATTLE_WORK w,int native,int configured)
 {
  if(w.Pointer!=Work||!ActiveRoster||Calculations.Count>=16)return;
  var defeated=new List<string>();if(w.dead_monster_data!=null&&w.monster_data!=null)for(int i=0;i<Math.Min((int)w.dead_monster,w.dead_monster_data.Length);i++){int index=w.dead_monster_data[i]?.chara_id??255;if(index<w.monster_data.Length&&w.monster_data[index]!=null)defeated.Add(w.monster_data[index].id);}
  Calculations.Add(new{NativeCalculated=native,ConfiguredFactor=Plugin.Options.PotchMultiplier.Value,AfterConfiguredFactor=configured,Prepared=w.exit_work?.monster_okane,AddedBonus=(long)(w.exit_work?.monster_okane??configured)-configured,DefeatedEnemies=defeated.ToArray(),SeparateAcquiredMoney=w.b_magic_status?.player_get_money,VictorySnapshot=Combat.State?.SnapshotTaken??false,Outcome=CombatReforgedDiagnostics.Outcome,ActualPurseCreditInferred=false});
 }
 internal static object Snapshot()=>new{Config.EnableExperimentalHunt,SelectedHuntId=HuntGuildRuntime.SelectedId,ActiveHuntId=Active?.Id,Catalogue=Definitions.Select(d=>new{d.Id,d.Name,d.Field,Enabled=Settings(d).Enabled,Bonus=HuntCatalogueRules.Bonus(d,Settings(d),Config),d.Audit.TableId,ReleaseAcceptance="Native component evidence; runtime lifecycle tracked separately"}).ToArray(),Access=HuntAccess.Snapshot(),Applications,Active=ActiveRoster&&Combat.State is{Ready:true,ReportWritten:false},Units=Units.Values.ToArray(),RewardCalculations,LastOutcome,NativeRewardCalculations=Calculations.ToArray(),ObservedNativePurseCredits=HuntRewardObservation.Credits.ToArray(),GuildItems=GuildItemRuntime.Snapshot(),Session=HuntSessionRuntime.Snapshot(),ObservedLifecycle=HuntSessionRuntime.Last?.LifecycleObserved??false,IndependentLaunchImplemented=false,ProgressionCapAdded=false};
}
