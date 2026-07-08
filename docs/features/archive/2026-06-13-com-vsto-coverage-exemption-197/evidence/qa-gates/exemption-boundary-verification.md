# Exemption Boundary Verification (P7-T7) — Design Memo §2

Timestamp: 2026-06-13T14-40

Method: source-level grep for `[ExcludeFromCodeCoverage]` on each enumerated type/member, plus
package/class presence cross-check against the post-change deduped Cobertura
(coverage/coverage.final.firstparty.cobertura.xml).

## (a) Enumerated COM/VSTO/WinForms exempt set — attribute present AND absent from post-change denominator

TaskVisualization assembly (memo §2.1): excluded via coverage.config + TaskMaster.runsettings ModulePath; package ABSENT from post-change denominator. CONFIRMED.

TaskMaster (memo §2.2) — class-level attribute = 1, absent from denominator = yes:
- ThisAddIn (single attr on partial type, code-behind), AddInUtilities, RibbonViewer, TryFunctionalityInConstruction, RibbonController, AppItemEngines.

ToDoModel (memo §2.3) — class-level attribute = 1, absent from denominator = yes:
- FileOperationsPST, ToDoSynchronizer, ToDoEvents, TreeOfToDoItems, ProjectController, ProjectViewer.
- IDList: 4 method-level attributes (2 Outlook ctors + 2 RefreshIDList overloads). IDList class REMAINS in denominator (method-granularity), GetNextToDoID measured.

QuickFiler (memo §2.4) — class-level attribute = 1, absent from denominator = yes:
- Controllers: QfcDatamodel, EfcItemController, QfcExplorerController, KeyboardHandler, EfcFormController, QfcCollectionController.
- Viewers: EfcViewer, QfcFormViewer, QfcItemViewer, QfcItemViewerExpanded, QfcItemViewerExpandedLight, QfcItemViewerLightSelected, QfcItemViewerV1, ItemViewer.

Tags (memo §2.5) — class-level attribute = 1, absent from denominator = yes:
- TagLauncher, CheckBoxController (Tags/Helper Classes/CheckBoxController.cs — the compiled WinForms CheckBox event handler in Tags.csproj; the non-compiled root Tags/CheckBoxController.cs left untouched and is not in the assembly/denominator).

All 28 enumerated class targets carry exactly one class-level attribute; IDList carries 4 method-level attributes. All exempt classes are ABSENT from the post-change first-party denominator (verified: 0 class-name matches each).

## (b) Enumerated testable seams — NOT annotated AND present in post-change denominator

Source attribute count = 0 for each; present in post-change denominator (>=1 class match each):
- TaskMaster: AppFileSystemFolderPaths(1), AppStagingFilenames(1), AppEvents(1), ApplicationGlobals(1), AppToDoObjects(1), AppQuickFilerSettings(1), AppOlObjects (not annotated), AppAutoFileObjects (not annotated).
- ToDoModel: IDList.GetNextToDoID (method unannotated; IDList class present=1), ToDoLoader(1), ProjectEntry(1), BaseChanger(1), ToDoDefaults (not annotated), PrefixItem (Tags).
- QuickFiler: KbdActions<TKey,UClass,VDelegate>(1), KaChar(1), KaKey(1), KaStringAsync(1), KaCharAsync/KaKeyAsync (not annotated), QfcHighConfidencePreFilter (class NOT annotated; only its pre-existing nested FolderScoringService adapter is exempt, predating this feature), QfcFormController(1), ConversationResolver(2), EfcDataModel(1), FilerQueue(1), FilerQueueItem (not annotated), QfcQueue(1), QfcItemGroup(1).
- Tags: TagController(1, pure-logic methods GetSelections/FilterArchive/ResolvePrefix/ToggleChoice/LoadSelections/LoadControls measured), PrefixItem(1).

All testable seams are NOT class-level annotated and remain present and measured in the post-change denominator.

## Verdict
PASS — exempt/non-exempt boundary matches design memo §2 exactly. No testable seam was exempted; no enumerated COM/VSTO/WinForms target was missed.
