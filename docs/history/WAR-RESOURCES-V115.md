# v1.15 Native War Resource Lifecycle

Development version: `1.15.0-native-war-resources.1`

DLL SHA256: `CC25A41F666009DC71D056FE06B5973411DF3F1F2F325C404514965DAF916001`

Verified 8 November 2026. Installed user DLL remains original v1.13. The v1.14
artifact remains preserved separately. This is not a player-facing beta update.

## Observable advance

The original War 1 OPD sub-coroutine used by WarChapter.WarResLoad now completes
inside an isolated resource adapter. This goes beyond v1.14's direct asset request:
it executes the original asynchronous request, resumes the native coroutine,
publishes its result in a private OPD cache, verifies original native lookups,
cleans up, and reacquires the resource through the same path.

Two processes ran the final exact DLL. Each completed two full resource cycles.
Each cycle required 40 original native MoveNext calls, including 19 empty atlas
sub-coroutines. Native GetData returned all 19 source sprite records. Actual native
timing-4 registration and scratch WarChapter cleanup released the acquired handle.
The separately retained castle source/handle and unrelated global caches survived.

No actual tactical battle, WarChapter.Start/Update, chapter dispatch or return to
an active field has occurred. The skirmish launcher is still hard-disabled.

## Ownership implementation

WarResourceOwnership records native identities by resource kind. Ownership of a
reference does not imply ownership of its referenced asset tree.

| Category | Implemented treatment |
|---|---|
| Session acquisition | One ResourceLoader yielded by the actual native War01 coroutine; eligible for exactly-once native unload |
| Owned cache binding | Private war_01 association created by the original coroutine; only that association may be removed |
| Shared game asset | Loaded OPD_RES2 and 19 OPD_DATA records are borrowed; no explicit destruction of these objects or their atlas state |
| Retained field resource | Existing full-signature vk08 MACHIDAT sentinel remains valid while the war reference is released; this is not a live field |
| Unknown/conflicting ownership | Blocks cleanup before callbacks, cache removal or unload; never converted silently to owned |

NativeWarReleaseDomain now validates the actual timing-4 entries before cleanup.
Unrecognized actions, loaders or timing registrations become unknown. An actual
native AddFunc registration with an unclassified callback was rejected before
execution: callback count and native teardown count both remained zero. Unknown
acquisition admission and cleanup were also rejected while retaining the castle
handle. Existing nested/thread/duplicate/fault/once-only safeguards still pass.

The original RemoveResPack cleanup calls DeleteAtlas on every OPD_DATA record and
releases the root asset. It cannot be used blindly on borrowed records. The adapter
instead removes its own private cache association and releases only its acquired
reference through original WarChapter.OnDestroy / ReleaseInfo / ResourceLoader.
Original engine reference counting remains authoritative; no Destroy is issued
against shared source assets. Private cache removal is not campaign flag editing.

## Asynchronous boundary and required context

The unloaded launcher initially routes resources to GSD1. An early isolated trial
therefore requested a GSD1 war key, failed resource loading and stopped at the
source guard. This was a test-context dependency, not a reason to weaken the guard.

The installed binary's inspected SystemObject.Changemode operation stores only
the transient GSD1/GSD2 resource selector. Each bounded native coroutine step now
selects GSD2 and installs the private OPD/release dictionaries, then restores all
three in finally. Nothing remains swapped while waiting for the async completion.
This selector is game identity, not Normal/Hard difficulty or a story flag. No
scene-transition function is called by it in this supported build.

The inspected completion callback updates its own ResourceLoader.Asset and loading
state. It does not publish the OPD cache or depend on the selector after the request
is queued. Cache publication occurs only when the parent coroutine is resumed
inside the guarded scope. Original cache entries, release loader identities,
resource-handle list and asset-cache identities are checked between stages.
Injected failure inside the combined scope also restored cache, selector and
release-table ownership. This is disposable unloaded engine execution, not a
legitimate campaign field-to-war transition.

## Atlas finding and remaining initialization blockers

