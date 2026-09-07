# Phase 2 — Red run ([P2-T3], expect-fail)

Timestamp: 2026-09-01T22-49

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`

EXIT_CODE: 1
ExpectedExitCode: 1

A failing run is the expected and required outcome of this task. The three named tests below assert the
corrected behaviour against the Phase 1 behaviour-preserving seam, which still carries the defect, so they
fail by assertion. The wrapper throws at `scripts/vscode/Invoke-MSTest.ps1` line 130 on a non-zero inner
exit, which is why the exit code is 1:

```
Test Run Failed.
Exception: <repo-root>\scripts\vscode\Invoke-MSTest.ps1:130:5
 130 |      throw "MSTest execution failed with exit code $LASTEXITCODE"
      | MSTest execution failed with exit code 1
```

## Discovered assemblies

```
Discovered 9 test assemblies.
```

## Runner summary block, transcribed verbatim

```
Total tests: 6934
     Passed: 6931
     Failed: 3
 Total time: 28.2743 Seconds
```

`Test Run Failed.` was written to the standard error stream.

## Acceptance reading 1 — the total-count arithmetic

`Total tests:` = **6934**.

`[P0-T12]` baseline total = 6927. Baseline plus seven = 6927 + 7 = **6934**. The figures are equal, so the
seven new methods were all discovered and all executed.

## Acceptance reading 2 — the failing-test list

Verbatim failing-test lines from the run, each recorded with the declaring type read from its stack
trace:

| Failing test name | Declaring type from stack trace | Declaration site from stack trace |
|---|---|---|
| `ClaimsAltChord_WithAltM_ReturnsFalse` | `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` | `QuickFiler.Test\Controllers\QfcFormKeyHandlerTests.cs:line 122` |
| `ClaimsAltChord_WithAltF4_ReturnsFalse` | `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` | `QuickFiler.Test\Controllers\QfcFormKeyHandlerTests.cs:line 141` |
| `ClaimsAltChord_WithAltLeft_ReturnsFalse` | `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` | `QuickFiler.Test\Controllers\QfcFormKeyHandlerTests.cs:line 158` |

The declaring type is recorded because `QuickFiler.Test` already declares
`ClaimsAltChord_WithAltM_ReturnsFalse` at QuickFiler.Test/Controllers/EfcViewerTests.cs:134 and
`ClaimsAltChord_WithNullHandler_ReturnsFalse` at QuickFiler.Test/Controllers/EfcViewerTests.cs:156, so two
of the seven new method names are not unique within the assembly. All three failures recorded here belong
to `QfcFormKeyHandlerTests`, not to the Email Filer fixture.

The failing list contains **exactly** those three names and **no other name**. BASELINE_FAILURE_SET from
`[P0-T12]` is the empty set, so the "in addition to a subset of BASELINE_FAILURE_SET" allowance
contributes nothing and the failing list is exactly the three expected reds.

## Failure detail, verbatim, with the worktree root rendered as `<repo-root>`

```
  Failed ClaimsAltChord_WithAltM_ReturnsFalse [73 ms]
  Error Message:
   Expected result to be False because Alt+M is the Move Options mnemonic on the hosted item viewers and must reach the base implementation, but found True.
  Stack Trace:
     at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message) in /_/Src/FluentAssertions/Execution/LateBoundTestFramework.cs:line 22
   at FluentAssertions.Execution.AssertionChain.FailWith(Func`1 getFailureReason) in /_/Src/FluentAssertions/Execution/AssertionChain.cs:line 277
   at FluentAssertions.Primitives.BooleanAssertions`1.BeFalse(String because, Object[] becauseArgs) in /_/Src/FluentAssertions/Primitives/BooleanAssertions.cs:line 62
   at QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests.ClaimsAltChord_WithAltM_ReturnsFalse() in <repo-root>\QuickFiler.Test\Controllers\QfcFormKeyHandlerTests.cs:line 122

  Failed ClaimsAltChord_WithAltF4_ReturnsFalse [1 ms]
  Error Message:
   Expected result to be False because Alt+F4 is the standard window-close chord and must not be consumed here, but found True.
  Stack Trace:
     ...
   at QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests.ClaimsAltChord_WithAltF4_ReturnsFalse() in <repo-root>\QuickFiler.Test\Controllers\QfcFormKeyHandlerTests.cs:line 141

  Failed ClaimsAltChord_WithAltLeft_ReturnsFalse [< 1 ms]
  Error Message:
   Expected result to be False because Alt+arrow is vestigial on this surface and must fall through unclaimed, but found True.
  Stack Trace:
     ...
   at QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests.ClaimsAltChord_WithAltLeft_ReturnsFalse() in <repo-root>\QuickFiler.Test\Controllers\QfcFormKeyHandlerTests.cs:line 158
```

The three FluentAssertions frames above the declaring-type frame are identical in all three stack traces
and are elided with `...` after the first, which is quoted in full. Every failure is `but found True`
against an expected `False`: the intermediate seam claims every Alt-bearing chord, which is the defect
this fix removes.

## The other four new tests

The run reports `Passed ClaimsAltChord_WithoutAltFlag_ReturnsFalse` and
`Passed ClaimsAltChord_WithNullHandler_ReturnsFalse` in the same block, and neither
`ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` nor
`ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` appears in the failing list. Those four pass in
both Phase 2 and Phase 3, exactly as the plan's reading guide states.

Output Summary: The red run failed as required, with exit code 1 against an expected exit code of 1. The
`Total tests:` figure is 6934, which equals the `[P0-T12]` baseline of 6927 plus seven. The failing list
contains exactly `ClaimsAltChord_WithAltM_ReturnsFalse`, `ClaimsAltChord_WithAltF4_ReturnsFalse` and
`ClaimsAltChord_WithAltLeft_ReturnsFalse`, all three declared on
`QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests`, and no other name. This is a genuine runtime red
produced by assertion failures rather than by a build break, so no fail-before exception dossier is
required.
