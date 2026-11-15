using NightmareRebalance;
using System.Reflection;
using System.Text.Json;

internal static class PublicationDefaultsTests
{
    internal static void Run()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName,"config","ReforgedNightmare"))) root = root.Parent;
        if (root == null) throw new Exception("Publication config root not found");
        string Read(string name) => File.ReadAllText(Path.Combine(root.FullName,"config","ReforgedNightmare",name));
        string Resource(string name) {
            using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("ReforgedContent."+name)??throw new Exception(name);
            using var reader=new StreamReader(stream);return reader.ReadToEnd();
        }
        int checks=0;
        void Check(bool ok,string label){checks++;if(!ok)throw new Exception("Public defaults: "+label);}
        var definitions=ContentRules.Read<HuntDefinition[]>(Resource("hunt-catalogue-audit.json"));
        var items=ContentRules.Read<GuildItemAudit[]>(Resource("guild-item-audit.json"));
        var catalogue=HuntCatalogueRules.Parse(Read("hunt-catalogue.json"),definitions);
        Check(catalogue.Hunts.Single(x=>x.Id=="sun-kings-retinue").Enabled,"Sun enabled");
        Check(catalogue.Hunts.Length==10&&catalogue.Hunts.All(x=>x.Enabled),"All native-tested contracts enabled in recommended profile");
        var guild=HuntGuildRules.Parse(Read("hunt-guild.json"),definitions,items);
        Check(guild.EnableSelector&&guild.EnableVictoryItems,"Native Guild and item fallback enabled");
        Check(ExpansionRules.Parse(Read("first-expansion.json")).EnableExperimentalHunt,"Hunt access enabled");
        using var castle=JsonDocument.Parse(Read("castle-guild.json"));
        Check(castle.RootElement.GetProperty("Enabled").GetBoolean()&&castle.RootElement.GetProperty("AutoRequestAcceptedContract").GetBoolean()&&castle.RootElement.GetProperty("EnableFieldReissue").GetBoolean(),"Castle automatic route enabled");
        using var boss=JsonDocument.Parse(Read("boss-tactics.json"));
        Check(!boss.RootElement.GetProperty("EnableMajorBosses").GetBoolean(),"Major boss opt-in off");
        var cfg=File.ReadAllText(Path.Combine(root.FullName,"config.example.cfg"));
        foreach(var key in new[]{"EnableHuntDebugTools","EnableDeveloperTools","RunNativeWarSelfTest","RunCombatSelfTest"})
            Check(cfg.Contains(key+" = false"),"No player debug/probe "+key);
        Console.WriteLine($"PASS: Publication defaults {checks} parser/safety checks; no gameplay acceptance inferred.");
    }
}
