# The Journey Begins — Fresh Campaign proposal (v1.48)

## Purpose and use

Choose **Normal** in Suikoden II. This is a reproducible configuration proposal,
not a declaration that the full campaign is certified, an installer, or permission
to overwrite personal settings. The installed Steam game is not changed.

From a source checkout, with PowerShell 7:

```powershell
./tools/prepare-fresh-campaign.ps1 -OutputDirectory ./fresh-campaign-staging
```

The destination must be empty. The tool generates configuration only under
BepInEx/config and a relative-path SHA256 manifest. It includes no game discovery,
DLL, saves, credentials, installation or deletion. Repeated generation to separate
empty directories produces matching hashes; a nonempty destination is rejected.
Review/merge deliberately if choosing to use it later. Existing personal files
are not consulted or replaced. Base authored module examples are copied and the
small profile overrides applied. Optional overlays are documented separately.

## Recommended decisions

| System | Fresh Campaign decision | Evidence / reason |
|---|---|---|
| Characters, Party Revolution, Star Bonds | On | Native profile/slot/lookup coverage; early party check, not full growth certification |
| Runes, physical techniques, Unites, healing | On | Established guarded native application and retained regressions |
| Regular profiles, formations, AI and pacing | On | Exact-source admission; live representative early/Mines battles |
| EXP / Potch | Native rates, 1.00 / 1.00 | No imposed grind gate or new currency penalty; differs from legacy 0.85 Potch example |
| Victory rune recovery | One lowest-depleted charge per eligible survivor | Existing native victory-only system; no recovery needed if no charge is missing |
| Existing boss tactics / six AI-only Revolution policies | Existing example defaults; MajorBosses off | Native legality/identity checks, no new story callbacks. Full boss playthrough remains unverified |
| Scripted Legendary Boss layer | Four independent switches off | Native-only transformation/transition evidence; retain as explicit advanced opt-in, never remove code |
| Original story-war profiles, AI, tactics, maneuvers | On | Audited original scenarios; native campaign outcomes remain owned by the game |
| Army Training | Off | Genuine historical battles exist, but runtime still requires the protected stock-transition harness |
| Hunts | Sun and The Undying Watch configured on | Historical Sun acceptance plus targeted current-candidate Undying Watch check. Other eight remain available through deliberate settings opt-ins |
| Guild attendant and field reissue | On | Normal talk/party-menu interaction; no developer overlay needed |
| Original Hall | Five source-backed memories configured on | Source-backed progression, preserved rendered history; exact-build limits remain documented |
| Nightmare tier | Off; separate optional overlay | Real prior-build wins/stone receipt, stronger optional challenge, not a main-story requirement |
| Sierra, second Harpy, Worm | Off and still normal-story locked | No invented completion witnesses |
| Developer tools, protection, selftests, exports, retired arena | Off | Not a campaign requirement or hidden cheat |

The profile does not rewrite global/embedded defaults. All ten Hunt definitions,
eight Original memories, three Nightmare profiles and prior war work are retained.
The configuration is intentionally narrower than enabling every experimental
switch. Enable additional hunts individually in the generated hunt-catalogue.json
when deliberately opting into them. The optional nightmare-opt-in.cfg is a small
merge overlay, not a replacement full configuration; keep StoryProgression and
developer tools off. Scripted boss options also remain individually available.

## Hunt and Hall roles

Hunts are exploration contracts: accept, travel/walk, earn native victory rewards,
return and report, then accept again. Session-only state clears on actual load;
never accept and then load a different checkpoint to skip travel. Report-back pays
nothing. The field-reissue action accepts a fresh local contract through the
original party menu (F6 on keyboard, or L1 + R1 on controller); it is a player
service, not developer damage protection or a fabricated completion.

Hall battles are headquarters rematches. The five supported original completion
witnesses are unchanged: Pest Rat B6/02, first Harpy 21/10, Golden Wolf 16/02,
Stone Golem C9/01 and Abomination 56/20. Other normal unlocks remain unresolved.
Nightmare rewards retain their established 25% conditional stone-offer policy
and native capacity rules. No new account currency or persistent reward database
exists. Original Hunt loot has priority; a particular battle cannot qualify for
both a uniquely bound Hunt source and an owned Hall echo table. Armed walking
requests/active training also prevent competing Hall entry. No theoretical farming
nerf, extra payout on report or artificial daily limit was added.

## Army readiness decision

**Story wars: recommended with guarded current modifiers; full campaign outcomes
still require the owner's playthrough. Army Training: protected-copy-only.**
TrainingWar.Enabled still depends on HasTransitionHarness: the stock transition
witness plus the exact private root-redirection hook. A normal campaign profile
cannot satisfy that boundary by setting a bool alone. Existing four drills on
three original scenarios, Veteran decisions and earned outcome/return history
remain valid at their recorded hashes. They are not declared unavailable research,
but they are not recommended for an ordinary installation. No harness was weakened
or new war launcher/restore experiment undertaken for this milestone. Commander
XP and persistent training are not implemented.

