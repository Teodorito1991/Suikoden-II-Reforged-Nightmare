# Publication audit

Prepared2026-11-07. This is a reviewed inventory/hygiene boundary, not blanket legal
clearance of a game's EULA or proof that an arbitrary future commit is safe.

## Included deliberately

- Authored C# plugin source and numerical balance/compatibility definitions from
  the v1.13 main or v1.12.2 beta snapshot; no gameplay rewrite.
- All retained managed regression inputs, including two authored archived arena
  model files used only by tests. No retired arena renderer/runtime is included.
- Public configuration examples, documentation, issue templates and read-only CI.
- MIT original-code licence, explicitly approved by the owner, and external notices.
- Two genuine user-supplied historical debug screenshots, clearly captioned.
- Sanitised evidence summaries and source-provenance hashes, not raw session logs.

## Excluded from tree AND imported history

The publication repository has a fresh allowlisted history. The old private Git
history/bundles are preserved locally and are not its ancestors. Excluded:

- GameAssembly/Unity/game binaries, original metadata and generated interop.
- Decompiled proprietary listings, method dumps, original script bytecode, extracted
  textures/fonts/models/audio and other raw game assets.
- Save collections, Steam account state, backup archives and personal settings.
- Local developer logs, F9 reports, native run directories and private file paths.
- Credentials, tokens, cached authentication and external dependency DLLs.
- Large build/test outputs and offline synthetic presentation mockups.

Small numerical signatures and authored policy tables remain because typed runtime
guards and regression tests depend on them. They contain IDs/numerical facts/hash
fingerprints rather than original dialogue, event bytecode or extracted artwork.
Their presence does not grant rights to redistribute the underlying original data.

## Verification and limitations

`tools/audit-publication.py` checks tracked plus untracked nonignored files for
forbidden paths/types, common secrets, personal paths/emails, invalid JSON and
generated-listing markers. Human/source review also inspected resource types and
large strings. A later history/ZIP inventory checks all proposed refs and the
installation archives. These checks are not an exhaustive secret scanner or a
legal opinion. No third-party runtime/game DLL is shipped in the plugin-only ZIP.

Both original candidate DLLs retain their original SHA256 privately. A personal
PDB path was found in each. Public copies replace only that CodeView string, with
new hashes, identical managed metadata and every other byte unchanged. No rebuild
occurred; neither derivative has runtime acceptance. A startup check is pending.
Managed suites and ten additional
public-default parser/safety checks were run separately on both source snapshots.
Neither DLL was rebuilt, installed or launched during this preparation. Exact-
build gameplay evidence remains scoped as documented in docs/ACCEPTANCE.md.

The proposed public defaults are not an already accepted fresh-install campaign.
The whole campaign and optional native-only features remain experimental.

## Remote/publication status

The GitHub connector recognises the connected account, but its exposed tools do
not create repositories or draft releases/upload release assets. Local Git
Credential Manager listed no authenticated GitHub account. No token was extracted
from another application and no private development data was uploaded.

The clean local repository, branches, proposed tags, source archives, clean
installation ZIPs, notes and checksums are prepared for review. Hosted private
repository creation/push and a hosted draft pre-release remain pending an
authenticated creation/upload path. Follow PUBLISHING.md. Public visibility and
release publication require separate explicit approval and have not occurred.
