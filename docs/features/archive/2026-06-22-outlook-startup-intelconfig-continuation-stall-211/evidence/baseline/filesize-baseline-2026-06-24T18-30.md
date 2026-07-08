# File-Size Baseline (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Command: `wc -l TaskMaster/AppGlobals/AppEvents.cs TaskMaster/AppGlobals/AppOlObjects.cs` plus existence checks for the two new files.

EXIT_CODE: 0

Output Summary:
- `TaskMaster/AppGlobals/AppEvents.cs` = 499 lines (matches expected 499; at the 500-line ceiling, no headroom for markers — partial-class extraction required in P2-T1 before any marker edit).
- `TaskMaster/AppGlobals/AppOlObjects.cs` = 424 lines (matches expected 424).
- `TaskMaster/AppGlobals/StartupInboxAttributionProbe.cs` = ABSENT (does not yet exist; to be created in P1-T1).
- `TaskMaster.Test/AppGlobals/StartupInboxAttributionProbeTests.cs` = ABSENT (does not yet exist; to be created in P1-T5).
