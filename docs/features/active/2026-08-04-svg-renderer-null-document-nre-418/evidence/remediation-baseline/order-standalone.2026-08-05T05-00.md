# Pre-Change Standalone Run — `[expect-fail]` Before-Half

- Task: `[P0-T7]` `[expect-fail]`
- Timestamp: 2026-08-04T23-34
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Nothing was built and nothing was changed before this run. The binaries under test are those already
  present in `SVGControl.Test/bin/Debug` at HEAD `dc00cf1d`.

## `[expect-fail]` declaration

**A non-zero exit code is the expected measurement outcome of this task, not a task failure.** This run
is the before-half of this cycle's only decisive proof. Its purpose is to demonstrate on this host and
in this session that the defect the fix addresses actually reproduces here. Formatting, linting, and
type checking remain normal pass/fail gates and are unaffected by this declaration.

## Command

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' SVGControl.Test\bin\Debug\SVGControl.Test.dll
```

Run from the repository root. Switch set: **no** `/EnableCodeCoverage`, **no** `/InIsolation`, **no**
`/Settings`. This is the identical switch set `[P1-T5]` and `[P2-T9]` use, so the before/after
comparison is like-for-like.

```
EXIT_CODE: 1
```

Runner: `VSTest version 18.8.0 (x64)`. `A total of 1 test files matched the specified pattern.`

## Result counts

```
Test Run Failed.
Total tests: 75
     Passed: 69
     Failed: 6
 Total time: 1.0967 Seconds
```

| Figure | Measured | Plan expectation | Match |
|---|---|---|---|
| Total | **75** | 75 | yes |
| Passed | **69** | 69 | yes |
| Failed | **6** | 6 | yes |

The failed count is greater than zero, so **the defect reproduces on this host**. No halt condition
fires and the plan proceeds.

## Every failed test, by name

Extracted with `grep -E '^\s+Failed ' <run output>`:

```
  Failed SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull [62 ms]
  Failed GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument [1 ms]
  Failed Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull [1 ms]
  Failed TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException [1 ms]
  Failed GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner [1 ms]
  Failed GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument [2 ms]
```

Six names, matching one-for-one the six the plan's `[P1-T5]` enumerates and the six recorded in
`remediation-inputs.2026-08-04T22-28.md` § R-7 and `policy-audit.2026-08-04T22-28.md` § 6. The set is
identical; no test outside that set failed and no member of that set passed.

## The assembly-load exception, verbatim, with the requested assembly identity

From `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument`:

```
Test method SVGControl.Test.SvgRendererParseContractTests.GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument threw exception:
System.InvalidOperationException: SvgRenderer could not parse the SVG payload: System.IO.FileNotFoundException: Could not load file or assembly 'ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a' or one of its dependencies. The system cannot find the file specified. ---> System.IO.FileNotFoundException: Could not load file or assembly 'ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a' or one of its dependencies. The system cannot find the file specified. ---> System.IO.FileNotFoundException: Could not load file or assembly 'ExCSS, Version=4.2.3.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a' or one of its dependencies. The system cannot find the file specified.
  Stack Trace:
     at Svg.SvgDocument.Create[T](XmlReader reader, String css)
   at Svg.SvgDocument.Open[T](Stream stream, SvgOptions svgOptions)
   at SVGControl.SvgRenderer.OpenFromBytes(Byte[] file) in C:\Users\DanMoisan\repos\TaskMaster\SVGControl\SvgRenderer.cs:line 270
   at SVGControl.SvgRenderer.TryGetSvgDocument(Byte[] file, Func`2 parse, SvgDocument& document, Exception& error) in C:\Users\DanMoisan\repos\TaskMaster\SVGControl\SvgRenderer.cs:line 290
--- End of inner exception stack trace ---
   at SVGControl.SvgRenderer.GetSvgDocumentOrThrow(Byte[] file) in C:\Users\DanMoisan\repos\TaskMaster\SVGControl\SvgRenderer.cs:line 339
   at SVGControl.Test.SvgRendererParseContractTests.GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument() in C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\SvgRendererParseContractTests.cs:line 318
```

**Requested assembly identity, verbatim:**
`ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a`, with the innermost request
being `ExCSS, Version=4.2.3.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a`. The outer `4.3.2.0`
is the redirect target; the inner `4.2.3.0` is what `Svg` was compiled against. The
`PublicKeyToken=bdbe16be9b936b9a` matches the token in the `ExCSS` reference `[P1-T1]` adds and the
token in the `SVGControl.Test/app.config` redirect. `The system cannot find the file specified` is the
operative clause: this is a **file-not-found**, not a version-mismatch, so a binding redirect cannot
remedy it — the assembly is absent from the probing path entirely.

The `Debug Trace` section of the same failure shows both diagnostic channels firing as AC-2 and AC-3
require, which confirms the production degrade-and-log behavior is working correctly and that the
failure is environmental:

```
 vstest.console.exe Warning: 0 : SvgRenderer load 'ExCSS': System.IO.FileNotFoundException: Could not load file or assembly 'ExCSS' or one of its dependencies. The system cannot find the file specified.
 vstest.console.exe Error: 0 : SvgRenderer could not parse the SVG payload: System.IO.FileNotFoundException: Could not load file or assembly 'ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a' or one of its dependencies. The system cannot find the file specified.
```

The `SvgRenderer load 'ExCSS'` warning is the `AssemblyResolve` fallback being reached and failing —
it probes `SVGControl.Test/bin/Debug`, the same directory that lacks `ExCSS.dll`.

## Corroboration that this is the same root cause the assertions were written against

The two `XmlException`-asserting tests fail with an assertion message that names the substitution
directly, which is why the `## Do Not Do` list forbids weakening them:

```
  Failed TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException [1 ms]
  Error Message:
   Expected type to be System.Xml.XmlException because an empty payload has no root element, so the XML reader raises rather than returning null, but found System.IO.FileNotFoundException.
```

The assertion is correct as written. The empty payload *does* raise `XmlException` once `ExCSS` is
resolvable; it raises `FileNotFoundException` first only because the parse cannot get far enough to
reach the XML reader. This cycle is what makes the assertion hold unconditionally.

## Output Summary

`EXIT_CODE: 1`, as expected for this `[expect-fail]` before-half. **75 total, 69 passed, 6 failed**,
matching the plan's expected triple exactly. All six failures are the six enumerated tests, each
traceable to `System.IO.FileNotFoundException` for
`ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a` (innermost request
`Version=4.2.3.0`) with the message `The system cannot find the file specified`. The defect reproduces
on this host and in this session, so the before-state of the fix is demonstrated and execution proceeds
to `[P0-T8]`.
