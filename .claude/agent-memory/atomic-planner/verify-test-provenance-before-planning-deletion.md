---
name: verify-test-provenance-before-planning-deletion
description: "Before a revert/remediation plan deletes a test as \"an artifact of the cycle being reverted\", read that test at the pre-cycle commit; a two-argument call shape does not prove the test was added by the cycle that added the parameter"
metadata:
  type: feedback
---

Before writing a plan task that deletes a test on the grounds that it "exists solely to pin the semantics this cycle deletes", read that test at the commit preceding the cycle (`git show <pre-cycle-sha>:<test-file-path>`). Classify by provenance, not by current call shape.

**Why:** In #614 remediation cycle 2 (a partial revert of cycle 1), the plan deleted two tests it called "cycle-1 root-parameter tests" because both passed a second `archiveRoot` argument (`, null` and `, string.Empty`). Only one was a cycle-1 addition. The other existed at the original delivery commit in exactly the single-argument form the revert restores — cycle 1 had merely appended `, null` to it. It was a rooted-rejection test the approved requirements explicitly required to survive, so the plan violated its own scope. Preflight caught it as Blocking. The misleading signal is structural: after a signature widening, every call site carries the new argument, so a pre-existing test and a newly added one look identical.

**How to apply:** Any plan that reverts a prior cycle. For each test named in a delete/remove task, state in the plan decision which commit introduced it and cite the sha. When two tests genuinely become byte-identical after an argument drop, delete the NEWER one and record the older one as RETAINED — its literal-gate row becomes a `still exactly 1` retention anchor rather than a zero-hit deletion gate, and it must be added to the must-stay-green list (updating any "the N rule-8 names" count). Then sweep every dependent figure: test-count arithmetic, the suite baseline-minus-delta gate, and the per-class contributed-test count. See [[project-614-store-root-leak-plan-seams]], [[acceptance-edits-must-be-false-before-true-after]], and [[stale-build-output-is-not-evidence-of-existence]].
