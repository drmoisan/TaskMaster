# P6-T6 — #440 Efc Right transition, fail-before (RED)

Timestamp: 2026-08-26T08-28

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall|FullyQualifiedName~HandleArrowKey_RightAfterExpansion_DescendsByChildActivation" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t6"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 1

ExpectedExitCode: 1

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t6/results.trx`

Output Summary:

- TRX `<Counters total="2" executed="2" passed="0" failed="2" ... />`. Test Run Failed. This is the expected fail-before state for the Efc Right transition of #440.
- `HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall` — Failed. Cause: `Moq.MockException` — "Expected invocation on the mock once, but was 0 times: p => p.GetImmediateSubfoldersAsync(It.Is<FolderTreeNodeKey>(...), ...)". The pre-fix `Right` arm short-circuits on `if (row.IsCollapsed) { row.ReExpand(); }` and never reaches `ExpandLeafAsync`, because `ActivateSegment` leaves `CollapsedAfterIndex` set.
- `HandleArrowKey_RightAfterExpansion_DescendsByChildActivation` — Failed. Cause: FluentAssertions equality mismatch — `Expected _router.SelectedFolderPath to be "Inbox\Projects\Alpha\Kid" ... but "Inbox\Projects" has a length of 14`. The pre-fix `Right` arm has no descent mechanism, so a Right press on an already-expanded active segment performs no transition and the selection stays on the activated parent.
