# First beta decision

**Recommend A: v1.12.2**, packaging tag **v1.12.2-beta.1**. Main stays at v1.13;
candidate B is a separate developer review package. No local rollback or rebuild.

| Criterion | A:1.12.2-guild-ui.1 | B:1.13.0-native-war-foundation.1 |
|---|---|---|
| DLL SHA256 |0B520FDD9182B6C024FE96376D0F284BE249DF93AF2E748DC331D21324910F46|1A59986703E8346E1B2F6D7968C451C0609A4FB990C8F88EB3D63DBCEBB2CD59|
| Sun full quest/items/report/repeat |User-assisted acceptance on this hash| Preserved code, no current-hash human session |
| Managed checks |703753|703771|
| Native checks |Two complete runs plus unloaded drawing|Two complete runs plus 28 foundation checks per run|
| Added playable war content |None|None|
| Skirmishes/training |Not implemented|Hard-disabled foundation; no XP/persistence|
| Dependencies |Tested BepInEx/game/interop family|Same references; research adds no new runtime family|
| Whole campaign |Experimental|Experimental|
| Proposed role |First beta|Development review|

B's additions are guarded/read-only or unloaded probes; no new regression is
claimed. A nevertheless has stronger real player-path evidence, and B adds hooks
without new playable benefit. This is an evidence-based recommendation, not a
statistical crash-rate comparison or full-campaign certification.

Original DLLs remain unchanged and preserved. Binary inspection found a personal
PDB path in each. Public review copies replace only that debug-path string, without
rebuilding or changing managed metadata/gameplay bytes. Their hashes are:

- A-derived: `8F3F085C3D9338C43FD74F80110D86CEFAD5F77A08A738EBA06D0B341C9DBD34`
- B-derived: `386CF34C6B549A185C5DACC6A5F64C38150FED935D36D1B9E9551E6CEDDBDE17`

Neither derivative inherits exact-hash runtime acceptance. The safer first-beta
recommendation is **A-derived, pending startup verification**, based on stronger
historical behaviour evidence and a strictly bounded metadata change, not a claim
that its new hash was played. See publication/binary-privacy-audit.json.
Tags identify packaging versions; internal versions remain unchanged. Candidate A
must not be labelled v1.13.

Fresh public defaults enable Sun castle entry, automatic request and native item
fallback. Unplayed contracts, major bosses, debug/probes and research witness are
off. Core/earlier boss/story-war settings stay as provided. This differs from
embedded developer defaults and local user opt-ins; it is not a migration and has
not received a complete fresh-install gameplay session. Config parser checks are
reported separately. No installed settings, saves or DLLs change for packaging.
