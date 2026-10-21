# P3-T6 — Mutation restored, test green again

Timestamp: 2026-09-01T20-03
Command: restore the guard's `catch (Exception ex)` arm, rebuild with the P0-T10 analyzer command, then re-run the identical vstest invocation from P3-T4 with `'/ResultsDirectory:coverage\testresults\p3-t6'`. The resolved test runner is recorded as `<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
EXIT_CODE: 0

## Restoration verified by source search

    Select-String -SimpleMatch '_ = ex;'                                                       →  0 matches
    Select-String -SimpleMatch 'WebViewInitializationErrorSink("WebView2 initialization failed.", ex);'  →  1 match

The mutation is fully reversed. The discard assignment introduced in P3-T5 is gone, and the sink invocation is present exactly once. Because the mutation was a single-line, exactly reversible substitution, this pair of counts is sufficient to establish that the file is back to its P1-T4 state; the guard's structure, its two catch arms, and its documentation comment were never touched by the mutation.

## Rebuild

    Build succeeded.
        5 Warning(s)
        0 Error(s)

Warning count unchanged at 5 (the pre-existing System.Reactive diagnostic). Zero coded diagnostics.

## Output Summary

      Passed InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault [219 ms]

    Test Run Successful.
    Total tests: 1
         Passed: 1

The vstest exit code is 0 and the named test is reported passed.

## The completed demonstration

| Step | Guard's `catch (Exception ex)` arm | EXIT_CODE | Result |
| --- | --- | --- | --- |
| P3-T4 | sink invoked | 0 | passed |
| P3-T5 | `_ = ex;` | 1 | failed — `but found <null>` |
| P3-T6 | sink invoked (restored) | 0 | passed |

Three runs of the same command against the same assembly path, differing only in the presence of one statement. The transition from green to red and back to green establishes that the test's result is caused by the sink invocation and by nothing else in the environment — not by run-to-run flakiness, not by a stale binary, and not by an accident of the test filter. Had P3-T6 remained red, the P3-T5 failure would have been attributable to something other than the mutation and the demonstration would have been void.

This is the substantive RED step required by the repository bugfix workflow, sequenced as research §9 and the plan's section 3 prescribe.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
