# Baseline nullable build (Issue #824, task P0-T10)

Timestamp: 2026-09-09T15-09

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $msb = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1 | Tee-Object -FilePath coverage/msbuild-nullable-baseline.log | Select-Object -Last 6'`

EXIT_CODE: 0

Output Summary:

Terminal console lines:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.92
```

Integers read from `coverage/msbuild-nullable-baseline.log`:

| Pattern | Count |
|---|---|
| `Skipping target "CoreCompile"` | 0 |
| `^\s*CoreCompile:` | 13 |
| `: error [A-Z]+[0-9]+:` | 0 |
| `: warning [A-Z]+[0-9]+:` | 0 |
| `: error CS86[0-9][0-9]:` | 0 |

Supporting count: `^\s*0 Error\(s\)$` = 1.

`/p:Nullable=enable` was not added. Nullable enforcement in this repository is per-file opt-in and
the solution-wide property conscripts files that never adopted the pragma. `/t:Rebuild` was used
rather than `/t:Build`, so the gate actually compiled: 13 `CoreCompile:` occurrences with zero
`Skipping target "CoreCompile"` lines.

The `CoreCompile:` count differs from the analyzer gate's count of 9 recorded in P0-T9. Per plan D14
this difference is expected on the same tree and is not a defect.

The log stays under `coverage/`, which is gitignored, and is not committed.
