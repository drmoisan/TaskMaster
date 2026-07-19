# Batch C — Nullable Pragma Gate

- Timestamp: 2026-07-19T11-45
- Task: [P3-T4]
- Batch C files: `InputBox.cs`, `NotImplementedDialog.cs`

## Step 1 — CSharpier format

- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1406 files`. Only the 2 Batch C files changed.

## Step 2 — Authoritative CS86xx detector (scoped isolated UtilitiesCS build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx count: 0
- Output Summary: `0 Error(s)`. Zero CS86xx across the 2 Batch C opted-in files.

## Step 3 — Plan-mandated solution-wide command (for the record)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- CS86xx count: 0
- Output Summary: Aborts on pre-existing vendored SVGControl CS0649 (invariant); zero CS86xx.

## Annotations applied (Batch C)

- `InputBox.cs`: pragma + `public static string? ShowDialog(...)` (returns `null` on cancel, CS8603;
  the XML doc already documents "or null if cancelled"). The `AsyncLocal<Func<InputBoxViewer,
  DialogResult>>` dialog-invoker seam and its `?? RealDialogInvoker` fallback are unmodified.
- `NotImplementedDialog.cs`: pragma only. `DisplayInvoker` has a non-null default lambda; no
  uninitialized non-nullable fields; zero CS86xx with no annotation edits.

Result: AC1 satisfied for Batch C; the `InputBox.ShowDialog` change is additive nullability matching
the documented behavior (AC5).
