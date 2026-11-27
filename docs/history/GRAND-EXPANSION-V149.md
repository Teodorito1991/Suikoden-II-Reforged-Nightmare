# v1.49 — The Grand Expansion

Build `1.49.0-grand-expansion.1`
SHA256 `D98E62D3B09E0FBFB9DFEC7AA8AE99AB661FF3F11D8DF14B79C2673056AC822F`

This is a development content candidate. The original Steam installation and its
fourteen saves are untouched. v1.48 (`56BAB506B49F10A9D2206C9E5903769C3C79A8D092B8748A67C6D53F4923D469`)
remains the accepted fallback. No campaign playthrough, release or beta publication.

## Delivered coverage

31 original `boss_*` battle tables identified, including repeat appearances, groups,
transformations and scripted encounters. These are **31 records, not 31 different
boss identities**. 19 Hall definitions now exist: eight preserved plus eleven
additions. Every definition has an optional Nightmare profile. 16 can become
available through normal read-only story or disclosed bonus gates; Sierra, second
Harpy and first Worm remain locked in ordinary StoryProgression.

New rematches: Bone Dragon; Celestial Sword; final vulnerable Neclord; Lucia's
Retinue; Highland Lucia; Gorudo and five knights; Seed & Culgan; Double Head;
transforming Mist Shade; the shorter castle Lucia duel; Rowd and five soldiers.
The three original Lucia castle duels are not presented as three new challenges.

## Shared integration

The accepted ordinary request, `battle_no=-1`, original engine, owned HD scene,
pack lease, native rewards, field return and cleanup remain one lifecycle.
The former single-monster restriction is replaced by exact multi-monster source
validation, original formation copying and owned Nightmare clones per template.
Original sources, levels, move tables and loot are not edited.

An explicit per-definition callback policy retains only the inspected battle-local
roles: ordinary opening everywhere; main controllers where required; original
phase callbacks for Double Head/Mist Shade; original command selection for the
Lucia/Gorudo records. **Every story ending stays detached.** No invented phase
completion, extra turns or victory injection.

Mist Shade directly resolves its pack from `SYS_WORK.battle_no` in its native
controller. A narrowly owned `GetBossResID(-1)` adapter supplies the current Hall
lease only while the matching trial/work/system is active. It does not set a boss
index or change stock requests. Calls outside that envelope remain original.
Mist Shade completed a real earned victory on this DLL; Double Head remains native-prepared, not played.

## Story and bonus unlocks

Five established witnesses are preserved. New original map records are fingerprinted
and checked by the startup native provenance audit: Double Head `07/40`, Bone Dragon
`BA/08` or `BA/10`, Lucia's Retinue `BB/10`, Gorudo `14/02`. Their post-battle writes
are inspected; the mod never executes event bytecode or writes these unlock flags.
These are source-backed completion witnesses, not a replay of their story events.

Seven entries are explicitly **bonus memories**, not assertions that their individual
story boss has been defeated: Mist Shade/Rowd follow Double Head; Celestial Sword
follows Abomination; final Neclord, Highland/castle Lucia and Seed & Culgan follow
Gorudo. These design prerequisites reuse independently proven completion flags.
They are disclosed in the Guild. They are not individual victory witnesses and can
precede an encounter's own story appearance. No sidecar or fabricated campaign identity.
The three previously unresolved unlocks remain unresolved rather than borrowing an
unrelated flag and calling it their victory.

## Nightmare progression and rewards

The accepted three profiles are retained. Sixteen additional initial profiles use
120–150% original HP and 8–10% extra incoming damage for most additions; the accepted
Golem/Wolf factors remain unchanged. Groups receive smaller increments. Native
moves, ranges, silence checks, use limits and original animations remain authoritative.
Existing legal-action scoring alternates pressure with lower-power recovery choices;
only native-legal target candidates are considered. Perfect balance is not claimed.

