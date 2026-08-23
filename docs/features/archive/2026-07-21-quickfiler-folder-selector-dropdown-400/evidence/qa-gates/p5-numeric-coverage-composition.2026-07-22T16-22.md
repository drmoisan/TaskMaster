# P5-T183 — Authoritative post-correction instrumented 17-class composition

Timestamp: 2026-07-22T16-22Z

Command: `$suffix='2026-07-22T16-22'; $evidence=(Resolve-Path 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates').Path; $coverageOutput=Join-Path $evidence "coverage-p5-numeric-correction.$suffix.cobertura.xml"; $coverageConfig=(Resolve-Path 'coverage.config').Path; $cliRunSettings=(Resolve-Path 'scripts\vscode\TaskMaster.cli.runsettings').Path; $quickFilerTestAssembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $p5Filter='FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests'; $preHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; $coverageArgs = @('collect','--output',$coverageOutput,'--output-format','cobertura','--settings',$coverageConfig,'--',$vstestPath,$quickFilerTestAssembly,"/Settings:$cliRunSettings",'/InIsolation',"/TestCaseFilter:$p5Filter"); & dotnet-coverage @coverageArgs; $code=$LASTEXITCODE; $postHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; exit $code`

EXIT_CODE: 0

## Run result

- Natural completion, `Test Run Successful.`, process exit code `0`.
- **Total tests: 160, Passed: 160, Failed: 0, Skipped: 0.** Total time 4.2324 s.
- `dotnet-coverage` version `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`.
- Inputs: only `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, unchanged repo-root `coverage.config`, and
  `scripts\vscode\TaskMaster.cli.runsettings`. The filter is byte-identical to the P5-T171 filter.
- `coverage.config` SHA-256 pre-run `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`, post-run
  `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` — unchanged.

## Cobertura artifact

| Item | Value |
|---|---|
| Path | `evidence/qa-gates/coverage-p5-numeric-correction.2026-07-22T16-22.cobertura.xml` |
| Root element | `coverage` (complete, parseable as XML) |
| Bytes | 17,330,039 |
| SHA-256 | `AC4E344AF35F929DD5B1FBE177A492FE13E5CBC9A639C747F3A09CA4384491C1` |

## Per-class counts (17 classes, sum = 160)

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
| BreadcrumbDropDownOpenCoordinatorTests | 10 (5 primary + 5 Part2) |
| BreadcrumbPopupBoundaryCoverageTests | 18 (5 primary + 13 Part2) |
| BreadcrumbDropDownLifecycleCoverageTests | 12 |
| BreadcrumbMessengerHubCoverageTests | 10 |
| BreadcrumbDropDownIntegrationTests | 10 |
| **Total** | **160** |

These are identical to the P5-T171 per-class inventory; the 17-class inventory and the 160-case total are preserved.

## First-party entries for every measurable P5 type

Deduplicated per-`<line>` measurement across all Cobertura packages (primary types only; generated state machines and
closures are enumerated in the P5-T185 decision):

| Type | Covered | Valid | Line rate |
|---|---:|---:|---:|
| `QuickFiler.Viewers.BreadcrumbUiDispatcher` | 144 | 144 | 100.00% |
| `QuickFiler.Viewers.BreadcrumbNavigationReadiness` | 96 | 96 | 100.00% |
| `QuickFiler.Viewers.BreadcrumbWebViewSurfaceFactory` | 16 | 16 | 100.00% |
| `QuickFiler.Viewers.BreadcrumbPopupUiOperations` | 75 | 76 | 98.68% |
| `QuickFiler.Viewers.BreadcrumbDropDownOpenLifetime` | 121 | 123 | 98.37% |
| `QuickFiler.Viewers.BreadcrumbDropDownHost` | 220 | 221 | 99.55% |
| `QuickFiler.Viewers.BreadcrumbMessengerHub` | 155 | 155 | 100.00% |
| `QuickFiler.Viewers.BreadcrumbMessengerHub.Attachment` | 10 | 10 | 100.00% |
| `QuickFiler.Viewers.BreadcrumbDropDownOpenCoordinator` (new coordinator) | 146 | 151 | 96.69% |

Every measurable P5 type, including the newly added `BreadcrumbDropDownOpenCoordinator`, is present as a first-party
entry in the Cobertura output.

## Relationship to superseded artifacts

The pre-correction `p5-numeric-coverage-composition.2026-07-22T14-46.md` and
`coverage-p5-numeric-correction.2026-07-22T14-44.cobertura.xml` remain non-authoritative 159/160 evidence and are not
cited as passing anywhere. This 2026-07-22T16-22 pair is the authoritative post-correction composition.

## Execution note (transparency)

Two earlier attempts at this gate on this machine stalled mid-run at 148/160 without any test failing, and a
`/Settings`-bearing uninstrumented control stalled at 150/160 in the same window. The stalls were traced to residual
`vstest.console.exe`/`testhost.exe` process state left by a prior externally terminated out-of-filter probe on the same
machine, not to any test outcome: after those processes were cleared, the uninstrumented 17-class control passed 160/160
and this instrumented gate passed 160/160 with natural exit `0`. No stalled attempt is cited as evidence, no test, filter,
configuration, or threshold was changed in response, and the class/case inventory is unchanged.

Output Summary: The authoritative post-correction instrumented 17-class `dotnet-coverage` gate reached natural
completion with `EXIT_CODE: 0` and **160/160 passed, 0 failed, 0 skipped**, restoring the case that P5-T172 diagnosed.
The Cobertura artifact is complete (root `coverage`, 17,330,039 bytes, SHA-256 `AC4E344A...384491C1`), `coverage.config`
is hash-identical pre and post run, per-class counts match the P5-T171 inventory exactly, and every measurable P5 type
including the new `BreadcrumbDropDownOpenCoordinator` has a first-party entry. EXIT_CODE: 0.
