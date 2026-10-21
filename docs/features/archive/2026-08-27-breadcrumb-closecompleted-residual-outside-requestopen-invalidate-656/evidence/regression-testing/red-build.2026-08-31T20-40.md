# Regression Testing — Build Before the Red Run (Issue #656)

Timestamp: 2026-09-01T14-41
Task: [P1-T2]

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" "/flp:LogFile=TestResults\msbuild\p1-t2-build.log;Verbosity=normal"
```

EXIT_CODE: 0

Results:

- Build summary: `5 Warning(s)` / `0 Error(s)`, elapsed 00:00:11.93. The five warnings are the same
  pre-existing System.Reactive `packages.config` diagnostic recorded in the Phase 0 baselines.
- `Test-Path QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` is True, so the scoped red run in P1-T3
  has a current test assembly containing the newly added test.

Significance: the new test was added to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`
before any production change, and it compiles against unmodified production code. It uses only
members that already exist on the unmodified tree — `CoordinatorHarness`, `ControlledHost.Enqueue`,
`ControlledHost.SetOpen`, `ControlledHost.CloseReasons`, `ControlledHost.IsOpen`,
`CoordinatorHarness.SelectorOpen`, and `BreadcrumbDropDownOpenCoordinator.SetDroppedDown` — and the
file's existing `using` directives already cover every type it references, so no `using` was added.
A non-zero exit here would have indicated a defect in the test text rather than the expected red.
The red observed in P1-T3 is therefore a runtime failure, not a compile failure.

Output Summary: Solution rebuilt successfully with 0 errors. The test assembly exists and contains
the new test.
