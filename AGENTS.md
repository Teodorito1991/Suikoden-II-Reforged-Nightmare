# Permanent project rule — GitHub synchronization

Development repository: https://github.com/Teodorito1991/Suikoden-II-Reforged-Nightmare

The owner has approved public visibility for this source repository. Keep the
repository public. Publishing a GitHub Release, installation ZIP or playable
beta requires separate explicit owner approval.

## Completed-milestone synchronization

After each successfully completed and verified version:

1. Commit and push the updated, audited source to the public source repository.
2. Update CHANGELOG.md with features, fixes and known limitations.
3. Update README.md and relevant documentation when functionality changes.
4. Record the version, exact DLL SHA256 and actual verification results. Distinguish
   managed/component tests, native execution, startup smoke and real gameplay.
5. Update appropriate configuration examples and build instructions.
6. Create a version tag when appropriate for a completed milestone; documentation-
   only work does not require a new gameplay version or tag.
7. Verify the remote received the intended commit and that CI passes for that commit.
8. Include the GitHub commit link and synchronization status in the final report.

Synchronize completed milestones, not every temporary compilation or intermediate
experimental build. Preserve the latest development source separately from beta
source references and player-facing artifacts.

## Mandatory safety boundaries

- Verify that the source repository remains **PUBLIC**.
- Do **not publish GitHub Releases**. Milestone synchronization is not permission
  to publish, create or modify player-facing release assets. Any release work needs
  its own explicit scope; existing private drafts must remain unpublished.
- Never upload original/private development history, private saves, personal
  information, credentials, proprietary game binaries/assets, generated interop,
  decompiled proprietary listings, backups or private development logs. Use the
  clean publication repository and audit new files before committing.
- Record sanitized verification summaries rather than raw logs. Check distributable
  binaries for private debug paths, and distinguish any changed DLL hash from the
  original binary's acceptance evidence.
- Keep development builds separate from player-facing releases. Preserve any
  existing release drafts and assets.
- If authentication, pushing, remote verification or CI fails, preserve local
  changes and report the failure or pending result. Never claim successful
  synchronization without verification.
- Preserve installed development DLLs, configurations and original saves unless
  the current task explicitly authorizes changing them.

## Current v1.51.1 integration

Build `1.51.1-feature-completion.1`, SHA256
DB2EA401C0F3C0DCC30E4F079AF8A29A2D5C8B70A31E42ABAE68E75998282AE9.
Full Preview selects22 supported Original echoes/10 Hunts, StoryProgression on,
Spoiler Mode off, training normal contract off. Luca source unchanged from d785831;
accepted7D368... DLL preserved, no full Luca retest. Beast Rune WinEnd/terminal1,
correct Guild return, protected5flags/war/selected progression unchanged and owned
cleanup passed on this DLL WITH OWNER-REQUESTED BOOSTED PARTY (lv99/HP999/stats255/
weapon16/9spellcharges). Do not call it normal-party balance acceptance. Owner
confirmed Guild reopened. Original saved records unaffected; copied manual slot
restored pre-boost and campaign testing disarmed. No new training save-outcome/fault
acceptance or XP. All existing systems retained. No beta/release/Steam install.
See docs/FEATURE-COMPLETION-V1511.md. Historical v1.51 accepted DLL must not be rebuilt.

## Historical v1.51 Luca checkpoint

Development `1.51.0-last-challenge.1`, SHA256
7D368B2E115082BEDA83AF7F2590E733114512C73A49487619F64A496705534B.
Owner completed Original Luca with18 distinct combatants across three teams:
native handoffs, HP6500→4318→1707, final earned victory, protected5flags/war/
selected-progression equality each stage, cleanup, original party restoration,
Guild reopening/preparation cancel and movement. Not Nightmare/defeat/second-run
acceptance. Source31 tables:24 used by22 definitions/profiles; unsafe/mirror records
excluded. Spoiler preference defaultOFF, second confirmation and reversible CFG;
per-entry opt-outs remain. Normal-build training direct-save experiment defaultOFF,
new fault/outcome gameplay pending. No commander XP. Beast Rune win still unproven.
Do not rebuild the accepted DLL for Git metadata. No Steam install, original-save,
release or beta change. See docs/LAST-CHALLENGE-V151.md.

