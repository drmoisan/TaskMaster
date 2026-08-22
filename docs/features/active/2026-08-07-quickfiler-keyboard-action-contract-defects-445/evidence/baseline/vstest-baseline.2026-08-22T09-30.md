# Phase 0 — Full-Suite Pass/Fail Baseline (Issue #445)

Timestamp: 2026-08-22T09-30

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' @assemblies /EnableCodeCoverage /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook' '/ResultsDirectory:coverage'
```
with `@assemblies` the 9-element relative-path list resolved in P0-T14. Run from `WS` = `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6e508cbcd1e0a79d` via `pwsh -NoProfile`. `/InIsolation` is present, per Non-negotiable Command Constraint 3.

EXIT_CODE: 0

## Repository-wide totals

| Measurement | Value |
|---|---|
| Verdict | `Test Run Successful.` |
| Total tests | 6437 |
| **Passed** | **6437** |
| **Failed** | **0** |
| **Skipped** | **0** |
| Assemblies run | 9 |

`Failed` is 0, so **the pre-existing-failure set is EMPTY**. Corroboration: a line-oriented count over the captured run log returns 6437 lines beginning `  Passed `, 0 beginning `  Failed `, and 0 beginning `  Skipped `.

## Consequence for the P5-T6 and P4-T5 gates

This is the strictest possible baseline. Because the baseline failing set is empty, the P5-T6 subset gate degrades to an absolute requirement: **the final full-suite run must report zero failures repository-wide.** Any failing test at P5-T6 is necessarily a new failure, since no failure can be a member of an empty baseline set. The same applies to the P4-T5 QuickFiler-scoped gate.

The two pump tests the plan names as candidate pre-existing failures owned by wave-0 siblings #511/#571 — `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` and `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` — **both PASSED in this run**. They are known to be load-flaky rather than deterministically red, and machine load was low during this capture. They are therefore not members of the baseline failing set and cannot be cited later as pre-existing failures against this baseline.

## Assembly-scoped baseline: QuickFiler.Test

```
& vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook'
Test Run Successful.
Total tests: 903
     Passed: 903
EXIT_CODE: 0
```

## Per-class baseline for the five classes the scope-lock gates pin

Captured with `/TestCaseFilter:FullyQualifiedName~<ClassName>` against `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` with `/InIsolation`. These are the figures P4-T5 compares against.

| Test class | Total | Passed | Failed | Exit |
|---|---|---|---|---|
| `KaStringAsyncTests` | 8 | 8 | 0 | 0 |
| `KaCharTests` | 10 | 10 | 0 | 0 |
| `KaKeyTests` | 9 | 9 | 0 | 0 |
| `KbdActionsTests` | 3 | 3 | 0 | 0 |
| `KbdActionsRemainingBranchesTests` | 10 | 10 | 0 | 0 |

`KaStringAsyncTests` at 8 agrees exactly with the Literal Register's `[TestMethod]` baseline count of 8 for `KaStringAsyncTests.cs`, cross-checking the structural and behavioural baselines against each other. The filter substring `KbdActionsTests` does not match `KbdActionsRemainingBranchesTests`, so the two rows are disjoint.

## Individual baseline outcomes for the eight `KaStringAsyncTests` methods

All eight passed:

```
Passed Constructor_LowercasesKeyAndStoresMembers
Passed KeySetter_LowercasesValue
Passed Delegate_AwaitsAndCompletesSynchronously
Passed KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue
Passed KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate
Passed KeyEquals_SingleCharNonMatchWhileActivated_InvokesToggleControlAndReturnsFalse
Passed KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse
Passed KeyEquals_NullDelegatesAreToleratedInNonMatchBranches
```

`KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse` is the test P1-T1 renames; it passes at baseline and must continue to pass under its new name, unmodified in body.

Output Summary: `Test Run Successful.` with EXIT_CODE 0. Repository-wide totals are Passed 6437, Failed 0, Skipped 0 across all 9 test assemblies. The pre-existing-failure set is therefore EMPTY, which makes the P4-T5 and P5-T6 subset gates equivalent to an absolute zero-failure requirement. The QuickFiler.Test assembly alone reports 903 Passed, 0 Failed. Per-class baselines for the five pinned classes are `KaStringAsyncTests` 8, `KaCharTests` 10, `KaKeyTests` 9, `KbdActionsTests` 3, `KbdActionsRemainingBranchesTests` 10, all fully passing. The two load-flaky pump tests owned by siblings #511/#571 both passed in this capture and are consequently NOT available as pre-existing-failure exemptions later.
