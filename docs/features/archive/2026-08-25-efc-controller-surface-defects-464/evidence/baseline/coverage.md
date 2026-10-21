# Phase 0 — baseline repository-wide coverage

Timestamp: 2026-08-27T23-36
Task: [P0-T14]
Command: `& .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\efc-controller-surface-defects-464\evidence\baseline\coverage-baseline.cobertura.xml` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1

## Exit code disposition

The script exits 1 because it throws when the inner test run reports any failure
(`Invoke-MSTestWithCoverage.ps1:236`). Fifteen tests failed, all of them pre-existing load-driven
flakiness described below. Coverage collection itself completed and the Cobertura file was written.
`[P0-T14]`'s stated acceptance is that the Cobertura file exists and that the rates, the assembly count
and the assembly list are recorded; it asserts no exit code. A non-zero baseline result is recorded as a
pre-existing repository condition, not as a failure of this task.

## Cobertura root element

The file exists at
`docs/features/active/efc-controller-surface-defects-464/evidence/baseline/coverage-baseline.cobertura.xml`
(17,950,778 bytes). Its root `<coverage>` attributes:

| Attribute | Value |
|---|---|
| `line-rate` | `0.7032289508955769` |
| `branch-rate` | `0.5912137948480122` |
| `lines-covered` | 57714 |
| `lines-valid` | 82070 |
| `branches-covered` | 14023 |
| `branches-valid` | 23719 |
| `complexity` | 25254 |

This is the raw repository-wide denominator, which includes vendored and third-party code. It is
recorded so that `[P10-T8]` can compare a post-change figure produced by the identical command against
it. No absolute coverage threshold is asserted by this plan.

## Discovered test assemblies

**Count: 9.** Verbatim list, with the worktree root rendered as `<repo-root>`; every entry is under this
worktree root:

```
<repo-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
<repo-root>\SVGControl.Test\bin\Debug\SVGControl.Test.dll
<repo-root>\Tags.Test\bin\Debug\Tags.Test.dll
<repo-root>\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
<repo-root>\TaskTree.Test\bin\Debug\TaskTree.Test.dll
<repo-root>\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
<repo-root>\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
<repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
<repo-root>\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

No entry resolves outside this worktree. No sibling checkout and no path belonging to another agent's
worktree was discovered.

## Test tally

```
Total tests: 6719
     Passed: 6704
     Failed: 15
 Total time: 8.2192 Minutes
```

## The fifteen pre-existing failures

All fifteen are in `QuickFiler.Test` and belong to three pre-existing test files that this feature does
not own and does not touch:

- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`

```
BuildPumpHarness_DoesNotCreateTheWebViewChildHandles
BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread
CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing
CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController
EnsureDispatcher_ScopeDisposedTwice_IsIdempotent
EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose
EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt
InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults
InitializeBool_ThroughThePumpHost_CompletesAndInitializesState
InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme
InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates
InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState
Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException
Transaction_DisposedTwice_DoesNotOverReleaseTheGate
Transaction_SecondCallerCannotInstallUntilTheFirstRestores
```

**These are load-dependent, not deterministic.** `[P0-T12]` ran the same `QuickFiler.Test.dll` alone with
`/InIsolation` about fifteen minutes earlier and reported **1099 passed, 0 failed** — the same assembly,
the same binaries, the same commit. The failures appear only under the repository-wide run, where all
nine assemblies execute concurrently with coverage instrumentation attached. The affected classes drive
a WinForms pump host and swap a shared static `UiThread.Dispatcher`, both of which are sensitive to
machine load. **No source change had been made when this run executed**, so none of the fifteen can be
attributed to this feature.

The authoritative per-assembly baseline for this feature's own test gate is `[P0-T12]`'s isolated run and
its empty `BASELINE_FAILED` set; `[P10-T6]` compares against that.

## Retention of the Cobertura file

The raw Cobertura XML is retained **on disk but untracked**: it is 18 MB of machine-generated
measurement data, and raw coverage XML is not committed. It matches no `.gitignore` rule, so it appears
as an untracked path under `evidence/baseline/`. This is recorded here so that a later clean-tree gate
is not surprised by it.

Output Summary: Repository-wide baseline coverage is line-rate 0.7032289508955769 (70.32%) and
branch-rate 0.5912137948480122 (59.12%), from 57714/82070 lines and 14023/23719 branches, measured over
9 discovered test assemblies. The run executed 6719 tests with 6704 passed and 15 failed; all 15 are
pre-existing load-driven flakiness in three QfcItemController test files that pass in isolation
([P0-T12]: 1099/1099), and no source change had been made at the time of this run.
