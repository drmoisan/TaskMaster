# Baseline — Full-Suite Test Run and Coverage (P0-T14)

Timestamp: 2026-08-27T20-05

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\baseline.cobertura.xml`

Launched with `Start-Process -PassThru -RedirectStandardOutput -RedirectStandardError -WindowStyle Hidden`
and `-WorkingDirectory WS`, polled at 10-second intervals on `HasExited` plus stdout-log growth, then
`WaitForExit()`.

- PID: 7016
- EXIT_CODE: 0 (taken from the process object's `ExitCode`, not inferred from log text)
- Wall time: approximately 60 seconds of test execution (the plan's ~20-minute estimate did not
  materialise on this host)
- stdout log: `FF/evidence/baseline/p0-t14-coverage-stdout.log` (497126 bytes, sanitized: the workspace
  path is rendered as the literal token `WS` and the account name as `<account>`)
- stderr log: `FF/evidence/baseline/p0-t14-coverage-stderr.log` (0 bytes — empty)

This is a baseline capture, not a gate. A non-zero exit code would be recorded, not remediated; the
observed code is 0.

## Test counts

```
Test Run Successful.
Total tests: 6701
     Passed: 6701
 Total time: 1.0002 Minutes
```

| Metric | Value |
| --- | ---: |
| Total | 6701 |
| Passed | 6701 |
| Failed | 0 |
| Skipped | 0 |

`vstest.console.exe` emits a `Failed:` line and a `Skipped:` line only when the corresponding count is
non-zero. Neither line appears anywhere in the run output, so both counts are 0.

## BASELINE_FAILURE_SET

**The baseline failure set is EXPLICITLY EMPTY.** Zero tests failed in this run, so the set of fully
qualified names of failing tests contains no members:

```
BASELINE_FAILURE_SET = { }   (explicitly empty; cardinality 0)
```

This is a valid value and is recorded here as an explicitly empty set rather than omitted. P7-T5's
acceptance requires the post-change failing-test set outside `QuickFiler.Test` to be a SUBSET of this
set; because this set is empty, that condition reduces to "no test outside `QuickFiler.Test` may fail
after the change".

## Coverage (numeric, from the Cobertura root element)

Artifact copied to `FF/evidence/baseline/baseline.cobertura.2026-08-27T20-01.xml` (10703603 bytes).
Root element read verbatim:

```xml
<coverage line-rate="0.85138" branch-rate="0.792096" complexity="25244" version="1.9"
          timestamp="1787861068" lines-covered="54387" lines-valid="63881"
          branches-covered="12927" branches-valid="16320">
```

| Metric | Raw rate | Percentage (2 dp) |
| --- | ---: | ---: |
| Repository `line-rate` | 0.85138 | **85.14%** |
| Repository `branch-rate` | 0.792096 | **79.21%** |

Supporting absolute figures: 54387 of 63881 lines covered; 12927 of 16320 branches covered.

No value in this artifact is the placeholder `UNVERIFIED`.

## Output Summary

`EXIT_CODE: 0`; 6701 total / 6701 passed / 0 failed / 0 skipped; `BASELINE_FAILURE_SET` explicitly
empty; baseline repository line-rate 85.14%, branch-rate 79.21%.
