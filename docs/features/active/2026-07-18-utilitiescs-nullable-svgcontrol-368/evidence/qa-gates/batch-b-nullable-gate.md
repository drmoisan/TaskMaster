# Batch B — CSharpier + Nullable Pragma Gate

Timestamp: 2026-07-19T02-15

## Step 1 — Formatting

Command: `dotnet tool run csharpier .`

EXIT_CODE: 0

Output Summary: `Checked 1406 files in 5003ms.` No formatting changes required after the Batch B
edits.

## Step 2 — Per-File Nullable Pragma Gate

Command: `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1 (2 pre-existing, unrelated `CS0649` errors in `SvgImageSelector.cs`, unchanged from
baseline)

Output Summary: **Zero nullable diagnostics** (`grep -oE "CS8[0-9]{3}"`, matches nothing) for
both Batch B files (`SvgResourceConverter.cs`, `DropDownEditor.cs`).

### Annotations applied (summary)

- `SvgResourceConverter.cs`: `#nullable enable` pragma added. The pre-existing `value is null`
  guard before the `(ISvgResource)value` cast is unchanged. One justified `!` was required beyond
  the pragma: `return resource.Name!;` in `ConvertTo` — `ISvgResource.Name` is nullable (Batch A),
  and the method's `object` return type is non-nullable; the `!` preserves the pre-existing
  behavior of returning whatever `Name` currently holds (including `null`) with no new fallback
  value or guard clause introduced.
- `DropDownEditor.cs`: `#nullable enable` pragma added. Exactly the three named null-flow points
  from the plan were resolved: `Assembly asm = null;` -> `Assembly? asm = null;`; the
  `IDesignerHost host = ...` assignment gained a null-forgiving operator —
  `(provider.GetService(typeof(IDesignerHost)) as IDesignerHost)!` — preserving the pre-existing
  NRE-on-null behavior at `host.RootComponentClassName`; and the field
  `private IWindowsFormsEditorService _editorService;` is now
  `private IWindowsFormsEditorService? _editorService;`. No post-condition attribute was added; no
  new `if (x is null) throw` guard was introduced.

All annotation choices are additive nullability metadata only; no runtime/IL behavior changed.
