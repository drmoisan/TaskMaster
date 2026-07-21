Timestamp: 2026-07-16T15-17

Command: `pwsh -NoProfile -Command '& { $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; if (-not (Test-Path $vswhere)) { exit 1 }; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; if (-not $vstest) { exit 1 }; $runSettings = (Resolve-Path "scripts/vscode/TaskMaster.cli.runsettings").Path; & $vstest "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "/Settings:$runSettings" "/InIsolation" "/TestCaseFilter:FullyQualifiedName~CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick"; exit $LASTEXITCODE }'`

EXIT_CODE: 1

Output Summary:

- EXPECTED FAIL: the newly discovered regression test ran against the unchanged production setter.
- Total tests: 1; passed: 0; failed: 1.
- The failure occurred specifically at the enabled-state assertion because `cancelButton.Enabled` was `False` after assigning a non-null `CancelSource`.
- The test reached the intended assertion; there was no discovery failure, build failure, reflection error, or unrelated exception.

Command Output Excerpt:

```text
Failed CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick [313 ms]
Error Message:
 Expected cancelButton.Enabled to be True because assigning CancelSource must enable cancellation while loading, but found False.
Stack Trace:
 at UtilitiesCS.Test.Threading.ProgressViewer_Tests.CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick() in UtilitiesCS.Test\Threading\ProgressViewer_Tests.cs:line 75

Total tests: 1
     Failed: 1
Total time: 1.5737 Seconds
Test Run Failed.
```
