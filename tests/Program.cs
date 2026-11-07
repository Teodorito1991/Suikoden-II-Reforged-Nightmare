using NightmareRebalance;

int assertions = 0;
void Check(bool ok, string message) { assertions++; if (!ok) throw new Exception(message); }
byte Pack(int current, int max) => (byte)((current << 4) | max);
byte[] example = { Pack(3,7), Pack(2,5), Pack(1,3), Pack(0,1) };
Check(Rules.Recover(example, 1, RecoveryPriority.LowestDepletedLevel) == 1, "Example budget");
Check(example.SequenceEqual(new byte[] {Pack(4,7),Pack(2,5),Pack(1,3),Pack(0,1)}), "Example output");
for (int n=0;n<3;n++) Rules.Recover(example, 1, RecoveryPriority.LowestDepletedLevel);
Rules.Recover(example, 1, RecoveryPriority.LowestDepletedLevel);
Check(example[0] == Pack(7,7) && example[1] == Pack(3,5), "Advance only after L1 full");
byte[] highest = {Pack(0,7),Pack(0,5),Pack(0,3),Pack(0,1)};
Rules.Recover(highest, 3, RecoveryPriority.HighestDepletedLevel);
Check(highest.SequenceEqual(new byte[] {Pack(0,7),Pack(0,5),Pack(2,3),Pack(1,1)}), "Highest priority and total budget");
byte[] malformed = {0x87,0x05,0x03,0x01};
var before = malformed.ToArray();
Check(Rules.Recover(malformed, 1, RecoveryPriority.LowestDepletedLevel) == 0 && malformed.SequenceEqual(before), "Invalid packed current rejected atomically");
Check(Rules.Recover(new byte[3], 1, RecoveryPriority.LowestDepletedLevel) == 0, "Unknown level count rejected");
var rng = new Random(1932640);
for (int n=0;n<10000;n++)
{
    byte[] mp = Enumerable.Range(0,4).Select(_ => { int max=rng.Next(16);return Pack(rng.Next(max+1),max); }).ToArray();
    var old = mp.ToArray();
    int amount = rng.Next(61);
    var priority = n%2 == 0 ? RecoveryPriority.LowestDepletedLevel : RecoveryPriority.HighestDepletedLevel;
    int actual=Rules.Recover(mp, amount, priority);
    int missing=old.Sum(b => (b&15)-(b>>4));
    Check(actual == Math.Min(amount, missing), "Exact total budget");
    int delta=0;
    for (int i=0;i<4;i++)
    {
        Check((mp[i]&15)==(old[i]&15), "Normal max preserved");
        Check((mp[i]>>4)<=(mp[i]&15) && (mp[i]>>4)>=(old[i]>>4), "Charge bounds");
        delta+=(mp[i]>>4)-(old[i]>>4);
    }
    Check(delta==actual, "Actual mutation matches reported budget");
    var order=priority==RecoveryPriority.LowestDepletedLevel?new[]{0,1,2,3}:new[]{3,2,1,0};
    bool remains=false;
    foreach (int i in order)
    {
        if (remains) Check(mp[i]==old[i], "No lower priority recovery before higher priority is full");
        if ((mp[i]>>4)<(mp[i]&15)) remains=true;
    }
}
var gate = new VictoryGate();
Check(!gate.TryConsume(), "Unknown outcome rejected");
gate.MarkVictory(); gate.MarkVictory();
Check(gate.TryConsume() && !gate.TryConsume(), "Duplicate completion rejected");
gate.MarkVictory(); Check(!gate.TryConsume(), "Late duplicate win rejected");
var escape=new VictoryGate(); escape.MarkNonVictory();escape.MarkVictory();
Check(!escape.TryConsume(), "Escape cannot become victory");
var interrupted=new VictoryGate();interrupted.MarkVictory();interrupted.MarkNonVictory();
Check(!interrupted.TryConsume(), "Non-victory cancels armed victory");
Check(Rules.Scale(100,1.60,32767)==160, "HP baseline");
Check(Rules.Scale(100,1.45,9999)==145, "Damage baseline");
Check(Rules.Scale(100,0.85,int.MaxValue)==85, "Potch baseline");
Check(Rules.Scale(123,1,int.MaxValue)==123, "XP unchanged");
Check(Rules.Scale(30000,20,32767)==32767, "Signed HP capacity");
Check(Rules.Scale(0,1.45,9999)==0 && Rules.Scale(-100,1.45,9999)==-100, "Non-damage preserved");
Check(Rules.Scale(100,double.NaN,9999)==100, "Invalid multiplier rejected");
Check(Rules.Scale(10,0.85,int.MaxValue)==9, "Documented midpoint rounding");
Console.WriteLine($"PASS: {assertions} assertions; 10,000 seeded charge cases plus victory lifecycle and scaling checks.");
StageATests.Run();
StageBTests.Run();

ReforgedTests.Run();
ContentRegressionTests.Run();
WarCommanderTests.Run();
ArenaTests.Run();
NativeWarTests.Run();

WarRevolutionTests.Run();
TotalWarTests.Run();
WarTacticsTests.Run();

CombatReforgedTests.Run();

CombatFairnessTests.Run();
CombatEvolutionTests.Run();

CombatMasteryTests.Run();

CombatMasteryScenarios.Run();

CombatRealityTests.Run();

MaintenanceTests.Run();

ExpansionTests.Run();
HuntAccessTests.Run();
HuntFieldTests.Run();

HuntSessionTests.Run();

HuntCatalogueTests.Run();
HuntGuildTests.Run();

BossTacticsTests.Run();

NativeWarEvidenceTests.Run();

for(int stage=0;stage<4;stage++){
 var integrationGate=new VictoryGate();
 if(stage==0)integrationGate.MarkNonVictory();
 if(stage==1){integrationGate.MarkVictory();integrationGate.TryConsume();}
 if(stage==2){integrationGate.MarkVictory();integrationGate.MarkNonVictory();}
 bool accepted=integrationGate.TryMarkVictory();
 if(accepted!=(stage==3)||integrationGate.TryConsume()!=(stage==3)||integrationGate.TryMarkVictory())throw new Exception("Terminal victory integrationGate regression");
}
Console.WriteLine("PASS: Integration outcome gate 12 assertions; non-victory, consumed, revoked and genuine victory.");
Check(!new HuntGuildFile().EnableVictoryItems,"Omitted experimental item setting stays off");
Check(!new BossTacticsFile().EnableMajorBosses,"Omitted experimental major boss setting stays off");
Check(!new HuntOverride().Enabled,"Omitted contract enabled setting stays off");
Console.WriteLine("PASS: Integration experimental defaults 3 assertions; missing values do not silently opt in.");

GuildQuestTests.Run();

GuildScreenTests.Run();

PublicationDefaultsTests.Run();
