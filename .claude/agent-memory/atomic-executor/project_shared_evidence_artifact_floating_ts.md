---
name: shared-evidence-artifact-floating-ts
description: Several tasks told to write into "the same" evidence artifact whose filename embeds a per-task <ts> can split into multiple files across a minute rollover
metadata:
  type: project
---

When a preflight delta makes N tasks record rows into one shared evidence artifact, check
whether the artifact filename embeds the plan's `<ts>` placeholder ("captured at the moment
the task runs"). If it does, the N tasks can each resolve a different filename.

**Why:** #468 preflight cycle 2 — the B5 delta turned `P16-T27/T28/T29` from checkbox flips
into "record DEFERRED-TO-ORCHESTRATOR in `p16-t30-ac-reconciliation.<ts>.md`", the artifact
`P16-T30` owns. At minute granularity the four tasks usually agree, but a rollover between
T27 and T30 yields orphaned partial artifacts and T30's "three deferred rows" acceptance
reads a file missing the earlier rows.

**How to apply:** Not a blocker on its own — the executor can pin one `<ts>` for the group —
but call it out as an advisory and prefer plan wording that says the first task creates the
artifact, records the `<ts>` it used, and later tasks append reusing that recorded value.
Distinct failure mode from [[project_evidence_timestamp_collision_clobbers_artifacts]],
which is same-day OVERWRITE of a committed artifact; this one is SPLIT into two files.
