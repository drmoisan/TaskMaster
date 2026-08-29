# [P6-T5] Full suite with code coverage (Issue 638)

Timestamp: 2026-08-29T12-38

Command:

```
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
# assembly list built by the rules below
& $vs <assembly list> /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p6-t5 /TestCaseFilter:"TestCategory!=LiveOutlook"
```

The `vstest.console.exe` path is recorded unresolved, as the vswhere expression, because
the resolved path is absolute. The `| Select-Object -First 1` suffix is required because
`-find` can emit several matching paths. The run was launched through
`Start-Process -Wait -NoNewWindow`, with output redirected under `TestResults\`
(gitignored under `.gitignore:39`).

EXIT_CODE: 0

Output Summary:

## Assembly discovery

`Get-ChildItem -Path . -Recurse -Filter '*.Test.dll'`, keeping only paths under
`\bin\Debug\`, rejecting paths under `\obj\` or `\ref\`, and rejecting only **relative**
paths containing `\.claude\`, where the relative path is computed as
`$_.FullName.Substring((Get-Location).Path.Length)`. The absolute path is deliberately not
tested for `\.claude\`: a worktree rooted under `.claude\worktrees` would match on every
candidate and yield an empty assembly list, which vstest reports as a run with zero
failures.

DISCOVERED_ASSEMBLY_COUNT: 9

Worktree-relative paths, computed as `$_.FullName.Substring((Get-Location).Path.Length)`:

```
\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
\SVGControl.Test\bin\Debug\SVGControl.Test.dll
\Tags.Test\bin\Debug\Tags.Test.dll
\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
\TaskTree.Test\bin\Debug\TaskTree.Test.dll
\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

The count is at least 4, and the list contains a path ending `QuickFiler.Test.dll` and a
path ending `TaskMaster.Test.dll`.

## Run totals

Console totals:

```
Total tests: 6870
     Passed: 6870
```

No `Failed:` or `Skipped:` summary line was emitted, so both are 0. The total is the 6859
of the [P0-T12] direct-harness baseline plus the 11 new tests.

## Per-namespace failure figures, derived from the TRX

Derived from the single TRX under `TestResults\p6-t5` by joining each
`UnitTestResult/@testId` to its `UnitTest/TestMethod` `className` and `name`, not from the
console `Failed:` total, which is an all-assembly aggregate:

```
TRX result rows:                6870   (Passed 6870, Failed 0)
QUICKFILER_NS_FAILED:              0   (fully qualified name begins "QuickFiler.")
TASKMASTER_NS_FAILED:              0   (fully qualified name begins "TaskMaster.")
Failures in any other namespace:   0
```

Baseline exceptions taken: **none**. The direct-harness `BASELINE_FAILURE_SET:` recorded by
[P0-T12] in `evidence/baseline/p0-t12-direct-harness-baseline.md` is the literal `none`, so
no carve-out was available and none was needed. No test in `EfcDataModelArchiveRootTests`
appears among any failures, because there are none.

Because the exception list is empty and both namespace figures are `Failed: 0`, [P8-T18]
takes the AC16 check-off branch and no `REMEDIATION-REQUIRED:` line is appended here.

## New tests executed

ARCHIVEROOT_TESTS_EXECUTED: 11 — all eleven tests whose fully qualified name contains
`EfcDataModelArchiveRootTests` appear in the TRX, so [P8-T13]'s AC11 precondition is met.

## LiveOutlook category

Executed tests carrying `[TestCategory("LiveOutlook")]`: **0**. Counted from the TRX by
intersecting the executed `testId` set with the `TestDefinitions` entries whose
`TestCategory/TestCategoryItem/@TestCategory` equals `LiveOutlook`. The run's
`/TestCaseFilter:"TestCategory!=LiveOutlook"` matches
`.github/workflows/_mstest-coverage.yml:83`.
