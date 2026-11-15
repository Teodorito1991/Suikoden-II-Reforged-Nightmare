using NightmareRebalance;
using System.Reflection;
using System.Text.Json.Nodes;
internal static class CombatMasteryTests
{
 internal static void Run()
 {
  int n=0;void Check(bool value,string label){n++;if(!value)throw new Exception("Mastery "+n+": "+label);}
  string Resource(string name){using var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("ReforgedContent."+name)!;using var r=new StreamReader(s);return r.ReadToEnd();}
  var audit=ContentRules.Read<MasteryAudit>(Resource("mastery-audit.json"));var prior=ContentRules.Read<CombatAudit>(Resource("combat-audit.json"));var evolution=ContentRules.Read<EvolutionAudit>(Resource("evolution-audit.json"));var bosses=prior.Bosses.Concat(evolution.Bosses).ToArray();string json=Resource("combat-mastery.json");var config=CombatMasteryRules.Parse(json,audit,bosses);
  void Reject(Action<JsonObject> mutate){var o=JsonNode.Parse(json)!.AsObject();mutate(o);bool failed=false;try{CombatMasteryRules.Parse(o.ToJsonString(),audit,bosses);}catch{failed=true;}Check(failed,"strict config");}
  Reject(o=>o["SchemaVersion"]=2);Reject(o=>o["Extra"]=true);Reject(o=>o["CadencePreference"]=61);Reject(o=>o["BossCadence"]![0]!["RecoverySelections"]=0);Reject(o=>o["BossCadence"]![0]!["PressureMode"]=3);Reject(o=>o["BossCadence"]![0]!["RecoveryMode"]=1);Reject(o=>o["Runes"]![0]!["Id"]=81);Reject(o=>o["Formations"]!.AsArray().RemoveAt(0));Reject(o=>o["Unites"]![0]!["Id"]=12);Reject(o=>o["Formations"]![0]!["Hidden"]=1);
  foreach(int slots in new[]{0,1,3,4,5,8})foreach(int count in new[]{-1,0,1,2,3,4,5,int.MaxValue})Check(CombatMasteryRules.AttackLayout(slots,count)==(slots==4&&(count==0||count==4)),"verified serialized action layout");
  Check(audit.Formations.Length==20&&audit.Runes.Length==4&&audit.Unites.Length==3,"substantial separate content");Check(!audit.Formations.Select(f=>f.PartyId).Intersect(evolution.Formations.Select(f=>f.PartyId)).Any(),"old formations retained");
  foreach(var f in audit.Formations)
  {
   Check(f.OldCodes.Select(x=>x>0).SequenceEqual(f.NewCodes.Select(x=>x>0)),"occupied native slots");Check(f.NewHp<=f.OldHp,"no base HP budget increase");Check(f.Hooks.Skip(1).All(x=>x==0)&&f.Flags==0,"ordinary scripted boundary");
   int Hp(int[] codes)=>codes.Where(c=>c>0).Sum(c=>f.Monsters.Single(m=>m.Index==c).Signature[0]);Check(Hp(f.NewCodes)==f.NewHp&&Hp(f.OldCodes)==f.OldHp,"audited HP budgets");
   int Level(int[] codes)=>codes.Where(c=>c>0).Max(c=>f.Monsters.Single(m=>m.Index==c).Signature[1]);Check(Level(f.NewCodes)<=Level(f.OldCodes)+2,"no large level transplant");
   foreach(var m in f.Monsters){Check(m.Signature.Length==47&&prior.Enemies.Any(a=>a.Id==m.Id&&a.Signature.SequenceEqual(m.Signature)),"original enemy signatures and existing native AI coverage");}
   foreach(int index in f.Indices)Check(CombatEvolutionRules.FormationMatches(f,f.TableId,f.PartyId,index,f.Flags,f.Item,f.OldCodes.Count(x=>x>0),f.OldIds,f.OldCodes,f.Hooks,0,f.Positions),"source indices");
  }
  foreach(var c in config.BossCadence)for(int hp=0;hp<=100;hp++)for(int mode=0;mode<4;mode++)for(int recovery=0;recovery<=3;recovery++)
  {
   int score=CombatMasteryRules.CadenceBonus(c,mode,hp,100,recovery,40);Check(score is >=-40 and <=40,"bounded cadence preference");Check(hp!=0||score==0,"no dead unit cadence");Check(recovery<=0||hp<=0||mode!=c.PressureMode||score==-40,"pressure penalty during recovery");Check(CombatMasteryRules.NextRecovery(c,c.PressureMode,recovery)==c.RecoverySelections,"pressure arms recovery");Check(CombatMasteryRules.NextRecovery(c,c.RecoveryMode,recovery)==Math.Max(0,recovery-1),"recovery ages by selected command");
  }
  foreach(var r in audit.Runes){Check(r.Id is 80 or 83 or 85 or 97,"targeted physical options");Check(r.Effect>r.Signature[3]&&r.Effect<=r.Signature[3]*1.25,"bounded physical coefficients");}
  Check(audit.Unites.Select(x=>x.Id).SequenceEqual(new[]{24,38,39}),"additional Unite identities");
  for(int damage=0;damage<500;damage++)for(int resistance=0;resistance<4;resistance++)foreach(var r in audit.Runes)
  {int result=CombatMasteryRules.PhysicalRuneDamage(damage,resistance,r.Effect);Check(result>=0,"physical result nonnegative");Check(resistance!=3||result==0,"immunity retained");Check(damage!=0||result==0,"no free damage");}
  var rpg=JsonNode.Parse(Resource("rpg-evolution-audit.json"))!.AsObject();
  var rpgContent=ContentRules.Read<MasteryAudit>(rpg.ToJsonString());
  var reality=ContentRules.Read<MasteryAudit>(Resource("reality-audit.json"));
  var retained=evolution.Formations.Concat(audit.Formations).Concat(reality.Formations).Select(f=>f.PartyId).ToHashSet();
  foreach(var f in rpgContent.Formations)
  {
   Check(retained.Add(f.PartyId),"RPG remix uses a separate native encounter");
   Check(f.OldCodes.Select(c=>c>0).SequenceEqual(f.NewCodes.Select(c=>c>0))&&f.Hooks.Skip(1).All(h=>h==0)&&f.Flags==0,"RPG remix retains occupied slots and ordinary callbacks");
   int Hp(int[] codes)=>codes.Where(c=>c>0).Sum(c=>f.Monsters.Single(m=>m.Index==c).Signature[0]);
   Check(Hp(f.NewCodes)==f.NewHp&&Hp(f.OldCodes)==f.OldHp&&f.NewHp<=f.OldHp,"RPG remix original HP budget");
  }
  Check(!rpgContent.Unites.Select(u=>u.Id).Intersect(evolution.Unites.Concat(audit.Unites).Select(u=>u.Id)).Any(),"RPG Unite table cannot stack old coefficients");
  Check(!rpgContent.Runes.Select(r=>r.Id).Intersect(audit.Runes.Concat(reality.Runes).Select(r=>r.Id)).Any(),"RPG rune coefficients are disjoint");
  foreach(var rune in rpgContent.Runes)
   Check(rune.Effect<=rune.Signature[3]*1.20&&CombatMasteryRules.PhysicalRuneDamage(120,3,rune.Effect)==0,"RPG rune cap and immunity");
  Console.WriteLine($"PASS: Combat Mastery {n} assertions; 20 formation guards/budgets, seven bounded selection cadences, four physical coefficients, three Unite identities and independent strict config.");
 }
}
