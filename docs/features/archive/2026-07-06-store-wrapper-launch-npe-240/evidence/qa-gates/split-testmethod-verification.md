# Split TestMethod Verification (Remediation Cycle, Issue #240)

- Timestamp: 2026-07-06T12-15
- Command: `Select-String -Path UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests*.cs -Pattern "\[TestMethod\]" | Measure-Object | Select-Object -ExpandProperty Count`
- EXIT_CODE: 0
- Output Summary: Combined `[TestMethod]` count across the three post-split files: 13 (`StoreWrapperController_Tests.cs`) + 19 (`StoreWrapperController_Tests.ButtonAndPopulate.cs`) + 7 (`StoreWrapperController_Tests.Launch.cs`) = **39**, matching the pre-split baseline count of 39 recorded via `git show HEAD:UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs` and the same command against the single pre-split file, and matching the count stated in this plan's "Proposed file split" section.

The sorted list of extracted method names from the pre-split file (via `git show HEAD:...`) and the sorted list of extracted method names across the three post-split files are byte-identical (`diff` produced no output), confirming no test method was dropped, added, or renamed. The specific method-name lists match those enumerated in P1-T1 (19 names), P1-T2 (7 names plus the `StubSelectFolderController` nested class), and P1-T3 (13 names).
