# Fail-Before Route Selection (Issue #751, P1-T1)

Timestamp: 2026-09-03T14-33

## Selected route

**Route 2 of the `spec.md` Test Strategy** — the `no-fail-before-rationale` dossier paired with the
repeat-run stress record (`spec.md:299-308`).

Exactly one route is selected. **Route 1 is not executed**, and **no instrumentation is added to any file at
any point in this plan.**

## The `spec.md` condition invoked

`spec.md:299-300` conditions route 2 on route 1 being judged to exceed a specific bar:

> 2. **Fallback — documented rationale plus stress substitute.** If route 1 is judged to exceed the change
>    budget or the remaining line headroom, author a `no-fail-before-rationale` dossier under [...]

The invoked condition is **"exceeds the change budget or the remaining line headroom"**. It is invoked on
the measured facts below, not as a general preference for the cheaper route.

### Measured line headroom (recorded by P0-T10)

| File | Pre-change lines | Cap | Headroom |
|---|---|---|---|
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 492 | 500 | **8** |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 490 | 500 | **10** |

Route 1's instrumentation would land in the second file, which has 10 lines of headroom, and this plan's
own change already consumes part of the first file's headroom.

## The five reasons

Transcribed from the "Selected fail-before route" section of
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/plan.2026-09-03T11-48.md`:

1. Route 1 requires temporary instrumentation that defers the fixture's increment past the assertion point.
   Every deterministic deferral mechanism available at that site is barred by a constraint this plan must
   hold. A wall-clock deferral is a banned determinism API under `.claude/rules/general-unit-test.md`
   ("Determinism Infrastructure") and under spec AC8. A gate-object deferral requires a new field or a new
   `TaskCompletionSource` in `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs`,
   which stands at 490 of 500 lines and whose remaining headroom spec AC7 protects. A deferral that merely
   biases the scheduler is not deterministic, so it does not produce a reliable red.

2. The red route 1 would produce is manufactured by an edit that changes the fixture's own ordering. It
   demonstrates that deferring the increment makes an unsynchronised read observe 0, which restates the race
   rather than reproducing it, and the artifact is not reproducible from the landed tree because the
   instrumentation is reverted.

3. `spec.md` conditions route 2 on route 1 being "judged to exceed the change budget or the remaining line
   headroom". Reason 1 is exactly that condition, and this record notes it as the invoked condition rather
   than as a general preference.

4. Route 2 has two in-repo precedents named by `spec.md`: the dossier shape at
   `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/no-fail-before-rationale.2026-09-02T10-30.md`
   and the repeat-run shape at
   `docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/repeat-run-comparison.2026-08-08T17-03.md`.
   Both files were confirmed present in this worktree during plan authoring, and both were re-confirmed
   present in this worktree by this task before the route was recorded.

5. Route 2 is strengthened here beyond the minimum: P0-T15 captures a **pre-fix** three-run series and P3-T2
   captures a **post-fix** five-run series under the same CI-shaped invocation, so the dossier's claim about
   pre-fix behaviour is backed by a recorded pre-fix observation rather than asserted. That observation is
   allowed to come out either way. If all three pre-fix runs are green, the dossier's negative claim ("a
   natural red is not reliably producible") stands and is evidenced. If any of the three records the target
   test as `Failed`, P0-T15's `NATURAL_RED_OBSERVED` branch applies: that is a genuine natural fail-before on
   the unmodified tree, it falsifies the negative claim, and P1-T2 must then cite the preserved TRX as the
   fail-before evidence instead of asserting non-reproducibility. The route selection is unaffected in either
   case, because route 1's defining act — adding temporary instrumentation — is still not performed.

## Observed P0-T15 branch

**All-green branch.**

The pre-change three-run series recorded by P0-T15 ran the target test
`TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` three times under the identical CI-shaped
invocation, and recorded it as `Passed` on all three runs (408 of 408 tests passing in each run, with zero
failures). **No natural red was observed in the pre-change series.**

The `NATURAL_RED_OBSERVED` branch was therefore not taken. Accordingly this record does not assert that a
natural red was subsequently observed, and P1-T2 writes its `WhyFailingRunImpossible:` section under the
all-green branch: it gives the mechanism for non-reproducibility and cites the P0-T15 observation as the
recorded evidence, together with the PR #746 CI failure as the historical red-before this defect actually
produced.

## Vacuous satisfaction of the AC6 instrumentation clause

Because route 2 is selected and route 1 is not executed, no instrumentation is added to any file. The AC6
clause "If route 1 is taken, no instrumentation remains in the branch diff" is satisfied vacuously, and is
additionally covered mechanically by the footprint gates P4-T8 and P4-T10.
