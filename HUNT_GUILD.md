# The Castle Hunt Guild

Sun King's Retinue has complete gameplay acceptance on **1.12.2-guild-ui.1**.
v1.19 refines its presentation and feedback; it has no separate user-assisted quest session.
See [exact-build evidence](docs/ACCEPTANCE.md).

The v1.37 catalogue has ten contracts. Press Up or Down to browse. The next
page appears automatically when the cursor crosses the end of the current
page. Tiers describe preparation, and the selected contract gives its
destination, enemies, reward and route advice. Report or withdraw under
**Guild Status**. [All seven additions, formations and rewards](docs/GRAND-GUILD-V137.md).

In v1.38 tier order is used in the list. Open means the Guild will accept the
contract, and the original story still controls travel. If field reissue is
enabled, open the normal party menu in an eligible hunting field and follow
its prompt. Press F6, or hold L1 and press R1. This takes a fresh contract and
removes the trip to headquarters to accept it again after a reload. The
outstanding report from the prior session still clears.

## Take a contract

1. Reach headquarters through the ordinary story. Visit the Suggestion Box
   corridor outside the second-floor meeting hall. Approach the attendant from
   below and face up.
2. Form a healthy party through **Leona before accepting**. Bring healing,
   physical/magical options and inventory space. No precise minimum party level
   has been calibrated for this elite encounter.
3. Talk using the game's normal confirm control. Open **Hunt Contracts** and
   accept **Sun King's Retinue**. The menu uses the game's navigation,
   confirm and cancel bindings.
4. Travel normally to **Cave of the Wind**, without loading another save.
5. Walk in the eligible Cave field. The contract automatically requests the next
   eligible legitimate walking encounter. **No F6/F10/Ctrl is needed.**
6. Fight Sun King and Sorcerer in the original RPG engine, finish native victory
   rewards, and return to the field.
7. Return to the attendant. **Guild Status** opens at **Report completion**
   when a hunt is ready to report. Report, then accept again if desired.

## State, rewards and retries

No Contract → Accepted → Completed/Awaiting Report → Reported → available again.
Accepting does not immediately launch combat. Reporting grants no extra money,
item, XP or rune recovery. Native battle rewards remain authoritative.

Sun's configured bonus is **2,500 Potch**. A total3,478 reward and actual3,478
credit were observed under the accepted session settings; totals depend on
configuration and the native money cap. No daily cap or artificial grind ceiling
is added.

Stone of Power is a **fallback when the original loot slot is empty**, not a
guaranteed extra item alongside rolled loot. Original drops retain priority.
Native inventory capacity/decline UI applies. An offer is not proof of receipt;
two native inventory receipts were observed on v1.12.2.

Escape leaves the accepted contract retryable and grants no victory bonus.
Unrelated or scripted encounters must not become the hunt. Withdrawal is available
through the Guild. **Actual loading/restarting clears session-only contracts**,
including unreported completion. A remembered selection is not saved quest progress.
Never accept at castle and load a Cave save as a shortcut.

| Original contract | Destination | Availability |
|---|---|---|
| Sun King's Retinue |Cave of the Wind, vd06_00| Enabled; accepted exact v1.12.2 quest |
| Dreamer's Circuit |Audited Sindar Ruins field vc12_00| Enabled in fresh v1.38; native-tested |
| Thornwake Patrol |Greenhill City area, ve03_00; precise route unplayed| Enabled in fresh v1.38; native-tested |

The v1.36 attendant is limited to **vk07, event variant 3**. The original
Suggestion Box interaction takes priority. Unsupported castle layouts may have
no attendant. **Army Training** has its own Guild page and opt-in in the copied
runtime. Contracts remain session-only. [Current relocation and menu evidence](docs/CASTLE-GUILD-TOTAL-WAR-V136.md).

## v1.19 presentation

Contract details separate destination, targets, preparation and victory spoils.
Unavailable contracts remain visible for browsing, with a clear unavailable prompt;
failed acceptance now appears in the screen's message line. Confirm/cancel retain
the original game's bindings. Cancel closes the panel without withdrawing a hunt.

Dreamer and Thornwake stay experimental and disabled by default. Thornwake's region
uses the original map registry and save-location text alias (3052 → 432, Greenhill
City); the exact walking route has not been played. No encounter, balance, reward
or drop probability was changed. Conditional item offers still depend on native
loot, eligibility and inventory handling; reporting grants nothing extra.

The historical v1.19 presentation DLL was `0EF0B648CC897C2119ED6919DA250FD1FDFFD1CAC5F8F536FB8BA8B5A8A1BEA7`. Original frame/font/cursor rendering was
exercised in an unloaded preview. Offline previews are not gameplay screenshots,
and no current-hash field, controller or complete-quest acceptance is claimed.

## v1.36.1 graphical verification

The existing Guild renderer produced eight in-engine screens in the copied game
at 1920×1080 and 3200×1800. Captures cover all contract cards, both Training
difficulties and Guild Status. The preview runs at the launcher and leaves
live attendant interaction and training return unverified on this build.
[Graphics and reproduction notes](docs/GUILD-PREVIEW-V1361.md).

## v1.25 catalogue integration

Development build `1.25.0-hunt-catalogue.1` with SHA256 `AA27445761E1E7581FF2D32DCF5FA608E47A072F06A5B307B809A46557EA09B0`.

| Contract | Original walking table | Preparation | Contract bonus | Possible fallback item |
|---|---|---|---|---|
| Dreamer's Circuit | Sindar Ruins, `vc12_00` | Nightmare can inflict Sleep. Prepare recovery while dealing with Mr. Venus and MegaWatt. | 1,200 Potch | Stone of Magic Protection |
| Thornwake Patrol | Greenhill City region, `ve03_00` | The two Holly Elves can inflict Balloon. Plan for the escorts alongside Melonzoo. | 3,000 Potch | Flowing Orb |

The original field records link eight Sindar and four Greenhill map variants
to their encounter table. These are verified map records, not directions to a
walkable route. Follow the ordinary campaign and walk where original encounters
occur. Normal eligibility requires the native Normal rate and a live, nonzero
party owner. Both contracts remain opt-in, with the existing Castle Guild
automatic request and per-contract switches. No configuration migration is needed.

The two isolated native runs exercised the production request observer with a
synthetic walking witness, then called original `BattleDataSetup` for each
contract. They verified the chosen party and unchanged RNG after the native
selection. Separate quest tests checked wrong field, foreign owners, scripted
requests, Hard mode, disabled contracts, foreign receipts, escape retry,
duplicate completion and report, reacceptance and reset. Native reward
calculation used synthetic victory and defeated-roster inputs. The prepared
totals were 2,008 Potch for Dreamer and 5,678 for Thornwake under the fixture
settings. The game still applies its normal purse cap, drop priority and
inventory handling. Reporting supplies no extra reward.

Loaded native records and selectors confirmed the Sleep and Balloon threat
described on the Guild cards. Party fixtures do not establish encounter fairness,
so stats and reward rates were preserved. A future acceptance session can select
one contract at the castle, travel normally without reloading, complete or
escape the encounter, and confirm the reward and report-back flow. The new
contracts have no rendered or player-earned quest acceptance. Sun's historical
v1.12.2 acceptance remains specific to that binary.

For protected play preparation and the latest candidate, see the [v1.26 acceptance guide](docs/HUNT-ACCEPTANCE-V126.md).
