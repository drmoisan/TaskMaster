# QC loop step 3 — analyzer gate (Issue #824, task P5-T4)

Timestamp: 2026-09-09T16-02

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $msb = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath coverage/msbuild-analyzers-final.log; exit $LASTEXITCODE'`

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:10.92
```

This is AC11 step 2, run character-for-character in the form CLAUDE.md quotes. `/t:Rebuild` was
used rather than `/t:Build`, because MSBuild's timestamp-based up-to-date check does not invalidate
on a command-line `/p:` change and a warm `/t:Build` would return exit 0 with `CoreCompile` skipped
on every project, running no analyzers.

The non-vacuity proof for this run is recorded separately in
`evidence/qa-gates/msbuild-analyzers-nonvacuity.2026-09-09T16-02.md`, as the plan requires: an exit
code alone cannot establish that this gate compiled anything.

The log stays under `coverage/`, which is gitignored, and is not committed.
