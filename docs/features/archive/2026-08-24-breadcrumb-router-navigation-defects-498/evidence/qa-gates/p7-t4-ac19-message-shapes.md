# P7-T4 — Decision-D1 Message-Shape Criterion (AC-19)

Timestamp: 2026-08-26T11-00

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~FolderBreadcrumbAssetContractTests" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/trx/p7-t4"; "EXIT_CODE: $LASTEXITCODE"'`

Second command: `git status --porcelain --untracked-files=all -- QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`

EXIT_CODE: 0

## Output Summary

**PASS at the primary acceptance condition. No degradation was used or available.**

### Test result

`Test Run Successful.` Counts read from the TRX `<ResultSummary><Counters>` element at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/trx/p7-t4/results.trx`:

| Metric | Value |
|---|---:|
| total | 15 |
| executed | 15 |
| passed | **15** |
| **failed** | **0** |

Total time 1.5660 seconds. The complete `FolderBreadcrumbAssetContractTests` class ran.

The test method the task names explicitly, `LeftAndRightBreadcrumbMessages_RemainSupported`
(`QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs:359-367`), is present in the run output
and **Passed**. The `FolderBreadcrumb.html` change made by `P6-T17` therefore did not alter or remove the
Left and Right breadcrumb message shapes the decision-D1 contract requires.

### Read-only confirmation

`git status --porcelain --untracked-files=all -- QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`
produced **no output**, so the MUST-NOT-WRITE contract test file is unmodified in the working tree. It is
also absent from the cumulative change set recorded by `P7-T3`.

### Degradation status

The `P0-T15` `BASELINE_FAILURE_SET` is EMPTY. The conditional degradation in this task is gated on that
set containing an identifier in this test class, so the degradation branch is **unavailable** and the
gate stands at its primary condition `failed 0`, which was met absolutely.

**AC-19 disposition: SATISFIED** (jointly with `P6-T17`).
