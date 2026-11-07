using System.Text.Json;
using System.Text.Json.Nodes;
using NightmareRebalance;

internal static class ReforgedTests
{
    internal static void Run()
    {
        int checks = 0;
        void Check(bool value, string message) { checks++; if (!value) throw new Exception("Reforged: " + message); }
        void Reject(Action action, string message)
        { bool failed = false; try { action(); } catch (Exception ex) when (ex is JsonException or InvalidOperationException or OverflowException or FormatException) { failed = true; } Check(failed, message); }
        string Resource(string name)
        {
            using var stream = typeof(ReforgedTests).Assembly.GetManifestResourceStream("ReforgedContent." + name)!;
            using var reader = new StreamReader(stream); return reader.ReadToEnd();
        }
        string cjson = Resource("characters.json"), mjson = Resource("magic.json");
        var baseline = ContentRules.Read<ContentBaseline>(Resource("baseline.json"));
        var chars = ContentRules.Characters(cjson, baseline); var magic = ContentRules.Magic(mjson, baseline);
        Check(chars.Characters.Length == 80 && baseline.Characters.Length == 80, "Full identified roster coverage");
        Check(magic.Spells.Length == 31 && baseline.Spells.Length == 31, "31 verified effect edits");
        Check(!chars.Characters.Any(c => new[] { 0, 79, 81, 83, 84 }.Contains(c.Id)), "Unresolved/empty records excluded");
        foreach (var c in chars.Characters)
        {
            var b = baseline.Characters.Single(b => b.Id == c.Id);
            Check(ContentRules.CharacterCompatible(b, b, c), "Vanilla character accepted");
            var applied = new CharacterBaseline { Growth = c.Growth, Affinity = c.Affinity, Slots = c.Slots, Restrictions = b.Restrictions, Fixed = b.Fixed, Equipment = b.Equipment, Panic = b.Panic };
            Check(ContentRules.CharacterCompatible(applied, b, c), "Repeated absolute application idempotent");
            Check(!ContentRules.Same(c.Growth,b.Growth) || !ContentRules.Same(c.Affinity,b.Affinity) || !ContentRules.Same(c.Slots,b.Slots), "Every shipped profile changes gameplay data");
            applied.Fixed ^= 1; Check(!ContentRules.CharacterCompatible(applied,b,c), "Foreign fixed-rune mutation rejected"); applied.Fixed ^= 1;
            applied.Restrictions = new byte[] { 255, 255, 255, 255 }; Check(!ContentRules.CharacterCompatible(applied,b,c), "Foreign restriction mutation rejected");
            for (int k = 0; k < 8; k++)
            {
                int before = ContentRules.Nibble(b.Growth,k), after = ContentRules.Nibble(c.Growth,k);
                Check(before > 8 ? after == before : after <= 8, "Special growth curves preserved");
                before = ContentRules.Nibble(b.Affinity,k); after = ContentRules.Nibble(c.Affinity,k);
                Check(before is 0 or >4 ? after == before : after is >=1 and <=4, "Only verified affinities edited");
            }
            for (int k = 0; k < 3; k++) Check(b.Slots[k] != 100 || c.Slots[k] == 100, "Absent slots preserved");
        }
        foreach (var c in magic.Spells)
        {
            var b = baseline.Spells.Single(b => b.Id == c.Id);
            Check(ContentRules.SpellCompatible(b,b,c), "Vanilla spell accepted");
            var applied = ContentRules.Read<SpellBaseline>(JsonSerializer.Serialize(b,ContentRules.JsonOptions));
            applied.Effect = c.Effect; Check(ContentRules.SpellCompatible(applied,b,c), "Spell application idempotent");
            applied.Effect++; Check(!ContentRules.SpellCompatible(applied,b,c), "Foreign effect mutation rejected");
            applied.Effect = c.Effect; applied.Attribute ^= 1; Check(!ContentRules.SpellCompatible(applied,b,c), "Foreign element mutation rejected");
        }
        Check(magic.Spells.Single(c=>c.Id==6).Effect == 100 && magic.Spells.Single(c=>c.Id==12).Effect == 210, "Finite low-level healing design");
        Check(magic.Spells.Single(c=>c.Id==22).Effect == 220 && magic.Spells.Single(c=>c.Id==30).Effect == 110, "Weak attacks improved");
        Check(magic.Spells.Single(c=>c.Id==29).Effect == 1350, "Judgment restrained");
        Check(!magic.Spells.Any(c=>new[]{10,26,28,38,39,40,41,46}.Contains(c.Id)), "Revival, instant death and Black Sword preserved");
        // Real parser rejection paths, including duplicate JSON keys and accidental renamed settings.
        Reject(()=>ContentRules.Characters(cjson.Replace("\"SchemaVersion\": 1", "\"SchemaVersion\": 2"),baseline),"Unknown schema");
        Reject(()=>ContentRules.Characters(cjson.Replace("\"SchemaVersion\": 1", "\"SchemaVersion\": 1, \"SchemaVersion\": 1"),baseline),"Duplicate property");
        Reject(()=>ContentRules.Characters(cjson.Replace("\"Role\"", "\"Rloe\""),baseline),"Unknown/missing property");
        Reject(()=>ContentRules.Magic(mjson.Replace("1.5", "3.0"),baseline),"Sleep multiplier capped");
        Reject(()=>ContentRules.Characters(new string(' ',262145),baseline),"Oversized config");
        for (int id = 0; id < 85; id++)
        {
            var node = JsonNode.Parse(cjson)!; node["Characters"]![0]!["Id"] = id;
            if (id != 1) Reject(()=>ContentRules.Characters(node.ToJsonString(),baseline),"Duplicate/unverified/mismatched character ID rejected");
        }
        for (int id = 0; id < 106; id++)
        {
            var node = JsonNode.Parse(mjson)!; node["Spells"]![0]!["Id"] = id;
            if (id != 1) Reject(()=>ContentRules.Magic(node.ToJsonString(),baseline),"Duplicate/unverified spell ID rejected");
        }
        var invalid = JsonNode.Parse(cjson)!; invalid["Characters"]![0]!["Growth"] = new JsonArray(255,255,255,255);
        Reject(()=>ContentRules.Characters(invalid.ToJsonString(),baseline),"Illegal growth codes");
        invalid = JsonNode.Parse(cjson)!; invalid["Characters"]![0]!["Slots"] = new JsonArray(0,1,1);
        Reject(()=>ContentRules.Characters(invalid.ToJsonString(),baseline),"Illegal slot threshold");
        var scoped = SleepScope.Enter(false);
        Check(SleepScope.Apply(0,100,1.5)==100,"No sleep boost outside eligible invocation");
        var outer = SleepScope.Enter(true);
        for (int value=-5;value<=200;value++)
        {
            Check(SleepScope.Apply(0,value,1.5)==(value<=0?value:Math.Min(200,(int)Math.Round(value*1.5,MidpointRounding.AwayFromZero))),"Sleep range, immunity and cap");
            Check(SleepScope.Apply(1,value,1.5)==value,"Unrelated status untouched");
        }
        var nested = SleepScope.Enter(false); Check(SleepScope.Apply(0,100,1.5)==100,"Nested unrelated invocation isolated");
        SleepScope.Exit(nested); Check(SleepScope.Apply(0,100,1.5)==150,"Outer scope restored");
        SleepScope.Exit(outer); Check(SleepScope.Apply(0,100,1.5)==100,"Exception/finalizer exit clears scope"); SleepScope.Exit(scoped);
        Check(ContentRules.SleepSusceptibility(250,1.5)==250,"Existing exceptional susceptibility never reduced by cap");
        Check(ContentRules.Characters(JsonSerializer.Serialize(chars,ContentRules.JsonOptions),baseline).Characters.Length==80,"Numeric arrays roundtrip");
        Console.WriteLine($"PASS: Reforged {checks} assertions; content validation, compatibility, idempotence and scoped status logic. No in-game acceptance claim.");
    }
}
