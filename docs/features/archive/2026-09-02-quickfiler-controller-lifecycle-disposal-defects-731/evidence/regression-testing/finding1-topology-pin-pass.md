# Finding 1 — three-owner monitor topology pin, passing run

Timestamp: 2026-09-03T14-10

Task: [P1-T7]
Issue: #731

## Command

1. Build:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`. Recording this absolute path in full is the narrow exception the Evidence path-hygiene rule states for an external build-tool executable that lives outside this worktree under `Program Files` and contains no account name.

2. Filtered test run:

```
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcMoveMonitorTopologyTests"
```

The vstest console was resolved with `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`, taking the first result, which is `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (VSTest version 18.9.0, x64).

EXIT_CODE: 0

The build exited 0 and the filtered test run exited 0.

## Output Summary

Build summary lines, as observed:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Test run output, as observed:

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Passed EachOwnerDeclaresExactlyOneEmailMoveMonitorInitializer [31 ms]
  Passed NoTypeDeclaresMoreThanOneEmailMoveMonitorField [29 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.3433 Seconds
```

- Total tests: **2**
- Passed: **2**
- Failed: **0**

Both `[TestMethod]` members of `QfcMoveMonitorTopologyTests` passed. `EachOwnerDeclaresExactlyOneEmailMoveMonitorInitializer` confirms that each of `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcDatamodel.cs` and `QuickFiler/Controllers/QfcQueue.cs` carries exactly one `EmailMoveMonitor` field initializer after the [P1-T1] through [P1-T3] comment insertions. `NoTypeDeclaresMoreThanOneEmailMoveMonitorField` confirms by reflection that no type in the QuickFiler assembly declares more than one `IEmailMoveMonitor` instance field and that exactly three types declare one.

This intermediate `/t:Build` is explicitly not the policy analyzer gate; the policy gate is [P5-T3] and uses `/t:Rebuild`.
