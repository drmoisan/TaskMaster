# Order Independence Re-Confirmed Inside the Final Clean Pass

- Task: `[P2-T9]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-24

## Why this task exists

A nine-assembly wrapper run passes **with or without** this fix and is therefore not evidence of it. That
is exactly how the defect survived two audits, and it was re-confirmed empirically this cycle: `[P2-T8]`
measured every `SVGControl` coverage figure as byte-identical before and after the fix, because the
nine-assembly ordering already supplied `ExCSS` to the test host from a sibling assembly's output
directory. The two runs below are what actually discriminate.

Both runs use the same switch set `[P0-T7]` and `[P0-T8]` used: **no** `/EnableCodeCoverage`, **no**
`/InIsolation`, **no** `/Settings`. Switch parity across before and after is what makes the comparison
meaningful.

## Run 1 — standalone

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' SVGControl.Test\bin\Debug\SVGControl.Test.dll
```

```
EXIT_CODE: 0
```

```
Test Run Successful.
Total tests: 75
     Passed: 75
 Total time: 1.4099 Seconds
```

Independently measured failed count: `grep -cE '^\s+Failed '` = **0**.

**75 total, 75 passed, 0 failed** — the figures `[P2-T9]` requires.

## Run 2 — `SVGControl.Test.dll` first, sibling second

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' SVGControl.Test\bin\Debug\SVGControl.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

```
EXIT_CODE: 0
```

```
Test Run Successful.
Total tests: 76
     Passed: 76
 Total time: 2.0941 Seconds
```

Independently measured failed count: `grep -cE '^\s+Failed '` = **0**.

**76 total, 76 passed, 0 failed.** This is the ordering `[P0-T8]` Run A measured at 6 failed.

## No residual `ExCSS` bind failure in either run

```
Command: grep -ci 'excss' <each run output>
Run 1: 0
Run 2: 0
```

Zero occurrences in any casing in either run.

## Before / after table — the closure of G-8 and CR-8 in one place

| Run shape | Before (Phase 0) | After (`[P2-T9]`) | Failed delta |
|---|---|---|---|
| **Standalone** `SVGControl.Test.dll` | `EXIT_CODE: 1` — 75 total, 69 passed, **6 failed** (`[P0-T7]`) | `EXIT_CODE: 0` — **75 total, 75 passed, 0 failed** | **−6** |
| **`SVGControl.Test` first**, `VBFunctions.Test` second | `EXIT_CODE: 1` — 76 total, 70 passed, **6 failed** (`[P0-T8]` Run A) | `EXIT_CODE: 0` — **76 total, 76 passed, 0 failed** | **−6** |
| `VBFunctions.Test` first, `SVGControl.Test` second | `EXIT_CODE: 0` — 76 total, 76 passed, 0 failed (`[P0-T8]` Run B) | not re-run — it passed before the fix, so it cannot discriminate | 0 |
| Nine-assembly wrapper | 6150/6150, 0 failed | 6150/6150, 0 failed (`[P2-T7]`) | 0 — passes either way |

Before the fix the failed count depended on the ordinal position of `SVGControl.Test.dll` on the command
line: **6 when first, 0 when second**. After the fix, **every ordering yields 0 failed**, including the
single-assembly shape that Test Explorer uses. The outcome is invariant under assembly ordering.

## The six formerly order-sensitive tests

All six passed in both runs above. Individually confirmed at `[P1-T5]`
(`evidence/regression-testing/order-standalone-after.2026-08-05T05-00.md`), which located each by name in
the standalone run:

`SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`,
`GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`,
`Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull`,
`TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException`,
`GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`,
`GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument`.

Their assertions are unchanged. In particular the two `XmlException` assertions the binding
`## Do Not Do` list protects now hold with their original text, because making `ExCSS` resolvable lets the
parse reach the XML reader where an empty payload genuinely raises `XmlException`.

## What this closes

| Finding | Statement | Status |
|---|---|---|
| **G-8** (`policy-audit.2026-08-04T22-28.md`) | "Six tests in `SVGControl.Test` produce different outcomes depending on the ordinal position of the assembly on the `vstest.console.exe` command line" | **CLOSED** — every ordering now yields 0 failed |
| **CR-8** (the code review's single Blocking row) | same condition, remedy "Add an explicit `ExCSS` reference ... Verify that `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` alone returns 75/75" | **CLOSED** — the standalone run returns exactly 75/75 |
| **AC-10** stated objective | "so the test host can resolve ExCSS through the binding redirect rather than depending on the `AssemblyResolve` fallback to mask it" | **Achievable in the standalone host** — `ExCSS.dll` is now on the probing path; `[P2-T11]` records the evidence note |

The three policy statements the defect violated are each satisfied:

- `.claude/rules/general-unit-test.md` UT1 Independence — "Tests must be able to run in any order without
  impacting each other." Satisfied: 0 failed in every ordering measured.
- `.claude/rules/general-unit-test.md` § External Dependencies — "Tests must not rely on mutable global
  state or external configuration that can change between runs." Satisfied: `SVGControl.Test`'s own output
  directory supplies `ExCSS`, so the outcome no longer depends on which sibling ran first.
- `.claude/rules/csharp.md` § Deterministic Test Rules — "Tests must produce identical results in the IDE
  test runner and in CLI runs." Satisfied: the standalone run, which is the Test Explorer shape, agrees
  with every CLI ordering.

## Loop-restart determination

Failed is **0** in both runs, so the restart condition ("if failed is greater than zero in either run, the
loop restarts from `[P2-T1]`") does not fire. **No restart.**

## Output Summary

Both runs returned `EXIT_CODE: 0` with **failed equal to zero**: the standalone run at **75 total, 75
passed** and the `SVGControl.Test`-first pair at **76 total, 76 passed**. Zero `ExCSS` occurrences in
either output. Tabulated against the Phase 0 before-figures — standalone 75/69/**6** and
`SVGControl.Test`-first 76/70/**6** — the failed count falls by 6 in both shapes, while the previously
passing sibling-first ordering and the nine-assembly wrapper were already green and cannot discriminate.
Test outcomes are now invariant under assembly ordering, closing **G-8** and the code review's single
Blocking finding (**CR-8**) and making AC-10's stated objective achievable in the standalone host. No loop
restart is triggered; stage 4b of toolchain pass 1 is clean.
