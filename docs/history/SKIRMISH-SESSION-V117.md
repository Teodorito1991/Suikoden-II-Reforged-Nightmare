# v1.17 Integrated Native Session and GO/NO-GO

Development version: `1.17.0-native-skirmish-session.1`

DLL SHA256: `DC44899D9F3A8182BE95686A831CD351C3C9C2C077ABEFB5A9C0E173DC8E8C97`

Separate development artifact, 8 November 2026. Installed v1.13, all original saves
and configurations, accepted Castle Hunt behavior and private beta are preserved.

## Observable implementation

NativeSkirmishSession now owns a single exclusive resource lifecycle. It reuses
NativeWarOpdStage, NativeWarUiLease, NativeWarReleaseDomain and their ownership
ledgers rather than implementing replacement loaders or cleanup. The prior header
source audit is shared by both the retained UI probe and the integrated owner.

Each session has a unique identity, main-thread ownership and explicit phases:
Created -> field-asset sentinel -> original OPD coroutine -> original header factory
-> Ready -> header destruction -> OPD/acquisition release -> Disposed.
A concurrent owner is rejected. Idempotent stop/disposal calls cannot repeat a
native unload. A pending stop drains the admitted native loader/coroutine to its
verified cleanup boundary; a timeout or uncertain failure blocks the session and
retains the ownership lock until the disposable process exits normally. It never
tries unclassified callbacks to recover. This is not yet a live-game abort API.

The castle sentinel is an acquired MACHIDAT resource, NOT a retained MapChapter.
There is no fabricated EVENTCON. Each coroutine step restores original resource
routing, OPD associations and release tables before yielding. Header root 7 is
private during bounded native steps. Between steps, the integrated owner checks
the original OPD cache contents/identity, release registrations, global handle/cache
lists, hierarchy entries and shared WarRes references. Font registry and shared
font/material identities must be restored after header destruction.

Cleanup order is deliberate: destroy only owned header instances and await native
font/tween callbacks; release the separately owned header audit reference; verify
factory reference balance; remove the private OPD association; run its original
native release domain; verify the sentinel survived; release that final acquisition.
Unknown/conflicting ownership rejects disposal before guessed destructive work.
The child domains remain distinct internally but have one session-level sequencing
and exclusive owner. No global war cleanup or shared asset-tree destruction is used.

## Genuine integrated native evidence

Two isolated processes each completed TWO combined OPD-plus-header sessions and a
third interrupted OPD session. Each complete session executed 40 original OPD
coroutine moves and initialized the original header with eleven native tweens and
three private font materials. Native cleanup released the tracked acquisitions,
removed the private cache association, destroyed those materials, deactivated the
tweens and restored the font callback registry. A new independent owner then repeated
the sequence. The interrupted acquisition drained and disposed without a header.
Concurrent creation was rejected, and repeated stop/step calls after disposal were
harmless. No tracked leak or duplicate release was observed.

This is original-engine execution, not fabricated battle/completion receipts.
The Canvas remains disabled. No rendered tactical UI, WarChapter.Start/Update,
chapter dispatch, original story command, earned outcome or live field return ran.
The resource envelope covers OPD data and the header only, not the full war scene.

The retained managed suite passed. Two production native component runs passed,
including native combat/admission/selector checks and cross-process history on the
second run. Retained OPD and direct asset adapters also passed on this exact DLL.
These checks do not certify a campaign playthrough or the current-hash Hunt Guild.

Known isolated-runtime limitation: the graphics run logs a TMP_SubMeshUI shutdown
NullReferenceException and a JobTempAlloc warning. A same-hash no-session graphics
baseline reproduces them. Bounded integrated lifecycle observation records zero
Unity errors/exceptions, but no claim of a globally leak-free/error-free application
is made. Only the listed resource/registry/reference lifecycles are established.

## Real field ownership: evidence and missing proof

Bounded read-only native inspection confirmed that MapChapter is a ChapterBase,
not a MonoBehaviour that can be suspended through enabled=false. Chapter.Update
owns coroutine stopping, old chapter destruction, replacement construction and
virtual update dispatch. Keeping an object reference does not preserve its active
continuation. RequestBack restores a type request, not a suspended chapter stack.

