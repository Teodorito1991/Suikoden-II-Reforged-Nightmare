# First Campaign Readiness Report

**Recommendation: GO for the owner's first campaign on unchanged v1.55 + Full
Content, choosing Normal. Stop pre-emptive balance engineering and collect real
campaign feedback.** This is a supported development candidate, not a claim that
the entire campaign or every optional encounter has been player-tested.

Recommended DLL: `1.55.0-story-bosses.1`, SHA256
`C5BE2F48118E205083096599F0C2B2285DF2CE4E2807DA26B5E3EB561497055E`.
Gameplay source: `c46873505f3899230cbf84782873207887645fc8`.
No gameplay/configuration changes or rebuild were justified in this review.

## Challenge verdict

**Substantially harder Normal gameplay is expected overall; not uniformly long or
punishing boss fights.** Most ordinary enemies use replacement HP factors1.20–1.60
and eligible damage1.20–1.40. They are not stacked again with the global defaults.
Full selects 22 source-audited boss policy records, including nine activations in
v1.55. Existing attack legality, finite uses, pressure/recovery windows, status and
physical/magic defence choices provide more than a spreadsheet HP increase.

Character growth, earlier legal rune slots, improved techniques and Unites also
help the player. Their value depends on recruitment, gear and available runes.
Capped conditional Star Bonds are not global damage multipliers or a demonstrated
immortality loop. Level 1–2 victory recovery provides limited field sustain; it is
one total charge per eligible surviving character, not per spell level, not a
mid-battle refill and not guaranteed after scripted bosses. Levels 3–4 remain
resources to prepare. Optional grinding can legitimately overpower encounters.

### Calculated boss-duration watchlist

These are **estimates**, not observed parties or wins. Assume4–6 participants early
and up to6 later, sensible found/shop gear and available weapon upgrades, one
ordinary caster when available, and healing/defence consuming actions. No rare
passive-rune stack, level99 party or optional farming is assumed. Indicative level
brackets are not game requirements or verified recruitment/travel itineraries.
Net party damage per round assumes defence/affinity losses and non-attacking
actions. These budgets are estimates, not exact recruit/gear simulations.

| Encounter / source | Assumed party bracket | HP budget | Assumed net damage/round | Calculated HP-budget rounds |
|---|---|---:|---:|---:|
| Mist Shade | Early, roughlyLv8–12 |800 current form|150–300|3–6|
| Double Head | Early/middle, roughlyLv15–22 |2875|300–650|5–10|
| Celestial Sword | Early/middle, roughlyLv18–25 |2500|300–650|4–9|
| Pest Rat | Middle, roughlyLv22–30 |6240|500–1000|7–13|
| Abomination / first Worm | Middle, roughlyLv28–40 |4500 /4200|700–1400|4–7 /3–6|
| Bone Dragon | Later middle, roughlyLv32–42 |8900|900–1600|6–10|
| Later Lucia variants | Later, if a full party is permitted |1500|1200–2200|1–2|

Formula: ceiling(HP / assumed damage per round). Do not sum Mist Shade's two
form templates into a fabricated1600HP fight. Transformations, native HP handoff,
opening scripts, roster restrictions, missed actions and healing can change these
figures. The tested magic helper uses affinity-adjusted effect + magic − MDEF: a neutral
effect180 + magic60 − MDEF40 gives200. A native physical attack95 versus DEF45 gave
51–58. These are input examples, not guaranteed stats for a level/weapon; critical
and passive-rune stacks are excluded.

**Remaining watch points:** opening tutorial soldiers can still die in one hit;
Mist Shade/short variants may be quick with burst damage; later1500HP Lucia fights
may be brief if their original roster permits a strong party. A short fight alone
does not establish undertuning, and native roster/script constraints cannot be
replaced with Hall party assumptions. No sufficiently demonstrated problem supports
an HP increase now. Original Luca and Beast Rune remain native-controlled; their
Hall acceptance does not certify campaign balance.

## Integration and save review

**No new confirmed integration defect.** The v1.55 guard requires command-0 entry,
current command 0 and matching generation/manager/work/actor/callback ownership.
Scripted, nested and stale contexts retain native choices. Finalizers restore the
scope without suppressing native exceptions. Unknown final-Neclord callback changes
still fall back; they are not a reason to weaken the guard.

Ordinary unowned saves pass through. Training's session fence retires after verified
return or fresh native load; unrelated settings saves are not fenced. Refusals
finish with failure callbacks. Luca temporarily fences saves while owning party
state. Loading resets contracts and does not restore old training arrays over new
campaign records. v1.53 actual save/readback/reload evidence is preserved, not repeated.

Existing configuration is read/validated, missing defaults are created, and
incompatible data is left unchanged. Generation refuses non-empty destinations.
Change opt-outs with the game closed and restart; do not expect mid-session changes
to undo already initialized character data.

## Full Content and normal access

The generated profile was checked for all core character/rune/combat modules,
intended role pacing, one charge limited to levels1–2, all 22 story policy records,
ten enabled Hunts,22 selected Original Hall definitions, four training drills,
supported Veteran selection and original story-war enhancements. Spoiler Mode,
Nightmare tiers, experimental defeat redirect, developer/debug/selftest flags,
legacy scripted-stat overrides and the retired arena are OFF. Choose Normal in
the game; configuration does not secretly change the game's difficulty choice.

Normal Guild, Hunt, Hall and training use the attendant and original talk/menu
controls. Accepted Hunts automatically request eligible walking encounters.
Training's normal-build return contract is selected; no private transition harness
is required. Optional legacy developer menus/testing shortcuts are not required.

Known optional limits, not campaign blockers:
- Attendant insertion supports the verified Suggestion Box room `vk07`, variant3.
  Earlier/other castle variants are not promised; no coordinates or flags are guessed.
- Contracts reset on load/restart. Reaccept through the Guild; report-back never
  pays a second reward. Some Hunts retain native-only rather than full-quest evidence.
- All 22 Hall definitions are selected, but story availability is still enforced.
  Sierra, first Worm and second Harpy lack verified automatic witnesses and remain
  hidden unless the player deliberately selects the warned Spoiler Mode option.
  Several other entries are labeled Bonus Echoes after a verified milestone,
  not an individual-clear claim. No story flag is changed to reveal them.
- Optional Luca defeat/interruption and some drill/tier paths retain incomplete
  current-build gameplay evidence. Commander Rout Standard save/readback/reload
  acceptance remains v1.53. Training defeat return is OFF; native Game Over applies.

## Verification and decision

Fresh managed regressions, Full/Fresh configuration tests and a comprehensive
check of the actual generated Full profile passed. The recommended DLL hash was
rechecked. Exact-C5 native component and graphics-startup results from v1.55 were
reviewed/reused because gameplay code/configuration is unchanged; no new native
execution, earned battle, full campaign or save write is claimed in this review.
All 14 original saves and accepted DLL hashes remain intact; Steam and personal
configuration were not changed. No new version, installer, beta or release.

**Ready after installing the recommended DLL and reviewed Full profile.** The
Steam installation is intentionally still unchanged. Do not begin another
infrastructure milestone or replay every boss to postpone play. Report concrete
campaign problems with party/equipment and logs, then fix demonstrated issues.
See [the concise setup guide](FIRST-CAMPAIGN-SETUP.md) and
[exact story coverage](STORY-BOSS-COVERAGE-V155.md).
