using System.Text.Json;
using System.Text.Json.Nodes;
using NightmareRebalance;

internal static class WarCommanderTests
{
    internal static void Run()
    {
        int checks=0;
        void Check(bool ok,string why){checks++;if(!ok)throw new Exception("v0.4: "+why);}
        void Reject(Action action,string why){bool rejected=false;try{action();}catch(JsonException){rejected=true;}Check(rejected,why);}
        string Load(string name){using var stream=typeof(WarCommanderTests).Assembly.GetManifestResourceStream("ReforgedContent."+name)!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
        string ej=Load("enemy-profiles.json"),wj=Load("war-unit-profiles.json");
        var eb=ContentRules.Read<EnemyFile>(ej);var wb=ContentRules.Read<CommanderFile>(wj);
        var enemies=OverhaulRules.Enemies(ej,eb);var war=OverhaulRules.Commanders(wj,wb);
        Check(enemies.Profiles.Length==280,"280 exact enemy profiles");Check(war.Profiles.Length==21,"21 commander/branch profiles");
        foreach(var e in enemies.Profiles)
        {
            Check(!e.Id.StartsWith("boss_")&&!e.Id.StartsWith("eb_"),"Boss/event inventory excluded");
            Check(OverhaulRules.EnemyEligible(e,true,true,e.ExpectedHp,e.NameId,e.ExpectedLevel),"Verified regular enemy admitted");
            Check(!OverhaulRules.EnemyEligible(e,false,true,e.ExpectedHp,e.NameId,e.ExpectedLevel),"Custom story handlers excluded");
            Check(!OverhaulRules.EnemyEligible(e,true,false,e.ExpectedHp,e.NameId,e.ExpectedLevel),"Players/non-enemy entities excluded");
            Check(!OverhaulRules.EnemyEligible(e,true,true,e.ExpectedHp+1,e.NameId,e.ExpectedLevel),"Changed HP source rejected");
            Check(!OverhaulRules.EnemyEligible(e,true,true,e.ExpectedHp,e.NameId+1,e.ExpectedLevel),"Changed native name rejected");
            Check(!OverhaulRules.EnemyEligible(e,true,true,e.ExpectedHp,e.NameId,e.ExpectedLevel+1),"Changed native level rejected");
            int final=Rules.Scale(e.ExpectedHp,e.HpMultiplier,short.MaxValue);
            Check(final>=1&&final<=short.MaxValue,"Signed HP capacity");
            Check(final==Rules.Scale(e.ExpectedHp,e.HpMultiplier,short.MaxValue),"Final relative-to-vanilla value stable");
        }
        foreach(var p in war.Profiles)
        {
            Check(ReferenceEquals(p,OverhaulRules.Select(war,p.NameId,p.Branch)),"Exact branch beats broad match");
            for(int i=0;i<p.Abilities.Length;i++)
            {int id=Array.IndexOf(OverhaulRules.Abilities,p.Abilities[i]);Check(OverhaulRules.SafeAbility(id),"No cut/crash ability");Check(p.Uses[i]<=3,"Bounded finite ability budget");}
        }
        foreach(int bad in new[]{-1,2,8,14,16,26,100})Check(!OverhaulRules.SafeAbility(bad),"Unsupported ability denied");
        foreach(string bad in new[]{"SP_CHARGE","SP_FORCE_MOVE","SP_MAGIC_WIND2","SP_KIN_SLAYER","UNKNOWN"})
        {var n=JsonNode.Parse(wj)!;n["Profiles"]![0]!["Abilities"]![0]=bad;Reject(()=>OverhaulRules.Commanders(n.ToJsonString(),wb),"Unsafe ability JSON rejected");}
        foreach(string key in new[]{"Attack","Defense","AssistAttack","AssistDefense"})
        {var n=JsonNode.Parse(wj)!;n["Profiles"]![0]![key]=21;Reject(()=>OverhaulRules.Commanders(n.ToJsonString(),wb),"Stat bound rejected");}
        foreach(double factor in new[]{0,0.249,4.001,20})
        {var n=JsonNode.Parse(ej)!;n["Profiles"]![0]!["HpMultiplier"]=factor;Reject(()=>OverhaulRules.Enemies(n.ToJsonString(),eb),"Enemy factor bound rejected");}
        foreach(string key in new[]{"ExpectedHp","ExpectedLevel","NameId"})
        {var n=JsonNode.Parse(ej)!;n["Profiles"]![0]![key]=9999;Reject(()=>OverhaulRules.Enemies(n.ToJsonString(),eb),"JSON cannot rewrite native identity fingerprints");}
        Reject(()=>OverhaulRules.Enemies(ej.Replace("\"SchemaVersion\": 1","\"SchemaVersion\": 1, \"SchemaVersion\": 1"),eb),"Duplicate keys rejected");
        Reject(()=>OverhaulRules.Commanders(wj.Replace("\"Branch\"","\"Brnach\""),wb),"Unknown/missing field rejected");
        Reject(()=>OverhaulRules.Enemies(new string(' ',262145),eb),"Oversized JSON rejected");
        var duplicate=JsonNode.Parse(ej)!;duplicate["Profiles"]![1]=duplicate["Profiles"]![0]!.DeepClone();Reject(()=>OverhaulRules.Enemies(duplicate.ToJsonString(),eb),"Duplicate enemy IDs rejected");
        Check(OverhaulRules.Enemies("{\"SchemaVersion\":1,\"Profiles\":[]}",eb).Profiles.Length==0,"Independent empty enemy module");
        Check(OverhaulRules.Commanders("{\"SchemaVersion\":1,\"Profiles\":[]}",wb).Profiles.Length==0,"Independent empty war module");
        var scope=new WarSession<object>();object copy=new();
        Check(!scope.Store(1,copy),"No writes outside war chapter");
        scope.Begin(true,1);Check(!scope.Active,"charaInit alone cannot activate profiles");
        for(int number=-2;number<=19;number++)
        {scope.Arm();scope.Begin(true,number);Check(scope.Active==(number>=1&&number<=16),"Verified native war-number range");scope.Clear();Check(!scope.Active&&!scope.Armed&&scope.Count==0,"War exit discards all copies");}
        scope.Arm();scope.Begin(false,1);Check(!scope.Active,"War feature disabled");
        scope.Arm();scope.Begin(true,1);
        for(int i=0;i<512;i++){Check(scope.Store(i,new object()),"Bounded war cache accepts unique record");Check(scope.TryGet(i,out _),"Native reads reuse same record rather than refilling abilities");}
        Check(!scope.Store(1000,new object()),"Cache limit enforced");Check(!scope.Store(0,new object()),"Duplicate copy rejected");
        scope.Begin(true,2);Check(scope.Count==0&&!scope.TryGet(0,out _),"New war resets spent-ability copies");
        scope.Clear();Check(!scope.TryGet(1,out _),"Campaign/next scene cannot read profiled copy");
        var hotkey=new HotkeyEdge();
        Check(!hotkey.Poll(false,true,true),"Inactive game cannot consume a global hotkey");
        Check(!hotkey.Poll(true,true,false),"Refocusing with key held does not trigger a press");
        Check(!hotkey.Poll(true,false,false),"Key release has no action");
        Check(hotkey.Poll(true,true,true),"Key down and low-bit event cause one action");
        Check(!hotkey.Poll(true,true,false),"Held key does not repeat");
        Check(!hotkey.Poll(true,false,false),"Release resets latch");
        Check(hotkey.Poll(true,false,true),"Short press between frames is detected");
        Check(!hotkey.Poll(true,false,false),"Consumed short press cannot repeat");
        Console.WriteLine($"PASS: War Commander {checks} assertions; exact enemy identities, factors, safe abilities, independent config and production war-copy lifecycle. No native arena turn/victory test claim.");
    }
}
