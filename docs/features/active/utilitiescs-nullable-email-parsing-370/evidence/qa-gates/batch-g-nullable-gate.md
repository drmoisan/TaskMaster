# Batch G Nullable Gate (AutoFile, SortEmail) — Final Batch

Timestamp: 2026-07-19T06-15

## 1. CSharpier

Command: `dotnet tool run csharpier -- format .`

EXIT_CODE: 0

Output Summary: `Formatted 1406 files in 1933ms.` No residual diff after the fix pass.

## 2. Scoped per-file nullable pragma gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 for both Batch G files (AC1 SATISFIED). Build
FAILED with the same 14 pre-existing, non-nullable errors as baseline (`CS0618` x13, `CS0168`
x1 in `AutoFile.cs`) — zero new non-nullable errors introduced by this batch. This is the final
batch; with Batch G clean, all 24 remediation-target files in the cluster now carry `#nullable
enable` with zero CS86xx.

## Fixes applied during this batch

- `AutoFile.cs`: pragma only for the bulk of the file (all Outlook interop and `MailItemHelper`
  member accesses are oblivious); `Category_IsAlreadySelected(dynamic objItem, string strCat)`'s
  `dynamic` parameter is left unannotated per the plan's explicit direction (exempt from
  nullable analysis), but the explicit `(objItem.Categories as string)` cast INSIDE the method
  body has a static type of `string?` (the `as` operator's result is always nullable) even
  though `objItem` itself is `dynamic` — a justified `!` is used at that one cast-then-`.Split(...)`
  call site.
- `SortEmail.cs` (1407 lines, not split — pre-existing >500-line condition per Scope
  Invariants): `Folder olDestination = null;`, `MailItem mailItemTemp = null;`, and one
  additional pre-existing pattern the plan's illustrative list did not name explicitly
  (`MailItem mailItemNew = null;`) are annotated `?`; `string[] strOutput = null;` and the
  uninitialized `string[,] strAryOutput;` local are annotated `?` per the plan's explicit
  direction. Both `ResolvePaths(...)` overloads' `out string deleteFsPath` /
  `out Folder destinationFolder` parameters (and their matching call-site inline `out`
  declarations) are nullable, since `deleteFsPath`/`destinationFolder` are only conditionally
  assigned. `GetAttachmentsInfo`/`GetAttachmentsInfoAsync`'s `deleteFsPath` parameters are
  nullable to match. `SanitizeArray`'s `string[,]? strAryOutput`/`ref string[]? strOutput`
  parameters match the nullable call-site locals, with a justified `!` at the pre-existing
  (unchanged) `strOutput[j] = ...` index-assignment. `object objItem = null` default parameter
  in `InitializeSortToExisting` is nullable. Justified `!` at `PushToUndoStack`/
  `CaptureMoveDetails` call sites consuming the nullable `mailItemTemp` local, consistent with
  the upstream `StringExtensions.IsNullOrEmpty` contract this file also consumes correctly
  elsewhere (AC5). No coverage-regression risk from any of `SortEmail.cs`'s own lines since the
  file is almost entirely `[ExcludeFromCodeCoverage]`.
- `EmailFiler.cs` (previously remediated in Batch D): one additional fix surfaced by this
  batch — `SaveAttachmentAsync(AttachmentHelper attachment)`'s call to
  `attachment.SaveAttachmentAsync(Config.SaveFsPath)` now resolves to `SortEmail.cs`'s extension
  method (`this AttachmentHelper attachmentHelper, string destinationPath`), which only became
  nullable-enabled in this batch; a justified `!` at `Config.SaveFsPath!` resolves the
  now-visible cross-batch nullable-contract mismatch without altering either file's public
  signatures.

Both Batch G files carry `#nullable enable`. No `<Nullable>` element was added to the csproj
(AC2). No post-condition attribute was added.
