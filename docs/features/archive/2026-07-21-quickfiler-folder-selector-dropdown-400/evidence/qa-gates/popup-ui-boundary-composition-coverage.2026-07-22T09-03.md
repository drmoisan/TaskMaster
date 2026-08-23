# P5 filtered popup UI-boundary composition coverage

Timestamp: `2026-07-22T09-03`

Command: `$suffix=[DateTimeOffset]::UtcNow.ToString('yyyy-MM-ddTHH-mm'); $evidence=(Resolve-Path 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates').Path; $coverageOutput=Join-Path $evidence "coverage-popup-ui-boundary-composition.$suffix.cobertura.xml"; $coverageConfig=(Resolve-Path 'coverage.config').Path; $cliRunSettings=(Resolve-Path 'scripts\vscode\TaskMaster.cli.runsettings').Path; $quickFilerTestAssembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $p5Filter='FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests'; $preHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; $coverageVersion=(& dotnet-coverage --version | Out-String).Trim(); $coverageArgs = @('collect','--output',$coverageOutput,'--output-format','cobertura','--settings',$coverageConfig,'--',$vstestPath,$quickFilerTestAssembly,"/Settings:$cliRunSettings",'/InIsolation',"/TestCaseFilter:$p5Filter"); & dotnet-coverage @coverageArgs; $code=$LASTEXITCODE; $postHash=(Get-FileHash -Algorithm SHA256 $coverageConfig).Hash; "SUFFIX=$suffix"; "DOTNET_COVERAGE_VERSION=$coverageVersion"; "COVERAGE_OUTPUT=$coverageOutput"; "PRE_HASH=$preHash"; "POST_HASH=$postHash"; if(Test-Path $coverageOutput){$xml=[xml](Get-Content -Raw $coverageOutput); "XML_ROOT=$($xml.DocumentElement.Name)"; "XML_COMPLETE=$([bool]$xml.DocumentElement)"; "XML_BYTES=$((Get-Item $coverageOutput).Length)"; "XML_SHA256=$((Get-FileHash -Algorithm SHA256 $coverageOutput).Hash)"}; exit $code`

EXIT_CODE: `0`

Output Summary: `PASS. The exact nine-class filtered coverage command completed naturally with dotnet-coverage 18.5.2, discovered 70 cases, and passed all 70 with zero failed and zero skipped. The unchanged coverage configuration produced a complete authoritative Cobertura document with first-party entries for every instrumented P5 source, including BreadcrumbMessengerHub and BreadcrumbCollapsedAttachment.`

## Execution facts

- `dotnet-coverage` version: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`.
- VSTest version: `18.8.0` x64.
- Test assembly: only `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- Selected classes: exactly `9`.
- Discovered: `70`.
- Passed: `70`.
- Failed: `0`.
- Skipped: `0`.
- Total test time: `3.2009 seconds`.
- Process result: natural completion; no timeout, termination, stall, or incomplete discovery.

## Configuration integrity

- Pre-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- Post-command `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- Runsettings: unchanged `scripts/vscode/TaskMaster.cli.runsettings`.

## Authoritative Cobertura artifact

- File: `coverage-popup-ui-boundary-composition.2026-07-22T09-03.cobertura.xml`.
- SHA-256: `63246A377D836B51A5EE2FF87C75790F62E88873A6BC9BCEAD1530C6B293DD1F`.
- Bytes: `16,977,671`.
- XML root: `coverage`.
- Structural result: complete and parseable.
- Root headline: `5,731` covered lines of `84,039` valid lines.

## First-party instrumentation entries

| P5 source | Representative first-party class entry | Present |
|---|---|---|
| `BreadcrumbUiDispatcher.cs` | `QuickFiler.Viewers.BreadcrumbUiDispatcher` | Yes |
| `BreadcrumbWebViewSurfaceFactory.cs` | `QuickFiler.Viewers.BreadcrumbWebViewSurfaceFactory` | Yes |
| `BreadcrumbPopupUiOperations.cs` | `QuickFiler.Viewers.BreadcrumbPopupUiOperations` | Yes |
| `BreadcrumbDropDownHost.cs` | `QuickFiler.Viewers.BreadcrumbDropDownHost` | Yes |
| `BreadcrumbDropDownOpenLifetime.cs` | `QuickFiler.Viewers.BreadcrumbDropDownOpenLifetime` | Yes |
| `BreadcrumbMessengerHub.cs` | `QuickFiler.Viewers.BreadcrumbMessengerHub` | Yes |
| `BreadcrumbMessengerHub.cs` | `QuickFiler.Viewers.BreadcrumbCollapsedAttachment` | Yes |
| `BreadcrumbCollapsedSurfaceController.cs` | `QuickFiler.Viewers.BreadcrumbCollapsedSurfaceController` | Yes |

`ItemViewer.Breadcrumb.cs` has no first-party entry because the enclosing ItemViewer type is not instrumented. The XML is otherwise complete; P5-T100 must classify that omitted required metric under its fail-closed threshold rule.
