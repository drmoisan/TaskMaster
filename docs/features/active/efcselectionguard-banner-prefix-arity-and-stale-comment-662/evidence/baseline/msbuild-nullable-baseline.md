# MSBuild Nullable / Type-Check Gate — Baseline (P0-T9)

Timestamp: 2026-09-01T15-50

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

The `$msbuild` prelude resolved MSBuild to the absolute path recorded in the
`Command:` field above. The `if (-not $msbuild) { throw ... }` guard did not
fire.

Output Summary:

MSBuild summary block, transcribed with the worktree root replaced by
`<repo-root>` per the artifact-hygiene rule:

```
Build succeeded.

    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.36
```

Build-result line: `Build succeeded.`
Warning count: 5. Error count: 0.

The five warnings are the same pre-existing
`System.Reactive.PackagesConfigCheck.targets` `packages.config` diagnostic
recorded in the P0-T8 artifact, emitted once per affected project. They are not
promoted to errors by `/p:TreatWarningsAsErrors=true` because they are emitted
by a targets file rather than by the compiler, so the gate passes with
`0 Error(s)`.

No `CS86xx` nullable-flow diagnostic was reported. Nullable enforcement in this
repository is per-file opt-in through the `#nullable enable` directive, which
both `EfcSelectionGuard.cs:1` and `FolderSuggestionTree.cs:1` carry, so both
files this work edits are already inside the gate's scope.

`/p:Nullable=enable` was NOT added; `/t:Rebuild` was used rather than
`/t:Build`. Both choices follow CLAUDE.md, which records why each is
load-bearing.
