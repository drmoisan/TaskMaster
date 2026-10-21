# P1-T10 — Frozen contracts survived finding 1

Timestamp: 2026-09-03T23-49

Command:

```
git add -A
git status --porcelain
git diff --cached --name-only origin/main -- TaskMaster/AppGlobals/ArchiveRootPathGuard.cs TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AppOlObjectsArchiveRootValidationTests&TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p1-t10.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p1-t10
```

EXIT_CODE: 0 (`git add -A` exit 0, the anchored diff exit 0, the vstest run exit 0)

## The anchored, index-reading diff

`git diff --cached --name-only origin/main -- <the three frozen paths>` printed **no lines**.

All three frozen paths exist on disk and are therefore genuinely covered by the pathspec:

- `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` — exists
- `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` — exists
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` — exists

The diff reads the **index** the preceding `git add -A` populated rather than a commit, so it
observes this phase's uncommitted edits. A three-dot `origin/main...HEAD` form would print nothing
for those three paths whether or not Phase 1 had edited them, because no task before this one commits
anything, so that form could not fail and is deliberately not used here.

## The `git status --porcelain` span

The span printed **24 lines**, which is what shows the index was in fact populated. None of them
names ArchiveRootPathGuard.cs, AppOlObjectsArchiveRootValidationTests.cs, or IOlObjects.cs.

```
A  TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs
M  TaskMaster.Test/TaskMaster.Test.csproj
A  TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs
M  TaskMaster/AppGlobals/AppOlObjects.cs
M  TaskMaster/TaskMaster.csproj
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t1-instructions-read.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t2-tree-baseline.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t3-csharpier-check.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t4-analyzer-rebuild.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t4-analyzer.min.log.txt
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t5-nullable-rebuild.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t5-nullable.min.log.txt
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t6-coverage.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t7-affected-classes.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t7/p0-t7-quickfiler.trx
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t7/p0-t7-taskmaster.trx
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t8-file-sizes.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t9-preexisting-counts.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t4-seam-build.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t7-finding1-red.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t7/p1-t7.trx
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t9-finding1-green.md
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t9/p1-t9.trx
M  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/plan.2026-09-02T12-02.md
```

The five code paths listed are exactly the five Write Set paths Phase 1 touches: the new archive-root
partial, the new COM-guard test file, the two project-file registrations, and the getter delegation
in AppOlObjects.cs.

## The unmodified validation class still passes

TRX records total **6**, passed **6**, failed **0**. `Test Run Successful.` Exactly one TRX file
exists under this task's results directory: `p1-t10.trx`.

Output Summary: the anchored index-reading diff over ArchiveRootPathGuard.cs,
AppOlObjectsArchiveRootValidationTests.cs, and IOlObjects.cs printed no lines, and the 24-line
porcelain span beside it proves the index was populated and names none of those three files.
AppOlObjectsArchiveRootValidationTests passes 6/6/0, unmodified.
