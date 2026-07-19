# Batch B Nullable Gate (CtfMap, CtfIncidenceList, CommonWords)

Timestamp: 2026-07-19T02-10

## 1. CSharpier

Command: `dotnet tool run csharpier -- format .`

EXIT_CODE: 0

Output Summary: `Formatted 1406 files in 1781ms.` No residual diff after the fix pass.

## 2. Scoped per-file nullable pragma gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 for all 3 Batch B files (AC1 SATISFIED). Build
FAILED with the same 14 pre-existing, non-nullable errors as baseline (`CS0618` x13, `CS0168`
x1 in `AutoFile.cs`) — zero new non-nullable errors introduced by this batch.

## Fixes applied during this batch

- `CtfMap.cs`: `TryDequeueEntry(ref Queue<string> lines)` return type changed to `CtfMapEntry?`
  (returns `null` in 3 catch branches, matching actual behavior); `ReadFileToArray`'s
  `string[] filecontents = null;` local changed to `string[]? filecontents = null;` (assigned
  before any reachable `return`, since every catch branch rethrows).
- `CtfIncidenceList.cs`: `TryDequeueIncidence(ref Queue<string> lines)` return type changed to
  `CtfIncidence?` (same catch-returns-null pattern); `ReadFileToArray` local likewise made
  `string[]?`. Four call sites that consume the now-nullable `CtfMapEntry.EmailFolder` /
  `CtfIncidence.EmailConversationID` contracts established in Batch A
  (`CtfIncidencePositionAdd` x2, `CTF_Incidence_SET`, `CTF_Incidence_Text_File_WRITE`) use a
  justified `!` null-forgiving operator, since these deprecated legacy methods are only ever
  called with fully-populated `CtfMapEntry`/`CtfIncidence` instances in existing usage — no new
  runtime guard was added (per Scope Invariants, preferring `!` over new guard statements to
  avoid new uncovered executable lines).
- `CommonWords.cs`: pragma only; no annotation changes were required (all methods operate on
  guaranteed non-null string/list parameters with no null-producing code paths).

All 3 Batch B files carry `#nullable enable`. No `<Nullable>` element was added to the csproj
(AC2). No post-condition attribute was added.
