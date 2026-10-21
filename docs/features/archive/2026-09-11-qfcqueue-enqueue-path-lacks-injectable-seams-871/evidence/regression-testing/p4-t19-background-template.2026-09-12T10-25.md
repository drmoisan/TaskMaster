# P4-T19 — background-template reference flows through to the queue entry

Timestamp: 2026-09-13T16-35

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t19

EXIT_CODE: 0

Counters element of TestResults\p4-t19\vstest-class-run.trx, recorded in full because P4-T24's
arithmetic identity reads the `total` from this artifact:

- total: 28
- executed: 28
- passed: 28
- failed: 0

Output Summary:
- New case: `EnqueueAsync_WithSubstitutedTemplate_FlowsThatPanelToTheDequeuedEntry`. It substitutes
  a second panel, distinguishable by reference from the harness sentinel, and asserts that the
  panel the dequeued entry carries is that same reference, which proves the value flows through
  unmodified rather than being rebuilt anywhere on the path.
- This is the last task of the phase that adds a case. P4-T20 and P4-T21 may move and reformat
  cases but add none, so the figure above is the class-scoped count of every case the new suite
  contributes.
