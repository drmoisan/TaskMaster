# P9-T10 — Scope-boundary gate over the committed diff

Timestamp: 2026-09-07T16-11
Task: [P9-T10]
Issue: #796
Channel used: A

## Commands

```
pwsh -NoProfile -Command 'git diff --name-status a6b259160f9ac1fbe251708d897fd4721486259e..HEAD'
pwsh -NoProfile -Command 'git status --porcelain --untracked-files=all'
```

Both EXIT_CODE: 0.

HEAD at the time of this gate is 676966ef2d36e20f231cfb3949591d44a9adb92d, the P9-T9
commit.

## The anchor

The diff is anchored to the explicit base ref
a6b259160f9ac1fbe251708d897fd4721486259e, written as the full forty-character SHA. An
unanchored diff compares the worktree against the index and would pass vacuously now that
the change is committed, which is why an explicit ref operand is used. The porcelain span
accompanies it because the two mechanisms are complementary and each alone is blind in
one state: the anchored diff cannot see an untracked file, and porcelain status goes
empty once a change is committed.

The anchor's ancestry was verified rather than assumed, because a two-dot diff from a
non-ancestor would not measure this item's footprint:

```
pwsh -NoProfile -Command 'git merge-base --is-ancestor a6b259160f9ac1fbe251708d897fd4721486259e HEAD'
```

EXIT_CODE: 0, which is the ancestor-true result. That commit is the origin/main commit
the third merge brought in, so a two-dot diff from it yields exactly this item's own
additions over main with no sibling's merged work re-billed to this item, and it is
precisely the pull request footprint.

## Result

TOTAL PATHS LISTED: 84.

| Class | Count |
|---|---|
| Inside the feature folder docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796 | 68 |
| One of the seventeen write-set paths | 15 |
| The single permitted path outside both | 1 |
| Any other path | 0 |
| Paths under .claude/, .codex/, .agents/, config/, .github/, UtilitiesCS/ or UtilitiesCS.Test/, or naming TaskMaster.sln or a repository-root build property file | 0 |

68 + 15 + 1 = 84, so every listed path is accounted for by exactly one class and none is
double-counted or unclassified.

The classification was computed mechanically from the diff output against the literal
seventeen-path write set, the feature-folder path prefix, and the one permitted path,
rather than read by eye.

### The fifteen write-set paths listed

```
M	QuickFiler/Controllers/QfcFormController.Deactivate.cs
M	QuickFiler/Controllers/QfcItemController.EventHandlers.cs
M	QuickFiler/Interfaces/IQfcFormViewer.cs
M	QuickFiler/QuickFiler.csproj
A	QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs
M	QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
M	QuickFiler/Viewers/BreadcrumbDropDownHost.cs
M	QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
M	QuickFiler/Viewers/QfcFormViewer.cs
M	QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs
M	QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
A	QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs
M	QuickFiler.Test/QuickFiler.Test.csproj
A	QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
M	QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs
```

Fifteen of the seventeen. The two write-set paths the diff does NOT list are
QuickFiler/Resources/FolderBreadcrumb.html and
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs, neither of which was changed.
Their absence does not fail this gate: the acceptance is an upper bound on which paths
may appear and not a lower bound on how many must appear, and no clause requires any
particular path to be listed. The HTML file's absence additionally matches the
`AC3-HTML-POINTERDOWN: NOT REQUIRED` decision recorded at P5-T5.

### The single permitted path outside the feature folder and the write set

```
A	docs/features/potential/promoted/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select.md
```

Exactly one such path was listed, and it is the one the acceptance clause permits by
name. It is the promoted potential record this item's own promotion created, and the
promotion lifecycle places that record outside the feature folder by construction. It is
deliberately not a member of the `## Write Set` section, whose count stays at seventeen,
because no task in this plan writes it and a backticked path would be read by downstream
blast-radius derivation as a write claim, which it is not.

## Porcelain span

```
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t9-final-commit.md
```

Two paths, both inside the feature folder: the plan file carrying the P9-T9 check-off,
and the P9-T9 evidence artifact, which records a SHA that did not exist until the P9-T9
commit had been made and therefore could not have been written before it. No path under
QuickFiler, under QuickFiler.Test, or anywhere else in the tree is untracked or modified.
The P9-T11 amend folds both into the final commit.

## Acceptance clause by clause

| Clause | Observed | Met |
|---|---|---|
| every listed path is inside the feature folder, or is one of the seventeen write-set paths, or is the single permitted promoted-record path | 68 + 15 + 1 = 84 of 84; 0 unclassified | yes |
| no listed path lies under the .claude, .codex or .agents trees, under config, or under .github | 0 such paths | yes |
| no listed path names TaskMaster.sln or a repository-root build property file | 0 such paths | yes |
| no path under UtilitiesCS/ or UtilitiesCS.Test/ is listed | 0 such paths | yes |

Output Summary: the anchored two-dot diff from a6b259160f9ac1fbe251708d897fd4721486259e,
whose ancestry of HEAD was verified, lists 84 paths: 68 inside this item's feature folder,
15 write-set paths, and exactly one path outside both, the promoted potential record the
acceptance clause permits by name. Zero paths fall outside those three classes and zero
lie under any prohibited tree. The accompanying porcelain span lists two paths, both
inside the feature folder and both explained by the plan. All four acceptance clauses are
met.
