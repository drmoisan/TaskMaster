# [P3-T14] Regression-test compile (Issue 638)

Timestamp: 2026-08-29T12-31

Command:

```
$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" 2>&1 | Tee-Object -FilePath 'TestResults\msbuild\p3-t14.log'
```

Same vswhere-resolved MSBuild as [P0-T10], including the `| Select-Object -First 1` suffix.
The resolved path is absolute and is therefore recorded unresolved.

EXIT_CODE: 0

Output Summary:

MSBuild's summary lines, quoted verbatim:

```
    5 Warning(s)
    0 Error(s)
```

The eleven new tests in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`
compile, and the `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` entry
added by [P3-T2] at `QuickFiler.Test/QuickFiler.Test.csproj:116` is picked up by the build.
The warning count is unchanged from the [P0-T10], [P0-T11] and [P2-T3] baselines, so the
new file introduces no diagnostic.

The [P3-T15] fail-before evidence can therefore be a runtime failure rather than a build
failure, which is the ordering hazard this task exists to close.
