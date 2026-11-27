namespace NightmareRebalance;
internal static class HuntRewardAvailabilityProbe
{
 internal static int Assertions,Recalculations;internal static readonly List<object> Rows=new();
 static void Check(bool x,string name){Assertions++;if(!x)throw new InvalidOperationException("Reward availability: "+name);}
 internal static void Run(BattleManager manager,BATTLE_WORK w)
 {
  var h=HuntRuntime.Active!;var rng=UnityEngine.Random.state;var exit=w.exit_work;bool enabled=HuntGuildRuntime.Config.EnableVictoryItems;
  try{
   var roster=w.dead_monster_data.Take(w.dead_monster).Select(x=>w.monster_data[x.chara_id]).ToArray();double lower=1;var weights=roster.Select(m=>m.item_prob.Select(x=>(int)x).ToArray()).ToArray();
   foreach(var a in weights){Check(a.Length==3&&a.Sum()<=254,"Audited native three-slot drop cumulative range");lower*=Math.Max(0,254-a.Sum())/254.0;}
   Check(w.monster_party_data.item_no==0&&lower>0,"Native guaranteed-party-item route absent; no-drop opportunity exists");
   var rule=HuntGuildRuntime.Config.Rewards.Single(x=>x.HuntId==h.Id);var item=HuntGuildRuntime.Items.Single(x=>x.Id==rule.RewardId);HuntGuildRuntime.Config.EnableVictoryItems=true;
   int done=0;for(int seed=0;seed<1024&&done<16;seed++){
    UnityEngine.Random.InitState(seed);if(!roster.All(m=>OldSrcBase.get_rand(254)>=m.item_prob.Sum(x=>(int)x)))continue;
    // Only verified no-drop seeds call full original reward calculation. No campaign inventory context is fabricated.
    GuildItemRuntime.Reset();w.exit_work=new EXIT_WORK{monster_item=new PARTY_ITEM()};UnityEngine.Random.InitState(seed);manager.get_monster_okane(w);
    Check(w.exit_work.monster_item.item_no==item.Number&&w.exit_work.monster_item.use_cnt==item.UseCount,"Repeat no-drop native calculation prepares same reward");done++;Recalculations++;
   }
   Check(done==16,"Sixteen independent repeat reward preparations");Rows.Add(new{h.Id,OriginalLootWeights=weights,NoDropProbabilityLowerBound=lower,ExpectedWinsUpperBoundForOpportunity=1/lower,NativeRepeatedPreparations=done,ActualReceipt=false,Interpretation="Source-roll lower bound; native item-specific rejection can increase fallback opportunity. No extra mod random roll or farming cap."});
  }finally{w.exit_work=exit;HuntGuildRuntime.Config.EnableVictoryItems=enabled;GuildItemRuntime.Reset();UnityEngine.Random.state=rng;}
 }
 internal static object Snapshot()=>new{Assertions,Recalculations,Rows=Rows.ToArray(),ActualPlayerReceipt=false};
}

