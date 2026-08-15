---
name: preexisting-issues-skip-potential-to-issue
description: When an epic decomposes an existing bug backlog, prep children must skip potential_to_issue and call only new_active_feature_folder, or every child files a duplicate issue
metadata:
  type: feedback
---

An epic scoped from an existing issue backlog (rather than from fresh potential entries) must tell
every preparation child, in the delegation prompt, NOT to run the first two promotion steps:

> Issue #N is already open and its potential entry is already promoted at
> `docs/features/potential/promoted/<file>`. Do NOT call `new_potential_bug_entry` or
> `potential_to_issue` — `potential_to_issue` always creates a NEW issue and would duplicate #N.
> Call only `new_active_feature_folder` with `issue_number = N`. Record the promotion receipts
> truthfully: note that the potential-entry and issue receipts refer to pre-existing artifacts
> rather than newly created ones. Do not fabricate receipts.

**Why:** `mcp__drm-copilot__potential_to_issue` has no idempotent path; it always creates a new
issue. A default-behaviour child following `feature-promotion-lifecycle` literally would file a
duplicate of every backlog issue the epic is supposed to close. The `preparation` route still
wants promotion receipts, so the child must record the pre-existing artifacts truthfully instead
of inventing them.

Two related traps seen in the Lane A run (2026-08-10):
- The promoted potential file under `docs/features/potential/promoted/` is often far richer than
  the GitHub issue body, which frequently reads "(not provided in potential file)" in every
  section. Point children at the promoted file, not just `gh issue view`.
- A backlog issue marked `- Work Mode: minor-audit` whose body has no populated
  `## Acceptance Criteria` fails the minor-audit eligibility check. Per
  `feature-promotion-lifecycle`, fail closed to the full path (`full-bug`) and say so in the
  prompt, rather than letting the child discover the integrity failure mid-run.

**How to apply:** Check `gh issue view <N> --json state` for every candidate during the
epic-worthiness gate. Any already-open issue gets this paragraph in its delegation prompt.
Related: [[concurrent-prep-children-worktree-isolation]],
[[check-inflight-branches-before-decomposition]].
