# v1.38 Nightmare Campaign

Build `1.38.0-nightmare-campaign.1`

DLL SHA256 `614B875B89504D8DB06AF96D8AA920A325BD420B930CA1A08371C17AC29DFC50`

## Recommended fresh campaign

Choose the original game's **Normal** difficulty. Use the repository's
`config.example.cfg` and `config/ReforgedNightmare` examples as references.
Merge selected keys into personal settings. Existing files and explicit
contract opt-outs retain priority.

| System | Recommended settings and scope |
|---|---|
| RPG overhaul | Character growth, rune slots, spells, Unites, physical techniques, formation remixes and ordinary AI on. Keep combat pacing enabled for the new recovery window. |
| Victory and runes | Keep one lowest-depleted charge per eligible survivor. Healing, original EXP, money caps and regular loot use the existing production paths. |
| Bosses | Keep the verified policy families at their example defaults. Leave `EnableMajorBosses` off for the older Lucia/final Neclord opt-ins. Luca's tactics remain deferred. |
| Hunt Guild | Enable the attendant, automatic requests and all ten contracts. Set `EnableVictoryItems=true` for optional fallback items and `EnableFieldReissue=true` for local acceptance. |
| War | Keep original story-war tactics and maneuvers on. The four bonus Training drills stay in the protected copy. Keep self-tests and debug protection off. |

## Guild continuity

Open the regular party menu at an eligible hunting ground. If the local
contract prompt appears, press F6, or hold L1 and press R1. This explicitly
accepts a fresh contract for the current native party. It requires Normal,
the owned main menu and active field, an eligible walking variant, its exact
encounter table and source signatures, and an enabled contract. It refuses to
replace an accepted hunt or an outstanding report. Close the menu and walk.
The original walking hook can request the encounter using the same rules as
castle acceptance.

The native save record has a schema version and a timestamp, but no campaign
or branch identity that survives copying, overwriting and reloading a slot.
The managed load hook receives JSON, with no slot identity that can be paired
with a verified completed write. Contracts and unreported completions remain
session-only. The new local action exchanges no money or items, and
restores no receipt. Only a new eligible victory can earn a reward.

The Guild orders contracts by tier and describes the region's ordinary
travel requirements. Open is contract availability. The original story
controls regional access, including gates and scripted travel. No new story
flags set prerequisites. The clerk still uses `vk07` event variant 3.
Variants 4 and 5 share the background but have distinct, much smaller event
sets and do not establish the verified Suggestion Box interaction.

## Army Training and rewards

The menu distinguishes a disabled Training option from the missing
protected-runtime guard. Ending result, field owner, teardown, coordinates,
original victory condition and original purse handling still control return
and reward. A committed drill keeps its teardown path even if the admission
flag is subsequently cleared. The deployment limit remains because the
flag restore and campaign-data diff happen only after native war code has
run. The comparison can detect some unexpected persistent writes after the
fact and disable repeat entry. It cannot undo unexpected casualties or
contain an unverified early save. Nothing was changed in native story-war
admission. Training stays at 750/1,500 Potch for its qualifying victories.

Hunts retain the ten-contract reward ladder, including permanent growth
stones and optional higher-tier orbs. The original loot slot, bag, purse,
victory and report conditions still apply. No new grant path or reward cap
was added. [Catalogue and native reward details](GRAND-GUILD-V137.md).

## Pacing and evidence

The existing ordinary AI discounted a status only as a fraction of useful
targets, which could leave area Sleep attractive into a party already
affected. The new preference is narrower. When at least half the eligible
party is asleep, ordinary area Sleep loses 36 score if combat pacing is on.
An opening against an awake party, single-target status, legal fallback moves, Hunt AI, boss
AI and original probability, target and damage rules are preserved.

The 36 managed projections cross early, middle and late party samples
with balanced, physical, magical and imperfect compositions. They use
audited encounter and move data, edited slot thresholds and explicit
equipment/HP/charge assumptions. Native setup also verified all ten Hunt
reward paths. The projections show potential pressure and give a bounded
Nightmare Sleep case. They do not model recruitment access exactly,
actual spell casts, equipment procs, levels earned, or calibrated victory
rates. No campaign-wide numerical rebalance was justified.

- Release build, managed tests, original-engine fixture audit and seven-war
  native tactics checks passed.
- Local reissue fixtures rejected Hard, a submenu, a missing walking source
  and replacement of an active quest. They bound a fresh Normal contract,
  observed an eligible walking request and exercised the same reset routine
  the native load hook uses.
- The separate-desktop preview rendered eleven native-style screens, including
  the local contract prompt. It stayed at the launcher.
- No human session, live contract reissue, natural battle, complete campaign,
  manual save/load or new Training drill was observed on this build.
