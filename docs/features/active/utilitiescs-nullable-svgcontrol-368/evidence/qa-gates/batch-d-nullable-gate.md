# Batch D — CSharpier + Nullable Pragma Gate

Timestamp: 2026-07-19T03-15

## Step 1 — Formatting

Command: `dotnet tool run csharpier .`

EXIT_CODE: 0

Output Summary: `Checked 1406 files in 6936ms.` No residual formatting changes.

## Step 2 — Per-File Nullable Pragma Gate

Command: `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1 (2 pre-existing, unrelated `CS0649` errors in `SvgImageSelector.cs`, unchanged)

Output Summary: **Zero nullable diagnostics** (`grep -oE "CS8[0-9]{3}"` matches nothing) for all 3
Batch D files (`SvgOptionsConverter.cs`, `SvgOptionsConverter2.cs`, `SVGFileNameEditor.cs`).

### Annotations applied (summary)

- `SvgOptionsConverter.cs` (class `SvgOptionsConverter1`, dead but in scope): `#nullable enable`
  pragma added; local `SvgImageSelector image = value as SvgImageSelector;` retyped
  `SvgImageSelector? image = ...` (the `as`-cast result is nullable; the existing
  `if (image != null)` guard is unchanged). Consumes the now-nullable `AboluteImagePath` from
  Batch C without re-editing `SvgImageSelector.cs`. Class not renamed or deleted.
- `SvgOptionsConverter2.cs` (class `SvgOptionsConverter`, live): `#nullable enable` pragma added;
  same `SvgImageSelector?` local retype as above, plus
  `string? resourceName = image.ResourceName.Name;` (both `ResourceName` and `.Name` are nullable
  from Batches C/A; the interpolated string `$"{resourceName} {autoSizeCode}"` accepts a nullable
  operand without further changes). Consumes the now-nullable `ResourceName`/`AutoSize` members
  from Batch C without re-editing `SvgImageSelector.cs`.
- `SVGFileNameEditor.cs`: `#nullable enable` pragma added; `private string _appPath;` given the
  same `= string.Empty;` inline-initializer idiom already used for `_currentValue`,
  `_absoluteFilepath`, and `_fileName` three lines above it (as specified); the other three
  fields' initializers are unchanged. One additional annotation needed beyond the plan's explicit
  list: `private OpenFileDialog _ofd;` -> `private OpenFileDialog? _ofd;` (the field is only ever
  assigned inside `InitializeDialog`, so it is genuinely nullable before that method runs; the
  existing `if (_ofd != null)` guard in `EditValue` already handles this correctly).

No post-condition attribute was added in any file. All annotation choices are additive
nullability metadata only; no runtime/IL behavior changed and no public API was removed.
