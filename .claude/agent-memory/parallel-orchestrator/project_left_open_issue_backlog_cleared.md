---
name: left-open-issue-backlog-cleared
description: All 11 issues that six family memories record as shipped-but-still-OPEN were verified CLOSED on 2026-09-01; those files' status claims are historical and must not be repeated as current
metadata:
  type: project
---

Every issue that the six "family shipped; issues left open" memories record as still OPEN is now
**CLOSED**. Verified 2026-09-01 with `gh issue view <n> --json state`, one call per issue, all
eleven: **286, 448, 458, 460, 462, 471, 473, 474, 499, 500, 502**. A twelfth, 488, was checked in
the same pass and is also closed.

**Why:** I had built a running claim across six families that shipping a bug family routinely
leaves its member issues open, and had escalated it to "the pattern is structural." The delivery
half of that observation is still true and still useful — the merge commits, AC counts, and the
reasons the closing keywords failed to fire are all accurate history. The *status* half expired: the
backlog was worked through outside my sessions. Nobody told me; I found it only because item 656's
child asserted four of those issues were closed, which contradicted my index, and I checked rather
than correcting the child.

**How to apply:**

- Never repeat "shipped with issues left open" as a current-state claim about these families. The
  six files remain valid as delivery records — what shipped, in which commit, with which ACs — and
  their status lines are superseded by this file.
- Do not re-derive the "structural pattern" conclusion from those six files alone. Six historical
  instances of a since-resolved condition are not evidence that the condition holds now.
- The operational rule that survives unchanged is
  [[verify-delivery-before-preparing-an-admission]]: check an issue's state and `main` before
  admitting it to a run. That rule is *why* this staleness was harmless — it forces a live check at
  the only moment the answer matters.
- Generalize the trigger, not just the fact. A memory that records the state of an external system
  — issue status, PR status, branch existence, backlog contents — decays without notifying me, and
  the decay is invisible while I only ever read my own index. **A subagent contradicting my memory
  on external state is a signal to verify, not to correct the subagent.** On this occasion the
  child was right and I was stale.
