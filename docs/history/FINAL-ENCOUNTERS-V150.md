# v1.50 — Final Encounters

Development work, not a published playable release. The installed Steam plugin,
personal configuration, original saves, v1.49 artifact and v1.48 fallback are preserved.
No full campaign was started for this milestone.

## Implemented encounter families

The catalogue now contains 21 Original Echo definitions and 21 optional Nightmare
profiles. Existing nineteen IDs and options keep their order and behavior.

| New entry | Original source | Native mechanics retained | Normal access |
|---|---|---|---|
| Worm & Poison Moth | `boss_imo2`, pack `imo2`, scene `bgf1` | Original opening, transformation phase callbacks, command and main controller; both original monster templates | Read-only original ending witness `event_flag[0x41] & 0x40` |
| Beast Rune Incarnation | `boss_las`, pack `las`, scene `bgx0` | Five active body parts, extra native controller template, original opening, phase, command and main roles | Read-only original ending witness `event_flag[0x56] & 0x02` |

Both copies deliberately exclude their original story ending. They use the existing
ordinary request (`battle_no == -1`), owned formation/resource scope, native damage,
loot, terminal handlers and guarded Guild return. Neither sets an unlock flag.

The first live final-boss attempt exposed an extra-texture dependency: the stock
`BattleDataLoad` only schedules `LoadExTexture` for story battle index 52. With
ordinary Hall index -1, `las_field_sub_func00` dereferenced the missing sprite.
`HallExtraTexture` now adds the same original loader and native coroutine wait only
for the admitted final encounter. The loader retains its own timing-3 release
registration. No field callback is suppressed and no boss index is spoofed.
The two normal access witnesses were identified by static inspection of the original
ending functions, not by playing those story battles. A checkpoint without them
shows `???`; development test availability is separate and never edits flags.

Nightmare copies all required templates once at 120% HP and applies the existing
reaction damage layer once at 1.08× configured Original. The final controller's
low auxiliary level is not presented as the encounter's recommended party level.
The optional tactical-selector overlay is excluded for these two IDs: original
scripted controllers retain their action authority. Existing nineteen profiles
are preserved. The Original Worm/Moth transformation and victory/return passed on final v1.50.2;
new Nightmare variants remain unplayed.

A Nightmare Worm/Moth offers Stone of Magic Protection; Nightmare Beast Rune offers
Stone of Magic. Each uses the existing 25% conditional fallback roll, original loot
priority and native inventory/currency limits. Original Echo does not grant that
Nightmare bonus. Reporting or changing tier never pays a reward.

## Luca: reusable foundation, not a playable gauntlet

`HallGauntletRules` implements ordered native-receipt and disposal boundaries for
`boss_luc1` → `boss_luc2` → `boss_luc3`, actual HP carry without rescaling, rejection
of duplicate/out-of-order receipts, failure termination and a final-only bonus
boundary. These state tests use synthetic receipts; they are not earned battles.

`HallLucaHpView` implements a synchronous, thread/epoch/battle-owner-bound native
`OldSrcBase.game_work` read view with an owned `GAME_DATA` clone. Only this view has
private `luka_hp`; the real campaign singleton and its data record are not replaced.
Other data is explicitly borrowed or flag-copied. Scope cleanup restores the ordinary
getter path. The dedicated unloaded graphics audit called the real getter for all
three original Luca records and verified independent HP reads and original-data
preservation. It did not execute original openings, actor playback or a Luca fight.

The unresolved boundary is authentic preparation/selection of three separate player
parties and safe restoration between stages. Native enemy counts (6/4/1) are not
player party sizes. The original event party selector consumes a legitimate event
context; an invented interpreter or reuse of one party is not an authentic solution.
Original stage openings and manager continuations also require that admitted scene.
No Luca menu row, public tactics option or gauntlet launch is exposed. Campaign
`luka_hp`, original callbacks and the real story sequence remain untouched.

## Army Training ownership contract

`EnableTrainingNormalBuild` is a separate, default-off experimental option. It requires
`EnableTrainingWar`, training defeat return, non-batch runtime and verified original
War 1 definitions. The legacy protected transition harness remains supported.
This is not a removal of its old gate without replacement.

The new contract retains all five original flag-array owners, installs owned value
copies before fade/stock field handover, and restores original references only at the
same session's verified post-teardown `MachiMain` step-48 prefix. It requires matching
campaign/field/epoch identity and unchanged original array bytes. Unexpected ownership
rejects guessed restoration. Native callbacks still execute against the private flags.
Existing training-only defeat routing, castle position handling and four drills remain.

The `GSD2SaveData.SaveFile` hook refuses requests while a normal-contract session is active.
This hook is not a universal interception of every native persistence path. After
return, existing `game_data`, `war_data`, `chara_data` and `party_data` differences are
checked; unexpected changes latch training unavailable and block saving for that
process. Restart and reload the last legitimate save before saving again. This is a
fail-closed experimental contract, not a generic save-restoration system or proof
that every Steam/achievement side effect is contained.

