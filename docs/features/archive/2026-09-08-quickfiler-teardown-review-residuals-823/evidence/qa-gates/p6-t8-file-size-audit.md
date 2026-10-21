# Phase 6 — 500-line ceiling audit of the Write Set source files

Timestamp: 2026-09-09T14-48

Task: [P6-T8]

Measured with the Read tool on the tree as it stands after the final [P6-T1] formatter pass, by
reading each file tail and identifying the number of the last content line. The nine measured paths
are the [P0-T13] set excluding `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, which is not in the
Write Set and is covered by [P6-T10].

LINES: 408 UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
LINES: 189 UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
LINES: 306 UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
LINES: 453 UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
LINES: 70 QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
LINES: 330 QuickFiler/Viewers/QfcFormViewer.cs
LINES: 181 QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs
LINES: 178 QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
LINES: 353 QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs

Deltas against the corresponding [P0-T13] values:

DELTA: +9 UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
DELTA: +5 UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
DELTA: +1 UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
DELTA: +80 UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
DELTA: +9 QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
DELTA: +5 QuickFiler/Viewers/QfcFormViewer.cs
DELTA: +6 QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs
DELTA: 0 QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
DELTA: +7 QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs

MAX-LINES: 453

The largest file is `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`
at 453 lines, 47 lines below the ceiling. Its +80 delta is the two added tests and their XML docs,
which is the largest single contribution this plan makes to any file.
`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` has a delta of 0 because [P4-T2] replaced one
token inside an existing comment line and added no line.

CEILING: MET

`MAX-LINES` is 453, which is at most 500. No Write Set source file approaches the ceiling closely
enough to require a partial-part split.

Output Summary: Nine Write Set source files measured after the final format pass. Largest is 453
lines, 47 under the 500-line ceiling; the ceiling is met by every file and no split is required.
