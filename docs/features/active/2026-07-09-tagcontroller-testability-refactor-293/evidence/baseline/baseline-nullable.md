# Baseline — Nullable / Type-Check Build (P0-T9)

Timestamp: 2026-07-09T21-56

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(executed via VS18 MSBuild 18.7.8 with dash-switch form and MSYS_NO_PATHCONV=1 under git-bash)
EXIT_CODE: 0

Output Summary: `Build succeeded. 0 Warning(s) 0 Error(s)`. Incremental build (all projects
up-to-date from the preceding analyzer build; legacy non-SDK projects do not recompile on a
global-property-only change). Baseline nullable/TWAE gate is green. This same command form is
reused for the final QA gate (P7-T3) to establish no-regression.