One victory ledger reuses the accepted 25% **empty-native-loot-slot** offer. Depending
on the profile: Stone of Power, Stone of Magic, Stone of Speed or Stone of Magic
Protection. Native rolled loot wins priority; inventory acceptance/capacity still
apply. The pending reward identity cannot change on a repeated callback, reroll or
pay after handoff. Defeat/Let Go/aborted initialization never authorize a reward.
There is no report payout, mandatory currency or daily cap. Original Echoes keep
original native rewards. Celestial Sword explicitly has no native Potch/item drop.

## Full Content Preview

`profiles/full-content` is separate from the preserved v1.48 conservative proposal.
Generate with `tools/prepare-full-content.ps1 -OutputDirectory <empty staging folder>`.
It generates configuration only, never installs a DLL, finds saves or overwrites a
nonempty destination. All ten existing Hunts are enabled; all sixteen legitimately
gated Original/bonus Echoes are opted in. Nightmare remains a separate optional
overlay. The three unresolved Hall entries, developer protection, availability
bypass, scripted Legendary overrides and Army Training remain off.

Hunt formations, native request guards, escape retry, exactly-once completion and
report behavior are unchanged. Sun and Undying Watch retain their historical earned
quest evidence; eight others still have native/component evidence only. Existing
original map/source restrictions remain; all regional travel routes and story locks
have not been player-traversed. Inaccessible areas do not cause a wrong-location
replacement: withdraw the contract and revisit when legitimately reachable.

## War and RPG scope

Original story-war profiles, specializations, native AI, objectives and seven-war
maneuvers are unchanged. Four training drills/three maps and Veteran variants retain
their historical real gameplay evidence, but still require the private stock-transition
harness. That gate is not merely advertised as a shipped launcher. Removing it alone
would not certify ambient Steam achievement/autosave behavior or campaign state in a
normal installation. No normal-build training rollout or commander XP/persistence
is claimed in this sprint. This is a bounded unresolved exception, not an impossible
engine claim or a restarted resource investigation.

Existing 80 character profiles, Party Revolution, four Star Bonds, rune/spell,
physical technique, Unite, recovery and combat layers are preserved. The general
battle-history wrapper now reports `Plugin.BuildLabel` instead of the stale v1.13
label. No unrelated balance multiplier or new progression framework was added.

## Evidence on this DLL

- Release build: deterministic, no private debug paths; zero warnings/errors.
- Managed regressions and both configuration-profile checks pass.
- Original-engine startup audit passes: all 19 source/pack/background/HP identities,
  callback-copy policies, **real native BattleDataSetup** selection on disposable
  owners, owned Nightmare copies and unchanged source records.
- Four growth-stone offers/native scratch inventory insertions pass; duplicate
  identity, rolled-loot priority and native capacity handling are covered.
- New completion-event records load and match exact fingerprints. No commands run.
- Actual gameplay on this DLL: **Mist Shade Original → win → Guild reopen**, then
  **Rowd's Patrol Original → win → Guild reopen**, in one copied runtime using
  Full Content Preview with developer/availability overrides off. Owner defended
  for two rounds before winning Mist Shade; native combat-main states progressed.
  Both setups matched; both returned to the exact Guild position, all five protected
  flag arrays/war/progression snapshots matched and owned cleanup completed.
- Native item slots increased: Mist Shade item9 +1; Rowd item1 +1. Potch credit was
  zero with the archived checkpoint's purse already at999,999. No wallet override,
  combat-stat injection, campaign flag edits, save intents or save-file calls.
- 33 original-resource preview captures produced; representative new catalogue and
  tier cards are readable. Some automated fifth-page captures intermittently omit
  fixed header/footer text. Advancing capture selection from Update after three
  retained frames prevents same-frame view switching, but does **not** establish
  that every capture is perfect. The accepted live label renderer is preserved;
  discarded glyph-warming/raster experiments are not in this build. Live Guild
  navigation/reopening passed in both owner fights. This visual limitation remains.
- Other new encounters, Double Head phase playback and new Nightmare variants
  remain native-only on this hash. No new defeat/Retry or same-boss repeat earned
  acceptance is claimed. Prior v1.43–v1.48 evidence retains its original hashes.

