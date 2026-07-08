# Post-Split Line Count — StoresWrapperTests.cs

- Timestamp: 2026-07-08T00-35
- Command: `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs').Count`
  (corrected form of the plan-specified `Measure-Object -Line` command; see
  `wc-stores-wrapper-tests-before.md` for the blank-line undercount rationale)
- EXIT_CODE: 0
- Output Summary: **415** lines. Under the 500-line cap (projected 417 in the plan; actual 415,
  a 2-line variance attributable to exact brace/blank-line placement during the split, not a
  functional difference). All 12 moved members (6 `InclusionFilters_*` methods, the disabled-store
  comment + 5 `[TestMethod]` blocks, and `AssertInclusionDecision`) are confirmed absent; all
  retained test method bodies are textually unchanged.
