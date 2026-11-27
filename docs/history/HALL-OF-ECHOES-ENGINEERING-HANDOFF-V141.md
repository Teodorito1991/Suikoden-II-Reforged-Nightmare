# Hall of Echoes engineering handoff — v1.41 investigation

Baseline is v1.40 commit `c4d19df78f5893271df011894c19a5b817cc4414`.
This is an untagged investigation. The staged development candidate for
gameplay remains v1.40. The exact DLL used to verify this source was
`8F838B25E0E1B4569462EE5F9D07D84BE8DB15D33D1B9955102B80DA22DC5EA4`
(SHA256, temporary test build).

## Decisions and evidence

| Question | State | Result |
|---|---|---|
| Guild entry | **IMPLEMENTED**, presentation preview | Hall of Echoes shows an unavailable information page. Its navigation has no start, unlock or guest action. |
| Original request and boss source | **NATIVE-FIXTURE VERIFIED** | Two separate scratch field owners successfully issued the native ordinary request and selected a detached Pest Rat party through original data setup. |
| Rendered RPG chapter entry | **BLOCKED** | The request recorder and data setup never ran under a genuinely loaded RPG field. No map tick, chapter dispatch, field setup or camera continuation ran. |
| Victory, defeat, escape and repeat return | **BLOCKED** | No combat was played. An owned field resume and its persistence boundary have yet to be witnessed for an Echo. |
| Jowy / McDohl | **NATIVE-FIXTURE VERIFIED** for record and alias inspection; **BLOCKED** for substitution | Both original fixed records exist. The native variable-data getter returns the campaign record itself. No actor was substituted. |
| Unlocks and growth | Design only | Account-wide unlocks could live outside the game save. Guest progression should start battle-local. |

The licensed copied runtime passed the managed regression suite, native
production audit and graphical Guild preview. The launcher preview is the
only rendered result here. Save contents and private captures stay local.

## Normal RPG entry and return

These are Suikoden II signatures for the supported installed build. The
other game in the remaster also has a class named BattleChapter. The
current interop references and supported binary gate identify the Suikoden
II types. Call relationships below come from static inspection unless
marked as executed.

1. **Request.** `EVENTCON.EventBatlleCheck()` is the original walking check.
   It owns eligibility and RNG. `EVENTCON.ECEventBattleGo(sbyte cno)` also
   tail-calls `EVENTCON.EventBatlleSet(byte _bno, byte btyp)` after consuming
   original event command data. Its tail-call does not appear in a
   direct-call-only xref search. The story command is unsuitable as a Guild
   trigger because it owns an event interpreter and advances that script.
2. **Write boundary.** The ordinary type-0 branch of
   `EventBatlleSet(1, 0)` sets the owning `MACHICON.btltyp` to 0,
   `MACHICON._mstep` to 40, sets `SYS_WORK.battle_no` to −1 and takes the
   source pointer from the requesting `EVENTCON.mdat.battle_dat[1]`.
   This executed against scratch map and event owners. It stages a
   transition. It neither starts the battle nor creates a return token.
   The special 0xF0 source path and type-2 path were outside the experiment.
3. **Field and chapter ownership.** Normal `MapChapter.Update()` calls
   `MACHICON.MachiMain()`. That function's battle branch uses the live
   field backdrop, event/player display and overlay capture, moves from
   step 40 toward 41 and 42, and performs sound/memory/object transitions.
   `GRChapterManager.G2_Mode_Change()` owns the chapter routing. The test
   stopped before the first field tick following the request. A constructed
   `MACHICON` with suitable data references supplied the setter's inputs
   but lacks the genuine scene, input, renderer and sound ownership for the
   next tick.
4. **Battle resources.** `BattleChapter.Start()` returns an enumerator.
   Its coroutine obtains a scene `Field`, calls
   `G2_PLY.ReLoadPlayerData()`, creates the manager through
   `BattleManager.Create(Action<BattleGameFunc> onChangeGameFunc)`, and
   runs `BattleManager.Init(Field field)`. The manager uses
   `BattleDataSetup()`, `BattleEffectLoad()`, `BattleDataLoad()`,
   `BattleMainInit()` and `Field.Setup(BATTLE_WORK work)`.
   Whole-battle evidence would have to cover the original background,
   classic/HD monster appearance, effects and original animation resources,
   and show the field's work is the manager's. The OPD animation records
   examined here are one dependency, not the scene's full resource lease.
