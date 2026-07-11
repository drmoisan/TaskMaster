---
name: 307-f2-scocollection-deletion-gate
description: F2 #307 ScoCollection/ScoStack deletion gate — full first-party reference set incl. tests beyond the spec list, and the ISubjectMapSco/IScoCollection F5 boundary
metadata:
  type: project
---

Epic swordfish-removal child F2 (#307) deletes concrete `ScoCollection.cs`/`ScoStack.cs`. The
spec's §7 test list is INCOMPLETE for the grep-clean deletion gate. A repo-wide
`rg "ScoCollection<|ScoStack<"` also surfaces these first-party references that MUST be re-pointed
or deleted before deletion: `SubjectMapSco_Tests.cs`, `ScoCollectionTests.cs`,
`ScoCollectionTests_UnfinishedStubs.cs` (all commented), `EmailDataMiner_TestSupport.cs`
(`IToDoObjects.PrefixList`/`LoadPrefixList` mock), `ClassifierGroups_Tests.cs:883`
(`new ScoCollection<IPrefix>`).

**Why:** the AC requires no first-party `ScoCollection<`/`ScoStack<` reference outside F5-reserved
interface files before deleting the concrete classes; missing any of these blocks the gate.

**How to apply — F5 boundary that is safe to leave:** `IScoCollection.cs`/`IScoCollection2.cs`
(F5) are SEPARATE files from the concrete `ScoCollection.cs`; deleting the concrete class leaves
them intact. `UtilitiesCS/Interfaces/IToDo/ISubjectMapSco.cs` inherits F5 `IScoCollection<SubjectMapEntry>`
and is used only as a parameter type at `QfcExplorerController.cs:275`. `SubjectMapSco` does NOT
declare `: ISubjectMapSco` (only `: ScoCollection<SubjectMapEntry>`), so re-basing `SubjectMapSco`
onto the clean collection does NOT force implementing the F5 interface. Leave `ISubjectMapSco`,
`IScoCollection*`, and the `QfcExplorerController` param untouched. Also: the injectable FS/Prompt
seams `IScoCollectionFileSystem`/`IScoCollectionPrompt` live INSIDE `ScoCollection.cs`, so deleting
it removes them — the clean `ConcurrentObservableCollection<T>` must supply replacement seams and
`SubjectMapSco_Tests.cs` must re-point to them.
