# Batch E — CSharpier + Nullable Pragma Gate

Timestamp: 2026-07-19T03-45

## Step 1 — Formatting

Command: `dotnet tool run csharpier .`

EXIT_CODE: 0

Output Summary: `Checked 1406 files in 5156ms.` No residual formatting changes.

## Step 2 — Per-File Nullable Pragma Gate

Command: `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1 (2 pre-existing, unrelated `CS0649` errors in `SvgImageSelector.cs`, unchanged)

Output Summary: **Zero nullable diagnostics** (`grep -oE "CS8[0-9]{3}"` matches nothing) for both
Batch E files (`ButtonSVG.cs`, `PictureBoxSVG.cs`) — both reached zero CS86xx on the first pass
after adding the pragma and the two specified `ButtonSVG.cs` signature changes; no further
diagnostics needed resolution.

### Annotations applied (summary)

- `ButtonSVG.cs`: `#nullable enable` pragma added. `ObjectToByteArray(Object obj)` retyped
  `ObjectToByteArray(object? obj)` (existing `if (obj != null)` guard unchanged);
  `GetStringForValue(object value)` retyped `GetStringForValue(object? value)` (existing
  `if (value == null) return "null";` guard unchanged). Event handler parameters
  (`ButtonSVG_Resize`, `ImageSVG_PropertyChanged`) left unannotated as oblivious framework
  delegate types, as specified. `base.Image = ImageSVG.Render();` required no change: `Render()`
  returns the nullable `Bitmap?` (Batch C), and `Control.Image` is an oblivious (non-nullable-
  annotated) net481 BCL property, so no CS86xx is raised assigning a nullable value to it.
- `PictureBoxSVG.cs`: `#nullable enable` pragma added; zero additional annotation changes were
  required. Note: the plan anticipated an "independent copy of `GetStringForValue`" in this file
  mirroring `ButtonSVG.cs`'s; on inspection, `PictureBoxSVG.cs` does not define its own
  `GetStringForValue` (confirmed via repository-wide search — only `ButtonSVG.cs` and
  `DropDownEditor.cs` define one), so there was nothing to mirror for that specific member. The
  two `this.Image = _imageSvg.Render();` / `base.Image = ImageSvg.Render();` assignments required
  no change for the same oblivious-BCL-property reason as `ButtonSVG.cs`. Event handler
  parameters (`Control_SizeChanged`, `ImageSVG_PropertyChanged`) left unannotated.

No post-condition attribute was added in either file. All annotation choices are additive
nullability metadata only; no runtime/IL behavior changed and no public API was removed.
