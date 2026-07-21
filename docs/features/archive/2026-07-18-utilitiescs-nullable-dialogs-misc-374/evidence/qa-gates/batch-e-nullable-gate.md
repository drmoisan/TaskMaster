# Batch E — Nullable Pragma Gate

- Timestamp: 2026-07-19T12-15
- Task: [P5-T4]
- Batch E files: `MyBoxModeless.cs`, `YesNoToAll.cs`

## Step 1 — CSharpier format

- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1406 files`. Only the 2 Batch E files changed.

## Step 2 — Authoritative CS86xx detector (scoped isolated UtilitiesCS build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx count: 0
- Output Summary: `0 Error(s)`. Zero CS86xx across the 2 Batch E opted-in files.

## Step 3 — Plan-mandated solution-wide command (for the record)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- CS86xx count: 0
- Output Summary: Aborts on pre-existing vendored SVGControl CS0649 (invariant); zero CS86xx.

## Annotations applied (Batch E)

- `MyBoxModeless.cs`: pragma + 5-argument overload parameter `Action<MyBoxViewer>? showAction` (the
  4-argument overload invokes it with `showAction: null`, CS8625), reflecting the file's documented
  "defaulting to `viewer => viewer.Show()` when null" behavior. The `[ExcludeFromCodeCoverage]`
  4-argument overload and the `var show = showAction ?? (v => v.Show());` fallback are unmodified.
  The existing `using System.Diagnostics.CodeAnalysis;` is for `[ExcludeFromCodeCoverage]` only — no
  post-condition attribute was added.
- `YesNoToAll.cs`: pragma only. The `AsyncLocal<YesNoToAllResponse>` seam is a value type (not
  nullable-reference-prone); `Properties.Resources.*` images are oblivious generated properties.
  Zero CS86xx with no annotation edits.

Result: AC1 satisfied for Batch E; the `showAction` change is additive nullability matching the
documented null-defaulting behavior (AC5).
