# Baseline failure set (P0-T12)

Timestamp: 2026-09-01T10-35
Task: [P0-T12]
Working directory: WORKTREE
Derived from: the P0-T10 run output (`Invoke-MSTestWithCoverage.ps1`, full suite, 2026-09-01T10-33-26
to 2026-09-01T10-34-21).

Command: `pwsh -NoProfile -File <scratchpad>/failscan.ps1 -Path <p0-t10 transcript>`
EXIT_CODE: 0

## Extraction method

The 6939-line P0-T10 transcript was scanned for lines beginning with the vstest per-test outcome tokens
`Failed` or `Error`. The match count is 0. The transcript's own summary block reports:

```
Test Run Successful.
Total tests: 6912
     Passed: 6912
```

vstest omits the `Failed:` and `Skipped:` lines when those counts are zero, and `Total tests` equals
`Passed`, so both are zero. The extracted member list and the P0-T10 counts are therefore consistent.

BASELINE_FAILURE_SET: NONE

Output Summary: The baseline run was green. No test failed, so the baseline failure set is empty and is
recorded with the single declaration `BASELINE_FAILURE_SET: NONE`. Exactly one `BASELINE_FAILURE_SET:`
declaration appears in this artifact.

Consequence for P7-T6: because the baseline failure set is empty, any test failure observed in the
post-change full-suite run is by definition not in that set and is therefore a regression introduced by
this change, which must be fixed and the Phase 7 loop restarted. There is no pre-existing failure that
P7-T6 may classify as inherited. In particular, the known intermittently-failing
`PhysicalFileInfoAdapter` test that opens the real solution file passed in this baseline run, so it is
not available as a pre-existing excuse for a later red run; should it fail later, it must be
re-evaluated on its own terms rather than waved through against this set.
