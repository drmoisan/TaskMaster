---
name: scodictionary-new-enumeration-order-and-removal-api
description: Swordfish ScoDictionary -> ScoDictionaryNew swaps silently change enumeration order and the removal API; watch for missing tie-breaks and .Remove->.TryRemove across the swordfish-removal epic (F1..F5)
metadata:
  type: project
---

The swordfish-removal epic re-points `ScoDictionary<TKey,TValue>` (Swordfish
`ConcurrentObservableDictionary`, insertion-ordered enumeration) to
`ScoDictionaryNew<TKey,TValue>` (clean ConcurrentDictionary-backed). Two silent
behavior consequences recur and must be checked on every child feature that does a
swap, not just F1 (#306):

1. **Enumeration order changes** from insertion order to non-deterministic
   ConcurrentDictionary bucket order. Any consumer that materializes the dictionary
   via LINQ (`OrderBy...`, `.Select`, `.ToArray`) and relied on stable ordering among
   equal sort keys needs an explicit deterministic tie-break. F1 fixed this in
   `FolderScorer.ToArray()`/`ToArray(int)` with `.ThenBy(x => x.Key, StringComparer.Ordinal)`.
   When reviewing, confirm equal-key ordering is tested (F1's is asserted by
   `LoadFromField_...FolderKeyArray` tests expecting `Equal("Archive\\Finance","Archive\\Ops")`).

2. **Removal API differs**: `ScoDictionaryNew` exposes no public `bool Remove(TKey)`
   accessible on the concrete/interface type, so consumers doing `.Remove(key)` must
   move to `.TryRemove(key, out _)`. This forces edits in consumer files (F1 touched
   FilterOlFoldersController.cs and FolderRemapController.cs, outside the plan
   scope-lock). Treat as in-scope-by-necessity/behavior-preserving, not a scope breach.

Persistence: the compatible on-disk path is `ScoDictionaryNew<...>.Static.Deserialize(fileName, folderPath)`
+ plain `Serialize()`/`SerializeToString()`. The globals path
(`GetSettingsJson<T>(globals)` / `ScoDictionaryConverter` / `PreserveReferencesHandling.All`)
emits an incompatible `$id`/`CoDictionary`/`RemainingObject` wrapper and must never be
used for a persisted dictionary. `ScoDictionaryNew` has no `SerializeAsync`, so
`await ...SerializeAsync()` call sites become synchronous `Serialize()`.

**Why:** these are the load-bearing correctness risks of an otherwise-mechanical type
swap; missing them causes flaky ordering tests or broken persisted-file loads.
**How to apply:** on each swordfish-removal child review, grep changed consumers for
`.Remove(` and for LINQ ordering over the swapped dict, and verify no persisted dict
uses the globals converter path. See [[csharp-coverage-artifact-is-cobertura]] for the
coverage-artifact parsing caveat that also applies here.
