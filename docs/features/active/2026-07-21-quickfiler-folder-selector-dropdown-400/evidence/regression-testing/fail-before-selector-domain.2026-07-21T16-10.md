# Fail-before selector domain

Timestamp: 2026-07-21T16-10Z

Build Command: `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU'`

Planned Build EXIT_CODE: 1

Planned Build Output Summary: The legacy project graph defines `Debug|AnyCPU`, not `Debug|Any CPU`. The literal plan command stopped before compilation with `The BaseOutputPath/OutputPath property is not set` in the test project and referenced projects. This is a plan command-token defect, not a source/test failure.

Repository-Compatible Build Command: `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Filtered Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbSelectionSessionTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~BreadcrumbStateModelSelectorTests"`

Filtered Test EXIT_CODE: 1

- Total: 23
- Passed: 0
- Failed: 23
- Skipped: 0

Intended fail-before signatures included:

- `ClosedNavigation_CommitsSelectableRows_SkipsLabelsAndStopsAtBoundaries`: `issue #400 requires stable identity and selectable-row metadata`.
- `OpenNavigation_ChangesPendingWithoutChangingCommittedOrModelSelection`: same committed/original/pending prerequisite.
- `AddScoredFallbackRow_RetainsIdentityTextAndSuppliedProbability`: `issue #400 requires scored fallback rows before hierarchy resolution`.
- `ViewMessage_RoundTripsModeOpenAndStableIdentities`: `issue #400 requires BreadcrumbSelectorViewMessage`.
- `SelectorKeyMessage_RoundTripsOnlySupportedKeys`: `issue #400 requires BreadcrumbSelectorKey`.
- `ActivationMessage_RoundTripsStableIdentity`: `issue #400 requires BreadcrumbSelectorActivationMessage`.

Output Summary: The repository-valid direct build compiled and discovered all 23 new tests. Every test failed for an intended missing selector-domain contract; there was no compilation, discovery, VSTest resolution, UI, or display failure in the valid build/test path. The literal plan platform spelling must be normalized to `AnyCPU` for direct legacy-project builds.

Compatibility Correction: The root orchestrator authorized `Platform=AnyCPU` as the mechanical equivalent for direct legacy-project build tasks. The temporary single-project alias experiment was removed; no configuration alias or non-Compile project edit remains.
