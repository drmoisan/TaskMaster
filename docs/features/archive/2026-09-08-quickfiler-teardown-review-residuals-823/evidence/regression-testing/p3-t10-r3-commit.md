# Phase 3 — R3 commit

Timestamp: 2026-09-09T14-24

Task: [P3-T10]

Command: `git add QuickFiler QuickFiler.Test docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git commit -m "fix(823): reject null arguments to BreadcrumbPopupOwnerRegistry.Register" -- QuickFiler QuickFiler.Test docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da...HEAD -- QuickFiler QuickFiler.Test`
Command: `git add ...` followed by `git commit --amend --no-edit -- ...` with the same pathspec list

The 40-character SHA in the diff command is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2.

EXIT_CODE: 0

The commit reported `8 files changed, 214 insertions(+), 14 deletions(-)`.

ANCHORED-DIFF-RESULT: the three-dot diff against the recorded base anchor, scoped to the two
QuickFiler trees, lists all three required paths and nothing else:

- `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs`
- `QuickFiler/Viewers/QfcFormViewer.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`

VERIFICATION: after the amend, the same anchored diff was re-run and printed exactly those three
paths, so the amend did not disturb the acceptance.

Output Summary: R3 guard change, the two rewritten XML docs, the rewritten rejection test and the
Phase 3 evidence committed at exit 0. The anchored three-dot diff lists exactly the three required
paths.
