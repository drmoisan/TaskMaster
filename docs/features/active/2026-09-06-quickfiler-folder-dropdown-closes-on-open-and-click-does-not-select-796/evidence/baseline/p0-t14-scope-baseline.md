# P0-T14 — Scope-lock baseline for the execution worktree

Timestamp: 2026-09-07T14-15
Task: [P0-T14]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command 'git rev-parse HEAD; git rev-parse --abbrev-ref HEAD; git status --porcelain --untracked-files=all'
```

EXIT_CODE: 0

HEAD: 336e30db5350845898e5a96f466df98678668d83
Branch: bug/quickfiler-folder-dropdown-closes-on-open-796
Base anchor used by every diff gate in this plan: c7ae69f1

`--untracked-files=all` is used because porcelain status otherwise collapses an
untracked directory to a single directory entry and would not enumerate the evidence
artifacts this plan creates.

## Full porcelain output

```
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t10-quickfiler-test-baseline.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t11-coverage-baseline.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t12-file-size-baseline.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t13-mcp-validator-probe.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t2-dotnet-sdk-install.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t3-nuget-restore.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t4-analyzer-version-skew.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t5-dotnet-tool-restore.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t6-dotnet-coverage-probe.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t7-csharpier-check-baseline.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t8-analyzer-rebuild-baseline.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/p0-t9-nullable-rebuild-baseline.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/baseline/phase0-instructions-read.md
```

Fourteen entries. Every one of them is inside the feature folder: one modified plan
file carrying this phase's check-offs, and thirteen untracked Phase 0 evidence
artifacts.

## PRE-EXISTING-DIRTY-SET:

EMPTY.

No porcelain path lies outside the feature folder. This is the normal result the plan
anticipates, because the preparation run that produced this plan committed the
feature folder and the promoted record and removed its own agent-memory writes before
finishing. Because the set is empty, every later porcelain gate in this plan is
strict: a gate that expects zero lines outside the feature folder and the write set
has no admitted exceptions to subtract.

## Permitted-change set — the sixteen write-set paths

| # | Path | Category |
|---|---|---|
| 1 | QuickFiler/Controllers/QfcFormController.Deactivate.cs | production, modify |
| 2 | QuickFiler/Interfaces/IQfcFormViewer.cs | production, modify |
| 3 | QuickFiler/Viewers/QfcFormViewer.cs | production, modify |
| 4 | QuickFiler/Viewers/BreadcrumbDropDownHost.cs | production, modify |
| 5 | QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | production, modify |
| 6 | QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | production, modify |
| 7 | QuickFiler/Controllers/QfcItemController.EventHandlers.cs | production, modify |
| 8 | QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs | production, modify |
| 9 | QuickFiler/Resources/FolderBreadcrumb.html | production, modify |
| 10 | QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs | production, create |
| 11 | QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | test, modify |
| 12 | QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | test, modify |
| 13 | QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs | test, create |
| 14 | QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs | test, create |
| 15 | QuickFiler/QuickFiler.csproj | compile entries, modify |
| 16 | QuickFiler.Test/QuickFiler.Test.csproj | compile entries, modify |

Sixteen paths. This is the permitted-change set for the Phase 9 scope-boundary gate.

## QuickFiler/QuickFiler.csproj.bak

The tracked file QuickFiler/QuickFiler.csproj.bak exists in this worktree
(verified: `Test-Path` returned True). It is NOT in the write set and must not be
edited. It is not a .cs file, so the formatter does not touch it, and no task in this
plan reads it. It is recorded here so a later search for compile entries does not
mistake it for the project file.

Output Summary: HEAD 336e30db, branch bug/quickfiler-folder-dropdown-closes-on-open-796,
14 porcelain entries all inside the feature folder, PRE-EXISTING-DIRTY-SET empty, and
the sixteen-path permitted-change set recorded.
