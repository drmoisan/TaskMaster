# Phase 0 — Pre-change line counts of the Write Set source files

Timestamp: 2026-09-09T13-58

Task: [P0-T13]

Each count was measured with the Read tool by reading the file tail and identifying the number of
the last content line.

LINES: 399 UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
LINES: 184 UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
LINES: 305 UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
LINES: 373 UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
LINES: 61 QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
LINES: 325 QuickFiler/Viewers/QfcFormViewer.cs
LINES: 175 QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs
LINES: 178 QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
LINES: 346 QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
LINES: 459 QuickFiler/Viewers/BreadcrumbDropDownHost.cs

MAX-LINES: 459

CEILING-HEADROOM: QuickFiler/Viewers/BreadcrumbDropDownHost.cs at 459 lines, 41 lines of headroom
to the 500-line ceiling. No other measured file reaches 450 lines; the next largest is
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs at 399, which is 101 lines below the
threshold this field reports on.

All ten counts agree with the figures the plan's "Pre-derived measurements" section records, so no
divergence needs carrying into a later task.

Output Summary: Ten Write Set source files measured. Largest is
QuickFiler/Viewers/BreadcrumbDropDownHost.cs at 459 lines, the only file at or above 450 and 41
lines under the ceiling. All counts match the plan's pre-derived figures.
