# Phase 1 — Pre-Fix Red Run (Issue #445) [expect-fail]

Timestamp: 2026-08-22T09-38

Command:
```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation '/TestCaseFilter:FullyQualifiedName~KaStringAsyncTests'
```
Run from `WS` via `pwsh -NoProfile`. `/InIsolation` present per Non-negotiable Command Constraint 3.

EXIT_CODE: 1
ExpectedExitCode: 1

A failing run is the expected and required outcome of this task. The four new tests and the one rename were authored against **unmodified production code** (P1-T8 built the solution with no production edit applied; Phase 2 had not yet begun), per CLAUDE.md Bugfix Workflow section 1.

## Totals

```
Total tests: 12
     Passed: 9
     Failed: 3
Test Run Failed.
 Total time: 1.4957 Seconds
```

12 total confirms the `[TestMethod]` count of 12 measured in P1-T6 (8 baseline plus 4 new).

## Per-test outcome, all 12 methods

| # | Test method | Outcome | Note |
|---|---|---|---|
| 1 | `Constructor_LowercasesKeyAndStoresMembers` | Passed | pre-existing |
| 2 | `KeySetter_LowercasesValue` | Passed | pre-existing |
| 3 | `Delegate_AwaitsAndCompletesSynchronously` | Passed | pre-existing |
| 4 | `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` | Passed | pre-existing; the AC3 anti-regression witness |
| 5 | `KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate` | Passed | pre-existing |
| 6 | `KeyEquals_SingleCharNonMatchWhileActivated_InvokesToggleControlAndReturnsFalse` | Passed | pre-existing |
| 7 | `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse` | Passed | **the P1-T1 rename**; body unchanged, passes under the new name before the fix as predicted |
| 8 | `KeyEquals_NullDelegatesAreToleratedInNonMatchBranches` | Passed | pre-existing |
| 9 | `KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse` | **Failed** | new test (a) — defect 1 |
| 10 | `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` | Passed | new test (b) — passes before and after by design |
| 11 | `KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther` | **Failed** | new test (c) — defect 2, empty probe |
| 12 | `KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther` | **Failed** | new test (d) — defect 2, null probe |

## Failure 1 — test (a), defect 1 (ungated branch 3)

`KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse` [121 ms]

```
Expected updates to be empty because no KeyEquals side effect may fire while Activated is false,
but found at least one item {"a"}.
   at QuickFiler.Controllers.Tests.KaStringAsyncTests.KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse()
   in ...\QuickFiler.Test\Controllers\KaStringAsyncTests.cs:line 186
```

The captured item is the literal string `"a"`. That is `Key.Substring(0, 1)` for `Key = "abc"`, which is precisely the argument the ungated branch-3 `Update` call passes. The defect is therefore reproduced directly, not inferred: `Update` fired while `Activated` was `false`.

## Failure 2 — test (c), defect 2 (empty probe)

`KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther` [3 ms]

```
Expected System.ArgumentException because an empty probe would otherwise match every registered
action, but no exception was thrown.
   at ...KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther() in ...KaStringAsyncTests.cs:line 234
```

The failure is on **variant 1** (default instance: `Activated` false, `Update` null). Line 234 is variant 1's assertion. "No exception was thrown" confirms the silent "empty matches everything" semantics the spec identifies as the reachable production misbehaviour: `Key.Contains(string.Empty)` is `true` for every key, so control entered branch 1 and returned `true`.

Because the test short-circuits at variant 1's assertion, **variant 2's pre-fix `ArgumentOutOfRangeException` was not independently observed in this run.** Its existence is established by the spec's and research artifact's reading of the offset arithmetic (`other.Length - 1 == -1` feeding `Key.Substring(-1, 1)`), not by a captured runtime failure here. That is a limitation of this evidence record and is stated rather than glossed. Both variants must pass at P2-T6, which is what closes AC6.

## Failure 3 — test (d), defect 2 (null probe) — parameter name recorded verbatim

`KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther` [2 ms]

```
Expected exception with parameter name "other", but found "value".
   at FluentAssertions.ExceptionAssertionsExtensions.WithParameterName[TException](...)
   at ...KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther() in ...KaStringAsyncTests.cs:line 274
```

**Observed pre-fix exception parameter name: `value`** (recorded verbatim, as the task requires).
**Observed pre-fix exception type: `ArgumentNullException`** — the `ThrowExactly<ArgumentNullException>` type assertion PASSED and the failure came only from `WithParameterName`, whose stack frame is `ExceptionAssertionsExtensions.WithParameterName`.

This confirms research section 4.3 exactly: today the throw originates inside `string.Contains`, whose parameter is named `value`, so the explicit guard changes the exception's **origin**, not its **type**. A type-only assertion (`ThrowExactly<ArgumentNullException>` alone) would have passed unchanged before the fix and would have gated nothing. The parameter-name clause is the only red-before lever available for the null case, and it worked: the test is genuinely Failed, not Passed.

**Consequence for AC15:** the P6-T16 contingency does not apply. That task instructs leaving AC15 unchecked only "if the P1-T9 artifact records the null-probe test as Passed before the fix rather than Failed". It is recorded here as **Failed**, so AC15 has a genuine red-before witness for both the empty and the null case.

## Fail-before dossier

No fail-before exception dossier is required. Real failing runs are available and are recorded above for all three intended tests. Nothing was deleted and no pre-existing test was weakened: `issue.md`'s claim that #430 left characterization tests asserting these defects is false, as the research artifact established and as this run corroborates — all eight pre-existing methods passed unchanged.

Output Summary: `Test Run Failed.` with EXIT_CODE 1, matching ExpectedExitCode 1. Totals are Total 12, Passed 9, Failed 3. All three failures are the intended new regression tests and no pre-existing test failed. Test (a) failed with the captured `Update` argument `{"a"}`, directly reproducing the ungated branch-3 invocation while `Activated` was false. Test (c) failed on variant 1 with "no exception was thrown", reproducing the silent empty-probe match; variant 2's pre-fix `ArgumentOutOfRangeException` was not independently observed because the test short-circuited at variant 1, and that limitation is stated rather than implied. Test (d) failed with `Expected exception with parameter name "other", but found "value"` — the pre-fix parameter name is recorded verbatim as `value` and the pre-fix type was already `ArgumentNullException`, so the parameter-name clause is what made this test red. The renamed test (#7) passed unmodified under its new name, as predicted. Test (b) passed before the fix by design.
