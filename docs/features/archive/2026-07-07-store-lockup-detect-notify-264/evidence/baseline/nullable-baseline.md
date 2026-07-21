# Nullable / TreatWarningsAsErrors Build Baseline (P0-T8)

Timestamp: 2026-07-08T07-58

Command:
`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(MSBuild from VS18 Community; run from repo root with MSYS_NO_PATHCONV=1.)

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 Warning(s), 0 Error(s). Time Elapsed 00:00:00.89.
- This is the incremental up-to-date no-op outcome: the immediately-preceding analyzer build
  (P0-T7) produced current outputs, so MSBuild's up-to-date check performed no recompile under
  the `/t:Build` target and reported clean. This is the documented behavior of the plan's
  `/t:Build` (not `/t:Rebuild`) command on this legacy net48 solution.
- Consequence for F4: F4's new and modified `.cs` files WILL be genuinely recompiled by the
  Phase 9 nullable gate (they change, forcing recompile of UtilitiesCS and TaskMaster), so the
  no-regression obligation is that F4 code introduces zero new nullable/TreatWarningsAsErrors
  diagnostics. Verified at P9-T3.
