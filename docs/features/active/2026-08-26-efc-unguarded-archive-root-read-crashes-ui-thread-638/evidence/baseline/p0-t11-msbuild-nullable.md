# [P0-T11] MSBuild nullable baseline (Issue 638)

Timestamp: 2026-08-29T12-19

Command:

```
$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1 | Tee-Object -FilePath 'TestResults\msbuild\p0-t11.log'
```

Same vswhere-resolved MSBuild as [P0-T10], including the `| Select-Object -First 1`
suffix. The command carries neither `/p:Nullable=enable` nor `/t:Build`.

EXIT_CODE: 0

Output Summary:

MSBuild's summary lines, quoted verbatim:

```
    5 Warning(s)
    0 Error(s)
```

BASELINE_NULLABLE_ERRORS: 0

The baseline is clean, so the [P6-T4] acceptance of `0 Error(s)` is reachable. Console
output was tee'd to `TestResults\msbuild\p0-t11.log`, outside the diff under
`.gitignore:39`.
