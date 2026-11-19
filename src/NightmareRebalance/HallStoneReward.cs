namespace NightmareRebalance;
// One pending native loot offer per owned trial, never a direct inventory grant.
internal sealed class HallStoneReward
{
    internal bool Attempted,Prepared,PayoutStarted;
    internal void Prepare(GAME_WORK game,EXIT_WORK exit,bool eligible)
    {
        if(!eligible||PayoutStarted||exit.item_work!=null||exit.monster_item?.item_no!=0)return;
        var stone=HuntGuildRuntime.Items.Single(x=>x.Id=="power-stone");
        if(Attempted)
        {
            if(Prepared){exit.monster_item.item_no=(byte)stone.Number;exit.monster_item.use_cnt=(byte)stone.UseCount;}
            return;
        }
        Attempted=true;
        if(!Plugin.CheckCompatibility()||!GuildItemRuntime.MatchesText(GuildItemRuntime.ItemSignature(
            G2_SYS.G2_data_get(game.ovit_data,stone.Kind,stone.Number),stone),stone))return;
        int roll=UnityEngine.Random.Range(0,100);
        Prepared=roll<HallNightmareRules.StoneChancePercent;
        if(Prepared){exit.monster_item.item_no=(byte)stone.Number;exit.monster_item.use_cnt=(byte)stone.UseCount;}
        Plugin.Logger.LogInfo($"HallNightmare: stone roll={roll}; prepared={Prepared}; native inventory/UI owns receipt.");
    }
}
