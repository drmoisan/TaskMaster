# Final QC Step 3 — Nullable Type-Check (Warnings-as-Errors) (Cycle 3)

Timestamp: 2026-06-08T19-44

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
Executed (git-bash dash-switch form, -t:Rebuild to surface the vendored baseline): MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -m

EXIT_CODE: 1

Output Summary:
- Result holds at the ESTABLISHED VENDORED-ONLY BASELINE. The non-zero exit is produced
  entirely by the two vendored projects; there is no first-party regression.
- Total errors: 168, ALL confined to the two vendored projects:
  - UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj: 100 errors
  - SVGControl\SVGControl.csproj: 68 errors
- First-party error lines (excluding vendored): 0. Distinct error-bearing projects are
  exactly the two vendored projects; no solution-level or untagged error lines.
- CS8032 count: 0 (no SecurityCodeScan loader failure; no CS8032 suppression present).
- AC5: the nullable gate does NOT regress. The formatting-only change to ToDoItemTests.cs
  introduces zero new errors; the vendored-only error set is unchanged from the prior
  accepted baseline. (Note: a forced-nullable -t:Rebuild is required to recompile the
  vendored assemblies and surface these pre-existing errors; an incremental -t:Build
  reports 0 because the vendored assemblies are not recompiled.)
- Rebuild with TreatWarningsAsErrors aborts downstream Debug test-DLL output; a plain
  -t:Build -p:Configuration=Debug is run afterward (before P2-T5) to restore the
  first-party test DLLs for vstest.
