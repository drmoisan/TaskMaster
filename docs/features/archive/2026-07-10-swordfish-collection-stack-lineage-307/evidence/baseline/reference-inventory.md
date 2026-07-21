# Phase 0 — Baseline Reference Inventory (P0-T2)

Timestamp: 2026-07-11T03-04
Command: `rg -n "ScoCollection<|ScoStack<" --glob '**/*.cs'`
EXIT_CODE: 0

Output Summary: 151 total matching lines across the repository at baseline. These establish the
deletion-gate starting set for Phase 7. The matches fall into these categories:

- F5-reserved interface files (MUST remain after Phase 7):
  - `UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection.cs:14`
  - `UtilitiesCS/Interfaces/IToDo/ISubjectMapSco.cs:7` (inherits `IScoCollection<SubjectMapEntry>`)
  - (`IScoCollection2.cs` present in tree; not matched by this grep line count but reserved.)

- Interface members to retype (F2 scope): `IToDoObjects.cs:25-26` (PrefixList/LoadPrefixList),
  `IAppAutoFileObjects.cs:31-32` (MovedMails/Filters).

- Legacy concrete types to delete (Phase 7): `ScoCollection.cs:55`, `ScoStack.cs:9`.

- Subclasses/consumers to re-base (Phases 3-5): `CtfMap.cs:10`, `SubjectMapSco.cs:24,74`,
  `AppToDoObjects.cs:388-396`, `AppAutoFileObjects.cs:176-186,461-494`,
  `OlFolderClassifierGroup.cs:120-140`, `RecentsList.cs:11` (dead — delete Phase 6),
  `SortEmail.cs:554`, `QfcFormController.cs:85`, `QfcDatamodel.cs:140`,
  `QfcCollectionController.cs:2204`, `QfcDatamodel`/`QfcCollectionController` interfaces.

- Test files to migrate/delete (Phases 3-7): `ManageFiltersControllerTests.cs`,
  `SubjectMapSco_Tests.cs`, `ScoStack_Tests.cs`, `ScoCollection_Tests.cs`,
  `ScoCollectionTests.cs`, `ScoCollectionTests_UnfinishedStubs.cs`, `EmailFiler_Tests.cs`,
  `EmailFiler_TestSupport.cs`, `EmailDataMiner_TestSupport.cs`, `ClassifierGroups_Tests.cs:883`,
  `SmartSerializableStatic_Tests.cs:41`.

Phase 7 deletion gate target: only the F5-reserved interface files
(`IScoCollection.cs`, `IScoCollection2.cs`, `ISubjectMapSco.cs`) may retain `ScoCollection<`
references after all re-points complete.
