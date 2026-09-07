# Final QC Stage 4 — `QuickFiler.Test` Gate ([P8-T5])

Timestamp: 2026-08-28T06-25

Command (under `pwsh -NoProfile` from the worktree root, vstest path resolved in `[P0-T4]`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings "/Logger:trx;LogFileName=final-quickfiler-test.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\qa-gates\trx-p8-t5
```

EXIT_CODE: 0

## Counts

| Measure | Value |
| --- | --- |
| Total | **1201** |
| Passed | **1201** |
| Failed | **0** |
| Skipped / not executed | **0** |
| Total time | 11.9796 seconds |
| Run result | `Test Run Successful.` |

Corroborated by the TRX `ResultSummary/Counters` element: `total="1201" passed="1201" failed="0"
notExecuted="0"`.

## Gate 1 — failing-test-name set is a subset of `BASELINE_FAILURE_SET`

| | Value |
| --- | --- |
| `BASELINE_FAILURE_SET` (`[P0-T12]`) | **(empty)** |
| Observed failing set | **(empty)** |

Because the baseline set is empty, the subset condition reduces to failed count 0 with `EXIT_CODE: 0`,
and both hold. **No failing name appears at all**, so no name outside the baseline set appears and no
name added by this feature appears.

## Gate 2 — passed count against `BASELINE_PASSED` + 9

| Quantity | Value |
| --- | --- |
| `BASELINE_PASSED` (`[P0-T12]`) | 1192 |
| Required minimum (`BASELINE_PASSED` + 9) | **1201** |
| Observed passed | **1201** |

The requirement is met exactly. The **+9** is ten added test methods minus the one deleted test method.

### The ten added test result names, all `Passed`

| # | Test | Unit |
| --- | --- | --- |
| 1 | `ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement` | D1 |
| 2 | `ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost` | D2 |
| 3 | `InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` | D3 |
| 4 | `InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` | D3 |
| 5 | `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic` | D4 |
| 6 | `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` | D4 |
| 7 | `InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException` | D5 |
| 8 | `LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException` | #475 |
| 9 | `ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` | #475 |
| 10 | `CaptureCurrent_NullAndControlledContexts_FailFastAndCapture` | #475 |

Every one was located in the final TRX with `outcome="Passed"`. Each is a plain `[TestMethod]` with no
`[DataRow]`, so each contributes exactly one test result.

### The one deleted test

`CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries` — searched for in the final
TRX and **not found**, confirming it no longer runs. Test 10 above replaces it.

## Host-identity scrub of the TRX evidence

`/EnableCodeCoverage` deposited a binary coverage attachment into the results directory whose **filename
embedded the account name and machine name** (`<account>_<host>_<timestamp>.coverage`). That filename is
produced by the collector and is not controllable through `LogFileName=`.

The plan's fail-closed rule states that **no artifact may embed a user account name, a home directory,
or a machine name**. Two corrections were therefore applied:

1. **The `.coverage` attachments were deleted** from the results directory, together with their
   now-empty attachment folder. They are not required by this task, whose acceptance concerns test
   counts and names; the coverage figures this plan needs come from the Cobertura runs in `[P0-T14]` and
   `[P8-T6]`.
2. **All 19 TRX files under the feature evidence tree were scrubbed.** The TRX format records
   `computerName="<host>"` on every `UnitTestResult` element and embeds the absolute worktree path, so
   the leak was systemic across every phase's TRX rather than specific to this one. The substitutions
   were: the machine name to `REDACTED-HOST`, the absolute worktree root to `<worktree-root>`, and any
   residual account name to `REDACTED-USER`.

Verification after the scrub: a recursive search of the entire feature evidence tree returns **0** files
containing the machine name and **0** containing the account name. The TRX counters are untouched —
`total="1201" passed="1201" failed="0"` — as are all test names, outcomes, and error messages, so no
evidence content was lost.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/trx-p8-t5/final-quickfiler-test.trx`

Output Summary: EXIT_CODE 0. **1201 total, 1201 passed, 0 failed.** The observed failing-test-name set is
**empty** and therefore a subset of the empty `BASELINE_FAILURE_SET`. The passed count **1201** equals
`BASELINE_PASSED` 1192 plus **9**, meeting the minimum exactly; all ten added tests are enumerated above
with outcome `Passed`, and the one deleted test is absent from the TRX. Host-identity leakage introduced
by `/EnableCodeCoverage` was removed and all 19 evidence TRX files were scrubbed, with zero residual
account-name or machine-name references and all counters preserved.
