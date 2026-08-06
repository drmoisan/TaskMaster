---
name: named-coverage-exception-verify-member-body
description: Before writing a named "untestable branch" coverage exception into a plan task, read the member body and confirm the branch actually lives there; and never place coverage remediation after the toolchain-clean-pass task
metadata:
  type: feedback
---

Two coupled rules for coverage-gate escape hatches in atomic plans.

**Rule 1 — a named branch exception must be verified against the member body, not inferred from the API shape.** Read the declaration and count its executable lines before writing the exception. If the decision the exception describes lives in a callee, the exception is moot and must not be written.

**Rule 2 — coverage remediation belongs in a task that runs *before* the toolchain-clean-pass task.** A gap predicted at plan time is closed by a dedicated early task, not by "add tests and rerun the coverage task" inside the coverage-delta task itself.

**Why:** #418 revision passes 6 and 7. Pass 6 added a `GetSvgDocumentOrThrow` "null-`InnerException` branch is unreachable" exception so the `>= 90%` new-member gate could not loop forever. Pass 7 preflight established the branch is not in that member at all — the member's `throw` is a single statement and the null/non-null decision lives in a `DescribeFailure` helper whose null arm was already covered through a different caller. The exception was removed as moot; leaving it would have misled a later reader. The *real* gap was different (no test drove the success `return`). Remediating that inside the coverage-delta task would have been wrong because the toolchain-clean-pass task runs earlier and records the single consecutive clean pass that the AC rests on — editing a test file afterwards and rerunning only the coverage task leaves the clean-pass artifact describing a state that pre-dates the edit, so the AC gets checked off against stale evidence.

**How to apply:**
- Before writing `COVERAGE_BRANCH_UNREACHABLE` / `Untestable branch:` for a member, open the file and confirm the branch is in that member's own body. Prefer "the equivalent branch is covered at `<file>:<line>` through `<test>`" over an exception.
- When a member's line count and test inventory let you predict a sub-threshold rate at plan time, insert an explicit gap-closure task as the phase's first task and renumber the rest; do not attach the fix to the measurement task.
- Pin the gated metric explicitly (`line-rate` vs `branch-rate` on the Cobertura `<method>` element). Preflight rejected a bare `>= 90%` because the two disagreed in opposite directions across the delivered members.
- Record known-below-threshold `branch-rate` values with the undriven condition named (defensive guards no in-scope caller reaches) rather than gating on them.

Related: [[research-claims-as-acceptance-clauses]], [[coverage-gate-clr-invoked-private-members]], [[plan-validator-task-id-sequential-constraint]].
