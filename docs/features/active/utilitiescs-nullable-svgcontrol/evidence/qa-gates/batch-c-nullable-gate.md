# Batch C — CSharpier + Nullable Pragma Gate (SvgImageSelector.cs)

Timestamp: 2026-07-19T02-50

## Step 1 — Formatting

Command: `dotnet tool run csharpier .`

EXIT_CODE: 0

Output Summary: `Checked 1406 files in 4669ms.` No residual formatting changes.

## Step 2 — Per-File Nullable Pragma Gate

Command: `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1 (2 pre-existing, unrelated `CS0649` errors in `SvgImageSelector.cs`, unchanged from
baseline — see `evidence/baseline/baseline-nullable-pragma-gate.md`)

Output Summary: **Zero nullable diagnostics** (`grep -oE "CS8[0-9]{3}"` matches nothing) for
`SvgImageSelector.cs`.

### Annotations applied (summary)

- `#nullable enable` pragma added.
- Fields: `_relativeImagePath` -> `string?`, `_absoluteImagePath` -> `string?`, `_svgResource` ->
  `ISvgResource?` (all 3 as specified by P3-T1).
- `ImagePath.get`'s `else` branch: `return _relativeImagePath!;` with an in-code comment — the
  central judgment call; full rationale in
  `evidence/other/imagepath-judgment-call-decision.md` (P3-T2/P3-T3).
- `ResourceName` property retyped `ISvgResource?`; internal `AboluteImagePath` property retyped
  `string?` (typo preserved, unchanged) (P3-T4).
- Additional annotations/justified `!` needed beyond the plan's explicit list, applied
  consistently with prior batches' conventions:
  - `PropertyChanged` event -> `PropertyChangedEventHandler?` (matches the pre-existing
    `?.Invoke` pattern already in the file, and the identical treatment applied to
    `SvgRenderer.PropertyChanged` in Batch A).
  - Public `Render()` -> `Bitmap?` (a direct passthrough of `_renderer.Render()`, itself
    `Bitmap?` since Batch A; no guard added).
  - `ResourceName` setter: `_renderer.Document = SvgRenderer.GetSvgDocument(value.Data!);` — a
    justified `!` on `value.Data` (nullable `ISvgResource.Data`), preserving the pre-existing
    pass-through behavior into `GetSvgDocument`'s non-nullable `file` parameter.
  - `SaveRendering` setter: `Image image = Render()!;` — justified because the enclosing `if`
    already guards on `_renderer.Document != null`, which guarantees `Render()` cannot return
    null on this path; no new guard clause introduced (the pre-existing guard is reused).

No post-condition attribute was added at any point. The class remains a plain class implementing
`INotifyPropertyChanged` (no `record`/`init`). All annotation choices are additive nullability
metadata only; no runtime/IL behavior changed and no public API was removed.
