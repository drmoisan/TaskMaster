# P4-T4 — six seam-contract tests

Timestamp: 2026-09-13T16-01

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression over the
  whole summary line rather than by a substring search.
- `2 Warning(s)`, both `CS0649` against the harness part, for the two fields the later tasks assign
  (`_itemGroupFailure`, assigned by P4-T10; `_itemGroupObserver`, assigned by P4-T9). They are
  compiler warnings rather than code-style diagnostics, so they would reach the nullable gate; both
  are expected to clear once those two tasks assign the fields, and P4-T23 is the gate that proves
  it.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t4

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t4\vstest-class-run.trx: `total=6 executed=6 passed=6 failed=0`
- `failed=0` and `passed` is 6, which is at least the six this task requires.
- The six cases are one per seam: `MoveMonitor`, `UiIdleDispatcher`, `ItemViewerFactory`,
  `ViewerRowPlacer`, `ItemGroupFactory` and `BackgroundTlpFactory`. Each body is a single call to
  the harness helper with that seam's getter and setter; the helper asserts the getter returns a
  non-null production default and that the setter throws `ArgumentNullException` on null.
- This run is also the detector P4-T3 could not be: the class-scoped filter matched cases, which
  proves the test-class part is in the test-project manifest, and the compile succeeded, which
  proves the harness part is.
