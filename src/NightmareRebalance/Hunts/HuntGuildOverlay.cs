using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NightmareRebalance;
// Existing explicit-rectangle renderer used by the native-talk castle Guild.
public sealed class HuntGuildOverlay:MonoBehaviour
{
 readonly Stopwatch timer=Stopwatch.StartNew();
 Font? fallbackFont;readonly Dictionary<int,GUIStyle> styles=new();bool faulted;readonly StableHotkeyEdge request=new(),scannerKey=new(),helpKey=new();
 bool smokeExitRequested,smokeCapturePending;
 int smokeCaptureFrame;
 HuntDefinition? fieldOffer;
 int smokeScreen,smokePaints;
 long nextCaptureAt=-1;
 static readonly (GuildPage Page,int Focus,int Drill,string File,bool FieldOffer)[] SmokeScreens =
 {
  (GuildPage.Services,0,0,"01-services.png",false),
  (GuildPage.Hunts,2,0,"02-hunt-sun-king.png",false),
  (GuildPage.Hunts,3,0,"03-hunt-dreamer.png",false),
  (GuildPage.Hunts,6,0,"04-hunt-thornwake.png",false),
  (GuildPage.Hunts,0,0,"09-patrol-contracts.png",false),
  (GuildPage.Hunts,9,0,"10-grand-trials.png",false),
  (GuildPage.Training,2,0,"05-army-training.png",false),
  (GuildPage.Difficulty,0,2,"06-standard.png",false),
  (GuildPage.Difficulty,1,2,"07-veteran.png",false),
  (GuildPage.Status,0,0,"08-guild-status.png",false),
  (GuildPage.Services,0,0,"11-field-contract.png",true),
  (GuildPage.Strategy,0,0,"12-party-strategy.png",false),
  (GuildPage.Echoes,0,0,"13-hall-story-locked.png",false),
  (GuildPage.Echoes,1,0,"14-hall-harpy-memory.png",false),
  (GuildPage.Echoes,4,0,"15-hall-second-page.png",false),
  (GuildPage.Echoes,5,0,"16-hall-stone-golem.png",false),
  (GuildPage.Echoes,6,0,"17-hall-abomination.png",false),
  (GuildPage.Echoes,7,0,"18-hall-worm.png",false),
  (GuildPage.EchoDifficulty,0,5,"19-original-tier.png",false),
  (GuildPage.EchoDifficulty,1,5,"20-nightmare-golem.png",false),
  (GuildPage.EchoDifficulty,1,6,"21-nightmare-abomination.png",false),
  (GuildPage.EchoDifficulty,1,2,"22-nightmare-wolf.png",false),
  (GuildPage.Echoes,8,0,"23-bone-dragon.png",false),
  (GuildPage.Echoes,10,0,"24-neclord-final.png",false),
  (GuildPage.Echoes,11,0,"25-lucia-retinue.png",false),
  (GuildPage.Echoes,14,0,"26-seed-culgan.png",false),
  (GuildPage.Echoes,16,0,"27-mist-shade.png",false),
  (GuildPage.EchoDifficulty,1,14,"28-nightmare-commanders.png",false),
  (GuildPage.EchoDifficulty,1,0,"29-nightmare-rat.png",false),
  (GuildPage.EchoDifficulty,1,10,"30-nightmare-neclord.png",false),
  (GuildPage.Echoes,17,0,"31-lucia-castle.png",false),
  (GuildPage.Echoes,18,0,"32-rowd-patrol.png",false),
  (GuildPage.EchoDifficulty,1,18,"33-nightmare-rowd.png",false)
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
  if(!faulted)
  {
   GetWindowThreadProcessId(GetForegroundWindow(),out uint fg);
   bool focusedApp=fg==(uint)Environment.ProcessId;
   short f3=GetAsyncKeyState(0x72);
   if(helpKey.Poll(focusedApp,(f3&0x8000)!=0,(f3&1)!=0,timer.ElapsedMilliseconds))
    Reforged2Runtime.ToggleHelpCard();
   if(Combat.State is {Ready:true})
   {
    short f4=GetAsyncKeyState(0x73);
    if(scannerKey.Poll(focusedApp,(f4&0x8000)!=0,(f4&1)!=0,timer.ElapsedMilliseconds))
     Reforged2Runtime.ToggleScanner();
   }
  }
  fieldOffer=null;
  if(!faulted&&CastleGuildRuntime.Config.EnableFieldReissue&&!HuntGuildRuntime.Visible)
  {
   try
   {
    if(HuntGuildRuntime.NativeMainContext)fieldOffer=CastleGuildRuntime.LocalOffer(HuntGuildRuntime.CommandOwner);
   }
   catch(Exception e){Plugin.Debug("Local contract prompt unavailable: "+e.GetType().Name);}
  }
  if(HuntGuildRuntime.Visible||CastleGuildRuntime.FeedbackVisible||fieldOffer!=null)GuildVisualResources.Poll();
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
   if(faulted||GuildVisualResources.Failed||timer.ElapsedMilliseconds>10000+SmokeScreens.Length*1100)
    throw new InvalidOperationException($"Preview incomplete: {smokePaints} repaint passes, {smokeScreen}/{SmokeScreens.Length} screenshots queued. Run on a separate Windows desktop with a visible game surface.");
   if(nextCaptureAt<0&&timer.ElapsedMilliseconds>=3000&&
       UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Main"&&
       GuildVisualResources.Poll()&&GuildVisualResources.Ready)
   {
    Directory.CreateDirectory(HuntGuildRuntime.CaptureDirectory!);
    nextCaptureAt=timer.ElapsedMilliseconds+2000;
   }
   // A screenshot is fulfilled after rendering. Keep the same view throughout
   // all repaint calls and the following frames; never advance inside OnGUI.
   if(smokeCapturePending && Time.frameCount>smokeCaptureFrame+2)
   {
    smokeCapturePending=false;smokeScreen++;nextCaptureAt=timer.ElapsedMilliseconds+700;
   }
   if(smokeScreen!=SmokeScreens.Length||timer.ElapsedMilliseconds<nextCaptureAt||
       !SmokeScreens.All(s=>new FileInfo(Path.Combine(HuntGuildRuntime.CaptureDirectory!,s.File)) is {Exists:true,Length:>0}))return;
   Plugin.Logger.LogInfo($"Guild render smoke PASS: {smokePaints} repaint passes; {smokeScreen} PNGs at {Screen.width}x{Screen.height}; original frame, cursor and font loaded.");
  }
  catch(Exception e){Plugin.Logger.LogError("Guild render smoke FAILED: "+e.Message);}
  smokeExitRequested=true;
  UnityEngine.Application.Quit();
 }
 public void OnDestroy()
 {
  if(frameStyle!=null){frameStyle.normal.background=null;frameStyle=null;}
  if(windowGradTex!=null){UnityEngine.Object.Destroy(windowGradTex);windowGradTex=null;}
  if(rowGradTex!=null){UnityEngine.Object.Destroy(rowGradTex);rowGradTex=null;}
  if(plaqueGradTex!=null){UnityEngine.Object.Destroy(plaqueGradTex);plaqueGradTex=null;}
  foreach(var style in styles.Values)style.font=null;styles.Clear();GuildVisualResources.Release();
  if(fallbackFont!=null)UnityEngine.Object.Destroy(fallbackFont);fallbackFont=null;
  if(HuntGuildRuntime.RenderSmoke)Plugin.Logger.LogInfo($"Guild render cleanup: original asset loads={GuildVisualResources.Loads}, releases={GuildVisualResources.Releases}.");
 }
 GUIStyle Style(int size)
 {
  if(!styles.TryGetValue(size,out var style)){style=new GUIStyle{fontSize=size,wordWrap=false,alignment=TextAnchor.UpperLeft,richText=false};style.normal.textColor=Color.white;styles[size]=style;}
  if(GuildVisualResources.Font!=null)style.font=GuildVisualResources.Font;
  else{if(fallbackFont==null)fallbackFont=Font.CreateDynamicFontFromOSFont("Georgia",22);style.font=fallbackFont;}
  return style;
 }
 static Color Tone(string tone)=>tone switch
 {
  "gold" => new Color(1,.88f,.54f),
  "success" => new Color(.54f,.94f,.68f),
  "alert" => new Color(.98f,.62f,.44f),
  "muted" => new Color(.76f,.82f,.86f),
  _ => new Color(.98f,.96f,.90f)
 };
 static Texture2D? windowGradTex,rowGradTex,plaqueGradTex;
 static Texture2D MakeVerticalGradient(Color top,Color mid,Color bottom)
 {
  var tex=new Texture2D(1,32,TextureFormat.RGBA32,false);
  tex.wrapMode=TextureWrapMode.Clamp;
  for(int y=0;y<32;y++)
  {
   float t=y/31f;
   Color c=t<0.5f?Color.Lerp(bottom,mid,t*2f):Color.Lerp(mid,top,(t-0.5f)*2f);
   tex.SetPixel(0,y,c);
  }
  tex.Apply(false,false);
  return tex;
 }
 static void EnsureGradients()
 {
  windowGradTex??=MakeVerticalGradient(new Color(.11f,.23f,.52f,.96f),new Color(.07f,.15f,.36f,.97f),new Color(.035f,.075f,.20f,.98f));
  rowGradTex??=MakeVerticalGradient(new Color(.24f,.40f,.72f,.95f),new Color(.17f,.29f,.55f,.95f),new Color(.11f,.19f,.39f,.95f));
  plaqueGradTex??=MakeVerticalGradient(new Color(.20f,.17f,.09f,.72f),new Color(.14f,.12f,.07f,.76f),new Color(.09f,.08f,.05f,.80f));
 }
 static void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,false);GUI.color=Color.white;}
 static Rect ToRect(GuildBox b,float x,float y,float scale)=>new(x+b.X*scale,y+b.Y*scale,b.W*scale,b.H*scale);
 static GUIStyle? frameStyle;
 static void Window(Rect r)
 {
  EnsureGradients();
  // Soft Suikoden II drop shadow behind window
  Fill(new Rect(r.x+4,r.y+5,r.width,r.height),new Color(0,0,0,.42f));
  // Classic Suikoden II royal-blue vertical gradient backing
  GUI.color=Color.white;
  GUI.DrawTexture(r,windowGradTex!,ScaleMode.StretchToFill,true);
  // Subtle antique-gold inner trim line
  var goldTrim=new Color(.86f,.73f,.42f,.42f);
  Fill(new Rect(r.x+6,r.y+6,r.width-12,1),goldTrim);
  Fill(new Rect(r.x+6,r.y+r.height-7,r.width-12,1),goldTrim);
  var f=GuildVisualResources.Frame;
  if(f==null){Fill(new Rect(r.x,r.y,r.width,2),new Color(.9f,.86f,.70f));Fill(new Rect(r.x,r.y+r.height-2,r.width,2),new Color(.9f,.86f,.70f));Fill(new Rect(r.x,r.y,2,r.height),new Color(.9f,.86f,.70f));Fill(new Rect(r.x+r.width-2,r.y,2,r.height),new Color(.9f,.86f,.70f));return;}
  if(frameStyle==null){frameStyle=new GUIStyle();frameStyle.border=new RectOffset(27,27,30,24);}
  frameStyle.normal.background=f.texture;
  GUI.Label(r,"",frameStyle);
 }
 static readonly GuildMenuRow[] Services =
 {
  new("Hunt Contracts", "Choose a quarry"),
  new("Army Training", "Replay an original battle"),
  new("Guild Status", "Review your contract"),
  new("Party Strategy", "Review Star Bonds"),
  new("Hall of Echoes", "Unavailable in development"),
  new("Leave", "Return to the castle")
 };

 static readonly GuildMenuRow[] Drills =
 {
  new("War 1", "Mercenary Fort skirmish"),
  new("Fort Defense", "Hold the fort (War 2)"),
  new("Commander Rout", "War 1 · No turn limit"),
  new("War 3", "Battle of Muse"),
  new("War 6", "Kiba's Vanguard"),
  new("War 13", "Rockaxe Front"),
  new("Skirmish I: Crimson", "Custom War · +2 Commendations"),
  new("Skirmish II: Siege", "Custom War · +3 Commendations"),
  new("Skirmish III: Grand", "Custom War · +4 Commendations"),
  new("Skirmish IV: Blitz", "Custom War · +4 Commendations"),
  new("Skirmish V: Nightmare", "Custom War · +5 Commendations")
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
   var rows = Services;
   if (preview || !developer && HallFirstContact.Available)
   {
    rows = Services.ToArray();
    rows[4] = new GuildMenuRow("Hall of Echoes", "Choose a boss rematch");
   }
   var row = rows[focus];
   return GuildRebornScreen.Paginate(new("SERVICES", rows, focus, row.Title, "", row.Note,
       "", "", status, focus == Services.Length-1 ? "Leave" : "Open", back, preview), pageSize:6);
  }

  if(page==GuildPage.LucaParties)return HallLucaSession.View(focus);
  if(page==GuildPage.SpoilerWarning)return HallSpoilerMode.View(focus,Plugin.Options.UnlockAllBosses.Value,preview);

  if (page == GuildPage.Echoes)
  {
   if(preview)
    return HallPresentation.View(focus,index=>index!=0,_=>true,
        "Preview · Sample availability, not a campaign observation.",back,true);
   if(!developer&&HallFirstContact.Available)
    return HallPresentation.View(focus,HallAvailability.Unlocked,HallAvailability.Configured,
        HallAvailability.Reason(focus),back,false,Plugin.Options.UnlockAllBosses.Value,ArmyProgressionRuntime.HallMedalFor);
   return new("HALL OF ECHOES",new[]{new GuildMenuRow("Boss rematches","Unavailable")},0,
       "Hall of Echoes","Unavailable","Enable the Hall in settings to recall your story memories.",
       "","",status,"Back",back,false);
  }

  if(page==GuildPage.EchoDifficulty)
  {
   int echo=preview?drillIndex:CastleGuildRuntime.Menu.Echo;
   var echoView = HallPresentation.Difficulty(echo,focus,preview||HallAvailability.Unlocked(echo),
       preview||HallAvailability.Configured(echo),preview||HallNightmareRuntime.CanSelect(echo),
       preview||Plugin.Options.EnableNightmareStoneRewards.Value,preview,preview?"":CastleGuildRuntime.Status);
   if (!preview)
   {
    int[] party = StarBondRuntime.FieldParty();
    int bonds = StarBondRules.Catalogue.Count(b => StarBondRules.Includes(b, party));
    string ready = GuildPresentation.ReadinessStrip(StarBondRuntime.PartyAverageLevel(), bonds, ArmyProgressionRuntime.Current.MoraleBonusCharges);
    echoView = echoView with { Condition = echoView.Condition + "\n" + ready };
   }
   return echoView;
  }

  if (page == GuildPage.Strategy)
  {
   var bonds = StarBondRules.Catalogue;
   int[] party = preview ? new[]{7,8,30,26,33,32} : StarBondRuntime.FieldParty();
   bool enabled = preview || Plugin.Options.EnableStarBonds.Value &&
       DifficultyProvenance.Difficulty(out int difficulty, out _) && difficulty == 0;
   string Availability(StarBond bond) => !enabled ? "Unavailable" :
       StarBondRules.Includes(bond,party) ? "In your party" : "Other combination";
   var rows = bonds.Select(bond => new GuildMenuRow(bond.Title, Availability(bond))).ToArray();
   var selected = bonds[focus];
   return GuildRebornScreen.Paginate(new("PARTY STRATEGY", rows, focus, selected.Title, selected.Partners,
       selected.Trigger, selected.Effect, "Normal battles without scripted commands.\nAll partners must be able to act.",
       enabled ? status : "Star Bonds are disabled or unavailable at this difficulty.", "Back", back, preview));
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
   var defs = HuntRuntime.BrowseOrder;
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
   string readinessLine = "";
   if (!preview)
   {
    int[] party = StarBondRuntime.FieldParty();
    int activeBonds = StarBondRules.Catalogue.Count(b => StarBondRules.Includes(b, party));
    readinessLine = "\n" + GuildPresentation.ReadinessStrip(StarBondRuntime.PartyAverageLevel(), activeBonds, ArmyProgressionRuntime.Current.MoraleBonusCharges);
   }
   return GuildRebornScreen.Paginate(new("HUNT CONTRACTS", rows, focus, card.Name, $"{card.Destination}  ·  {card.Danger}",
       $"{card.Monsters}\n{card.Tactics}", $"Battle Potch + {bonus} bonus" +
           (itemLabel == null ? "" : $"\nPossible item  {itemLabel}"), reason + readinessLine, status, confirm, back, preview));
  }

  if (page == GuildPage.ArmyRoster)
  {
   var roster = ArmyProgressionRuntime.RosterSnapshot();
   if (roster.Length == 0)
   {
    return new("COMMANDER ROSTER", new[] { new GuildMenuRow("No veterans yet", ArmyProgressionRuntime.Enabled ? "Complete a drill" : "Disabled in settings") }, 0,
        "Army Progression", ArmyProgressionRuntime.Enabled ? "Ready" : "EnableArmyProgression = false",
        "Allied commanders earn persistent XP from qualifying Army Training victories and Hall Nightmare clears.",
        "Ranks: Recruit -> Lieutenant (100) -> Captain (250) -> General (500) -> Marshal (900).",
        "Stored in a content-addressed save-bound ledger. Native save files are untouched.",
        status, "Back", back, preview);
   }
   focus = Math.Clamp(focus, 0, roster.Length - 1);
   var rows = roster.Select(c =>
   {
    var rank = ArmyProgressionRules.RankForXp(c.Xp);
    return new GuildMenuRow(c.Name, $"{ArmyProgressionRules.RankTitle(rank)} · {c.Xp} XP · {ArmyProgressionRules.DoctrineTitle(c.Doctrine)}");
   }).ToArray();
   var sel = roster[focus];
   var selRank = ArmyProgressionRules.RankForXp(sel.Xp);
   int nextXp = ArmyProgressionRules.NextRankXp(selRank);
   int floorXp = ArmyProgressionRules.RankThresholds[(int)selRank];
   float progressFrac = selRank == ArmyRank.Marshal ? 1f : Math.Clamp((float)(sel.Xp - floorXp) / Math.Max(1, nextXp - floorXp), 0f, 1f);
   string nextNote = selRank == ArmyRank.Marshal ? "Maximum rank reached" : $"Next rank at {nextXp} XP";
   return GuildRebornScreen.Paginate(new("COMMANDER ROSTER", rows, focus, sel.Name,
       $"{ArmyProgressionRules.RankTitle(selRank)}  ·  {sel.Xp} XP ({nextNote})",
       $"Doctrine: {ArmyProgressionRules.DoctrineTitle(sel.Doctrine)}\n{ArmyProgressionRules.DoctrineDescription(sel.Doctrine)}",
       $"Drills won: {sel.DrillsWon}   Story wars: {sel.StoryWarsWon}   Nightmare clears: {sel.NightmareClears}",
       "Confirm cycles Doctrine (None / Vanguard / Bulwark / Tactician). Save at an inn or orb to bind progress.",
       status, "Cycle Doctrine", back, preview, GuildMenuNavigation.Breadcrumb(page), progressFrac));
  }

  if (page == GuildPage.Quartermaster)
  {
   var stock = Reforged2Rules.QuartermasterStock;
   focus = Math.Clamp(focus, 0, stock.Length - 1);
   int balance = ArmyProgressionRuntime.Current.Commendations;
   var qRows = stock.Select(o => new GuildMenuRow(o.Name,
       balance >= o.CostCommendations ? $"{o.CostCommendations} Commendation(s) · Ready" : $"{o.CostCommendations} Commendation(s) · Locked")).ToArray();
   var sel = stock[focus];
   bool canAfford = balance >= sel.CostCommendations;
   return GuildRebornScreen.Paginate(new("QUARTERMASTER", qRows, focus, sel.Name,
       $"{sel.Category}  ·  Balance: {balance} Commendation(s)",
       sel.Description,
       $"Cost: {sel.CostCommendations} Dunan Commendation(s)",
       "Earn Commendations from Hunts, Army Drills, Custom Skirmishes, and Hall of Echoes clears. Delivered straight to your bag.",
       status, canAfford ? "Exchange" : "Insufficient", back, preview));
  }

  if (page == GuildPage.Codex)
  {
   var ledger = ArmyProgressionRuntime.Current;
   var hRank = Reforged2Rules.HunterRankForClears(ledger.HuntsCompleted);
   int[] party = preview ? new[]{7,8,30,26,33,32} : StarBondRuntime.FieldParty();
   int activeBonds = StarBondRules.Catalogue.Count(b => StarBondRules.Includes(b, party));
   var codexRows = new[]
   {
    new GuildMenuRow("Army Campaign", $"{ledger.TotalDrillsWon} drills · {ledger.Commendations} Commendations"),
    new GuildMenuRow("Allied Stance", Reforged2Rules.StanceTitle(ArmyProgressionRuntime.ActiveStance)),
    new GuildMenuRow("Hunt Guild", $"{Reforged2Rules.HunterRankTitle(hRank)} · {ArmyProgressionRules.HuntModifierTitle(ArmyProgressionRuntime.ActiveHuntModifier)}"),
    new GuildMenuRow("Active Star Bonds", $"{activeBonds} active in current party")
   };
   focus = Math.Clamp(focus, 0, codexRows.Length - 1);
   var (cTitle, cArea, cDesc, cReward, cCond) = focus switch
   {
    0 => ("Army Campaign Record", $"Drills: {ledger.TotalDrillsWon} ({ledger.TotalVeteranDrillsWon} Veteran) · Commendations: {ledger.Commendations}",
          $"Tracked commanders: {ledger.Commanders.Count}. Active Directive: {ArmyProgressionRules.DirectiveTitle(ArmyProgressionRuntime.ActiveDirective)}.",
          ArmyProgressionRuntime.Enabled ? "Army Progression ON · Spend Commendations at Quartermaster" : "Army Progression OFF (EnableArmyProgression)",
          "Progress binds automatically to your save fingerprint whenever you save."),
    1 => ($"Allied War Stance · {Reforged2Rules.StanceTitle(ArmyProgressionRuntime.ActiveStance)}", $"Morale Charges: {ledger.MoraleBonusCharges}  ·  Hall Nightmare: {ledger.HallNightmareClears}",
          Reforged2Rules.StanceDescription(ArmyProgressionRuntime.ActiveStance) + "\nVeteran drills arm +1 morale rune charge; Hall Nightmare clears grant +35 War XP.",
          "Confirm cycles Stance (Balanced / All-Out Assault / Iron Phalanx / Swift March).",
          "Applied cleanly to allied war units on disposable clones."),
    2 => ($"Hunt Guild · {Reforged2Rules.HunterRankTitle(hRank)}", $"Cleared: {ledger.HuntsCompleted}  ·  Potch Bonus: x{Reforged2Rules.HunterPotchBonusMultiplier(hRank):0.00}",
          ArmyProgressionRules.HuntModifierDescription(ArmyProgressionRuntime.ActiveHuntModifier) + "\n" + CastleGuildRuntime.QuestSummary,
          ArmyProgressionRuntime.HuntModifiersEnabled ? "Confirm cycles Hunt Risk Modifier (+35% bonus Potch)." : "Enable EnableHuntModifiers in settings to toggle Hunt modifiers.",
          "Reporting completed hunts grants +1 Dunan Commendation and advances Hunter Rank."),
    _ => ("Party Star Bonds", $"{activeBonds} of {StarBondRules.Catalogue.Length} active",
          "Check Party Strategy from the main Guild menu for exact triggers and bonuses.",
          "Normal battles without scripted commands.",
          "All bond partners must be alive and able to act.")
   };
   return new("TACTICIAN'S CODEX", codexRows, focus, cTitle, cArea, cDesc, cReward, cCond, status,
       focus == 1 ? "Cycle Stance" : focus == 2 && ArmyProgressionRuntime.HuntModifiersEnabled ? "Cycle Modifier" : "Back", back, preview);
  }

  if (page == GuildPage.Training && focus >= GuildMenuNavigation.TrainingDrillCount)
  {
   var trainingAllRows = Drills.Select((row, i) =>
       new GuildMenuRow(row.Title, preview || TrainingWar.Unavailable((TrainingWar.Drill)i).Length == 0 ? row.Note : "Unavailable"))
       .Concat(new[]
       {
        new GuildMenuRow("Commander Roster", ArmyProgressionRuntime.Enabled ? $"{ArmyProgressionRuntime.Current.Commanders.Count} tracked" : "Progression OFF"),
        new GuildMenuRow("Drill Directive", ArmyProgressionRuntime.DirectivesEnabled ? ArmyProgressionRules.DirectiveTitle(ArmyProgressionRuntime.ActiveDirective) : "Directives OFF"),
        new GuildMenuRow("Quartermaster Shop", $"{ArmyProgressionRuntime.Current.Commendations} Commendation(s)"),
        new GuildMenuRow("Tactician's Codex", "Ranks, Hunter & Synergy")
       }).ToArray();
   var (mTitle, mArea, mDesc, mReward, mCond, mConfirm) = focus switch
   {
    GuildMenuNavigation.RosterRow => ("Commander Roster & Doctrines", ArmyProgressionRuntime.Enabled ? "Army Progression ON" : "Optional opt-in",
        "Inspect persistent commander ranks, XP, and assign Vanguard, Bulwark, or Tactician doctrines.",
        "Ranks unlock stat bonuses and safe native SP_* abilities.",
        "Bound to save fingerprints in army-ledger.json; native saves stay vanilla.", "Open Roster"),
    GuildMenuNavigation.DirectiveRow => ("Drill Directive: " + ArmyProgressionRules.DirectiveTitle(ArmyProgressionRuntime.ActiveDirective),
        ArmyProgressionRuntime.DirectivesEnabled ? "Active for next drill" : "EnableDrillDirectives = false",
        ArmyProgressionRules.DirectiveDescription(ArmyProgressionRuntime.ActiveDirective),
        $"XP x{ArmyProgressionRules.DirectiveXpMultiplier(ArmyProgressionRuntime.ActiveDirective):0.00}   ·   Potch x{ArmyProgressionRules.DirectivePotchMultiplier(ArmyProgressionRuntime.ActiveDirective):0.00}",
        "Confirm cycles: Standard -> Iron Wall -> No Field Medicine -> Forced March.",
        ArmyProgressionRuntime.DirectivesEnabled ? "Cycle Directive" : "Disabled"),
    GuildMenuNavigation.QuartermasterRow => ("Castle Quartermaster", $"Balance: {ArmyProgressionRuntime.Current.Commendations} Dunan Commendation(s)",
        "Exchange Dunan Commendations earned from Hunts, Custom Skirmishes, and Hall Medals for rare Runes, Growth Stones, and supplies.",
        "10 audited native items delivered straight to your bag.",
        "Requires an open bag slot. Save at an inn or orb to persist your Commendation ledger.", "Open Shop"),
    _ => ("Tactician's Codex", "Dunan War & Guild Overview",
        "View Army Progression records, Hunter Rank, Cross-Theater Synergy morale charges, and active Star Bonds.",
        "Confirm on Hunt Guild row to cycle Hunt Risk Modifiers.", "No campaign flags or saves are modified.", "Open Codex")
   };
   return GuildRebornScreen.Paginate(new("ARMY TRAINING", trainingAllRows, focus, mTitle, mArea, mDesc, mReward, mCond, status, mConfirm, back, preview));
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
   TrainingWar.Drill.War6 => "Clash against Kiba's vanguard on the audited War 6 battlefield under full training save protection.",
   TrainingWar.Drill.War13 => "Hold the line on the audited War 13 Rockaxe Front against coordinated Highland forces.",
   TrainingWar.Drill.SkirmishCrimson => Reforged2Rules.Skirmishes[0].Description,
   TrainingWar.Drill.SkirmishIronSiege => Reforged2Rules.Skirmishes[1].Description,
   TrainingWar.Drill.SkirmishGrandArmy => Reforged2Rules.Skirmishes[2].Description,
   TrainingWar.Drill.SkirmishBlitz => Reforged2Rules.Skirmishes[3].Description,
   TrainingWar.Drill.SkirmishNightmare => Reforged2Rules.Skirmishes[4].Description,
   _ => "Practice on the original War 1 battlefield. The story timer can end the drill."
  };
  var trainingRows = page == GuildPage.Training ? Drills.Select((row,i) =>
      new GuildMenuRow(row.Title, preview || TrainingWar.Unavailable((TrainingWar.Drill)i).Length == 0 ?
          row.Note : "Unavailable"))
      .Concat(new[]
      {
       new GuildMenuRow("Commander Roster", ArmyProgressionRuntime.Enabled ? $"{ArmyProgressionRuntime.Current.Commanders.Count} tracked" : "Progression OFF"),
       new GuildMenuRow("Drill Directive", ArmyProgressionRuntime.DirectivesEnabled ? ArmyProgressionRules.DirectiveTitle(ArmyProgressionRuntime.ActiveDirective) : "Directives OFF"),
       new GuildMenuRow("Quartermaster Shop", $"{ArmyProgressionRuntime.Current.Commendations} Commendation(s)"),
       new GuildMenuRow("Tactician's Codex", "Ranks, Hunter & Synergy")
      }).ToArray() :
      new[] { new GuildMenuRow("Standard", "Current scenario rules"),
          new GuildMenuRow("Veteran", veteranAllowed ? "Coordinated enemy tactics" : "Unavailable") };
  string directiveNote = ArmyProgressionRuntime.DirectivesEnabled && ArmyProgressionRuntime.ActiveDirective != DrillDirective.None
      ? $"  ·  Directive: {ArmyProgressionRules.DirectiveTitle(ArmyProgressionRuntime.ActiveDirective)}" : "";
  string reward = Plugin.Options.EnableTrainingRewards.Value ?
      (page == GuildPage.Training && veteranAllowed ?
          $"{TrainingResultRules.Potch(false):N0} Standard  /  {TrainingResultRules.Potch(true):N0} Veteran Potch{directiveNote}" :
          TrainingResultRules.Potch(veteran).ToString("N0") + " Potch" + directiveNote) : "Training rewards disabled";
  string condition = unavailable.Length != 0 ? unavailable : veteran && !veteranAllowed ?
      "Veteran requires the native tactics options and a verified battlefield." :
      "Rewards require routing the opposing commander. Timed endings and defeat pay no Potch.";
  return GuildRebornScreen.Paginate(new(page == GuildPage.Training ? "ARMY TRAINING" : "CHOOSE DIFFICULTY", trainingRows, focus,
      Drills[(int)drill].Title, Drills[(int)drill].Note, description, reward, condition, status,
      unavailable.Length != 0 || veteran && !veteranAllowed ? "Unavailable" :
          page == GuildPage.Training ? "Choose difficulty" : "Start drill", back, preview));
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
  style.normal.textColor=new Color(.015f,.03f,.09f,.88f);
  GUI.Label(new Rect(rect.x+1.5f,rect.y+1.5f,rect.width,rect.height),content,style);
  style.normal.textColor=Tone(text.Tone);
  GUI.Label(rect,content,style);
 }

 public void OnGUI()
 {
  bool previewSmoke=HuntGuildRuntime.RenderSmoke;
  if(previewSmoke&&(nextCaptureAt<0||smokeScreen>=SmokeScreens.Length||Event.current?.type!=EventType.Repaint))return;
  var offer=previewSmoke&&SmokeScreens[smokeScreen].FieldOffer
      ? HuntRuntime.BrowseOrder.FirstOrDefault() :fieldOffer;
  bool prompt=!previewSmoke&&!HuntGuildRuntime.Visible&&CastleGuildRuntime.Idle&&CastleGuildRuntime.Near;
  bool feedback=!previewSmoke&&!HuntGuildRuntime.Visible&&CastleGuildRuntime.FeedbackVisible;
  bool battleHud=!previewSmoke&&Combat.State is {Ready:true}&&(Plugin.Options?.EnableBattleTacticalHud.Value==true||Reforged2Runtime.BannerVisible||Reforged2Runtime.ScannerVisible);
  bool helpCard=!previewSmoke&&Reforged2Runtime.HelpCardVisible;
  if(smokeExitRequested||faulted||(!previewSmoke&&!helpCard&&!battleHud&&!prompt&&!feedback&&offer==null&&(!HuntGuildRuntime.Visible||(!HuntGuildRuntime.Context&&!HuntGuildRuntime.Preview))))return;
  try
  {
   var matrix=GUI.matrix;var color=GUI.color;
   try
   {
    GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;
    if(helpCard)
    {
     var led=ArmyProgressionRuntime.Current;
     var hr=Reforged2Rules.HunterRankForClears(led.HuntsCompleted);
     float hw=460,hh=252;
     var hbox=new Rect(Screen.width-hw-18,66,hw,hh);
     Window(hbox);
     Label(new(new(18,12,hw-36,26),"REFORGED 2.0 QUICK GUIDE   (F3 to hide)",17,"gold"),hbox.x,hbox.y,1);
     Label(new(new(18,44,hw-36,112),
         "F4   Toggle Battle Tactical Scanner\nF6   Accept Local Field Hunt\n←/→  Fast Page-Flip in Castle Guild\nCorridor NPCs: Quartermaster · Sergeant · Attendant · Archivist",16),hbox.x,hbox.y,1);
     Label(new(new(18,162,hw-36,76),
         $"Commendations: {led.Commendations}   ·   {Reforged2Rules.HunterRankTitle(hr)}\nStance: {Reforged2Rules.StanceTitle(ArmyProgressionRuntime.ActiveStance)}   ·   Directive: {ArmyProgressionRules.DirectiveTitle(ArmyProgressionRuntime.ActiveDirective)}\nMorale Charges: {led.MoraleBonusCharges}   ·   Combos & Surges Active",15,"success"),hbox.x,hbox.y,1);
     if(!battleHud&&!prompt&&!feedback&&offer==null&&!HuntGuildRuntime.Visible)return;
    }
    if(battleHud)
    {
     if(Plugin.Options?.EnableBattleTacticalHud.Value==true)
     {
      int[] party=StarBondRuntime.FieldParty();
      var active=StarBondRules.Catalogue.Where(b=>StarBondRules.Includes(b,party)).Select(b=>b.Title).ToArray();
      int morale=ArmyProgressionRuntime.Current.MoraleBonusCharges;
      string apexTag=Reforged2Runtime.ApexBattleActive?"⚠ APEX · ":"";
      string text=apexTag+(active.Length>0?"★ "+string.Join(" · ",active.Take(2)):"F4 Scanner · F3 Guide")+(morale>0?$"   Morale +{morale}":"");
      float w=Math.Min(520,Math.Max(240,text.Length*10+36));
      var pill=new Rect(Screen.width-w-18,16,w,40);
      Window(pill);
      Label(new(new(14,9,w-28,24),text,16,"gold"),pill.x,pill.y,1);
     }
     if(Reforged2Runtime.BannerVisible)
     {
      float bw=Math.Min(680,Math.Max(300,Reforged2Runtime.BannerText.Length*12+48));
      var banner=new Rect((Screen.width-bw)/2,18,bw,46);
      Window(banner);
      Label(new(new(18,11,bw-36,26),Reforged2Runtime.BannerText,19,"gold"),banner.x,banner.y,1);
     }
     if(Reforged2Runtime.ScannerVisible&&Framework.Singleton<BATTLE_WORK>.instance?.pm_data is {} pmUnits)
     {
      var enemies=pmUnits.Where(u=>u!=null&&!u.IsPlayer&&u.hp>0&&!u.IsDead).Take(6).ToArray();
      if(enemies.Length>0)
      {
       float sw=500,sh=52+enemies.Length*56;
       var scanBox=new Rect(18,16,sw,sh);
       Window(scanBox);
       string scanHeader=Reforged2Runtime.ApexBattleActive?"TACTICAL SCANNER  [APEX PACK]   (F4)":"TACTICAL SCANNER   (F4 to hide)";
       Label(new(new(18,10,sw-36,28),scanHeader,18,"gold"),scanBox.x,scanBox.y,1);
       for(int i=0;i<enemies.Length;i++)
       {
        var e=enemies[i]!;
        string displayName=Reforged2Runtime.ResolveEnemyDisplayName(e,out string role);
        int lv=e.monster_data?.level??0;
        string weak=e.calc_para is {Length:>3}?Reforged2Rules.TacticalWeaknessHint(e.calc_para[2],e.calc_para[3]):"Balanced";
        string statusTag=((e.status&b_game_h.STATUS_SLEEP)!=0?" [SLP]":"")+((e.status&b_game_h.STATUS_POISON)!=0?" [PSN]":"");
        float hpFrac=e.max_hp>0?Math.Clamp((float)e.hp/e.max_hp,0f,1f):0f;
        float ry=44+i*56;
        Label(new(new(18,ry,sw-170,24),$"{displayName}{(lv>0?$" Lv{lv}":"")}{statusTag}",17),scanBox.x,scanBox.y,1);
        Label(new(new(sw-152,ry+1,134,22),$"{e.hp} / {e.max_hp}",16,"gold"),scanBox.x,scanBox.y,1);
        Label(new(new(18,ry+21,sw-36,18),$"{role}  ·  {weak}",14,"muted"),scanBox.x,scanBox.y,1);
        Fill(new Rect(scanBox.x+18,scanBox.y+ry+40,sw-36,9),new Color(.04f,.07f,.16f,.95f));
        Fill(new Rect(scanBox.x+20,scanBox.y+ry+42,(sw-40)*hpFrac,5),hpFrac<=0.25f?new Color(.95f,.35f,.30f,.98f):hpFrac<=0.50f?new Color(.96f,.76f,.32f,.98f):new Color(.38f,.90f,.58f,.98f));
       }
      }
     }
     return;
    }
    if(prompt||feedback||offer!=null)
    {
     float width=Math.Min(880,Screen.width-48);
     var box=new Rect((Screen.width-width)/2,Screen.height-128,width,100);
     Window(box);
     if(offer!=null)
      Label(new(new(26,20,width-52,60),$"Local contract: {offer.Name}\nF6 or hold L1 and press R1 to accept",20),box.x,box.y,1);
     else if(feedback)
      Label(new(new(26,20,width-52,60),CastleGuildRuntime.FeedbackText,20),box.x,box.y,1);
     else
     {
      var (header,greeting)=GuildPresentation.NpcBanner(CastleGuildRuntime.Quest.State,CastleGuildRuntime.Contract,
          ArmyProgressionRuntime.Current.MoraleBonusCharges,ArmyProgressionRuntime.Current.Commanders.Count,
          ArmyProgressionRuntime.Enabled,CastleGuildRuntime.ActiveNpc);
      // Classic Suikoden II floating Speaker Nameplate perched on top-left of dialogue box
      float plateW=Math.Min(420,Math.Max(220,header.Length*11+40));
      var plate=new Rect(box.x+18,box.y-30,plateW,36);
      Window(plate);
      Label(new(new(16,6,plateW-32,24),header,18,"gold"),plate.x,plate.y,1);
      Label(new(new(28,22,width-56,48),greeting,20),box.x,box.y,1);
      float pulse=previewSmoke?1f:0.65f+0.35f*Mathf.Sin(Time.unscaledTime*5f);
      GUI.color=new Color(1,1,1,pulse);
      Label(new(new(width-210,66,184,24),"▼  Confirm to speak",16,"gold"),box.x,box.y,1);
      GUI.color=Color.white;
     }
     if(previewSmoke)CapturePreview();
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
    if(string.IsNullOrEmpty(view.Breadcrumb))view=view with{Breadcrumb=GuildMenuNavigation.Breadcrumb(page)};
    float scale=GuildRebornScreen.Scale(Screen.width,Screen.height);
    float x=(Screen.width-GuildRebornScreen.Width*scale)/2,y=(Screen.height-GuildRebornScreen.Height*scale)/2;
    // Soft full-screen dim vignette so the Castle Guild windows pop like native Suikoden II menus
    if(!previewSmoke)Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.01f,.02f,.06f,.32f));
    foreach(var box in new[]{GuildRebornScreen.Heading,GuildRebornScreen.List,GuildRebornScreen.Detail,GuildRebornScreen.Footer})
     Window(ToRect(box,x,y,scale));

    // Layered Suikoden II Detail Panel accents: Header band, Gold divider, Spoils plaque
    EnsureGradients();
    Fill(ToRect(new(386,104,680,100),x,y,scale),new Color(.02f,.05f,.14f,.45f));
    Fill(ToRect(new(396,204,660,2),x,y,scale),new Color(.88f,.75f,.42f,.68f));
    if(view.Reward.Length>0)
    {
     var plaqueRect=ToRect(new(392,324,668,74),x,y,scale);
     GUI.DrawTexture(plaqueRect,plaqueGradTex!,ScaleMode.StretchToFill,true);
     Fill(ToRect(new(392,324,4,74),x,y,scale),new Color(.95f,.80f,.42f,.90f));
    }
    if(view.Condition.Length>0)
     Fill(ToRect(new(392,404,668,72),x,y,scale),new Color(.02f,.04f,.12f,.42f));

    // Glowing gradient highlight + gold left accent bar on selected list row
    var row=GuildRebornScreen.Row(view.Focus,view.Rows.Length);
    var rowRect=ToRect(row,x,y,scale);
    GUI.DrawTexture(rowRect,rowGradTex!,ScaleMode.StretchToFill,true);
    Fill(ToRect(new(row.X,row.Y,4,row.H),x,y,scale),new Color(.96f,.82f,.44f,.95f));
    Fill(ToRect(new(row.X+7,row.Y+row.H-1,row.W-14,1),x,y,scale),new Color(.90f,.76f,.44f,.72f));
    float bob=previewSmoke?0f:Mathf.Sin(Time.unscaledTime*7f)*3f;
    var cursor=GuildVisualResources.Cursor;
    if(cursor!=null)GUI.DrawTexture(ToRect(new(row.X+7+bob,row.Y+12,17,19),x,y,scale),cursor.texture,ScaleMode.StretchToFill,true);
    else Label(new(new(row.X+7+bob,row.Y+8,20,24),">",20,"gold"),x,y,scale);
    if(view.ProgressFraction is float frac)
    {
     var barTrack=new GuildBox(401,310,634,12);
     Fill(ToRect(barTrack,x,y,scale),new Color(.04f,.07f,.16f,.95f));
     Fill(ToRect(new(barTrack.X+2,barTrack.Y+2,(barTrack.W-4)*Math.Clamp(frac,0f,1f),barTrack.H-4),x,y,scale),new Color(.98f,.82f,.38f,.98f));
    }
    foreach(var line in GuildRebornScreen.Text(view))Label(line,x,y,scale);
    HuntGuildRuntime.Drawn();
    if(previewSmoke)CapturePreview();
   }
   finally{GUI.matrix=matrix;GUI.color=color;}
  }
  catch(Exception e){faulted=true;HuntGuildRuntime.RenderFailed(e);}
 }
 void CapturePreview()
 {
  smokePaints++;
  if(smokeCapturePending||timer.ElapsedMilliseconds<nextCaptureAt)return;
  var file=SmokeScreens[smokeScreen].File;
  smokeCapturePending=true;smokeCaptureFrame=Time.frameCount;
  ScreenCapture.CaptureScreenshot(Path.Combine(HuntGuildRuntime.CaptureDirectory!,file));
  Plugin.Logger.LogInfo($"Guild preview painted {file} at {Screen.width}x{Screen.height}.");
  nextCaptureAt=timer.ElapsedMilliseconds+700;
 }
}
