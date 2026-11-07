# First beta decision

**Recommend A: v1.12.2**, packaging tag **v1.12.2-beta.1**. Main stays at v1.13;
candidateB is a separate developer review package. No local rollback or rebuild.

| Criterion | A:1.12.2-guild-ui.1 | B:1.13.0-native-war-foundation.1 |
|---|---|---|
| DLL SHA256 |0B520FDD9182B6C024FE96376D0F284BE249DF93AF2E748DC331D21324910F46|1A59986703E8346E1B2F6D7968C451C0609A4FB990C8F88EB3D63DBCEBB2CD59|
| Sun full quest/items/report/repeat |User-assisted acceptance on this hash| Preserved code, no current-hash human session |
| Managed checks |703753|703771|
| Native checks |Two complete runs plus unloaded drawing|Two complete runs plus28 foundation checks/run|
| Added playable war content |None|None|
| Skirmishes/training |Not implemented|Hard-disabled foundation; no XP/persistence|
| Dependencies |Tested BepInEx/game/interop family|Same references; research adds no new runtime family|
| Whole campaign |Experimental|Experimental|
| Proposed role |First beta|Development review|

B's additions are guarded/read-only or unloaded probes; no new regression is
claimed. A nevertheless has stronger real player-path evidence, and B adds hooks
without new playable benefit. This is an evidence-based recommendation, not a
statistical crash-rate comparison or full-campaign certification.

Exact DLLs are copied unchanged. Tags identify packaging versions; internal plugin
versions and hashes stay unchanged. CandidateA must not be labelled v1.13.

Fresh public defaults enable Sun castle entry, automatic request and native item
fallback. Unplayed contracts, major bosses, debug/probes and research witness are
off. Core/earlier boss/story-war settings stay as provided. This differs from
embedded developer defaults and local user opt-ins; it is not a migration and has
not received a complete fresh-install gameplay session. Config parser checks are
reported separately. No installed settings, saves or DLLs change for packaging.
