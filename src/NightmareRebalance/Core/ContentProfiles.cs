using System.Text.Json;
using System.Text.Json.Serialization;

namespace NightmareRebalance;

internal sealed class CharacterProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public byte[] Growth { get; set; } = Array.Empty<byte>();
    public byte[] Affinity { get; set; } = Array.Empty<byte>();
    public byte[] Slots { get; set; } = Array.Empty<byte>();
}
internal sealed class CharacterBaseline
{
    public int Id { get; set; }
    public byte[] Growth { get; set; } = Array.Empty<byte>();
    public byte[] Affinity { get; set; } = Array.Empty<byte>();
    public byte[] Slots { get; set; } = Array.Empty<byte>();
    public byte[] Restrictions { get; set; } = Array.Empty<byte>();
    public byte Fixed { get; set; }
    public byte Equipment { get; set; }
    public byte Panic { get; set; }
}
internal sealed class SpellProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public short Effect { get; set; }
}
internal sealed class SpellBaseline
{
    public int Id { get; set; }
    [JsonPropertyName("name")] public int Name { get; set; }
    [JsonPropertyName("text")] public int Text { get; set; }
    [JsonPropertyName("info")] public byte[] Info { get; set; } = Array.Empty<byte>();
    [JsonPropertyName("atrb")] public byte Attribute { get; set; }
    [JsonPropertyName("status")] public byte[] Status { get; set; } = Array.Empty<byte>();
    [JsonPropertyName("eff_val")] public short Effect { get; set; }
    [JsonPropertyName("uni_no")] public byte Unite { get; set; }
    [JsonPropertyName("uni_magic")] public byte UniteMagic { get; set; }
}
internal sealed class ContentBaseline
{
    public CharacterBaseline[] Characters { get; set; } = Array.Empty<CharacterBaseline>();
    public SpellBaseline[] Spells { get; set; } = Array.Empty<SpellBaseline>();
}
internal sealed class CharacterFile
{
    public int SchemaVersion { get; set; }
    public CharacterProfile[] Characters { get; set; } = Array.Empty<CharacterProfile>();
}
internal sealed class MagicFile
{
    public int SchemaVersion { get; set; }
    public double WindOfSleepSusceptibilityMultiplier { get; set; } = 1;
    public SpellProfile[] Spells { get; set; } = Array.Empty<SpellProfile>();
}
internal static class ContentRules
{
    // System.Text.Json treats byte[] as base64 by default. These editable files use numeric arrays.
    internal sealed class Bytes : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected numeric byte array.");
            var values = new List<byte>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            { if (values.Count >= 16) throw new JsonException("Array too long."); values.Add(reader.GetByte()); }
            return values.ToArray();
        }
        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        { writer.WriteStartArray(); foreach (byte b in value) writer.WriteNumberValue(b); writer.WriteEndArray(); }
    }
    internal static readonly JsonSerializerOptions JsonOptions = new() { Converters = { new Bytes() } };
    internal static T Read<T>(string json) where T : class =>
        json.Length <= 262144 ? JsonSerializer.Deserialize<T>(json, JsonOptions) ?? throw new JsonException("Null file.") : throw new JsonException("File exceeds 256 KiB.");
    static void Keys(JsonElement element, params string[] keys)
    {
        if (element.ValueKind != JsonValueKind.Object) throw new JsonException("Expected object.");
        var found = new HashSet<string>();
        foreach (var p in element.EnumerateObject())
            if (!keys.Contains(p.Name) || !found.Add(p.Name)) throw new JsonException($"Unknown or duplicate property {p.Name}.");
        if (!found.SetEquals(keys)) throw new JsonException("Missing required properties.");
    }
    internal static CharacterFile Characters(string json, ContentBaseline baseline)
    {
        if (json.Length > 262144) throw new JsonException("File exceeds 256 KiB.");
        using var doc = JsonDocument.Parse(json); Keys(doc.RootElement, "SchemaVersion", "Characters");
        foreach (var row in doc.RootElement.GetProperty("Characters").EnumerateArray()) Keys(row, "Id", "Name", "Role", "Growth", "Affinity", "Slots");
        var file = Read<CharacterFile>(json);
        if (file.SchemaVersion != 1 || file.Characters == null || file.Characters.Length > 80) throw new JsonException("Invalid character schema/count.");
        var ids = new HashSet<int>();
        foreach (var c in file.Characters)
        {
            if (c == null) throw new JsonException("Null character record.");
            var b = baseline.Characters.SingleOrDefault(b => b.Id == c.Id);
            if (b == null || !ids.Add(c.Id) || c.Name == null || c.Name.Length > 80 || c.Role == null || c.Role.Length > 120 ||
                c.Growth?.Length != 4 || c.Affinity?.Length != 4 || c.Slots?.Length != 3) throw new JsonException("Invalid character record.");
            for (int k = 0; k < 8; k++)
            {
                int old = Nibble(b.Growth, k), next = Nibble(c.Growth, k);
                if (old > 8 ? next != old : next > 8) throw new JsonException("Special growth curves must remain unchanged; ordinary tiers are 0..8.");
                old = Nibble(b.Affinity, k); next = Nibble(c.Affinity, k);
                if (old is 0 or > 4 ? next != old : next is < 1 or > 4) throw new JsonException("Only verified affinity codes 1..4 may be edited.");
            }
            for (int k = 0; k < 3; k++)
                if (c.Slots[k] is < 1 or > 100 || b.Slots[k] == 100 && c.Slots[k] != 100) throw new JsonException("Absent rune slots must remain absent; thresholds are 1..100.");
        }
        return file;
    }
    internal static MagicFile Magic(string json, ContentBaseline baseline)
    {
        if (json.Length > 262144) throw new JsonException("File exceeds 256 KiB.");
        using var doc = JsonDocument.Parse(json); Keys(doc.RootElement, "SchemaVersion", "WindOfSleepSusceptibilityMultiplier", "Spells");
        foreach (var row in doc.RootElement.GetProperty("Spells").EnumerateArray()) Keys(row, "Id", "Name", "Effect");
        var file = Read<MagicFile>(json);
        if (file.SchemaVersion != 1 || file.Spells == null || file.Spells.Length > 31 || !double.IsFinite(file.WindOfSleepSusceptibilityMultiplier) ||
            file.WindOfSleepSusceptibilityMultiplier is < 1 or > 2) throw new JsonException("Invalid magic schema/count/sleep multiplier (1..2).");
        var ids = new HashSet<int>();
        foreach (var c in file.Spells)
            if (c == null || !baseline.Spells.Any(b => b.Id == c.Id) || !ids.Add(c.Id) || c.Effect is < 1 or > 32000 || c.Name == null || c.Name.Length > 100) throw new JsonException("Invalid or unverified spell.");
        return file;
    }
    internal static int Nibble(byte[] bytes, int index) => index % 2 == 0 ? bytes[index / 2] >> 4 : bytes[index / 2] & 15;
    internal static bool Same(byte[]? a, byte[]? b) => a != null && b != null && a.SequenceEqual(b);
    internal static bool Accept(byte[] current, byte[] vanilla, byte[] target) => Same(current, vanilla) || Same(current, target);
    internal static bool CharacterCompatible(CharacterBaseline actual, CharacterBaseline vanilla, CharacterProfile target) =>
        Accept(actual.Growth, vanilla.Growth, target.Growth) && Accept(actual.Affinity, vanilla.Affinity, target.Affinity) &&
        Accept(actual.Slots, vanilla.Slots, target.Slots) && Same(actual.Restrictions, vanilla.Restrictions) &&
        actual.Fixed == vanilla.Fixed && actual.Equipment == vanilla.Equipment && actual.Panic == vanilla.Panic;
    internal static bool SpellCompatible(SpellBaseline actual, SpellBaseline vanilla, SpellProfile target) =>
        actual.Name == vanilla.Name && actual.Text == vanilla.Text && actual.Attribute == vanilla.Attribute &&
        actual.Unite == vanilla.Unite && actual.UniteMagic == vanilla.UniteMagic && Same(actual.Info, vanilla.Info) &&
        Same(actual.Status, vanilla.Status) && (actual.Effect == vanilla.Effect || actual.Effect == target.Effect);
    internal static int SleepSusceptibility(int vanilla, double multiplier) => vanilla <= 0 ? vanilla : Math.Max(vanilla, (int)Math.Min(200, Math.Round(vanilla * multiplier, MidpointRounding.AwayFromZero)));
}
internal static class SleepScope
{
    [ThreadStatic] static bool active;
    internal static bool Enter(bool eligible) { bool previous = active; active = eligible; return previous; }
    internal static void Exit(bool previous) => active = previous;
    internal static int Apply(int taisei, int result, double multiplier) => active && taisei == 0 ? ContentRules.SleepSusceptibility(result, multiplier) : result;
}
