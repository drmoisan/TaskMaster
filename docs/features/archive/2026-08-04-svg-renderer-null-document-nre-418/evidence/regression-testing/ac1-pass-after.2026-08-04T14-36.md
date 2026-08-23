# AC-1 Pass-After — Post-Fix Regression Run (Issue #418, task P1-T23)

Timestamp: 2026-08-04T19-09

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` (version 0.7)
Task: `[P1-T23]`
Branch: `bug/svg-renderer-null-document-nre-418`
Cross-reference: `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` (the pre-fix failing run)

## Command

Build:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU
```

Build EXIT_CODE: 0 — `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`. The `csc.exe`
command line confirms all three new test files in the compile set:
`SvgAssemblyProbeDirectoryTests.cs`, `SvgRendererNullToleranceTests.cs`, and
`SvgRendererParseContractTests.cs`, compiled at `/langversion:7.3` as expected.

Test:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug
```

Test EXIT_CODE: 0

## Output Summary

`Test Run Successful.`

- total: 6139
- passed: 6139
- failed: 0
- skipped: 0
- assemblies discovered: 9 (`Discovered 9 test assemblies.`)
- total time: approximately 55 seconds

The repo-wide `-SearchRoot .` form was used per plan version 0.7. The narrower
`-SearchRoot SVGControl.Test` form is blocked by the `scripts/vscode/Invoke-MSTest.ps1`
scalar-`.Count` defect and its fix is out of scope. The repo-wide form additionally proves no
regression across the other eight test assemblies.

### The four task P1-T8 regression tests — all passed

Each of these failed with `System.NullReferenceException` at `SvgRenderer.cs:133` in
`ac1-fail-before.2026-08-04T14-36.md` and now passes:

| Test | Result |
|---|---|
| `Constructor_WithMalformedBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull` | Passed (65 ms) |
| `Constructor_WithMalformedBytesAndMargin_DoesNotThrowAndLeavesDocumentNull` | Passed (< 1 ms) |
| `Constructor_WithEmptyBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull` | Passed (< 1 ms) |
| `Constructor_WithEmptyBytesAndMargin_DoesNotThrowAndLeavesDocumentNull` | Passed (< 1 ms) |

This is the AC-1 fail-before / pass-after pair: 4 failed before the fix, 0 failed after, with no
change to the assertions those four tests make.

### All 27 new tests in SVGControl.Test — all passed

`SvgRendererParseContractTests` (13): the four task P1-T8 constructor regressions above, plus
`GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`,
`GetSvgDocument_WithNullPayload_ThrowsArgumentNullException`,
`TryGetSvgDocument_WithNullPayload_ThrowsArgumentNullException`,
`TryGetSvgDocument_WithMalformedBytes_ReturnsFalseAndCapturesTheException`,
`TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException`,
`TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError`,
`GetSvgDocumentOrThrow_WithMalformedBytes_ThrowsWithTheParserExceptionInner`,
`GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`,
`TryGetSvgDocument_WithInjectedParseSeam_SurfacesTheSameExceptionInstance`.

`SvgRendererNullToleranceTests` (5): `DocumentSetter_AssignedNull_SucceedsAndLeavesDocumentNull`,
`Render_WithNullDocument_ReturnsNull`,
`SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`,
`DefaultImageConstructor_DoesNotThrow`,
`UseDefaultImageSetterToFalse_DoesNotThrowAndRecordsTheNewValue`.

`SvgAssemblyProbeDirectoryTests` (9): four `TryGetDirectoryFromCodeBase` cases (valid `file://`
URI, `null`, `""`, whitespace-only, non-URI string) and four `GetProbeDirectories` cases (all three
inputs populated with order preserved, empty `assemblyLocation` skipped, case-variant directories
de-duplicated, all inputs null returning an empty list).

Test-count reconciliation: the authoritative baseline
`evidence/baseline/test-coverage.2026-08-04T21-04.md` recorded 6112 tests across 9 assemblies.
6139 - 6112 = 27, which is exactly the 27 new tests added by `[P1-T8]`, `[P1-T20]`, `[P1-T21]`, and
`[P1-T22]`. No pre-existing test was added, removed, renamed, or skipped.

### Pre-existing tests in the same run

Pre-existing tests in same run: `GetRelativePath_Test` and `RelativePathCoverageTests` together
contribute 37 tests to the `SVGControl.Test` assembly. All 37 passed. Because the run reports
`failed: 0` across all 6139 tests and the count reconciliation above accounts for every added test,
no pre-existing test in `SVGControl.Test` or in the other eight assemblies regressed. In
`ac1-fail-before.2026-08-04T14-36.md` the same 37 passed while the 4 new tests failed (41 total /
37 passed / 4 failed).

