# v1.51 — Luca's first complete Hall gauntlet

Development checkpoint, not a published beta or full-campaign certification.
Build `1.51.0-last-challenge.1`, SHA256 `7D368B2E115082BEDA83AF7F2590E733114512C73A49487619F64A496705534B`.

## Actual gameplay on this DLL

The owner completed the Original Luca Gauntlet in the copied runtime. Three
separate six-character teams (18 distinct native combatants) entered the original
`boss_luc1`, `boss_luc2` and `boss_luc3` encounters. Riou served in the third team.
The first two fights ended through their original command-driven handoffs; the
third ended in an earned native victory. These are not three ordinary WinEnd receipts.

Luca's observed HP sequence was 6,500 → 4,318 → 1,707. The original later openings
read the private carried HP, without writing campaign `GAME_DATA.luka_hp`.
Each return matched all five protected flag arrays, war data and the selected
progression snapshot. All three owned battle scopes completed cleanup. The owner
confirmed original party restoration, reopened the Guild/preparation menu,
cancelled and moved normally. This is one complete run, not a second full-run or
all-failure-path acceptance. Native purse credit was zero; no extra reward receipt
is claimed for this Original-tier run.

## Narrow implementation

- Native roster builder and current-party records supply eligible healthy characters;
  two-slot combatants are excluded. A draft stores IDs only in mod configuration
  and is revalidated against the current campaign before use.
- Original player models finish loading before party arrays are rebound. Existing
  field models are borrowed. Only newly acquired, identity-matched stage cache
  entries are removed after battle scene teardown; retained field models survive.
- A private equipment cache registers the selected characters' original equipment
  through native lookup/registration methods. Original cache associations are
  restored, retaining definitions for any legitimately received held loot.
- Both native command hooks observe handoffs without replacing outcomes. Source
  ending/story callbacks remain excluded. Original battle_no=-1 is preserved.
- The coordinator waits for MACHICON step20 and EVENTCON step2. EVENTCON alone
  became idle at return step17, which caused the previous rejected second entry.
  Input fencing never suppresses native fade/return steps. Released command-ended
  combat state is retired only under the matching destroyed battle owner.
- Earlier failed roster, graphics/equipment and black-screen attempts are not
  acceptance evidence. The successful exact DLL supersedes those candidates.

## Spoiler access and other v1.51 work

Hall **Unlock All Bosses** defaults OFF. It displays the requested major-spoiler
warning, defaults to Cancel and requires a second explicit confirmation. The
reversible CFG preference bypasses story locks only; disabled trials, source,
compatibility, resource and location guards remain. **Restore Story Progression**
reverses it. Warning/confirmation and preference survival across restarts were
observed on intermediate v1.51 candidates; do not transfer that exact-hash claim
to every later candidate. Component/native gate checks cover the final source.

Beast Rune is a disclosed late-game bonus after the audited Gorudo milestone,
not a claim of an individually defeated final boss or reachable post-ending save.
Its earned victory remains unverified. The prior rendered defeat is historical.

A default-off training persistence experiment now checks UISaveLoad2 save intent
and direct SystemSave.Save campaign payloads against its restored session owner,
five flag arrays and four native data records. The production save hook no longer
relies only on GSD2SaveData.SaveFile. New win/defeat/autosave fault-path gameplay
has not been accepted on this DLL. Keep this experimental contract OFF for a
campaign recommendation until those checks pass. No commander XP is implemented.

## Coverage and remaining gates

22 selectable Hall definitions / 22 Nightmare profiles use 24 of the 31 identified
boss source tables. Three mirror appearances are omitted; two invulnerable Neclord
records, scripted 32,000-HP Moth and story-capture Rowd remain excluded. The combined
Worm/Moth encounter already covers the playable transformation family. Historical
roster reports are retained unchanged.

Release build and managed checks pass. Original-engine source/copy/setup checks
pass for all24 source records, with a native equipment-attribute fixture and
independent spoiler/opt-out checks. These fixtures are not rendered victories.
Current Original Luca gameplay is separate from unplayed Nightmare Luca, defeat,
interruption and a second complete gauntlet. Final-only conditional Nightmare
stone logic is implemented but not a live reward receipt from this session.

All RPG/Hunt/story-war modules remain. The Steam plugin, original saves and v1.50
fallback are unchanged. No installation ZIP, beta or GitHub Release is published.
