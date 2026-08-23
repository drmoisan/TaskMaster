# Final QC Step 5 — Tests with Coverage (Issue #449, [P7-T6], [P7-T15])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command (repeats [P0-T12] verbatim, including the WORKTREE-relative `\.claude\` exclusion and
`/InIsolation`):
```
dotnet-coverage collect `
  --output <WORKTREE>\coverage\postchange-p7t6.cobertura.xml `
  --output-format cobertura `
  --settings coverage.config `
  -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" `
     <9 discovered assemblies> `
     /Settings:scripts\vscode\TaskMaster.cli.runsettings `
     /InIsolation `
     /TestCaseFilter:TestCategory!=LiveOutlook
```
EXIT_CODE: 0

Test-assembly discovery recursed from WORKTREE for `*.Test.dll`, keeping only paths whose suffix
**after WORKTREE** matches `\bin\Debug\` and excluding suffixes matching `\obj\`, `\ref\`, and
`\.claude\`. The exclusion is applied to the WORKTREE-relative suffix, never to the absolute path,
because WORKTREE itself lies under `.claude\worktrees\`. 18 raw matches, **9 retained** — the same 9
assemblies as the baseline run.

## Test counts

```
Test Run Successful.
Total tests: 6452
     Passed: 6452
```

| Metric | Baseline | Final | Delta |
| --- | --- | --- | --- |
| Total | 6437 | **6452** | **+15** |
| Passed | 6437 | **6452** | **+15** |
| **Failed** | 0 | **0** | 0 |
| **Skipped** | 0 | **0** | 0 |

**EXIT_CODE 0, zero tests failed.** The +15 delta is exactly the 15 test cases added by this change
(14 test methods, one of which is a `[DataTestMethod]` with two `[DataRow]` cases). No pre-existing
test was removed or renamed; see `suite-comparison-before-after.2026-08-22T09-16.md`.

### One flaky run was observed and resolved before this recorded result — disclosed in full

The FIRST attempt at this step returned `COVERAGE_EXIT=1` with `Total tests: 6452, Passed: 6451,
Failed: 1`. The single failure was:

```
Failed InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker [793 ms]
  Expected threadException to be <null> because the STA thread must not throw, but it threw:
  System.NullReferenceException: Object reference not set to an instance of an object.
     at UtilitiesCS.Threading.ProgressTrackerAsync.<InitializeAsync>d__6.MoveNext()
        in ...\UtilitiesCS\Threading\ProgressTrackerAsync.cs:line 35
```

That test is `UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests`, a WPF-`Dispatcher` STA test in
the `UtilitiesCS` project. It is **outside this change's scope in every sense**: this change edits only
`QuickFiler/Controllers/QfcExplorerController.cs`,
`QuickFiler/Interfaces/IQfcExplorerController.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, and two
new files under `QuickFiler.Test/Controllers/`. Nothing under `UtilitiesCS/Threading/` is touched, and
`QfcExplorerController` has no relationship to `ProgressTrackerAsync`.

It was diagnosed as load-related flakiness rather than a regression, by re-running the affected test
class in isolation:

Command:
```
vstest.console.exe "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~ProgressTrackerAsync_Tests"
```
EXIT_CODE: 0
Output: `Total tests: 8 / Passed: 8`, with
`Passed InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker [191 ms]`.

The test passed in isolation in 191 ms against 793 ms under full-suite load, which is the signature of
a Dispatcher-parking timing race under CPU saturation rather than a deterministic defect. The full
suite was then re-run and returned **6452 passed, 0 failed, EXIT_CODE 0** — the result recorded above —
and the independent second consecutive run in [P7-T7] also returned 6452/6452 with a byte-identical
pass set. Across the two clean consecutive runs the test passed twice.

**No test was modified, weakened, retried, or given a timing tolerance** to obtain this result. The
flake is disclosed here rather than silently discarded, and it is recorded as a latent
determinism defect in an unrelated project for separate promotion.

## Coverage values (numeric)

Read from `coverage\postchange-p7t6.cobertura.xml`.

| Value | Baseline | **Post-change** | Delta |
| --- | --- | --- | --- |
| Repo-wide root `line-rate` | 0.8532899236682991 = 85.3290 % | `0.8535709020220277` = **85.3571 %** | **+0.0281 pp** |
| Root `lines-covered` / `lines-valid` | 155,943 / 182,755 | **156,317 / 183,133** | +374 / +378 |
| `QuickFiler` package `line-rate` | 0.8091631603553062 = 80.9163 % | `0.8098982423681776` = **80.9898 %** | **+0.0735 pp** |
| `QfcExplorerController` | **absent from the report** | **87.8261 %** (101 / 115) | now measured |

