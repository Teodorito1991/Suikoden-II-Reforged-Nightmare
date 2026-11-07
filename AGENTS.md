# Permanent project rule — GitHub synchronization

Development repository: https://github.com/Teodorito1991/Suikoden-II-Reforged-Nightmare

Development continues privately until the owner explicitly authorizes a public
release. This is a standing instruction for every future development milestone.

## Completed-milestone synchronization

After each successfully completed and verified version:

1. Commit and push the updated, audited source to the private development repository.
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

- Verify that the repository remains **PRIVATE**. Never make it public without
  separate explicit authorization.
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
- Do not automatically replace the existing downloadable beta with untested
  binaries. Keep development builds separate from player-facing releases.
- If authentication, pushing, remote verification or CI fails, preserve local
  changes and report the failure or pending result. Never claim successful
  synchronization without verification.
- Preserve installed development DLLs, configurations and original saves unless
  the current task explicitly authorizes changing them.
