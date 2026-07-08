# Phase 1 Test Run with Coverage (Cycle 2)

Timestamp: 2026-06-12T17:08Z

Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage
(VS18 Community. Coverage merged to artifacts/csharp/coverage.xml via dotnet-coverage merge -f xml.)

EXIT_CODE: 1 (single pre-existing out-of-scope flaky failure; see below)

Output Summary:
- Total tests: 3904; Passed: 3903; Failed: 1.
- The single failure is AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
  (IdleAsyncQueue), the documented pre-existing flake (ci-flaky-test-isolation-176),
  explicitly recorded as OUT OF SCOPE in remediation-inputs.2026-06-12T16-45.md. It passes
  in isolation: re-run with /TestCaseFilter on that test name returned 1/1 Passed. The
  failure error ("Failed loading language 'eng'") is unrelated to the test split and to
  LcppnFolderPredictor.
- All LcppnFolderPredictor tests pass: scoped run /TestCaseFilter:"...~LcppnFolderPredictor"
  returned 33/33 Passed (the 14 File A + 9 File B in-scope predictor cases plus the 10
  serialization cases matching the substring). No LcppnFolderPredictor test failed.
- LcppnFolderPredictor strict coverage (post-split): line_coverage = 97.71%
  (covered=171, partial=4, not=0); block_coverage = 97.58% (covered=242, not=6).
  Identical to baseline — every test moved intact. >= 90% threshold held, no regression.
- UtilitiesCS.dll module line_coverage = 85.46% (>= 80% floor).
- Canonical coverage XML written to artifacts/csharp/coverage.xml.

Restart note: the failure is the known out-of-scope flake (passes in isolation), not a
file-change-induced failure and not a regression from this work; per the cycle-2 inputs
it is explicitly excluded. No re-run loop is required for an out-of-scope pre-existing
flake. All in-scope tests pass and coverage is preserved.
