# P7-T1 — ScoCollection / ScoStack Deletion Gate

Timestamp: 2026-07-11T00-25
Command: `rg -n "ScoCollection<|ScoStack<" --glob '**/*.cs'` (executed via the Grep tool; `/bin/` and `/obj/` excluded)
EXIT_CODE: 0

## Output Summary

After the Phase 3–5 re-pointing, the P7-T2 moot-assertion rewrite, and the Phase 7 deletions
(ScoCollection.cs, ScoStack.cs, and the four direct legacy tests), the gate returns exactly two
hits, both in the F5-reserved interface files that F2 must NOT modify:

- `UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection.cs:14`
  (`public interface IScoCollection<T> : IConcurrentObservableBase<T>, IList<T>, IList`).
- `UtilitiesCS/Interfaces/IToDo/ISubjectMapSco.cs:7`
  (`public interface ISubjectMapSco : IScoCollection<SubjectMapEntry>`, inherits the F5 interface).

No first-party production or test reference to the concrete `ScoCollection<`/`ScoStack<` types
remains. `IScoCollection2.cs` (F5) exists but is a separate interface file and does not reference
the concrete `ScoCollection<`/`ScoStack<` tokens. The solution builds clean (EXIT 0) after deletion.

## Gate History

- Pre-deletion inventory confirmed every non-F5 hit was confined to the deletion set
  (ScoCollection.cs, ScoStack.cs, ScoCollection_Tests.cs, ScoStack_Tests.cs, ScoCollectionTests.cs,
  ScoCollectionTests_UnfinishedStubs.cs) plus the single moot assertion in
  SmartSerializableStatic_Tests.cs (rewritten in P7-T2). No unexpected live consumer existed.
- Post-deletion re-run (above) is clean: only the two F5-reserved interface files remain.
