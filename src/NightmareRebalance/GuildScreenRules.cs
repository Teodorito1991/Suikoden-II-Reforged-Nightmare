namespace NightmareRebalance;
// Shared logical layout: runtime and offline previews use these exact rectangles.
internal readonly record struct GuildBox(float X,float Y,float W,float H);
internal sealed record GuildContractCard(string Id,string Name,string Destination,string Danger,string Monsters,string Tactics,string Evidence);
internal sealed record GuildDrawText(GuildBox Box,string Text,int Size,string Tone="body");
internal sealed class GuildScreenModel
{
 internal string[] Names=Array.Empty<string>(),Availability=Array.Empty<string>();
 internal int Focus;internal string Selected="",State="No contract",Active="None",Objective="Choose a contract to begin.",Action="Close",Status="",Bonus="",Item="Original loot",Prompt="Confirm: accept contract";
 internal bool Castle=true,Preview;
}
internal static class GuildScreenRules
{
 internal const float Width=1100,Height=680;
 internal static readonly GuildBox Catalogue=new(0,80,350,376),Detail=new(366,80,734,376),Active=new(0,470,1100,124),Footer=new(0,608,1100,72);
 internal static readonly GuildContractCard[] Cards={
  new("sun-kings-retinue","Sun King's Retinue","Cave of the Wind","Elite contract","Sun King / Sorcerer","A sturdy guardian fights beside a dangerous caster. Prepare both physical and magical options.","Experimental quest: final gameplay acceptance pending."),
  new("dreamers-circuit","Dreamer's Circuit","Sindar Ruins","Elite / status pressure","Mr. Venus / Nightmare / MegaWatt","Sleep and mixed attacks punish an unprepared party. Bring recovery options.","Experimental: battle and travel await gameplay confirmation."),
  new("thornwake-patrol","Thornwake Patrol","Forest area (route unconfirmed)","Elite / disruption","Melonzoo / two Holly Elves","Prepare status recovery and varied attacks. Field identity is audited; travel label remains unverified.","Experimental contract: destination and gameplay await confirmation.")};
 internal static GuildContractCard Card(string id)=>Cards.FirstOrDefault(x=>x.Id==id)??Cards[0];
 internal static int Move(int focus,bool up,bool down,int contracts){if(contracts<1)return 0;if(up==down)return Math.Clamp(focus,0,contracts);return (Math.Clamp(focus,0,contracts)+(down?1:contracts))%(contracts+1);}
 internal static float Scale(int w,int h)=>Math.Max(.1f,Math.Min((w-32)/Width,(h-32)/Height));
 internal static IEnumerable<string> Wrap(string text,int columns)
 {
  columns=Math.Max(8,columns);foreach(var paragraph in text.Replace("\r","").Split('\n')){
   string line="";foreach(string word in paragraph.Split(' ',StringSplitOptions.RemoveEmptyEntries)){
    if(line.Length+word.Length+1>columns&&line.Length>0){yield return line;line="";}
    // User-controlled status/reward text cannot overflow a line with one giant token.
    string rest=word;while(rest.Length>columns){if(line.Length>0){yield return line;line="";}yield return rest[..(columns-1)]+"-";rest=rest[(columns-1)..];}
    line+=(line.Length>0?" ":"")+rest;
   }yield return line;
  }
 }
 internal static string Fit(string text,int columns,int lines){var rows=Wrap(text,columns).ToArray();if(rows.Length<=lines)return string.Join("\n",rows);return string.Join("\n",rows.Take(lines-1).Append(rows[lines-1].TrimEnd('.')+"..."));}
 internal static List<GuildDrawText> Text(GuildScreenModel m)
 {
  var a=new List<GuildDrawText>();void T(float x,float y,float w,float h,string t,int s=22,string tone="body")=>a.Add(new(new(x,y,w,h),t,s,tone));
  T(24,4,760,40,"Hunt Guild",34,"title");T(26,48,850,25,"Dunan Castle   /   Optional contracts",20,"muted");
  if(m.Preview)T(860,14,232,30,"OFFLINE / TEST PREVIEW",16,"gold");
  T(24,98,300,30,"Contracts",25,"gold");
  for(int i=0;i<Math.Min(3,m.Names.Length);i++){
   T(48,145+i*76,278,31,Fit(m.Names[i],26,1),22);
   T(48,177+i*76,278,26,Fit(m.Availability.ElementAtOrDefault(i)??"Unavailable",32,1),17,"muted");
  }
  T(48,399,274,40,Fit(m.Action,28,2),20,m.Focus==3?"gold":"body");
  var card=Card(m.Selected);T(392,98,680,36,card.Name,28,"title");
  T(392,141,680,28,card.Destination,23,"gold");T(392,177,680,25,card.Danger+"  /  Normal difficulty",19,"muted");
  T(392,217,680,29,"Targets: "+card.Monsters,21);
  T(392,255,680,72,Fit(card.Tactics,65,3),21);
  T(392,332,680,30,"Potch: battle reward + "+m.Bonus+" bonus",22,"gold");
  T(392,370,680,50,Fit("Possible item: "+m.Item,65,2),20);
  T(392,426,680,23,Fit(card.Evidence,79,1),16,"muted");
  T(24,486,245,27,"Active contract",23,"gold");T(275,486,799,27,Fit(m.State+"  /  "+m.Active,76,1),22);
  T(24,526,1052,52,Fit(m.Objective,102,2),21);
  T(24,619,1052,25,m.Prompt+"     Cancel: close",20,"gold");
  T(24,649,1052,24,"Session only: loading a save or restarting clears the contract.",18,"muted");
  return a;
 }
}