### AC-7 corroboration status — no narrowing required

`[P1-T21]` carried a contingency: if `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`
failed because the ExCSS bind does not succeed in the testhost, the observed exception was to be
recorded here as AC-7 corroboration and the assertion narrowed to `SetDefaultImage()` merely not
throwing. **That contingency was not triggered.** The test passed in 411 ms with the full
`Document`-non-null assertion intact, which means the `AssemblyResolve` fallback delivered by
`[P1-T15]`/`[P1-T18]` plus the ExCSS binding redirect delivered by `[P1-T2]` together satisfy the
ExCSS bind inside the vstest testhost. AC-7's root-cause condition is therefore corroborated as
*resolved* in the test host rather than as observed-and-narrowed.

## Correction Applied During This Task — Empirical Premise of the Element-Free Path

Two tests authored under `[P1-T20]` initially failed on the first execution of this task (6138
total / 6136 passed / 2 failed). The failure was in the tests' premise, not in the production fix.

`[P1-T20]`'s task text, following research §1.4, states that `Array.Empty<byte>()` reaches the
element-free path where `SvgDocument.Open` "returns `null` without throwing". The measured behavior
contradicts this. For an empty payload the XML reader raises before any SVG element handling:

```
Expected error to be <null> ... but found System.Xml.XmlException: Root element is missing.
   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
   at System.Xml.XmlTextReaderImpl.ParseDocumentContent()
   at Svg.SvgTextReader.Read()
   at Svg.SvgDocument.Create[T](XmlReader reader, SvgElementFactory elementFactory, List`1 styles)
   at Svg.SvgDocument.Open[T](Stream stream, SvgOptions svgOptions)
   at SVGControl.SvgRenderer.OpenFromBytes(Byte[] file) ... line 403
   at SVGControl.SvgRenderer.TryGetSvgDocument(...) ... line 423
```

An empty payload is therefore an *exception* failure shape, identical in kind to malformed input,
not the exception-free null shape. No plain byte payload reaches the element-free path.

Resolution, entirely within the test files and without altering production code or weakening any
assertion:

1. `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseWithNoCapturedException` was retargeted to assert
   the measured behavior and renamed
   `TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException`. It now asserts
   `error.Should().BeOfType<XmlException>()`, a **stricter** assertion than the original
   `NotBeNull` would have been.
2. `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithoutAnInnerException` was retargeted and renamed
   `GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`, asserting
   `InnerException` is an `XmlException`.
3. A new test, `TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError`,
   covers the element-free path through the `[P1-T11]` delegate seam configured with
   `Setup(...).Returns((SvgDocument)null)`. This is the only deterministic route to that branch and
   it mutates no global state, consistent with Design Decision 5's purpose for the seam. The
   asymmetry AC-5 describes — failure reported with no captured exception — is therefore still
   proven, on the branch that actually exhibits it.

Net effect on `[P1-T20]`'s acceptance clause: test count rose from 12 to 13 (its clause requires at
least nine), Moq and FluentAssertions are still used, the `BeSameAs` sentinel-identity assertion is
unchanged, and the file remains under 500 lines. Two now-known-false comments in the `[P1-T8]` block
describing empty input as "the exception-free null path" were corrected to describe the real failure
shape; no assertion in those four tests changed, and all four still pass.

Residual coverage note for `[P2-T8]`: because no plain payload reaches the element-free path,
`GetSvgDocumentOrThrow`'s null-`InnerException` branch (`DescribeFailure(null)` reached through the
public `Try` overload) is not drivable from a unit test, since `GetSvgDocumentOrThrow` deliberately
takes no seam parameter. The corresponding branch inside the seam-bearing `TryGetSvgDocument` **is**
covered by the new seam test.

## Post-`csharpier` Line Counts (all five in-scope C# files, informational)

| File | Lines | Limit |
|---|---|---|
| `SVGControl/SvgRenderer.cs` | 495 | 500 |
| `SVGControl/SvgAssemblyProbe.cs` | 67 | 500 |
| `SVGControl.Test/SvgRendererParseContractTests.cs` | 312 | 500 |
| `SVGControl.Test/SvgRendererNullToleranceTests.cs` | 143 | 500 |
| `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` | 187 | 500 |

`[P2-T2]` re-records these after the Phase 2 formatting run.

## Verdict

PASS. Satisfies AC-1 (the four regression tests failed before the fix and pass after it) and the
"tests execute under the test runner" half of AC-9 (`SVGControl.Test` is a solution member, builds
at `EXIT_CODE: 0`, and its tests are discovered and executed by `vstest.console.exe` in the
repo-wide run).
