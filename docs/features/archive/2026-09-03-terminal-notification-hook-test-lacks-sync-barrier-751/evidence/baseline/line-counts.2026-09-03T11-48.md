# P0-T10 — Pre-Change Line Counts (Issue #751)

Timestamp: 2026-09-03T14-25

## Commands

Command: `(Get-Content -LiteralPath 'TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceTests.cs').Count`
EXIT_CODE: 0

Command: `(Get-Content -LiteralPath 'TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceLifecycleTests.cs').Count`
EXIT_CODE: 0

## Output Summary

| File | Pre-change lines | Cap | Headroom (500 minus count) |
|---|---|---|---|
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | **492** | 500 | **8** |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | **490** | 500 | **10** |

Both integers are strictly less than 500.

These are the authoritative pre-change values for this execution. They agree with the planning-time
observation recorded in the plan (492 and 490) and with research §4.4, but the values recorded here are the
ones the P4-T7 audit compares against.

## Bearing on later tasks

- **P4-T7** compares the post-format counts against these two integers. Its acceptance requires the growth of
  `AppOlObjectsFolderTreeServiceTests.cs` to be at least 1 and at most 2 lines relative to 492 (so a
  post-change count of 493 or 494), and requires the count of
  `AppOlObjectsFolderTreeServiceLifecycleTests.cs` to remain exactly 490.
- **P1-T1** cites the headroom figures above as the measured basis for the `spec.md:299-300` condition
  ("exceeds the change budget or the remaining line headroom") that selects fail-before route 2.
