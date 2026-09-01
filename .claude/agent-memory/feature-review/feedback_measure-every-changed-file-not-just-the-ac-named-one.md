---
name: measure-every-changed-file-not-just-the-ac-named-one
description: Compute per-file coverage for EVERY changed production file from the Cobertura XML, not just the one the AC names; executor evidence routinely covers only the primary file and hides call-site regressions
metadata:
  type: feedback
---

When auditing coverage, parse the Cobertura document yourself and compute line coverage for **every** changed production file in the branch diff, not only the file the acceptance criterion names.

**Why:** On #647 the spec's AC20 scoped its coverage obligation to `UtilitiesCS/To Depricate/FileIO2.cs` and the repository-wide figure, and every executor evidence artifact reported exactly those two. Measuring the two call-site files directly exposed what the evidence never showed: `QuickFiler/Controllers/QfcHomeController.Metrics.cs` sat at 77.05% (94/122) with the six lines the change *added* (the new `if (!metricsWritten) logger.Error(...)` block) reading `hits="0"`, a regression from roughly 80.18%. That is the single most valuable untested block in the whole diff — the one place a caller consumes the new failure signal through a testable seam — and no gate in the plan looked at it.

**How to apply:** After confirming the repo-wide root attributes, run a per-file aggregation over the `<class>`/`<line>` elements keyed by the `filename` attribute (a partial class spans several `<class>` elements, so aggregate by filename and take max hits per line number). Then, for each changed production file, select the diff's added line numbers and report their hit counts. Reconstruct the baseline per-file rate arithmetically from the diff when no baseline XML exists. Two follow-on judgments this enables:
- New uncovered lines are materially worse than pre-existing uncovered lines. [[677-review-residuals]] dispositioned a sub-80 modified file non-blocking *because* all uncovered lines were proven pre-existing; that leg does not hold when the change itself added them.
- Before escalating, check whether a covering test could assert anything. On #647 the uncovered lines were a `logger.Error` on a **static** log4net field, so a test would produce coverage without assertion power ([[501-review-residuals]]). Correct remedy is a promoted logging-seam issue, not a coverage-chasing test — which is why this stayed non-blocking.
