---
name: preflight-defect-trend-scope-confound
description: A rising preflight defect count is not divergence if you widened the inspected surface that round; hold scope fixed for at least one round before judging convergence, and track re-introductions separately from raw counts
metadata:
  type: feedback
---

When running a bounded preflight loop (`atomic-executor` with `DIRECTIVE: PREFLIGHT VALIDATION ONLY`),
do NOT read the raw defect count as the convergence signal on its own. Two corrections are needed.

**1. Scope-widening confounds the count.** If you direct a round to inspect areas earlier rounds
never examined, its count measures a LARGER surface and can legitimately rise. Verified on issue
#493 (2026-08-25): counts ran 26 → 13 → **16** → 1 → 0. The rise at round 3 was entirely a scope
effect — that round was explicitly told to widen into task ordering, fixture-contract compilability,
cross-consistency, and AC traceability. Rounds 4 and 5 re-validated that SAME widened surface and
returned 1 then 0. Read against round 2's 13 the round-3 number looks like divergence; read
like-for-like against round 4 it is a step change.

**How to apply:** when you widen scope in a round, (a) say so in the delegation prompt, (b) record a
`scope_note` on that iteration in the checkpoint, and (c) run the NEXT round with scope explicitly
held fixed — tell the executor "do not widen into a new category; match the surface the previous
round established." Only that round's count is comparable, and it is the one a keep-or-drop decision
should turn on.

**2. Re-introductions are the real health metric, not the count.** Track a `defect_class_ledger` with
per-class `instances_by_iteration`, and require the executor to classify every defect as NEW-at-this-
location or RE-INTRODUCTION, with an explicit count even when zero. Distinguish a site an earlier
round never REACHED (new — fine) from a site an earlier round CORRECTED that has reverted (a real
regression). On #493 the D-26 class appeared in rounds 1, 2, and 3 at four DIFFERENT sites; that is
incomplete coverage, not regression, and treating it as regression would have wrongly condemned a
converging plan.

**3. Defect-class migration is a strong terminal signal.** Early rounds finding false claims about the
repository (wrong line citations, wrong namespaces, unsatisfiable assertions) versus a late round
finding only internal dangling references means the plan has run out of factual surface to be wrong
about. That migration predicted the ALL CLEAR better than the count did.

**Why:** a predecessor plan for #493 was discarded after five rounds because its round 5 re-introduced
a class its round 4 had fixed. Round 5 is where these loops break, so warn the final-round executor
explicitly that it sits at that point, that every edit it makes is itself the regression risk, and
that a clean result is a legitimate outcome rather than a failure to look hard enough. On this run
round 5 returned ALL CLEAR with ZERO edits.

See [[remediation-loop-strict-handoff]] and [[atomic-planner-lacks-mcp-validator-tool]] — the executor
likewise cannot run the MCP plan validator, so the orchestrator must run that mandatory gate itself
after every revision round.
