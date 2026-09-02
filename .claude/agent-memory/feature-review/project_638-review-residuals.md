---
name: 638-review-residuals
description: "#638 EFC archive-root guard review: PASS/0 blocking, 20/20 AC; residuals CR-1 NRE-barrier test, CR-2 message-only duplicate throw-condition test, G5 missing promoted records; EfcDataModel.cs 66.2% modified-file FAIL dispositioned non-blocking"
metadata:
  type: project
---

Review of `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638` (2026-08-29, base `ecdb1c84`).
Verdict PASS, **0 blocking**, 20 of 20 acceptance criteria verified against evidence.

**Why:** the outcome and its residuals should not be re-derived if #638 comes back for a second
cycle or if #696/#697/#698 reach review.

**How to apply:**

- **Modified-file coverage FAIL, dispositioned non-blocking at 66.2%.** `QuickFiler/Controllers/EfcDataModel.cs`
  is 188/284 = 66.20% line, well under the 85% modified-file floor and under the >=80% bar that
  [[modified-file-subfloor-nonblocking-disposition-230]] set. Dispositioned non-blocking anyway on
  five grounds: 26 added executable lines all covered; changed-line coverage 93.10% (independently
  recomputed, uncovered lines 366/390 only); baseline arithmetically bounded at 62.40-62.79% so the
  file *improved* 3.4-3.8 points; the residual uncovered mass is COM-bound (`EmailFiler`,
  `FolderPredictor`, `MAPIFolder`, `MailItem`); and spec AC17 makes the blocking clause change-scoped.
  This extends the #230 precedent below 80% — the deciding factor was that every uncovered range
  requires a live Outlook object, verified by grouping uncovered line numbers into ranges and reading
  each range's source.
- **AC17 remediation adjudication technique.** A `raw` vs `koverage-processed` baseline mismatch is a
  denominator artefact (14 packages / 82363 valid vs 9 / 64221) and its delta is meaningless. Judge
  the re-measured baseline by arithmetic, not narrative: package-count equality (9 = 9), denominator
  delta equal to the added executable-line count (`64221-64195 = 26`), and numerator delta sign and
  magnitude (`+67`) consistent with the new tests. All three closed here.
- **CR-1 (promotion candidate).** `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` uses
  `ThrowAsync<NullReferenceException>()` from `EmailFiler.SortAsync` as its stopping barrier. The real
  assertion is the `VerifyGet(Times.Once())`. If `EmailFiler` later null-guards, this test fails
  pointing at the wrong subsystem. It is also the only test covering line 339, so deleting it drops
  changed-line coverage to ~89.7%.
- **CR-2.** The two "both throw conditions" tests differ only by the injected exception's *message*,
  and `TryGetArchiveRoot` dispatches on type — identical production statements, coverage without
  discriminating power. AC9 is still PASS because a stronger test is not constructible at the
  `IOlObjects` seam and `AppOlObjectsArchiveRootValidationTests` already pins the messages upstream.
- **G5 (owed).** The dossier's RESOLVED appendix says the #696/#697/#698 promoted records are under
  `docs/features/potential/promoted/`; no entry for those three exists in the review worktree and its
  tree is clean. Confirm before archiving the feature folder.
- **Caller-directive vs hook conflict, resolved without a mirror.** See
  [[review-worktree-differs-from-session-cwd-mirror-artifacts]] for the cwd-portable traversal path
  that satisfies the SubagentStop hook from either cwd.
