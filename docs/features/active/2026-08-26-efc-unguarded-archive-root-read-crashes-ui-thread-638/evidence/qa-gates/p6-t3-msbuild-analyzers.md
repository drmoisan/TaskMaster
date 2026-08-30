# [P6-T3] MSBuild analyzer gate (Issue 638)

Timestamp: 2026-08-29T12-37

Command:

```
$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath 'TestResults\msbuild\p6-t3.log'
```

Same vswhere-resolved MSBuild as [P0-T10], including the `| Select-Object -First 1` suffix.
The resolved path is absolute and is therefore recorded unresolved.

EXIT_CODE: 0

Output Summary:

MSBuild's own summary lines, quoted verbatim:

```
    5 Warning(s)
    0 Error(s)
```

The error count is read from that summary line, not by counting `error CS` occurrences.

## Non-vacuity

The tee'd log `TestResults\msbuild\p6-t3.log` contains **0** occurrences of the literal
`Skipping target "CoreCompile"`, so no project skipped compilation and the analyzers
actually ran. For contrast, the same log contains 86 lines mentioning `CoreCompile`,
which are the target invocations themselves.

This is why `/t:Rebuild` is used rather than `/t:Build`: MSBuild's incremental up-to-date
check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would return
exit 0 with `CoreCompile` skipped on every project and would run no analyzers at all.

The warning count is unchanged from the `BASELINE_ANALYZER_ERRORS: 0` / 5-warning baseline
recorded by [P0-T10], so this change introduces no analyzer diagnostic.
