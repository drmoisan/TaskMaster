---
name: sanitisation-task-cannot-sweep-its-own-record
description: A "sanitise every committed artifact" task always leaves exactly one path unswept — its own record — so validate the record's content constraint, not the sweep scope
metadata:
  type: project
---

A plan task that sanitises host identifiers out of "every path the close-out commit will
touch" can never cover its own evidence record, because the record is written after the
sweep enumerates. Broadening the sweep scope does not close this; the fix is a content
constraint on the record itself (e.g. "lists only AFTER values — quoting a BEFORE value
reintroduces the identifier into a committed file").

**Why:** Verified on issue #680 preflight round 5. `P7-T3` swept the union of
`<FEATURE>/**` and `git status --porcelain`, which provably covers everything `P7-T4`
commits *except* `<FEATURE>/evidence/other/trx-sanitisation.<ts>.md`. Chasing the scope
one level deeper is an infinite regress; the plan already carried the right mitigation.

**How to apply:** When validating a sanitisation/redaction gate, enumerate the later
commit set against the sweep set and expect exactly one residual — the gate's own record.
Confirm the plan constrains that record's *content* rather than demanding it be swept.
Two mechanical checks that make the scope claim decidable:
- `git status --porcelain` collapses untracked directories, so a "no path *name* contains
  the literal" check over that half is blind to nested names. It is only safe when the
  collapsed directories also fall under an independently file-enumerated half (here
  `<FEATURE>/**`). Tracked-but-modified trees like `.claude/agent-memory/**` list per-file
  and are not affected.
- Plan check-offs written after the sweep (e.g. the mandatory pre-commit `[x]` flip of the
  final task) touch an already-swept file but add no identifier, so they are benign.

See [[project_trx_sanitisation_must_be_case_insensitive]] and
[[project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate]].
