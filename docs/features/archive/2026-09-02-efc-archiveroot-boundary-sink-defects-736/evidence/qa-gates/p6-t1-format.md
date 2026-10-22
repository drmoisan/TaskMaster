# P6-T1 — Repository-wide formatter pass

Timestamp: 2026-09-04T01-35

Command:

```
git status --porcelain
dotnet tool run csharpier format .
git status --porcelain
```

EXIT_CODE: 0

The formatter rewrites tracked source and exits 0 whether or not it rewrote anything, so the
acceptance below is a tree observation and not the exit code.

Summary line the formatter printed, verbatim:

```
Formatted 1580 files in 2390ms.
```

**This artifact records the second execution of P6-T1, run after P6-T13.** P6-T13 appended three
success-path tests and one shared arrangement helper to
`QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs`, which is a change to tracked source,
so the Phase 6 toolchain loop restarted from step 1 exactly as CLAUDE.md's General Code Change
Policy requires. The figures below supersede those of the first execution; they do not reverse it.
For the record, the first execution reported `Formatted 1580 files in 2287ms.` and also produced an
empty status difference, and it was itself preceded by a byte-order-mark repair to
`QuickFiler/Controllers/EfcFormController.cs` and `QuickFiler/Controllers/EfcDataModel.cs` that had
forced its own restart. Both files still carry their byte-order marks; the newly edited
`EfcFormControllerTests.Part2.cs` carries none at the merge base and carries none now, which is a
pure addition and not a stripped mark — its unstaged diffstat is 149 insertions and 0 deletions, so
line 1 is untouched.

## Before-status span

