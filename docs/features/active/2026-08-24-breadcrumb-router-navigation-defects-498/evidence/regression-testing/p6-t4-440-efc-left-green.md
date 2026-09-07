# P6-T4 — #440 Efc Left transition, pass-after (GREEN)

Timestamp: 2026-08-26T08-12

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment|FullyQualifiedName~HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t4"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 0

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t4/results.trx`

Output Summary:

- Test Run Successful. Total tests: 2, Passed: 2, Failed: 0.
- `HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment` — Passed (666 ms).
- `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior` — Passed (37 ms).
- Fix under test: the `Left` arm of `BreadcrumbBridgeRouter.HandleArrowKeyAsync` now attempts `row.ActivateSegment(row.ActiveSegmentIndex.Value - 1)` first and falls through to the pre-existing `row.LeftArrow()` behavior when that transition is refused.

Satisfies AC-15 and the AC-28 pass-after obligation for the Efc surface.
