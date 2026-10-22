# P1-T3 — Post-Split Router Test Run (behavior-neutrality proof)

Timestamp: 2026-08-26T08-55

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouter" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p1-t3"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

`Test Run Successful.` The `P1-T2` relocation is proved behavior-neutral: every value required to match
the `P1-T1` pre-split baseline matches exactly.

### Comparison against the `P1-T1` baseline

| Value | `P1-T1` (pre-split) | `P1-T3` (post-split) | Equal? |
|---|---:|---:|:--:|
| Total | 40 | **40** | YES |
| Passed | 40 | **40** | YES |
| Failed | 0 | **0** | YES |
| Failing-test identifier set | empty | **empty** | YES |

`<Counters>` verbatim from
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p1-t3/results.trx`:
`total="40" executed="40" passed="40" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
notExecuted="0"`. Total time 1.4850 seconds.

### Failing-test identifier set

```
(empty — zero failing tests post-split)
```

### Additional check: the executed test-name sets are identical, not merely equinumerous

Beyond the totals the task requires, the 40 `testName` values were extracted from both TRX files,
sorted, and compared. The comparison returned no difference, so the post-split run executed the *same
40 tests*, not a different 40 that happen to sum to the same count. This closes the residual
possibility that the split silently removed one test from discovery while another became discoverable.

Class distribution is likewise unchanged from `P1-T1`: `BreadcrumbBridgeRouterTests` 16,
`BreadcrumbBridgeRouterQueueTests` 14, `BreadcrumbBridgeRouterIssue439Tests` 10.

### Interpretation

`BreadcrumbBridgeRouterIssue439Tests` — a MUST-NOT-WRITE class carrying the inherited pull request #605
and #611 regression coverage — is among the three classes exercised here, and all ten of its methods
pass after the split. That is early corroboration for `P7-T8`, which re-runs the same class as a
dedicated gate.

No divergence of any kind was observed, so the relocation stands as mechanical and `P1-T2` is not
reopened.