## Representative acceptance

Exact-build native production auditing and managed regressions passed, followed
by a copied-runtime startup using the actual generated profile and 22 original-
resource Guild previews. Component audit configuration temporarily enables its
required test opt-ins; that is separate from the recommended campaign settings.
The gameplay copy uses the generated profile with developer/selftests/protection,
input-feed and wallet fixtures off. Original Steam DLL and fourteen saves remain
outside the test writes.

A legitimate archived early checkpoint and Tinto Mines checkpoint were staged in
the copied save directory. Their prior Normal conversion changed only difficulty;
no new party, story flags, equipment or levels were fabricated. The early party
has existing levels 3–6; the Mines party is approximately levels 44–46.

### Actual exact-build gameplay

- Early checkpoint: a three-character party at existing levels 3–6 won an ordinary
  walking battle against six wb_00_00 enemies. Native reactions recorded 14 HP
  per enemy, four player HP lost, three healed and a changed native target choice.
  This weak introductory sample showed no punishing spike; it does not certify
  every early formation. No max-level party or forced win was injected.
- The owner performed the requested native save/reload check. The copied Data1
  write/file change is recorded. Original saves are unchanged. This is a protected
  checkpoint test, not a complete new-game creation or long campaign save audit.
- The Undying Watch: accepted through the original party-menu field prompt in
  Tinto Mines, then automatically requested from a legitimate walking encounter.
  Live source evidence matched vg15/vg15_00, Normal, session, table, scene and
  compatible hooks. The native formation became Zombie Unicorn, Zombie, Skeleton
  and Neclordia. A real victory credited **12,236 Potch** (including configured
  3,800 bonus), and one Sacrificial Jizo reached inventory through the native UI.
  There was one currency-credit call and one observed item transfer/increase.
- Native field resumption moved the contract to awaiting report. The owner used
  ordinary travel and an original Escape Talisman, returned to the Suggestion Box
  Guild, reported once and accepted the same contract again. One completion and
  one report are recorded; reporting made no reward grant. This is reacceptance,
  not a claimed second earned Hunt victory. No checkpoint was loaded to skip
  the accepted contract's travel.
- During the return journey, an ordinary victory restored one rune charge, with
  native charge totals 80→81. This is separate from the early fight and Hunt,
  which needed no observed recovery. Further ordinary wins and escapes exercised
  real native formations, damage and controls. Some Mines/route battles inflicted
  substantial damage, so full-party balance is not certified from these samples.

This completes a previously unaccepted non-Hall quest loop without rewriting its
working encounter/reward engine. No broader numerical rebalance was justified.
The player found the route back confusing; Escape Talisman solved it legitimately.
This guide calls out that native exit option, without promising a teleport or
inventing travel links. A startup preview remains separate evidence.
No full new-game creation, whole chapter, mandatory war, current-hash Hall battle,
Nightmare defeat/Retry or campaign-wide fairness claim follows from these checks.

## Prioritized release readiness

A. **Serious blockers:** no demonstrated new save/progression crash has been found
in the scoped checks. This is not proof of an entire campaign. Unsupported scripted
boss changes and protected-harness training stay off. Existing character 26/30
mismatches in older checkpoints remain guarded, not forcibly overwritten.

B. **Gameplay quality:** early/late pacing, route clarity, party variety, reward
economy and rune availability need real playthrough feedback. Keep normal leveling
and optional rewards; do not infer mandatory grinding or impose reward caps from
isolated calculations. Live quest journals now identify the actual build instead
of a stale v1.13 label. Contract text no longer claims an enabled experiment is
necessarily disabled by default.

C. **Optional experiments:** other eight Hunts, scripted Legendary changes,
Nightmare balance and training outside the protected runtime remain separate
opt-ins/unverified scopes. Hall's three unresolved unlocks remain locked.

D. **Lower priority:** additional castle coverage and cosmetic refinements. The
older general battle-history wrapper still carries a legacy v1.13 label; current
Hunt/quest journals, runtime log and exact DLL hash identify this acceptance.
Do not transfer old wrapper labels as current-hash evidence. The
attendant is verified for the Suggestion Box corridor vk07 variant 3; do not
promise it appears in every headquarters layout or at the first castle visit.
Field reissue offers a legitimate Hunt fallback where the audited ground is open.

The next priority is a fresh Normal campaign using the reviewed profile, with
small fixes from actual play. More boss/resource research is not a prerequisite.
No public playable beta, installation ZIP or GitHub Release is produced here.
