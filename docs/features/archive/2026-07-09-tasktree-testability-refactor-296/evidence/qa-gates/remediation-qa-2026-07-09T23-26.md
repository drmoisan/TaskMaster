# Remediation QA Gates — #296 (E4/E5/E6 seam replacement, cycle pass 1)

Scope: replace three non-pre-ratified `[ExcludeFromCodeCoverage]` exemptions on testable seams
(E4 `ActivateOlItem`, E5 `ActivateOlItemAsync`, E6 `HandleModelDropped` drop routing) with mockable
seams plus real tests. Full C# toolchain run in strict order; a single clean pass is recorded below.

## Format (csharpier)

Timestamp: 2026-07-09T23-20
Command: csharpier format TaskTree TaskTree.Test
EXIT_CODE: 0
Output Summary: Formatted 16 files. Post-format `git status` shows only the intended changed/new files
(TaskTree/TaskTreeController.cs, TaskTree/TaskTreeController.MoveLogic.cs, TaskTree.Test/*.csproj,
TaskTreeControllerTests.cs, and the two new test files). No re-format churn on unrelated files.

## Lint / Analyzers

Timestamp: 2026-07-09T23-22
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded. No analyzer diagnostics introduced by the TaskTree/TaskTree.Test
changes. Pre-existing CS8632/CS0067 warnings in unrelated `*.Test` projects are baseline debt and are
not treated as errors in this (non-TWAE) analyzer gate.

## Type-Check (Nullable + TreatWarningsAsErrors)

Timestamp: 2026-07-09T23-24
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build succeeded, 0 Error(s), 0 Warning(s). Genuine full rebuild of all projects
(including TaskTree and TaskTree.Test). No new nullable warning-as-error introduced by the seam
refactor. The typed-`object` `ActivateOlItem(Async)` and the extracted `RouteDrop`/`ApplyPostDropView`
compile clean under strict nullable.

## Test + Coverage (Cobertura)

Timestamp: 2026-07-09T23-25
Command: dotnet-coverage collect -f cobertura -o artifacts/csharp/coverage.xml -s TaskTree.Test/coverage.tasktree.runsettings "<vstest.console.exe>" TaskTree.Test\bin\Debug\TaskTree.Test.dll /InIsolation
EXIT_CODE: 0
Output Summary:
- Tests: 51 passed, 0 failed (was 37; +14 new tests covering the previously-exempt seams).
- Runsettings scope: `TaskTree.dll` only, with the standard MS Code Coverage attribute excludes,
  including `ExcludeFromCodeCoverageAttribute` (so the 4 remaining thin/host-bound exemptions stay out
  of the denominator).

Numeric post-change coverage (TaskTree.dll):
- TaskTree.dll line coverage: **96.34%** (263/273 lines) — up from 94.04% baseline.
- TaskTree.dll branch coverage: **91.49%** (86/94 branches).
- TaskTree/TaskTreeController.cs: **100.0% line** (branch 96.15%) — E4/E5 selectable/Display/valid-type
  caller paths now executed; up from 95.65%.
- TaskTree/TaskTreeController.MoveLogic.cs: **94.54% line** (branch 89.71%) — new `RouteDrop`
  (all `DropTargetLocation` branches) and `ApplyPostDropView` covered; up from 93.29%.

Thresholds: line >= 80% (repo/TaskTree) and >= 90% new/changed code — met (96.34% overall; changed
controller file 100%, changed MoveLogic file 94.54%). Branch >= 75% — met (91.49%).

Known-flaky note: the `PhysicalFileInfoAdapter_..._MirrorFileInfo` test is in a different assembly and
is not part of the TaskTree.Test run; no re-run was required. All 51 TaskTree.Test cases passed on the
first attempt.

## Remaining `[ExcludeFromCodeCoverage]` (exactly four, all legitimate)

Verification command: `grep -rn "ExcludeFromCodeCoverage" TaskTree/*.cs`

1. `TaskTree/TaskTreeForm.cs` (type) — E1, Form-derived host surface.
2. `TaskTree/TreeListViewVisual.cs` (type) — E2, minimal ObjectListView host adapter.
3. `TaskTree/TaskTreeController.cs` `FormatRow` — E3, thin residual event-handler wrapper
   (FormatRowEventArgs/OLVListItem not constructible; decision extracted to covered `ResolveRowStyle`).
4. `TaskTree/TaskTreeController.MoveLogic.cs` `HandleModelDropped` — E6 residual wrapper only
   (builds E2 adapters from live `e.ListView`/`e.SourceListView` and calls `e.RefreshObjects()`;
   routing extracted to covered `RouteDrop`, post-drop view re-application to covered
   `ApplyPostDropView`).

E4 and E5 exemptions removed; no `[ExcludeFromCodeCoverage]` remains on a testable seam.
