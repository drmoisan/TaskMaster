# P0-T11 — Baseline Build

Timestamp: 2026-09-03T11-25
Command: MSBuild.exe TaskMaster.sln -t:Build -m -p:Configuration=Debug -p:Platform="Any CPU"
(dash-switch form used instead of the plan's literal slash-switch form; git-bash/MSYS path
conversion mangles a leading `/t:Build` into a bare path argument and MSBuild fails with
MSB1008 "Only one project can be specified" — the dash-switch spelling is functionally identical
MSBuild CLI syntax and avoids the shell's path-conversion of leading `/`. Solution path passed as
an absolute path to the item worktree's TaskMaster.sln, since the Bash tool's default working
directory is the session worktree, not the item worktree.)
EXIT_CODE: 0
Output Summary: Build succeeded. 5 Warning(s) (all identical System.Reactive 7.0.0
packages.config PackagesConfigCheck notices, one per owning project: QuickFiler, TaskMaster,
UtilitiesCS.Test, and two others among the solution's packages.config projects), 0 Error(s).
Time Elapsed 00:00:15.96.
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll exists: True (verified via Test-Path-equivalent
file check).
