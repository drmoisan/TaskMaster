# Phase 4 — R4 commit

Timestamp: 2026-09-09T14-28

Task: [P4-T4]

Command: `git add QuickFiler docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git commit -m "docs(823): correct the stale line-count comment in BreadcrumbDropDownHost.Open.cs" -- QuickFiler docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence`
Command: `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da...HEAD -- QuickFiler/Viewers`
Command: `git add ...` followed by `git commit --amend --no-edit -- ...` with the same pathspec list

The 40-character SHA is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2.

EXIT_CODE: 0

The commit reported `3 files changed, 57 insertions(+), 1 deletion(-)`: the one-token comment
correction plus the two Phase 4 evidence artifacts.

ANCHORED-DIFF-RESULT: the three-dot diff scoped to `QuickFiler/Viewers` lists
`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, and does NOT list
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, which is the file the corrected figure measures and
which no task of this plan edits. The two other listed paths,
`QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` and `QuickFiler/Viewers/QfcFormViewer.cs`, are
the R3 changes committed by [P3-T10] and are both Write Set paths.

VERIFICATION: after the amend, the same anchored diff was re-run and printed the same three paths,
still listing `BreadcrumbDropDownHost.Open.cs` and still not listing `BreadcrumbDropDownHost.cs`.

Output Summary: R4 comment correction and Phase 4 evidence committed at exit 0. The anchored
three-dot diff lists `BreadcrumbDropDownHost.Open.cs` and does not list `BreadcrumbDropDownHost.cs`.
