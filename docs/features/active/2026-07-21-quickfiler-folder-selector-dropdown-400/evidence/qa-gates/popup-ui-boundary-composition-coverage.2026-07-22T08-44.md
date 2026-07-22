# P5 filtered popup UI-boundary coverage failure

Timestamp: `2026-07-22T08:44:00+00:00`

Command: `$suffix=[DateTimeOffset]::UtcNow.ToString('yyyy-MM-ddTHH-mm'); $evidence=(Resolve-Path 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates').Path; $coverageOutput=Join-Path $evidence "coverage-popup-ui-boundary-composition.$suffix.cobertura.xml"; $coverageConfig=(Resolve-Path 'coverage.config').Path; $cliRunSettings=(Resolve-Path 'scripts\vscode\TaskMaster.cli.runsettings').Path; $quickFilerTestAssembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $p5Filter='FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests'; $coverageArgs=@('collect','--output',$coverageOutput,'--output-format','cobertura','--settings',$coverageConfig,'--',$vstestPath,$quickFilerTestAssembly,"/Settings:$cliRunSettings",'/InIsolation',"/TestCaseFilter:$p5Filter"); & dotnet-coverage @coverageArgs`

EXIT_CODE: `1`

Output Summary: `FAIL. The filtered coverage process completed naturally and discovered exactly 70 cases, but 69 passed and one failed; zero were skipped. BreadcrumbSelectorToggleUiBoundaryTests.WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary observed context.PostCount == 1 where the retained assertion requires a value greater than one. The generated XML is complete but non-authoritative because the test command failed. This failure is outside the authorized P5-T80 two-file correction scope and requires atomic replanning.`

## Execution facts

- `dotnet-coverage` version: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`.
- VSTest version: `18.8.0` x64.
- Test assembly: only `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- Filter: the exact nine-class P5-T86 filter.
- Discovered: `70`.
- Passed: `69`.
- Failed: `1`.
- Skipped: `0`.
- Total test time: `3.2171 seconds`.

## Exact failure

- Test: `BreadcrumbSelectorToggleUiBoundaryTests.WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`.
- Assertion: `context.PostCount` must be greater than `1`.
- Observed: `1`.
- Source: `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:81`.
- The same case passed in P5-T86 without coverage instrumentation.
- `BreadcrumbSelectorToggleUiBoundaryTests.cs` is outside the P5-T80 production/test correction tuple, so no correction is authorized in this execution batch.

## Artifact integrity and classification

- Cobertura path: `coverage-popup-ui-boundary-composition.2026-07-22T08-44.cobertura.xml`.
- Cobertura SHA-256: `7D19A7AFB1BA278EA1BD8A80AE20BABB603220BD8FEB8CB7548EF11DA0495AAB`.
- Bytes: `16,976,584`.
- XML root: `coverage`.
- XML headline: `5,724` covered lines out of `84,039` valid lines.
- Pre-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- Post-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- Classification: non-authoritative failed-test diagnostic. It must not be used for P5-T88 numeric decisions.

P5-T87, P5-T88, P5-T89, P5-T67, P5-T68, and superseded P5-T73 through P5-T78 remain unchecked. The plan's out-of-scope failure rule requires an atomic-planner revision before another coverage attempt.
