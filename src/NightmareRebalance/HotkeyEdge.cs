namespace NightmareRebalance;

internal sealed class HotkeyEdge
{
    bool wasDown;
    internal bool Poll(bool focused,bool down,bool pressedSinceLastPoll)
    {bool fire=focused&&((down&&!wasDown)||pressedSinceLastPoll);wasDown=down;return fire;}
}
