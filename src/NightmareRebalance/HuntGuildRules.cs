using System.Text.Json;
namespace NightmareRebalance;
internal sealed class GuildRewardSetting{public string HuntId{get;set;}="";public bool Enabled{get;set;}=true;public string RewardId{get;set;}="";}
internal sealed class HuntGuildFile
{
 public int SchemaVersion{get;set;}=1;public bool EnableSelector{get;set;}=true;public bool EnableVictoryItems{get;set;}=false;public GuildRewardSetting[] Rewards{get;set;}=Array.Empty<GuildRewardSetting>();
}
internal sealed class GuildItemAudit
{
 public string Id{get;set;}="";public string Name{get;set;}="";public int Kind{get;set;}public int Number{get;set;}public int UseCount{get;set;}public JsonElement Template{get;set;}
}
internal static class HuntGuildRules
{
 internal static HuntGuildFile Parse(string json,HuntDefinition[] hunts,GuildItemAudit[] items)
 {
  if(json.Length>16384)throw new JsonException("Guild config too large");using var doc=JsonDocument.Parse(json);OverhaulRules.Keys(doc.RootElement,"SchemaVersion","EnableSelector","EnableVictoryItems","Rewards");
  foreach(var row in doc.RootElement.GetProperty("Rewards").EnumerateArray())OverhaulRules.Keys(row,"HuntId","Enabled","RewardId");
  var c=ContentRules.Read<HuntGuildFile>(json);if(c.SchemaVersion!=1||c.Rewards==null||c.Rewards.Any(x=>x==null)||!c.Rewards.Select(x=>x.HuntId).OrderBy(x=>x).SequenceEqual(hunts.Select(x=>x.Id).OrderBy(x=>x))||c.Rewards.Any(x=>!items.Any(i=>i.Id==x.RewardId)))throw new JsonException("Unknown or duplicate guild identity");return c;
 }
 internal static bool MatchesTemplate(string? text,JsonElement template){if(text==null)return false;using var d=JsonDocument.Parse(text);return template.EnumerateObject().All(p=>d.RootElement.TryGetProperty(p.Name,out var v)&&v.GetRawText()==JsonSerializer.Serialize(p.Value));}
 internal static bool CanSelect(bool enabled,bool menuProven,bool open,bool animating,bool main,bool normal,bool combat,bool war)=>enabled&&menuProven&&open&&!animating&&main&&normal&&!combat&&!war;
 internal static bool CanPrepare(bool enabled,bool bound,bool victory,bool snapshot,bool reportsClosed,bool payoutStarted,bool slotEmpty)=>enabled&&bound&&victory&&snapshot&&!reportsClosed&&!payoutStarted&&slotEmpty;
 internal static string? Preference(string json,IEnumerable<string> ids){if(json.Length>4096)return null;using var d=JsonDocument.Parse(json);OverhaulRules.Keys(d.RootElement,"SchemaVersion","HuntId");return d.RootElement.GetProperty("SchemaVersion").GetInt32()==1&&d.RootElement.GetProperty("HuntId").GetString() is {} id&&ids.Contains(id)?id:null;}
}
