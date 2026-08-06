# Decisive Verification, Part 2 — The Previously Failing Ordering, After the Fix

- Task: `[P1-T6]`
- Timestamp: 2026-08-05T00-01
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Before-half: `evidence/remediation-baseline/order-paired.2026-08-05T05-00.md` (`[P0-T8]`), **Run A**

## Command

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' SVGControl.Test\bin\Debug\SVGControl.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

Run from the repository root.

```
EXIT_CODE: 0
```

`A total of 2 test files matched the specified pattern.`

## This is the same ordering and the same sibling assembly `[P0-T8]` Run A used

Stated explicitly, as `[P1-T6]` requires, so the comparison is unambiguously like-for-like:

| Property | `[P0-T8]` Run A | `[P1-T6]` | Same? |
|---|---|---|---|
| First argument | `SVGControl.Test\bin\Debug\SVGControl.Test.dll` | `SVGControl.Test\bin\Debug\SVGControl.Test.dll` | **yes** |
| Second argument | `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll` | `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll` | **yes** |
| Sibling assembly | `VBFunctions.Test` | `VBFunctions.Test` | **yes** |
| `/EnableCodeCoverage` | not passed | not passed | **yes** |
| `/InIsolation` | not passed | not passed | **yes** |
| `/Settings` | not passed | not passed | **yes** |
| Runner | `vstest.console.exe` at the `[P0-T6]`-resolved path | same path | **yes** |

`SVGControl.Test.dll` is **first** in both, which is the ordering `[P0-T8]` measured at **6 failed**.
This is deliberately the failing ordering rather than the passing one: `[P0-T8]` Run B (sibling first)
already passed before the fix, so re-running Run B would prove nothing. Only the Run A ordering
discriminates.

## Result counts

```
Test Run Successful.
Total tests: 76
     Passed: 76
 Total time: 1.5772 Seconds
```

| Figure | Required by `[P1-T6]` | Measured |
|---|---|---|
| Total | (not fixed) | **76** |
| Passed | (not fixed) | **76** |
| **Failed** | **0** | **0** |

Independently measured failed count rather than inferred:

```
Command: grep -cE '^\s+Failed ' <run output>
Output:  0
```

**Failed equals zero. Acceptance satisfied.** No halt condition fires.

## No residual `ExCSS` bind failure

```
Command: grep -ci 'excss' <run output>
Output:  0
```

Zero occurrences in any casing, where `[P0-T8]` Run A emitted the identity in six failure messages.

## Before / after comparison — the order dependence is closed

| Run | Ordering | `EXIT_CODE` | Total | Passed | Failed |
|---|---|---|---|---|---|
| `[P0-T8]` Run A (before) | `SVGControl.Test` **first** | 1 | 76 | 70 | **6** |
| `[P0-T8]` Run B (before) | sibling first | 0 | 76 | 76 | 0 |
| **`[P1-T6]` (after)** | `SVGControl.Test` **first** | **0** | **76** | **76** | **0** |
| `[P1-T5]` (after) | standalone, one assembly | 0 | 75 | 75 | 0 |

Before the fix, the failed count depended on argument order: 6 with `SVGControl.Test` first, 0 with the
sibling first. After the fix, **both orderings yield 0 failed**, and the standalone single-assembly run
yields 0 as well. The outcome is now invariant under assembly ordering.

That invariance is the substance of the three policy statements the defect violated:

- `.claude/rules/general-unit-test.md` UT1: "Tests must be able to run in any order without impacting
  each other." **Satisfied** — the same 76 tests now pass in the ordering that previously failed.
- `.claude/rules/general-unit-test.md` § External Dependencies: "Tests must not rely on mutable global
  state or external configuration that can change between runs." **Satisfied** — the outcome no longer
  depends on which assembly's output directory happens to supply `ExCSS` to the test host, because
  `SVGControl.Test`'s own output now supplies it.
- `.claude/rules/csharp.md` § Deterministic Test Rules: "Tests must produce identical results in the IDE
  test runner and in CLI runs so local and CI behavior agree." **Satisfied** — the standalone run
  `[P1-T5]`, which is the shape Test Explorer uses, agrees with every CLI ordering at 0 failed.

## Output Summary

`EXIT_CODE: 0` with **76 total, 76 passed, 0 failed** in the exact ordering — `SVGControl.Test.dll`
first, `VBFunctions.Test.dll` second — that `[P0-T8]` Run A measured at 6 failed, using the identical
switch set and the identical sibling assembly. Zero occurrences of `ExCSS` remain in the output. Combined
with `[P1-T5]`'s standalone 75/75, the outcome is now invariant under assembly ordering, closing the
order dependence recorded as G-8 and as the code review's single Blocking finding. `[P2-T9]` re-confirms
both runs inside the final clean toolchain pass.