The GSD2 MapChapter.Update path reaches MACHICON.MachiInit and MachiMain. Map start
creates menu/backlog windows and fades the UI; original event and autosave machinery
can run again on reconstruction. AutoSaveCheck directly reaches UISaveLoad2.Save.
Shared root 7, field render/input owners, menus, event interpreter state, coroutine
ownership and scene audio therefore need a coordinated suspension/restore boundary.
Bank loading can release old sound banks before loading replacements; restoring a
static list alone cannot restore audio already released.

No genuine loaded field was paused or resumed in this milestone. Existing adapter
guards deliberately reject an active MapChapter. Neither a retained-field host nor
field reconstruction is verified safe; reconstruction currently has more known
initialization/event side effects, not an evidenced safety advantage. The resource
sentinel must not be described as genuine field retention.

## Campaign-write boundary

Inspection of DriveUnitDestroy identified TWO direct G2_cha_flag calls with mode33,
using the actual event character number. Its inspected native branch computes
(old | 0x90) & 0xDD. The existing disposable singleton/flag fork now rehearses this
exact primitive: scratch 0x27 becomes 0x95 while the original source remains 0x80.
This is actual native flag mutation on isolated arrays, NOT casualty processing.
No casualty RNG, selection, recruitment, permanent party update or war dialogue ran.

CheckDead itself consumes native RNG and writes static casualty work. warmain
reaches native battle/demo processing and global UI/effect cleanup. WarChapter
ending has a helper and an inlined write path, in addition to existing initializer
character writes. These facts rule out treating five copied flag arrays as a full
sandbox. Full GAME_WORK/war/chara/static state, recruitment and casualty closure,
RNG ownership, interpreter callbacks and save/autosave containment remain incomplete.
No real campaign flags, original saves or live casualty state were modified.

Native fingerprints and narrowly confirmed edges are recorded in
[the sanitized boundary record](../publication/native-skirmish-boundaries-v117.json).
Raw disassembly, private logs and proprietary resources are not distributed.

## GO/NO-GO decision

| Boundary | Classification | Decision and exact gap |
|---|---|---|
| A. Independent resource ownership/release | Verified within the OPD/header envelope | Two complete independent sessions per process, clean tracked resource disposal and interrupted loading. Full war resource ownership is not established. |
| B. Native initialization without story bytecode | Partially verified | Genuine OPD/header initialization needs no interpreter. Active WarChapter initialization, tactical UI/effects, character/unit state and audio are still outside the owned envelope. |
| C. Genuine field retention/restoration | Unresolved; blocks live execution | No loaded MapChapter suspension/resumption, input/menu/audio restoration or safe second field-to-war transition has been demonstrated. Native dispatch is destructive, not a stack. |
| D. Campaign/casualty/autosave isolation | Partially verified primitives; full containment unresolved | Selected writes are isolated on scratch arrays. Static casualty/RNG and complete mutable owner graph are not closed; save blocking alone is inadequate. |
| E. Next safe integrated experiment | Defined, prerequisites unresolved | Observe a normally loaded protected field, identify all executing chapter/field owners, then prove a reversible resource-only session under that field with unchanged interpreter position, no autosave and normal continuation. Do not start tactical war until suspension and write ownership are proven. |

**NO-GO for an active independent war or player-facing launcher.** This is an
engineering safety decision based on missing ownership evidence, not a declaration
that original-engine skirmishes are impossible. The integrated resource foundation
is now useful and proven; further standalone prefab experiments should stop.

Recommendation: change the next investigation to ONE bounded real-field suspension
and continuation proof, with no war simulation. It needs a legitimately loaded
protected field, stable MACHI/EVENTCON and manager/coroutine identities, rendering/
input/menu/audio inventory and before/after interpreter/save observations. Only
then combine this existing envelope beneath that retained field. Do not consume
story bytecode or rebuild field state by guessing. If that boundary cannot be
closed without broad reconstruction, temporarily defer skirmishes and ship the
working RPG/Hunt Guild/story-war overhaul. Do not spend successive milestones
accumulating unrelated resource probes.

The launcher stays unconditionally disabled. No XP, persistence, commander training,
new NPC/menu, custom arena or balance changes were added. No human session requested.