All three required values are **numeric**; none is `UNVERIFIED`.

### The `QfcExplorerController` figure and why the aggregation seam was necessary

The figure aggregates **every** Cobertura `<class>` element whose `filename` ends with the path segment
`QuickFiler\Controllers\QfcExplorerController.cs`, summing hit and total line counts. The search
matched **four** elements:

```
QFCEXPL_MATCHED_CLASS_COUNT=4
  QuickFiler.Controllers.QfcExplorerController
  QuickFiler.Controllers.QfcExplorerController.<>c
  QuickFiler.Controllers.QfcExplorerController.<>c__DisplayClass24_0
  QuickFiler.Controllers.QfcExplorerController.<OpenQFItem>d__24
QFCEXPL_LINES_HIT=101
QFCEXPL_LINES_TOTAL=115
QFCEXPL_LINE_RATE_PCT=87.8261
```

This empirically confirms the plan's coverage-measurement seam. `OpenQFItem` is `async`, so the
compiler emits its state machine as the separate `<OpenQFItem>d__24` element; the lambdas passed to
`Task.Run` emit the `<>c` lambda cache and the `<>c__DisplayClass24_0` closure. Reading a single
`<class>` element would have reported a figure for a fragment of the file. The direct
`dotnet-coverage collect` invocation performs no closure post-processing, so all four elements are
present in the raw report.

Baseline matched **zero** such elements, because the class-level `[ExcludeFromCodeCoverage]` suppressed
every member including the compiler-generated ones — which is why the baseline value is recorded as
absent rather than 0 %.

### Denominator caveat carried forward to [P7-T9]

This direct `dotnet-coverage collect --settings coverage.config` invocation does not apply the
effective-config test-assembly `ModulePath` exclusion that `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
derives, so the nine `*.Test` packages are IN this denominator. The absolute repo-wide figure is
therefore not directly comparable to the 80 % helper gate at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`. Baseline and post-change runs use the
identical method, so the DELTA is sound even though the absolute value is not the gated figure.

---

## [P7-T15] — No Python toolchain step was run, because none exists

Command: `ls -d scripts/dev_tools` -> EXIT_CODE 2,
`ls: cannot access 'scripts/dev_tools': No such file or directory`
Command: `ls -1 pyproject.toml poetry.lock` -> EXIT_CODE 2, both absent
Command: `ls -1 scripts/` -> EXIT_CODE 0, output `dev-tools/`, `temp-extract-coverage.ps1`, `vscode/`
Command: `ls -1 scripts/dev-tools/` -> EXIT_CODE 0, output `run-actionlint.ps1`
Command: `git ls-files "*.py"` -> EXIT_CODE 0, 2 files, both inside
`docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/`

There is **no `scripts/dev_tools/` directory** (the only similar path is `scripts/dev-tools/`,
hyphenated, holding a single PowerShell script) and **no Poetry manifest**. The importable package
`scripts.dev_tools` does not exist and there is no Poetry environment to run it in, so any skill step
naming `poetry run python -m scripts.dev_tools.*` is **unrunnable by absence**. It is recorded here as
such: **no result is fabricated for it and it is not silently omitted.**

C# coverage in this plan is collected by `dotnet-coverage` and read from the Cobertura report. No
Python coverage runner exists here to consume a coverage-target argument, which is why no task in this
plan states one and why no `--cov` value appears anywhere in it.

## Output Summary

Final QC suite: **6,452 total, 6,452 passed, 0 failed, 0 skipped**, EXIT_CODE 0, across the same 9
discovered assemblies with `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook`. The +15
delta over the 6,437-test baseline is exactly the tests this change adds. Coverage: repo-wide root line
rate **85.3571 %** (up 0.0281 pp), `QuickFiler` package **80.9898 %** (up 0.0735 pp), and
`QfcExplorerController` **87.8261 %** (101/115) aggregated across four `<class>` elements — previously
absent from the report entirely. One unrelated `UtilitiesCS` Dispatcher STA test flaked on a first
attempt, was proven flaky by passing in isolation and in two subsequent clean consecutive full-suite
runs, and is disclosed rather than concealed; no test was modified to obtain the result. No Python
toolchain step was run because none exists in this repository.
