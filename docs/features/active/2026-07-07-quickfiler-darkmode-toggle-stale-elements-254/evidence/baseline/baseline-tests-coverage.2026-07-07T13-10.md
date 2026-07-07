# Baseline — Tests + Coverage (Issue #254)

Timestamp: 2026-07-07T13-10

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`

Note on coverage extraction: The `.coverage` binary emitted by the vstest Code Coverage collector is not offline-convertible to a numeric report in this environment (`dotnet-coverage merge` of the `.coverage` yields an empty `<packages/>` Cobertura). Numeric coverage was therefore obtained via `dotnet-coverage collect --output-format cobertura -- vstest.console.exe <same two DLLs> /InIsolation`, which instruments the test host directly and emits a populated Cobertura report. This is the repository's established reliable numeric-coverage path for this toolchain.

EXIT_CODE: 1 (non-deterministic; see test-flakiness note)

## Output Summary

Test execution: 4658 total tests.
- Literal `/EnableCodeCoverage` run: 4657 passed / 1 failed (first run), 4656 passed / 2 failed (repeat run). Failing tests differ between runs.
- Named flaky failures observed: `TryAddValuesAsync_UpdatesExistingValue`, `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`, each reported at ~22s. Both PASS in isolation at ~65ms (verified: `vstest.console.exe UtilitiesCS.Test.dll /Tests:TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream,TryAddValuesAsync_UpdatesExistingValue` -> Passed 2/2, exit 0).
- Root cause of flakiness: UtilitiesCS.Test runs with MSTest class-level parallelization at 24 workers; a small set of timing-sensitive tests (OneDrive download, file-stream writer, dictionary async) time out under parallel contention plus coverage instrumentation. Pre-existing, environmental, and unrelated to Theme / QuickFiler dark-mode code or to issue #254.
- The `dotnet-coverage collect` instrumented run amplified the contention to 20 flaky timing failures (heavier instrumentation overhead); none in Theme or QuickFiler dark-mode tests.

## Numeric Baseline Coverage (Cobertura via dotnet-coverage collect)

- Overall line-rate: 64.28% (lines-covered 110106 / lines-valid 171299); branch-rate 33.13%.
- UtilitiesCS module (production assembly, non-exempt): 87.89% line coverage (70284 / 79967).
- Theme class (partial `Theme.cs`): 62.71% (296 / 472 lines).
- Theme class (partial `Theme.Rendering.cs` — the file to be changed): 44.78% (60 / 134 lines).

The changed target is the mail read/unread branch inside the private `SetQfcTheme()` in `Theme.Rendering.cs`; baseline coverage of that private render body is low because existing tests exercise dispatch routing, not the full synchronous render.
