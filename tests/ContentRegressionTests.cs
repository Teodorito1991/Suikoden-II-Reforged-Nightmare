using System.Text.Json;
using System.Text.Json.Nodes;
using NightmareRebalance;

internal static class ContentRegressionTests
{
    internal static void Run()
    {
        int count=0;
        void Check(bool ok,string why) { count++; if(!ok) throw new Exception("Content regression: "+why); }
        string Load(string name) {
            using var stream=typeof(ContentRegressionTests).Assembly.GetManifestResourceStream("ReforgedContent."+name)!;
            using var reader=new StreamReader(stream); return reader.ReadToEnd();
        }
        var baseline=ContentRules.Read<ContentBaseline>(Load("baseline.json"));
        var chars=ContentRules.Characters(Load("characters.json"),baseline);
        var magic=ContentRules.Magic(Load("magic.json"),baseline);
        T Clone<T>(T value) where T:class => ContentRules.Read<T>(JsonSerializer.Serialize(value,ContentRules.JsonOptions));
        foreach(var target in chars.Characters) {
            var vanilla=baseline.Characters.Single(b=>b.Id==target.Id);
            for(int field=0;field<7;field++) {
                var changed=Clone(vanilla);
                switch(field) {
                    case 0: changed.Fixed^=1; break;
                    case 1: changed.Equipment^=1; break;
                    case 2: changed.Panic^=1; break;
                    default: changed.Restrictions[field-3]^=1; break;
                }
                Check(!ContentRules.CharacterCompatible(changed,vanilla,target),"Every preserved character field rejects foreign changes");
            }
            for(int field=0;field<3;field++) {
                var changed=Clone(vanilla);
                if(field==0)changed.Growth[0]=255;
                if(field==1)changed.Affinity[0]=255;
                if(field==2)changed.Slots[0]=255;
                Check(!ContentRules.CharacterCompatible(changed,vanilla,target),"Unexpected writable array mutation rejected");
            }
        }
        foreach(var target in magic.Spells) {
            var vanilla=baseline.Spells.Single(b=>b.Id==target.Id);
            for(int field=0;field<10;field++) {
                var changed=Clone(vanilla);
                switch(field) {
                    case 0: changed.Name++; break;
                    case 1: changed.Text++; break;
                    case 2: changed.Attribute^=1; break;
                    case 3: changed.Unite^=1; break;
                    case 4: changed.UniteMagic^=1; break;
                    case 5: changed.Info[0]^=1; break;
                    case 6: changed.Info[1]^=1; break;
                    default: changed.Status[field-7]^=1; break;
                }
                Check(!ContentRules.SpellCompatible(changed,vanilla,target),"Every preserved spell field rejects foreign changes");
            }
        }
        // An exception and another worker thread must not leave sleep boosting enabled.
        bool previous=SleepScope.Enter(true);
        try {
            Check(Task.Run(()=>SleepScope.Apply(0,100,1.5)).GetAwaiter().GetResult()==100,"Sleep scope does not leak to worker thread");
            Check(SleepScope.Apply(0,100,1.5)==150,"Worker cannot clear main-thread scope");
        } finally { SleepScope.Exit(previous); }
        try {
            previous=SleepScope.Enter(true);
            try { throw new InvalidOperationException("Synthetic original-method failure"); }
            finally { SleepScope.Exit(previous); }
        } catch(InvalidOperationException) { }
        Check(SleepScope.Apply(0,100,1.5)==100,"Exception cleanup leaves subsequent invocations unmodified");
        // Do not treat an absent/new module as a parser failure: subsets are independently configurable.
        Check(ContentRules.Characters("{\"SchemaVersion\":1,\"Characters\":[]}",baseline).Characters.Length==0,"Empty character subset allowed");
        Check(ContentRules.Magic("{\"SchemaVersion\":1,\"WindOfSleepSusceptibilityMultiplier\":1,\"Spells\":[]}",baseline).Spells.Length==0,"Empty magic subset and disabled boost allowed");
        Console.WriteLine($"PASS: Content regression {count} assertions; full preserved-field fingerprints, thread/exception isolation and independent subsets.");
    }
}
