# P6-T3 — Compile entry for the AC4 latch test file

Timestamp: 2026-09-07T14-38
Task: [P6-T3]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command 'Select-String -Path QuickFiler.Test\QuickFiler.Test.csproj -SimpleMatch "QfcItemController.SearchLeaveLatchTests.cs"'
```

EXIT_CODE: 0

Matching lines: 1

```
L159: <Compile Include="Controllers\QfcItemController.SearchLeaveLatchTests.cs" />
```

Exactly one matching line, as the gate requires. The entry was placed alongside the existing
Controllers entries, immediately after the entry for Controllers\QfcFormControllerDeactivateTests.cs
which stood at line 158, so that entry now stands at 158 and the new one at 159.

The project is non-SDK-style, so without this entry the new file would be silently not compiled and
the fail-before evidence in task P6-T4 would be vacuous. Task P6-T4 additionally fails rather than
passes if its run reports a Total of 0, which is the independent check that the entry took effect.

Diff scope for this task:

```
git diff --stat -- QuickFiler.Test/QuickFiler.Test.csproj
```

```
 QuickFiler.Test/QuickFiler.Test.csproj | 1 +
 1 file changed, 1 insertion(+)
```

One inserted line and nothing else.

## Compile check

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p6-t3\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0
Build summary: 1 Warning(s). 0 Error(s).
Raw log (gitignored): TestResults/796/p6-t3/analyzer-rebuild.log

The single warning is the same transient CS0649 recorded at task P6-T1 against the not-yet-assigned
`_searchOwnedDismissal` field. It is unrelated to this task's edit, which is one line in a project
file, and it clears when task P6-T5 adds the producers.

Output Summary: exactly one compile entry for the new test file, at line 159; the solution rebuilds
clean apart from the transient CS0649 carried from P6-T1.
