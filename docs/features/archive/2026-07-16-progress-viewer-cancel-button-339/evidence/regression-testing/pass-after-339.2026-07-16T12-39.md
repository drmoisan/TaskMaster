Timestamp: 2026-07-16T15-19

Command: `pwsh -NoProfile -Command '& { $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; if (-not (Test-Path $vswhere)) { exit 1 }; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; if (-not $vstest) { exit 1 }; $runSettings = (Resolve-Path "scripts/vscode/TaskMaster.cli.runsettings").Path; & $vstest "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "/Settings:$runSettings" "/InIsolation" "/TestCaseFilter:FullyQualifiedName~CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick"; exit $LASTEXITCODE }'`

EXIT_CODE: 0

Output Summary:

- PASS: `CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick` passed after the targeted setter fix.
- Total tests: 1; passed: 1; failed: 0.
- The test verified that assigning a non-null source through `CancelSource` enabled the real Cancel button after `viewer.Show()`.
- The test then selected the button with `PerformClick()` and verified that the token captured from that same configured source reported cancellation.

Command Output:

```text
Passed CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick [237 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
Total time: 1.4748 Seconds
```
