# MSBuild Nullable / TreatWarningsAsErrors Baseline (Issue #232)

Timestamp: 2026-07-03T11-31

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(invoked from git-bash as `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -m -clp:Summary`.)

EXIT_CODE: 0

Output Summary: `Build succeeded. 0 Warning(s) 0 Error(s)`. Time Elapsed 00:00:00.92. This is an
incremental (up-to-date, no-recompile) pass: the immediately-preceding analyzer build already
produced current Debug|Any CPU outputs, so no project was recompiled under the nullable property
set. At final QA (Phase 5), the touched project source files will be newer than their outputs, so
they will be recompiled and validated under this exact property set (the executor forces a genuine
recompile of the changed files at P5-T3 to make the gate meaningful). Baseline capture records the
command result as-is per the plan.
