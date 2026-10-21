# Phase 1 — Fail-before commit

Timestamp: 2026-09-09T14-05

Task: [P1-T4]

Command: `git add UtilitiesCS.Test docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git commit -m "test(823): add the R1 per-store fail-before regression test" -- UtilitiesCS.Test docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da...HEAD -- UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`
Command: `git add docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git commit --amend --no-edit -- UtilitiesCS.Test docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`

The 40-character SHA in the diff command is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2; no `BASE-SHA` token was left in the command.

EXIT_CODE: 0

The commit reported `3 files changed, 129 insertions(+)`: the test file plus the two Phase 1
evidence artifacts. This record was then written and folded into the same commit by the amend.

ANCHORED-DIFF-RESULT: the diff printed exactly one path,
`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`, which is the one
path this acceptance requires it to list.

Output Summary: Fail-before test and Phase 1 evidence committed at exit 0. The three-dot diff
against the recorded base anchor, scoped to the test file, lists that one path.
