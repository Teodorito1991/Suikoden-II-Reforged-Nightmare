using System.Text.Json;
namespace NightmareRebalance;
internal sealed class ExpansionFile
{
 public int SchemaVersion{get;set;}=1;public bool EnableExperimentalHunt{get;set;}=false;
 public double SunKingHpMultiplier{get;set;}=2;
 public double SorcererHpMultiplier{get;set;}=1.4;
 public double HuntDamageMultiplier{get;set;}=1.15;
 public int VictoryBonusPotch{get;set;}=2500;
}
internal static class ExpansionRules
{
 internal static ExpansionFile Parse(string text){if(text.Length>4096)throw new JsonException("Expansion config too large");using var d=JsonDocument.Parse(text);OverhaulRules.Keys(d.RootElement,"SchemaVersion","EnableExperimentalHunt","SunKingHpMultiplier","SorcererHpMultiplier","HuntDamageMultiplier","VictoryBonusPotch");var c=ContentRules.Read<ExpansionFile>(text);if(c.SchemaVersion!=1||new[]{c.SunKingHpMultiplier,c.SorcererHpMultiplier,c.HuntDamageMultiplier}.Any(x=>!double.IsFinite(x)||x<1||x>5)||c.VictoryBonusPotch is <0 or >1000000)throw new JsonException("Unsupported expansion values");return c;}
}
internal sealed class FlatRewardGate
{
 int? previousOutput;
 internal int Apply(int nativeTotal,int bonus){if(nativeTotal<0||bonus<0)return nativeTotal;if(previousOutput==nativeTotal)return nativeTotal;int result=(int)Math.Min(int.MaxValue,(long)nativeTotal+bonus);previousOutput=result;return result;}
}
internal sealed class StableHotkeyEdge
{
 bool wasDown;long previous=-1000;
 internal bool Poll(bool focused,bool down,bool pressed,long milliseconds){bool edge=!wasDown&&(down||pressed);wasDown=down;if(!focused||!edge||milliseconds-previous<160)return false;previous=milliseconds;return true;}
}
internal sealed class HuntRequest
{
 internal bool Armed{get;private set;}
 internal bool Toggle(bool available){Armed=available&&!Armed;return Armed;}
 internal bool Consume(bool eligible){bool use=Armed&&eligible;Armed=false;return use;}
 internal void Cancel()=>Armed=false;
}
internal sealed class NativeFieldWindow
{
 int last=-1;
 internal void Record(int frame){if(frame>=0)last=frame;}
 internal bool IsCurrent(int frame)=>last>=0&&frame>=last&&(long)frame-last<=2;
 internal void Clear()=>last=-1;
}
