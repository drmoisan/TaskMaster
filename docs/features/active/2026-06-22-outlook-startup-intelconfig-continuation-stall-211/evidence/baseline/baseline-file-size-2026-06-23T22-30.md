# Baseline — Production File Sizes (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30
Command: `(Get-Content <path>).Length` (equivalent `wc -l` used in git-bash)
EXIT_CODE: 0

Output Summary:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: 359 lines (<= 500 OK)
- `TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs`: 97 lines (<= 500 OK)

Both files are well under the 500-line repository limit at baseline.
