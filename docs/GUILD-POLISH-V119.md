# v1.19 Hunt Guild Gameplay Polish

Development version: `1.19.0-hunt-guild-polish.1`

DLL SHA256: `0EF0B648CC897C2119ED6919DA250FD1FDFFD1CAC5F8F536FB8BA8B5A8A1BEA7`

8 November 2026. Separate development candidate only. Installed v1.13, original
saves/configuration, previous artifacts and the private beta remain unchanged.
Independent skirmish research is paused; no war resource/flag/chapter experiment ran.

## Player-facing improvements

The existing accepted Guild renderer, attendant and original frame/font/cursor
remain in use. Contract details now have distinct destination, danger, targets,
preparation and victory-spoils blocks with consistent spacing. Titles and catalogue
labels retain their full readable names; long user-controlled strings stay bounded.

A message line now displays CastleGuildRuntime.Status. Previously acceptance failures
could update that field without appearing on screen. Disabled contracts now have a
clear unavailable prompt and a friendly rejection message. The action row says
Report completion or Withdraw contract, rather than a long technical reward warning.

Available, Accepted, Completed/Awaiting Report and Reported/ready-again states have
clear text. The reported contract's name remains visible through LastReportedId;
this is existing session state, not new persistence. Acceptance names the destination,
victory asks for a report, and reporting thanks the player and offers another hunt.
A single short footer explains that loading/restarting ends the contract.

Original directional/confirm/cancel processing, render-ready input fencing, scene/
owner checks, cancellation and native control restoration were not rewritten. No
new hotkeys, NPC, menu system or battle engine was added.

## Three-contract review

| Contract | Region / targets | Spoils and status |
|---|---|---|
| Sun King's Retinue | Cave of the Wind; Sun King and Sorcerer | Existing 2,500-Potch default bonus; possible Stone of Power. Historical full quest acceptance on original v1.12.2 only. |
| Dreamer's Circuit | Sindar Ruins; Mr. Venus, Nightmare and MegaWatt | Existing 1,200-Potch bonus; possible Stone of Magic Protection. Experimental, disabled by default; no full battle acceptance. |
| Thornwake Patrol | Greenhill City area; Melonzoo and two Holly Elves | Existing 3,000-Potch bonus; possible Flowing Orb. Experimental, disabled by default; precise travel route and full battle unplayed. |

Thornwake's region is now verified through the ORIGINAL MachiLoader map-ID registry,
matching ve03 uniquely at its actual native indices (area4/town2). The native
savedata_c.GetMachiNameSID result3052 maps through original text_gsd2_tbl alias432
to original English text Greenhill City. The earlier msdat.name404 is empty and was
not a sufficient travel label. No geographical label was guessed from monster lore.

This confirms the shared location/region label, not a visited submap or a demonstrated
travel route. Original field links and walking encounter records still pass exact
native source/table checks. The definition's descriptive Field text changed; enemy
signatures, roster, hooks, positions, units and balance values did not.

Sanitized provenance: [hunt-location-v119.json](../publication/hunt-location-v119.json).
No original text tables, assets, raw native listings or logs are redistributed.

## Rewards remain authoritative in battle

The screen shows the configured bonus separately from base battle Potch, and calls
items POSSIBLE. Original loot takes priority; native item eligibility, offer/decline
and inventory capacity remain authoritative. The purse limit is respected; the UI
does not promise an uncapped credit or guaranteed additional drop. Reporting does
not invoke money, inventory, XP or rune grants.

GuildQuest transitions, victory receipts, Escape retry, wrong-field/script exclusions,
automatic walking requests and load/session reset are unchanged. Added acceptance
preflight only reports an already-disabled contract clearly; the existing Select
and Quest.Accept guards remain in place. No payout path or reward probability changed.
Existing user opt-ins were not edited, and public/default Dreamer/Thornwake stay OFF.

## Verification

- Build: zero warnings/errors, no private CodeView build path.
- Retained managed regressions plus31 focused presentation checks passed: visible
  rejection feedback, state labels, potential reward wording, capacity/cap notes
  and opt-outs. Existing421 quest checks retain repeated reporting, Escape/retry,
  foreign/duplicate receipts and reset coverage. Models do not prove real gameplay.
- Exact-hash unloaded original-engine rendering: original frame, cursor and font
  ready; four presentation scenarios drawn through the real Label/DrawTexture path,
  with966 draw calls and owned asset references released. This is native drawing,
  not a field, talk/controller or human readability acceptance session.
- Exact-hash production native RPG/Hunt component checks passed, including original
  sources and all three catalogue/reward fixtures. Native purse/inventory fixtures
  do not prove a rendered earned victory or actual player receipt.
- Offline catalogue, accepted and report layouts were generated from the shared
  production rectangles and reviewed visually. They are explicitly labelled
  previews, not game screenshots. Original extracted assets remain private.
- One preparatory render test timed out on original-asset loading; retry on that
  same preparatory DLL passed. The final hash's render smoke passed. Unloaded UI
  asset loading remains timing-sensitive; no broad stability guarantee follows.
- Known isolated-runtime shutdown TMP_SubMeshUI exception/JobTempAlloc warning are
  recorded where observed; no engine fix or globally error-free claim is made.

No user gameplay session was requested. The actual v1.12.2 Sun quest acceptance
remains historical, scoped to0B520FDD9182B6C024FE96376D0F284BE249DF93AF2E748DC331D21324910F46.
The current DLL has no human Guild/complete-quest, rendered Dreamer/Thornwake, boss,
war or full-campaign acceptance. All research/adapters/passive war instrumentation
remain preserved; independent launching is hard-disabled.

## Candidate and remaining checks

Keep this development candidate separate from the beta and installed v1.13. A
future brief protected session can check the message line, unavailable selection,
cancel/control restoration and one normal Sun quest naturally; none is required
for this milestone. The two experimental contracts must not be advertised as fully
played. Do not introduce new hunts or balance changes to mask these validation gaps.
