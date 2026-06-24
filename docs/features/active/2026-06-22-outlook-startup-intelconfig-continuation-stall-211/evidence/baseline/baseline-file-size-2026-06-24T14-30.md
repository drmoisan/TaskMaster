# Baseline File Sizes (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30
Command: `(Get-Content <path>).Length` (equivalent `wc -l` used in git-bash)
EXIT_CODE: 0

Output Summary:
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` = 267 lines (<= 500 OK; will be MODIFIED with Stopwatch instrumentation).
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` = 563 lines (pre-existing; NOT modified by this plan — the new tests go in a separate file; this plan does not touch its content).
- `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs` = does not exist yet (0 lines; will be CREATED).
- `UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs` = does not exist yet (0 lines; will be CREATED).

Acceptance: existing counts recorded; StoresWrapper.cs <= 500. The two new files confirmed non-existent (0 lines). StoresWrapperTests.cs is over 500 but is a pre-existing file whose content is out of this plan's modification scope (only Compile-Include wiring of the new test file is added to the csproj).
