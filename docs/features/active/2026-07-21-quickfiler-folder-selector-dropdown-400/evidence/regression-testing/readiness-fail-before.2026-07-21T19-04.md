# Popup Readiness Fail-Before

Timestamp: 2026-07-21T19-04Z
Command: $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess,OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce
EXIT_CODE: 1
Output Summary: Both exact readiness regressions were discovered and failed for the intended missing readiness-aware popup surface contract. The test assembly built and loaded successfully; the failure was not a compile, discovery, tool-resolution, UI, or environmental error.

## Results

- Total tests: 2
- Passed: 0
- Failed: 2
- Skipped: 0
- Test time: 0.9664 seconds

Failing tests:

1. `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`
2. `OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce`

Pre-fix diagnostic for both tests:

```text
Expected constructor not to be <null> because the popup host requires a readiness-aware surface contract before it can defer messenger exposure, cached replay, show, and focus.
```

This is the required assertion failure for missing document-readiness handling. It establishes the pre-production failing state before any modification to `BreadcrumbDropDownHost.cs`.
