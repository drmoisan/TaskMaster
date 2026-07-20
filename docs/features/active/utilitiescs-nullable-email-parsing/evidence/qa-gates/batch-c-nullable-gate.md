# Batch C Nullable Gate (SubjectMapEncoder, SubjectMapEntry, SubjectMapSco + SubjectMapSco.Orchestration, SubjectMapMetrics)

Timestamp: 2026-07-19T03-20

## 1. CSharpier

Command: `dotnet tool run csharpier -- format .`

EXIT_CODE: 0

Output Summary: `Formatted 1406 files in 1682ms.` No residual diff after the final fix pass.

## 2. Scoped per-file nullable pragma gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 across all 5 Batch C files (AC1 SATISFIED).
Build FAILED with the same 14 pre-existing, non-nullable errors as baseline (`CS0618` x13,
`CS0168` x1 in `AutoFile.cs`) — zero new non-nullable errors introduced by this batch.

## Fixes applied during this batch

- `SubjectMapEncoder.cs`: `_filename`/`_folderpath`/`_encoder`/`_decoder`/`_subjectMap` annotated
  nullable (only the parameterized constructor sets them; the existing `RebuildEncoding()`
  `NullReferenceException` guard on `_subjectMap` is unchanged). `Decoder` getter's `return
  _decoder!;` and 3 further `!` usages (`x.Folderpath!.Split(...)`, `_encoder!.Serialize()`,
  `_encoder![x]`) are justified since each precedes a call already gated indirectly by
  `ReadyToEncode`/existing null-anticipating logic that the compiler cannot trace through
  post-condition attributes (banned on net481).
- `SubjectMapEntry.cs`: annotated every field/property left unset by the parameterless/
  regex-only constructors (`_commonWords`, `_folderPath`, `_folderName`, `_subjectText`,
  `_encoder`, `_folderWordLengths`, `_folderTokens`, `_subjectTokens`, `_folderEncoded`,
  `_subjectEncoded`, `_subjectWordLengths`) as nullable; `_tokenizerRegex` stays non-nullable
  (every constructor sets it directly). `IsNull(object?, ...)`, `TokensToEncode()` and the two
  private `Encode(ISubjectMapEncoder, ...)` overloads that return `null` are now nullable-typed.
  Justified `!` is used at call sites gated by `ReadyToEncode`/`IsNull` helper calls (the
  compiler cannot trace their null-narrowing effect without banned post-condition attributes)
  and at `TryRepair`/`Validate`'s dereferences of fields only ever populated via `Init`.
- `SubjectMapSco.cs` + `SubjectMapSco.Orchestration.cs` (mandatory combined partial-class
  batch): `Find(string, string)` return type changed to `SubjectMapEntry?` (returns `null` when
  not found); `ResolveFolder(...)` return type changed to `MAPIFolder?` per the plan's explicit
  direction, consistent with the already-null-safe `.Where(tuple => tuple.Folder != null)`
  filter in `QueryOlFolders`; the post-filter projection uses `tuple.Folder!` (logically
  non-null after the filter, but LINQ `.Where()` does not narrow tuple-element types); one `!`
  on `archiveRoot!.StoreID` in `GetFolderTreeSnapshot` (reachable only when
  `archiveRoot?.StoreID` was non-blank, i.e. `archiveRoot` non-null, but the compiler cannot
  connect a `?.`-guarded content check to the receiver's own null-state); `Consume<T>`'s local
  `list` declared nullable (`List<T>?`); the nested `SummaryMetric` class's `FolderName`/
  `FolderPath` fields and the partial type's `summaryMetrics` field are nullable (none are set
  by any of `SubjectMapSco`'s 5 constructors; all are populated later via object-initializer/
  assignment).
- `SubjectMapMetrics.cs`: pragma only; no annotation changes required (the Designer-generated
  sibling `SubjectMapMetrics.Designer.cs` remains unmodified and excluded, and its
  oblivious-declared fields do not trigger CS8618 from this file's pragma).

All 5 Batch C files carry `#nullable enable`; `SubjectMapMetrics.Designer.cs` is unmodified. No
`<Nullable>` element was added to the csproj (AC2). No post-condition attribute was added.
