using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NightmareRebalance;
// Existing explicit-rectangle renderer used by the native-talk castle Guild.
public sealed class HuntGuildOverlay:MonoBehaviour
{
 readonly Stopwatch timer=Stopwatch.StartNew();
 Font? fallbackFont;readonly Dictionary<int,GUIStyle> styles=new();bool faulted;readonly StableHotkeyEdge request=new();
 bool smokeExitRequested;
 int smokeScreen,smokePaints;
 long nextCaptureAt=-1;
 static readonly (GuildPage Page,int Focus,int Drill,string File)[] SmokeScreens =
 {
  (GuildPage.Services,0,0,"01-services.png"),
  (GuildPage.Hunts,0,0,"02-hunt-sun-king.png"),
  (GuildPage.Hunts,1,0,"03-hunt-dreamer.png"),
  (GuildPage.Hunts,2,0,"04-hunt-thornwake.png"),
  (GuildPage.Hunts,4,0,"09-patrol-contracts.png"),
  (GuildPage.Hunts,9,0,"10-grand-trials.png"),
  (GuildPage.Training,2,0,"05-army-training.png"),
  (GuildPage.Difficulty,0,2,"06-standard.png"),
  (GuildPage.Difficulty,1,2,"07-veteran.png"),
  (GuildPage.Status,0,0,"08-guild-status.png")
 };
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 public HuntGuildOverlay(IntPtr p):base(p){}
 public void Update()
 {
  if(HuntGuildRuntime.RenderSmoke)
  {
   UpdateRenderSmoke();
   return;
  }
  if(HuntGuildRuntime.Visible||CastleGuildRuntime.FeedbackVisible)GuildVisualResources.Poll();
  if(faulted||(!CastleGuildRuntime.Modal&&
      (!HuntGuildRuntime.Config.EnableSelector||!Plugin.Options.EnableDeveloperTools.Value)))return;
  try{
   if(CastleGuildRuntime.Modal&&!CastleGuildRuntime.Context)CastleGuildRuntime.Close(false);
   if(HuntGuildRuntime.Visible&&!HuntGuildRuntime.Context&&!HuntGuildRuntime.Preview)HuntGuildRuntime.Visible=false;
   if(CastleGuildRuntime.Modal)return;
   GetWindowThreadProcessId(GetForegroundWindow(),out uint foreground);bool focused=foreground==(uint)Environment.ProcessId;
   short rv=GetAsyncKeyState(0x34);if(request.Poll(focused&&HuntGuildRuntime.Visible,(rv&0x8000)!=0,(rv&1)!=0,timer.ElapsedMilliseconds))HuntGuildRuntime.Requests(!HuntRuntime.Config.EnableExperimentalHunt);
  }catch(Exception e){faulted=true;HuntGuildRuntime.RenderFailed(e);}
 }

