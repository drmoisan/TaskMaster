# Phase 4 — R4 sibling-file fence

Timestamp: 2026-09-09T14-27

Task: [P4-T3]

Command: `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da -- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs`
Command: `git status --porcelain --untracked-files=all -- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs`

The 40-character SHA is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2. The porcelain span is present because a
name-listing diff cannot report an untracked path.

EXIT_CODE: 0

SIBLING-DIFF-LINES: 0
SIBLING-PORCELAIN-LINES: 0

Both commands printed nothing and exited 0. These three sibling doc comments carry the same class
of stale line-count figure that R4 corrects and are deliberately out of scope per the
specification's "Reported-only observations" section; the fence proves none of them was touched.
The specification records their claimed and measured figures for a later reader:
`BreadcrumbBridgeCoordinator.Search.cs:11` claims 487 against a measured 437;
`BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10` claims 481 against a measured 497; and
`BreadcrumbDropDownOpenLifetime.Focus.cs:8` claims 477 against a figure that was not measured.

Output Summary: Neither the anchored diff nor the porcelain status reports any of the three
deliberately out-of-scope sibling files. Both counts are 0.
