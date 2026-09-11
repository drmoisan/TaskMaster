---
name: donotparallelize-census-misses-lazy-init-writers
description: A "zero writers left in the parallel bucket" claim built from a reflective field-name grep misses the production writer reachable through sibling lazy-init properties
metadata:
  type: project
---

When an executor claims `[DoNotParallelize]` now covers **every** writer of a process-global static,
check what the supporting census actually enumerated. A grep for the reflective field-name literal
(e.g. `git grep -F '"_dispatcher"'`) finds only *test* writers. It cannot find the production writer.

**Why:** #584. `UiThread._dispatcher` is written by `UiThread.Initialize()`, which `UiThread.Init()`
calls. `Init()` is in turn called *lazily* by two sibling accessors in the same class —
`UiThread.UiSyncContext` and `UiThread.AutoScaleFactor` — whenever their own backing field is null.
So any parallel-bucket test that transitively reads either sibling property can write the static that
`[DoNotParallelize]` was supposed to have quarantined. The census recorded "zero writers of that field
remain in the parallel bucket"; that statement is true only of reflective writers.

**How to apply:** after reading the executor's census, grep the *test assembly* for every accessor
that can reach the initialiser, not just the field name:
`UiThread\.(Init|UiSyncContext|AutoScaleFactor|Dispatcher)` over the test tree, then grep the
*production* tree for the same lazy accessors and ask which of those production call sites is
exercised under test.

On #584 the gap turned out to be closed in fact, but only coincidentally: no `UtilitiesCS.Test` file
reads either lazy sibling directly; `ThreadMonitor.cs:143` reads `UiSyncContext` but
`ThreadMonitorTests` already had `[DoNotParallelize]`; and `FolderPredictor.cs:178` reads it but its
driver `FolderPredictorTests.cs:479` reflectively pre-sets `_uiSyncContext` so the lazy branch is
never taken. Report it as a Low informational residual with that chain spelled out — the guarantee
holds by an unrelated test's arrangement, not by construction, so it can silently break.

See [[584-review-residuals]] and [[verify-parity-claims-in-remediation-inputs]].
