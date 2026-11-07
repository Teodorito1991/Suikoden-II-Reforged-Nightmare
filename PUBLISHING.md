# Publication checklist

The repository and packages are prepared for review, not approved for public
publication. Keep repository visibility PRIVATE and releases DRAFT until the
owner explicitly approves making them public/publishing.

Recommended first tag: **v1.12.2-beta.1**, on `beta/v1.12.2`, internal
**1.12.2-guild-ui.1**. Main retains latest **v1.13** source. Optional development
tag **v1.13.0-dev.1** must not be presented as gameplay-accepted.

## Before any upload

- Run `python tools/audit-publication.py` and inspect staged files/history.
- Include only the clean publication history, never push the original private
  development repository/history bundle with logs, saves or extracted material.
- Confirm MIT approval, retained third-party notices and original-game exclusions.
- Verify ZIP/DLL hashes and the documented debug-path-only privacy change. Never
  substitute a rebuild or transfer exact-hash proof without disclosure.
- Complete a safe startup smoke check for each proposed public DLL hash before
  recommending publication. Static equality is not runtime acceptance.
- Keep acceptance exact-build scoped; do not advertise playable war skirmishes/XP.
- Check installation, update/config preservation and rollback instructions.
- Keep missing genuine screenshot slots honest; no generated mockup as evidence.

## If repository creation/push access is unavailable

The connected GitHub profile can be authenticated without exposing repository-
creation or release-upload tools. Local Git credentials may be separate. Do not
extract tokens from another app or post credentials into issues/chat.

Using GitHub or an authenticated official GitHub CLI, create
`Suikoden-II-Reforged-Nightmare` as **private**, without an auto-generated README
or license; those are already in this clean repository. Then attach only this
repository to the new remote and push `main`, `beta/v1.12.2` and the reviewed tags.
Never use `git push --mirror` on the private development project.

Prepare a **draft pre-release** targeting the reviewed beta tag. Attach only the
clean installation ZIP and checksums, with notes stating unchanged internal version,
new public hash and original-to-public mapping. A private repo is still an upload;
keep proprietary/private content excluded even before public publication.

No public visibility change or release publication is authorised by these
preparation instructions. Ask for that final approval with concrete reviewed files.
