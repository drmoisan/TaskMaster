---
name: compute-the-complexity-floor-before-choosing-a-band
description: Run Get-ComplexityFloor with the item's signals before choosing a delegation band; a race or flaky-test item carries the concurrency_or_ordering floor signal, which forces C3 (opus), so a "small, contained" C2 judgment sits below the floor
metadata:
  type: feedback
---

Before choosing the complexity band for any `Agent(orchestrator)` delegation, list the item's floor
signals and run `Get-ComplexityFloor -SignalsPresent <signals>` from
`.claude/lib/model-routing/ModelRouting.psm1`. Choose the band at or above that floor, and only then
call `Resolve-DelegationModel`.

**Why:** On `/parallel-add 942` (2026-09-29), a flaky-test race touching one production file and
one test file, I judged it C2 on footprint alone and launched the preparation child on `sonnet`. The
child assessed C3 because `concurrency_or_ordering` is a floor signal, and the floor ignores
footprint entirely. Under the orchestrator-state invariants, a band below its floor makes the entry
malformed. My C2 receipt was also the only one below the floor in the run. The parallel checkpoint
validator does not check that receipt, so nothing flagged it. The preparation still cleared.
However, the research also overturned the issue's own diagnosis of the race, which is the kind of
reasoning the floor exists to protect.

**How to apply:** For any item whose title or body says race, flaky, intermittent, ordering,
deadlock or thread, expect the `concurrency_or_ordering` signal and a C3 floor. Footprint size does
not lower the floor. Record the floor in the delegation receipt's note. When a child's
`complexity_assessments[]` band differs from the one you launched at, use the child's band for the
execution launch and say so in the admission note. See [[parallel-run-execution-playbook]] for the
`-Agent`/`-Band` parameter names.
