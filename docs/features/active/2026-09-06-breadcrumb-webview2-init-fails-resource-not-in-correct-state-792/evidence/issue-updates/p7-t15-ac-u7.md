# [P7-T15] AC-U7 check-off

- Issue: #792
- Timestamp: 2026-09-17T21-18
- Command: edit `spec.md` line 312 from `- [ ] AC-U7:` to `- [x] AC-U7:` (checkbox only); then `Select-String -LiteralPath $FEATURE/spec.md -Pattern '^- \[x\] AC-U7:'` and `'^- \[ \] AC-U7:'`; byte comparison of the text after the checkbox against the same line of `git show "${SpecRefSha}:$FEATURE/spec.md"` with `$SpecRefSha` bound by CMD-BASE (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, console encoding UTF-8; the helper's opening branch assertion passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `AC-U7: line 312 | checked=1 open=0 | ref-state '- [ ]' | text-byte-identical-to-ref=True`; `SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4`.
- PostedAs: none (local `spec.md` check-off only; nothing was posted to GitHub)

## Check-off line (verbatim, `spec.md:312`)

```
- [x] AC-U7: The breadcrumb outbound queue is not left to grow without bound after a failed initialization: a failure notification drains or discards it explicitly, and a test asserts its pending count is zero afterwards.
```

## Evidence the check-off rests on

- [P3-T7] `evidence/regression-testing/p3-t7-fail-before.md` — observed failing: the queue tests (`DiscardPending_ReturnsTheDiscardedCountAndLeavesZeroPending`, `DiscardPending_OnAnEmptyQueue_ReturnsZero`, `NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting`) failed on their pre-predicted assertions on the unfixed tree.
- [P4-T11] `evidence/regression-testing/p4-t11-pass-after.md` — pass-after: `Total tests: 234`, `Passed: 234`.
- [P5-T4] `evidence/regression-testing/p5-t4-ac-u7-mutation.md` — non-vacuity: with the discard skipped, the pre-predicted `PendingCount` assertion failed (expected 0, found 2); restoration returned `Passed: 1`.
- Final pass: [P7-T8] gate 3 records the three `DiscardPending` statements (`BreadcrumbOutboundQueue.cs:75-77`) and the router's `_outboundQueue.DiscardPending()` call (`BreadcrumbBridgeRouter.cs:355`) covered.
