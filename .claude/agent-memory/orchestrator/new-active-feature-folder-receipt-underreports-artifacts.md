---
name: new-active-feature-folder-receipt-underreports-artifacts
description: new_active_feature_folder scaffolds spec.md and plan.<ts>.md too, but its receipt lists only issue.md — files appearing "from nowhere" seconds later are its output, not a concurrent peer agent
metadata:
  type: project
---

`mcp__drm-copilot__new_active_feature_folder` writes MORE than its receipt reports. On issue #821
(2026-09-08, `type: bug`, `work_mode: full-bug`) the receipt's `artifacts[]` array named exactly one
file:

```
"artifacts":["<abs>/docs/features/active/<folder>/issue.md"]
```

A `Glob` of the folder minutes later returned three files: `issue.md`, `spec.md`, and
`plan.2026-09-08T23-50.md`. Both extra files are real scaffolds — `spec.md` is the bug template with
the promoted record's prose mapped into Context / Repro & Evidence / Root Cause Analysis / Test
Strategy, and the plan file is the generic 8-phase bug template full of `<spec link>` placeholders
and a `tests/bugs/<YYYY>/#<N>-<desc>.py` path that is wrong for a C# repo.

**Why this matters twice.**

1. **Misdiagnosis risk.** The files carry a `Last Updated:` timestamp minutes after folder creation,
   and `git status` shows the whole folder as one untracked `??` line, so it does not reveal them.
   The natural reading is that a concurrent peer agent is writing into your worktree — see
   [[shared-checkpoint-read-modify-write-corrupts]] and
   [[parent-session-can-commit-into-child-worktree]], both of which are real. Before standing down
   for a suspected peer, check whether the "foreign" files are template scaffolds: placeholder
   angle-brackets and a wrong-language test path are the tell. Check the reflog too — a peer merging
   or committing leaves entries; the MCP tool leaves none.
2. **Plan-path continuity.** `feature-promotion-lifecycle` step 5a says to reuse the earliest
   existing `plan*.md` in the feature folder as `${plan-path}`. That scaffolded
   `plan.<timestamp>.md` IS that file, so the canonical plan path is decided for you at promotion
   time. Recording `plan.md` in the checkpoint and letting the planner create it produces two plan
   files and violates the Plan-Path Continuity Contract.

**How to apply:** immediately after `new_active_feature_folder` returns, `Glob` the folder rather
than trusting `artifacts[]`. Adopt the scaffolded `plan.<ts>.md` as `${plan-path}` in the
checkpoint. Treat the scaffolded `spec.md` as a starting template that must be REWRITTEN, not
appended to: for a consolidated issue it carries only the promoted record the tool happened to copy
(Site A of two), and its generic acceptance criteria are unfalsifiable boilerplate that must be
deleted — see [[preflight-catches-vacuous-gates]].
