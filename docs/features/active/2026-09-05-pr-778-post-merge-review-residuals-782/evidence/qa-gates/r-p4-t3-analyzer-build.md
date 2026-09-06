# [P4-T3] Final QC step 3 — analyzer build

Timestamp: 2026-09-06T01-50

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Run from the worktree root, in the same uninterrupted toolchain pass as [P4-T1] and [P4-T2].
`/t:Rebuild` is used rather than `/t:Build` so `CoreCompile` is not skipped by MSBuild
incrementality and the analyzers actually run.

EXIT_CODE: 0

Output Summary: the build succeeded with no analyzer diagnostics. The final summary lines, verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

FINAL-ANALYZER-WARNINGS: 0
FINAL-ANALYZER-ERRORS: 0

The three figures are identical to the [P0-T8] baseline, so this remediation introduces no analyzer
diagnostic.
