using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NightmareRebalance;
// Existing explicit-rectangle renderer used by the native-talk castle Guild.
public sealed class HuntGuildOverlay:MonoBehaviour
{
 readonly StableHotkeyEdge[] select={new(),new(),new()};readonly Stopwatch timer=Stopwatch.StartNew();
 Font? fallbackFont;readonly Dictionary<int,GUIStyle> styles=new();readonly HashSet<int> smokeScenarios=new();bool faulted;readonly StableHotkeyEdge request=new();bool smokeStarted,smokeExitRequested;long smokeReadyAt=-1;
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 public HuntGuildOverlay(IntPtr p):base(p){}
 public void Update()
 {
  if(HuntGuildRuntime.Visible||CastleGuildRuntime.FeedbackVisible||
      (HuntGuildRuntime.RenderSmoke&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Main"&&timer.ElapsedMilliseconds>=3000))
      GuildVisualResources.Poll();
  if(faulted&&HuntGuildRuntime.RenderSmoke){UnityEngine.Application.Quit();return;}
  if(faulted||(!HuntGuildRuntime.RenderSmoke&&!CastleGuildRuntime.Modal&&
      (!HuntGuildRuntime.Config.EnableSelector||!Plugin.Options.EnableDeveloperTools.Value)))return;
  try{
   if(HuntGuildRuntime.RenderSmoke){
    if(!smokeStarted&&GuildVisualResources.Ready){smokeStarted=true;smokeReadyAt=timer.ElapsedMilliseconds;}
    if(timer.ElapsedMilliseconds>35000&&!smokeExitRequested){smokeExitRequested=true;Plugin.Logger.LogError("Guild render smoke FAILED: "+System.Text.Json.JsonSerializer.Serialize(GuildVisualResources.Snapshot())+"; draws="+HuntGuildRuntime.RenderCalls+"; presentations="+smokeScenarios.Count+"; normal exit requested.");UnityEngine.Application.Quit();return;}
    if(!smokeExitRequested&&smokeReadyAt>=0&&timer.ElapsedMilliseconds-smokeReadyAt>=10000&&smokeScenarios.Count==7){smokeExitRequested=true;Plugin.Logger.LogInfo("Guild render resources: "+System.Text.Json.JsonSerializer.Serialize(GuildVisualResources.Snapshot())+"; presentation scenarios="+string.Join(",",smokeScenarios.OrderBy(x=>x)));Plugin.Logger.LogInfo("Guild render smoke PASS: native-looking OnGUI Label/DrawTexture calls="+HuntGuildRuntime.RenderCalls+"; preview only, no save loaded.");OnDestroy();Plugin.Logger.LogInfo("Guild render cleanup: "+System.Text.Json.JsonSerializer.Serialize(GuildVisualResources.Snapshot()));UnityEngine.Application.Quit();return;}
   }
   if(CastleGuildRuntime.Modal&&!CastleGuildRuntime.Context)CastleGuildRuntime.Close(false);
   if(HuntGuildRuntime.Visible&&!HuntGuildRuntime.Context&&!HuntGuildRuntime.Preview)HuntGuildRuntime.Visible=false;
   if(CastleGuildRuntime.Modal)return;
   GetWindowThreadProcessId(GetForegroundWindow(),out uint foreground);bool focused=foreground==(uint)Environment.ProcessId;
   short rv=GetAsyncKeyState(0x34);if(request.Poll(focused&&HuntGuildRuntime.Visible,(rv&0x8000)!=0,(rv&1)!=0,timer.ElapsedMilliseconds))HuntGuildRuntime.Requests(!HuntRuntime.Config.EnableExperimentalHunt);
   for(int i=0;i<select.Length;i++){short v=GetAsyncKeyState(0x31+i);if(select[i].Poll(focused&&HuntGuildRuntime.Visible,(v&0x8000)!=0,(v&1)!=0,timer.ElapsedMilliseconds)&&i<HuntRuntime.Definitions.Length)HuntGuildRuntime.Select(HuntRuntime.Definitions[i].Id);}
  }catch(Exception e){faulted=true;HuntGuildRuntime.RenderFailed(e);}
 }
 public void OnDestroy(){if(frameStyle!=null){frameStyle.normal.background=null;frameStyle=null;}foreach(var style in styles.Values)style.font=null;styles.Clear();GuildVisualResources.Release();if(fallbackFont!=null)UnityEngine.Object.Destroy(fallbackFont);fallbackFont=null;}
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
  if (developer) status = "1–3 Browse     F10 Close     4 Toggle requests";

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
   if(defs.Length!=GuildScreenRules.Cards.Length)
    return new("HUNT CONTRACTS",GuildScreenRules.Cards.Select(card=>new GuildMenuRow(card.Name,"Unavailable")).ToArray(),
        focus,"Contracts unavailable","","The Guild could not load its contract definitions.","","",status,"Unavailable",back,preview);
   var definition = defs[focus];
   var card = GuildScreenRules.Card(definition.Id);
   var setting = HuntRuntime.Settings(definition);
   bool enabled = HuntRuntime.Config.EnableExperimentalHunt && CombatReforgedRuntime.Config.Enabled;
   bool normal = preview || DifficultyProvenance.Difficulty(out int mode, out _) && mode == 0;
   var rows = defs.Select(d => new GuildMenuRow(d.Name, GuildPresentation.Availability(state, d.Id,
       CastleGuildRuntime.Contract ?? CastleGuildRuntime.Quest.LastReportedId,
       HuntRuntime.Settings(d).Enabled, enabled && normal))).ToArray();
   var rule = HuntGuildRuntime.Config.Rewards.FirstOrDefault(x => x.HuntId == definition.Id && x.Enabled);
   string? item = HuntGuildRuntime.Config.EnableVictoryItems && rule != null ?
       HuntGuildRuntime.Items.FirstOrDefault(x => x.Id == rule.RewardId)?.Name : null;
   string bonus = HuntCatalogueRules.Bonus(definition, setting, HuntRuntime.Config).ToString("N0");
   string reason = !normal ? "Available on Normal difficulty." : !enabled ? "Hunt requests are disabled." :
       !setting.Enabled ? card.Evidence : state == GuildQuestState.AwaitingReport ? "Report the completed hunt under Guild Status." :
       state == GuildQuestState.Accepted ? "Finish or withdraw your active contract first." :
       item == null ? "Original battle loot and purse limits apply." : "The item depends on original loot and available bag space.";
   string confirm = normal && enabled && setting.Enabled && state is not (GuildQuestState.Accepted or GuildQuestState.AwaitingReport)
       ? "Accept" : "Unavailable";
   return new("HUNT CONTRACTS", rows, focus, card.Name, $"{card.Destination}  ·  {card.Danger}",
       $"{card.Monsters}\n{card.Tactics}", $"Battle Potch + {bonus} bonus" +
           (item == null ? "" : $"\nPossible item  {item}"), reason, status, confirm, back, preview);
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
  bool previewSmoke=HuntGuildRuntime.RenderSmoke&&smokeStarted&&!Application.isBatchMode;
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
    bool developer=!CastleGuildRuntime.Modal&&!HuntGuildRuntime.RenderSmoke;
    var page=developer ? GuildPage.Hunts : menu.Page;
    int focus=developer ? Math.Max(0,Array.FindIndex(HuntRuntime.Definitions,
        d=>d.Id==(HuntGuildRuntime.Preview?HuntGuildRuntime.PreviewId:HuntGuildRuntime.SelectedId))) : menu.Focus;
    int drill=menu.Drill;
    int previewFrame=-1;
    if(HuntGuildRuntime.RenderSmoke)
    {
     int preview=smokeReadyAt<0?0:Math.Min(6,(int)((timer.ElapsedMilliseconds-smokeReadyAt)/1400));
     (page,focus,drill)=preview switch
     {
      0 => (GuildPage.Services,0,0),
      1 or 2 or 3 => (GuildPage.Hunts,preview-1,0),
      4 => (GuildPage.Training,2,0),
      5 => (GuildPage.Difficulty,1,2),
      _ => (GuildPage.Status,0,0)
     };
     previewFrame=preview;
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
    if(previewFrame>=0&&GuildVisualResources.Ready&&Event.current.type==EventType.Repaint&&
        smokeScenarios.Add(previewFrame))
    {
     Plugin.Logger.LogInfo($"Guild presentation {previewFrame} drawn at {Screen.width}x{Screen.height}.");
     if(Environment.GetCommandLineArgs().Contains("--reforged-private-startup-smoke")&&
         File.Exists(Path.Combine(BepInEx.Paths.GameRootPath,"PRIVATE_DO_NOT_UPLOAD.txt")))
      ScreenCapture.CaptureScreenshot(Path.Combine(BepInEx.Paths.GameRootPath,
          $"guild-preview-{Screen.width}x{Screen.height}-{previewFrame}.png"));
    }
   }
   finally{GUI.matrix=matrix;GUI.color=color;}
  }
  catch(Exception e){faulted=true;HuntGuildRuntime.RenderFailed(e);}
 }
}
