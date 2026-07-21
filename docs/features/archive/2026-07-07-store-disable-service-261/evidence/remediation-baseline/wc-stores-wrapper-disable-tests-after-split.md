# Post-Split Line Count — StoresWrapperDisableTests.cs (New File)

- Timestamp: 2026-07-08T00-35
- Command: `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperDisableTests.cs').Count`
  (corrected form of the plan-specified `Measure-Object -Line` command; see
  `wc-stores-wrapper-tests-before.md` for the blank-line undercount rationale)
- EXIT_CODE: 0
- Output Summary: **368** lines. Under the 500-line cap (projected 361 in the plan; actual 368, a
  7-line variance attributable to exact formatting/brace placement, not a functional difference).
  Contains: usings/namespace/class boilerplate, the 6 moved `InclusionFilters_*` methods, the
  disabled-store comment + 5 moved `[TestMethod]` blocks, and the moved `AssertInclusionDecision`
  helper plus duplicated `CreateGlobalsWithStores`, `CreateStore`, and
  `CreateRootFolderWithPrimarySmtpAddress` helpers, per the plan.
