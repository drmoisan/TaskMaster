# [P2-T1] Coverage Gap Closure — GetSvgDocumentOrThrow Success Path

Timestamp: 2026-08-04T19-55

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU`

EXIT_CODE: 0

Output Summary:

- `Build succeeded. 0 Warning(s) 0 Error(s)`. `SVGControl.Test -> SVGControl.Test\bin\Debug\SVGControl.Test.dll`.
- **Test added (exactly one):** `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument` in
  `SVGControl.Test/SvgRendererParseContractTests.cs`. Arranges `byte[] valid = Defaults.GetDefault.SvgImage;`,
  acts with `SvgDocument document = SvgRenderer.GetSvgDocumentOrThrow(valid);`, asserts
  `document.Should().NotBeNull("the fail-fast member returns the parsed document for a well-formed payload")`.
  FluentAssertions, explicit Arrange-Act-Assert comments, no `?` annotation (project compiles as C# 7.3
  per `/langversion:7.3` in the observed csc command line), no temporary file, no network.
  This drives `return document!;` at `SVGControl/SvgRenderer.cs:469`, which previously had no covering test.
- **`[TestMethod]` count in `SvgRendererParseContractTests.cs`: 14** (was 13). No name collision: the
  pre-existing `GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument` is a distinct member and
  remains unmodified.
- **`SVGControl.Test/SvgRendererParseContractTests.cs` line count: 332** (was 312) — `<= 500`.
- **No other test method was modified.** The only other change in this file is the class-level XML doc
  quoted below. No existing `[TestMethod]` body, name, or assertion was touched.

## Corrected comment 1 of 2 — production, `SVGControl/SvgRenderer.cs:397-398`

Before (2 lines):

```csharp
        // Returns null when the payload holds no SVG elements, which is how the parser reports that
        // condition without raising. No handler here by design: TryGetSvgDocument is the boundary.
```

After (4 lines):

```csharp
        // Can return null in principle. That path is driven in tests through the injected parse
        // delegate on TryGetSvgDocument; whether a well-formed-XML-but-no-SVG-element payload
        // reaches it here is unmeasured (open question U-3). An empty payload does not: it raises
        // XmlException. No handler here by design: TryGetSvgDocument is the boundary.
```

The replaced comment no longer asserts the unmeasured half of open question U-3 as settled fact.

## Corrected comment 2 of 2 — test class XML doc, `SVGControl.Test/SvgRendererParseContractTests.cs:16-17`

Before (2 lines):

```csharp
    /// Two distinct failure shapes are covered: malformed input, where the underlying parser
    /// throws, and element-free input, where the parser returns null without throwing.
```

After (4 lines):

```csharp
    /// Two distinct failure shapes are covered: input the underlying parser rejects by throwing
    /// (malformed bytes, and an empty payload, which raises XmlException for a missing root
    /// element), and the element-free path where the parser returns null without throwing, which
    /// is driven deterministically through the injected parse delegate.
```

The doc no longer describes element-free input as the shape produced by an empty payload; the
empty-payload tests assert `XmlException`.

## Seven-line comment budget

`SVGControl/SvgRenderer.cs` post-edit line count: **497** — `<= 500`.

The production comment being replaced was 2 lines with 5 lines of headroom (495 -> 500), giving a hard
ceiling of 7 lines. The replacement is **4 lines**, consuming 2 of the 5 available, so the file grew
495 -> 497 and 3 lines of headroom remain. Counted before rebuilding; csharpier does not reflow
comments, so this count is stable across `[P2-T2]`.

Contributes to AC-5.
