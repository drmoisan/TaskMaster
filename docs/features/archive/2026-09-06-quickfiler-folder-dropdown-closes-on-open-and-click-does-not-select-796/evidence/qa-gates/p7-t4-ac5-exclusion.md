# P7-T4 — AC5 exclusion gate

Timestamp: 2026-09-07T14-55
Task: [P7-T4]
Issue: #796
Channel used: A

## Commands

```
pwsh -NoProfile -Command 'git add QuickFiler QuickFiler.Test docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796'
pwsh -NoProfile -Command 'git diff --cached --name-status d78ae7f7'
pwsh -NoProfile -Command 'git status --porcelain --untracked-files=all'
```

EXIT_CODE: 0 for all three.

The staging span accompanies the name-listing diff because such a diff cannot see an untracked
file, and this item creates two.

## Anchor

The anchor is d78ae7f7, the second merge commit, and deliberately not c7ae69f1. Anchored at
c7ae69f1 this diff would enumerate the 59 files the second merge of origin/main brought in, several
of which lie under UtilitiesCS/ and UtilitiesCS.Test/, and this gate's acceptance requires that no
such path be listed — so the gate would fail on work this item did not do.

## Result of `git diff --cached --name-status d78ae7f7`

Code and project paths listed (12):

| Status | Path | Write-set entry |
|---|---|---|
| M | QuickFiler/Controllers/QfcFormController.Deactivate.cs | 1 |
| M | QuickFiler/Interfaces/IQfcFormViewer.cs | 2 |
| M | QuickFiler/Viewers/QfcFormViewer.cs | 3 |
| M | QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 4 |
| M | QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 5 |
| M | QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 6 |
| M | QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 7 |
| M | QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | 11 |
| M | QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 12 |
| M | QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs | 13 |
| A | QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs | 14 |
| M | QuickFiler.Test/QuickFiler.Test.csproj | 16 |

Every one of the twelve is a member of the sixteen-path permitted-change set recorded in
evidence/baseline/p0-t14-scope-baseline.md. Four write-set entries do not appear, each for a stated
reason: entry 10, QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs, and entry 15,
QuickFiler/QuickFiler.csproj, were both changed by Phase 1, whose commit is an ancestor of the
anchor, so they carry no delta against it; entry 8,
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs, is untouched because the decision record
records `AC3-ENFORCEMENT-SITE: HOST`; and entry 9, QuickFiler/Resources/FolderBreadcrumb.html, is
untouched because the decision record records `AC3-HTML-POINTERDOWN: NOT REQUIRED`.

All remaining listed paths lie inside the feature folder
docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796.

## The acceptance condition

No path in the diff output begins with `UtilitiesCS/`. No path begins with `UtilitiesCS.Test/`.

## Result of `git status --porcelain --untracked-files=all`

```
M  QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
M  QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs
A  docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/regression-testing/p7-t3-ac1-ac5-guards.md
```

Three entries, all staged. Two are write-set paths (entries 12 and 13) and one is inside the
feature folder. No path in this output begins with `UtilitiesCS/` or `UtilitiesCS.Test/` either.

## Pre-existing dirty set

The `PRE-EXISTING-DIRTY-SET:` recorded in evidence/baseline/p0-t14-scope-baseline.md is EMPTY, so
this gate is strict: no path was excluded from either output on that basis, and none needed to be.

## Why the exclusion is what makes AC5 meaningful

AC5 is the issue #438 AC-3 regression guard: a row-set refresh while the selector is open must not
close the list. It is satisfied by leaving the session-preserving replacement path in UtilitiesCS
untouched, which this gate is the proof of. The guard test
RowSetRefreshWhileOpen_NeverClosesHost, recorded Passed at task P7-T3, observes that untouched path
rather than a modified one.

Output Summary: 12 code paths changed, all inside the sixteen-path write set; no path under
UtilitiesCS/ or UtilitiesCS.Test/ in either the anchored name-status diff or the porcelain status;
pre-existing dirty set empty so no exclusions were applied.
