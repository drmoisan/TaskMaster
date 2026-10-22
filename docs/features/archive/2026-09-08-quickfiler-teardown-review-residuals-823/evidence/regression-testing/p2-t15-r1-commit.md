# Phase 2 — R1 commit

Timestamp: 2026-09-09T14-16

Task: [P2-T15]

Command: `git add UtilitiesCS UtilitiesCS.Test docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git commit -m "fix(823): rescope the SMTP retry budget from per-controller to per-store" -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da...HEAD`
Command: `git add ...` followed by `git commit --amend --no-edit -- ...` with the same pathspec list

The 40-character SHA in the diff command is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2.

EXIT_CODE: 0

The commit reported `10 files changed, 322 insertions(+), 23 deletions(-)`.

ANCHORED-DIFF-RESULT: the three-dot diff against the recorded base anchor lists all four required
production and test paths:

- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`

It also lists `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md`,
which is the [P2-T14] correction block and is a Write Set path, and this feature's own evidence
artifacts. No other path appears.

VERIFICATION: after the amend, `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da...HEAD
-- UtilitiesCS UtilitiesCS.Test` was re-run and printed exactly the four paths listed above, so the
amend did not disturb the acceptance.

Output Summary: R1 production change, the two new tests, the four rewritten prose sites, the
issue-812 correction block and the Phase 1 and Phase 2 evidence committed at exit 0. The anchored
three-dot diff lists all four required paths.
