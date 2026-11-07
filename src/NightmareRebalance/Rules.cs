namespace NightmareRebalance;

public enum RecoveryPriority { LowestDepletedLevel, HighestDepletedLevel }

/// <summary>Pure rules; no Unity objects, native offsets or global game state.</summary>
public static class Rules
{
    public static int Scale(int value, double multiplier, int maximum)
    {
        if (value <= 0 || multiplier == 1) return value;
        if (!double.IsFinite(multiplier) || multiplier < 0) return value;
        return (int)Math.Clamp(Math.Round(value * multiplier, MidpointRounding.AwayFromZero), 0, maximum);
    }

    public static int Recover(Span<byte> charges, int amount, RecoveryPriority priority)
    {
        if (charges.Length != 4 || amount <= 0 || !Enum.IsDefined(typeof(RecoveryPriority), priority)) return 0;
        // Reject malformed input as a unit; never "repair" a character's normal maximum.
        foreach (byte b in charges) if ((b >> 4) > (b & 15)) return 0;
        int restored = 0;
        for (int n = 0; n < 4 && amount > 0; n++)
        {
            int i = priority == RecoveryPriority.LowestDepletedLevel ? n : 3 - n;
            int max = charges[i] & 15, current = charges[i] >> 4;
            int add = Math.Min(amount, max - current);
            charges[i] = (byte)(((current + add) << 4) | max);
            amount -= add;
            restored += add;
        }
        return restored;
    }
}

/// <summary>One normal battle generation; result callbacks may be repeated.</summary>
public sealed class VictoryGate
{
    private bool won, consumed;
    public bool TryMarkVictory() { if (consumed) return false; won = true; return true; }
    public void MarkVictory() => TryMarkVictory();
    public void MarkNonVictory() { won = false; consumed = true; }
    public bool TryConsume()
    {
        if (!won || consumed) return false;
        consumed = true;
        return true;
    }
}