The full fresh campaign remains the eventual integration/balance test, at the owner's
chosen time. It has not begun. New content is a preview, with remaining uncertainty
around Double Head, additional retained command controllers and new Nightmare groups. Use the independent
options to defer one problem encounter without removing the rest of the catalogue.

## Complete record inventory

| Original table | Selectable identity | Status | Unlock or exact limitation |
|---|---|---|---|
| `boss_dor` | Bone Dragon | Implemented / native-prepared | BA/08 or BA/10 |
| `boss_gol` | Stone Golem | Implemented / native-prepared | C9/01 |
| `boss_gor` | Gorudo's Guard | Implemented / native-prepared | 14/02 |
| `boss_gwo1` | Golden Wolf | Implemented / native-prepared | 16/02 |
| `boss_gwo2` | — | Duplicate/deferred | Mirrored Golden Wolf appearance; same HP and native attack profile. Not padded into another challenge. |
| `boss_hap1` | Harpy | Implemented / native-prepared | 21/10 |
| `boss_hap2` | Harpy · Second Encounter | Implemented / native-prepared | Unresolved; testing-only |
| `boss_imo1` | Worm | Implemented / native-prepared | Unresolved; testing-only |
| `boss_imo2` | — | Blocked | Worm-to-Moth transformation, command controller and outcome require an additional audited multi-form adapter. |
| `boss_imo3` | — | Blocked | Three original BE commands and 32,000-HP scripted Moth record; cannot be treated as an ordinary standalone boss. |
| `boss_ki1` | Mist Shade | Implemented / native-prepared | Bonus after Double Head |
| `boss_kul_sid` | Seed & Culgan | Implemented / native-prepared | Bonus after Gorudo |
| `boss_las` | — | Blocked | Five active body parts plus additional controller record; coordinated phase/command/main and death rules remain outside this adapter. |
| `boss_luc1` | — | Blocked | First Luca party and persistent luka_hp handoff; not an interchangeable isolated rematch. |
| `boss_luc2` | — | Blocked | Second Luca party and persistent luka_hp handoff; not an interchangeable isolated rematch. |
| `boss_luc3` | — | Blocked | Original opening reads GAME_DATA.luka_hp into the actor. No Hall-local replacement for that campaign-owned input is proven. |
| `boss_lus1` | Lucia's Retinue | Implemented / native-prepared | BB/10 |
| `boss_lus2` | Lucia · Highland | Implemented / native-prepared | Bonus after Gorudo |
| `boss_lus3` | Lucia · Castle | Implemented / native-prepared | Bonus after Gorudo |
| `boss_lus4` | — | Duplicate/deferred | Another castle Lucia appearance with the same short duel profile as lus3; omitted from the selectable roster. |
| `boss_lus5` | — | Duplicate/deferred | Another castle Lucia appearance with the same short duel profile as lus3; omitted from the selectable roster. |
| `boss_nec1` | — | Blocked | Story-specific invulnerability and supporting zombies; not the final vulnerable Neclord battle. |
| `boss_nec2` | — | Blocked | Earlier invulnerable Neclord encounter; scripted story outcome cannot become an earned ordinary victory by clearing an ending. |
| `boss_nec3` | Neclord · Final Encounter | Implemented / native-prepared | Bonus after Gorudo |
| `boss_nik` | Abomination | Implemented / native-prepared | 56/20 |
| `boss_rat` | Pest Rat | Implemented / native-prepared | B6/02 |
| `boss_rau1` | Rowd's Patrol | Implemented / native-prepared | Bonus after Double Head |
| `boss_rau2` | — | Deferred | 30,000-HP story-specific Rowd encounter. Its intended capture/outcome remains unverified; first Rowd encounter is implemented instead. |
| `boss_ryo` | Double Head | Implemented / native-prepared | 07/40 |
| `boss_sei` | Celestial Sword | Implemented / native-prepared | Bonus after Abomination |
| `boss_sie1` | Sierra | Implemented / native-prepared | Unresolved; testing-only |

Blocked means the required adapter/evidence is missing; it does not prove impossibility.
Near-duplicate/deferred entries are identified separately. No raw proprietary listings,
resources, private saves or runtime traces are included in this source report.
