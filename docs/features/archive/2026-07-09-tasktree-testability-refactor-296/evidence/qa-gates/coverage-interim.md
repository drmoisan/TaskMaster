# QA Gate — Interim Coverage (P6-T3)

Timestamp: 2026-07-09T17-45
Command: dotnet-coverage collect -f cobertura -o artifacts/csharp/coverage.xml -s TaskTree.Test/coverage.tasktree.runsettings "<vstest.console.exe>" TaskTree.Test\bin\Debug\TaskTree.Test.dll /InIsolation
EXIT_CODE: 0
Output Summary:
- Tests: 37 passed, 0 failed (TaskTreeControllerTests + TaskTreeControllerMoveLogicTests).
- Coverage measured with the standard MS Code Coverage attribute excludes (DebuggerHidden,
  DebuggerNonUserCode, CompilerGenerated, GeneratedCode, ExcludeFromCodeCoverage) so that the
  compiler-generated closures/state-machines of the [ExcludeFromCodeCoverage] COM methods are not
  counted (this mirrors the MS collector's default behavior).

Numeric headline (TaskTree.dll):
- **TaskTree.dll line coverage: 94.04%** (>= 80% floor: PASS)
- **TaskTree/TaskTreeController.cs: 95.65% (66/69)** (>= 90% new-file target: PASS)
- **TaskTree/TaskTreeController.MoveLogic.cs: 93.29% (139/149)** (>= 90% new-file target: PASS)
- TaskTree/TaskTreeForm.cs, TaskTree/TreeListViewVisual.cs: excluded via class-level
  [ExcludeFromCodeCoverage] (E1/E2); not counted.

Note (raw dotnet-coverage without attribute excludes reports 87.80% for the assembly and 80.00%
for TaskTreeController.cs, because it attributes the exempt COM methods' generated closures
separately; the standard-attribute-exclude run above is the policy-consistent measurement).

Binary outcome: TaskTree.dll >= 80% line AND both new files >= 90% line. PASS.
