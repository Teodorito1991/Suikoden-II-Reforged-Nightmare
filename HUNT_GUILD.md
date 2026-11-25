# The Castle Hunt Guild

No Guild/Hunt/Hall behavior changed in v1.52. The Full Content Preview retains all22
supported Original echoes and10 Hunts. Normal-build Army Training remains default
OFF after its save-contract fixes, pending a genuine restored autosave check.
[Army Legacy decision](docs/ARMY-LEGACY-V152.md).

## Current v1.51.1 integration

Full Content Preview selects all22 supported Original Hall definitions and10 Hunts.
Original Luca's accepted implementation is unchanged and its exact DLL preserved.
Beast Rune victory/return/cleanup passed with a deliberately boosted disposable
party: this verifies mechanics, not normal-party balance. StoryProgression and
per-entry safety/configuration gates remain; Spoiler Mode defaults OFF and requires
confirmation. The three unresolved story witnesses can use that explicit mode.
Nightmare stays optional. Training's normal-build direct-save contract remains
OFF pending current-build restored autosave and fault-path acceptance. No XP.
[Current evidence and blockers](docs/FEATURE-COMPLETION-V1511.md).

Earlier sections below retain their milestone-specific historical claims.

## v1.51 current checkpoint

Original Luca's three-team gauntlet now has one exact-build owner completion,
private HP carry, final earned win, clean returns and original-party restoration.
22 Hall definitions/22 optional Nightmare profiles; Nightmare Luca and failure/
repeat-run acceptance remain open. `EnableHall_luca_gauntlet` is an opt-in.
**Unlock All Bosses** is a reversible CFG preference (default OFF), with an explicit
spoiler warning and second confirmation. It changes availability only, preserving
per-entry opt-outs. Use **Restore Story Progression** to reverse it. Party drafts
store revalidated IDs, not progression. Training's new native-save checks remain
experimental/default OFF; no commander XP. Beast Rune victory remains unverified.
[Exact evidence and limits](docs/LAST-CHALLENGE-V151.md).

The sections below retain their milestone-specific historical evidence.

## v1.50 optional encounter families

Hall:21 original-source definitions and21 optional Nightmare profiles, with six-page
browsing through the original confirm/cancel controls. Worm/Moth and the multipart
final boss retain their native controllers. Their normal access uses read-only
original ending witnesses; new complex encounters remain independently configurable
and experimental. Three old unresolved memories still stay normal-locked.

All10 Hunts, native loot priority, escape retry and reporting without a second
payout are preserved. Existing Sun and Undying Watch acceptance retains its historical
DLL identity. Luca is a stage/private-HP foundation, not an available gauntlet.
Training has a new default-off normal-build ownership contract alongside the old
protected harness. No commander XP is implemented.
[Current implementation and evidence](docs/FINAL-ENCOUNTERS-V150.md).

Sun King's Retinue has complete gameplay acceptance on **1.12.2-guild-ui.1**.
v1.19 refines its presentation and feedback; it has no separate user-assisted quest session.
See [exact-build evidence](docs/ACCEPTANCE.md).

**The Undying Watch now has full v1.48 gameplay acceptance:** field acceptance,
legitimate walking battle, earned victory, 12,236 Potch credited and Sacrificial
Jizo received, field return, ordinary travel to the Guild, report and reacceptance.
The level-44–46 checkpoint party was unchanged. Escape Talisman helped the owner
leave the dungeon; no teleport/load shortcut or fabricated progression was used.
[Current profile and exact-build evidence](docs/FRESH-CAMPAIGN-V148.md).
The fresh proposal enables Sun and Undying; the other eight remain explicit opt-ins.

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

## Hall of Echoes in v1.43

The same attendant now offers a separate Hall page containing Pest Rat and Harpy.
Choose an encounter with Up/Down and confirm a fresh press. Cancel returns to
Services. The Hall requires EnableHallOfEchoes; Harpy also requires
EnableHallHarpy. Both default to false.

The owner completed Pest Rat → Harpy → Pest Rat victories in the copied game.
Harpy uses its native mountain scene and original attacks. Its reward is native
Potch with no item drop. Pest Rat retains its possible Poison Orb. The Hall
uses ordinary native victory processing, with no report-back reward.
[Exact v1.43 evidence and limitations](docs/HALL-SECOND-CHALLENGER-V143.md).

## Hall expansion in v1.44

The Hall now has five paginated memories: Pest Rat, Harpy, Golden Wolf, Sierra and
Harpy · Second Encounter. Original native foe levels and potential native spoils
are shown without promising a fixed Potch credit. Locked entries hide their names.
Three progression gates are source-backed; Pest Rat and the second Harpy remain
locked under StoryProgression. Testing availability requires both an explicit
UnrestrictedTesting policy and developer tools, and never changes story state.
Golden Wolf/Sierra owner victories and the second Harpy defeat belong to the first
candidate; revised exact-build evidence is separate. Historical sections above
retain their original versions and conclusions.
See [v1.44 evidence and configuration](docs/HALL-ECHOES-PAST-V144.md).

## v1.45 progression correction

The v1.44 witness interpretation above is historical and superseded. Correct
source-backed witnesses are Pest Rat B6/02, first Harpy 21/10 and Golden Wolf
16/02. These correspond to original category-15 boss loads and post-battle
completion writes. Sierra and second Harpy have no proven normal unlock yet;
ordinary map indices and shared battle-result flags are insufficient. All five
encounters remain implemented without combat changes.
[Current exact-build evidence](docs/HALL-ECHOES-EARNED-V145.md).

Exact v1.45 DLL gameplay also passed Sierra and second Harpy victories under the
explicit testing override (850 and 127,500 Potch), followed by Pest Rat flee with
zero payout and working Guild interaction. This verifies their battle paths, not
a normal-play unlock for those two trials.

## Current Hall catalogue — v1.46

Eight entries retain original order/IDs: Pest Rat, Harpy, Golden Wolf, Sierra,
second Harpy, Stone Golem, Abomination and Worm. Four rows per page; normal
navigation keeps selection visible. New trials have independent default-off
switches. Locked names remain ???. Golem and Abomination read source-backed
C9/01 and 56/20 witnesses; Worm, Sierra and second Harpy stay developer-only.
Owner earned all three new wins on the exact v1.46 DLL, then fled Golem and Pest
Rat and reopened the Guild. All observed returns preserved protected state.
[Current native, graphical and gameplay evidence](docs/HALL-NEW-MEMORIES-V146.md).
Earlier sections document historical versions, not current catalogue size.

## Current Nightmare tier — v1.47

Selecting an available memory opens Original Echo / Nightmare Echo. The latter
requires its independent opt-in and is offered only for Golden Wolf, Stone Golem
and Abomination. Cancel returns to the selected catalogue row; locked names stay
hidden. Existing story and individual-trial gates apply to both tiers. Sierra,
second Harpy and Worm still have no invented normal-play witnesses.

Nightmare offers stronger owned HP, restrained native tactical preferences and
10–15% extra submitted damage. With rewards enabled, an earned victory with no
native loot makes one 25% Stone of Power offer roll. Native inventory capacity
and item acceptance apply. Original rewards do not change, and neither reporting
nor fleeing pays a bonus. [Exact rules and current acceptance](docs/HALL-NIGHTMARE-TRIALS-V147.md).
