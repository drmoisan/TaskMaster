# Build after adding the AC2 gate (Issue #824, task P1-T2)

Timestamp: 2026-09-09T15-24

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $msb = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msb TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" 2>&1 | Tee-Object -FilePath coverage/build-p1t2.log | Select-Object -Last 5'`

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.21
```

Lines matching `: error [A-Z]+[0-9]+:` in `coverage/build-p1t2.log`: **0**.

`LoadOpCodes_DoesNotRepublishPublishedTables` compiles against the unfixed tree. The test is
expected to fail when run, which P1-T3 records; it is expected to compile, which this task records.
The log stays under `coverage/` and is not committed.
