---
name: package-rate-not-lower-gate-trips-on-unrelated-file-variance
description: A whole-package "final rate not lower than baseline" coverage gate (e.g. UtilitiesCS JaCoCo LINE/BRANCH) fails on run-to-run variance in files the item never touches; one re-measurement does not absorb it
metadata:
  type: project
---

In the #940 run (2026-09-30) the plan's D-7 gate required the UtilitiesCS package LINE and BRANCH rates and the first-party REPO-LINE rate of the final run to be no lower than the Phase 0 baseline, with one identical re-measurement allowed. The item's own adapter files gained 12 covered lines, yet measurement 1 lost 2 branches and measurement 2 lost 2 lines and 3 branches. Every loss was in unrelated files (UtilitiesCS PropertyStore.cs, OlTableExtensions.Etl.cs, SubjectMapSco.Orchestration.cs), and the loss set differed between two identical runs. The plan's rule made this `AC8: NOT MET`, so the run stopped.

**Why:** these files are reached nondeterministically by other parallel tests, so a package-wide or repo-wide covered count varies by a few units per run. When the item's net delta is small (test-only change), that variance exceeds it.

**How to apply:** at preflight, flag any package-level or repo-level "not lower than baseline" gate on a small-delta item as likely to fail from variance; propose scoping the no-regression comparison to the files the item touches (per-file covered-line gates held here) or admitting a tolerance. At execution, run a per-line diff of the baseline and final Cobertura documents to locate the shortfall before reporting the stop. See [[exact-count-gate-vs-remediation-loop]].
