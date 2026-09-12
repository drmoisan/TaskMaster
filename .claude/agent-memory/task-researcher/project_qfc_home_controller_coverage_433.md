---
name: qfc-home-controller-coverage-433
description: F7/#433 research on QfcHomeController.cs (the main partial) — live-form test debt, the line-133/136 sync-context coupling, and the rule that a partial split must be coverage-checked on BOTH halves
metadata:
  type: project
---

Research completed 2026-08-07 for epic `quickfiler-per-file-coverage` child F7 (issue #433),
targeting `QuickFiler/Controllers/QfcHomeController.cs` (487/500 lines). Companion artifact to
[[qfc-home-controller-metrics-433]], which covers the Metrics partial — read both; the dead
metrics-consumer defect and the #424 non-overlap are recorded there and are not repeated here.

**Why:** issue #136 mandates one production file at a time, and this file is 13 lines from the
500-line hard limit, so any seam work forces a split decision before a single test can be written.

**How to apply:** reuse before re-deriving for any QuickFiler home-controller or partial-split work.

1. **A partial split must be coverage-checked on BOTH halves before it is chosen.** The
   cohesive-looking split here (the seven `Func<>` loader seams, lines 159-245) measures 16 covered /
   13 uncovered = **55%** — it would create a brand-new file that fails the epic's own 80% bar on
   creation. The right split was the `#region Public Properties` block (406-485): 18/22 covered,
   reaching 100% with one new test. Cohesion alone is the wrong selection criterion in a
   per-file-coverage epic.

2. **Lines 133 and 136 of `InitAsync` are coupled and the coupling is invisible.**
   `_formViewer = new QfcFormViewer()` (133) is what makes
   `TaskScheduler.FromCurrentSynchronizationContext()` (136) succeed — constructing a WinForms `Form`
   auto-installs a `WindowsFormsSynchronizationContext` on the calling thread. Any seam that swaps in
   a mock viewer MUST add a `UiSchedulerLoader` seam in the same change, or
   `InitAsync_InitializesCorrectly` starts throwing `InvalidOperationException`.

3. **The existing QuickFiler.Test suite already constructs live WinForms forms.**
   `QfcHomeControllerTests.Init_InitializesCorrectly` and `.InitAsync_InitializesCorrectly` both
   execute `new QfcFormViewer()`. Pre-existing debt against the epic's "never construct a live form"
   rule — surface it rather than inheriting it silently.

4. **`LaunchAsync` is 0% covered for a structural reason, not a difficulty reason.** It is `static`
   and constructs the controller *inside itself* (line 53), so no instance seam can be pre-assigned by
   a test; and it calls `new ProgressTracker(ts).Initialize()`, which shows a real `ProgressViewer`
   form (`UtilitiesCS/Threading/ProgressTracker.cs:31-58`). The fix is extraction of a host-neutral
   `LaunchCoreAsync`, not a factory-delegate static field (which would add cross-test global state).

5. **Deriving a per-line hit map without running coverage.** The newest committed
   `evidence/qa-gates/coverage-final.cobertura.xml` under a recently-merged feature folder gives an
   exact per-method and per-line hit map, including which half of each `??`/`?.` branch is unexercised.
   Use it as a planning input (never as acceptance evidence) whenever a coverage harness is upstream.

Related: [[qfc-home-controller-metrics-433]], [[qfc424-high-confidence-startup-stall]],
[[qfc-high-confidence-dual-pipeline]], [[qfc-item-controller-227-r2-denial]].
