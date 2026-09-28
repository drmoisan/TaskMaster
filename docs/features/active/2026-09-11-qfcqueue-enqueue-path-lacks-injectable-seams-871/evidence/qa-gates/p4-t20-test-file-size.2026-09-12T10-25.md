# P4-T20 — size of the two new test files against the 470 trigger

Timestamp: 2026-09-13T16-35

Command: (Get-Content -LiteralPath <P>).Count

EXIT_CODE: 0

Output Summary:
- Both files measure below the 470 trigger, so no test method and no arrangement was moved between
  the two parts at this task.

Measured physical line counts:

- QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs: 419
- QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs: 339

Remaining headroom to the 500-line ceiling:

- QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs: 81
- QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs: 161

Notes:

- The trigger is 470 rather than 500 because the repo-wide format in P5-T1 can add physical lines:
  it chain-wraps a fluent assertion past the 100-column default and inserts a blank line before a
  comment that follows a statement. Both files sit far enough below 470 that neither is at risk of
  crossing the ceiling there, and P5-T6 re-measures after the final format.
- The authoritative measurement for the file-size acceptance criterion is the one P5-T6 takes.
