# P5-T209 — Post-dead-code-removal instrumented 17-class composition (170/170)

Timestamp: 2026-07-22T19-32Z

Command: `$suffix='2026-07-22T19-32'; $evidence=(Resolve-Path 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates').Path; $coverageOutput=Join-Path $evidence "coverage-p5-deadcode-removal.$suffix.cobertura.xml"; $coverageConfig=(Resolve-Path 'coverage.config').Path; $cliRunSettings=(Resolve-Path 'scripts\vscode\TaskMaster.cli.runsettings').Path; $quickFilerTestAssembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $p5Filter='<the P5-T171/P5-T183/P5-T201 17-class filter, byte-identical>'; $coverageArgs = @('collect','--output',$coverageOutput,'--output-format','cobertura','--settings',$coverageConfig,'--',$vstestPath,$quickFilerTestAssembly,"/Settings:$cliRunSettings",'/InIsolation',"/TestCaseFilter:$p5Filter"); & dotnet-coverage @coverageArgs; $code=$LASTEXITCODE`

EXIT_CODE: 0

## Result

- **Total tests: 170, Passed: 170, Failed: 0, Skipped: 0.** Natural exit code 0. Total run time 5.23s
  (no stall on this attempt; no residual runners before or after — 0 processes).
- The case total is unchanged from P5-T201 (170) because the P5-T203 removal is production-only and adds
  or removes no case.
- Cobertura artifact: `coverage-p5-deadcode-removal.2026-07-22T19-32.cobertura.xml` (complete document,
  ~17.4 MB, well-formed, closing `</coverage>`).
- `coverage.config` hash identical to the P5-T202 baseline:
  `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` (dotnet-coverage does not modify
  `coverage.config`; verified by direct SHA-256 after the run — the in-script `Get-FileHash` capture was
  unavailable in the launcher shell, so the hash was recomputed with `sha256sum`).
- Filter string byte-identical to the P5-T171/P5-T183/P5-T201 17-class filter; not narrowed and not
  extended.
- First-party entries present for every measurable P5 type, including `BreadcrumbDropDownOpenCoordinator`,
  `BreadcrumbDropDownOpenLifetime`, and `BreadcrumbDropDownHost`.

## Key numeric closure — `<CompleteOpenAsync>d__16` now 24/24 = 100%

- In this Cobertura, `QuickFiler.Viewers.BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` reports
  `line-rate="1" branch-rate="1"` = **100%**, up from the P5-T201 baseline `line-rate="0.8571428571428571"`
  (= 24/28 = 85.71%).
- Former lines 153-156 (the unreachable inner recovery `catch`) no longer exist in the denominator, so
  the decision-tool covered/valid pair moves from 24/28 to **24/24 = 100%** (equivalently, the raw
  Cobertura `<line>` sequence points for the state machine are now all covered). This is the
  production-only closure of the ninth P5-T185 unit.

## Per-class composition (17 classes, sum = 170)

Identical to the P5-T201 verified composition (production-only change; no case added or removed), under
the same grouping the P5-T183 doc used:

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
| BreadcrumbDropDownOpenCoordinatorTests | 15 |
| BreadcrumbPopupBoundaryCoverageTests | 23 |
| BreadcrumbDropDownLifecycleCoverageTests | 12 |
| BreadcrumbMessengerHubCoverageTests | 10 |
| BreadcrumbDropDownIntegrationTests | 10 |
| **Total** | **170** |

This equals the required composition `70+13+12+5+15+23+12+10+10` = 170 under the same nine-bucket
grouping used by the P5-T183/P5-T201 docs.

## Authoritative supersession

This run supersedes the P5-T201 170/170 Cobertura (`coverage-p5-branch-coverage-correction.2026-07-22T18-58.cobertura.xml`)
as the authoritative composition for the numeric closure (P5-T210), decision (P5-T211), and audit
(P5-T212), because P5-T201 still recorded `<CompleteOpenAsync>d__16` at 24/28 whereas this run records
24/24 = 100%. The pre-correction `2026-07-22T16-22` Cobertura and `2026-07-22T16-29` decision remain
non-authoritative below-threshold evidence and are not cited as passing.

## Environmental stall disclosure

No stall occurred on this attempt: a single `dotnet-coverage collect` invocation completed with natural
exit 0 and 170/170 in 5.23 seconds, and the process table showed 0 residual `dotnet-coverage`/`testhost`/
`vstest.console` runners both before and after. No test, filter, threshold, `coverage.config`, or
runsettings value was changed to obtain the pass.

## Output Summary

The exact P5-T171/P5-T183/P5-T201 17-class filter ran under the direct `dotnet-coverage collect ...
cobertura ... --settings coverage.config -- vstest.console.exe QuickFiler.Test.dll
/Settings:TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:$p5Filter` shape with natural exit 0,
exactly 17 classes and exactly 170 cases (`70+13+12+5+15+23+12+10+10`), 170 passed, 0 failed, 0 skipped.
The Cobertura document is complete, `coverage.config` is hash-identical to the baseline, all measurable
P5 first-party types are present, and `<CompleteOpenAsync>d__16` now reports 100% (24/24) because the
four removed dead lines left the denominator. This run supersedes the P5-T201 composition as
authoritative for P5-T210, P5-T211, and P5-T212.
