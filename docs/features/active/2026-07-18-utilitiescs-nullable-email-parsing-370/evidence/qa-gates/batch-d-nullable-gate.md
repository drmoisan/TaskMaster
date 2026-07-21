# Batch D Nullable Gate (EmailFilerConfig, EmailFiler)

Timestamp: 2026-07-19T04-10

## 1. CSharpier

Command: `dotnet tool run csharpier -- format .`

EXIT_CODE: 0

Output Summary: `Formatted 1406 files in 1996ms.` No residual diff after the fix pass.

## 2. Scoped per-file nullable pragma gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 for both Batch D files (AC1 SATISFIED). Build
FAILED with the same 14 pre-existing, non-nullable errors as baseline (`CS0618` x13, `CS0168`
x1 in `AutoFile.cs`; `EmailFiler.cs`'s pre-existing `CS0618` line number shifted from 268 to 273
due to added lines, same diagnostic) — zero new non-nullable errors introduced by this batch.

## Fixes applied during this batch

- `EmailFilerConfig.cs`: annotated `_destinationOlPath`, `_globals`/`Globals`,
  `_fsAncestorEquivalent`, `_saveFsPath`, `_deleteFsPath`, `_originFolder`, `_originOlStem`,
  `_destinationOlFolder`/`DestinationOlFolder` as nullable (none set by the parameterless
  constructor); `TryResolveDestinationFolder()` return type changed to `Folder?` per the plan's
  explicit direction (already returns `null` in both the not-found and catch branches);
  `IsDeleteRelevant`'s `currentFolder.ThrowIfNull()` guard is unchanged; justified `!` used at
  `Globals!.Ol.InboxPath` / `Globals!.Ol.App` (no direct null-check precedes these dereferences).
- `EmailFiler.cs`: `Config`/`Globals`/`MailHelpers` backing fields seeded with a justified
  `default!` rather than widening their public property types to `?` — these are required
  dependencies already validated via `Config.ThrowIfNull(...)` /
  `MailHelpers.ThrowIfNullOrEmpty(...)` / `Globals.ThrowIfNull(...)` in `ValidateParameters()`,
  and widening them would cascade `!` across the many unguarded call sites throughout this
  class; `TryMoveMailItemHelperAsync`'s tuple `Moved` element changed to `MailItem?` (per the
  plan's explicit direction) without changing the tuple shape or the deconstruction call sites
  in `ProcessMailHelperAsync`/`TryMoveMailItemForProcessingAsync`; the nested `MoveMailResult`
  class's `Moved` property/constructor parameter changed to `MailItem?` to match (its own
  consumer, `ProcessMailHelperAsync`, already null-checks `mailItemTemp` before use); justified
  `!` used at 4 call sites consuming `EmailFilerConfig`'s now-nullable `Globals`/`SaveFsPath`/
  `DestinationOlFolder` contracts from this same batch, consistent with the upstream
  `NullExtensions.ThrowIfNull<T>`/`ThrowIfNullOrEmpty` and `StringExtensions.IsNullOrEmpty`
  contracts this cluster consumes (AC5).

Both Batch D files carry `#nullable enable`. No `<Nullable>` element was added to the csproj
(AC2). No post-condition attribute was added.
