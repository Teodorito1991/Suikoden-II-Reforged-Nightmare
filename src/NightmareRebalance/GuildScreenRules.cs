namespace NightmareRebalance;
// Shared logical layout: runtime and offline previews use these exact rectangles.
internal readonly record struct GuildBox(float X,float Y,float W,float H);
internal sealed record GuildContractCard(string Id,string Name,string Destination,string Danger,string Monsters,string Tactics,string Evidence);
internal sealed record GuildDrawText(GuildBox Box,string Text,int Size,string Tone="body");
internal sealed class GuildScreenModel
{
 internal string[] Names=Array.Empty<string>(),Availability=Array.Empty<string>();
 internal int Focus=0;internal string Selected="",State="No contract",Active="None",Objective="Choose a contract to begin.",Action="Close",Status="",Bonus="",Item="Original loot",Prompt="Confirm: accept contract";
 internal string TrainingName="";
 internal bool Castle=true,Preview=false;
}
internal static class GuildScreenRules
{
 internal const float Width=1100,Height=680;
 internal static readonly GuildBox Catalogue=new(0,80,350,376),Detail=new(366,80,734,376),Active=new(0,470,1100,124),Footer=new(0,608,1100,72);
 internal static readonly GuildContractCard[] Cards={
  new("sun-kings-retinue","Sun King's Retinue","Cave of the Wind","Elite contract","Sun King / Sorcerer","A sturdy guardian fights beside a dangerous caster. Prepare both physical and magical options.","Optional hunt. Report completion to take the contract again."),
  new("dreamers-circuit","Dreamer's Circuit","Sindar Ruins","Elite / status pressure","Mr. Venus / Nightmare / MegaWatt","Nightmare can inflict sleep. Bring a way to wake allies.","Experimental contract. Disabled by default."),
  new("thornwake-patrol","Thornwake Patrol","Greenhill City area","Elite / disruption","Melonzoo / two Holly Elves","The Holly Elves can inflict Balloon. Plan to deal with the escorts.","Experimental contract. Disabled by default.")};
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
  if(m.TrainingName.Length>0)
  {
   T(392,100,680,38,Fit(m.TrainingName,45,1),28,"title");
   T(392,148,680,28,m.State+" difficulty",22,"gold");
   T(392,186,680,27,m.State=="Veteran"?"Enemy squads coordinate and use terrain.":"Original enemy tactics.",18,"muted");
   T(392,224,680,76,Fit(m.Objective,64,3),20);
   T(392,325,680,24,"Training reward",20,"gold");
   T(392,355,680,28,m.Bonus.Length==0?"Rewards disabled":m.Bonus+" Potch for routing the enemy commander",22);
   T(392,390,680,27,"Defeat or withdrawal pays no Potch.",20);
   T(24,486,1052,27,"Castle Army Training",23,"gold");
   T(24,526,1052,52,"Replay a completed campaign battle with its original army and battlefield.",20);
   T(24,610,1052,24,Fit(m.Status,104,1),18);
   T(24,637,1052,23,Fit(m.Prompt+"     Cancel: close",108,1),18,"gold");
   return a;
  }
  var card=Card(m.Selected);  T(392,100,680,38,Fit(card.Name,45,1),28,"title");
  T(392,148,680,28,Fit("Destination: "+card.Destination,55,1),22,"gold");
  T(392,186,680,24,card.Danger+"  /  Normal difficulty",18,"muted");
  T(392,224,680,28,Fit("Targets: "+card.Monsters,61,1),21);
  T(392,261,680,54,Fit(card.Tactics,64,2),20);
  T(392,325,680,24,"Victory spoils",20,"gold");
  T(392,355,680,28,"Battle Potch + "+m.Bonus+" contract bonus",22);
  T(392,390,680,27,Fit("Possible item: "+m.Item,65,1),20);
  T(392,427,680,23,"Other loot takes priority; bag space and the purse limit apply.",15,"muted");
  T(24,486,245,27,"Your contract",23,"gold");T(275,486,799,27,Fit(m.State+"  /  "+m.Active,70,1),21);
  T(24,526,1052,52,Fit(m.Objective,100,2),20);
  T(24,610,1052,24,Fit(m.Status.Length>0?m.Status:card.Evidence,104,1),18,"body");
  T(24,637,1052,23,Fit(m.Prompt+"     Cancel: close",108,1),18,"gold");
  T(24,664,1052,16,"Loading a save or restarting ends your contract.",15,"muted");
  return a;
 }
}
