# Phase 6 — Coverage-Enabled Full Test Suite (final pass, GREEN)

Timestamp: 2026-08-27T14-19
Task: [P6-T5]
Command: `pwsh -NoProfile -File "scripts\vscode\Invoke-MSTestWithCoverage.ps1" -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
EXIT_CODE: 0

RunDisposition: CLEAN

Started 2026-08-27T14:18:12Z, ended 2026-08-27T14:19:36Z. This is the exact command from [P0-T9].

## Acceptance status

**This task meets its acceptance condition.** Zero failed tests; a numeric repository-wide
line-rate is recorded; five numeric per-file line-rates are recorded.

## Output Summary

### Test counts

```
Test Run Successful.
Total tests: 6701
     Passed: 6701
 Total time: 46.0644 Seconds
```

- Passed: **6701**
- Failed: **0**
- Skipped: **0** (none reported)

The [P0-T9] baseline was 6503 total / 6503 passed / 0 failed. The total rose by 198. Two
independent causes:

- This feature added 17 tests across its two owned test files and deleted 2
  (`QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract` by [P1-T8] and
  `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` by [P5-T11]), a net +15.
- The remaining +183 arrived with the recorded merge of
  `origin/epic/quickfiler-bug-family-integration` (merge commit `c1826965`), which brought in
  sibling epic children's test suites.

Both deleted tests are confirmed absent from the run: a search of the run log for either name
returns zero occurrences.

### The previously failing test now passes

The prior run of this gate (`evidence/qa-gates/mstest-coverage.2026-08-26T11-30.md`) recorded
`TESTS_FAILED (1 failed test)`:

`QuickFiler.Controllers.Tests.EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`

It threw `System.ArgumentException: Object of type 'System.Boolean' cannot be converted to type
'System.Int32'` because the test injected a boxed `bool` by reflection into `_isExecuting`, which
[P3-T5] converted to `private int`. Commit `889fa298` changed
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:64` to pass `1`. **This run confirms the
test now passes** rather than assuming it: the run log records
`Passed ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`.

That write is on this plan's forbidden-to-write list and is recorded as a documented deviation, not
as a clean gate. See `evidence/qa-gates/ownership-gate.2026-08-27T14-03.md`.

### Named tests, all passing

Each acceptance-criterion test named by the plan and the spec is confirmed `Passed` exactly once in
the run log:

`WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`,
`WriteMetricsAsync_CompletesWriterTaskBeforeReturning`,
`WriteMetricsAsync_PassesUncancelledTokenToWriter`,
`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`,
`WriteMetricsAsync_ReadsMovedStopwatchForDuration`,
`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`,
`WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`,
`BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration`,
`BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding`,
`BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`,
`BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields`,
`BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields`,
`BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator`,
`StopWatch_AfterControllerConstruction_IsRunning`,
`TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse`,
`TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue`,
`QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow`,
`QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload`.

### Coverage artifact state

Neither of the runner's two run-related throws fired. `Invoke-MSTestWithCoverage.ps1:236` did not
fire because the dotnet-coverage/vstest process exited zero. `Invoke-MSTestWithCoverage.ps1:341`
did not fire because `Assert-CoberturaLineCoverageThreshold` found the repository-wide line rate
above the 80 percent floor. The run therefore reached `ConvertTo-KoverageCoberturaXml` at `:340`
and the `Set-Content` at `:344`, and the log's closing lines confirm it:

```
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <repo-root>/coverage/coverage.cobertura.xml
```

The Cobertura document on disk is the **post-processed** form: repository-relative `filename`
attributes, a `<sources>` element, third-party packages excluded from the denominator, and one
pre-merged `<class>` element per source file. It is therefore directly comparable with the
[P0-T9] baseline document, which was also post-processed. The fallback reading procedure the task
describes for a `COVERAGE_FLOOR_TRIPPED` run was not needed on either side of the comparison.

The Cobertura file itself is not committed: `coverage/coverage.cobertura.xml` is covered by the
`coverage/*` rule at `.gitignore:144`, and this repository's standing convention is against
committing raw coverage output. The figures below are the record.

### Repository-wide rates

Read from the attributes of the root `<coverage>` element:

| Metric | Raw attribute | Percentage |
| --- | --- | --- |
| `line-rate` | `0.851255` | **85.13%** |
| `branch-rate` | `0.792096` | **79.21%** |

Supporting counters: `lines-covered="54379"`, `lines-valid="63881"`,
`branches-covered="12927"`, `branches-valid="16320"`.

Both figures clear the repository thresholds: the 80 percent line floor in CLAUDE.md § UT2, the
85 percent line floor in `.claude/rules/general-unit-test.md`, and the 75 percent branch floor in
`.claude/rules/quality-tiers.md`.

### Per-file line-rate, five owned production files

Each figure is computed by collecting every `<line>` element from every `<class>` element sharing
the same `filename` attribute, keying by line number so a line contributed by more than one class
is counted once, and dividing the number of line numbers with hits greater than zero by the number
of distinct line numbers. This is the identical aggregation applied to the [P0-T9] baseline, so the
two sides are method-comparable.

| Owned production file | Line-rate | Covered | Total | `<class>` elements merged |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | **76.23%** | 170 | 223 | 1 |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | **80.00%** | 88 | 110 | 1 |
| `QuickFiler/Controllers/EfcHomeController.cs` | **98.25%** | 224 | 228 | 1 |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | **100.00%** | 64 | 64 | 1 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | **89.86%** | 62 | 69 | 1 |

All five values are numeric. The delta against the baseline, and the treatment of the one file that
moved down, are recorded in `evidence/qa-gates/coverage-delta.2026-08-27T14-19.md`.

Instrumentation was performed by the same `dotnet-coverage` toolchain used for the baseline.

### Note on the aborted attempt earlier in this session

An attempt to run this gate was already in flight in this worktree when the session resumed, having
been launched at 2026-08-27T13:46:27Z. It was subsequently confirmed hung: 28.7 seconds of CPU
accumulated across 30 minutes of wall time, no test-result file written and no coverage output
written. Its inputs had also been invalidated, because a `/t:Rebuild` at 2026-08-27T13:55 replaced
the assemblies it had loaded. It was terminated and the toolchain restarted from [P6-T1]. The run
recorded above is the only run of this gate whose result is claimed. Full sequence:
`evidence/qa-gates/toolchain-loop.2026-08-27T14-18.md`.
