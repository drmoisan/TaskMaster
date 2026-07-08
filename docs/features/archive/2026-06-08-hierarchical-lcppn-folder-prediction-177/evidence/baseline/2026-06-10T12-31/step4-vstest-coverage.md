# Baseline Step 4 — MSTest run with coverage (UtilitiesCS.Test)

Timestamp: 2026-06-10T12-38 (UTC)
Command: `vstest.console.exe C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
EXIT_CODE: 0
Output Summary: Test Run Successful — Total tests: 3814, Passed: 3814, Failed: 0 (40.0 s).
Coverage headline (from `coverage.xml`, this folder): `UtilitiesCS.dll` line coverage 85.31% strict-covered (35047/41083), 87.49% including partially covered lines (35944/41083). This satisfies the >= 80% repository policy threshold for the production assembly in scope.

Notes:
- An initial full-assembly run had 1 intermittent failure (`UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`). The test passed 3/3 in isolated reruns and the full assembly passed 3814/3814 on the immediate rerun; the failure is pre-existing intermittent behavior on the unchanged tree, unrelated to this feature, and is recorded as an out-of-scope finding.
- `/InIsolation` is required so the test host honors the assembly's binding redirects for Moq's dependency chain.
- Binary `.coverage` converted via `Microsoft.CodeCoverage.Console.exe merge <file> -f xml -o coverage.xml`.

Canonical baseline coverage artifact: `artifacts/csharp/coverage.xml` (copy stored in this folder as `coverage.xml`; full vstest console log stored as `vstest-run.log`).
