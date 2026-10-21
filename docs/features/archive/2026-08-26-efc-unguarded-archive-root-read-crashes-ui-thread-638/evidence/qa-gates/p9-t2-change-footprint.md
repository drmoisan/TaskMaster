# [P9-T2] Change footprint against the merge base (Issue 638)

Timestamp: 2026-08-29T12-50

Command:

```
git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD
git status --porcelain -uall -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638
```

The anchored diff enumerates the tracked change against the `origin/main` merge base; the
porcelain status is its required companion, because a name-listing diff is blind to files
this plan created that are not yet committed.

EXIT_CODE: 0

Output Summary:

## `git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD`, verbatim

```
QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/EfcDataModel.cs
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t10-msbuild-analyzers.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t11-msbuild-nullable.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t12-direct-harness-baseline.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t12-vstest-coverage.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t6-dotnet-tool-restore.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t7-solution-restore.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t8-dotnet-coverage-probe.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t9-csharpier-check.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p1-t4-tree-facts.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p2-t3-seam-compile.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p3-t14-tests-compile.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p4-t6-fix-compile.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p4-t7-file-size.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p5-t3-untouched-tests.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p8-t2-followup-issue-dossier.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t1-csharpier-format.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t2-csharpier-check.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t3-msbuild-analyzers.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t4-msbuild-nullable.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t5-vstest-coverage.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t6-loop-closure.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t1-coverage-postchange.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t2-coverage-changed-lines.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t3-coverage-delta.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t4-canonical-coverage-artifact.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p3-t15-regression-fail-before.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p5-t1-regression-pass-after.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p5-t2-sentinel-tests.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/issue.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/plan.2026-08-29T07-41.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/research/2026-08-29T08-05-archive-root-guard-research.md
docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md
```

36 paths.

## `git status --porcelain -uall` companion, verbatim

```
```

The output is empty: everything within the three pathspecs is committed.

## Assessment against the acceptance conditions

- Names `QuickFiler/Controllers/EfcDataModel.cs`: **yes**.
- Names `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`: **yes**.
- Names `QuickFiler.Test/QuickFiler.Test.csproj`: **yes**.
- Names no path under `QuickFiler/` other than `QuickFiler/Controllers/EfcDataModel.cs`:
  **satisfied** — the only other `QuickFiler`-prefixed paths are under `QuickFiler.Test/`,
  a different directory.
- Names no path under `TaskMaster/`, `UtilitiesCS/`, `ToDoModel/` or `.github/`:
  **satisfied** — none appears.
- Every remaining named path lies under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/`:
  **satisfied** — the remaining 33 paths are all under that folder. `issue.md` and
  `research/2026-08-29T08-05-archive-root-guard-research.md` appear because they were added
  by the branch's earlier commit `f07b6299`, unmodified by this plan, exactly as the plan's
  Change Footprint anticipated.

In particular the diff does **not** name any of the four read-only citations:

- `QuickFiler/Controllers/EfcFormController.cs` — absent
- `TaskMaster/AppGlobals/AppOlObjects.cs` — absent
- `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` — absent
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` — absent

It also does not name `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` or
`QuickFiler/Controllers/EfcSelectionGuard.cs`, the two remaining read-only citations.

Commit: `254fd56d2e9415753c12b677476b441129306366` —
`fix(638): guard the three unguarded archive-root reads in EfcDataModel`.
