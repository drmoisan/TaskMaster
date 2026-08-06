# Decisive Verification, Part 1 — Standalone `SVGControl.Test` Run After the Fix

- Task: `[P1-T5]`
- Timestamp: 2026-08-04T23-58
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Before-half: `evidence/remediation-baseline/order-standalone.2026-08-05T05-00.md` (`[P0-T7]`)

## Command

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' SVGControl.Test\bin\Debug\SVGControl.Test.dll
```

Run from the repository root with the **identical switch set `[P0-T7]` used**: no `/EnableCodeCoverage`,
no `/InIsolation`, no `/Settings`. Switch parity is what makes the before/after comparison meaningful; a
different switch set could change probing behavior and would invalidate the comparison.

```
EXIT_CODE: 0
```

## Result counts

```
Test Run Successful.
Total tests: 75
     Passed: 75
 Total time: 1.2967 Seconds
```

| Figure | Required by `[P1-T5]` | Measured | Verdict |
|---|---|---|---|
| Total | 75 | **75** | met |
| Passed | 75 | **75** | met |
| Failed | 0 | **0** | met |

The failed count was measured independently rather than inferred from the absence of a `Failed:` summary
line:

```
Command: grep -cE '^\s+Failed ' <run output>
Output:  0
```

**75 total, 75 passed, 0 failed. Acceptance satisfied.** No halt condition fires.

## The six formerly failing tests, each with its individual passing outcome

Each of the six tests named by `[P1-T5]` — the same six that failed in `[P0-T7]` — was located
individually in the run output and its outcome recorded:

| # | Test | `[P0-T7]` outcome | `[P1-T5]` outcome | Line as read |
|---|---|---|---|---|
| 1 | `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull` | Failed | **Passed** | `Passed SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull [289 ms]` |
| 2 | `GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument` | Failed | **Passed** | `Passed GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument [< 1 ms]` |
| 3 | `Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull` | Failed | **Passed** | `Passed Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull [< 1 ms]` |
| 4 | `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException` | Failed | **Passed** | `Passed TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException [< 1 ms]` |
| 5 | `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner` | Failed | **Passed** | `Passed GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner [< 1 ms]` |
| 6 | `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument` | Failed | **Passed** | `Passed GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument [< 1 ms]` |

All six pass. None was skipped, retargeted, or renamed.

### The two `XmlException` assertions now hold unconditionally

Tests 4 and 5 are the two the binding `## Do Not Do` list specifically protects. In `[P0-T7]` test 4
failed with:

```
Expected type to be System.Xml.XmlException because an empty payload has no root element, so the XML
reader raises rather than returning null, but found System.IO.FileNotFoundException.
```

It now passes **with its assertion unchanged**. No assertion was weakened, retargeted, or deleted, no
`app.config` was edited, and no second reference was added. The assertion was correct all along; making
`ExCSS` resolvable is what allows the parse to reach the XML reader, where the empty payload raises
`XmlException` as the test asserts. This is precisely the outcome
`remediation-inputs.2026-08-04T22-28.md` predicted: "those assertions are correct and R-7 is what makes
them hold unconditionally."

## No residual `ExCSS` bind failure anywhere in the run

```
Command: grep -ci 'excss' <run output>
Output:  0
```

**Zero occurrences of the string `ExCSS` in any casing.** In `[P0-T7]` the same run emitted the identity
`ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a` in six failure messages plus
`SvgRenderer load 'ExCSS'` on the `Trace` channel. All of it is gone: the assembly now resolves from the
output directory, so the `AssemblyResolve` fallback is never reached and no bind diagnostic is emitted.

```
Command: grep -ci 'FileNotFoundException' <run output>
Output:  1
```

The single occurrence is **not** an error. It is the display name of an unrelated passing data-driven
case:

```
  Passed GetExceptionForWin32Error_ReturnsSpecificExceptionTypes (2,"missing.svg",System.IO.FileNotFoundException) [6 ms]
```

The type name appears because it is a `[DataRow]` argument. Recorded here so a reaudit grepping for the
token does not misread it as a surviving failure.

## Before / after comparison

| Measurement | `[P0-T7]` (before) | `[P1-T5]` (after) | Delta |
|---|---|---|---|
| `EXIT_CODE` | 1 | **0** | fixed |
| Total | 75 | 75 | unchanged |
| Passed | 69 | **75** | **+6** |
| **Failed** | **6** | **0** | **−6** |
| `ExCSS` occurrences in output | present in 6 failures + Trace | **0** | eliminated |

The total is unchanged at 75, which confirms no test was added, removed, or filtered out — the +6 passed
is exactly the six formerly failing tests turning green, with nothing else changing.

## Forbidden responses — none was needed and none was taken

`[P1-T5]` names three forbidden responses to a failure. Recorded for the audit trail: the run passed on
the first attempt, so no remedial action of any kind was taken. Specifically, **no second reference was
added** (no `Fizzler`, per Design Decision 3 — `Fizzler.dll` remains absent from the output as `[P1-T4]`
confirms), **no `app.config` was edited** (forbidden by the `## Do Not Do` list), and **no assertion was
weakened** (all 75 tests ran with their assertions exactly as authored).

## Output Summary

`EXIT_CODE: 0` with **75 total, 75 passed, 0 failed**, satisfying `[P1-T5]`'s acceptance exactly. Each of
the six previously failing tests was located individually and each passed. The two protected
`XmlException` assertions now hold with unchanged text. Zero occurrences of `ExCSS` remain in the run
output, down from six failure messages plus a `Trace` warning in `[P0-T7]`. Running `SVGControl.Test`
alone — the ordering a developer's Test Explorer session uses — is now green, which is the condition
`remediation-inputs.2026-08-04T22-28.md` § Exit Criteria requires to close G-8 and restore AC-10 to PASS.
