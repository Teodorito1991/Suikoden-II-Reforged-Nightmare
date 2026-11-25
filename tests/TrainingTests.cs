using NightmareRebalance;

internal static class TrainingTests
{
    internal static void Run()
    {
        static void Check(bool result, string message)
        {
            if (!result) throw new InvalidOperationException(message);
        }

        Check(TrainingResultRules.CanRedirectDefeat(3, 2, 3, -1), "War 3 defeat return");
        Check(!TrainingResultRules.CanRedirectDefeat(3, 2, 1, -1), "Unknown War 3 ending");
        Check(!TrainingResultRules.CanRedirectDefeat(3, 2, 3, 0), "Missing War 3 defeat flag");
        Check(!TrainingResultRules.CanRedirectDefeat(3, 1, 3, -1), "Ordinary return");
        Check(!TrainingResultRules.CanRedirectDefeat(2, 2, 3, -1), "Unverified War 2 interception");
        Check(TrainingResultRules.VictoryPotch(3, true, 1, 1, 0, true, true, false) == 1500, "Veteran victory");
        Check(TrainingResultRules.VictoryPotch(1, false, 1, 1, 0, false, true, false) == 0, "Timed ending");
        Check(TrainingResultRules.VictoryPotch(3, true, 2, 3, -1, true, true, false) == 0, "Redirected defeat");
        Check(TrainingResultRules.VictoryPotch(2, false, 1, 4, 0, true, true, false) == 0, "Fort breach");
        Check(TrainingResultRules.VictoryPotch(2, false, 1, 4, 0, true, false, true) == 0, "Allied rout");
        Check(TrainingResultRules.VictoryPotch(2, false, 1, 4, 0, true, true, true) == 750, "Fort victory");

        var labels = GuildScreenRules.Text(new GuildScreenModel
        {
            TrainingName = "Commander Rout", State = "Veteran", Bonus = "1,500"
        });
        Check(labels.Any(label => label.Text.Contains("1,500 Potch")), "Training reward card");
        Check(labels.All(label => !label.Text.Contains("Possible item")), "No hunt loot on training card");
        byte[][] flags=[[1,2],[3],[4],[5],[6]];
        string[] records=["{\"timer\":1}","{\"leader_no\":[1,2]}","{\"hp\":50}","{\"gold\":750}"];
        var data=new Dictionary<string,object>{["chara_flag"]=flags[0],["town_flag"]=flags[1],
            ["event_flag"]=flags[2],["scpoi_flag"]=flags[3],["t_box_flag"]=flags[4]};
        string[] names=["game_data","war_data","chara_data","party_data"];
        for(int i=0;i<4;i++)data[names[i]]=System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(records[i]);
        string Payload()=>System.Text.Json.JsonSerializer.Serialize(data);
        // byte arrays must use the native numeric-array representation, not .NET base64.
        for(int i=0;i<5;i++)data[new[]{"chara_flag","town_flag","event_flag","scpoi_flag","t_box_flag"}[i]]=flags[i].Select(b=>(int)b).ToArray();
        Check(TrainingPersistenceRules.PayloadMatches(Payload(),flags,records,out _),"Restored native payload accepted");
        var reversed=data.Reverse().ToDictionary(x=>x.Key,x=>x.Value);
        Check(TrainingPersistenceRules.PayloadMatches(System.Text.Json.JsonSerializer.Serialize(reversed),flags,records,out _),"JSON property order is immaterial");
        string good=Payload();
        Check(!TrainingPersistenceRules.PayloadMatches(good.Replace("\"event_flag\":[4]","\"event_flag\":[128]"),flags,records,out _),"Stale training flag refused");
        Check(!TrainingPersistenceRules.PayloadMatches(good.Replace("\"hp\":50","\"hp\":0"),flags,records,out _),"Changed character payload refused");
        Check(!TrainingPersistenceRules.PayloadMatches(good.Replace("\"leader_no\":[1,2]","\"leader_no\":[2,1]"),flags,records,out _),"Changed war roster refused");
        Check(!TrainingPersistenceRules.PayloadMatches(good.Replace("\"gold\":750","\"gold\":1500"),flags,records,out _),"Changed party reward payload refused");
        Check(!TrainingPersistenceRules.PayloadMatches(good[..^1]+",\"event_flag\":[4]}",flags,records,out _),"Duplicate native root properties refused");
        Check(!TrainingPersistenceRules.PayloadMatches(good.Replace("\"hp\":50","\"hp\":50,\"hp\":0"),flags,records,out _),"Duplicate nested properties refused");
        Check(!TrainingPersistenceRules.PayloadMatches("[]",flags,records,out _),"Nonobject refused");
        Check(!TrainingPersistenceRules.PayloadMatches("{",flags,records,out _),"Truncated JSON refused");
        data.Remove("party_data");
        Check(!TrainingPersistenceRules.PayloadMatches(Payload(),flags,records,out _),"Missing record refused");
        Console.WriteLine("PASS: training reward and defeat classification and Guild presentation.");
    }
}