5. **Action and outcome.** The original manager creates and traverses action
   and reaction queues. `Field.CalcCameraData()` assumes the manager's work
   and queue cursor. Terminal handling includes `WinEnd()`, `NotWinEnd()`,
   `BattleEnd()`, `BattleEndWait()` and the
   `battle_exit()` / `battle_exit99()` reward path. Original EXP, money,
   loot and party-state processing are separate from story-boss callbacks.
   Zero callback indices alone cannot prove campaign state stays intact.
6. **Teardown and field return.** The original destruction path is
   `BattleChapter.OnDestroy()` → `UIBattleManager.DestroyBattleUI()`
   and `BattleManager.Release()`. Release unloads registered resources,
   destroys its objects and calls `BATTLE_WORK.PreRelease()`. The map
   branch calls `MACHICON.OnEndBattle()`, which assumes its native field
   and event owners. To approve Echoes, see the exact chapter teardown,
   original return path, same map and entry position, functioning
   movement/menus and safe resource reuse on a second entry. For defeat,
   first observe the actual result routing. A genuine test must establish
   a safe return without skipping terminal cleanup or writing a fake win.

Relevant prior evidence is in [Luca ownership](LUCA-BATTLE-CONTEXT-V125.md).
Even an authentic queue allocation does not demonstrate
`BattleMakeData` against a scene-owned `Field`.

## Where the current mod can intercept

- [HuntLaunchProof.cs](../src/NightmareRebalance/HuntLaunchProof.cs) observes
  the walking call and passes the proven source pointer to setup.
- [HuntAccess.cs](../src/NightmareRebalance/HuntAccess.cs) substitutes the
  selector result only for an already authorised walking request.
  A contract and an RNG postfix provide no transition trigger from idle.
- [CastleGuildRuntime.cs](../src/NightmareRebalance/CastleGuildRuntime.cs)
  already checks the paired event/map owners, sees original input first,
  and closes when context changes. A future launch must take place after
  releasing its modal input ownership, with that frame's native field
  context checked again. Choosing a row does not itself convey ownership.
- [HuntSessionRuntime.cs](../src/NightmareRebalance/HuntSessionRuntime.cs)
  observes owned outcome and later map updates. A map tick alone cannot
  prove movement, same position or the absence of persistent writes.
- [HallEchoesProbe.cs](../src/NightmareRebalance/HallEchoesProbe.cs)
  is a test-only caller on disposable native objects. It adds no field
  or campaign hook.

The historical [field continuation decision](FIELD-CONTINUATION-V118.md)
tested a war-related retention boundary. The later
[War 1 training trace](SKIRMISH-TRANSITION-V129.md) established a different
original handover, starting at field step 45 with separate return checks.
Those findings help locate ownership witnesses. Neither is an RPG entry
or defeat harness. The protected launcher guard used here prohibits
campaign loads. The previous purpose-built training environment and its
historical results cannot authorize an RPG encounter in this runtime.

## Pest Rat experiment

The existing audit identifies `boss_rat / boss_rat_00`, one original
monster record and a start/end callback pair with no native phase, main or
command callbacks. Double Head has start, phase, main and ending behavior.
Its discretionary selector existing in the mod does not prove its
original opening or phase assumptions can be removed.

The probe checks the Pest Rat asset and formation against the existing
audit, including placement, callback indices and move signature. It
constructs a separate table with one detached party, neutral flags, no
party loot, six zero callback indices and an empty command array. The
loaded boss stat and asset objects stay read-only. The original native
setup creates its own monster record, and only that copy receives the
25% HP increase. This increase is a lab input, not a recommended early
rematch tuning target.

| Witness | Observed |
|---|---|
| Source and request | Original asset loaded and original request executed twice, using new scratch MACHI, event and battle work on each pass |
| Setup | Explicit one-based `SYS_WORK.yobi[0] = 1` selected the detached callback-free party |
| Boss stats | Original 5,200 HP; disposable record and fixture actor 6,500 HP |
| Resources and legal moves | 28 original sprite animation entries; 48 native selector calls, covering legal modes 0, 1 and 2 |
| Cleanup | Original boss source, global game/work identities and RNG verified after scratch cleanup |
| Historical regression | Production audits for the existing battle modules passed on the test DLL |

**Important negative result.** The normal random branch for one party
calls `OldSrcBase.get_rand_maxlimit(0)`. In two attempts with our patched
runtime, that setup reached a native breakpoint in the selector routine
and the copied process exited. The teardown shell restored the copied
DLL and configuration. Its relationship to the patched short/zero
branch is a hypothesis, so the failure is not generalized to stock
Suikoden II. The final fixture used the native explicit one-based
selector, leaving the live RNG patch alone. A valid explicit selection
can remain in `yobi[0]`; the probe resets that scratch byte before the
second request. A live adapter must prove the corresponding lifetime
with proper native ownership and reject a stale or already pending request.

