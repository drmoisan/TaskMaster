# QA Gate — Phase 1 Builds (P1-T8, re-run after the SD18 revert)

Timestamp: 2026-09-05T21-59

This artifact overwrites the superseded record in place. Both builds were re-run over the tree
produced by the rewritten P1-T3, which reverts the C03 latch re-arm under SD18. The superseded
record's acceptance carried a clause asserting that the re-armed latch introduced no analyzer
diagnostic; that clause is removed, there being no re-armed latch after SD18. The `Timestamp:`
above is this re-run's own instant, not the superseded one.

Command:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"

msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

`/t:Rebuild` is used rather than `/t:Build` in both cases: MSBuild's up-to-date check does not
invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile`
skipped on every project and the gate cannot fail. `/p:Nullable=enable` is not added to the second
command; no project in this repository carries a `<Nullable>` element and CI omits the property
deliberately.

EXIT_CODE: 0

That is the larger of the two observed exit codes. The analyzer build exited 0 and the nullable
build exited 0.

Output Summary:

### Analyzer build

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Observed exit code: 0.

### Nullable build

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Observed exit code: 0.

Both builds recorded `0 Warning(s)` and `0 Error(s)` over the reverted Phase 1 tree, which carries
the P1-T1 shared message constant, the P1-T2 getter rewrite, the P1-T4 `WpfDispatcherYield` change,
the P1-T5 test-assertion change, the P1-T6 lambda-capture change in both `ProgressTracker` files, and
the P1-T7 dead-null-comparison removal in `RibbonViewer.EngineCommands.cs`, and which carries no
latch re-arm in `UiThread.Init()`.
