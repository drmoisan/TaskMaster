# Final QA Gate 7 — Diff Scope (issue #742, [P5-T7])

Timestamp: 2026-09-14T02-26

Command: `git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test`

EXIT_CODE: 0

Output Summary (the full porcelain listing):

```
 M QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/Controllers/EfcHomeController.Metrics.cs
 M QuickFiler/Controllers/EfcItemController.cs
 M QuickFiler/Controllers/QfcCollectionController.cs
 M QuickFiler/Controllers/QfcHomeController.Metrics.cs
 M QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
?? QuickFiler.Test/Controllers/QuickFilerInvariantCultureIssue742Tests.cs
```

Acceptance: satisfied. The output lists exactly the eight Write Set paths and no others:

1. `QuickFiler/Controllers/QfcHomeController.Metrics.cs` — modified
2. `QuickFiler/Controllers/EfcHomeController.Metrics.cs` — modified
3. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — modified
4. `QuickFiler/Controllers/QfcCollectionController.cs` — modified
5. `QuickFiler/Controllers/EfcItemController.cs` — modified
6. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` — modified
7. `QuickFiler.Test/Controllers/QuickFilerInvariantCultureIssue742Tests.cs` — untracked (`??`), as
   expected until [P5-T8] stages it
8. `QuickFiler.Test/QuickFiler.Test.csproj` — modified

The check is scoped to the `QuickFiler` and `QuickFiler.Test` source trees on purpose. The same
change also adds Markdown evidence artifacts and updates `plan.2026-09-12T16-09.md` and `spec.md`
inside this feature folder, which is expected rather than a scope violation, so a repository-wide
phrasing of this criterion would be unsatisfiable.

`--untracked-files=all` is required because a name-listing diff enumerates tracked changes only and
would be blind to the one file this change creates.

## Scope-boundary confirmation

No file was touched in the UtilitiesCS project, the ToDoModel project, the uncompiled sources under
the QuickFiler Legacy folder, the non-Metrics partial-class file of the QfcHomeController class, the
sortable-key method in the EmailSorter file, the repository editor-configuration file, the
banned-symbols list, or the QuickFiler production project file. A separate check,
`git diff --name-only origin/main...HEAD -- "*.csproj" "*.config" "*.props" "*.targets"`, printed no
line before the Phase 1 project-file edit, confirming this branch carried no prior project-file
change; the only project-file change this branch makes is the single `Compile Include` line in
`QuickFiler.Test/QuickFiler.Test.csproj` added by [P1-T2].
