---
name: revert-plans-must-check-test-provenance
description: When planning a revert, verify each test's provenance with git show against the pre-change sha before classifying it as an artifact of the change being reverted
metadata:
  type: feedback
---

A revert plan classified `IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected` as a
test that "exists solely to pin the semantics this cycle deletes" and scheduled it for deletion.
`git show cee78979:<path>` showed the test present at line 54 **before** the change being reverted,
in exactly the single-argument form the revert restores. The prior cycle had only appended `, null`
to it. Deleting it would have removed a rejection test that the approved scope explicitly required
to survive — a silent loss of regression coverage, dressed as cleanup.

The distinguishing check is one command per test:

```
git show <pre-change-sha>:<test-file> | grep -n "<TestName>"
```

Absent at the pre-change sha means it is genuinely an artifact of the change and may go. Present
means it predates the change and must survive, usually reverting for free under whatever mechanical
transformation the plan already applies.

Watch for the companion case: two tests that assert the same behavior through different parameters
collapse into byte-identical duplicates once the revert drops the parameter. That is real
de-duplication, but delete the NEWER one and keep the pre-existing one, so the surviving test keeps
its original provenance and comment.

**Why:** Test deletions in a revert look like bookkeeping and get low scrutiny, but a rejection test
is the only thing standing between a reverted guard and a silent regression. This was caught by
executor preflight, not by the plan validator, and not by me on first read — the plan's prose
justification was internally coherent and simply rested on a false premise about history.

**How to apply:** Any plan that deletes or inverts a test during a revert or remediation cycle must
state each test's provenance sha, and that claim must be verified against git before execution.
Deleting a test changes suite-total arithmetic, so also re-check every dependent count
(post-change total, per-file contribution, net delta) for mutual consistency — a stale total turns
into a falsely-firing gate later. Related: [[preflight-catches-vacuous-gates]] covers the inverse
shape, gates that cannot fail.
