using System.Text.Json;
using NightmareRebalance;

// Pressure projections. Party numbers are deliberately transparent equipment
// and growth assumptions. Encounter HP/moves and edited growth/rune-slot rules
// come from the audits used by the plugin.
internal static class CampaignPacingScenarios
{
    internal static void Run()
    {
        T Read<T>(string name)
        {
            using var stream=typeof(CampaignPacingScenarios).Assembly.GetManifestResourceStream("ReforgedContent."+name)!;
            return JsonSerializer.Deserialize<T>(stream)!;
        }

        var audit=Read<CombatAudit>("combat-audit.json");
        var gorudo=Read<JsonElement[]>("legendary-boss-audit.json").Select(b=>b.GetProperty("Data"))
            .Single(b=>b.GetProperty("TableId").GetString()=="boss_gor");
        var gorudoSignatures=gorudo.GetProperty("NewCodes").EnumerateArray().Select(x=>x.GetInt32()).Where(x=>x>0)
            .Select(code=>gorudo.GetProperty("Monsters").EnumerateArray().Single(m=>m.GetProperty("Index").GetInt32()==code)
                .GetProperty("Signature").EnumerateArray().Select(v=>v.GetInt32()).ToArray()).ToArray();
        var parties=Read<HuntDefinition[]>("hunt-catalogue-audit.json");
        var characters=Read<JsonElement>("characters.json").GetProperty("Characters").EnumerateArray()
            .ToDictionary(row=>row.GetProperty("Id").GetInt32());
        var phases=new[]
        {
            (Name:"Early",Level:17,Hp:170,Attack:85,Defense:52,Magic:62,Charges:2,Heal:180,
                Hunt:"dreamers-circuit",Boss:"boss_ryo",Parties:new[]{
                    new[]{1,18,19,20},new[]{1,3,23,26},new[]{1,20,24,53},new[]{1,21,28,30}}),
            (Name:"Middle",Level:32,Hp:330,Attack:150,Defense:88,Magic:99,Charges:4,Heal:240,
                Hunt:"matilda-choir",Boss:"boss_hap1",Parties:new[]{
                    new[]{1,2,3,18,8,53},new[]{1,3,16,32,39,46},new[]{1,2,8,20,24,53},new[]{1,7,17,29,42,55}}),
            (Name:"Late",Level:53,Hp:520,Attack:230,Defense:142,Magic:156,Charges:6,Heal:240,
                Hunt:"rakutei-vanguard",Boss:"boss_gor",Parties:new[]{
                    new[]{1,2,33,36,45,10},new[]{1,9,10,32,39,69},new[]{1,8,36,45,53,61},new[]{1,7,17,25,41,56}})
        };
        var rows=new List<object>();
        string[] builds=["Balanced","Physical","Magic","Imperfect"];
        foreach(var phase in phases)
        {
            var hunt=parties.Single(h=>h.Id==phase.Hunt);
            var bossSignatures=phase.Boss=="boss_gor"?gorudoSignatures:[audit.Bosses.Single(b=>b.TableId==phase.Boss).Signature];
            foreach(var (ids,index) in phase.Parties.Select((party,i)=>(party,i)))
            {
                var profiles=ids.Select(id=>characters[id]).ToArray();
                int count=ids.Length;
                int casters=profiles.Count(p=>p.GetProperty("Role").GetString() is "Elementalist" or "Warden" or "Battle mage");
                int runeSlots=profiles.Sum(p=>p.GetProperty("Slots").EnumerateArray().Count(x=>x.GetInt32()<=phase.Level));
                int armor=phase.Defense+(index==1?12:index==3?-16:0), ward=phase.Defense+(index==2?14:index==3?-16:0);
                int offense=phase.Attack+(index==1?18:index==3?-16:0);
                foreach(var (label,signatures) in new[]{
                    ("Ordinary source",hunt.Audit.OldCodes.Where(c=>c>0).Select(c=>hunt.Audit.Monsters.Single(m=>m.Index==c).Signature).ToArray()),
                    ("Optional hunt",hunt.Roster.Select(id=>hunt.Audit.Monsters.Single(m=>m.Id==id).Signature).ToArray()),
                    ("Boss "+phase.Boss,bossSignatures)})
                {
                    int openingPressure=0,sleepActors=0;
                    foreach(var signature in signatures)
                    {
                        var attacks=Enumerable.Range(0,4).Select(i=>signature.Skip(3+i*11).Take(11).ToArray()).Where(x=>x[1]>0).ToArray();
                        sleepActors+=attacks.Any(x=>x[5]==10)?1:0;
                        openingPressure+=attacks.Select(x=>
                        {
                            bool single=(x[8]&0xf00) is not (0x200 or 0x400 or 0x800);
                            return (single?1:count)*CombatReforgedRules.BaseDamage(x[3],phase.Magic/2,armor,ward,(x[7]&0x10)!=0,4);
                        }).DefaultIfEmpty().Max();
                    }
                    int potch=label=="Optional hunt"?hunt.VictoryBonusPotch:0;
                    int projectedPartyHp=count*phase.Hp;
                    rows.Add(new{phase.Name,phase.Level,Build=builds[index],Roster=profiles.Select(p=>p.GetProperty("Name").GetString()).ToArray(),
                        EquipmentDefense=armor,EquipmentMagicDefense=ward,ProjectedAttack=offense,AuditedUnlockedSlots=runeSlots,Casters=casters,
                        phase.Charges,phase.Heal,ProjectedPartyHp=projectedPartyHp,Encounter=label,NativeEnemyHp=signatures.Sum(x=>x[0]),
                        MaximumUnscaledOpeningPressure=openingPressure,SleepCapableEnemies=sleepActors,BonusPotch=potch,
                        RuneRecoveryPerSurvivor=1,CampaignAccessUnverified=true});
                }
            }
        }

        var nightmare=audit.Enemies.Single(e=>e.Id=="vc12_00_01").Signature.AsSpan(14,11);
        if(nightmare[5]!=10||(nightmare[8]&0xf00)!=0x800)throw new InvalidOperationException("Expected original Nightmare area sleep");
        var move=new CombatAction(1,nightmare[0],nightmare[3],false,true,nightmare[5],nightmare[1],1,true);
        var ready=new CombatTarget(0,150,150,50,50,65,80,0);
        CombatTarget[] awake=[ready,ready with{Slot=1}], halfAsleep=[ready,ready with{Slot=1,StatusUseful=false}];
        if(CombatReforgedRules.OrdinarySleepPenalty(move,awake)!=0||
            CombatReforgedRules.OrdinarySleepPenalty(move,halfAsleep)!=36||
            CombatReforgedRules.OrdinarySleepPenalty(move with{Multi=false},halfAsleep)!=0)
            throw new InvalidOperationException("Ordinary sleep pressure boundaries");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"campaign-pacing.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"PASS: {rows.Count} campaign pressure projections and the audited Nightmare sleep boundary. No simulated victory or campaign acceptance.");
    }
}
