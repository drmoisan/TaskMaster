# Build after adding the two AC3 gates (Issue #824, task P1-T5)

Timestamp: 2026-09-09T15-29

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $msb = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msb TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" 2>&1 | Tee-Object -FilePath coverage/build-p1t5.log | Select-Object -Last 5'`

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.25
```

Lines matching `: error [A-Z]+[0-9]+:` in `coverage/build-p1t5.log`: **0**.

`SingleByteOpCodes_FieldIsInitOnly` and `MultiByteOpCodes_FieldIsInitOnly` compile against the
unfixed tree, together with the `using System.Reflection;` directive P1-T4 added. Both tests are
expected to fail when run, which P1-T6 records. The log stays under `coverage/` and is not
committed.
