# P0-T10 — Pre-Existing Failing / Flaky Test Set and the Final-QC Pass Rule

Timestamp: 2026-08-08T20-45

Source run: P0-T9, `<FEATURE>\evidence\baseline\tests-with-coverage.2026-08-08T20-44.md`.

## Observed failing or skipped tests in P0-T9

**None.** The P0-T9 run reported `Total tests: 6399 / Passed: 6399` with zero failed and zero
skipped. No test is named here as an observed failure because none occurred.

## Recorded pre-existing flake (in the set regardless of this run's outcome)

- `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`

This test is a pre-existing **order-dependent flake** tracked by issue **#508**. It did **not**
fail in the P0-T9 run, but it is recorded as a member of the pre-existing set regardless, exactly
as the task requires, because its failure is order-dependent and may surface in a later run
without any causal relationship to this change. Issue **#508 is out of scope for this delivery and
must not be fixed inside this change.**

## The final-QC pass rule (stated verbatim, binding on Phase 5)

> A Phase 5 test run passes when the only failures are members of this recorded set; any failure
> outside this set is a real regression that restarts the Phase 5 loop at P5-T1; issue #508 must
> not be fixed inside this change.

The recorded set is therefore exactly:

```
UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict
```

Because the baseline run was fully green, the strictest reading also holds: a Phase 5 run with
zero failures is the expected outcome, and any non-empty failure set other than the single #508
test above is a regression.

Binary outcome: PASS.
