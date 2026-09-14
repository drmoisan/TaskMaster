# Revision R7 Rebuild After New Test File

Timestamp: 2026-09-14T12-41

Command:
```
pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$msb = @(& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe")[0]
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "TestResults/r7-build/r7-build-console.txt"
$LASTEXITCODE
'
```

EXIT_CODE: 0
ExpectedExitCode: 0

Output Summary:

```
R7_BUILD_LOG_DIR_PRESENT=True
ZERO_ERRORS_LINES=1
SKIPPED_CORECOMPILE=0
CONTROL_BUILD_OUTPUT=8
```

The rebuild compiled the new file
`UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`, registered by `[P4-T14]`,
and completed with no errors. `SKIPPED_CORECOMPILE=0` establishes that no project skipped
compilation, so the count is a reading of a real compile rather than of an up-to-date check.
`CONTROL_BUILD_OUTPUT=8` is the positive control: the log names `UtilitiesCS.Test.dll` eight
times, so the file is a genuine build log of the project this revision changes and the search
mechanism is live.

The console log is written to `TestResults/r7-build/r7-build-console.txt`, which is under the
git-ignored `TestResults/` scratch tree. It is therefore not one of the six raw console logs
that `[P5-T11]` projects and `[P5-T12]` removes.

Build lock: acquired for item 879 before the rebuild and released after it.
