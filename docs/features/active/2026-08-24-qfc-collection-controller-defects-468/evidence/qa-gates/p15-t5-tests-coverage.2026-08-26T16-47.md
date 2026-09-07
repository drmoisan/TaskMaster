# [P15-T5] Final QA loop, step 4 — full-suite tests with coverage

Timestamp: 2026-08-26T16-47

Command:

```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . -Configuration Debug `
    -CoverageOutput docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\coverage-final.cobertura.xml
```

EXIT_CODE: 0

ExpectedExitCode: 0

Coverage artifact:
`docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/coverage-final.cobertura.xml`
(10,669,373 bytes).

## Output Summary

**Test Run Successful. Total tests: 6581, Passed: 6581, Failed: 0, Skipped: 0. Total time 51.9390
seconds. Root Cobertura `line-rate` = 84.9435%, `branch-rate` = 78.9377%.**

### Test counts

```
Test Run Successful.
Total tests: 6581
     Passed: 6581
 Total time: 51.9390 Seconds
```

Independently confirmed by counting per-test result lines in the run log: **6581** lines matching
`^  Passed `, **0** matching `^  Failed `, **0** matching `^  Skipped `.

| Metric | Value |
|---|---|
| Total tests | **6581** |
| Passed | **6581** |
| **Failed** | **0** |
| Skipped | **0** |

Nine test assemblies were discovered and executed in one vstest invocation: `QuickFiler.Test`,
`SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`,
`ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`. No test-host crash occurred and no retry
was needed; this is the first attempt.

### Numeric coverage, from the root `<coverage>` element

```
line-rate="0.849435" branch-rate="0.789377" complexity="25189" version="1.9"
lines-covered="54143" lines-valid="63740" branches-covered="12840" branches-valid="16266"
```

| Metric | Value as a percentage |
|---|---|
| **`line-rate`** | **84.9435%** |
| **`branch-rate`** | **78.9377%** |

Absolute counts: 54,143 of 63,740 lines covered; 12,840 of 16,266 branches covered.

### Skipped count for the five new test files' classes

**Zero.** The suite-wide skipped count is 0, so no class anywhere was skipped. Beyond that
aggregate, each of the 28 test methods declared across the five new test files was verified
individually to appear as `Passed` in the run log:

| Test file | Test methods | All passed? |
|---|---|---|
| `QfcCollectionController.TestSupport.cs` | 0 (helper file, no `[TestMethod]`) | n/a |
| `QfcCollectionControllerDefects468Tests.cs` | 8 | yes |
| `QfcCollectionControllerDefects468MoveTests.cs` | 9 | yes |
| `QfcCollectionControllerDefects468ConversationTests.cs` | 9 | yes |
| `QfcCollectionControllerLayout.StaTests.cs` | 2 | yes |
| **total** | **28** | **28 of 28** |

Zero of the 28 were skipped, and the `[STATestClass]` in `QfcCollectionControllerLayout.StaTests.cs`
executed normally under the coverage runner — an STA class silently skipping under instrumentation is
the specific failure this clause exists to catch.

### Reconciliation of the total against the P0-T14 baseline

| Run | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| P0-T14 baseline | 6482 | 6482 | 0 | 0 |
| P15-T5 (this run) | **6581** | **6581** | **0** | **0** |
| delta | **+99** | +99 | 0 | 0 |

The 99 added tests are the 28 this feature adds plus 71 brought in by the two merges of
`origin/epic/quickfiler-bug-family-integration`. No test was removed and no test regressed.

`QuickFiler.Test` alone contributed **1024** passed tests, the same figure as the standalone P13-T7
run. A naive segmentation of the log by `Test Parallelization enabled for` markers reports 1023 for
that assembly, because one result line —
`Passed ItemViewerQueue_ResetCoreForTesting_UsesResettableProductionDefaults` at log line 1036 —
printed one line **after** the `SVGControl.Test` marker at line 1035. That is console interleaving
between assemblies running concurrently, not a missing test. Comparing the two full name sets shows
the sets are identical; only the print position differs.

## Runner substitution: how the policy's `vstest.console.exe /EnableCodeCoverage` step is discharged

CLAUDE.md §CUT3 step 4 names `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. That step
is discharged here by `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, the repository's canonical
coverage runner. The substitution is deliberate and the runner is a strict superset of the policy
command:

- The runner resolves the same `vstest.console.exe` through `vswhere`:
  `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
- It discovers the test assemblies rather than requiring them to be listed —
  `Discovered 9 test assemblies.`
- Instead of `/EnableCodeCoverage`, it wraps the vstest invocation in
  `dotnet-coverage collect --settings coverage.config`, version `18.5.2.0 [win-x64 - .NET 10.0.10]`.
  The inner runsettings (`scripts/vscode/TaskMaster.cli.runsettings`) carries the MSTest
  parallelization only and **no** coverage data collector, so the Code Coverage collector is never
  activated twice; instrumentation comes solely from the outer `dotnet-coverage` path. This is what
  makes the exclusions in `coverage.config` effective, which `/EnableCodeCoverage` alone would not
  guarantee.
- It passes `/InIsolation`, which is mandatory for the Moq-based assemblies in this repository.
- It passes `/TestCaseFilter:TestCategory!=LiveOutlook`, excluding tests that require a live Outlook
  process. No test in `QuickFiler.Test` carries that category, so the filter removed nothing from
  this feature's scope.
- It emits Cobertura directly and post-processes the document for Koverage compatibility, which is
  why the artifact carries a root `line-rate`/`branch-rate` pair that the raw
  `/EnableCodeCoverage` `.coverage` binary format does not.

## Host-identifier hygiene

The committed coverage XML was scanned case-insensitively for all five host-identifier patterns after
the runner's post-processing step:

| Pattern class | Occurrences |
|---|---|
| account name | **0** |
| 8.3 short-name form of the account name | **0** |
| machine name | **0** |
| drive-letter-plus-`Users` absolute-path prefix | **0** |
| worktree directory name | **0** |

No sanitisation pass was required. The runner's Koverage post-processing rewrites every `filename`
attribute to a repository-relative path and the `<source>` element to `.`, so no absolute path
survives into the artifact.

## Acceptance verification

| Clause | Status |
|---|---|
| `EXIT_CODE: 0` | met |
| a failed count of exactly `0` | met — `Failed: 0`, and 0 lines matching `^  Failed ` |
| `Output Summary:` carries the numeric post-change `line-rate` and `branch-rate` as percentages | met — **84.9435%** and **78.9377%** |
| a skipped count of `0` for the five new test files' classes | met — suite-wide skipped is 0; all 28 methods verified `Passed` individually |
| the runner-substitution statement present | met — see the section above |
