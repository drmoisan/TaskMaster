# remove-unnecessary-interlocked-increment-in-fileio2 (Issue #709)

- Date captured: 2026-08-31
- Author: Dan Moisan

- Status: Promoted -> docs/features/active/remove-unnecessary-interlocked-increment-in-fileio2/ (Issue #709)

- Issue: #709
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/709
- Last Updated: 2026-08-31
## Problem / Why

The retry loop in `UtilitiesCS/To Depricate/FileIO2.cs` increments its attempt counter with `Interlocked.Increment(ref attempts)`. The counter is a method-local captured by the async state machine and is never touched by more than one logical thread, so the interlocked operation guards against contention that cannot occur. It reads as evidence of a concurrency concern that is not present, which is misleading to a later reader.

## Proposed Behavior

Replace `Interlocked.Increment(ref attempts)` with a plain increment, leaving the loop's control flow, the 100-attempt budget, and the 100-millisecond interval unchanged.

## Acceptance Criteria (early draft)

- [ ] `UtilitiesCS/To Depricate/FileIO2.cs` contains zero occurrences of `Interlocked.Increment`.
- [ ] The existing seam-driven tests in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` still assert a writer-factory invocation count of 100 and a delay-delegate invocation count of 99 on the exhaustion path, and still pass.
- [ ] No other behavior of `WriteTextFileAsync` changes.

## Constraints & Risks

- The change is cosmetic. The existing call is unnecessary but harmless, so the value is readability rather than correctness, and the item should not be prioritized above defect work.
- Issue #647 listed replacing this call as an explicit non-goal and deliberately retained it, so the change must not be folded into any in-flight work on that file.
- If `WriteTextFileAsync` is later deleted by the `To Depricate` migration item, this item becomes moot and should be closed rather than executed.

## Test Conditions to Consider

- [ ] Unit coverage areas: the two existing exhaustion-path assertions are sufficient; no new test is required.
- [ ] Integration scenarios: none; the change is local to one method.
- [ ] CLI/API examples: not applicable.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/remove-unnecessary-interlocked-increment-in-fileio2/` folder from the template
