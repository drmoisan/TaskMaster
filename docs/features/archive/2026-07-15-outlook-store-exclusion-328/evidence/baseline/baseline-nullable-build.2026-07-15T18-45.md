# Baseline — Nullable / TreatWarningsAsErrors Build (Issue #328)

Timestamp: 2026-07-15T18-45
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). Incremental -t:Build finds
first-party assemblies up-to-date after the analyzer build, so no touched-path nullable
warnings surface at baseline.

Note: A forced `-t:Rebuild` under this gate surfaces ~84 pre-existing nullable errors
confined to vendored SVGControl and UtilitiesSwordfish (documented repo behavior); those
vendored assemblies are out of scope for #328. The meaningful gate for this feature is
whether the touched first-party projects (UtilitiesCS, ToDoModel, TaskMaster and their
test projects) introduce new nullable warnings when they recompile incrementally after
edits — the loop re-runs this command after each change.
