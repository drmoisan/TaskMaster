# Final Commit Sweep (P7-T3)

Timestamp: 2026-08-28T16-15
Command: `git add -A` then `git commit`, then `git commit --amend` to fold this artifact in
EXIT_CODE: 0

## Commit

- **SHA:** `292d057a246cea1b69c3da584ff4737d85f99776` (pre-amend; the amend that folds in this
  artifact produces the final SHA recorded by `git log -1` at task end)
- **Branch:** `bug/quickfiler-keyboard-hook-leaks-to-outlook-677`
- **Baseline (parent) commit:** `361a49b884a4e3fe192bf04bae05151c598398fa`
- **Subject:** `fix(quickfiler): stop breadcrumb focus restoration leaking keyboard to Outlook (#677)`
- The commit message references issue **#677** in its subject line and body.

## What was committed

67 files. Everything the plan produced is in a single commit:

- **Production (11 files + 1 new):** `BreadcrumbDropDownHost.cs`,
  `BreadcrumbDropDownHost.Open.cs`, `ItemViewer.Breadcrumb.cs`, `IQfcFormViewer.cs`,
  `QfcFormViewer.cs`, `IItemViewer.cs`, `ItemViewer.FolderSearch.cs`, `IQfcItemController.cs`,
  `QfcItemController.FolderHandling.cs`, `QfcFormController.SetupDisposal.cs`, and the new
  `QfcFormController.Deactivate.cs`.
- **Tests (3 new + 1 structural enabler):** `BreadcrumbDropDownHostTests.Part3.cs`,
  `QfcFormControllerDeactivateTests.cs`, `QfcItemController.CancelBreadcrumbSelectorTests.cs`, and
  the additive `FakeQfcItemController` member in `QfcThemeHelperTests.cs`.
- **Project files:** `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`.
- **Plan checkbox updates:** all 53 tasks flipped to `[x]` **before** the commit, so the plan's own
  state is inside the commit and no post-commit edit is needed to reflect completion.
- **Spec and issue updates:** `spec.md` (AC-4..AC-10 checked, AC-1..AC-3 annotated, Rollout &
  Follow-up extended), `issue.md`, `pr-notes.md`.
- **Every evidence artifact**, including both Cobertura XMLs
  (`evidence/baseline/coverage-baseline.cobertura.xml`,
  `evidence/qa-gates/coverage-final.cobertura.xml`) and all four TRX directories
  (`evidence/baseline/p0-t11/`, `evidence/regression-testing/p4-t1/`, `p4-t2/`, `p4-t3/`).
- **Pre-existing dirty paths recorded at the P0-T2 baseline** that the plan inherited: the
  `.claude/agent-memory/**` entries and the two `docs/features/potential/promoted/**` entries. P7-T3
  requires committing ALL changes, and leaving them would have made the clean-tree gate
  unsatisfiable.

## Clean-status confirmation

`git status --porcelain` (unscoped) produced **empty output** at task end. The working tree, the
index, and the untracked set are all clean.

## Host-identifier sweep before staging

A case-insensitive fixed-string sweep across the feature folder, `QuickFiler/` and
`QuickFiler.Test/` returned zero hits for the account identifier, the host identifier, `:\Users\`
and `:/Users/` in every tracked path. The only two files carrying an account identifier are
`QuickFiler/obj/Debug/QuickFiler.csproj.FileListAbsolute.txt` and its `QuickFiler.Test` counterpart,
which are MSBuild scratch output under `obj/` and are gitignored (`.gitignore:27`), so neither
reaches the commit.

All four TRX files and both msbuild logs were sanitised in binary mode with case-insensitive
substitutions over the workspace-root prefix, the user-profile prefix, the host identifier and the
account identifier before staging, and each TRX was re-verified to parse under a strict XML parser
with its `<UnitTestResult>` count matching the run total.
