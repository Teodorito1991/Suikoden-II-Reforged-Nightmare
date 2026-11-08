# First beta decision

**Recommend A: v1.12.2**, packaging tag **v1.12.2-beta.1**. The original staging comparison used v1.13; main now develops v1.15, separately from beta;
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

Neither derivative inherits original-hash runtime acceptance. Candidate A has now
passed its OWN isolated startup smoke: BepInEx loaded the exact public DLL/fresh
settings, the original launcher rendered, and the copy exited normally. Candidate B
remains runtime-untested. Recommend **A-derived for owner beta review**, based on
that startup proof and stronger historical quest evidence, not a claim that the
public hash completed a hunt. See publication/startup-smoke-A.json and the binary audit.
Tags identify packaging versions; internal versions remain unchanged. Candidate A
must not be labelled v1.13.

Fresh public defaults enable Sun castle entry, automatic request and native item
fallback. Unplayed contracts, major bosses, debug/probes and research witness are
off. Core/earlier boss/story-war settings stay as provided. This differs from
embedded developer defaults and local user opt-ins; it is not a migration and has
not received a complete fresh-install gameplay session. Config parser checks are
reported separately. No installed settings, saves or DLLs change for packaging.


## v1.16 development note

Latest separate development artifact: v1.16 `1CA1F3F56200561B49EDCADCCF1295989716F0FB7D3AA4A7D077CC27261893C1`. This does not replace either reviewed beta candidate or its acceptance evidence. No player-facing assets or release status changed. Native-only UI lifecycle details: [v1.16](WAR-UI-V116.md).


## v1.17 development note

Latest separate development artifact is v1.17 `DC44899D9F3A8182BE95686A831CD351C3C9C2C077ABEFB5A9C0E173DC8E8C97`. It does not replace the beta or its verification status. Repository remains private; no release assets changed.


## v1.18 development note

Separate v1.18 artifact `376F51AB416C9A785F18B12B6884B690224BC6BEE4DC1F32B8607CA4937AD691` contains passive diagnostics; it does not replace or re-accept the beta. Installed v1.13 and unpublished beta assets remain unchanged.


## v1.19 development note

Separate v1.19 candidate `0EF0B648CC897C2119ED6919DA250FD1FDFFD1CAC5F8F536FB8BA8B5A8A1BEA7` is development-only. Installed v1.13 and beta unchanged. Native rendering/components do not replace human full-quest acceptance; no automatic beta substitution.
