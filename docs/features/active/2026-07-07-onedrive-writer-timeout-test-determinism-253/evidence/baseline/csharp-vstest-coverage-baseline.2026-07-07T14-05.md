# Baseline MSTest Coverage — Full `UtilitiesCS.Test` Suite (Issue #253)

Timestamp: 2026-07-07T16-36

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`

Environment note: executed via the full vstest.console.exe path (`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`), with `MSYS_NO_PATHCONV=1` to prevent git-bash path mangling, and with `/InIsolation` appended (required for this Moq-based test assembly per prior-session findings; without it, Setup Test Environment can fail with a `FileNotFoundException` for the STTE adapter). This is an environment-shell adaptation only; the effective vstest invocation and coverage-collection behavior are unchanged from the plan's specified command.

EXIT_CODE: 0

## Results

Total tests: 4170
Passed: 4170
Failed: 0
Total time: 42.3632 seconds

## Targeted test in this baseline run

A follow-up scoped run with `/TestCaseFilter:"FullyQualifiedName~OneDriveDownloader_Tests"` (same assembly, no timing change to the run) shows `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` passed in this baseline execution, duration 3 ms. Per the plan's bugfix-workflow nuance, a single run passing does not prove or disprove the underlying race; the flakiness is documented separately via the P0-T7 fail-before exception dossier.

## Coverage headline

The `.coverage` binary produced at `TestResults/<guid>/DanMoisan_MEGALODON4_2026-07-07.12_35_51.coverage` was converted to Cobertura XML via `dotnet-coverage merge -f cobertura` (the VS-bundled `CodeCoverage.exe analyze` tool is deprecated and does not offer an equivalent text-summary command in this VS 18 install) for numeric extraction:

- Repository-wide (all modules loaded by the full `UtilitiesCS.Test` run, including test-only and third-party dependency assemblies such as `Mono.Reflection`) `line-rate`: **60.23%** (`lines-covered=96579`, `lines-valid=160363`).
- `UtilitiesCS` package (production assembly under test) `line-rate`: **87.98%**.
- `UtilitiesCS.OneDriveHelpers.OneDriveDownloader` class `line-rate`: **100%** (already fully covered pre-change by the existing test suite).

This baseline coverage-headline figure (repository-wide 60.23%, `UtilitiesCS` package 87.98%) is the pre-change reference for the Phase 2 final coverage comparison (P2-T4/P2-T6). The repository-wide figure as measured here is a raw multi-module aggregate (not the first-party-only denominator described in project memory `project_coverage_firstparty_denominator_method`); it is used only for baseline-vs-final delta comparison within this plan, not as a standalone pass/fail gate against the 80%/90% policy thresholds.

## Output Summary

Full `UtilitiesCS.Test` suite: 4170/4170 passed, 0 failed, in 42.36s (EXIT_CODE 0). Baseline coverage headline: 60.23% repository-wide (raw multi-module), 87.98% for the `UtilitiesCS` package, 100% for the `OneDriveDownloader` class specifically. `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` passed in this run at 3 ms duration (a single pass does not prove non-flakiness; see P0-T7 dossier).
