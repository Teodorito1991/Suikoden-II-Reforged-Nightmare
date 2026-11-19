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
