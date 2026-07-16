# Final QA — File-Size Compliance (Issue #328, P4-T6)

Timestamp: 2026-07-15T19-48
Command: wc -l over every touched production and test file
EXIT_CODE: 0

Output Summary:
All touched files are within the 500-line limit, with the single documented pre-existing exception
of `AppToDoObjects.cs`.

Production files (line count):
- UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs        175
- UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs                 443
- UtilitiesCS/OutlookObjects/Store/StoresWrapper.Filtering.cs       108
- UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs                  232
- UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs        477
- UtilitiesCS/OutlookObjects/Store/IStoreWrapperViewer.cs            29
- UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.cs            133
- UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.Designer.cs   322
- ToDoModel/Data Model/Tree/TreeOfToDoItems.cs                      494
- ToDoModel/Data Model/ToDo/ToDoEvents.cs                           467  (was 594 at baseline; P2-T3 deletion + P2-T10 relocation)
- ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs                 102  (new partial)
- ToDoModel/Data Model/Project/ProjectData.cs                       390
- TaskMaster/Ribbon/RibbonController.cs                             270
- TaskMaster/AppGlobals/AppToDoObjects.cs                           503  (documented pre-existing exception)
- TaskMaster/Ribbon/TryFunctionalityInConstruction.cs              297

Test files (line count):
- UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs                     431
- UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.StoreIdExclusion.cs    222
- UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs            486
- UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs                      285
- UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ExcludeStore.cs  164
- ToDoModel.Test/Data Model/Project/ProjectDataCoverageExpansionTests.cs          320
- ToDoModel.Test/Data Model/StoreFilterRoutingTests.cs                            211
- TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs                         275  (P4-T4 get_StoresWrapper case)

Result: PASS.
- `ToDoEvents.cs` (467) and `ToDoEvents.Filtering.cs` (102) are both <= 500 after the P2-T3 deletion of
  the two dead methods and the P2-T10 relocation.
- `AppToDoObjects.cs` is 503 lines, equal to its pre-#328 baseline (503). P2-T6 changed only two
  `ProjectData.Rebuild` call-site arguments in it (no line added), so the file has NOT GROWN beyond its
  documented 503-line baseline. This is the single documented exception per the plan.
- No file other than the documented `AppToDoObjects.cs` exception exceeds 500 lines.
