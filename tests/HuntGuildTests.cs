using NightmareRebalance;
using System.Text.Json;
internal static class HuntGuildTests
{
 internal static void Run()
 {
  int n=0;void Check(bool x,string label){n++;if(!x)throw new Exception("Guild: "+label);}
  string Resource(string name){using var s=typeof(HuntGuildTests).Assembly.GetManifestResourceStream("ReforgedContent."+name)!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var hunts=ContentRules.Read<HuntDefinition[]>(Resource("hunt-catalogue-audit.json"));var items=ContentRules.Read<GuildItemAudit[]>(Resource("guild-item-audit.json"));var c=HuntGuildRules.Parse(Resource("hunt-guild.json"),hunts,items);
  Check(c.EnableSelector&&!c.EnableVictoryItems&&c.Rewards.Length==10,"Independent modules/default rewards");
  var legacy=new HuntGuildFile{EnableSelector=false,EnableVictoryItems=true,Rewards=c.Rewards.Take(3).ToArray()};
  legacy.Rewards[0].Enabled=false;
  var upgraded=HuntGuildRules.Parse(JsonSerializer.Serialize(legacy),hunts,items,c);
  Check(upgraded.Rewards.Length==10&&!upgraded.Rewards[0].Enabled&&!upgraded.EnableSelector&&upgraded.EnableVictoryItems,"Legacy reward and global flags preserved while adding seven entries");
  foreach(var a in items){Check((a.UseCount>>4)==a.Kind&&a.Number>0&&a.Number<256,"Native packet kind/index bounds");Check(a.Kind is 0 or 2&&a.Template.GetProperty("name").GetInt32()>0,"Only audited consumables/orbs");}
  Check(items.Single(x=>x.Id=="flowing-orb").UseCount==32,"Non-stackable orb has original kind2/count0 encoding");Check(items.Single(x=>x.Id=="power-stone").UseCount==1,"Single consumable encoding");
  for(int mask=0;mask<256;mask++){bool B(int i)=>(mask&(1<<i))!=0;bool expected=B(0)&&B(1)&&B(2)&&!B(3)&&B(4)&&B(5)&&!B(6)&&!B(7);Check(HuntGuildRules.CanSelect(B(0),B(1),B(2),B(3),B(4),B(5),B(6),B(7))==expected,"Native MAIN/open/Normal/no battle context matrix");}
  for(int mask=0;mask<128;mask++){bool B(int i)=>(mask&(1<<i))!=0;bool expected=B(0)&&B(1)&&B(2)&&B(3)&&!B(4)&&!B(5)&&B(6);Check(HuntGuildRules.CanPrepare(B(0),B(1),B(2),B(3),B(4),B(5),B(6))==expected,"Victory/snapshot/local scope/empty loot/no late payout matrix");}
  void Invalid(Action<HuntGuildFile> edit){var bad=HuntGuildRules.Parse(Resource("hunt-guild.json"),hunts,items);edit(bad);bool rejected=false;try{HuntGuildRules.Parse(JsonSerializer.Serialize(bad),hunts,items);}catch(JsonException){rejected=true;}Check(rejected,"Invalid config rejected");}
  Invalid(x=>x.SchemaVersion=2);Invalid(x=>x.Rewards=x.Rewards.Take(2).ToArray());Invalid(x=>x.Rewards[0].RewardId="unverified-item");Invalid(x=>x.Rewards[1].HuntId=x.Rewards[0].HuntId);
  Check(HuntGuildRules.Preference("{\"SchemaVersion\":1,\"HuntId\":\"dreamers-circuit\"}",hunts.Select(x=>x.Id))=="dreamers-circuit","Known preference");Check(HuntGuildRules.Preference("{\"SchemaVersion\":1,\"HuntId\":\"bad\"}",hunts.Select(x=>x.Id))==null,"Unknown preference ignored");Check(HuntGuildRules.Preference(new string('x',4097),Array.Empty<string>())==null,"Oversized preference ignored");
  foreach(var item in items){Check(HuntGuildRules.MatchesTemplate(JsonSerializer.Serialize(item.Template),item.Template),"Canonical numeric arrays match full template");Check(!HuntGuildRules.MatchesTemplate(JsonSerializer.Serialize(item.Template).Replace("\"info\":"+item.Template.GetProperty("info").GetRawText().Replace(" ","").Replace("\r","").Replace("\n",""),"\"info\":\"ERQ=\""),item.Template),"Byte-array base64 serialization rejected");}
  var run=new HuntRunProgress();run.ItemPrepared("power-stone");run.ItemHandoff();run.ItemTransfer(true);Check(run.PreparedItemId==null&&!run.NativeItemHandoff&&run.NativeItemTransferCalls==0,"No non-victory award observations");run.Victory();run.ItemPrepared("power-stone");Check(run.InventoryIncreaseObservations==0,"Preparation is not an award");run.ItemHandoff();run.ItemTransfer(false);Check(run.InventoryIncreaseObservations==0&&run.NativeItemTransferCalls==1,"Failed/native no-change attempt distinct");run.ItemTransfer(true);Check(run.InventoryIncreaseObservations==1,"Actual observed increase distinct");run.Finish("VictoryRewardsCompleted",1);run.ItemTransfer(true);Check(run.InventoryIncreaseObservations==1,"Late callbacks excluded");
  Console.WriteLine($"PASS: Hunt Guild {n} assertions; selector context, independent rewards, packet identity, preference and observational award lifecycle. No rendered guild/item award claim.");
 }
}
