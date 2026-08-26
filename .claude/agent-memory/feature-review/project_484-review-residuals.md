---
name: 484-review-residuals
description: "#484 qfc-item-controller-defects review (2026-08-26): PASS, 0 blocking, 50/50 AC; residuals F1 ApplyReadEmailFormat TOCTOU race (Major non-blocking) and F4 OneDrive silent-skip promotion candidate; D-1/D-2 plan-provision AC divergences accepted"
metadata:
  type: project
---

Review of `bug/qfc-item-controller-defects-484` @ 4f2b55f1 vs merge-base 61edc19b (epic
`quickfiler-bug-family`): PASS, zero blocking findings, 50/50 spec ACs, artifacts timestamped
2026-08-26T10-22 in the feature folder.

Residuals to track at epic close or in sibling reviews:

- **F1 (Major, non-blocking)**: `ApplyReadEmailFormat` (`FocusAndTheme.cs:318-336`) guards four
  fields for null then RE-READS them; parameterless `Timer.Dispose()` does not wait for in-flight
  callbacks, so `Cleanup()` can null a field between guard and use — residual NRE window on a
  thread-pool timer callback (process-fatal if it fires). Fix: snapshot fields to locals or
  `Dispose(WaitHandle)`. Per the user's promote-latent-defects rule this should become an issue.
- **F2 (Minor, latent)**: `UnwireControlTreeEvents` re-forms detach delegates from the CURRENT
  `_kbdHandler`; safe only while `_kbdHandler` is assigned once per lifetime.
- **F4 (Info, promotion candidate)**: `MoveMailAsync` OneDrive-missing path still silently returns
  success to the bulk loop after a debug log (pre-existing, outside #483's scope).
- **D-1/D-2 pattern reconfirmed**: spec AC prose embedding stale projections (per-file line-count
  table; predicted-zero coverage for a single-line lambda initializer, which registers a
  construction hit and reads 100%) can stay CHECKED when the criterion defers to a plan provision
  (C2 "starting assignment, not a per-file mandate" + capacity rule 3 relocation) and the binding
  requirement is met — same acceptance path as [[449-review-residuals]]. Executor self-reporting in
  `evidence/other/ac-reconciliation.md` made this cheap to adjudicate; expect the same file in
  sibling children.
- Repo-wide C# line coverage at this head: 84.8323% (baseline 84.775%) — above CLAUDE.md's 80%,
  below the rules' 85%; the 80-vs-85 floor conflict is still unresolved and was surfaced again.
- File-size pressure: ViewerSetup.cs 499, EventWiringTests.cs 499, ViewerSetupTests.cs 498,
  MailActionsTests.cs 498 — sibling features touching these files must extract before adding.
