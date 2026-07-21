# Batch 5 — Nullable Pragma Gate (P5-T3)

Timestamp: 2026-07-19T10-04

## Commands

1. `dotnet tool run csharpier format .` — EXIT_CODE 0 (clean).
2. Pragma gate: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`), isolated-compile methodology per P0-T5.

## Output Summary

Batch 5 (4 files: LockingLinkedListNode, LockingLinkedList, LockingObservableLinkedListNode,
LockingObservableLinkedList) cluster diagnostics:
- CS86xx count attributed to `ReusableTypeClasses/`: 0 (AC1 for Batch 5)
- CS8714 count: 0
- Pre-existing non-cluster UtilitiesCS TWAE errors: 14 (unchanged; out of scope)
- The isolated compile builds the whole UtilitiesCS assembly, so the 0 cluster CS86xx count
  confirms all 32 Batch 1-5 pragma'd files are simultaneously clean.

Annotations applied (annotation/null-safety only; locking behavior unchanged):
- `LockingLinkedListNode` / `LockingObservableLinkedListNode`: nullable node-graph links
  (`list`/`next`/`prev`/`innerNode` fields), nullable `List`/`Next`/`Previous`, nullable internal
  ctor `list` param, justified `!` on `this.list!` in the four Move methods (a detached node throws,
  as before).
- `LockingLinkedList`: nullable `First`/`Last`/`Find`/`FindLast` and `ToLocking` (empty list /
  not-found return null), `T? TryTakeFirst()` and `T[]? TryTakeFirst(int n)`, justified `!` on
  `innerNode` args to base LinkedList methods and on `node!.Value`/`base.First!.Value`/`base.Last!.Value`
  (RemoveFirst/RemoveLast throw on empty, so the node is present when read).
- `LockingObservableLinkedList` (528 lines; pre-existing >500 — NOT split): nullable
  `CollectionChanged` event, nullable `OnCollectionChanged` node params and `newNode`/`oldNode`
  locals, nullable `First`/`Last`/`Find`/`FindLast`/`ToLocking`, `T? TakeFirst()`/`T? TakeLast()`,
  `T[]? TryTakeFirst(int n)`/`T[]? TryTakeLast(int n)`, justified `!` on `innerNode` derefs and on
  count-guarded `TakeFirst()!`/`TakeLast()!`, nullable-value sentinel `KeyValuePair<Node, HashSet?>`
  in `RemovePartialObserver(params)` (filtered before the returned dictionary).
- `LockingObservableLinkedListChangedEventArgs` (Batch 1 file, already pragma'd): constructor
  `newNode`/`oldNode` params made nullable to match the nullable properties and the callers that
  pass a null node for Add/Remove/Reset; Batch 1 remains zero-CS86xx.

No `System.Diagnostics.CodeAnalysis` post-condition attribute was added; no file was split.
