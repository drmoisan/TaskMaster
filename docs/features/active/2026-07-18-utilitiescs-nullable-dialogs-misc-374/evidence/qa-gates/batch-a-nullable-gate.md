# Batch A — Nullable Pragma Gate

- Timestamp: 2026-07-19T11-20
- Task: [P1-T5]
- Batch A files: `DelegateButtonTemplate.cs`, `FolderNotFoundViewer.cs`, `MyBoxViewer.cs`, `InputBoxViewer.cs`

## Step 1 — CSharpier format

- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1406 files in 6005ms`. Only the 4 Batch A files changed (pragma add + annotations); no other file reformatted.

## Step 2 — Authoritative CS86xx detector (scoped isolated UtilitiesCS build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx count: 0
- Output Summary: `0 Error(s)`, `15 Warning(s)` (the pre-existing non-nullable CS0618/CS0168/CS0649 debt, demoted from errors by the scoped `WarningsNotAsErrors`). Zero CS86xx across the 4 Batch A opted-in files. This build actually compiles the cluster (past the SVGControl short-circuit), so it is the authoritative AC1 signal.

## Step 3 — Plan-mandated solution-wide command (for the record)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- CS86xx count: 0
- Output Summary: Aborts on 2 pre-existing vendored `SVGControl` CS0649 errors (4 log occurrences) before reaching the cluster; zero CS86xx emitted. Invariant across all batches (unrelated to issue #374). The scoped isolated build in Step 2 is the authoritative per-file CS86xx signal.

## Annotations applied (Batch A)

- `DelegateButtonTemplate.cs`: pragma only (no uninitialized non-nullable members).
- `InputBoxViewer.cs`: pragma only (static members initialized; handlers referenced only in Designer).
- `FolderNotFoundViewer.cs`: pragma + `public string? FolderAction { get; set; }` (was uninitialized non-nullable auto-property, CS8618; assigned only in the four `*_Click` handlers).
- `MyBoxViewer.cs`: pragma + `private readonly Dictionary<string, Delegate>? _map` (set only in the 2-arg ctor, CS8618); `Button1_Click`/`Button2_Click` use justified `_map!` (populated whenever a click can fire) and `(DialogResult)result!` (delegate returns a non-null DialogResult) instead of new runtime guards.

Result: AC1 satisfied for Batch A (zero CS86xx under the per-file pragma).
