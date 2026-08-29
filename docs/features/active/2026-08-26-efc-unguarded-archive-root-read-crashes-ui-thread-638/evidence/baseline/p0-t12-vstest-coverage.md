# [P0-T12] Baseline test suite in coverage mode (Issue 638)

Timestamp: 2026-08-29T12-20

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`

Run through `Start-Process -Wait -NoNewWindow`, with standard output and standard error
redirected to `TestResults\p0-t12-harness.out.log` and `TestResults\p0-t12-harness.err.log`
(gitignored under `.gitignore:39`), because the full suite takes longer than a single
foreground command window is reliable for.

EXIT_CODE: 1

`ExpectedExitCode:` is deliberately **not** declared. See "Exit-code justification" below.

Output Summary:

## vstest counts observed in this coverage-harness run

```
Total tests: 6859
     Passed: 6858
     Failed: 1
```

`Skipped:` was not emitted by the runner for this run, so the skipped count is 0.

Total time reported: 46.7143 seconds.

## COVERAGE_HARNESS_FAILURE_SET

COVERAGE_HARNESS_FAILURE_SET: `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`

The recorded failure message is a FluentAssertions timing assertion:

```
Expected entered.Task.Wait(TimeSpan.FromSeconds(5)) to be True because the started worker must reach the injected loader, but found False.
```

The stack trace is deliberately not reproduced here: its frames carry absolute source
paths, and no artifact this plan writes may contain an absolute filesystem path, an account
name or a machine name. The exception surface is a FluentAssertions assertion failure
raised from a five-second wall-clock wait, in a test unrelated to the archive-root read
sites this change touches.

`BASELINE_FAILURE_SET:` is **not** set from this run. It is set in the companion artifact
`p0-t12-direct-harness-baseline.md` from the second, direct-harness run, so that the
carve-out list [P6-T5] consumes is commensurable with the harness that gates it.

## Coverage

Read from the root `coverage` element of `coverage/coverage.cobertura.xml`:

```
line-rate      = 0.7069679346308415
branch-rate    = 0.5934052103872132
lines-covered  = 58228
lines-valid    = 82363
package count  = 14
```

BASELINE_REPO_LINE_COVERAGE_PERCENT: 70.70

COVERAGE_XML_MODE: raw

The script exited non-zero, so per the task rule the mode is `raw`.
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:341` asserts the threshold on the
post-processed content and `:343` writes that content back only afterwards; this run
terminated at `:236`, well before either, so the file on disk carries the raw
`dotnet-coverage` root attributes computed over every instrumented module — including the
fourteen packages listed, which contain `.Test` assemblies that
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:417-421` would have removed and
`:442-445` would have recomputed around.

## Exit-code justification — REMEDIATION-REQUIRED

The task text authorizes `ExpectedExitCode: 1` only when the terminating message emitted by
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:489` — the one ending with the
literal `is below the required 80% threshold.` — is present in the run output.

A `Select-String -SimpleMatch 'is below the required 80% threshold.'` over both redirected
logs returned **0** matches. The literal is absent.

The non-zero exit came from a different cause. The terminating message was raised at
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:236`:

```
MSTest with coverage failed with exit code 1
```

preceded by `Test Run Failed.` — that is, the single failing test above, not the coverage
threshold. The script therefore never reached its `:341` threshold assert.

Per the task's own branch, this artifact records the observed `EXIT_CODE:` without an
`ExpectedExitCode:` field, and the task outcome is:

REMEDIATION-REQUIRED: [P0-T12] coverage-harness run exited 1 for a cause other than the
80 percent coverage threshold — one pre-existing failing test,
`QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`,
which is a five-second wall-clock timing assertion in a test file this change does not
touch. The numeric coverage value required by the acceptance is still readable and is
recorded above, and both `COVERAGE_XML_MODE:` and `COVERAGE_HARNESS_FAILURE_SET:` are
recorded, so the artifact's own field-completeness acceptance is met.
