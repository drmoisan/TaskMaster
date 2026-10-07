---
name: outcome-branch-sets-keyed-on-route-miss-derived-flag-cases
description: Preflight check for coverage-task branch lists - branches keyed on the route (RUNNER/DIRECT) go stale when a later fix adds a derived flag (RAW), and (a)/(b)/(c) lists rarely cover "non-zero exit, empty failed set"
metadata:
  type: project
---

Found on the #931 round-2 preflight (2026-09-28). A round-1 fix made `RAW` True for a failed RUNNER
coverage run (the runner throws before post-processing), so the floor lines printed by the plan's own
post-processing payload became the only floor evidence for that run. The baseline floor branch still
read those lines only "under DIRECT", so a RUNNER run with failed-but-admissible tests could complete
the baseline with an unmet floor that no branch examined.

The same task's outcome list was (a) exit 0 and floors met, (b) non-zero with a non-empty admissible
failed set, (c) floor failure. It had no branch for a non-zero exit with an EMPTY failed set, which is
what a testhost abort, a blame hang kill, or a collector crash produces. A fixed-name runner output
left over from the baseline run also makes the final run's `_PRESENT` flags read True after an abort.

**Why:** a planner writes branch conditions against the route that existed when the branch was
authored. A later fix that introduces a derived flag changes which evidence exists per route, and the
planner re-checks the fixed line, not the sibling branch that consumes the evidence.

**How to apply:** when a revision adds or redefines a derived flag, grep every branch and acceptance
clause that names a route or the flag and ask which evidence each (route, flag) pair produces. For every
outcome-branch list, require a residual "any other outcome" stop. For fixed-name tool outputs reused
across stages, require the stale file be deleted before the run.

Related: [[pinned-target-source-prose-carries-census-tokens]]
