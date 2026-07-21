# Baseline C# Nullable Build (Issue #269)

- Timestamp: 2026-07-08T09-42
- Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` (dash switches; incremental `-t:Build` immediately following the prior analyzer build, per `.claude/agent-memory/atomic-executor/project_build_test_env.md`)
- EXIT_CODE: 0

## Output Summary

`Build succeeded. 0 Warning(s). 0 Error(s).` All projects reported up-to-date/skipped `CoreCompile` targets since the immediately-preceding analyzer build already compiled all first-party projects. Note (documented environment quirk, not a defect): an incremental `-t:Build` under `-p:Nullable=enable -p:TreatWarningsAsErrors=true` does not force recompilation, so this baseline does not, by itself, surface the known pre-existing ~84 forced-nullable errors confined to vendored `SVGControl`/`UtilitiesSwordfish` (which require `-t:Rebuild` to surface). This baseline run uses the exact command specified by the plan/policy toolchain text (`-t:Build`, no `-t:Rebuild`), so its literal EXIT_CODE 0 is recorded as the baseline signal.
