# Debt 2 — PeopleScoDictionaryNew.cs `#nullable disable`/`#nullable enable` Island Decision

Timestamp: 2026-07-20T00-52

## Outcome: (b) — island retained (reverted after test)

The `#nullable disable` (original line 29) / `#nullable enable` (original line 32) island
wrapping the `PeopleScoDictionaryNew` class declaration line was removed and the solution was
rebuilt in isolation (`MSBuild.exe UtilitiesCS/UtilitiesCS.csproj /t:Rebuild
/p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`).

**Result: CS8644 reappeared** — 12 distinct interface-member nullability-mismatch errors, e.g.:

- `'PeopleScoDictionaryNew' does not implement interface member
  'ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string>)'. Nullability of
  reference types in interface implemented by the base type doesn't match.`
- Six more `ICollection<KeyValuePair<string,string>>` members (`Contains`, `CopyTo`,
  `IsReadOnly`, `IsReadOnly.get`, `Remove`), two `IDictionary<string,string>` members (`Add`,
  `Remove`), and three `ISmartSerializable<ScoDictionaryNew<string,string>>` members
  (`Deserialize<U>`, `Deserialize<U>(..., Func<...>)`, `DeserializeAsync<U>(..., Func<...>)`).

`EXIT_CODE: 1` (44 Error(s) total on this test rebuild, 12 CS8644 plus the batch's own
not-yet-forgiven CS8604 residual — the CS8644 count and identity is the load-bearing finding).

## Root-cause confirmation (why #366 did not resolve this)

Verified via direct source inspection that both of `PeopleScoDictionaryNew`'s base types remain
nullable-oblivious (no `#nullable enable` pragma anywhere in either file):

- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs` —
  confirmed via `grep -n "^#nullable"` returning no matches. `#366`'s `where TKey : notnull`
  constraint IS present (confirmed at the class declaration), but the file itself carries no
  nullable-context pragma, so its interface-implementing members are still emitted under the
  oblivious nullable context.
- `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Dictionary/ConcurrentObservableDictionary.cs`
  (the deeper base) — same confirmation, no `#nullable enable` pragma present.

Because BOTH base-type files remain nullable-oblivious, an oblivious base's interface-member
implementations (e.g., `ICollection<KeyValuePair<string,string>>.Add`) are compiled with
oblivious (non-annotated) parameter/return nullability. When `PeopleScoDictionaryNew`'s own
class-declaration line is placed in an ENABLED nullable context (by removing the island), the
compiler must reconcile the derived type's enabled-context interface list against the oblivious
base's already-compiled oblivious member signatures — and flags the mismatch as CS8644. This is
exactly the mechanism the original island comment (lines 22–28, preserved verbatim) already
documented, and it is confirmed still accurate: `#366`'s merged work added the `notnull`
constraint but did not add a project-wide or file-wide `#nullable enable` context to either base
type, so the original justification for this island has NOT been superseded.

## Decision

**The island is retained.** Lines 29 (`#nullable disable`) and 32 (`#nullable enable`) were
re-inserted immediately after the test rebuild confirmed outcome (b); `git diff
UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNew.cs` confirms the class-declaration
region (lines 22–32) is byte-identical to its pre-task state — only the two unrelated P2-T13
null-forgiving fixes (`AddMissingEntries`'s `helper.Sender!`, `RefineValidateCategory`'s
`InputBox.ShowDialog(...)!`/`DefaultResponse: newPerson!`) remain as this session's actual net
change to the file.

## Cross-reference for Phase 5 (P5-T3, AC6)

This finding is cross-referenced for inclusion in `spec.md`'s Maintainer Decision Summary
(P5-T3): `#366`'s `where TKey : notnull` constraint merge was insufficient to resolve
`PeopleScoDictionaryNew.cs`'s pre-existing `#nullable disable`/`#nullable enable` island, because
neither `ScoDictionaryNew<,>` nor its base `ConcurrentObservableDictionary<,>` carry a
project-wide or file-wide `#nullable enable` pragma — a full resolution would require opting one
or both of those base-type files into an enabled nullable context, which is out of scope for this
capstone (would touch shared `ReusableTypeClasses` infrastructure well beyond this batch's
file-scoped remediation and beyond the already-merged `#366` child's own scope lock).
