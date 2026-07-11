# P8-T2 — Legacy ScoDictionary<> Production Reference Census

Timestamp: 2026-07-11T04-08

Purpose: Determine whether the legacy concrete `ScoDictionary<TKey,TValue>` class
(`UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`) is
production-unreferenced after Phases 1-4, to gate the optional P8-T3 deletion.

Commands:

1. Generic-usage census (concrete class only; excludes `IScoDictionary<`, `ScoDictionaryNew<`,
   all-caps `SCODictionary`, the definition file, build output, packages, test projects, and
   comment-only lines):

   `grep -rnE "\bScoDictionary<" --include=*.cs . | grep -viE "/bin/|/obj/|/packages/|\.Test/|/SCODictionary\.cs:|ScoDictionaryNew|SCODictionary" | grep -vE ":\s*//|^\s*//"`

   Result: no matches.

2. Whole-word census (any reference, generic or not; same exclusions):

   `grep -rnE "\bScoDictionary\b" --include=*.cs . | grep -viE "/bin/|/obj/|/packages/|\.Test/|/SCODictionary\.cs:|ScoDictionaryNew|SCODictionary|IScoDictionary" | grep -vE ":[0-9]+:\s*//"`

   Result: no matches.

Live production references to the concrete legacy `ScoDictionary<>` class (outside its own
definition file `SCODictionary.cs`): NONE.

Notes:
- After Phases 1-4, every production consumer (`IToDoObjects`, `ISubjectMapEncoder`,
  `IEmailDetailsWrapper`, `AppToDoObjects`, `SubjectMapEncoder`, `FolderScorer`, `EmailDetails`,
  `EmailDetailsWrapper`, `SortEmail`) plus the two ripple consumers (`FolderRemapController`,
  `FilterOlFoldersController`) reference only the `ScoDictionaryNew<>` lineage.
- The legacy class IS still referenced by TEST code (e.g., `SCODictionary_Tests.cs`,
  `SCODictionary_Additional_Tests.cs`, and the `SmartSerializable*_Tests.cs` files use it as a
  convenient non-`ISmartSerializable` sample). Deleting the class (P8-T3) would therefore require
  migrating those test files as well.

Gate outcome for P8-T3: production references are zero, so the optional deletion is technically
eligible. Deletion remains OPTIONAL per the plan's Scope Boundary ("SCODictionary.cs deletion is
OPTIONAL (Phase 8), not a goal of F1") and spec Non-Goals; see P8-T3 for the election decision.
