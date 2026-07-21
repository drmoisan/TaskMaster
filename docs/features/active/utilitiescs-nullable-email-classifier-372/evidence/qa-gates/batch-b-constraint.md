# Batch B — DO-NOT-ALTER Constraint Verification

Timestamp: 2026-07-19T01-30

Scope: Batch B corpus/count core (`Corpus.cs`, `CorpusInherit.cs`). Per-file confirmation that only nullability annotations and justified `!` (with `// why` comments) changed; no set-arithmetic, threshold, or control-flow change; no new `if (x is null) throw` guard on any scoring/corpus path.

## Bayesian/Corpus.cs (13 CS86xx closed)
Changes:
- `#nullable enable`.
- `_tokenFrequency` field declaration initialized `= null!` with a `// why` comment. Rationale: two pre-existing legacy concurrency-level constructors (`Corpus(int, IEnumerable, IEqualityComparer)` and `Corpus(int, int, IEqualityComparer)`) construct a `ConcurrentDictionary` but discard it without assigning the field — a pre-existing latent behavior that is PRESERVED. `= null!` assigns the same default-null the field already had while keeping the analyzer's non-null posture; no runtime state change.
- `Clone()`, operator `+`, operator `-`, `SubtractFilter`, and `SubtractAsync`: each `x.Clone() as Corpus` / `MemberwiseClone() as Corpus` wrapped in `( ... )!` with a `// why` comment (Clone/MemberwiseClone on a Corpus always yields a Corpus). No throw added.
- `SubtractAsync(..., SegmentStopWatch? sw = null)`: parameter annotated nullable; the pre-existing `sw ??= new SegmentStopWatch().Start()` already handles the null default (unchanged).

DO-NOT-ALTER confirmation: operator `+`/`-` token-frequency arithmetic, `SubtractAsync`/`SubtractFilter` `TryUpdate`/`TryRemove` flow, and the `negTokenWt`/`minCt` threshold expressions are byte-for-byte unchanged. No post-condition attribute added. No new `if (x is null) throw` guard.

## Bayesian/CorpusInherit.cs (10 CS86xx closed)
Changes:
- `#nullable enable`.
- `_id`/`Id` typed `string?` (reflects the get/set with no initialization).
- `_timer` field typed `TimerWrapper?` (assigned conditionally in `RequestSerialization`).
- `DeserializeJson` return type and its `collection` local typed `CorpusInherit?` (JsonConvert.DeserializeObject may return null).
- `Deserialize(FilePathHelper, bool)`: `dictionary` local typed `CorpusInherit?`; `dictionary!.Serialize()` and `return dictionary!` with `// why` comments explaining all reaching paths leave it non-null (success path throws when DeserializeJson returns null; each catch path assigns CreateEmpty non-null or throws). The pre-existing `if (dictionary is null) throw new InvalidOperationException(...)` guard is unchanged.

DO-NOT-ALTER confirmation: `AddOrIncrementToken`/`DecrementOrRemoveToken` count logic, the lock, serialization control flow, and threshold/count constants are unchanged. No post-condition attribute added. No new `if (x is null) throw` guard beyond the pre-existing one.

Public signature changes are limited to additive nullability annotations reflecting actual null behavior (AC5); annotation/null-safety only (AC3).