## Historical v1.50 Final Encounters

Development `1.50.0-final-encounters.2`, SHA256
EAF67EB039F569149C9AB811AE770C82EC48DCB6D466AD070458C39D1D5338DF.
21 definitions/profiles; native Worm/Moth and multipart final controllers retained,
original story endings excluded. Original extra-texture coroutine fixes the live
final-boss field callback failure without changing battle_no=-1. Luca stage receipts
and private native HP getter are foundations only; no gauntlet launch/party owner.
Normal-build training contract is a separate default-off private-flag/save-fence
experiment; exact-build Commander Rout win/return passed, but direct SystemSave
autosave at map step14 bypasses the GSD2 save-file counter. Do not claim zero saves
or full normal-install certification. Autosave flags/war/party subsequently matched
the exact checkpoint; savedHP loss224 matches Worm. Fault paths remain unverified. Exact-build transformed Worm/Moth win/return
passed. Final-boss combat/defeat was on intermediate259139; victory unverified.
No commander XP. Exact build/evidence distinctions are mandatory.
Steam installation, original saves/config and previous fallback artifacts stay
unchanged. Preserve historical reports; no release or beta publication.
See docs/FINAL-ENCOUNTERS-V150.md and publication/development-build-v150.json.

## Historical v1.49 Grand Expansion

Build1.49.0-grand-expansion.1 / SHA256 D98E62D3B09E0FBFB9DFEC7AA8AE99AB661FF3F11D8DF14B79C2673056AC822F.
19 trial definitions/19 Nightmare profiles; eleven additions. Native source and
manager setup checks cover all19. Exact-hash owner Original Mist Shade win and
Rowd group win/Guild reopens passed; protected snapshots unchanged, owned cleanup
complete, native item9+1/item1+1, purse capped999999/zero credit, no saves. Other
new fights and Nightmare rewards remain native-only. Automated fifth-page captures
can omit fixed header/footer text; do not call all33 screenshots visually perfect. Seven bonus memories use
explicitly disclosed verified milestone gates; four new direct completion witnesses;
three old unresolved entries remain locked. Original8 IDs/options preserved.
Owned group clones and explicit local phase/command roles; no story endings.
Mist Shade direct GetBossResID(-1) uses only the active trial's owned/borrowed pack,
without changing battle_no. Four conditional growth stones, duplicate identity
protected. Full Content Preview is separate; all10 Hunts selected, training,
unsafe scripted switches and developer overrides off. No Steam install/save change,
no release. v1.48 fallback preserved. See docs/GRAND-EXPANSION-V149.md.

## Current v1.48 campaign integration

Build 1.48.0-campaign-integration.1 / SHA256
56BAB506B49F10A9D2206C9E5903769C3C79A8D092B8748A67C6D53F4923D469.
Generated Fresh Campaign profile tested without developer/input/reward fixtures.
Early levels3–6 party ordinary victory: 4 HP lost, 3 healed; protected save/reload.
Undying Watch real loop: local native-menu accept at vg15, walking request/native
formation, earned victory, 12,236 Potch actual credit and Sacrificial Jizo native
inventory+1, field return, Escape Talisman/ordinary travel, Guild report and
reacceptance. One completion/report; no report payout or second-win claim. Later
ordinary victory restored one missing charge. Managed/native5054 checks and22
previews passed; profile hashes reproducible and nonempty destinations refused.
Hunt/quest/export version labels corrected; general history wrapper still legacy.
Do not rewrite acceptance hashes to hide that metadata limit. Training still
requires private stock-transition harness; fresh profile leaves it off, along
with scripted Legendary options and Nightmare tiers. Core/story-war guarded
layers on; full campaign unverified. Steam DLL and14 originals untouched, testing
disarmed, exactv147 fallback preserved. No beta/release. See docs/FRESH-CAMPAIGN-V148.md.

## Current v1.47 Nightmare evidence

