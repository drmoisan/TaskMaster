---
name: package-counter-delta-corroborates-new-type-coverage
description: When per-class coverage detail has been stripped from committed evidence, a package-level missed/covered delta between the baseline and final JaCoCo summaries can still prove new-code coverage and changed-line no-regression.
metadata:
  type: project
---

TaskMaster feature evidence is trending toward committing package-level JaCoCo summaries instead of full Cobertura reports (on #503, commit `d0955dc4` replaced two ~10 MB / ~187k-line Cobertura files with 39-line JaCoCo summaries). That removes exactly the per-class detail that the per-type (`>= 90%` new module) and per-file (no changed-line regression) acceptance criteria assert against, so the executor's tabulated per-type numbers become unverifiable from committed artifacts alone.

The package-counter delta recovers both claims without rerunning coverage. Diff the baseline and final `<counter type="LINE">` for the package containing the new code:

- On #503 the `TaskMaster` package went `missed=1464 covered=3329` -> `missed=1464 covered=3515`.
- `covered` rose by exactly 186, which is the exact line total of the four new types (48+48+72+18) reported in `new-type-coverage.<ts>.md`.
- `missed` did not move at all.

Both facts together are decisive: had ANY of the 186 new lines been uncovered, `missed` must have risen; and since `missed` is unchanged, no previously-covered line in that package lost coverage either. That proves the new-code floor AND the changed-line no-regression gate from a 39-line artifact.

**Why:** Grading such an AC UNVERIFIED because the named artifact lost granularity would be both unhelpful and wrong — the substance is provable. Grading it PASS on the executor's word alone would be unverified deference.

**How to apply:** Compute this delta before accepting or rejecting a per-type coverage claim whose source artifact was summarized. Record it explicitly as an independent corroboration route in the policy audit, and separately record the evidence-durability regression as a Low finding recommending the summariser retain per-class counters for changed files. Note the branch counter usually will NOT be this clean (on #503 `BRANCH` missed rose 387->389 while covered rose +62), so attribute branch deltas cautiously. See [[csharp-coverage-artifact-is-cobertura]] for the related format trap.
