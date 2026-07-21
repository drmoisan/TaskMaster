# Final QA — Test Coverage (P7-T4)

Timestamp: 2026-07-09T17-55
Command: dotnet-coverage collect -f cobertura -o artifacts/csharp/coverage.xml -s TaskTree.Test/coverage.tasktree.runsettings "<vstest.console.exe>" TaskTree.Test\bin\Debug\TaskTree.Test.dll /InIsolation
EXIT_CODE: 0
Output Summary:
- Tests: 37 passed, 0 failed.
- Raw Cobertura coverage XML written to artifacts/csharp/coverage.xml (review-gate consumable).
- Coverage measured with the standard MS Code Coverage attribute excludes (CompilerGenerated,
  GeneratedCode, DebuggerHidden, DebuggerNonUserCode, ExcludeFromCodeCoverage).

Numeric post-change coverage (TaskTree.dll):
- TaskTree.dll line coverage: **94.04%**
- TaskTree/TaskTreeController.cs: **95.65% (66/69)**
- TaskTree/TaskTreeController.MoveLogic.cs: **93.29% (139/149)**

Binary outcome: all tests pass and coverage numbers recorded.
