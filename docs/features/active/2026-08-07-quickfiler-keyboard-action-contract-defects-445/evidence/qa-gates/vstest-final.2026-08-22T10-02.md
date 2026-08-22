# Phase 5 Stage 4 — Testing, Full Suite (Issue #445, AC21 stage 4)

Timestamp: 2026-08-22T10-02

Command:
```powershell
$assemblies = Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' } | ForEach-Object { Resolve-Path -LiteralPath $_.FullName -Relative } | Where-Object { $_ -notmatch '\\\.claude\\' }
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' @assemblies /EnableCodeCoverage /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook' '/ResultsDirectory:coverage'
```
This is the CLAUDE.md CUT3 step 4 command. `$assemblies` was re-resolved with the P0-T14 workspace-relative idiom rather than reused. Run from `WS` via `pwsh -NoProfile`. `/InIsolation` present per Non-negotiable Command Constraint 3.

EXIT_CODE: 0

## Assemblies actually run (9, re-resolved)

```
.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
.\SVGControl.Test\bin\Debug\SVGControl.Test.dll
.\Tags.Test\bin\Debug\Tags.Test.dll
.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
.\TaskTree.Test\bin\Debug\TaskTree.Test.dll
.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

The count is 9, identical to the P0-T14 resolution, so the final run covers exactly the same scope as the baseline and the comparison below is like-for-like. No assembly from a sibling agent worktree was collected: the `\.claude\` exclusion was applied to the workspace-RELATIVE path, which is load-bearing because `WS` itself sits under `.claude\worktrees\`.

## Repository-wide totals

```
Test Run Successful.
Total tests: 6441
     Passed: 6441
```

| Measurement | Baseline (P0-T15) | Final (P5-T6) | Delta |
|---|---|---|---|
| Total tests | 6437 | **6441** | +4 |
| **Passed** | 6437 | **6441** | +4 |
| **Failed** | 0 | **0** | 0 |
| **Skipped** | 0 | **0** | 0 |
| Exit code | 0 | **0** | — |

Corroborating line-oriented counts over the run output: lines matching `^\s+Failed\s` = **0**; lines matching `^\s+Skipped\s` = **0**.

The Total rose by exactly 4, which is the four new tests added by P1-T2 through P1-T5 (test (a), test (b), test (c), test (d)). No test was lost: 6437 + 4 = 6441. The AC12 rename does not change the count because a renamed method is still one method.

## QuickFiler.Test Passed count

The assembly-scoped figure captured at P4-T5 under the same build is **907 Passed, 0 Failed** (baseline 903), which is the same +4 delta localized to the assembly this change touches. The remaining 5534 tests across the other eight assemblies are unaffected, consistent with this change touching no file outside `QuickFiler` and `QuickFiler.Test`.

## Failing-test-name set and gate evaluation

**Failing-test-name set: EMPTY.**

The gate passes only when the failing-test-name set is a subset of the P0-T15 baseline failing set **and** no failing test belongs to `KaStringAsyncTests`, `KaCharTests`, `KaKeyTests`, `KbdActionsTests`, or `KbdActionsRemainingBranchesTests`.

- The P0-T15 baseline failing set is **empty**. As the plan states, "An empty baseline failing set therefore requires zero failures repository-wide."
- The observed failing set is **empty**, so zero failures repository-wide is satisfied.
- The empty set is trivially a subset of the empty baseline set.
- The empty set contains no test in any of the five named classes.

**Gate: PASS**, under its strictest possible form. Because the baseline failing set was empty, no pre-existing failure was available as an exemption and this gate was equivalent to an absolute zero-failure requirement.

## Surviving pre-existing failures

**None.** There are no surviving pre-existing failures to list by name. The two pump tests the plan identifies as owned by wave-0 siblings #511/#571 — `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` and `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` — passed in the baseline, at P4-T5, and in this final run. They are load-flaky rather than deterministically red, machine load was low throughout, and neither is claimed as an exemption anywhere in this plan's evidence.

## No mass-failure artefact

`/InIsolation` was supplied. The known failure mode of omitting it — roughly 1,695 phantom failures with empty messages and sub-millisecond durations, surfacing as a Moq `TypeInitializationException` via `System.Threading.Tasks.Extensions` — did not occur, and no test was "fixed" to work around an assembly-load problem. There was no aggregate "Test host process crashed" report, so no per-assembly re-run was required.

Output Summary: `Test Run Successful.` with EXIT_CODE **0**. Repository-wide totals are **Total 6441, Passed 6441, Failed 0, Skipped 0** across all 9 re-resolved test assemblies, against a baseline of 6437/6437/0/0 — a delta of exactly +4, which is the four new regression tests, with no test lost. The `QuickFiler.Test` assembly reports 907 Passed (baseline 903), the same +4 localized to the only assembly this change touches. The **failing-test-name set is empty**, so the gate passes in its strictest form: because the P0-T15 baseline failing set was also empty, the subset condition reduced to an absolute zero-failure requirement repository-wide, and no failing test exists in `KaStringAsyncTests`, `KaCharTests`, `KaKeyTests`, `KbdActionsTests`, or `KbdActionsRemainingBranchesTests`. There are no surviving pre-existing failures to name. `/InIsolation` was supplied and no phantom mass-failure or test-host crash occurred. Stage 4 of the AC21 final toolchain pass is green.
