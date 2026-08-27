# P6-T2 — #440 Efc Left transition, fail-before (RED)

Timestamp: 2026-08-26T08-05

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment|FullyQualifiedName~HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t2"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 1

ExpectedExitCode: 1

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t2/results.trx`

Output Summary:

- Test Run Failed. Total tests: 2, Failed: 2, Passed: 0. This is the expected fail-before state for the Efc Left transition of #440.
- `HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment` — Failed. Cause: `Moq.MockException`. The pre-fix `Left` arm of `HandleArrowKeyAsync` calls only `row.LeftArrow()`, which sets `CollapsedAfterIndex` and leaves `ActiveSegmentIndex` on the leaf. The subsequent `leafExpandToggle` therefore takes the `row.IsCollapsed` re-expand branch and never reaches `ExpandLeafAsync`, so `GetImmediateSubfoldersAsync` is invoked zero times with the parent key `Inbox\Projects` against the expected `Times.Once`.
- `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior` — Failed. Cause: FluentAssertions count mismatch, "Expected renders to contain 3 item(s), but found 2". Pre-fix, the first two Left presses each produce a collapse render and the third is refused by `row.LeftArrow()` at the root, so only two row renders are posted instead of the three the tree transition plus decision-D1 fall-through produce.

Satisfies the AC-28 fail-before obligation for the Efc surface.