Build 1.47.0-nightmare-trials.2 / SHA256
7283F4B6873C6AFA8BE4277ADF6A7E09CEB42E4607C168D3AB3A381CABE211BF.
Final owner evidence: Golem win + four Abomination wins; native stone item21 +1
on last win; Nightmare/Original Abomination Let Go pay zero. Seven clean returns,
protected flags/war/progression unchanged, owned cleanup complete. Original HP
4,500 vs Nightmare6,750 observed. Golem10500 and actual reaction damage78->90.
Revision .1's calculation hook missed special damage; .2 moves only the Nightmare
increment to guarded DamageSetReActionData. Never re-add a second calculation
increment. Wolf won on .1, not .2; don't transfer exact-hash acceptance. Native
clone/reward/cap tests and 22-screen graphics passed, full campaign/new-tier defeat
unplayed. Global Hall and Nightmare default off. Original Steam DLL/14 saves and
v1.46 fallback preserved. Copied testing disarmed; no binary/release publication.
See docs/HALL-NIGHTMARE-TRIALS-V147.md and sanitized publication manifest.

## Current v1.46 Hall evidence

Build 1.46.0-new-memories.1 / SHA256
35F2E3BC0A8A84EB401E92C535D503ABF3059F87112AA6F3921BE87BDD59F50D.
Eight trials. Exact-hash owner wins Golem/Abomination/Worm; Golem then Pest Rat
Let Go, zero payouts and Guild reopening. Protected flags/war/progression unchanged,
owned cleanup complete. Source-backed new gates C9/01 and 56/20. Worm 3D/10 is
pre-battle and MUST NOT become a victory witness. Sierra/second Harpy unresolved;
keep all three normal-locked. Preserve v1.45 corrected gates and fallback CADC0476.
Native 5,054-check production audit, managed regressions and 18-screen graphics
preview passed; not full campaign acceptance. Original Steam DLL and 14 saves
unchanged. Global/new options default off, copied testing disarmed. No releases.
See docs/HALL-NEW-MEMORIES-V146.md; historical sections below retain old evidence.

## v1.45 progression correction

v1.44's three unlock claims are superseded: ordinary map loads / nonordinary
battle IDs were mistaken for boss provenance. Correct source-backed witnesses
are Pest Rat B6/02, first Harpy 21/10 and Golden Wolf 16/02, with category-15
preloads and immediate native post-battle writes. Sierra and second Harpy remain
locked normally; never restore the old witnesses or use shared C2/80 as proof.
Historical reports/artifacts stay preserved. All five battle definitions remain
implemented. Exact v1.45 hash owner wins: Golden Wolf / first Harpy under normal
story gating; Sierra / second Harpy with explicit testing availability, followed
by Pest Rat flee and Guild reopening. Second Harpy credited 127,500 Potch. See
docs/HALL-ECHOES-EARNED-V145.md and its verification manifest.

## v1.44 development evidence

Development source now includes five Hall trials, read-only source-backed gates
for first Harpy/Sierra/Golden Wolf, independent default-off new trial switches,
and an explicit developer-only availability override. Pest Rat and second Harpy
remain locked under StoryProgression until reliable witnesses are established.
Candidate .1 owner wins: Golden Wolf and Sierra; second Harpy earned defeat,
stock Game Over and zero payout. Revised candidate .2 is a distinct hash with
separate native/graphics verification and an owner-earned Pest Rat victory with
rewards, clean return and Guild reopening. Preserve the accepted v1.43 artifact.
See docs/HALL-ECHOES-PAST-V144.md and publication/development-build-v144.json.
No release publication or original Steam DLL change is authorized by this work.

## Current Hall development evidence

v1.43 development artifact is 1.43.0-second-challenger.1, SHA256
4A73164EC7DF9A16169BB858FE14A2BE1652505E0B63F70BBDFA3862D2554763.
The owner completed Pest Rat → Harpy → Pest Rat victories, Harpy flee, and
Harpy defeat → stock Retry → Guild → Pest Rat → flee on this exact DLL.
Native receipts preserved protected flags, war state, progression and Guild
return coordinates. Both Hall switches remain false by default. The accepted
v1.42 fallback is preserved. The original Steam DLL was not replaced.
See docs/HALL-SECOND-CHALLENGER-V143.md for evidence boundaries.
Never transfer acceptance to a different rebuild hash.
