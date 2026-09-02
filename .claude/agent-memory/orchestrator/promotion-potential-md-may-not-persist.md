---
name: promotion-potential-md-may-not-persist
description: MCP promotion tools create the GitHub issue and populate active issue.md, but the potential .md (and its promoted/ copy) may not persist to the worktree filesystem — recreate for audit trail
metadata:
  type: feedback
---

During preparation of epic child 9003 (2026-07-15, worktree
`agent-a2519325ca99f84ae`), the MCP promotion chain behaved as follows:

- `new_potential_entry` returned a worktree path for the potential `.md`, and a
  `Write` to that path succeeded (the file existed at that moment).
- `potential_to_issue` returned `ok:true`, created the real GitHub issue (#325),
  reported a `destination_path` under `docs/features/potential/promoted/`, and its
  content flowed correctly into the active folder `issue.md`.
- BUT afterward NEITHER `docs/features/potential/2026-07-15-...md` NOR
  `docs/features/potential/promoted/2026-07-15-...md` existed on the worktree
  filesystem (find + git ls-files both empty). Only `issue.md` carried the content.

**Why:** the MCP extension resolves/moves files against its own resolution that
did not leave the promoted potential `.md` on the worktree. The substantive
outputs (GitHub issue, active folder issue.md/spec/user-story) were all correct;
only the intermediate potential-entry audit artifact was missing.

**How to apply:** after `potential_to_issue`, do not assume the potential/promoted
`.md` is on disk. Verify with `find`/`git ls-files`. If absent and you referenced
it in the checkpoint `relativeFile`, recreate a concise promoted potential doc so
the reference resolves and the promotion audit trail is complete before committing.
Do NOT treat its absence as a promotion failure — trust the issue URL + populated
issue.md as the real success signals. Observed once; verify before relying on it.
Related: [[potential-to-issue-creates-github-issue]].

**Converse, observed 2026-09-01 on issue #663 (parallel-add prep):** when NO
promoted source exists at all, `new_active_feature_folder` returns `ok:true` and
emits `spec.md` plus the plan template but **no `issue.md` whatsoever**. It had
nothing to copy. The tool does not warn. That leaves the folder without the
`- Work Mode:` marker every downstream mode-resolution rule reads, so planning
would fail closed to `full-feature` and demand a `user-story.md` the bug route
does not want.

**How to apply:** always `ls` the active folder immediately after
`new_active_feature_folder` and confirm `issue.md` exists. If it does not, author
it from the GitHub issue body (`gh issue view <N> --json body`) and persist the
correct `- Work Mode:` marker yourself before delegating anything. Record in the
checkpoint that the tool did not produce it, so a later audit does not read the
hand-authored file as drift.
