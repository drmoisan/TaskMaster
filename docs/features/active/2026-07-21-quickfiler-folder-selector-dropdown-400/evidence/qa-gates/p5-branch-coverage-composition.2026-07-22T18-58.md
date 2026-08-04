# P5-T201 — Post-correction instrumented 17-class composition (170/170)

Timestamp: 2026-07-22T18-58Z

Command: `$suffix='2026-07-22T18-58'; $evidence=(Resolve-Path 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates').Path; $coverageOutput=Join-Path $evidence "coverage-p5-branch-coverage-correction.$suffix.cobertura.xml"; $coverageConfig=(Resolve-Path 'coverage.config').Path; $cliRunSettings=(Resolve-Path 'scripts\vscode\TaskMaster.cli.runsettings').Path; $quickFilerTestAssembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $p5Filter='<the P5-T171/P5-T183 17-class filter, byte-identical>'; $preHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; $coverageArgs = @('collect','--output',$coverageOutput,'--output-format','cobertura','--settings',$coverageConfig,'--',$vstestPath,$quickFilerTestAssembly,"/Settings:$cliRunSettings",'/InIsolation',"/TestCaseFilter:$p5Filter"); & dotnet-coverage @coverageArgs; $code=$LASTEXITCODE; $postHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; exit $code`

EXIT_CODE: 0

## Result

- **Total tests: 170, Passed: 170, Failed: 0, Skipped: 0.** Natural exit code 0.
- Cobertura artifact: `coverage-p5-branch-coverage-correction.2026-07-22T18-58.cobertura.xml` (complete document, ~17 MB).
- `coverage.config` hash identical before and after the run:
  `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` (PRE == POST).
- Filter string byte-identical to the P5-T183 17-class filter; not narrowed and not extended.
- First-party entries present for every measurable P5 type, including `BreadcrumbDropDownOpenCoordinator`,
  `BreadcrumbDropDownOpenLifetime`, and `BreadcrumbDropDownHost`.
- Current test-file state at the time of the run is byte-identical to the gated state
  (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` = `6ec48542…`, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`
  = `594d96f2…`).

## Per-class composition (17 classes, sum = 170)

| Class | Cases |
|---|---:|
| BreadcrumbUiThreadDispatchTests | 9 |
| BreadcrumbSelectorToggleUiBoundaryTests | 4 |
| BreadcrumbPopupControlDispatchTests | 13 |
| BreadcrumbSelectorOpenRetryTests | 8 |
| BreadcrumbDropDownReadinessTests | 12 |
| BreadcrumbCollapsedSurfaceReadinessTests | 10 |
| BreadcrumbDropDownCoverageThresholdTests | 7 |
| BreadcrumbDuplicateIdentityIntegrationTests | 4 |
| BreadcrumbBridgeCoordinatorProbabilityTests | 3 |
| BreadcrumbDropDownHostTests | 13 |
| BreadcrumbMessengerHubTests | 12 |
| ItemViewerBreadcrumbDropDownContractTests | 5 |
| **BreadcrumbDropDownOpenCoordinatorTests** | **15** (was 10; +5 batch N1) |
| **BreadcrumbPopupBoundaryCoverageTests** | **23** (was 18; +5 batch N2) |
| BreadcrumbDropDownLifecycleCoverageTests | 12 |
| BreadcrumbMessengerHubCoverageTests | 10 |
| BreadcrumbDropDownIntegrationTests | 10 |
| **Total** | **170** |

This equals the P5-T186 required composition `70+13+12+5+15+23+12+10+10` = 170 under the same grouping the
P5-T183 doc used, with the two edited classes moved from 10→15 and 18→23. The superseded 160 total is superseded
solely by the ten cases added in P5-T188 and P5-T195. The pre-correction `2026-07-22T16-22` Cobertura and
`2026-07-22T16-29` decision remain non-authoritative below-threshold evidence and are not cited as passing.

## Environmental stall disclosure (per operational note)

This gate exhibited the same documented environmental stall recorded in the 16-22 composition
("earlier attempts stalled mid-run ... without any test failing ... after clearing processes ... passed with natural
exit 0"). Under the runsettings' `Workers=0` (=24) ClassLevel parallelism plus dotnet-coverage instrumentation, a
`BreadcrumbPopupControlDispatchTests` worker's testhost intermittently deadlocked during concurrent WinForms
host-handle creation, leaving vstest waiting with zero failures. In addition, a first attempt was terminated by an
external 10-minute harness timeout mid-merge, which then had to be cleared. On each stall the residual
`dotnet-coverage`/`testhost`/`vstest.console` processes were killed, a clean process table (0 runners, no respawn)
was confirmed, and the exact command was re-run unchanged. No stalled attempt is cited as a result: only this
natural-exit-0, 170/170 run is authoritative. No test, filter, threshold, `coverage.config`, or runsettings value
was changed to obtain the pass.

## Output Summary

The exact P5-T171/P5-T183 17-class filter ran under the direct `dotnet-coverage collect ... cobertura ... --settings
coverage.config -- vstest.console.exe QuickFiler.Test.dll /Settings:TaskMaster.cli.runsettings /InIsolation
/TestCaseFilter:$p5Filter` shape with natural exit 0, exactly 17 classes and exactly 170 cases
(`70+13+12+5+15+23+12+10+10`), 170 passed, 0 failed, 0 skipped. The Cobertura document is complete, `coverage.config`
is hash-identical pre/post, and all measurable P5 first-party types are present. The stale 160 total is superseded.
