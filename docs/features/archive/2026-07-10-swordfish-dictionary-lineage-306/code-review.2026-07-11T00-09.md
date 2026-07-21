# Code Review — swordfish-dictionary-lineage (F1, Issue #306)

- Timestamp: 2026-07-11T00-09
- Branch: feature/swordfish-dictionary-lineage-306 (HEAD a0073fc1)
- Base: origin/epic/swordfish-removal-integration
- Reviewer: feature-review agent (review-only)

## Executive Summary

The change is a disciplined, mechanical migration from the Swordfish-based
`ScoDictionary<TKey,TValue>` to the Swordfish-free `ScoDictionaryNew<TKey,TValue>`
across nine production files, with fixture migrations across fourteen test files and
a new 219-line on-disk compatibility test suite. Construction is correctly reconciled
from the legacy self-loading `(filename, folderpath)` constructor to the factory
`Static.Deserialize(fileName, folderPath)` path, and persistence uses the default
`Serialize()`/`SerializeToString()` route rather than the incompatible globals
converter path. Two consumer adaptations (`.Remove` -> `.TryRemove`) and one
determinism fix (`FolderScorer` ordinal tie-break) are correct and behavior-preserving.
One latent bug is fixed as a side effect (the `Decoder` null-encoder branch previously
dereferenced a null `_encoder`). No blocking code-quality findings.

Overall code-review verdict: PASS. Blocking findings: 0.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | SubjectMapEncoder.cs | Decoder getter, lines 38-44 | Null-encoder branch changed from `_encoder.Deserialize()` (NRE on null) to `_encoder = ScoDictionaryNew<...>.Static.Deserialize(_filename,_folderpath)`. Latent bug fixed; now fails fast with ArgumentNullException when filename/folderpath are unset. | None required. | Correct fail-fast; the migrated test asserts the new exception type. | SubjectMapEncoder.cs:38-44; SubjectMapEncoder_Tests.cs Decoder_WhenEncoderIsNull_ThrowsArgumentNullException |
| Info | SubjectMapEncoder.cs | RebuildEncoding, lines 127-135 | Legacy `new ScoDictionary<>(words, filename, folderpath)` replaced by `new ScoDictionaryNew<>(words)` + `Config.Disk = new FilePathHelper(...)` + `Serialize()`; `_encoder.ToDictionary()` replaced by `new Dictionary<string,int>(_encoder)`. | None required. | Behavior-equivalent construct-set-disk-serialize sequence; duplicate-key rebuild path preserved. | SubjectMapEncoder.cs:115-135 |
| Low | SortEmail.cs | lines 205, 440 | `await Encoder.Encoder.SerializeAsync()` replaced by synchronous `Encoder.Encoder.Serialize()` (new type has no async serialize). The write now runs synchronously on the calling thread inside an `async` method. | Accept; optionally add an async serialize overload to `ScoDictionaryNew` in a later feature if the synchronous write becomes a hot path. | Mechanically necessary and consistent with the spec's plain-`Serialize()` contract; persist-on-completion semantics preserved; methods are `[ExcludeFromCodeCoverage]` UI-orchestration paths. | SortEmail.cs:202-205, 437-440; spec.md API/CLI Surface |
| Low | FolderScorer.cs | ToArray()/ToArray(int), lines 238-255 | Ordinal key tie-break added to restore deterministic ordering lost when moving to the ConcurrentDictionary-backed type. Directly asserted only via equal-score array-load tests; no dedicated "explicit tie" unit test with a named assertion of the tie rationale. | Optional: add a focused test constructing two equal-score folders and asserting ordinal order, to lock the tie-break contract independently. | The two `LoadFromField_...FolderKeyArray` tests already assert `Equal("Archive\\Finance","Archive\\Ops")` (an equal-score ordinal pair), so the branch is covered; a named test would improve intent clarity. | FolderScorer.cs:238-255; FolderScorerTests.cs:478-520 |
| Info | AppToDoObjects.cs | DictRemap/FilteredFolderScraping/FolderRemap loaders, lines 292-465 | Self-loading constructors replaced by `Static.Deserialize`; lazy `Initialized`/`Initializer.GetOrLoad` patterns preserved; property/field types moved to the new lineage. | None required. | Faithful to spec; no globals path; interface member types match implementers. | AppToDoObjects.cs diff |
| Info | FilterOlFoldersController.cs / FolderRemapController.cs | removal loops | `.Remove(x)` -> `.TryRemove(x, out _)` inside `.ForEach`. | None required. | Behavior-preserving key removal against the new concurrent API; covered by existing controller tests. | controllers diff; FolderRemapController_Tests.cs; FilterOlFoldersController_Tests.cs |

## Design-Principle Assessment

- Simplicity: PASS. No new abstractions; the migration reuses existing lazy-load and
  serialization seams.
- Separation of concerns: PASS. Persistence remains behind `ScoDictionaryNew`; no I/O
  leaks into domain logic. The pre-existing `MessageBox.Show` in the `Decoder`
  duplicate-key rebuild path is untouched by F1 (not a new UI-in-logic violation).
- Error handling: PASS. Fail-fast improved in the `Decoder` null-encoder branch.
- Reusability/extensibility: PASS. Interface members are the extension points and
  changed consistently across implementers and callers.
- Naming/readability: PASS. The `FolderScorer` tie-break carries a why-comment.

## Test Quality Assessment

- Framework compliance: MSTest + FluentAssertions (Moq where mocking is needed). PASS.
- Determinism: no `Thread.Sleep`/`Task.Delay`/wall-clock; embedded string payloads;
  no temp files. PASS.
- The new suite proves both directions (existing flat payload loads with entry
  fidelity; default write path re-emits a flat payload free of the globals wrapper
  tokens) for all four persisted dictionaries plus an all-types default-path assertion.
- Migrated fixtures compile against the new lineage and preserve their original
  scenarios (controller save/discard, EmailDataMiner stubs, EmailDetails/Wrapper).

## Verdict

PASS. No blocking code-quality findings. Two Low advisory items (optional async
serialize overload; optional named tie-break test) do not require remediation.
