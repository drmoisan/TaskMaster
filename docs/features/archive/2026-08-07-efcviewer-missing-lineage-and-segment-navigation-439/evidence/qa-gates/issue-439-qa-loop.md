# Issue #439 Final C# QA Loop

## P4-T2 — Formatting

Timestamp: 2026-08-24T19:35:00-04:00

```powershell
dotnet tool run csharpier format .
```

EXIT_CODE: `0`

Output Summary: CSharpier 1.2.6 completed. The repeated final pass left the tracked diff and the newly added headless router-test file unchanged; see `csharpier-final.md`. The P0 legacy command and its failed compatibility diagnostic remain preserved in `evidence/baseline/csharpier-baseline.md`.

## P4-T3 — Analyzers

Timestamp: 2026-08-24T19:36:10-04:00

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: `0`

Output Summary: Analyzer diagnostic count `0`, equal to the P0-T3 numeric baseline; zero new findings. The build has five established System.Reactive `packages.config` support warnings and zero errors.

## P4-T4 — Compiler and nullable analysis

Timestamp: 2026-08-24T19:37:10-04:00

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

EXIT_CODE: `0`

Output Summary: Compiler diagnostics `0` and nullable diagnostics `0`, each equal to the P0-T4 successful-retry baseline; zero new findings. The build has five established System.Reactive `packages.config` support warnings and zero errors.

## Restart before P4-T2

Timestamp: 2026-08-24T19:46:00-04:00

Reason: The archive-root `BindRowsAsync` overload was narrowed from public to internal to preserve the no-public-API-change boundary. Earlier P4-T2 through P4-T4 results and the pre-remediation coverage XML are superseded for final QA.

## P4-T2 restart — Formatting

```powershell
dotnet tool run csharpier format .
```

EXIT_CODE: `0`

Output Summary: CSharpier 1.2.6 completed idempotently after the accessibility remediation; see `csharpier-final.md`.

## P4-T3 restart — Analyzers

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: `0`

Output Summary: Analyzer diagnostic count `0`, equal to the P0-T3 numeric baseline; zero new findings and five established System.Reactive `packages.config` warnings.

## P4-T4 restart — Compiler and nullable analysis

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

EXIT_CODE: `0`

Output Summary: Compiler diagnostics `0` and nullable diagnostics `0`, each equal to baseline `0`; zero new findings and five established System.Reactive `packages.config` warnings.

## P4-T5 — Coverage and tests

```powershell
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.cobertura.xml
```

EXIT_CODE: `0`

Output Summary: Final post-remediation Cobertura XML generated. Repository coverage is `70.14%`; target-file numeric and structural-exception details are in `csharp-coverage-final.md`. The prior pre-remediation XML record is not used for final QA.

## P4-T6 — Coverage comparison

```powershell
# Read-only comparison of P0-T5 and P4-T5 Cobertura XML plus the baseline-commit diff.
```

EXIT_CODE: `1`

Output Summary: `REMEDIATION_REQUIRED`. Repository coverage `85.58% -> 70.14%`, Router `97.87% -> 95.74%`, Row `98.02% -> 97.42%`, and changed/new instrumentable coverage `59.19%`; see `issue-439-coverage-comparison.md`. Final QA is not clean.

## P4-T7 — Final QA loop status

Status: `REMEDIATION_REQUIRED`

The restarted format → analyzers → nullable → coverage-test sequence completed with CSharpier idempotence, analyzer diagnostic count `0`, compiler/nullable diagnostic counts `0`, and a generated final Cobertura XML. The coverage comparison exits `1` for the documented repository, per-file, and changed/new-line threshold failures. The loop is therefore fail-closed and does not establish final C# QA approval.

## Restart after P3-T7/P3-T8

Timestamp: 2026-08-24T20:20:30-04:00
Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Output Summary: The formatter changed newly added Issue #439 test formatting. P4-T1 through P4-T7 must be repeated; prior P4 results remain historical evidence only.

## P4-T2 restarted formatting

Timestamp: 2026-08-24T20:21:40-04:00
Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Output Summary: CSharpier 1.2.6 completed with no change. The full tracked diff hash remained `f402884b41dbaecfec2408195fd0a3588f01666a`; see `csharpier-final.md`.

## P4-T3 restarted analyzers

