# Phase 0 — Defect-Encoding Tests Green Before Any Change (issue #440, plan task P0-T14)

Timestamp: 2026-08-29T06-28

Command:

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges|FullyQualifiedName~Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft" "/Logger:trx;LogFileName=p0-t14.trx" "/ResultsDirectory:coverage\trx\p0-t14"
```

`$vstest` is the absolute path recorded by P0-T8. The run was issued through
`pwsh -NoProfile` from the repository root.

EXIT_CODE: 0 (expected 0)

## Output Summary

```
  Passed Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges [36 ms]
  Passed Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft [179 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
```

The total is exactly 2, so the filter matched precisely the two intended tests and no
others. No rerun with a corrected filter was required.

## Interpretation

Both tests are green against the unmodified tree at `BASE`. Their later modification
by P2-T3 and P2-T4 is therefore a deliberate correction of assertions that encode the
defect, not the repair of an already-broken test.

The TRX was written to `coverage\trx\p0-t14\p0-t14.trx`, under the gitignored
`coverage/` tree with a task-scoped results directory and an explicit log file name,
so no default account-named or host-named TRX was produced and no raw TRX is copied
under this feature folder.
