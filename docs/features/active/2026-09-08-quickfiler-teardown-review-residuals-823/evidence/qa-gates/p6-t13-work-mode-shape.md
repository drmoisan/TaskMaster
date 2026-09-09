# Phase 6 — full-bug document-shape check

Timestamp: 2026-09-09T14-56

Task: [P6-T13]

Command: directory listing of
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/`, with
`Test-Path` probes for `spec.md` and `user-story.md` and a filtered enumeration of `plan*.md`

EXIT_CODE: 0

The folder contains three files, `issue.md`, `plan.2026-09-08T23-50.md` and `spec.md`, and two
directories, `evidence` and `research`.

SPEC-PRESENT: YES
USER-STORY-PRESENT: NO
PLAN-FILE-COUNT: 1
PLAN-FILE-NAME: plan.2026-09-08T23-50.md

`full-bug` work mode requires `spec.md` and requires `user-story.md` to be absent. Both conditions
hold. `spec.md` is therefore the sole authoritative acceptance-criteria source for this feature,
carrying AC1 through AC29. The single plan file matches the Plan-Path Continuity Contract: no
timestamped sibling plan was created during this cycle.

Output Summary: `spec.md` present, `user-story.md` absent, exactly one plan file named
`plan.2026-09-08T23-50.md`. The `full-bug` document shape is correct.
