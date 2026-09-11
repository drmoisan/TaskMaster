---
name: diff-the-status-doc-header-against-the-checkpoint
description: parallel-status.md drifts from the checkpoint despite being a generated projection; diff every header field at each regeneration boundary instead of editing only the fields your own operation touched
metadata:
  type: feedback
---

When you regenerate `docs/features/parallel/<slug>/parallel-status.md`, diff EVERY header-block
field against `artifacts/orchestration/parallel-orchestrator-state.json` — not just the fields the
operation you are performing changed.

**Why:** On the `bugs-2026-09-06` close (2026-09-08) the doc's `current_cohort` read `4` while the
checkpoint read `5`. Nothing in the close touches `current_cohort`, so an edit scoped to
`last_updated`, `next_step`, and the `mutations[]` row would have carried the stale value forward
and left a wrong number in the one artifact a human actually reads. The doc is *documented* as a
generated projection, which makes it easy to assume it is already faithful; it is only as faithful
as the last agent that regenerated it, and a partial regeneration by an earlier boundary leaves
exactly this kind of residue.

**How to apply:** The skill says regenerate IN FULL, and on TaskMaster the practical route is the
Edit tool (see [[parallel-run-execution-playbook]] — there is no status template and heredocs die
on apostrophes). Edit-based regeneration is inherently partial, so compensate: before editing,
print the checkpoint's `parallel_slug`, `mode`, `max_concurrency`, `current_cohort`,
`recolor_generation`, `last_updated`, and `next_step` and compare all seven against the doc. Do the
same for the item table's `merge_status` and `merge_commit_sha` columns, which are the other fields
a stale projection silently misreports. A mismatch is a defect in the doc, never in the checkpoint —
the checkpoint is the authority here, and the doc is never an input.