To reproduce, use the approved startup-only copied runtime, enable the
existing combat self-test setting and run the ordinary production audit
with `--reforged-private-startup-smoke --reforged-production-audit
--reforged-hall-echoes-probe --reforged-test-exit -batchmode -nographics`.
The normal game path does not run this fixture. Read the `HallEchoes`
and `EchoGuestOwnership` fields of the local production-audit report.
No source game assets or raw native trace are needed in the repository.

## Guest ownership and persistence

The loaded fixed character records exist for Jowy (80) and McDohl (82).
For each ID the original `G2_SYS.G2_varia_dat(int chano)` returned the
exact pointer installed in a disposable `CHARA_DATA.c_varia_dat` entry.
It did not return a battle-owned copy. This is evidence about aliasing,
not guest creation, fixed rune access or command eligibility.

Static `BattleManager.BattleCharaInit()` calls
`BattleManager.PlayerInit()`. The player constructor consumes the party
count and IDs, original graphic and size lookups, and the passed
`C_VARIA_DAT` reference. `BattleChapter.Start()` has already called
`ReLoadPlayerData()` by then. Swapping an ID in the finished actor or
`BATTLE_WORK.c_varia_dat` cannot retroactively make models, ability
definitions, formation and roster-dependent resource loads agree. It also
leaves result code that consults original party or global character data
unaddressed. Swapping the saved party during only `PlayerInit` is too
late. Keeping it swapped across scene and field handlers risks original
story, save and recruitment reads.

A credible trial needs a battle-local roster and deep copy of mutable
character state from before resource lookup through result handling.
Verify all required consumers use that view, including legal fixed runes
and equipment, original animations, HP/status/charges, steals, rewards,
survivor regeneration and return. Route guest writes exclusively to
discarded guest records. Refuse a duplicate native ID, forced party,
script, unsupported original game mode or ambiguous lookup domain. Keep
guest activation off in ordinary battles until one controlled Arena guest
passes entry, action, KO/escape, return, reload and another battle.

**Unlocks.** Offer an optional, explicitly account-wide local mod profile.
Use a small versioned schema with allowlisted unlock IDs, strict validation
and atomic same-directory replacement. A write should follow one earned,
verified Arena victory receipt and safe field restoration. Test duplicates,
failed writes and malformed files. With the option off, any future
unlocks can remain session-only. No store is implemented at this stage.

**Growth.** Do not call an account-wide level a campaign level. Use
battle-local levels derived deterministically from the current native
party if and when a guest lease works. Their rune use and stats belong
to that one battle, and XP is not committed. The native load hook
`GSD2SaveData.LoadDataCore(string json, bool isTmpPosReload)` lacks a
reliable campaign or branch key, as documented in
[Nightmare Campaign](NIGHTMARE-CAMPAIGN-V138.md). Real recruitment and
reselection of McDohl remain a separate original game path. Plan both
guests as accessible introductory rewards. Advanced rematches follow
the unlocks.

## Two bounded next experiments

1. **Stock RPG ownership trace, then a scoped Guild request.** Establish a
   separately approved campaign-capable copy, with saves, imports, cloud
   and achievements isolated and a legitimate loaded Normal field.
   First capture one ordinary rendered battle without changing results.
   Observe request, field and event identity, chapter dispatch, actual
   Field/BATTLE_WORK binding, render/resource ownership, native terminal
   callbacks, field actor position and resumed input. Record save intents
   and persistent data fingerprints, including party, flags, HP, items,
   XP and runes. Then test the detached candidate through the real native
   map owner only, after Guild modal release. Require a known persistent
   write policy and a safe defeat route before enabling it for players.
   A divergence at the next original map tick or teardown rejects this
   approach. An asset loaded in the launcher cannot satisfy these checks.
2. **Walking-backed Echo as a diagnostic fallback.** From that same
   protected campaign, use a proven walking ground and owners, replace
   just the proven battle source at its existing setup boundary, and
   track all native returns. This narrows the uncertain transition and
   can establish whether the detached boss resources and generic RPG
   result handlers work. It does not satisfy direct Guild launching.
   Keep guests disabled during this test. If the first rendered
   callback-free case shows reward leakage, unexpected party mutation
   or a map that cannot accept a second request, keep both routes closed.

The second engineer should prioritize one complete protected return over
adding tiers, extra boss actors or save systems.