Commander XP remains unimplemented. The verified native stat-copy seam does not
establish a documented, campaign-owned persistent XP field. No unknown save bytes,
reused-slot sidecar identity, temporary counters advertised as persistence, or live
commander bonuses were added.

## Deliberate exclusions

Earlier invulnerable Neclord and second Rowd capture/loss sources remain excluded;
no scripts were stripped to invent normal victories. Mirrored Golden Wolf and short
Lucia duplicates do not add distinct rows. Standalone `boss_imo3` has its own scripted
commands and is not offered as a misleading extra Moth. Luca's three records remain
an explicitly blocked family, rather than three ordinary rematches.

## Evidence and remaining gates

- Release build and managed/component checks passed.
- Combined native production audit passed for all 21 source/copy/setup paths. It
  did not execute the new controllers through a complete battle scene.
- Dedicated native Luca getter audit passed for all three loaded records; original
  opening/ending, story transition and gauntlet completion were not executed.
- Real encounter and normal-training results are recorded in the milestone's
  sanitized verification manifest once observed. Missing evidence is not PASS.
- Historical earned victories, defeats, rewards and repeats retain their original
  DLL hashes. They are not silently assigned to this development DLL.
- Whole-campaign balance, every Nightmare variation and normal installation-side
  behavior remain experimental. No installable beta or release was published.

## Observed gameplay

Final `1.50.0-final-encounters.2` / SHA256
`EAF67EB039F569149C9AB811AE770C82EC48DCB6D466AD070458C39D1D5338DF`:
owner used one ordinary attacker until the Worm transformed; the native controller
reported transformation at turn3, progressed through its original main delegates,
and reached an earned victory. Guild-position return, all five protected flag
arrays, serialized war snapshot, selected progression markers and owned resource
teardown passed. Observed Potch credit was0; no nonzero reward or Nightmare stone
receipt is claimed for this Original fight. No save operation occurred during that Hall loop.

Intermediate `.1` / SHA256
`259139F596B015CA51F46DFA62FCD30862D352D65284EA481560A4D8A37BF192`:
final multipart boss rendered and executed native combat after the texture fix.
The owner lost; native Game Over and cleanup passed, with zero reward, unchanged
protected state and no save. No earned final-boss victory or Guild-return acceptance
is claimed, on either binary. The test party was not sufficiently prepared. This
optional encounter stays disabled in Full Content Preview pending its victory gate.

Both new live checks used explicit copied-runtime test availability, not fabricated
story flags. The checkpoint correctly displayed `???` under normal gating. Original
stats were not changed to manufacture a win. Historical boss/quest tests remain
associated with their actual hashes.

The final-hash separate-desktop preview produced33 captures and native asset
load/release1/1. A representative fifth-page capture is readable; another still
omits portions of fixed/dynamic text. This is an unresolved intermittent capture
limitation, not a claim of flawless visual acceptance or an established live-renderer
root cause. No broad font/UI replacement was shipped to hide that uncertainty.

## Normal-build training result and autosave limitation

The owner completed Commander Rout on the exact final DLL and returned with normal
movement. The legacy stock-transition harness was absent; the new normal-build
contract admitted the session. Native result1/manager result1, matching castle
position, restored five flags and repeat eligibility passed. Full `war_data`,
`chara_data` and `party_data` comparison at the return prefix had no differences.
Only return coordinates and elapsed play/event timers changed in `game_data`.
The once-only reward classified750 Potch earned; actual credit0 at the purse cap.

The persistence witness subsequently observed one **native autosave in the copied
runtime**, during map initialization at step14, after the verified step48 restoration.
The `GSD2SaveData.SaveFile` counter was0 because this path called `SystemSave.Save`
directly. This is not a zero-save test and that hook must not be advertised as a
universal save fence. No original save was written.

A read-only comparison then decoded the copied autosave using the existing offline
decoder. Its baseline was matched by the exact loaded checkpoint file hash, not by
slot or campaign name. All five saved flag arrays, `war_data` and `party_data` match
that baseline. Saved character HP loss totals224, exactly the recorded Worm damage.
Other variable-character differences are EXP and the native finishing-blow counter;
fixed-data differences correspond to existing party-profile initialization.
Map history/position, elapsed timers, message speed and three shop timer/event fields
also differ across the whole loaded session. Their timing is not isolated to training;
this is not a claim of byte-identical persistence or a complete mutable-state graph.

The observed native autosave therefore did not persist changed story flags, war
progress or party inventory. It does not require moving the accepted Guild NPC.
However, the GSD2-only fence is incomplete for direct native writes, particularly an
unobserved faulted return. Other drills, defeat, repeat entry and ordinary-install
side effects under the new flag-owner contract remain unverified. The normal-build
option and training remain off in generated Full Content Preview. Earlier four-drill
proof retains its historical protected-harness scope. No commander XP is implemented.
