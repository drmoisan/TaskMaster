# Batch F Nullable Gate (EmailDataMiner Partial-Class Group — Mandatory Single Batch)

Timestamp: 2026-07-19T05-40

## 1. CSharpier

Command: `dotnet tool run csharpier -- format .`

EXIT_CODE: 0

Output Summary: `Formatted 1406 files in 1597ms.` No residual diff after the final fix pass.

## 2. Scoped per-file nullable pragma gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 across all 4 Batch F files (AC1 SATISFIED).
Build FAILED with the same 14 pre-existing, non-nullable errors as baseline (`CS0618` x13,
`CS0168` x1 in `AutoFile.cs`) — zero new non-nullable errors introduced by this batch.

## Plan-text file-attribution note (P6-T4)

The plan's P6-T4 task text attributes `TryLoadObjectAndGetMemorySize<T>`'s tuple and the
`FolderStruct` primary-constructor struct to `EmailDataMiner.Transform.cs`. On inspection,
`TryLoadObjectAndGetMemorySize<T>` is actually declared in `EmailDataMiner.Serialization.cs`,
and `FolderStruct` is actually declared in `EmailDataMiner.FolderExtraction.cs` — neither
member exists in `EmailDataMiner.Transform.cs`. This is a file-name mislabeling in the plan
text (all four files are remediated together in this same mandatory single batch regardless),
not a scope-invariant violation; the described annotation work was applied to the file that
actually contains each member.

## Fixes applied during this batch

- `EmailDataMiner.cs`: `_globals` needs no nullable annotation (the single constructor always
  assigns it); `_sw` is annotated nullable (`SegmentStopWatch? _sw = default;`) since it is never
  reassigned anywhere across the four partial files after its `= default` (null) inline
  initializer — justified `!` at all 9 consumption sites in `EmailDataMiner.FolderExtraction.cs`
  (`_sw!.LogDuration(...)`, `_sw!.WriteToLog(...)`). `MineEmails()`'s return type is
  `Task<ScBag<MinedMailInfo>?>` (returns `null` when the AppData special folder is not found).
- `EmailDataMiner.Serialization.cs`: `Deserialize<T>`, `DeserializeFromFolder<T>`, both
  `DeserializeAsync<T>` overloads, and `DeserializeForValidation<T>` return unconstrained `T?`
  per the plan's explicit direction, replacing the `default(T)` sentinel on a missing
  lookup/file. `TryLoadObjectAndGetMemorySize<T>`'s tuple `Object` element is `(T? Object, long
  Size)`. `ValidateJson<T>`'s `obj` local declared `T?` to match (already null-checked via
  `if (obj != null)`). One `!` at a `new MinedMailInfo(mailInfo!)` call site consuming the
  now-nullable `TryLoadObjectAndGetMemorySize` tuple result.
- `EmailDataMiner.Transform.cs`: `!` at 2 call sites consuming Batch A's now-nullable
  `MinedMailInfo.FolderInfo` (`FilterExcluded`, `RemapFolderPaths` — 3 total dereferences across
  those two methods); `ToIItemInfoArray`'s early `return default!;` and `ToMinedMail(IItemInfo[]
  items)`'s `(await Task.Run(...))!` preserve their original non-nullable declared return types
  (avoiding a delegate-covariance ripple into `FolderGroupTransformer<T>`/`Func<Tin,
  Task<Tout>>`); `Deserialize<Tin>`'s consuming local changed to `Tin? obj` (already
  null-checked via the existing `if (obj is not null)`); `Load<T>`'s `DeserializeAsync<T>`
  consumption uses `(await ...)!` to keep `Load<T>`'s own declared return type unchanged.
- `EmailDataMiner.FolderExtraction.cs`: `FolderStruct` remains a plain `internal struct` with
  its C# 12 primary-constructor syntax (no `record`/`record struct` conversion); the `Scan(...)`
  seed's `default(FolderWrapper)!` argument is justified (the seed's `FolderInfo` member is
  never read by the accumulator lambda). `QueryOlFolders(FolderTreeSnapshot)`'s
  `resolver.TryResolve(node, out var folder) ? folder : null` ternary plus `.OfType<MAPIFolder>()`
  filter and `CreateFolderWrapper`'s `TryResolve(...) && folder is MAPIFolder mapiFolder` branch
  already compiled clean with no annotation changes needed. `TryResolveMapiHandles(FolderTree,
  FolderWrapper[])`'s `FolderWrapper? handle = null;` local is annotated per the plan's explicit
  direction; both dereference sites narrow cleanly (local variable, reassigned in every loop
  iteration before use). `GetOlFolderTree(ProgressTracker?)` and
  `GetOlFolderSnapshotAsync(ProgressTracker? progress = null)` parameters are nullable (called
  with a literal `null`); one `!` at `archiveRoot!.StoreID` (same `?.`-guarded-content-check
  pattern seen in Batch C's `SubjectMapSco.Orchestration.cs`).

All 4 Batch F files carry `#nullable enable`. No `<Nullable>` element was added to the csproj
(AC2). No post-condition attribute was added.
