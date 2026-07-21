# Fail-before coordinator surfaces

Timestamp: 2026-07-21T16-25Z

Build Command: `msbuild QuickFiler.Test/QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Filtered Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests|FullyQualifiedName~BreadcrumbMessengerHubTests"`

Filtered Test EXIT_CODE: 1

- Total: 13
- Passed: 4
- Failed: 9
- Skipped: 0

Intended fail-before signatures:

- `SetSuggestions_ImmediatelyPostsScoredFallbackBeforeProviderCompletes`: the immediate coordinator render projected the fallback as a non-suggestion, so its probability was not shown.
- `ClosedDown_CommitsNextSelectableAndRaisesOneSelection`: the coordinator did not expose `HandleSelectorKey`.
- `OpenDown_ChangesPendingOnlyThenEnterCommitsAndCloses`: the coordinator did not expose an observable selector open/close transition.
- `EscapeAndUncommittedClose_RestoreOpeningSelectionWithoutNotification` and `MouseActivation_CommitsStableIdentityExactlyOnce`: the coordinator did not expose `OpenSelector` or the associated commit/cancel behavior.
- All four `BreadcrumbMessengerHubTests`: `BreadcrumbMessengerHub` did not exist, so the two-surface broadcast, view-mode specialization, single inbound routing, and idempotent attach/detach behavior were absent.

Compatibility Correction: `Platform=AnyCPU` is the root-authorized mechanical equivalent for the invalid direct-project `Platform='Any CPU'` token. No behavioral scope changed.

Output Summary: The test project compiled, 13 tests were discovered, and nine failed only on the intended immediate-probability, selector-routing, open/close-transition, and missing-surface-hub assertions. There was no compile, discovery, tool-resolution, UI, display, or other environmental failure.
