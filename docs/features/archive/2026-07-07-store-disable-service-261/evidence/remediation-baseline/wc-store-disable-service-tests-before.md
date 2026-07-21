# Baseline Line Count — StoreDisableServiceTests.cs (Pre-N1-Fix)

- Timestamp: 2026-07-08T00-08
- Command: `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoreDisableServiceTests.cs').Count`
  (corrected form of the plan-specified `Measure-Object -Line` command; see
  `wc-stores-wrapper-tests-before.md` for the `Measure-Object -Line` blank-line undercount
  rationale that applies identically here)
- EXIT_CODE: 0
- Output Summary: **311** lines. File not touched by the R1 split; only the N1 await-fix task
  (P1-T9/P1-T10) modifies it (adds `async`/`await` to two method signatures/statements, net line
  count unaffected by this edit).
