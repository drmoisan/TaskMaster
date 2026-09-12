---
name: preflight-evidence-field-token-scan
description: Preflight check for the four evidence fields must be a literal token scan; prose forms like "the `git diff ...` command" read as compliant but omit the literal `Command:` label
metadata:
  type: project
---

When validating a plan for the mandatory command-step evidence fields (`Timestamp:`, `Command:`,
`EXIT_CODE:`, `Output Summary:`), scan for the **literal tokens**, not for meaning. Planners
frequently write the Command slot as prose — e.g. ``records `Timestamp:`, the `git diff <sha> -- Foo.cs`
command, `EXIT_CODE:`, and an empty-diff `Output Summary:` `` — which is semantically complete and in
the right order but never emits the literal `Command:` label.

**Why:** issue #430 preflight round 2. A per-task regex over acceptance lines found exactly two such
tasks (P1-T18, P3-T16) out of 25 checked; both had been read as compliant by a prose review, and one
of them (P3-T16) was on the round-1 remediation list for precisely this rule. Reading for meaning
misses it every time; a token scan finds it in one pass.

**How to apply:** run a scripted check over every `- Acceptance:` line whose task is command-bearing
(matches `EXIT_CODE:` or `Command:` or ` command`), assert all four literal tokens are present and in
ascending index order, and report the misses. Treat a prose-form Command slot as **non-blocking** when
the exact command is still named in the correct position and the other three tokens are present —
`evidence-and-timestamp-conventions` independently obliges the executor to write `Command:` into the
artifact, so there is no execution ambiguity. Blocking on it is disproportionate, especially if an
earlier preflight round already cleared the identical phrasing elsewhere in the same plan.

Related: [[project_plan_task_ids_digit_only_forces_renumbering]],
[[project_418_plan_rationale_clauses_are_evidence]]
