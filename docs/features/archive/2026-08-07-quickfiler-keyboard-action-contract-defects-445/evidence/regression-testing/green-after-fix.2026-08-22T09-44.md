# Phase 2 — Post-Fix Green Run (Issue #445)

Timestamp: 2026-08-22T09-44

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation '/TestCaseFilter:FullyQualifiedName~KaStringAsyncTests'
```
Run from `WS` via `pwsh -NoProfile`. Same scope and same filter as the P1-T9 red run, so the two are directly comparable.

EXIT_CODE: 0

Build step: `Build succeeded.` with 5 warnings (the pre-existing third-party System.Reactive advisories) and 0 errors, exit code 0. `QuickFiler.Test.dll` timestamp `2026-08-22T09:42:31` postdates the last source edit, so the assembly under test carries the Phase 2 production change.

## Totals

```
Total tests: 12
     Passed: 12
Test Run Successful.
 Total time: 1.2385 Seconds
```

**Passed 12, Failed 0, Skipped 0.**

## Per-test outcome, all 12 methods

| # | Test method | Red run (P1-T9) | Green run | Status |
|---|---|---|---|---|
| 1 | `Constructor_LowercasesKeyAndStoresMembers` | Passed | **Passed** | unchanged |
| 2 | `KeySetter_LowercasesValue` | Passed | **Passed** | unchanged |
| 3 | `Delegate_AwaitsAndCompletesSynchronously` | Passed | **Passed** | unchanged |
| 4 | `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` | Passed | **Passed** | AC3 witness, unmodified |
| 5 | `KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate` | Passed | **Passed** | unchanged |
| 6 | `KeyEquals_SingleCharNonMatchWhileActivated_InvokesToggleControlAndReturnsFalse` | Passed | **Passed** | unchanged |
| 7 | `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse` | Passed | **Passed** | the P1-T1 rename, body unchanged |
| 8 | `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` | Passed | **Passed** | unchanged |
| 9 | `KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse` | **Failed** | **Passed** | red-to-green |
| 10 | `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` | Passed | **Passed** | pins existing behaviour |
| 11 | `KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther` | **Failed** | **Passed** | red-to-green |
| 12 | `KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther` | **Failed** | **Passed** | red-to-green |

## The four new tests and the renamed test, individually listed as Passed

- `KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse` — **Passed** [3 ms] (new test (a))
- `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` — **Passed** [8 ms] (new test (b))
- `KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther` — **Passed** [3 ms] (new test (c))
- `KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther` — **Passed** [< 1 ms] (new test (d))
- `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse` — **Passed** [< 1 ms] (the rename)

## Red-to-green transitions, and what each proves

**Test (a), defect 1.** Red with "found at least one item {"a"}"; now green. The branch-3 guard acquired the `Activated &&` conjunct, so the ungated `Update(Key.Substring(0, 1))` no longer fires while `Activated` is `false`. AC1 and AC13 are established by this pair.

**Test (c), defect 2, empty probe.** Red with "no exception was thrown" on variant 1; now green on **both** variants. Variant 1 (default instance, `Activated` false, `Update` null) previously returned `true` silently and now throws `ArgumentException` naming `other`. Variant 2 (`Activated = true`, non-null `Update`) previously threw `ArgumentOutOfRangeException` from the negative substring offset and now throws `ArgumentException` naming `other`. Because the assertion is `ThrowExactly<ArgumentException>`, and `ArgumentOutOfRangeException` derives from `ArgumentException`, variant 2 passing proves the exception is now exactly `ArgumentException` and no longer the derived out-of-range type. That is what closes AC6: the guard clause runs before any offset arithmetic, so the negative start index is unreachable.

**Test (d), defect 2, null probe.** Red with `Expected exception with parameter name "other", but found "value"`; now green. The throw has moved from inside `string.Contains` (parameter `value`) to the explicit guard clause (parameter `other`), which is exactly the change research section 4.3 predicted. The exception type was `ArgumentNullException` both before and after; the parameter name is the discriminator, and it is now correct. AC4 and AC15 are established by this pair.

## Anti-regression corroboration

Tests 4 and 10 both passed. Test 4 asserts `ka.Activated.Should().BeTrue()` after a matching probe, and test 10 asserts the captured `Update` sequence `"b"` then `"a"` across a match-then-non-match transition. Both would fail if branch 1's early return had been removed. Their passing, combined with the P2-T4 structural counts (`return true;` = 1, `Activated = false` = 1), establishes AC3 from two independent directions.

No pre-existing test changed outcome and no pre-existing test was modified: all eight passed in the baseline, in the red run, and in this green run.

Output Summary: `Test Run Successful.` with EXIT_CODE 0. Totals are Total 12, Passed 12, Failed 0, Skipped 0. All three previously failing tests transitioned red to green: test (a) (defect-1 branch-3 gating), test (c) (empty probe, now passing on both instance-state variants, which closes the `ArgumentOutOfRangeException` path because `ThrowExactly<ArgumentException>` rejects the derived type), and test (d) (null probe, whose thrown parameter name moved from `value` to `other`). The four new tests and the renamed test are each individually listed as Passed above. All eight pre-existing tests passed unmodified in the baseline, the red run, and this run, including the AC3 anti-regression witness `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue`. The preceding rebuild succeeded with 0 errors and only the 5 pre-existing third-party warnings.