```
M  QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
AM QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs
M  QuickFiler.Test/Controllers/EfcFormControllerTests.cs
M  QuickFiler.Test/QuickFiler.Test.csproj
M  QuickFiler/Controllers/EfcDataModel.cs
M  QuickFiler/Controllers/EfcFormController.cs
A  TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs
M  TaskMaster.Test/TaskMaster.Test.csproj
A  TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs
M  TaskMaster/AppGlobals/AppOlObjects.cs
M  TaskMaster/TaskMaster.csproj
A  <feature-folder>/evidence/baseline/p0-t1-instructions-read.md
A  <feature-folder>/evidence/baseline/p0-t2-tree-baseline.md
A  <feature-folder>/evidence/baseline/p0-t3-csharpier-check.md
A  <feature-folder>/evidence/baseline/p0-t4-analyzer-rebuild.md
A  <feature-folder>/evidence/baseline/p0-t4-analyzer.min.log.txt
A  <feature-folder>/evidence/baseline/p0-t5-nullable-rebuild.md
A  <feature-folder>/evidence/baseline/p0-t5-nullable.min.log.txt
A  <feature-folder>/evidence/baseline/p0-t6-coverage.md
A  <feature-folder>/evidence/baseline/p0-t7-affected-classes.md
A  <feature-folder>/evidence/baseline/p0-t7/p0-t7-quickfiler.trx
A  <feature-folder>/evidence/baseline/p0-t7/p0-t7-taskmaster.trx
A  <feature-folder>/evidence/baseline/p0-t8-file-sizes.md
A  <feature-folder>/evidence/baseline/p0-t9-preexisting-counts.md
A  <feature-folder>/evidence/qa-gates/p6-t1-format.md
A  <feature-folder>/evidence/qa-gates/p6-t2-format-check.md
 A <feature-folder>/evidence/qa-gates/p6-t4-analyzer.min.log.txt
 A <feature-folder>/evidence/qa-gates/p6-t5-nullable.min.log.txt
A  <feature-folder>/evidence/regression-testing/p1-t10-frozen-contracts.md
A  <feature-folder>/evidence/regression-testing/p1-t10/p1-t10.trx
A  <feature-folder>/evidence/regression-testing/p1-t4-seam-build.md
A  <feature-folder>/evidence/regression-testing/p1-t7-finding1-red.md
A  <feature-folder>/evidence/regression-testing/p1-t7/p1-t7.trx
A  <feature-folder>/evidence/regression-testing/p1-t9-finding1-green.md
A  <feature-folder>/evidence/regression-testing/p1-t9/p1-t9.trx
A  <feature-folder>/evidence/regression-testing/p2-t10-finding2-green.md
A  <feature-folder>/evidence/regression-testing/p2-t10/p2-t10.trx
A  <feature-folder>/evidence/regression-testing/p2-t4-finding2-red.md
A  <feature-folder>/evidence/regression-testing/p2-t4/p2-t4.trx
A  <feature-folder>/evidence/regression-testing/p2-t6-seam-build.md
A  <feature-folder>/evidence/regression-testing/p2-t8-finding2-red.md
A  <feature-folder>/evidence/regression-testing/p2-t8/p2-t8.trx
A  <feature-folder>/evidence/regression-testing/p3-t2-finding5-red.md
A  <feature-folder>/evidence/regression-testing/p3-t2/p3-t2.trx
A  <feature-folder>/evidence/regression-testing/p3-t4-finding5-green.md
A  <feature-folder>/evidence/regression-testing/p3-t4/p3-t4.trx
A  <feature-folder>/evidence/regression-testing/p4-t2-seam-build.md
A  <feature-folder>/evidence/regression-testing/p4-t4-finding4-red.md
A  <feature-folder>/evidence/regression-testing/p4-t4/p4-t4.trx
A  <feature-folder>/evidence/regression-testing/p4-t6-finding4-green.md
A  <feature-folder>/evidence/regression-testing/p4-t6/p4-t6.trx
A  <feature-folder>/evidence/regression-testing/p4-t7-controller-class-green.md
A  <feature-folder>/evidence/regression-testing/p4-t7/p4-t7.trx
A  <feature-folder>/evidence/regression-testing/p5-t2-finding6-red.md
A  <feature-folder>/evidence/regression-testing/p5-t2/p5-t2.trx
A  <feature-folder>/evidence/regression-testing/p5-t5-finding6-green.md
A  <feature-folder>/evidence/regression-testing/p5-t5/p5-t5.trx
A  <feature-folder>/evidence/regression-testing/p5-t6-com-propagation-unchanged.md
 M <feature-folder>/plan.2026-09-02T12-02.md
?? <feature-folder>/evidence/qa-gates/p6-t3-file-sizes.md
?? <feature-folder>/evidence/qa-gates/p6-t4-analyzer-rebuild.md
?? <feature-folder>/evidence/qa-gates/p6-t5-nullable-rebuild.md
?? <feature-folder>/evidence/qa-gates/p6-t6-coverage.md
?? <feature-folder>/evidence/qa-gates/p6-t7-coverage-delta.md
?? <feature-folder>/evidence/qa-gates/p6-t8-newfile-coverage.md
?? <feature-folder>/evidence/regression-testing/p6-t13-kbd-success-path.md
?? <feature-folder>/evidence/regression-testing/p6-t13/
```

`<feature-folder>` above abbreviates
`docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736`; the abbreviation is
presentational only and every path was captured in full. Both spans carried 67 lines.

## After-status span

The after-status span is **line-for-line identical to the before-status span reproduced above**. The
identity is not asserted by inspection: a `Compare-Object` over the two captured spans returned an
empty difference set, recorded as `NO_STATUS_CHANGE`.

## Acceptance

**The set of paths whose status code changed between the before-status and the after-status is
empty.** The formatter rewrote nothing, so the clause requiring every such path to be a member of
the ratified Write Set holds with no member to check. In particular no path outside the Write Set
appears, which is the condition that would otherwise have indicated a defect in this item's own
edits.

`Formatted 1580 files in 2390ms.` reports the number of files processed, not the number rewritten;
the empty status difference is the observation establishing that it rewrote none of them. P6-T13's
own edits were formatted with `dotnet tool run csharpier format` on that file as they were written,
which is why the repository-wide pass found nothing left to do.

Output Summary: `dotnet tool run csharpier format .` exited 0 and printed
`Formatted 1580 files in 2390ms.` The `git status --porcelain` spans taken immediately before and
immediately after are identical, verified mechanically by an empty `Compare-Object` result, so no
file was rewritten and no path outside the ratified Write Set changed status. This is the second
execution of the task, triggered by the toolchain-loop restart that P6-T13 caused.
