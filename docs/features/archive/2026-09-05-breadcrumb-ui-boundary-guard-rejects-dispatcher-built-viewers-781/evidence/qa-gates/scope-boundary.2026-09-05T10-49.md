# AC7 Scope Boundary (issue #781)

Timestamp: 2026-09-05T16-55

Task: [P1-T10]

The base ref is `main`, the value recorded under `BASE_REF:` in
`FEATURE/evidence/baseline/worktree-context.2026-09-05T10-49.md`. All four invocations were
issued from the repository root as `git` commands.

## Invocation 1 — complete the diff for created files

Command: `git add --intent-to-add --all -- QuickFiler QuickFiler.Test`

EXIT_CODE: 0

Without this entry the new test file is untracked and invisible to any diff.

## Invocation 2 — anchored two-dot diff against the working tree

Command: `git diff --name-status main -- QuickFiler QuickFiler.Test`

EXIT_CODE: 0

```
M	QuickFiler.Test/QuickFiler.Test.csproj
M	QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs
A	QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
M	QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
```

The two-dot form is required and the three-dot form is not substituted: `main...HEAD` compares
two commits and never reads the working tree, so because this plan never commits it would report
an empty list for these pathspecs however the files were edited.

## Invocation 3 — porcelain status companion

Command: `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test`

EXIT_CODE: 0

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs
 A QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
 M QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
```

This span observes untracked paths directly and would still report them had the intent-to-add
step been skipped. It reports the same four paths as invocation 2.

## Invocation 4 — commit-to-commit form

Command: `git diff --name-only main..HEAD -- QuickFiler QuickFiler.Test`

EXIT_CODE: 0

INHERITED PATHS:

(no path listed; the command produced no output)

No commit already on this branch touches either pathspec, so every path reported by invocation 2
is this plan's own uncommitted work. No path had to be subtracted from the union.

## Output Summary

Union of the paths reported by invocations 2 and 3 (four paths; the two invocations agree
exactly):

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`

All four acceptance conditions hold:

1. `INHERITED PATHS:` is present and lists no path.
2. The union contains **zero** paths beginning `QuickFiler/Controllers/`.
3. The union contains **zero** paths matching `QfcCollectionController` or `QfcItemController`.
   `QfcCollectionController.cs`, `QfcItemController.FolderHandling.cs`, and
   `QfcItemController.ViewerSetup.cs`, the three files AC7 names explicitly, are therefore
   unchanged.
4. Every path in the union is one of the four the plan's Write Set permits.
