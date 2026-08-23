# Phase 4 — QuickFiler Test Suite: No Pre-Existing Test Deleted or Weakened (Issue #445, AC16 through AC19)

Timestamp: 2026-08-22T09-50

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook'
```
plus one `/TestCaseFilter:FullyQualifiedName~<ClassName>` run per pinned class, and `git status --porcelain` per test file. Run from `WS` via `pwsh -NoProfile`.

EXIT_CODE: 0

## Assembly-wide result

```
Test Run Successful.
Total tests: 907
     Passed: 907
```

| Measurement | Value |
|---|---|
| Total tests | 907 |
| **Passed** | **907** |
| **Failed** | **0** |
| **Skipped** | **0** |
| Failing-test-name set | **empty** |
| Lines matching `^\s+Failed\s` in the run output | 0 |

Baseline (P0-T15) for this assembly was 903 Passed / 0 Failed. The count rose by exactly 4, which is the four new tests added by P1-T2 through P1-T5. No test was lost: 903 + 4 = 907.

## Gate evaluation

The gate passes when the failing-test-name set is a subset of the `QuickFiler.Test` portion of the P0-T15 baseline failing set **and** contains no test in `KaStringAsyncTests`, `KaCharTests`, `KaKeyTests`, `KbdActionsTests`, or `KbdActionsRemainingBranchesTests`.

- The P0-T15 baseline failing set is **empty** (6437 Passed, 0 Failed repository-wide).
- The observed failing set here is **empty**.
- The empty set is a subset of the empty set, so the subset condition holds.
- The empty set contains no test in any of the five pinned classes, so the second condition holds.

**Gate: PASS.** Because the baseline failing set is empty, this gate was equivalent to an absolute zero-failure requirement, which is the strictest form it can take. No pre-existing failure was available as an exemption.

## Surviving pre-existing failures

**None.** There are no surviving pre-existing failures to list. In particular the two pump tests named in the plan as owned by wave-0 siblings #511/#571 — `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` and `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` — both passed here, as they did in the P0-T15 baseline. They are load-flaky rather than deterministically red and machine load was low for both captures. Neither is claimed as an exemption by this artifact.

## Per-class Passed counts against the P0-T15 baseline

| Test class | Baseline Total/Passed | Now Total/Passed | Failed | Exit | Verdict |
|---|---|---|---|---|---|
| `KaStringAsyncTests` | 8 / 8 | 12 / 12 | 0 | 0 | +4 by design (P1-T2 through P1-T5) |
| `KaCharTests` | 10 / 10 | **10 / 10** | 0 | 0 | **identical to baseline** |
| `KaKeyTests` | 9 / 9 | **9 / 9** | 0 | 0 | **identical to baseline** |
| `KbdActionsTests` | 3 / 3 | **3 / 3** | 0 | 0 | **identical to baseline** |
| `KbdActionsRemainingBranchesTests` | 10 / 10 | **10 / 10** | 0 | 0 | **identical to baseline** |

The four classes the task requires to be unchanged each report exactly their baseline Passed count. `KaStringAsyncTests` is the one class this plan legitimately grows, from 8 to 12, and all 12 pass.

## `git status --porcelain` per test file

| Test file | Status lines | Expected |
|---|---|---|
| `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 1 | modified: the AC12 rename plus the four AC13-AC15 tests |
| `QuickFiler.Test/Controllers/KaCharTests.cs` | **0** | unmodified |
| `QuickFiler.Test/Controllers/KaKeyTests.cs` | **0** | unmodified |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | **0** | unmodified |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | **0** | unmodified |

The four files the task requires to show no modification each report 0 status lines. Only `KaStringAsyncTests.cs` is modified, which is the single file the plan authorises to change.

## Why `KaCharTests` and `KaKeyTests` survive the member deletions unchanged

Phase 3 deleted `DelegateType` from `KaChar` and `KaKey` and `Update` from four types. Those tests still pass with no edit because they never referenced the deleted members. Verified by search rather than assumed:

- `DelegateType` across `QuickFiler.Test/**/*.cs`: **0 files match.**
- `Update` in `KaCharTests.cs` and `KaKeyTests.cs`: **0 hits.**

This corroborates the spec's zero-read-site claim from the test side as well as the production side, and it is why the six member deletions required no new test: their safety rests on the zero-read-site evidence plus the clean compile at P3-T11.

## AC19 out-of-scope retention counts (P4-T4, restated here for the AC19 citation)

| Token | File | Baseline | Now | Required |
|---|---|---|---|---|
| `Key.Substring(other.Length - 1, 1)` | `KaStringAsync.cs` | 1 | **1** | 1 |
| `Key.Contains(other)` | `KaStringAsync.cs` | 1 | **1** | 1 |
| `Be("b"` | `KaStringAsyncTests.cs` | 1 | **1** | 1 |

All three equal their P0-T19 baseline. The out-of-scope fourth defect was not fixed, branch 1's substring semantics are unchanged, and the pre-existing `.Be("b")` assertion at the prefix case is untouched.

Output Summary: `Test Run Successful.` with EXIT_CODE 0 on the `QuickFiler.Test` assembly: Total 907, Passed 907, Failed 0, Skipped 0, and an empty failing-test-name set. Against the 903-Passed baseline the count rose by exactly the 4 new tests, so no test was lost. The gate passes: the empty observed failing set is a subset of the empty P0-T15 baseline failing set and contains no test in any of the five pinned classes, which made this gate an absolute zero-failure requirement rather than a subset allowance. There are no surviving pre-existing failures to list; the two load-flaky pump tests owned by siblings #511/#571 both passed and are not claimed as exemptions. The four classes required to be unchanged report exactly their baseline Passed counts (`KaCharTests` 10, `KaKeyTests` 9, `KbdActionsTests` 3, `KbdActionsRemainingBranchesTests` 10) and their four source files each report 0 `git status --porcelain` lines. Searches confirm no test anywhere in `QuickFiler.Test` referenced `DelegateType`, and neither `KaCharTests.cs` nor `KaKeyTests.cs` referenced `Update`, which is why the member deletions required no test change. The three AC19 retention counts all hold at 1.
