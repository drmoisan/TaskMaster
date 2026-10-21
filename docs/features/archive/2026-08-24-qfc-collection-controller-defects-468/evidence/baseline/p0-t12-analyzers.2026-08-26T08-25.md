# [P0-T12] Analyzer baseline (.NET analyzers)

Timestamp: 2026-08-26T08-25

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild`

Emitted MSBuild command line (the wrapper's `Get-MSBuildBuildArguments` output, host paths replaced
with `<WS>`):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" <WS>\TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m
```

This is the `CLAUDE.md` §C#1.2 policy command; the wrapper emits `/m` last rather than immediately
after `/t:Rebuild`, which is switch-order-equivalent. `/p:Nullable=enable` is deliberately absent.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

MSBuild version: `18.8.2+ce25c0108 for .NET Framework`, resolved through `vswhere.exe` to Visual
Studio **18** Community. The wrapper's pre-step reported
`Sync-PackageReferences: All HintPaths are up to date`, so it rewrote no `.csproj`.

### Result counts

| Metric | Value |
|---|---|
| Exit code | **0** |
| Errors | **0** |
| Warnings | **5** |
| Analyzer diagnostics (any of the five wired analyzers) | **0** |
| Distinct projects that executed `CoreCompile` | **18** |
| `Skipping target "CoreCompile"` occurrences | **0** |
| Wall time | 00:00:19.26 |

### Non-vacuity proof

The plan requires a non-zero count of projects that executed `CoreCompile`, because a warm
`/t:Build` returns exit 0 having skipped `CoreCompile` on every project and run no analyzers.

- `grep -c 'Skipping target "CoreCompile"'` over the build log returns **0**. No project's
  compilation was skipped.
- **18** distinct `csc.exe` invocations appear in the log, matching 18 distinct `/out:` targets:
  `QuickFiler.dll`, `QuickFiler.Test.dll`, `SVGControl.dll`, `SVGControl.Test.dll`, `Tags.dll`,
  `Tags.Test.dll`, `TaskMaster.dll`, `TaskMaster.Test.dll`, `TaskTree.dll`, `TaskTree.Test.dll`,
  `TaskVisualization.dll`, `TaskVisualization.Test.dll`, `ToDoModel.dll`, `ToDoModel.Test.dll`,
  `UtilitiesCS.dll`, `UtilitiesCS.Test.dll`, `VBFunctions.dll`, `VBFunctions.Test.dll`.
- The analyzer assemblies were genuinely loaded. A representative `csc.exe` command line carries
  `/analyzer:..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`
  and `/analyzer:..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\...`, which
  is exactly the pair P0-T8 back-filled. Had P0-T8 been skipped, these would have been `CS0006`.

The gate is therefore non-vacuous.

### The five warnings, enumerated

All five are the **same** diagnostic, emitted once per project that consumes System.Reactive 7.0.0
through `packages.config`. None is an analyzer diagnostic and none originates in any file this
feature touches.

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference. (You can suppress this message by setting the
RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.)
```

Emitting projects: `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`,
`TaskMaster.csproj`, `UtilitiesCS.Test.csproj`.

This is the **baseline warning set**. P1-T6 and every later analyzer gate assert "no new diagnostic
relative to the P0-T12 baseline", meaning: exit code 0, error count 0, and a warning set that is a
subset of these five.

### Working-tree side effects

`git status --porcelain` immediately after the build is byte-identical to the P0-T10 capture: two
modified Markdown files and one untracked evidence directory. The rebuild introduced no `.csproj`,
`.cs`, `.xml`, or `.sln` change.

Result: PASS. All four acceptance conditions are met — the exact MSBuild command line, the exit
code, the warning and error counts, and a non-zero `CoreCompile` project count.
