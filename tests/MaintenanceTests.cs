using NightmareRebalance;
internal static class MaintenanceTests
{
 internal static void Run()
 {
  int n=0;void Check(bool x){n++;if(!x)throw new Exception("Maintenance assertion "+n);}
  foreach(int hp in Enumerable.Range(1,32767))foreach(int difficulty in new[]{0,2})
  {var a=new int[47];a[0]=hp;a[1]=30;var b=(int[])a.Clone();float rate=difficulty==0?1f:1.2f;b[0]=MaintenanceRules.NativeHp(hp,rate);Check(MaintenanceRules.Signature(a,b,difficulty,rate));b[0]++;Check(!MaintenanceRules.Signature(a,b,difficulty,rate));b[0]--;b[46]++;Check(!MaintenanceRules.Signature(a,b,difficulty,rate));}
  Check(MaintenanceRules.NativeHp(280,1.2f)==336);Check(MaintenanceRules.NativeHp(1500,1.2f)==1800);Check(MaintenanceRules.NativeHp(32767,1.2f)==32767);
  {var ea=new int[47];ea[0]=500;ea[1]=30;var eb=(int[])ea.Clone();eb[0]=MaintenanceRules.NativeHp(500,0.8f);Check(MaintenanceRules.Signature(ea,eb,1,0.8f));Check(!MaintenanceRules.Signature(ea,eb,1,1.2f));}
  Check(MaintenanceRules.Factor(1.6,1.2,1.7,1.6)==1.7);Check(MaintenanceRules.Factor(1.6,1.2,1.6,1.6)==1.2);Check(MaintenanceRules.Factor(1.6,null,1.6,1.6)==1.6);
  var hpLedger=new HpLedger();hpLedger.Baseline(1,100);Check(hpLedger.Observe(1,75)==-25);Check(hpLedger.Observe(1,75)==0);Check(hpLedger.Observe(1,90)==15);Check(hpLedger.Observe(1,-4)==-90);Check(hpLedger.Observe(1,0)==0);Check(hpLedger.Observe(2,50)==0);
  var history=new BoundedHistory<string>(12);for(int i=0;i<30;i++)history.Add(i%3==2?"Escape"+i:"Victory"+i);Check(history.Items.Count==12);Check(history.Items[0]=="Victory18");Check(history.Items.Count(x=>x.StartsWith("Victory"))==8);Check(history.Items.Last()=="Escape29");
  string directory=Path.Combine(Path.GetTempPath(),"ReforgedNightmare-history-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);string path=Path.Combine(directory,"history.json");
  for(int i=0;i<30;i++)DiagnosticHistoryStore.Append(path,System.Text.Json.JsonSerializer.Serialize(new{Generation=i,Outcome=i%3==2?"Escape":"Victory",ChargesRestored=i%3==2?0:2}));
  using(var doc=System.Text.Json.JsonDocument.Parse(File.ReadAllText(path))){var rows=doc.RootElement.GetProperty("Battles");Check(rows.GetArrayLength()==12);Check(rows[0].GetProperty("Generation").GetInt32()==18);Check(rows[11].GetProperty("Outcome").GetString()=="Escape");Check(rows.EnumerateArray().Count(x=>x.GetProperty("ChargesRestored").GetInt32()==2)==8);}
  string previous=File.ReadAllText(path);try{DiagnosticHistoryStore.Append(path,"{");Check(false);}catch(System.Text.Json.JsonException){Check(File.ReadAllText(path)==previous);}
  try{DiagnosticHistoryStore.Append(path,new string('x',262145));Check(false);}catch(InvalidDataException){Check(File.ReadAllText(path)==previous);}
  var win=new VictoryGate();win.MarkVictory();Check(win.TryConsume());Check(!win.TryConsume());var escape=new VictoryGate();escape.MarkNonVictory();Check(!escape.TryConsume());
  string Resource(string name){using var stream=typeof(MaintenanceTests).Assembly.GetManifestResourceStream("ReforgedContent."+name)!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
  foreach(var b in ContentRules.Read<CombatAudit>(Resource("combat-audit.json")).Bosses.Concat(ContentRules.Read<EvolutionAudit>(Resource("evolution-audit.json")).Bosses)){
   var id=new EncounterIdentity(b.TableId,b.PartyId,0,b.Flags,6,1,b.Hooks[5]!=0,true,b.Hooks,b.HookNames,new[]{b.EnemyId},new[]{b.Signature[0]},new[]{1,0,0,0,0,0},new[]{0,1,0,0,0,0},0,true);
   Check(CombatReforgedRules.BossMatches(b,id));Check(!CombatReforgedRules.BossMatches(b,id with{EnemyMaxHp=new[]{MaintenanceRules.NativeHp(b.Signature[0],1.2f)}}));
  }
  Console.WriteLine($"PASS: Maintenance {n} assertions; Normal/Hard exact HP, invalid signatures, override priority, delta deduplication and bounded victory/escape history.");
 }
}