The public OPD_DATA.LoadAtlas(folder) method actually used by this OPD coroutine
returns an empty iterator in the installed remaster. Both native inspection and
all 19 runtime nested calls confirm that no atlas texture request happens there.
The private path/name/callback overload contains real loading behavior, but is a
different operation; it was not invoked without an established ownership context.
Do not equate OPD cache completion with loaded tactical UI, textures or animations.

WarRes.Load still performs the additional UI/effect work, including original
hierarchy-root-7 initialization and shared rendering/global state. This is outside
the admitted coroutine stack. Any nested operation other than the verified empty
atlas iterator is unknown and blocks the experiment. It is not silently skipped.

Sound bank 61 is also not loaded. Native SeUtility2.loadBank can call ReleaseSE on
existing entries before LoadSE and updates the active bank list. Copying that list
would not restore audio already released by SoundManager. Its release ownership,
completion callbacks and field-audio restoration remain unresolved.

## Field continuation

Targeted inspection confirms MapChapter's start coroutine creates the menu window
and backlog panel, performs fade handling, and resets manager timing/pause state.
MapChapter.Update uses GameStep and SYS_WORK.mcon, can create MACHI with MachiInit,
and then runs MachiMain. That path can re-enter field events and autosave checks.
It must not be called as a harmless resource-restoration helper.

A safe continuation still needs genuine prior MACHI/EVENTCON/asset ownership,
menu/input ownership, rendering/filter and audio restoration, correct chapter
creation state, and proof that no event-bytecode continuation or autosave advances.
The existing field lease stays read-only. No made-up ready flag, EVENTCON or live
return component was added without evidence. A surviving MACHIDAT alone remains
insufficient. The ownership ledger cannot authorize a chapter transition.

## Verification levels

| Level | Result on this DLL |
|---|---|
| Managed/component | All 703,835 checks passed, including 54 new ownership-policy checks |
| Native OPD lifecycle | Two processes, two completed 40-move cycles each, private cache publication/lookups, native cleanup and reacquisition |
| Earlier v1.14 native bridge | Repeated direct acquisition/cleanup with colliding retained-resource ledger passed on the new DLL |
| Full retained native regressions | Two processes passed 3,649 production checks and 7,919 maintenance checks each; second-process diagnostic history verified |
| Rendered tactical battle | Not attempted |
| Live field return or second battle | Not attempted |
| Current-hash human RPG/Hunt acceptance | Not performed; earlier accepted Guild quest remains scoped to original v1.12.2 |

The retained full native regression run includes 560 Normal/Hard admissions and
native selector checks; it is not evidence of live actions or rendered battles.
Headless resource checks do not validate shader rendering or audio playback.
New native diagnostic check totals vary with async wait frames; the observed
resource operations and complete coroutine cycles are the meaningful result.

## Preservation and next boundary

All 37 installed protected files remained byte-identical: 14 saves, 22 configs
and original v1.13 DLL. The v1.14 development artifact also retained its exact hash.
Game-specific registry preferences were restored byte-identically after testing.
The private copied runtime used the existing separate Steam-root isolation guard;
no original campaign save was loaded/copied or written. Processes exited normally
through Application.Quit; no force-close or manual session was required.

No RPG balance, accepted Guild behavior, rewards, character/rune/boss content or
story-war policies changed. No new player setting or JSON migration was added.
No XP/training/persistence or custom arena exists. The beta draft/assets remain
unchanged and unpublished. The new DLL is a separate local development artifact.

The next bounded experiment should establish ownership of the first necessary
WarRes UI/effect dependency and its cleanup callback, without entering chapter
simulation. Shared hierarchy/render/audio owners must be accounted for before
any transition. Complete casualty/story/save write isolation and genuine field
continuation remain prerequisites for an independent battle. Resource readiness
alone does not relax the launch gate.

Built from clean parent `69d25f9e17a1a737d1ee06eb2814d1346b9b62d4` plus the v1.15
implementation tree. Do not rebuild merely to stamp the later documentation
commit. [Sanitized evidence](../publication/development-build-v115.json).
No raw logs, extracted assets, native listings, private helper or saves are uploaded.