Timestamp: 2026-08-24T20:22:10-04:00
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:minimal`
EXIT_CODE: 0
Output Summary: Analyzer diagnostic count `0`, equal to the P0-T3 successful baseline retry count; zero new findings and five established System.Reactive `packages.config` warnings.

## P4-T4 restarted nullable analysis

Timestamp: 2026-08-24T20:22:50-04:00
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:minimal`
EXIT_CODE: 0
Output Summary: Compiler and nullable diagnostic counts are both `0`, equal to the P0-T4 successful baseline retry counts; zero new findings and five established System.Reactive `packages.config` warnings.

## Restart after P4-T5 full-suite test failure

Timestamp: 2026-08-24T20:29:09-04:00
Command: `vstest.console.exe <all nine test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"TestCategory!=LiveOutlook" /InIsolation`
EXIT_CODE: 1
Output Summary: 6472 passed and 2 failed: `Bind_WhenProviderCanceled_PropagatesCancellation` and `LeafExpand_WhenKeyUnresolvedAtExpandTime_LeavesStateUnchanged`. The stale expectations contradicted the checked P2-T3 cancellation-fallback and P3-T3 binding-captured-key contracts. Both tests were renamed to state those contracts, corrected using the same headless Moq/router boundary, cleanly built, and passed 2/2. P4-T1 through P4-T7 restarted as required.

## P4-T1 restarted regression after correction

Timestamp: 2026-08-24T20:29:09-04:00
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbRowBuilderTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation`
EXIT_CODE: 0
Output Summary: 83 passed, 0 failed. All selected tests remain headless as described in the P4-T1 regression record.

## P4-T1 restarted regression with queue-test coverage

Timestamp: 2026-08-24T20:31:00-04:00
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbRowBuilderTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation`
EXIT_CODE: 0
Output Summary: 97 passed, 0 failed. The corrected queue-test contracts are included in this P4-T1 restart and stay within the headless router/Moq boundary.

## P4-T2 restarted formatting after queue-test correction

Timestamp: 2026-08-24T20:31:37-04:00
Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Output Summary: CSharpier 1.2.6 formatted 1520 files without changing tracked edits or either modified/new Issue #439 test source; see `csharpier-final.md` for hashes.

## P4-T3 restarted analyzers after queue-test correction

Timestamp: 2026-08-24T20:32:16-04:00
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:minimal`
EXIT_CODE: 0
Output Summary: Analyzer diagnostics: 0, equal to baseline; zero new findings, five established System.Reactive `packages.config` warnings, and zero errors.

## P4-T4 restarted nullable analysis after queue-test correction

Timestamp: 2026-08-24T20:32:59-04:00
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:minimal`
EXIT_CODE: 0
Output Summary: Compiler diagnostics: 0; nullable diagnostics: 0; both equal to baseline. The build had zero errors and five established System.Reactive `packages.config` warnings.

## P4-T5 authoritative normalized coverage restart

Timestamp: 2026-08-24T20:35:41-04:00
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.normalized.cobertura.xml`
EXIT_CODE: 0
Output Summary: The one retained parent wrapper session exited after 6474/6474 tests passed and emitted both required post-processing completion strings. Normalized coverage is 53757/63405 = 84.7835%; all XML invariants pass. See `csharp-coverage-final.md`.

## P4-T6 authoritative normalized comparison restart

Timestamp: 2026-08-24T20:37:03-04:00
Command: read-only normalized Cobertura comparison and `git diff --unified=0 c83468e2a15560233e20735b0d9a049823fc7613 -- '*.cs'` changed-line calculation.
EXIT_CODE: 0
Output Summary: normalized inputs passed all invariants; final repository coverage is 84.7835%, all six required files are non-regressed, final EfcFormController coverage is numeric (81/721), DocumentAssets is the sole structural exception, and changed/new production coverage is 200/203 = 98.522167%. See `issue-439-coverage-comparison.md`.

## P4-T7 final QA status

Timestamp: 2026-08-24T20:37:33-04:00
Command: final evidence reconciliation of the restarted P4-T1 through P4-T6 commands.
EXIT_CODE: 0
Output Summary: PASS. The clean restarted headless sequence is: 97 focused regression tests passed; CSharpier 1.2.6 made no source changes; analyzer diagnostics were 0; compiler and nullable diagnostics were 0; the one authoritative coverage-wrapper parent session exited 0 after 6474/6474 tests and emitted both completion markers; normalized XML invariants passed; repository coverage was 84.7835%; all six required comparison files were non-regressed; EfcFormController was numeric at 81/721; DocumentAssets remained the sole structural exception; and changed/new production coverage was 200/203 = 98.522167%.

The raw 70.14% XML and failed comparison are retained solely as historical non-comparable evidence. This P4 result is final QA approval for the current working tree.
