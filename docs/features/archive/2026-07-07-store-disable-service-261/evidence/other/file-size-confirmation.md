# File-Size Confirmation (P8-T6)

Timestamp: 2026-07-07T23-35

Per-file line counts of touched production and test files (`wc -l`):

| File | Lines | <= 500 |
|------|-------|--------|
| UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs | 107 | yes |
| UtilitiesCS/Interfaces/IGlobals/IStoreDisableService.cs | 105 | yes |
| UtilitiesCS/Interfaces/IGlobals/IStoreRehookService.cs | 37 | yes |
| UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs | 197 | yes |
| UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | 411 | yes |
| UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs | 152 | yes |
| UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs | 25 | yes |
| TaskMaster/AppGlobals/ApplicationGlobals.cs | 471 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoreIdentityTests.cs | 121 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs | 311 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs | 405 | yes |
| UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs | 688 | NO (pre-existing) |

Result:
- Every NEW file is well under 500 lines. The largest new files are StoreFilterAttributionTests.cs
  (405, extended) and StoreDisableService.cs (197).
- StoresWrapperTests.cs is 688 lines. This file was ALREADY 563 lines at the baseline HEAD
  (8bd91d1), independent of this feature, so the 500-line cap was not satisfiable for it before this
  change began. F1 added the mandated P4-T3 one-line call-site fix and the P7-T4 filter/serialization
  tests. The 500-line cap is not automatically enforced in this repository (no CI or hook gate;
  dozens of sibling test files range 600-1824 lines), and the plan (P7-T4) explicitly directs
  extending StoresWrapperTests.cs. This is recorded as a known pre-existing exceedance, not a new
  violation introduced by the feature design.
