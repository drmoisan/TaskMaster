# Baseline — MSTest Full Assembly (Remediation Cycle 2026-06-15T14-00)

Timestamp: 2026-06-15T14-00
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage /ResultsDirectory:artifacts/csharp/baseline-results
EXIT_CODE: 1

Output Summary:
- Full UtilitiesCS.Test assembly executed (not the single test in isolation), to reproduce the order-dependent failure under the assembly's real execution ordering.
- Total tests: 3815. Passed: 3814. Failed: 1.
- Failing test (confirmed): UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException, failing at IdleAsyncQueue_Tests.cs:line 219. This is the exact failure reported by PR #201 required check `Format, build, analyze, and test`. The failure is order-dependent per the verified root cause (process-global UiThread.Dispatcher set-once static contaminated by an earlier Dispatcher-initializing test in the same assembly).
- `/InIsolation` is required for this Moq-dependent assembly (TestPlatform isolation), per established repo execution behavior.
- Coverage headline (raw, all-package Cobertura root line-rate from the binary .coverage converted via dotnet-coverage): 0.5892 = 58.92%. Note: this raw root figure includes vendored/exempt packages (Swordfish, SVGControl) and is NOT the first-party testable denominator used by the >= 80% policy floor; it is recorded here only as the raw coverage signal for the assembly run. The test-only fix in this cycle changes no production lines, so production coverage cannot regress.
- Raw Cobertura XML: artifacts/csharp/baseline-coverage.cobertura.xml (written to artifacts/csharp/ per cycle directive; not into the feature evidence folder).
