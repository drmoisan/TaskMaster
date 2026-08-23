# Phase 0 — Pre-Existing Failing Test Set and Phase 3 Pass Rule (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T13]
Source measurement: `evidence/remediation-baseline/tests-with-coverage.2026-08-08T14-52.md` (P0-T11)

## The recorded pre-existing failing set

The P0-T11 full-suite run reported 6338 total, 6338 passed, 0 failed, 0 skipped.

**The recorded set is EMPTY.** No test failed in the baseline run.

This is the explicit empty case the task text requires be enumerated rather than omitted.

## The #508 flake

`UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` is a **pre-existing order-dependent flake** tracked by issue **#508**. It is **explicitly out of scope for this remediation cycle** and **must not be fixed here**.

It **passed** in the P0-T11 baseline run, which is consistent with its recorded character: the failure is order-dependent, not deterministic, so a single green baseline run neither proves it fixed nor removes it from the known-flake set. Because the plan's section 3 rule 9 names it explicitly as a member of the tolerated set, it is admitted to the pass rule below even though it did not fail in this particular baseline. Admitting it is the conservative reading: it prevents a known, already-routed flake from being misclassified as a regression introduced by this cycle.

## Pass rule for Phase 3, stated verbatim

A Phase 3 test run passes when the only failures are members of this recorded set; any test not in this set that fails is a real regression that restarts Phase 3 at P3-T1; issue #508 must not be fixed in this cycle.

## The set, enumerated explicitly

| # | Fully-qualified test name | Basis for admission |
|---|---|---|
| 1 | `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` | Named explicitly by plan section 3 rule 9 as a pre-existing order-dependent flake tracked by issue **#508**, out of scope. Did **not** fail in the P0-T11 baseline. |

No other test is a member. The P0-T11 observed failure set was empty, so a Phase 3 run with **zero** failures is the expected outcome, and a Phase 3 run whose only failure is entry 1 above is also a pass. Any other failure restarts Phase 3 at P3-T1.
