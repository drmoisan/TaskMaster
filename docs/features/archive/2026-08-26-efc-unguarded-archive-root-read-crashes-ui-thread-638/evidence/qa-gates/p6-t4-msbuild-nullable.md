# [P6-T4] MSBuild nullable / warnings-as-errors gate (Issue 638)

Timestamp: 2026-08-29T12-37

Command:

```
$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1 | Tee-Object -FilePath 'TestResults\msbuild\p6-t4.log'
```

The `Command:` above contains neither `/p:Nullable=enable` nor `/t:Build`. Both omissions
are load-bearing and are recorded in `CLAUDE.md` § C#1.3: `/p:Nullable=enable` is a
solution-wide opt-in absent from CI's command that conscripts every file which has never
adopted the pragma, and a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on
every project.

Same vswhere-resolved MSBuild as [P0-T10], including the `| Select-Object -First 1` suffix.
The resolved path is absolute and is therefore recorded unresolved.

EXIT_CODE: 0

Output Summary:

MSBuild's own summary lines, quoted verbatim:

```
    5 Warning(s)
    0 Error(s)
```

## Non-vacuity

The tee'd log `TestResults\msbuild\p6-t4.log` contains **0** occurrences of the literal
`Skipping target "CoreCompile"`, so compiler and nullable-flow diagnostics actually ran on
every project.

The warning count is unchanged from the `BASELINE_NULLABLE_ERRORS: 0` / 5-warning baseline
recorded by [P0-T11], so this change introduces no nullable or compiler diagnostic. Neither
`QuickFiler/Controllers/EfcDataModel.cs` nor
`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` carries a `#nullable enable`
directive, so neither opts into nullable flow analysis; the gate still compiles both under
warnings-as-errors.
