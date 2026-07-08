# EmailMoveMonitor cleanup coverage rerun

Date: 2026-07-03

Cause:

- `EmailMoveMonitorTests.Cleanup` snapshots and asserts the process-global `UtilitiesCS.UiThread.Dispatcher` value.
- `TaskMaster.runsettings` enables MSTest class-level parallelization.
- Other QuickFiler tests intentionally replace `UiThread.Dispatcher` with dedicated WPF dispatchers, including `QfcItemControllerTestSupport.RunningDispatcher`.
- The failing cleanup observed that shared static mutation while `EmailMoveMonitorTests` was running in parallel.

Fix:

- Marked `EmailMoveMonitorTests` with `[DoNotParallelize]` because the class validates process-global dispatcher state.

Validation:

- `dotnet tool run csharpier -- format .`
- `dotnet tool run csharpier -- check .`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName=QuickFiler.Helper_Classes.Tests.EmailMoveMonitorTests.UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread"`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-emailmove-fix-results`
- `dotnet-coverage merge <coverage attachment> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-emailmove-fix.cobertura.xml -f cobertura`

Results:

- Targeted failing test: passed.
- QuickFiler coverage run: passed, 382 tests passed.
- Parsed Cobertura metrics from the rerun: line-rate `0.1834014426373385`, branch-rate `1`, lines-covered `13374`, lines-valid `72922`.
- The rerun coverage XML and `.coverage` attachment were removed after metric extraction to avoid adding another large generated coverage artifact.
