using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NightmareRebalance;
// Existing explicit-rectangle renderer used by the native-talk castle Guild.
public sealed class HuntGuildOverlay:MonoBehaviour
{
 readonly Stopwatch timer=Stopwatch.StartNew();
 Font? fallbackFont;readonly Dictionary<int,GUIStyle> styles=new();bool faulted;readonly StableHotkeyEdge request=new(),scannerKey=new(),helpKey=new(),bestiaryKey=new(),cleanHudKey=new();
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
   short f2=GetAsyncKeyState(0x71);
   if(cleanHudKey.Poll(focusedApp,(f2&0x8000)!=0,(f2&1)!=0,timer.ElapsedMilliseconds))
    Reforged2Runtime.ToggleCleanHud();
   short f3=GetAsyncKeyState(0x72);
   if(helpKey.Poll(focusedApp,(f3&0x8000)!=0,(f3&1)!=0,timer.ElapsedMilliseconds))
    Reforged2Runtime.ToggleHelpCard();
   short f5=GetAsyncKeyState(0x74);
   if(bestiaryKey.Poll(focusedApp,(f5&0x8000)!=0,(f5&1)!=0,timer.ElapsedMilliseconds))
    Reforged2Runtime.ToggleBestiary();
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
  new("Skirmish V: Nightmare", "Custom War · +5 Commendations"),
  new("Skirmish VI: Iron", "Custom War · +4 Commendations"),
  new("Skirmish VII: Bastion", "Custom War · +5 Commendations"),
  new("Skirmish VIII: Apex", "Custom War · +6 Commendations"),
  new("Skirmish IX: Blades", "Custom War · +6 Commendations"),
  new("Skirmish X: Finale", "Custom War · +8 Commendations")
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
       DifficultyProvenance.AllowedForFeature(out _, out _);
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
   bool normal = preview || DifficultyProvenance.AllowedForFeature(out _, out _);
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
   var upgrades = Reforged2Rules.CastleUpgrades;
   var stock = Reforged2Rules.QuartermasterStock;
   int total = upgrades.Length + stock.Length;
   focus = Math.Clamp(focus, 0, total - 1);
   int balance = ArmyProgressionRuntime.Current.Commendations;
   var qRows = upgrades.Select(u =>
   {
    bool owned = ArmyProgressionRuntime.HasUpgrade(u.Id);
    return new GuildMenuRow(u.Name, owned ? "OWNED · Active" : balance >= u.CostCommendations ? $"{u.CostCommendations} Comm. · Unlock" : $"{u.CostCommendations} Comm. · Locked");
   }).Concat(stock.Select(o => new GuildMenuRow(o.Name,
       balance >= o.CostCommendations ? $"{o.CostCommendations} Comm. · Ready" : $"{o.CostCommendations} Comm. · Locked"))).ToArray();
   if (focus < upgrades.Length)
   {
    var u = upgrades[focus];
    bool owned = ArmyProgressionRuntime.HasUpgrade(u.Id);
    bool canAfford = !owned && balance >= u.CostCommendations;
    return GuildRebornScreen.Paginate(new("QUARTERMASTER", qRows, focus, u.Name,
        $"Permanent Castle Upgrade  ·  Balance: {balance} Commendation(s)",
        u.Description,
        owned ? "Already Unlocked (Active across all battles)" : $"Cost: {u.CostCommendations} Dunan Commendation(s)",
        "Stored permanently in your save-bound ledger. Save at an inn or orb after unlocking.",
        status, owned ? "Owned" : canAfford ? "Unlock Upgrade" : "Insufficient", back, preview));
   }
   var sel = stock[focus - upgrades.Length];
   bool canAffordItem = balance >= sel.CostCommendations;
   return GuildRebornScreen.Paginate(new("QUARTERMASTER", qRows, focus, sel.Name,
       $"{sel.Category}  ·  Balance: {balance} Commendation(s)",
       sel.Description,
       $"Cost: {sel.CostCommendations} Dunan Commendation(s)",
       "Earn Commendations from Hunts, Skirmishes, Apex Packs, Bestiary Milestones, and Field Bounties.",
       status, canAffordItem ? "Exchange" : "Insufficient", back, preview));
  }

  if (page == GuildPage.PartyTalents)
  {
   var list = ArmyProgressionRuntime.PartyMasterySnapshot();
   if (list.Length == 0)
   {
    return new("PARTY TALENTS", new[] { new GuildMenuRow("No heroes tracked yet", "Win a battle") }, 0,
        "Party Character Talents (108 Stars)", "Mastery Rank I–V",
        "Every party member earns Mastery XP from battles and can specialize as a Berserker, Arcanist, Bulwark, or Tactician.",
        "Rank III (250 XP) upgrades every Talent's power tier!",
        "Stored permanently in your save-bound ledger. Native saves stay vanilla.",
        status, "Back", back, preview);
   }
   focus = Math.Clamp(focus, 0, list.Length - 1);
   var rows = list.Select(m => new GuildMenuRow(m.Name, $"Rank {Reforged5Rules.MasteryRankForXp(m.Xp)} · {Reforged5Rules.TalentTitle(m.Talent)}")).ToArray();
   var sel = list[focus];
   int rank = Reforged5Rules.MasteryRankForXp(sel.Xp);
   int floorXp = Reforged5Rules.MasteryThresholds[rank - 1];
   int nextXp = rank >= 5 ? Reforged5Rules.MasteryThresholds[4] : Reforged5Rules.MasteryThresholds[rank];
   float pFrac = rank >= 5 ? 1f : Math.Clamp((float)(sel.Xp - floorXp) / Math.Max(1, nextXp - floorXp), 0f, 1f);
   return GuildRebornScreen.Paginate(new("PARTY TALENTS", rows, focus, sel.Name,
       $"Mastery Rank {rank}  ·  {sel.Xp} XP  ·  {Reforged5Rules.TalentTitle(sel.Talent)}",
       Reforged5Rules.TalentDescription(sel.Talent),
       rank >= 3 ? "Rank III+ Mastery Bonus ACTIVE!" : $"Reach 250 XP (Rank III) to empower {Reforged5Rules.TalentTitle(sel.Talent)}.",
       "Confirm cycles Talent: Unassigned -> Berserker -> Arcanist -> Bulwark -> Tactician.",
       status, "Cycle Talent", back, preview, GuildMenuNavigation.Breadcrumb(page), pFrac));
  }

  if (page == GuildPage.WarSquads)
  {
   var roster = ArmyProgressionRuntime.RosterSnapshot();
   if (roster.Length == 0)
   {
    return new("WAR SQUADS", new[] { new GuildMenuRow("No commanders enlisted", "Win a drill first") }, 0,
        "Custom War Squad Branch Builder", "Applies to Custom Skirmishes & Drills",
        "Customize any enlisted commander's unit branch (Infantry / Archer / Magicians) on disposable clones.",
        "War Synergy Pairs: Flik+Viktor · Shu+Apple · Luc+Mazus · Humphrey+Georg",
        "Complete a qualifying Army Training drill to enlist commanders.",
        status, "Back", back, preview);
   }
   focus = Math.Clamp(focus, 0, roster.Length - 1);
   var rows = roster.Select(c => new GuildMenuRow(c.Name, Reforged5Rules.BranchOverrideTitle(c.BranchOverride))).ToArray();
   var sel = roster[focus];
   var syn = ArmyProgressionRules.WarSynergies.FirstOrDefault(s => s.FirstNameId == sel.NameId || s.SecondNameId == sel.NameId);
   string synLine = syn != null ? $"War Synergy Pair: {syn.Title} (+{syn.AtkBonus} ATK, +{syn.DefBonus} DEF when grouped together)" : "No named War Synergy pair for this commander.";
   return GuildRebornScreen.Paginate(new("WAR SQUADS", rows, focus, sel.Name,
       $"{Reforged5Rules.BranchOverrideTitle(sel.BranchOverride)}  ·  {ArmyProgressionRules.DoctrineTitle(sel.Doctrine)}",
       synLine,
       "Branch overrides reshape your squad's tactical role during Custom Skirmishes without touching native story data.",
       "Confirm cycles Branch: Default -> Infantry -> Archer -> Magicians.",
       status, "Cycle Branch", back, preview, GuildMenuNavigation.Breadcrumb(page)));
  }

  if (page == GuildPage.TowerOfDunan)
  {
   var led = ArmyProgressionRuntime.Current;
   if (TowerOfDunanRuntime.PendingRelicDraft)
   {
    var choices = TowerOfDunanRuntime.DraftChoices;
    focus = Math.Clamp(focus, 0, choices.Length - 1);
    var rows = choices.Select(r => new GuildMenuRow(r.Name, Reforged5Rules.IsWardenFloor(TowerOfDunanRuntime.FloorsCleared) ? "★ LEGENDARY Relic" : "Roguelite Relic")).ToArray();
    var sel = choices[focus];
    return new("TOWER RELIC DRAFT", rows, focus, sel.Name,
        $"Floor {TowerOfDunanRuntime.FloorsCleared} Cleared  ·  Active Relics: {TowerOfDunanRuntime.ActiveRelics.Count}",
        sel.Summary,
        $"Next Floor: Floor {TowerOfDunanRuntime.FloorsCleared + 1} (HP x{Reforged5Rules.TowerFloorHpMultiplier(TowerOfDunanRuntime.FloorsCleared + 1):0.00})",
        "Choose 1 of 3 Relics for this Tower run. Floor 5/10/15 Warden clears grant Legendary Relics!",
        status, "Draft Relic", back, preview);
   }
   bool active = TowerOfDunanRuntime.RunActive;
   int nextFloor = active ? TowerOfDunanRuntime.FloorsCleared + 1 : 1;
   int cashPotch = Reforged5Rules.TowerCashOutPotch(TowerOfDunanRuntime.FloorsCleared, TowerOfDunanRuntime.HasOwnedRelic("golden-chalice"), TowerOfDunanRuntime.BloodOathActive);
   int cashComm = Reforged5Rules.TowerCashOutCommendations(TowerOfDunanRuntime.FloorsCleared, TowerOfDunanRuntime.HasOwnedRelic("golden-chalice"), TowerOfDunanRuntime.BloodOathActive);
   var tRows = active
       ? new[]
       {
        new GuildMenuRow($"Ascend to Floor {nextFloor}", $"HP x{Reforged5Rules.TowerFloorHpMultiplier(nextFloor):0.00} · DMG x{Reforged5Rules.TowerFloorDamageMultiplier(nextFloor):0.00}"),
        new GuildMenuRow("Cash Out & Claim Spoils", $"{cashPotch:N0} Potch · +{cashComm} Comm.")
       }
       : new[]
       {
        new GuildMenuRow("Begin Tower Ascent", $"Floor 1 · Record: Floor {led.HighestTowerFloor}"),
        new GuildMenuRow("Tower Pact: " + (TowerOfDunanRuntime.BloodOathActive ? "Blood Oath ON" : "Standard"), TowerOfDunanRuntime.BloodOathActive ? "+15% Enemy DMG · 1.5x Potch & +2 Comm." : "Confirm to toggle Blood Oath")
       };
   focus = Math.Clamp(focus, 0, tRows.Length - 1);
   var allRelics = Reforged5Rules.TowerRelics.Concat(Reforged5Rules.LegendaryRelics);
   string relicsText = TowerOfDunanRuntime.ActiveRelics.Count > 0
       ? "Active Relics: " + string.Join(", ", allRelics.Where(r => TowerOfDunanRuntime.ActiveRelics.Contains(r.Id)).Select(r => r.Name))
       : "No Relics drafted yet. Clear floors to draft Relics; every 5th floor is a Warden Boss dropping a Legendary Relic!";
   return new("TOWER OF DUNAN", tRows, focus,
       active && focus == 1 ? "Cash Out Tower Run" : !active && focus == 1 ? "Tower Risk Pact (Blood Oath)" : $"Tower of Dunan · Floor {nextFloor}",
       $"Highest Floor Record: {led.HighestTowerFloor}  ·  Pact: {(TowerOfDunanRuntime.BloodOathActive ? "Blood Oath" : "Standard")}",
       relicsText,
       active ? $"Current Spoils: {cashPotch:N0} Potch · +{cashComm} Commendation(s)"
              : "Endless Roguelite Gauntlet · 12 Relics + 3 Legendary Warden Relics.",
       "Requires Hall of Echoes enabled. Losing a floor ends the run with no Cash-Out!",
       status, active && focus == 1 ? "Cash Out" : !active && focus == 1 ? "Toggle Pact" : "Ascend", back, preview);
  }

  if (page == GuildPage.Codex)
  {
   var ledger = ArmyProgressionRuntime.Current;
   var hRank = Reforged2Rules.HunterRankForClears(ledger.HuntsCompleted);
   var bounty = Reforged2Rules.ActiveBounty(ledger.ActiveBountyIndex);
   int[] party = preview ? new[]{7,8,30,26,33,32} : StarBondRuntime.FieldParty();
   int activeBonds = StarBondRules.Catalogue.Count(b => StarBondRules.Includes(b, party));
   var codexRows = new[]
   {
    new GuildMenuRow("Army Campaign", $"{ledger.TotalDrillsWon} drills · {ledger.Commendations} Commendations"),
    new GuildMenuRow("Allied Stance", Reforged2Rules.StanceTitle(ArmyProgressionRuntime.ActiveStance)),
    new GuildMenuRow("Hunt Guild", $"{Reforged2Rules.HunterRankTitle(hRank)} · {ArmyProgressionRules.HuntModifierTitle(ArmyProgressionRuntime.ActiveHuntModifier)}"),
    new GuildMenuRow("Active Star Bonds", $"{activeBonds} active · Bestiary: {ledger.Bestiary?.Count ?? 0}"),
    new GuildMenuRow("War Mutator", Reforged2Rules.MutatorTitle(ArmyProgressionRuntime.ActiveMutator)),
    new GuildMenuRow("Dunan Honors", $"{ledger.UnlockedHonors?.Count ?? 0} / {Reforged2Rules.Honors.Length} unlocked"),
    new GuildMenuRow("Party Talents", $"{ledger.PartyMastery?.Count ?? 0} heroes · Build Trees"),
    new GuildMenuRow("Tower of Dunan", TowerOfDunanRuntime.PendingRelicDraft ? "★ Relic Draft Ready!" : TowerOfDunanRuntime.RunActive ? $"Floor {TowerOfDunanRuntime.FloorsCleared} Active" : $"Endless · Best Fl.{ledger.HighestTowerFloor}"),
    new GuildMenuRow("War Squads", $"{ledger.Commanders.Count} commanders · Branches")
   };
   focus = Math.Clamp(focus, 0, codexRows.Length - 1);
   var (cTitle, cArea, cDesc, cReward, cCond) = focus switch
   {
    0 => ("Army Campaign Record", $"Drills: {ledger.TotalDrillsWon} ({ledger.TotalVeteranDrillsWon} Veteran) · Commendations: {ledger.Commendations}",
          $"Tracked commanders: {ledger.Commanders.Count}. Castle Upgrades: {ledger.CastleUpgrades?.Count ?? 0}/{Reforged2Rules.CastleUpgrades.Length}.\nActive Bounty: {bounty.Title} ({ledger.ActiveBountyProgress}/{bounty.Target})."
          + (ledger.ActiveNemesis != null ? $"\n☠ Nemesis: {ledger.ActiveNemesis.Name} {ledger.ActiveNemesis.Title} (Tier {ledger.ActiveNemesis.Tier})" : ""),
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
    3 => ("Star Bonds & Hunter's Bestiary", $"{activeBonds}/{StarBondRules.Catalogue.Length} Bonds · {ledger.Bestiary?.Count ?? 0} Species Catalogued",
          "Press F5 anytime to inspect your Hunter's Bestiary. Every 10 unique species catalogued pays +1 Dunan Commendation.",
          $"Bounties Completed: {ledger.BountiesCompleted}   ·   Nemeses Slain: {ledger.NemesesSlain}",
          "Check Party Strategy from the main Guild menu for exact Star Bond triggers."),
    4 => ($"War Mutator · {Reforged2Rules.MutatorTitle(ArmyProgressionRuntime.ActiveMutator)}", "Applies to Army Training & Custom Skirmishes",
          Reforged2Rules.MutatorDescription(ArmyProgressionRuntime.ActiveMutator),
          ArmyProgressionRuntime.ActiveMutator != WarMutator.Standard ? "+1 Bonus Dunan Commendation on Drill/Skirmish Victory!" : "No extra bonus under Standard Battlefield.",
          "Confirm cycles Mutator (Standard / Blood Moon / Iron Attrition)."),
    5 => ("Dunan Honors & Achievements", $"{ledger.UnlockedHonors?.Count ?? 0} of {Reforged2Rules.Honors.Length} Honors Unlocked (+2 Comm. each)",
          string.Join(" · ", Reforged2Rules.Honors.Select(h => (ledger.UnlockedHonors?.Contains(h.Id) == true ? "★ " : "○ ") + h.Title)),
          $"Apex Packs Cleared: {ledger.ApexPacksCleared}  ·  Bounties: {ledger.BountiesCompleted}  ·  Drills: {ledger.TotalDrillsWon}",
          "Each Honor unlocks automatically in-game and awards +2 Dunan Commendations."),
    6 => ("Party Character Talents (108 Stars)", $"{ledger.PartyMastery?.Count ?? 0} Heroes Tracked",
          "Assign Berserker, Arcanist, Bulwark, or Tactician builds to any of the 108 Stars. Rank III (250 XP) empowers every Talent!",
          "Makes every favorite Suikoden II character viable for Nightmare endgame.",
          "Confirm opens the Party Talents builder."),
    7 => ("Tower of Dunan · Endless Roguelite", $"Highest Floor: {ledger.HighestTowerFloor}  ·  Active Relics: {TowerOfDunanRuntime.ActiveRelics.Count}",
          "Battle through escalating floors, draft 1 of 3 Relics after every floor (Legendary Relics on Floor 5/10/15!), and Cash Out for Potch & Commendations!",
          "12 Relics + 3 Legendary Warden Relics + optional Blood Oath Risk Pact.",
          "Confirm opens the Tower of Dunan."),
    _ => ("War Squad Branch Builder", $"{ledger.Commanders.Count} Enlisted Commanders",
          "Reassign any enlisted commander's unit branch (Infantry / Archer / Magicians) for Custom Skirmishes and inspect War Synergy pairs.",
          "Synergy Pairs: Flik+Viktor · Shu+Apple · Luc+Mazus · Humphrey+Georg.",
          "Confirm opens the War Squads customizer.")
   };
   return GuildRebornScreen.Paginate(new("TACTICIAN'S CODEX", codexRows, focus, cTitle, cArea, cDesc, cReward, cCond, status,
       focus == 1 ? "Cycle Stance" : focus == 2 && ArmyProgressionRuntime.HuntModifiersEnabled ? "Cycle Modifier" : focus == 4 ? "Cycle Mutator" : focus == 5 ? "Export Chronicle" : focus is 6 or 7 or 8 ? "Open" : "Back", back, preview));
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
   TrainingWar.Drill.SkirmishIronVanguard => Reforged2Rules.Skirmishes[5].Description,
   TrainingWar.Drill.SkirmishBastion => Reforged2Rules.Skirmishes[6].Description,
   TrainingWar.Drill.SkirmishImperialApex => Reforged2Rules.Skirmishes[7].Description,
   TrainingWar.Drill.SkirmishTwinBlades => Reforged2Rules.Skirmishes[8].Description,
   TrainingWar.Drill.SkirmishDunanFinale => Reforged2Rules.Skirmishes[9].Description,
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
  var skSpec = (int)drill >= 6 && (int)drill - 6 < Reforged2Rules.Skirmishes.Length ? Reforged2Rules.Skirmishes[(int)drill - 6] : null;
  int projectedComm = (skSpec != null ? skSpec.Commendations : (veteran ? 2 : 1))
      + (ArmyProgressionRuntime.ActiveMutator != WarMutator.Standard ? 1 : 0);
  int basePotch = skSpec != null ? skSpec.BasePotch : TrainingResultRules.Potch(veteran);
  string briefing = page == GuildPage.Difficulty
      ? $"\nLoadout: {Reforged2Rules.StanceTitle(ArmyProgressionRuntime.ActiveStance)} · {Reforged2Rules.MutatorTitle(ArmyProgressionRuntime.ActiveMutator)}{directiveNote}"
      : "";
  string reward = Plugin.Options.EnableTrainingRewards.Value ?
      (page == GuildPage.Training && veteranAllowed ?
          $"{(skSpec != null ? skSpec.BasePotch : TrainingResultRules.Potch(false)):N0} Standard  /  {(skSpec != null ? skSpec.BasePotch : TrainingResultRules.Potch(true)):N0} Veteran Potch · +{projectedComm} Comm.{directiveNote}" :
          $"{basePotch:N0} Potch  ·  +{projectedComm} Commendation(s){directiveNote}") : "Training rewards disabled";
  string condition = unavailable.Length != 0 ? unavailable : veteran && !veteranAllowed ?
      "Veteran requires the native tactics options and a verified battlefield." :
      "Rewards require routing the opposing commander. Timed endings and defeat pay no Potch." + briefing;
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
  bool battleHud=!previewSmoke&&Combat.State is {Ready:true}&&((!Reforged2Runtime.CleanHudMode&&Plugin.Options?.EnableBattleTacticalHud.Value==true)||Reforged2Runtime.BannerVisible||Reforged2Runtime.ScannerVisible);
  bool warHud=!previewSmoke&&WarRuntime.Active&&!Reforged2Runtime.CleanHudMode&&Plugin.Options?.EnableBattleTacticalHud.Value==true;
  bool helpCard=!previewSmoke&&Reforged2Runtime.HelpCardVisible;
  bool bestiaryCard=!previewSmoke&&Reforged2Runtime.BestiaryVisible;
  if(smokeExitRequested||faulted||(!previewSmoke&&!helpCard&&!bestiaryCard&&!warHud&&!battleHud&&!prompt&&!feedback&&offer==null&&(!HuntGuildRuntime.Visible||(!HuntGuildRuntime.Context&&!HuntGuildRuntime.Preview))))return;
  try
  {
   var matrix=GUI.matrix;var color=GUI.color;
   try
   {
    GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;
    if(warHud)
    {
     string warText=$"WAR COMMAND  ·  {Reforged2Rules.StanceTitle(ArmyProgressionRuntime.ActiveStance)}  ·  {Reforged2Rules.MutatorTitle(ArmyProgressionRuntime.ActiveMutator)}"+(ArmyProgressionRuntime.ActiveDirective!=DrillDirective.None?$"  ·  {ArmyProgressionRules.DirectiveTitle(ArmyProgressionRuntime.ActiveDirective)}":"");
     float ww=Math.Min(740,Math.Max(360,warText.Length*10+44));
     var wbox=new Rect((Screen.width-ww)/2,14,ww,40);
     Window(wbox);
     Label(new(new(18,9,ww-36,24),warText,16,"gold"),wbox.x,wbox.y,1);
    }
    if(helpCard)
    {
     var led=ArmyProgressionRuntime.Current;
     var hr=Reforged2Rules.HunterRankForClears(led.HuntsCompleted);
     var bounty=Reforged2Rules.ActiveBounty(led.ActiveBountyIndex);
     float hw=490,hh=298;
     var hbox=new Rect(Screen.width-hw-18,66,hw,hh);
     Window(hbox);
     Label(new(new(18,12,hw-36,26),"REFORGED 6.0 QUICK GUIDE   (F3 to hide)",17,"gold"),hbox.x,hbox.y,1);
     Label(new(new(18,44,hw-36,124),
         "F2   Toggle Clean HUD Mode\nF4   Battle Scanner · Turn Order · Buffs · Live DPS\nF5   Cycle Bestiary -> Honors -> Army Dossier -> Off\nF6   Accept Local Field Hunt\n←/→  Fast Page-Flip in Castle Guild",15),hbox.x,hbox.y,1);
     string nemLine=led.ActiveNemesis is {} an?$"☠ Wanted: {an.Name} {an.Title} (T{an.Tier} · +{Reforged5Rules.NemesisCommendationReward(an.Tier)} Comm.)":$"Bounty: {bounty.Title} ({led.ActiveBountyProgress}/{bounty.Target})";
     Label(new(new(18,172,hw-36,44),nemLine,14,led.ActiveNemesis!=null?"alert":"gold"),hbox.x,hbox.y,1);
     Label(new(new(18,220,hw-36,68),
         $"Commendations: {led.Commendations}  ·  {Reforged2Rules.HunterRankTitle(hr)}  ·  Honors: {led.UnlockedHonors?.Count??0}/{Reforged2Rules.Honors.Length}\nStance: {Reforged2Rules.StanceTitle(ArmyProgressionRuntime.ActiveStance)}  ·  Mutator: {Reforged2Rules.MutatorTitle(ArmyProgressionRuntime.ActiveMutator)}\nUpgrades: {led.CastleUpgrades?.Count??0}/{Reforged2Rules.CastleUpgrades.Length}  ·  Streak: {Reforged2Runtime.WinStreak}  ·  Bestiary: {led.Bestiary?.Count??0}",14,"success"),hbox.x,hbox.y,1);
    }
    if(bestiaryCard)
    {
     var led=ArmyProgressionRuntime.Current;
     if(Reforged2Runtime.BestiaryViewMode==3)
     {
      var roster=ArmyProgressionRuntime.RosterSnapshot().Take(6).ToArray();
      int hallClears=led.HallRecords?.Values.Count(r=>r.OriginalCleared||r.NightmareCleared)??0;
      float dw=540,dh=104+Math.Max(1,roster.Length)*32;
      var dbox=new Rect(18,66,dw,dh);
      Window(dbox);
      Label(new(new(18,12,dw-36,26),$"COMMANDER'S DOSSIER  ({led.Commanders.Count} Enlisted · {hallClears} Hall Medals)",16,"gold"),dbox.x,dbox.y,1);
      Label(new(new(18,38,dw-36,20),$"Drills: {led.TotalDrillsWon} ({led.TotalVeteranDrillsWon} Vet) · Story Wars: {led.TotalStoryWarsWon} · Hall NM: {led.HallNightmareClears}  (F5)",13,"muted"),dbox.x,dbox.y,1);
      if(roster.Length==0)
       Label(new(new(18,68,dw-36,24),"Win an Army Training Drill or Skirmish to enlist commanders!",15),dbox.x,dbox.y,1);
      else
      {
       for(int ci=0;ci<roster.Length;ci++)
       {
        var c=roster[ci];
        var rk=ArmyProgressionRules.RankForXp(c.Xp);
        float ry=68+ci*32;
        Label(new(new(18,ry,dw-210,22),$"{c.Name}  ·  {ArmyProgressionRules.RankTitle(rk)}",15),dbox.x,dbox.y,1);
        Label(new(new(dw-192,ry,174,22),$"{c.Xp} XP · {ArmyProgressionRules.DoctrineTitle(c.Doctrine)}",14,"success"),dbox.x,dbox.y,1);
       }
      }
     }
     else if(Reforged2Runtime.BestiaryViewMode==2)
     {
      int unlockedCount=led.UnlockedHonors?.Count??0;
      float hw2=560,hh2=82+((Reforged2Rules.Honors.Length+1)/2)*30;
      var hbox2=new Rect(18,66,hw2,hh2);
      Window(hbox2);
      Label(new(new(18,12,hw2-36,26),$"DUNAN HONORS  ({unlockedCount}/{Reforged2Rules.Honors.Length} Unlocked · +2 Comm. Each)",16,"gold"),hbox2.x,hbox2.y,1);
      Label(new(new(18,38,hw2-36,20),"F5 again for Army Dossier · Combos: "+led.TotalCombosLanded+" · S-Ranks: "+led.TotalSRanks+" · Overdrives: "+led.TotalOverdrivesTriggered,13,"muted"),hbox2.x,hbox2.y,1);
      for(int hi=0;hi<Reforged2Rules.Honors.Length;hi++)
      {
       var h=Reforged2Rules.Honors[hi];
       bool got=led.UnlockedHonors?.Contains(h.Id)==true;
       float colX=(hi%2==0)?18:hw2/2+6;
       float rowY=66+(hi/2)*30;
       Label(new(new(colX,rowY,hw2/2-24,24),(got?"★ ":"○ ")+h.Title,14,got?"success":"muted"),hbox2.x,hbox2.y,1);
      }
     }
     else
     {
      var entries=ArmyProgressionRuntime.BestiarySnapshot();
      int totalSpecies=entries.Length;
      int totalKills=entries.Sum(e=>e.Kills);
      int totalApex=entries.Sum(e=>e.ApexKills);
      int nextMilestone=(totalSpecies/10+1)*10;
      int shown=Math.Min(8,totalSpecies);
      float bw=500,bh=94+Math.Max(1,shown)*34;
      var bbox=new Rect(18,66,bw,bh);
      Window(bbox);
      Label(new(new(18,12,bw-36,26),$"HUNTER'S BESTIARY  ({totalSpecies} Species · Next +1 Comm: {totalSpecies}/{nextMilestone})",16,"gold"),bbox.x,bbox.y,1);
      Label(new(new(18,38,bw-36,20),$"Lifetime Kills: {totalKills:N0}  ·  Apex: {totalApex}  ·  F5 again for Honors",13,"muted"),bbox.x,bbox.y,1);
      if(shown==0)
       Label(new(new(18,66,bw-36,24),"No enemies catalogued yet. Win battles to fill your Bestiary!",15),bbox.x,bbox.y,1);
      else
      {
       for(int i=0;i<shown;i++)
       {
        var be=entries[i];
        float ry=66+i*34;
        string apexStr=be.ApexKills>0?$"  ★{be.ApexKills} Apex":"";
        Label(new(new(18,ry,bw-150,22),$"{be.Name}{(be.HighestLevel>0?$" Lv{be.HighestLevel}":"")}  ({be.WeaknessHint})",15),bbox.x,bbox.y,1);
        Label(new(new(bw-132,ry,114,22),$"{be.Kills} kills{apexStr}",14,"success"),bbox.x,bbox.y,1);
       }
      }
     }
    }
    if(!battleHud&&!prompt&&!feedback&&offer==null&&!HuntGuildRuntime.Visible)return;
    if(battleHud)
    {
     if(!Reforged2Runtime.CleanHudMode&&Plugin.Options?.EnableBattleTacticalHud.Value==true)
     {
      int[] party=StarBondRuntime.FieldParty();
      var active=StarBondRules.Catalogue.Where(b=>StarBondRules.Includes(b,party)).Select(b=>b.Title).ToArray();
      int morale=ArmyProgressionRuntime.Current.MoraleBonusCharges;
      string apexTag=Reforged2Runtime.NemesisBattleActive?"☠ NEMESIS · "
          :TowerOfDunanRuntime.BattleInProgress?$"TOWER FL.{TowerOfDunanRuntime.CurrentFloor} · "
          :Reforged2Runtime.ApexBattleActive?$"⚠ APEX {Reforged2Rules.ApexAffixShort(Reforged2Runtime.ActiveApexAffix)} · ":"";
      string momTag=Reforged2Runtime.MomentumEnabled?(Reforged2Runtime.OverdriveActive()?" ★ OVERDRIVE!":$" · Mom {Reforged2Runtime.Momentum}%"):"";
      string streakTag=Reforged2Runtime.WinStreak>=3?$" · Streak x{Reforged2Runtime.WinStreak}":"";
      string text=apexTag+(active.Length>0?"★ "+string.Join(" · ",active.Take(2)):"F4 Scan · F3 Guide")+momTag+streakTag+(morale>0?$" · Morale +{morale}":"");
      float w=Math.Min(600,Math.Max(260,text.Length*9.5f+36));
      var pill=new Rect(Screen.width-w-18,16,w,40);
      Window(pill);
      Label(new(new(14,9,w-28,24),text,15,Reforged2Runtime.OverdriveActive()?"success":"gold"),pill.x,pill.y,1);
     }
     var feedLines=Reforged2Runtime.ActiveFeed();
     if(feedLines.Length>0)
     {
      for(int fi=0;fi<feedLines.Length;fi++)
      {
       var fl=feedLines[fi];
       float bw=Math.Min(680,Math.Max(300,fl.Text.Length*11.5f+48));
       var banner=new Rect((Screen.width-bw)/2,18+fi*46,bw,40);
       Window(banner);
       Label(new(new(18,9,bw-36,24),fl.Text,17,fl.Tone),banner.x,banner.y,1);
      }
     }
     if(Reforged2Runtime.ScannerVisible&&Framework.Singleton<BATTLE_WORK>.instance?.pm_data is {} pmUnits)
     {
      var enemies=pmUnits.Where(u=>u!=null&&!u.IsPlayer&&u.hp>0&&!u.IsDead).Take(6).ToArray();
      if(enemies.Length>0)
      {
       float sw=500,sh=52+enemies.Length*56;
       var scanBox=new Rect(18,16,sw,sh);
       Window(scanBox);
       string scanHeader=Reforged2Runtime.NemesisBattleActive
           ?$"TACTICAL SCANNER  [☠ NEMESIS · {Reforged2Rules.ApexAffixShort(Reforged2Runtime.ActiveApexAffix)}]  (F4)"
           :TowerOfDunanRuntime.BattleInProgress
           ?$"TACTICAL SCANNER  [TOWER FL.{TowerOfDunanRuntime.CurrentFloor}]  (F4)"
           :Reforged2Runtime.ApexBattleActive
           ?$"TACTICAL SCANNER  [APEX · {Reforged2Rules.ApexAffixShort(Reforged2Runtime.ActiveApexAffix)}]  (F4)"
           :"TACTICAL SCANNER   (F4 to hide)";
       Label(new(new(18,10,sw-36,28),scanHeader,18,"gold"),scanBox.x,scanBox.y,1);
       for(int i=0;i<enemies.Length;i++)
       {
        var e=enemies[i]!;
        string displayName=Reforged2Runtime.ResolveEnemyDisplayName(e,out string role);
        int lv=e.monster_data?.level??0;
        string weak=e.calc_para is {Length:>3}?Reforged2Rules.TacticalWeaknessHint(e.calc_para[2],e.calc_para[3]):"Balanced";
        string statusTag=((e.status&b_game_h.STATUS_SLEEP)!=0?" [SLP]":"")+((e.status&b_game_h.STATUS_POISON)!=0?" [PSN]":"");
        int brk=Reforged2Runtime.ArmorBreakStacksFor(e);
        if(brk>0)statusTag+=$" [BRK x{brk}]";
        float hpFrac=e.max_hp>0?Math.Clamp((float)e.hp/e.max_hp,0f,1f):0f;
        float ry=44+i*56;
        Label(new(new(18,ry,sw-170,24),$"{displayName}{(lv>0?$" Lv{lv}":"")}{statusTag}",17),scanBox.x,scanBox.y,1);
        Label(new(new(sw-152,ry+1,134,22),$"{e.hp} / {e.max_hp}",16,"gold"),scanBox.x,scanBox.y,1);
        Label(new(new(18,ry+21,sw-36,18),$"{role}  ·  {weak}",14,"muted"),scanBox.x,scanBox.y,1);
        Fill(new Rect(scanBox.x+18,scanBox.y+ry+40,sw-36,9),new Color(.04f,.07f,.16f,.95f));
        Fill(new Rect(scanBox.x+20,scanBox.y+ry+42,(sw-40)*hpFrac,5),hpFrac<=0.25f?new Color(.95f,.35f,.30f,.98f):hpFrac<=0.50f?new Color(.96f,.76f,.32f,.98f):new Color(.38f,.90f,.58f,.98f));
       }
       var bwInst=Framework.Singleton<BATTLE_WORK>.instance;
       var (frontAlive,backAlive)=Reforged2Runtime.FormationCounts(bwInst);
       var partyBuffs=Reforged2Runtime.PartyBuffSummary(bwInst);
       var turnOrder=Reforged2Runtime.TurnOrderPreview(bwInst);
       var dpsList=Reforged2Runtime.PartyDpsSummary();
       var formTags=new List<string>(3);
       if(frontAlive>=3)formTags.Add("Phalanx (-6% Front DMG)");
       if(backAlive>=3)formTags.Add("Crossfire (+6% Back DMG)");
       if(Reforged2Runtime.ActiveTalentSet!=TalentSetBonus.None)formTags.Add("★ "+Reforged5Rules.TalentSetTitle(Reforged2Runtime.ActiveTalentSet));
       if(Reforged2Rules.IsLastStandActive(Reforged2Runtime.KoAllyCount(bwInst)))formTags.Add("LAST STAND (+15% ATK/-10% Taken)");
       if(formTags.Count>0||partyBuffs.Length>0||turnOrder.Length>0||dpsList.Length>0)
       {
        float py=scanBox.y+scanBox.height+8;
        float ph=34+(turnOrder.Length>0?22:0)+(formTags.Count>0?22:0)+Math.Min(3,partyBuffs.Length)*19+(dpsList.Length>0?22+Math.Min(4,dpsList.Length)*19:0);
        bool dockRight=py+ph>Screen.height-140&&!helpCard;
        var pbox=dockRight?new Rect(Screen.width-sw-18,66,sw,ph):new Rect(18,py,sw,ph);
        Window(pbox);
        Label(new(new(18,8,sw-36,22),"PARTY BUFFS, TURN ORDER & LIVE DPS",14,"gold"),pbox.x,pbox.y,1);
        float cy=28;
        if(turnOrder.Length>0){Label(new(new(18,cy,sw-36,20),"SPD: "+string.Join(" > ",turnOrder),12,"muted"),pbox.x,pbox.y,1);cy+=22;}
        if(formTags.Count>0){Label(new(new(18,cy,sw-36,20),string.Join("  ·  ",formTags),13,"success"),pbox.x,pbox.y,1);cy+=22;}
        for(int bi=0;bi<Math.Min(3,partyBuffs.Length);bi++){Label(new(new(18,cy,sw-36,18),partyBuffs[bi],12),pbox.x,pbox.y,1);cy+=19;}
        if(dpsList.Length>0)
        {
         Label(new(new(18,cy+2,sw-36,18),"LIVE BATTLE DAMAGE",12,"gold"),pbox.x,pbox.y,1);cy+=20;
         for(int di=0;di<Math.Min(4,dpsList.Length);di++)
         {
          var d=dpsList[di];
          Label(new(new(18,cy,sw-130,18),$"{d.Name}  ({(int)Math.Round(d.Share*100)}%)",12),pbox.x,pbox.y,1);
          Label(new(new(sw-110,cy,92,18),$"{d.Damage:N0}",12,"gold"),pbox.x,pbox.y,1);
          cy+=19;
         }
        }
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
