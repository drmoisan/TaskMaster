# P5 17-class uninstrumented control run (diagnostic for the P5-T172 failure)

Timestamp: `2026-07-22T14-46`

Command: `$asm=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $p5Filter='FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests'; & $vstestPath $asm '/InIsolation' "/TestCaseFilter:$p5Filter"; $LASTEXITCODE`

EXIT_CODE: `0`

Output Summary: `PASS as a control only. Total tests: 160, Passed: 160, Failed: 0, Skipped: 0 against the current unchanged worktree. This reproduces the recorded P5-T171 result and confirms the tree has not regressed; it is a diagnostic control for the P5-T172 instrumented failure and does NOT satisfy P5-T172, which requires the dotnet-coverage collect command shape.`

## Purpose

This control isolates the differentiating condition for the P5-T172 failure recorded in `evidence/qa-gates/p5-numeric-coverage-composition.2026-07-22T14-46.md`:

- Uninstrumented, exact 17-class filter: `160/160` passed, exit `0`.
- `dotnet-coverage` instrumented, exact 17-class filter: `159/160`, exit `1`, twice consecutively.
- `dotnet-coverage` instrumented, `BreadcrumbUiThreadDispatchTests` alone: `9/9` passed, three consecutive runs.

The failure therefore requires the combination of coverage instrumentation and the full 17-class parallel composition. It is not a stall, timeout, or partial-artifact condition.

## Scope

No production, test, project, runsettings, `coverage.config`, threshold, filter, exclusion, or designer file was modified to produce this control.
