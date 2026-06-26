# Final QC — File Sizes (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30
Command: `(Get-Content <path>).Length` (equivalent `wc -l` used in git-bash)
EXIT_CODE: 0

Output Summary (all <= 500-line repository limit):

| File | Lines | Limit | Baseline | Note |
|---|---|---|---|---|
| `TaskMaster/AppGlobals/ApplicationGlobals.cs` | 398 | 500 | 359 | OK (+39) |
| `TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs` | 171 | 500 | 97 | OK (+74) |
| `TaskMaster.Test/AppGlobals/StartupDiagnosticsProbeTests.cs` | 257 | 500 | 151 | OK (+106) |
| `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` | 500 | 500 | 500 | OK (== baseline; NOT increased, per the at-limit constraint) |
| `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` | 317 | 500 | 316 | OK (+1, seam-rename reflow) |
| `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` | 123 | 500 | 122 | OK (+1, seam-rename reflow) |

All touched files are <= 500 lines. `ApplicationGlobalsTests.cs` remained at exactly its 500-line
baseline (the seam-override edits were balanced by condensing the seam comment block, so the file
did not exceed the at-limit baseline).
