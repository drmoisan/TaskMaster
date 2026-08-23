# Pre-Change Two-Assembly Order Asymmetry — `[expect-fail]` Before-Half

- Task: `[P0-T8]` `[expect-fail]`
- Timestamp: 2026-08-04T23-37
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Nothing was built and nothing was changed before either run.

## `[expect-fail]` declaration

**A non-zero exit code for Run A is the expected measurement outcome of this task, not a task failure.**
This pair is the before-half of the order-dependence proof: the two runs execute the same binaries and
differ only in the ordinal position of `SVGControl.Test.dll` on the command line.

## Run A — `SVGControl.Test.dll` first

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' SVGControl.Test\bin\Debug\SVGControl.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

```
EXIT_CODE: 1
```

```
Test Run Failed.
Total tests: 76
     Passed: 70
     Failed: 6
 Total time: 1.3710 Seconds
```

Failed tests, extracted with `grep -E '^\s+Failed '`:

```
  Failed SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull [62 ms]
  Failed GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument [< 1 ms]
  Failed Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull [< 1 ms]
  Failed TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException [1 ms]
  Failed GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner [1 ms]
  Failed GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument [2 ms]
```

The same six tests as `[P0-T7]`, with the same root cause.

## Run B — `VBFunctions.Test.dll` first

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' VBFunctions.Test\bin\Debug\VBFunctions.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll
```

```
EXIT_CODE: 0
```

```
Test Run Successful.
Total tests: 76
     Passed: 76
 Total time: 1.8001 Seconds
```

Failed-line count, measured with `grep -cE '^\s+Failed '`: **0**. Among the passing tests in this
ordering are `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument` — one of Run A's six
failures — which passed in `< 1 ms`.

## Measured comparison

| Run | Argument order | EXIT_CODE | Total | Passed | Failed | Plan expectation | Match |
|---|---|---|---|---|---|---|---|
| A | `SVGControl.Test` first, `VBFunctions.Test` second | 1 | **76** | **70** | **6** | 76/70/6 | yes |
| B | `VBFunctions.Test` first, `SVGControl.Test` second | 0 | **76** | **76** | **0** | 76/76/0 | yes |

**Both triples match the plan's expectations exactly.**

## The two runs executed the same binaries

Stated explicitly, and verified by measurement rather than asserted. Both runs named the identical two
file paths, in the same session, with no build, no restore, and no file write between them. Content and
timestamp identity:

```
Command: sha256sum SVGControl.Test/bin/Debug/SVGControl.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll
6691dc7ba68b8ac750507516fba91263a38867485dc5dd9fcd9b5172a0b1c99e *SVGControl.Test/bin/Debug/SVGControl.Test.dll
523a9248098ef9fea99e7317525c3a75664068b51eb4691a7debb0b513fa9160 *VBFunctions.Test/bin/Debug/VBFunctions.Test.dll

Command: ls -l --time-style=full-iso <same two paths>
-rwxr-xr-x 1 DanMoisan 197121 54272 2026-08-04 22:23:49.209201400 -0400 SVGControl.Test/bin/Debug/SVGControl.Test.dll
-rwxr-xr-x 1 DanMoisan 197121  6656 2026-08-04 22:24:59.801849100 -0400 VBFunctions.Test/bin/Debug/VBFunctions.Test.dll
```

Both modification timestamps precede this session's first command, so neither assembly was rewritten
between Run A and Run B.

## The failed counts differ, which is the order-dependence this cycle closes

**Run A failed 6; Run B failed 0.** Same two binaries, same host, same session, same switch set — the
only variable is the ordinal position of `SVGControl.Test.dll` on the command line. Six tests therefore
pass or fail according to argument order alone.

This is a direct violation of three policy statements, each quoted from the file that states it:

- `.claude/rules/general-unit-test.md`, § Core Principles item 1 (UT1 Independence): "Tests must be
  able to run in any order without impacting each other."
- `.claude/rules/general-unit-test.md`, § External Dependencies: "Tests must not rely on mutable global
  state or external configuration that can change between runs."
- `.claude/rules/csharp.md`, § Deterministic Test Rules: "Tests must produce identical results in the
  IDE test runner and in CLI runs so local and CI behavior agree."

The mechanism, which `[P0-T9]` censuses: the test host's assembly-probing path follows the directory of
the **first** assembly on the command line. `VBFunctions.Test/bin/Debug` contains `ExCSS.dll`;
`SVGControl.Test/bin/Debug` does not. When the sibling is first, its directory supplies `ExCSS` to the
host and the parse succeeds; when `SVGControl.Test` is first, nothing supplies it and the parse fails
with `FileNotFoundException`.

Run B is also the reason this defect survived two audits: it is the passing ordering, and a
nine-assembly wrapper run reaches a comparable state. `[P1-T6]` re-runs **Run A's** ordering after the
fix and requires 0 failed, which is the comparison that actually discriminates.

## Output Summary

Run A `EXIT_CODE: 1` at **76 total / 70 passed / 6 failed**; Run B `EXIT_CODE: 0` at **76 total / 76
passed / 0 failed**. Both triples match the plan's expectations. The two runs executed the same
binaries — verified by SHA-256 and by modification timestamps predating this session — in the same
session, differing only in argument order, and their failed counts differ by 6. Order dependence is
therefore demonstrated on this host, and the before-state of the fix is established for `[P1-T6]` to
compare against.
