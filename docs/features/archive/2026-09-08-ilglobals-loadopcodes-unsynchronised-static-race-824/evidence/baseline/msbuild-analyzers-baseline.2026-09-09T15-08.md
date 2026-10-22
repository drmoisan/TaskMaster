# Baseline analyzer build (Issue #824, task P0-T9)

Timestamp: 2026-09-09T15-08

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $msb = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath coverage/msbuild-analyzers-baseline.log | Select-Object -Last 6'`

EXIT_CODE: 0

Output Summary:

Terminal console lines:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:14.93
```

Four integers read from `coverage/msbuild-analyzers-baseline.log`:

| Pattern | Count |
|---|---|
| `Skipping target "CoreCompile"` | 0 |
| `^\s*CoreCompile:` | 9 |
| `: error [A-Z]+[0-9]+:` | 0 |
| `: warning [A-Z]+[0-9]+:` | 0 |

Supporting count: `^\s*0 Error\(s\)$` = 1.

The `CoreCompile:` count of 9 with zero `Skipping target "CoreCompile"` lines establishes that the
gate actually compiled. This matters because MSBuild's timestamp-based up-to-date check does not
invalidate on a command-line `/p:` change, so a gate evidenced only by `EXIT_CODE: 0` could not
fail; `/t:Rebuild` plus the two directional counts is what makes it non-vacuous.

A search for the bare word `error` is not used, because a successful msbuild run prints it about 35
times and `Select-String` is case-insensitive by default.

Invocation note: `| Select-Object -Last 6` was appended after the `Tee-Object` stage so the console
transcript stays small. `Tee-Object` writes the complete stream to the log file before that stage,
so the log the counts are read from is unaffected. The trailing `exit $LASTEXITCODE` of the plan
form was replaced by `Write-Output ("EXIT=" + $LASTEXITCODE)` so the value appears in the console
transcript; the value asserted is the same. The log stays under `coverage/`, which is gitignored,
and is not committed.