 void UpdateRenderSmoke()
 {
  if(smokeExitRequested)return;
  try
  {
   if(faulted||GuildVisualResources.Failed||timer.ElapsedMilliseconds>35000)
    throw new InvalidOperationException($"Preview incomplete: {smokePaints} repaint passes, {smokeScreen}/{SmokeScreens.Length} screenshots queued. Run on a separate Windows desktop with a visible game surface.");
   if(nextCaptureAt<0&&timer.ElapsedMilliseconds>=3000&&
       UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Main"&&
       GuildVisualResources.Poll()&&GuildVisualResources.Ready)
   {
    Directory.CreateDirectory(HuntGuildRuntime.CaptureDirectory!);
    nextCaptureAt=timer.ElapsedMilliseconds+2000;
   }
   if(smokeScreen!=SmokeScreens.Length||timer.ElapsedMilliseconds<nextCaptureAt||
       !SmokeScreens.All(s=>new FileInfo(Path.Combine(HuntGuildRuntime.CaptureDirectory!,s.File)) is {Exists:true,Length:>0}))return;
   Plugin.Logger.LogInfo($"Guild render smoke PASS: {smokePaints} repaint passes; {smokeScreen} PNGs at {Screen.width}x{Screen.height}; original frame, cursor and font loaded.");
  }
  catch(Exception e){Plugin.Logger.LogError("Guild render smoke FAILED: "+e.Message);}
  smokeExitRequested=true;
  UnityEngine.Application.Quit();
 }
 public void OnDestroy(){if(frameStyle!=null){frameStyle.normal.background=null;frameStyle=null;}foreach(var style in styles.Values)style.font=null;styles.Clear();GuildVisualResources.Release();if(fallbackFont!=null)UnityEngine.Object.Destroy(fallbackFont);fallbackFont=null;if(HuntGuildRuntime.RenderSmoke)Plugin.Logger.LogInfo($"Guild render cleanup: original asset loads={GuildVisualResources.Loads}, releases={GuildVisualResources.Releases}.");}
 GUIStyle Style(int size)
 {
  if(!styles.TryGetValue(size,out var style)){style=new GUIStyle{fontSize=size,wordWrap=false,alignment=TextAnchor.UpperLeft,richText=false};style.normal.textColor=Color.white;styles[size]=style;}
  if(GuildVisualResources.Font!=null)style.font=GuildVisualResources.Font;
  else{if(fallbackFont==null)fallbackFont=Font.CreateDynamicFontFromOSFont("Georgia",22);style.font=fallbackFont;}
  return style;
 }
 static Color Tone(string tone)=>tone=="gold"?new Color(1,.86f,.52f):tone=="muted"?new Color(.72f,.78f,.79f):new Color(.96f,.94f,.86f);
 static void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,false);GUI.color=Color.white;}
 static Rect ToRect(GuildBox b,float x,float y,float scale)=>new(x+b.X*scale,y+b.Y*scale,b.W*scale,b.H*scale);
 static GUIStyle? frameStyle;
 static void Window(Rect r)
 {
  Fill(r,new Color(.029f,.063f,.145f,.97f));
  var f=GuildVisualResources.Frame;
  if(f==null){Fill(new Rect(r.x,r.y,r.width,2),new Color(.9f,.88f,.78f));Fill(new Rect(r.x,r.y+r.height-2,r.width,2),new Color(.9f,.88f,.78f));Fill(new Rect(r.x,r.y,2,r.height),new Color(.9f,.88f,.78f));Fill(new Rect(r.x+r.width-2,r.y,2,r.height),new Color(.9f,.88f,.78f));return;}
  if(frameStyle==null){frameStyle=new GUIStyle();frameStyle.border=new RectOffset(27,27,30,24);}
  frameStyle.normal.background=f.texture;
  GUI.Label(r,"",frameStyle);

 }
 static readonly GuildMenuRow[] Services =
 {
  new("Hunt Contracts", "Choose a quarry"),
  new("Army Training", "Replay an original battle"),
  new("Guild Status", "Review your contract"),
  new("Leave", "Return to the castle")
 };

 static readonly GuildMenuRow[] Drills =
 {
  new("War 1", "Original battlefield"),
  new("Fort Defense", "Hold the fort"),
  new("Commander Rout", "Lead the allied squads"),
  new("War 3", "Original campaign scenario")
 };

 [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
 GuildMenuView Model(GuildPage page, int focus, int drillIndex, bool preview, bool developer)
 {
  var state = CastleGuildRuntime.Quest.State;
  string status = preview ? "Presentation preview" : CastleGuildRuntime.Status;
  string back = developer ? "F10 closes" : page == GuildPage.Services ? "Close" : "Back";
  if (developer) status = HuntGuildRuntime.Message;

  if (page == GuildPage.Services)
  {
   var row = Services[focus];
   return new("SERVICES", Services, focus, row.Title, "", row.Note,
       "", "", status, focus == 3 ? "Leave" : "Open", back, preview);
  }

  if (page == GuildPage.Status)
  {
   if(preview)return new("GUILD STATUS",new[]{new GuildMenuRow("Report completion","Spoils already paid")},
       0,"Sun King's Retinue","Completed · Awaiting report",
       "Return your report to the Guild. You can take the contract again once the report is recorded.",
       "", "Contracts last until a save is loaded or the game is restarted.", status,"Report completion",back,true);
   string action = state == GuildQuestState.AwaitingReport ? "Report completion" :
       state == GuildQuestState.Accepted ? "Withdraw contract" : "Back";
   string id = CastleGuildRuntime.Contract ?? CastleGuildRuntime.Quest.LastReportedId ?? "";
   string title = id.Length == 0 ? "No active contract" : GuildScreenRules.Card(id).Name;
   return new("GUILD STATUS", new[] { new GuildMenuRow(action, "") }, 0, title,
       GuildPresentation.State(state), CastleGuildRuntime.QuestSummary, "",
       "Contracts last until a save is loaded or the game is restarted.", status, action, back, preview);
  }

  if (page == GuildPage.Hunts)
  {
   var defs = HuntRuntime.Definitions;
   if(defs.Length==0)
    return new("HUNT CONTRACTS",new[]{new GuildMenuRow("Unavailable","")},0,
        "Contracts unavailable","","The Guild could not load its contract definitions.","","",status,"Unavailable",back,preview);
   focus=Math.Clamp(focus,0,defs.Length-1);
   var definition = defs[focus];
   var card = GuildScreenRules.Card(definition.Id);
   var setting = HuntRuntime.Settings(definition);
   bool enabled = HuntRuntime.Config.EnableExperimentalHunt && CombatReforgedRuntime.Config.Enabled;
   bool normal = preview || DifficultyProvenance.Difficulty(out int mode, out _) && mode == 0;
   var rows = defs.Select(d => new GuildMenuRow(d.Name, $"Tier {GuildScreenRules.Card(d.Id).Tier} · "+GuildPresentation.Availability(state, d.Id,
       CastleGuildRuntime.Contract ?? CastleGuildRuntime.Quest.LastReportedId,
       HuntRuntime.Settings(d).Enabled, enabled && normal))).ToArray();
   var rule = HuntGuildRuntime.Config.Rewards.FirstOrDefault(x => x.HuntId == definition.Id && x.Enabled);
   var item = HuntGuildRuntime.Config.EnableVictoryItems && rule != null ?
       HuntGuildRuntime.Items.FirstOrDefault(x => x.Id == rule.RewardId) : null;
   string? itemLabel = item?.Name;
   if(item is {Kind:0,UseCount:>1})itemLabel += $" ({item.UseCount} uses)";
   string bonus = HuntCatalogueRules.Bonus(definition, setting, HuntRuntime.Config).ToString("N0");
   string rewardCondition = item == null ? "" : " Only if native loot is empty and your bag has room.";
   string reason = !normal ? "Available on Normal difficulty." : !enabled ? "Hunt requests are disabled." :
       !setting.Enabled ? (definition.WalkingVariants.Length>0 ? "Disabled in Guild settings." : card.Evidence) :
       state == GuildQuestState.AwaitingReport ? "Report the completed hunt under Guild Status." :
       state == GuildQuestState.Accepted ? "Finish or withdraw your active contract first." :
       definition.WalkingVariants.Length>0 ? card.Evidence+rewardCondition :
       item == null ? "Original battle loot and purse limits apply." : "Only if native loot is empty and your bag has room.";
   string confirm = normal && enabled && setting.Enabled && state is not (GuildQuestState.Accepted or GuildQuestState.AwaitingReport)
       ? "Accept" : "Unavailable";
   if(developer&&confirm!="Unavailable")confirm="Select";
   return GuildRebornScreen.Paginate(new("HUNT CONTRACTS", rows, focus, card.Name, $"{card.Destination}  ·  {card.Danger}",
       $"{card.Monsters}\n{card.Tactics}", $"Battle Potch + {bonus} bonus" +
           (itemLabel == null ? "" : $"\nPossible item  {itemLabel}"), reason, status, confirm, back, preview));
  }

  var drill = (TrainingWar.Drill)(page == GuildPage.Difficulty ? drillIndex : focus);
  string unavailable = preview ? "" : TrainingWar.Unavailable(drill);
  bool veteranAllowed = preview || TrainingWar.VeteranAvailable(drill);
  bool veteran = page == GuildPage.Difficulty && focus == 1;
  string description = drill switch
  {
   TrainingWar.Drill.FortDefense => "Protect the fort with four squads. Rout the opposing commander before the breach.",
   TrainingWar.Drill.CommanderRout => "Command the allied squads and fight beyond turn three until one army is routed.",
   TrainingWar.Drill.War3 => "Fight on the original War 3 battlefield. The story timer can end the drill.",
   _ => "Practice on the original War 1 battlefield. The story timer can end the drill."
  };
  var trainingRows = page == GuildPage.Training ? Drills.Select((row,i) =>
      new GuildMenuRow(row.Title, preview || TrainingWar.Unavailable((TrainingWar.Drill)i).Length == 0 ?
          row.Note : "Unavailable")).ToArray() :
      new[] { new GuildMenuRow("Standard", "Current scenario rules"),
          new GuildMenuRow("Veteran", veteranAllowed ? "Coordinated enemy tactics" : "Unavailable") };
  string reward = Plugin.Options.EnableTrainingRewards.Value ?
      (page == GuildPage.Training && veteranAllowed ?
          $"{TrainingResultRules.Potch(false):N0} Standard  /  {TrainingResultRules.Potch(true):N0} Veteran Potch" :
          TrainingResultRules.Potch(veteran).ToString("N0") + " Potch") : "Training rewards disabled";
  string condition = unavailable.Length != 0 ? unavailable : veteran && !veteranAllowed ?
      "Veteran requires the native tactics options and a verified battlefield." :
      "Rewards require routing the opposing commander. Timed endings and defeat pay no Potch.";
  return new(page == GuildPage.Training ? "ARMY TRAINING" : "CHOOSE DIFFICULTY", trainingRows, focus,
      Drills[(int)drill].Title, Drills[(int)drill].Note, description, reward, condition, status,
      unavailable.Length != 0 || veteran && !veteranAllowed ? "Unavailable" :
          page == GuildPage.Training ? "Choose difficulty" : "Start drill", back, preview);
 }

 [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
 void Label(GuildDrawText text,float x,float y,float scale)
 {
  var rect=ToRect(text.Box,x,y,scale);
  var content=new GUIContent(text.Text);
  int size=Math.Max(12,(int)Math.Round(text.Size*scale));
  var style=Style(size);
  style.wordWrap=true;
  int min=Math.Max(11,(int)(14*scale));
  while(size>min&&style.CalcHeight(content,rect.width)>rect.height)
  {
   style=Style(--size);
   style.wordWrap=true;
  }
  style.normal.textColor=Tone(text.Tone);
  GUI.Label(rect,content,style);
 }

 public void OnGUI()
 {
  bool previewSmoke=HuntGuildRuntime.RenderSmoke;
  if(previewSmoke&&(nextCaptureAt<0||smokeScreen>=SmokeScreens.Length||Event.current?.type!=EventType.Repaint))return;
  bool prompt=!previewSmoke&&!HuntGuildRuntime.Visible&&CastleGuildRuntime.Idle&&CastleGuildRuntime.Near;
  bool feedback=!previewSmoke&&!HuntGuildRuntime.Visible&&CastleGuildRuntime.FeedbackVisible;
  if(smokeExitRequested||faulted||(!previewSmoke&&!prompt&&!feedback&&(!HuntGuildRuntime.Visible||(!HuntGuildRuntime.Context&&!HuntGuildRuntime.Preview))))return;
  try
  {
   var matrix=GUI.matrix;var color=GUI.color;
   try
   {
    GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;
    if(prompt||feedback)
    {
     float width=Math.Min(820,Screen.width-48);
     var box=new Rect((Screen.width-width)/2,Screen.height-110,width,84);
     Window(box);
     Label(new(new(26,18,width-52,51),feedback?CastleGuildRuntime.FeedbackText:
         CastleGuildRuntime.QuestPrompt+"\nConfirm to speak",20),box.x,box.y,1);
     return;
    }

    var menu=CastleGuildRuntime.Menu;
    bool developer=!CastleGuildRuntime.Modal&&!previewSmoke;
    var page=developer ? GuildPage.Hunts : menu.Page;
    int focus=developer ? HuntGuildRuntime.DeveloperFocus : menu.Focus;
    int drill=menu.Drill;
    if(previewSmoke)
    {
     var screen=SmokeScreens[smokeScreen];
     (page,focus,drill)=(screen.Page,screen.Focus,screen.Drill);
    }
    var view=Model(page,focus,drill,HuntGuildRuntime.Preview||HuntGuildRuntime.RenderSmoke,developer);
    float scale=GuildRebornScreen.Scale(Screen.width,Screen.height);
    float x=(Screen.width-GuildRebornScreen.Width*scale)/2,y=(Screen.height-GuildRebornScreen.Height*scale)/2;
    foreach(var box in new[]{GuildRebornScreen.Heading,GuildRebornScreen.List,GuildRebornScreen.Detail,GuildRebornScreen.Footer})
     Window(ToRect(box,x,y,scale));

    var row=GuildRebornScreen.Row(view.Focus);
    Fill(ToRect(row,x,y,scale),new Color(.19f,.24f,.37f,.94f));
    Fill(ToRect(new(row.X+7,row.Y+row.H-1,row.W-14,1),x,y,scale),new Color(.80f,.67f,.39f,.64f));
    var cursor=GuildVisualResources.Cursor;
    if(cursor!=null)GUI.DrawTexture(ToRect(new(row.X+7,row.Y+12,17,19),x,y,scale),cursor.texture,ScaleMode.StretchToFill,true);
    else Label(new(new(row.X+7,row.Y+8,20,24),">",20,"gold"),x,y,scale);
    foreach(var line in GuildRebornScreen.Text(view))Label(line,x,y,scale);
    HuntGuildRuntime.Drawn();
    if(previewSmoke)
    {
     smokePaints++;
     if(timer.ElapsedMilliseconds>=nextCaptureAt)
     {
      var file=SmokeScreens[smokeScreen++].File;
      ScreenCapture.CaptureScreenshot(Path.Combine(HuntGuildRuntime.CaptureDirectory!,file));
      Plugin.Logger.LogInfo($"Guild preview painted {file} at {Screen.width}x{Screen.height}.");
      nextCaptureAt=timer.ElapsedMilliseconds+700;
     }
    }
   }
   finally{GUI.matrix=matrix;GUI.color=color;}
  }
  catch(Exception e){faulted=true;HuntGuildRuntime.RenderFailed(e);}
 }
}
