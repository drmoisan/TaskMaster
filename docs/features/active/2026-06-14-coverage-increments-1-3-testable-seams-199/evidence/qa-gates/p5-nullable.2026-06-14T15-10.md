# P5-T9 — Nullable + Warnings-as-Errors Build (Phase 5)

- Timestamp: 2026-06-14T15-10
- Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` (VS18 Community MSBuild)
- EXIT_CODE: 1 (solution aggregate, due to vendored projects only — see below); first-party result: PASS (0 errors)

## Output Summary

The nullable / warnings-as-errors gate is clean for all first-party code, including the Phase 5
changes. The solution-aggregate exit is non-zero solely because the solution-wide
`-p:Nullable=enable` flag forces nullable analysis onto the two vendored projects that ship with
nullable disabled.

Evidence:
- Total `: error` lines: 168 (84 distinct CS86xx/CS06xx diagnostics x build phases).
- Distinct projects reporting errors: exactly two — `SVGControl.csproj` and
  `UtilitiesSwordfish.NET.General.csproj`. Both are vendored projects explicitly excluded from
  the repository analyzer stack (per `.claude/rules/csharp.md`: "the 4 vendored projects ... are
  excluded"). They are not touched by this feature.
- Non-vendored error count: 0. Filtering the full build log for `.cs(` errors outside
  `SVGControl.csproj`/`UtilitiesSwordfish` yields zero matches. No diagnostic appears in
  `UtilitiesCS`, `TaskMaster`, `ToDoModel.Test`, or `TaskMaster.Test`.

The two Phase 5 production seams introduce no nullable warnings:
- `UtilitiesCS/Properties/AssemblyInfo.cs` — assembly attribute only.
- `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` — pure-helper extraction
  (`internal static string MatchBestSpecialFolder(IReadOnlyDictionary<string,string>, string)`),
  return type and nullability identical to the original instance method.

Note (build mechanics, consistent with prior-phase evidence and the documented forced-nullable
behavior): the first incremental nullable `-t:Build` reported 0/0 because all outputs were
up-to-date from the preceding analyzer build and were skipped; touching the changed source files
forced recompilation, at which point the global `Nullable=enable` flag also re-evaluated the
vendored projects and surfaced their pre-existing nullable violations. After confirming the
first-party result is clean, a plain `MSBuild ... -t:Build -p:Configuration=Debug` (no nullable
flag) was run to restore the Debug outputs: Build succeeded, 0 errors, 62 pre-existing warnings.
