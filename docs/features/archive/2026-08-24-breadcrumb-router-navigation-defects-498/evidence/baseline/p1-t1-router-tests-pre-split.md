# P1-T1 — Pre-Split Router Test Baseline

Timestamp: 2026-08-26T08-51

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouter" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/baseline/trx/p1-t1"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

`Test Run Successful.` Counts read from the `<Counters>` element of the TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/baseline/trx/p1-t1/results.trx`:

| Metric | Value |
|---|---:|
| Total | **40** |
| Passed | **40** |
| Failed | **0** |

Verbatim from the TRX: `total="40" executed="40" passed="40" failed="0" error="0" timeout="0"
aborted="0" inconclusive="0" notExecuted="0"`. Total time 1.5544 seconds.

### Failing-test identifier set

```
(empty — zero failing tests pre-split)
```

### Class breakdown of the 40 matched tests

The filter `FullyQualifiedName~BreadcrumbBridgeRouter` matched three test classes in
`QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`:

| Test class | Tests matched |
|---|---:|
| `BreadcrumbBridgeRouterTests` | 16 |
| `BreadcrumbBridgeRouterQueueTests` | 14 |
| `BreadcrumbBridgeRouterIssue439Tests` | 10 |

The `BreadcrumbBridgeRouterIssue439Tests` count of 10 agrees with the ten `[TestMethod]` members
catalogued by `P0-T8`, including the three `Issue609_*` methods added by pull request #611.

### Comparison basis for `P1-T3`

`P1-T3` must reproduce, after the decision-D8 partial-class split, all three of:

- total = **40**
- passed = **40**
- failing-test identifier set = **empty**

These three values are the comparison basis and are not asserted against any absolute value here. Any
divergence in `P1-T3` means the `P1-T2` relocation was not mechanical.

### Notes on artifact hygiene

The run wrote exactly one results file, named `results.trx` by the explicit
`/Logger:trx;LogFileName=results.trx`, into the explicit `/ResultsDirectory:` named by the task, so no
results file is named after the machine or the user account. The TRX's own
`TestRun/@name`, `TestRun/@runUser` and `Deployment/@runDeploymentRoot` attributes are emitted by
`vstest.console.exe` itself and are retained unmodified, matching the existing convention of the 50 TRX
files already tracked in this repository under other feature folders. No count, test identifier or
outcome in the file was edited.
