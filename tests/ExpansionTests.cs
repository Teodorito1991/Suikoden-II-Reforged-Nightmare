using NightmareRebalance;
using System.Text.Json;
internal static class ExpansionTests
{
 internal static void Run(){int n=0;void Check(bool x){n++;if(!x)throw new Exception("Expansion "+n);}
  string Resource(string name){using var s=typeof(ExpansionTests).Assembly.GetManifestResourceStream("ReforgedContent."+name)!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var config=ExpansionRules.Parse(Resource("first-expansion.json"));Check(!config.EnableExperimentalHunt);Check(config.VictoryBonusPotch==2500);var a=ContentRules.Read<FormationAudit>(Resource("hunt-audit.json"));Check(a.PartyId=="vd06_00_03"&&a.Indices.SequenceEqual(new[]{3}));Check(a.OldCodes.Count(c=>c>0)==a.NewCodes.Count(c=>c>0)&&a.NewCodes.SequenceEqual(new[]{3,0,4,0,0,0}));Check(a.Hooks.Skip(1).All(h=>h==0)&&a.Flags==0&&a.Item==0);
  var old=ContentRules.Read<EvolutionAudit>(Resource("evolution-audit.json")).Formations.Concat(ContentRules.Read<MasteryAudit>(Resource("mastery-audit.json")).Formations).Concat(ContentRules.Read<RealityAudit>(Resource("reality-audit.json")).Formations);Check(old.Count()==51&&!old.Any(x=>x.PartyId==a.PartyId));
  for(int amount=0;amount<10000;amount+=7){var gate=new FlatRewardGate();Check(gate.Apply(amount,2500)==amount+2500);Check(gate.Apply(amount+2500,2500)==amount+2500);Check(gate.Apply(amount,2500)==amount+2500);}
  var saturate=new FlatRewardGate();Check(saturate.Apply(int.MaxValue-1,2500)==int.MaxValue);
  var hotkey=new StableHotkeyEdge();Check(hotkey.Poll(true,true,true,0));Check(!hotkey.Poll(true,true,true,20));Check(!hotkey.Poll(true,false,true,40));Check(!hotkey.Poll(true,false,false,100));Check(hotkey.Poll(true,true,true,200));Check(!hotkey.Poll(false,true,true,250));Check(!hotkey.Poll(true,true,false,300));Check(!hotkey.Poll(true,false,false,320));Check(hotkey.Poll(true,false,true,500));
  bool bad=false;try{ExpansionRules.Parse(Resource("first-expansion.json").Replace("2500","-1"));}catch(JsonException){bad=true;}Check(bad);
  Console.WriteLine($"PASS: Expansion {n} assertions; disabled prototype, exact optional party, preserved51 formations, repeatable reward preparation and stable hotkey edges. No playable hunt claim.");
 }
}
