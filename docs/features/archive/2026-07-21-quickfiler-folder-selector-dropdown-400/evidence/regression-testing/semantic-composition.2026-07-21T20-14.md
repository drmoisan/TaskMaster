# Semantic Composition Regressions

Timestamp: 2026-07-21T20-14Z
Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce,InboundSelectorKeyUp_MovesPendingPastSeparatorAndClampsWithoutDuplicatePublication`
EXIT_CODE: 0
Output Summary: Both exact semantic composition regressions passed. Native automatic close restored the opening selection without publication and returned focus once. Inbound Up moved pending selection across a separator, retained the committed identity, clamped at the first selectable row, and emitted one selector-view transition without duplication.

## Results

- Resolved vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- Total tests: 2
- Passed: 2
- Failed: 0
- Skipped: 0
- Test time: 1.8940 seconds

## Assertion Outcomes

`NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce`:

- Opening and Down navigation succeeded with committed identity `A` and pending identity `B`.
- Native close ended the selector session.
- Committed identity and viewer selection remained `A`.
- Pending identity was cleared.
- `SelectionChanged` publication count remained zero.
- Focus-return count was exactly one.

`InboundSelectorKeyUp_MovesPendingPastSeparatorAndClampsWithoutDuplicatePublication`:

- The populated model was `A`, separator, `B` with committed identity `B`.
- First inbound Up skipped the separator and moved only pending identity to `A`.
- Committed identity and selected folder remained `B`.
- A second inbound Up clamped at `A` without another state transition.
- Exactly one additional `selectorView` message was posted across both Up inputs.
- Committed `SelectionChanged` publication count remained zero.
