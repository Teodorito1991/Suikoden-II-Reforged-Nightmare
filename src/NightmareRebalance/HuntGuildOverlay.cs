using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NightmareRebalance;
// Existing explicit-rectangle renderer used by the native-talk castle Guild.
public sealed class HuntGuildOverlay:MonoBehaviour
{
 readonly StableHotkeyEdge[] select={new(),new(),new()};readonly Stopwatch timer=Stopwatch.StartNew();
 Font? fallbackFont;readonly Dictionary<int,GUIStyle> styles=new();readonly HashSet<int> smokeScenarios=new();bool faulted;readonly StableHotkeyEdge request=new();bool smokeStarted,smokeExitRequested;
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 public HuntGuildOverlay(IntPtr p):base(p){}
 public void Update()
 {
  if(HuntGuildRuntime.Visible||CastleGuildRuntime.FeedbackVisible||HuntGuildRuntime.RenderSmoke)GuildVisualResources.Poll();
  if(faulted&&HuntGuildRuntime.RenderSmoke){UnityEngine.Application.Quit();return;}
  if(faulted||(!CastleGuildRuntime.Modal&&(!HuntGuildRuntime.Config.EnableSelector||!Plugin.Options.EnableDeveloperTools.Value)))return;
  try{
   if(HuntGuildRuntime.RenderSmoke){
    if(!smokeStarted){smokeStarted=true;HuntGuildRuntime.Visible=true;}
    if(timer.ElapsedMilliseconds>20000&&!smokeExitRequested){smokeExitRequested=true;Plugin.Logger.LogError("Guild render smoke FAILED: resources/drawing incomplete; normal exit requested.");UnityEngine.Application.Quit();return;}
    if(!smokeExitRequested&&HuntGuildRuntime.RenderCalls>=4&&timer.ElapsedMilliseconds>=10000&&GuildVisualResources.Ready&&smokeScenarios.Count==4){smokeExitRequested=true;Plugin.Logger.LogInfo("Guild render resources: "+System.Text.Json.JsonSerializer.Serialize(GuildVisualResources.Snapshot())+"; presentation scenarios="+string.Join(",",smokeScenarios.OrderBy(x=>x)));Plugin.Logger.LogInfo("Guild render smoke PASS: native-looking OnGUI Label/DrawTexture calls="+HuntGuildRuntime.RenderCalls+"; preview only, no save loaded.");OnDestroy();Plugin.Logger.LogInfo("Guild render cleanup: "+System.Text.Json.JsonSerializer.Serialize(GuildVisualResources.Snapshot()));UnityEngine.Application.Quit();return;}
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
  Fill(r,new Color(.025f,.075f,.09f,.97f));
  var f=GuildVisualResources.Frame;
  if(f==null){Fill(new Rect(r.x,r.y,r.width,2),new Color(.9f,.88f,.78f));Fill(new Rect(r.x,r.y+r.height-2,r.width,2),new Color(.9f,.88f,.78f));Fill(new Rect(r.x,r.y,2,r.height),new Color(.9f,.88f,.78f));Fill(new Rect(r.x+r.width-2,r.y,2,r.height),new Color(.9f,.88f,.78f));return;}
  if(frameStyle==null){frameStyle=new GUIStyle();frameStyle.border=new RectOffset(27,27,30,24);}
  frameStyle.normal.background=f.texture;
  GUI.Label(r,"",frameStyle);

 }
 [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
 GuildScreenModel Model()
 {
  bool castle=CastleGuildRuntime.Modal;var defs=HuntRuntime.Definitions;int focus=castle?CastleGuildRuntime.Focus:Array.FindIndex(defs,d=>d.Id==(HuntGuildRuntime.Preview?HuntGuildRuntime.PreviewId:HuntGuildRuntime.SelectedId));focus=Math.Clamp(focus,0,defs.Length);if(HuntGuildRuntime.RenderSmoke)focus=Math.Min(3,(int)(timer.ElapsedMilliseconds/2200));
  string selected=focus<defs.Length?defs[focus].Id:CastleGuildRuntime.Contract??HuntGuildRuntime.SelectedId;
  var def=defs.FirstOrDefault(d=>d.Id==selected)??defs[0];var setting=HuntRuntime.Settings(def);
  var reward=HuntGuildRuntime.Config.Rewards.FirstOrDefault(x=>x.HuntId==def.Id&&x.Enabled);var item=reward==null?null:HuntGuildRuntime.Items.FirstOrDefault(x=>x.Id==reward.RewardId);
  string itemText=HuntGuildRuntime.Config.EnableVictoryItems&&item!=null?item.Name:"Battle loot";
  var state=CastleGuildRuntime.Quest.State;
  string stateName=GuildPresentation.State(state);
  string objective=GuildPresentation.Objective(state,CastleGuildRuntime.Contract??selected);
  var availability=defs.Select(d=>GuildPresentation.Availability(state,d.Id,CastleGuildRuntime.Contract??CastleGuildRuntime.Quest.LastReportedId,HuntRuntime.Settings(d).Enabled,HuntRuntime.Config.EnableExperimentalHunt)).ToArray();
  string prompt=castle&&CastleGuildRuntime.TrainingSelected?(TrainingWar.FortDefenseAvailable||TrainingWar.ChallengeAvailable?"Left / Right: choose drill    Confirm: train":"Confirm: train"):GuildPresentation.Prompt(state,focus==defs.Length,setting.Enabled&&HuntRuntime.Config.EnableExperimentalHunt);
  if(castle&&CastleGuildRuntime.TrainingSelected)
   objective=CastleGuildRuntime.SelectedTraining switch {
    TrainingWar.Drill.FortDefense => "War 2. Command four squads and defend the fort against the Highland advance. No reward.",
    TrainingWar.Drill.CommanderRout => "War 1 challenge. Fight past turn three and rout the enemy commander. No reward.",
    _ => "War 1 standard drill. Play the original battle and its timed ending. No reward."
   };
  if(!castle)prompt="Developer:1/2/3 browse    F10 close    4 requests";
  return new(){Names=defs.Select(d=>d.Name).ToArray(),Availability=availability,Focus=focus,Selected=def.Id,State=stateName,Active=CastleGuildRuntime.ContractName.Length>0?CastleGuildRuntime.ContractName:state==GuildQuestState.Reported?defs.FirstOrDefault(d=>d.Id==CastleGuildRuntime.Quest.LastReportedId)?.Name??"None":"None",Objective=objective,Action=castle?CastleGuildRuntime.ActionLabel:"Close",Status=castle?CastleGuildRuntime.Status:"Developer preview — no live contract changed",Bonus=HuntCatalogueRules.Bonus(def,setting,HuntRuntime.Config).ToString("N0"),Item=itemText,Prompt=prompt,Castle=castle,Preview=HuntGuildRuntime.Preview};
 }
 public void OnGUI()
 {
  bool prompt=!HuntGuildRuntime.Visible&&CastleGuildRuntime.Idle&&CastleGuildRuntime.Near;
  bool feedback=!HuntGuildRuntime.Visible&&CastleGuildRuntime.FeedbackVisible;
  if(smokeExitRequested||faulted||(!prompt&&!feedback&&(!HuntGuildRuntime.Visible||(!HuntGuildRuntime.Context&&!HuntGuildRuntime.Preview))))return;
  try{
   var matrix=GUI.matrix;var color=GUI.color;
   try{
    GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;
    if(prompt||feedback){float w=Math.Min(900,Screen.width-32);var box=new Rect((Screen.width-w)/2,Screen.height-110,w,92);Window(box);GUI.Label(new Rect(box.x+30,box.y+19,w-60,66),GuildScreenRules.Fit(feedback?CastleGuildRuntime.FeedbackText:CastleGuildRuntime.QuestPrompt+"\nConfirm to inspect contracts",Math.Max(25,(int)((w-60)/11)),2),Style(18));return;}
    float scale=GuildScreenRules.Scale(Screen.width,Screen.height),x=(Screen.width-GuildScreenRules.Width*scale)/2,y=(Screen.height-GuildScreenRules.Height*scale)/2;
    var m=Model();
    if(HuntGuildRuntime.RenderSmoke){
     // Presentation-only scenarios: never change Quest, selection, receipt or native state.
     int scenario=Math.Min(3,(int)(timer.ElapsedMilliseconds/2200));m.Focus=scenario;
     if(scenario==3){m.State="Completed — report at Guild";m.Active="Sun King's Retinue";m.Action="Report completion";m.Objective="The quarry is defeated. Report to the attendant; battle spoils are already yours.";m.Prompt="Confirm: report completion";}
     smokeScenarios.Add(scenario);
    }
    Window(ToRect(GuildScreenRules.Catalogue,x,y,scale));Window(ToRect(GuildScreenRules.Detail,x,y,scale));Window(ToRect(GuildScreenRules.Active,x,y,scale));
    float rowY=m.Focus<3?140+m.Focus*76:392;
    Fill(ToRect(new(26,rowY,298,m.Focus<3?66:48),x,y,scale),new Color(.24f,.22f,.12f,.9f));
    var cursor=GuildVisualResources.Cursor;if(cursor!=null)GUI.DrawTexture(ToRect(new(29,rowY+14,15,18),x,y,scale),cursor.texture,ScaleMode.StretchToFill,true);else GUI.Label(ToRect(new(28,rowY+13,18,28),x,y,scale),">",Style(Math.Max(12,(int)(21*scale))));
    foreach(var t in GuildScreenRules.Text(m)){
     var rect=ToRect(t.Box,x,y,scale);var style=Style(Math.Max(10,(int)Math.Round(t.Size*scale)));style.normal.textColor=Tone(t.Tone);
     // Bound every physical line by measured native font width; never permit overlap.
     var lines=t.Text.Split('\n');for(int i=0;i<lines.Length;i++){
      string line=lines[i];while(line.Length>1&&style.CalcSize(new GUIContent(line)).x>rect.width)line=line[..^1];
      if(line.Length<lines[i].Length&&line.Length>3)line=line[..^3]+"...";lines[i]=line;
     }
     string text=string.Join("\n",lines);int fitted=style.fontSize;
     while(fitted>10&&style.CalcSize(new GUIContent(text)).y>rect.height){fitted--;style=Style(fitted);style.normal.textColor=Tone(t.Tone);}
     GUI.Label(rect,text,style);
    }
    HuntGuildRuntime.Drawn();
   }finally{GUI.matrix=matrix;GUI.color=color;}
  }catch(Exception e){faulted=true;HuntGuildRuntime.RenderFailed(e);}
 }
}
