# Phase 2 — Site A and Site A' diff shape

Timestamp: 2026-09-09T13-01
Task: [P2-T4]

The anchored diff is paired with a porcelain span so untracked paths cannot hide: a name-listing or
numstat diff enumerates tracked changes only and cannot report a path that is not yet tracked.

## Span 1 — anchored numstat diff

Command: `git diff --numstat (git merge-base HEAD origin/main) -- QuickFiler/Controllers/QfcHomeController.cs QuickFiler/Controllers/EfcHomeController.cs`
EXIT_CODE: 0

Verbatim output:

```text
3	1	QuickFiler/Controllers/EfcHomeController.cs
3	1	QuickFiler/Controllers/QfcHomeController.cs
```

| File | Insertions | Deletions | Required shape | Met |
|---|---|---|---|---|
| `QuickFiler/Controllers/EfcHomeController.cs` | **3** | **1** | 3 insertions, 1 deletion | yes |
| `QuickFiler/Controllers/QfcHomeController.cs` | **3** | **1** | 3 insertions, 1 deletion | yes |

Both rows report exactly three insertions and one deletion, which is the intended edit and nothing
else. Any other shape would mean the edit touched a sibling region and that the
`catch (System.Exception e)` blocks at lines 382-385 and 399-402, or the disposal statements at lines
389-390, may have moved. They did not: `[P2-T5]` confirms the two catch lines are still at 382 and
399, and `[P3-T9]` confirms `_tokenSource?.Dispose();` is still at line 389.

## Span 2 — porcelain status companion

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

Verbatim output:

```text
 M QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs
 M QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
 M QuickFiler/Controllers/EfcHomeController.cs
 M QuickFiler/Controllers/QfcHomeController.cs
 M docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/plan.2026-09-08T23-50.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/catch-block-baseline.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/coverage-class-shape.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/coverage-figures.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/csharpier-check.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/file-line-counts.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/git-anchor.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/git-status.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/msbuild-analyzers.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/msbuild-nullable.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/mstest-coverage.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/outlook-process.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/phase0-instructions-read.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/requirements-read.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/baseline/toolchain-bootstrap.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/other/file-line-counts-after-site-a.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/fail-before-cleanup-sites.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/file-line-counts-after-cleanup-tests.2026-09-09T00-05.md
?? docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/prefix-build.2026-09-09T00-05.md
```

Every entry belongs to the permitted set: four Write Set files edited so far, this plan file, and
evidence artifacts under the feature folder's `evidence/` tree. No `.csproj`, no `.editorconfig`, no
`BannedSymbols.txt`, nothing under `.claude/`, nothing under `.github/`, and nothing under
`coverage/`.

Output Summary: the numstat row for each of the two production files reports exactly 3 insertions and
1 deletion, the required shape. The porcelain companion shows no untracked path outside the feature
folder's evidence tree, so no change is hiding from the anchored diff.
