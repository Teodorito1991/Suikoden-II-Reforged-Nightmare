using NightmareRebalance;
using System.Text.Json;
internal static class GuildScreenTests
{
 internal static void Run()
 {
  int n=0;void Check(bool v,string label){n++;if(!v)throw new Exception("Guild screen: "+label);}
  // Test the exact layout/model shared with production, not a separately drawn mockup.
  var models=new List<GuildScreenModel>();
  foreach(var state in new[]{"No contract","Accepted","Completed — report at Guild","Reported — available again"})foreach(int focus in Enumerable.Range(0,4)){
   var m=new GuildScreenModel{Names=GuildScreenRules.Cards.Select(x=>x.Name).ToArray(),Availability=new[]{"Available — elite hunt","Available — experimental","Available — experimental"},Selected=GuildScreenRules.Cards[focus==3?0:focus].Id,State=state,Active=state=="No contract"?"None":"Sun King's Retinue",Objective=state=="Accepted"?"Travel to Cave of the Wind and defeat the target. Escape allows another attempt.":state.StartsWith("Completed")?"Return to the attendant and report. Your battle rewards have already been handled; reporting pays nothing extra.":"Accept a contract, prepare your party and travel to its destination. Hunting is optional and repeatable.",Action=state=="Accepted"?"Withdraw accepted contract":state.StartsWith("Completed")?"Report completed hunt (no extra reward)":"Close",Bonus="2,500",Item="Stone of Power (when no other loot is found; free space required)",Focus=focus,Preview=true};
   m.Bonus=focus==1?"1,200":focus==2?"3,000":"2,500";m.Item=(focus==1?"Stone of Magic Protection":focus==2?"Flowing Orb":"Stone of Power")+" (when no other loot is found; free space required)";
   var questState=state=="Accepted"?GuildQuestState.Accepted:state.StartsWith("Completed")?GuildQuestState.AwaitingReport:state.StartsWith("Reported")?GuildQuestState.Reported:GuildQuestState.NoContract;
   m.State=GuildPresentation.State(questState);m.Objective=GuildPresentation.Objective(questState,"sun-kings-retinue");
   m.Action=questState==GuildQuestState.Accepted?"Withdraw contract":questState==GuildQuestState.AwaitingReport?"Report completion":"Close";
   m.Item=focus==1?"Stone of Magic Protection":focus==2?"Flowing Orb":"Stone of Power";
   m.Status=questState==GuildQuestState.NoContract?"Welcome to the Guild. Which quarry will you pursue?":questState==GuildQuestState.Accepted?"Your contract is accepted. Seek your quarry, then return with your report.":questState==GuildQuestState.AwaitingReport?"Quarry defeated! Return to the Guild with your report.":"Well hunted! Your report is recorded. You may take another contract.";
   m.Availability=GuildScreenRules.Cards.Select(c=>GuildPresentation.Availability(questState,c.Id,"sun-kings-retinue",c.Id=="sun-kings-retinue",true)).ToArray();
   m.Prompt=GuildPresentation.Prompt(questState,focus==3,focus==0||focus==3);
   models.Add(m);var rows=GuildScreenRules.Text(m);
   Check(rows.All(t=>t.Box.X>=0&&t.Box.Y>=0&&t.Box.W>0&&t.Box.H>0&&t.Box.X+t.Box.W<=1100&&t.Box.Y+t.Box.H<=680),"all labels inside design bounds");
   Check(rows.All(t=>t.Text.Split('\n').Length*t.Size<=t.Box.H+5),"line heights fit assigned rectangles");
   Check(rows.Any(t=>t.Text.Contains("Loading a save or restarting")),"temporary quest lifetime visible in every state");
   foreach(var size in new[]{(640,480),(1280,720),(1920,1080),(2560,1080),(3840,2160)}){
    float scale=GuildScreenRules.Scale(size.Item1,size.Item2);Check(scale>0&&1100*scale<=size.Item1-31&&680*scale<=size.Item2-31,"centered responsive bounds");
   }
  }
  int selection=0;for(int i=0;i<100;i++){selection=GuildScreenRules.Move(selection,false,true,3);Check(selection==(i+1)%4,"cycle down including quest action");}
  Check(GuildScreenRules.Move(0,true,false,3)==3,"up wraps to report/withdraw");Check(GuildScreenRules.Move(2,true,true,3)==2,"opposing inputs do not double navigate");Check(GuildScreenRules.Move(1,false,false,3)==1,"no input retains focus");
  string longText=new string('X',600);Check(GuildScreenRules.Fit(longText,30,2).Split('\n').Length==2,"giant user token bounded");Check(GuildScreenRules.Fit(longText,30,2).Length<=64,"oversize status bounded");
  var options=new JsonSerializerOptions{WriteIndented=true,IncludeFields=true};File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"guild-ui-preview-models.json"),JsonSerializer.Serialize(models.Select(m=>new{m.Focus,m.Selected,m.State,Rectangles=new[]{GuildScreenRules.Catalogue,GuildScreenRules.Detail,GuildScreenRules.Active},Text=GuildScreenRules.Text(m)}),options));
  Console.WriteLine($"PASS: Guild screen {n} assertions; shared layout bounds, state messages, navigation and long text. Offline models are not gameplay.");
 }
}
