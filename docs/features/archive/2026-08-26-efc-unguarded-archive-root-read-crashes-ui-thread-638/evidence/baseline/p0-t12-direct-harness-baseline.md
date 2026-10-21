# [P0-T12 companion] Direct-harness baseline failure set (Issue 638)

Timestamp: 2026-08-29T12-23

Command:

```
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
# assembly list built by the [P6-T5] rules (see below)
& $vs <assembly list> /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p0-t12-direct /TestCaseFilter:"TestCategory!=LiveOutlook"
```

The `vstest.console.exe` path is recorded unresolved, as the vswhere expression, because
the resolved path is absolute. The run was launched through `Start-Process -Wait
-NoNewWindow`, with output redirected to `TestResults\p0-t12-direct.out.log` and
`TestResults\p0-t12-direct.err.log` (gitignored under `.gitignore:39`).

EXIT_CODE: 0

`ExpectedExitCode:` is omitted, per the task rule: this run exited 0 and its
`BASELINE_FAILURE_SET:` is the literal `none`.

Output Summary:

## Assembly list

Built by the [P6-T5] rules: `Get-ChildItem -Path . -Recurse -Filter '*.Test.dll'`, keeping
only paths under `\bin\Debug\`, rejecting paths under `\obj\` or `\ref\`, and rejecting
only **relative** paths containing `\.claude\`, where the relative path is computed as
`$_.FullName.Substring((Get-Location).Path.Length)`.

DISCOVERED_ASSEMBLY_COUNT: 9

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

## Direct-harness counts

```
DIRECT_HARNESS_TOTAL_TESTS: 6859
DIRECT_HARNESS_PASSED:      6859
DIRECT_HARNESS_FAILED:      0
DIRECT_HARNESS_SKIPPED:     0
```

The runner emitted `Test Run Successful.` and no `Failed:` or `Skipped:` summary line, so
both counts are 0. Total time reported: 51.7657 seconds.

## BASELINE_FAILURE_SET

BASELINE_FAILURE_SET: none

This is the carve-out list [P6-T5] consumes. It is empty, so [P6-T5] admits no
baseline exception and [P8-T18] can only take the AC16 check-off branch if [P6-T5] itself
records `Failed: 0` for both namespaces.

## Divergence against the coverage-harness run

COVERAGE_HARNESS_FAILURE_SET (copied from `p0-t12-vstest-coverage.md`):
`QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`

Tests appearing in one set and not the other:

- `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`
  — present in `COVERAGE_HARNESS_FAILURE_SET:`, absent from `BASELINE_FAILURE_SET:`.

Both runs executed the same 6859 tests. The divergence is therefore a test whose outcome
depends on the instrumentation path rather than on any source change: it asserts that a
started worker reaches an injected loader within a five-second wall-clock wait, and the
`dotnet-coverage`-instrumented harness is slower than the direct
`vstest.console.exe /EnableCodeCoverage` harness. It is not a defect introduced by this
change, which had not yet been made when both runs executed.
