# Issue #791 update mirror

Timestamp: 2026-09-06T15-17

Command: manual edit of `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/issue.md`, appending the Outcome section reproduced verbatim below and adding two Next Step entries.

EXIT_CODE: 0

Output Summary: The local `issue.md` now carries the Outcome section below verbatim, plus two added Next Step checklist entries: `- [x] Implement the fix and record evidence (2026-09-06; see Outcome below)` and `- [ ] Human live-Outlook confirmation per `runbooks/live-outlook-cancel-teardown-verification.runbook.md` (human-interaction exception HI-1; does not gate the automated review)`. The narrative AC copy in `issue.md` is deliberately left unchecked; `spec.md` is the sole authoritative acceptance-criteria source and carries the six check-offs.

PostedAs: local file update only. This text was NOT posted to GitHub issue #791 by this execution. Posting to the remote issue is the orchestrators step; this artifact is the mirror the evidence conventions require, and it carries the exact text intended for that post.

---

## Exact text appended to issue.md

## Outcome

Implemented on 2026-09-06 on branch
`bug/quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791`.

Both reported defects are fixed and pinned by deterministic MSTest regression tests.

**Defect 1 — the deadline policy.** The first-batch deadline is now an advisory checkpoint rather
than a return. When it expires with zero acceptances the gate logs the cutoff, the scanned and
accepted counts, the elapsed time and the remaining headroom on both bounds, resets the checkpoint
interval, and keeps scanning. Two hard bounds terminate the extended scan — a cap of 250 candidates
scored without an acceptance, and a 120-second ceiling that bounds the wait while the background
loader is still refilling — and a bounded exit is reported as the new stop reason
`QfcDequeueStop.ScanCapReached`, which callers treat exactly as they treated `DeadlineExpired`: the
UI queue stays open. A launch line now records the cutoff (the reported 900 was never logged), the
requested quantity, the checkpoint interval and both bounds. Both bounds are internal constants with
constructor test seams and introduce no settings surface.

**Defect 2 — the Cancel teardown.** `ActionCancelAsync` is reordered and made exception-safe: it
logs entry, cancels the token before its first await, marshals to the UI context, resets the
keyboard-active flag, parks WebView2 focus and cancels every breadcrumb selector through a routine
extracted from the `Form.Deactivate` handler, unregisters navigation and form handlers before the
item rows are removed, hides the form, awaits the new `IQfcDatamodel.QuiesceLoaderAsync` before any
datamodel field is nulled, cleans up the groups, and reaches `Cleanup()` — and through it
`RibbonController.ReleaseQuickFiler` — from a `finally`. Every stage runs through a helper that logs
its completion at DEBUG and any escaping exception at ERROR with the stage name, so no stage is
silent and a throwing stage cannot skip a later one. `Worker_DoWork` now captures the loader task so
there is something to await; `TryQueueRemainingMailItemAsync` snapshots and guards `_masterQueue`
and `_moveMonitor` and returns `false` instead of constructing a delegate over a null instance, which
is the exact `ArgumentException` the attached log records; `QfcDatamodel.Cleanup()` is null-guarded
so a second Cancel is inert; and `QfcHomeController.Cleanup()` is two guarded blocks under a
`finally` that also disposes the token source and detaches the worker-completed handler.
`ButtonCancel_Click` no longer rethrows — a deliberate behaviour change, since an `async void`
rethrow becomes an unhandled Outlook UI-thread exception that reports nothing actionable, which the
stage-level ERROR logging replaces.

**Verification.** 7023 tests passed with 0 failures across the nine first-party test assemblies;
`QuickFiler.Test` alone went from 1339 to 1362 passing with no newly-failing test. The toolchain
passed in the CLAUDE.md order in one uninterrupted final pass: 1587 files formatter-clean, 0
analyzer warnings and errors, 0 nullable warnings and errors. First-party line coverage moved from
84.50 % to 84.51 % and branch coverage from 79.14 % to 79.19 %; no changed line lost coverage, and
90.8 % of the executable changed lines are covered.

**Acceptance criteria.** All six are checked off in this feature folder's `spec.md`, which is the
sole authoritative acceptance-criteria source for this work. The two criteria restated in this file
above are a narrative copy and are deliberately left unchecked so there is one place of record.

**Superseded criteria**, stated deliberately rather than regressed silently: the #424 criterion at
`docs/features/archive/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md:231` and
the #608 criterion at
`docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/spec.md:184`
are both superseded by #791 AC1. #446 AC-6 is preserved: `QfcHomeController.Iteration.cs` is
unmodified and `CompleteAddingAsync` remains reachable only under `SourceExhausted`.

**Still open.** The live-Outlook confirmation is a human follow-up (HI-1) and does not gate the
automated review. Issue #792 tracks the breadcrumb WebView2 initialization failure (0x8007139F),
which is out of scope here.
