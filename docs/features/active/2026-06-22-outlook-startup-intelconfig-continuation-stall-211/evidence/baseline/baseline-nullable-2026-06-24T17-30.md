# Baseline — Nullable / TreatWarningsAsErrors (AC10, issue #211)

Timestamp: 2026-06-24T19-09
Command: msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true
EXIT_CODE: 0

Note: MSBuild resolved to VS18 (18.7.8).

Output Summary:
- Build succeeded. 0 nullable/TWAE errors. All 19 projects produced output, including TaskMaster.dll and TaskMaster.Test.dll.
- Environment caveat (carried from prior baselines): an incremental `-t:Build` does not recompile vendored projects (SVGControl, UtilitiesSwordfish) which carry pre-existing nullable diagnostics only surfaced by `-t:Rebuild`. The plan command specifies `-t:Build` and was run verbatim. The first-party files this plan touches (TaskMaster, TaskMaster.Test) compile clean under the nullable/TWAE gate.
- Baseline nullable/TWAE state: PASS.
