# [P0-T10] MSBuild analyzer baseline (Issue 638)

Timestamp: 2026-08-29T12-18

Command:

```
$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath 'TestResults\msbuild\p0-t10.log'
```

The MSBuild path is recorded unresolved, as the vswhere expression, because the resolved
path is absolute. The `| Select-Object -First 1` suffix is present because `-find` can
emit several matching paths.

EXIT_CODE: 0

Output Summary:

MSBuild's own summary lines, quoted verbatim:

```
    5 Warning(s)
    0 Error(s)
```

BASELINE_ANALYZER_ERRORS: 0

The baseline is clean, so the [P6-T3] acceptance of `0 Error(s)` is reachable. Console
output was tee'd to `TestResults\msbuild\p0-t10.log`, which `.gitignore:39`
(`[Tt]est[Rr]esult*/`) keeps outside the diff.

`scripts/vscode/Invoke-VSBuild.ps1` was not invoked: it runs
`scripts/vscode/Sync-PackageReferences.ps1` over every `.csproj` and would rewrite project
files outside this change footprint.
