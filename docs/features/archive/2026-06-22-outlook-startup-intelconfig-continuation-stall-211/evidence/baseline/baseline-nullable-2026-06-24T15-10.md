# Baseline — Nullable / TreatWarningsAsErrors Build (issue #211)

Timestamp: 2026-06-24T15-10

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(executed via git-bash with dash-switches)

EXIT_CODE: 0

Output Summary:
- Result: `Build succeeded. 0 Warning(s) 0 Error(s)`.
- Baseline nullable/TWAE build is clean for first-party projects prior to any edit.
- Note: policy gate uses `-t:Build` (not `-t:Rebuild`); a forced Rebuild would surface only pre-existing vendored-project (SVGControl, UtilitiesSwordfish) errors that are outside this plan's scope.
