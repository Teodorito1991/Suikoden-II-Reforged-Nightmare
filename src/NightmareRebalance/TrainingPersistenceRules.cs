using System.Text.Json;
namespace NightmareRebalance;
internal static class TrainingPersistenceRules
{
    internal static bool IsCampaignPath(string? path)=>path!=null&&System.Text.RegularExpressions.Regex.IsMatch(
        path.Replace('\\','/'),@"(?:^|/)gsd2/Data[0-9]+$",System.Text.RegularExpressions.RegexOptions.CultureInvariant|System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    // These are existing native serialized records. No mod progression or save
    // extension is added. Compare values without relying on JSON property order.
    internal static bool PayloadMatches(string json, IReadOnlyList<byte[]> flags,
        IReadOnlyList<string> records, out string reason)
    {
        reason="invalid native payload";
        if(flags.Count!=5||records.Count!=4)return false;
        try
        {
            using var document=JsonDocument.Parse(json);
            var root=document.RootElement;
            if(root.ValueKind!=JsonValueKind.Object || !UniqueProperties(root))return false;
            string[] flagNames=["chara_flag","town_flag","event_flag","scpoi_flag","t_box_flag"];
            for(int i=0;i<5;i++)
            {
                reason="payload flag ownership: "+flagNames[i];
                if(!root.TryGetProperty(flagNames[i],out var a)||a.ValueKind!=JsonValueKind.Array||a.GetArrayLength()!=flags[i].Length)return false;
                int j=0;
                foreach(var value in a.EnumerateArray())
                    if(!value.TryGetByte(out byte b)||b!=flags[i][j++])return false;
            }
            string[] names=["game_data","war_data","chara_data","party_data"];
            for(int i=0;i<4;i++)
            {
                reason="payload differs from restored live record: "+names[i];
                using var live=JsonDocument.Parse(records[i]);
                if(!root.TryGetProperty(names[i],out var value)||!Equal(live.RootElement,value))return false;
            }
            reason="";
            return true;
        }
        catch(JsonException){return false;}
        catch(InvalidOperationException){return false;}
    }
    static bool UniqueProperties(JsonElement element)
    {
        var names=new HashSet<string>(StringComparer.Ordinal);
        return element.EnumerateObject().All(p=>names.Add(p.Name));
    }
    static bool Equal(JsonElement a,JsonElement b)
    {
        if(a.ValueKind!=b.ValueKind)return false;
        if(a.ValueKind==JsonValueKind.Object)
            return UniqueProperties(a)&&UniqueProperties(b)&&a.EnumerateObject().Count()==b.EnumerateObject().Count()&&
                a.EnumerateObject().All(p=>b.TryGetProperty(p.Name,out var value)&&Equal(p.Value,value));
        if(a.ValueKind==JsonValueKind.Array)
            return a.GetArrayLength()==b.GetArrayLength()&&a.EnumerateArray().Zip(b.EnumerateArray(),Equal).All(equal=>equal);
        return a.ValueKind==JsonValueKind.String ? a.GetString()==b.GetString() : a.GetRawText()==b.GetRawText();
    }
}
