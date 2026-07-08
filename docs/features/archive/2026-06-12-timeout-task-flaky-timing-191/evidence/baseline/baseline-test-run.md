# Baseline Test Run (Affected Test, Parallel + Coverage)

Timestamp: 2026-06-13T00-33

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:RunWithTimeout_FuncT1TResult_ShouldReturnResult /InIsolation /EnableCodeCoverage

(Environment note: vstest.console.exe resolved to "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"; `/InIsolation` added because the Moq-backed test assembly requires it in this environment per recorded toolchain quirks; run under git-bash with `MSYS_NO_PATHCONV=1`.)

EXIT_CODE: 0

Output Summary:
- Test Parallelization enabled (Workers: 24, Scope: ClassLevel) — confirms class-level parallelism active at baseline.
- Passed RunWithTimeout_FuncT1TResult_ShouldReturnResult [102 ms]
- Total tests: 1; Passed: 1.
- Coverage attachment produced: TestResults\0ef323d5-3ca0-4c9b-9349-fbfbb276323a\DanMoisan_MEGALODON4_2026-06-12.20_33_13.coverage
- Coverage headline: a single-test targeted run produces a .coverage binary attachment; numeric module-coverage percent is captured at the full-suite level in Phase 2 (P2-T4). Baseline single-test execution passed under parallel + coverage on this run (the defect is intermittent/load-dependent per the fail-before dossier, not a deterministic baseline failure).
