---
name: read-the-clock-with-git-var-when-pwsh-is-refused
description: In a worktree-isolated session with pwsh refused, `git var GIT_COMMITTER_IDENT` is the only clock; read it at each delegation, because receipts need started_at and completed_at and estimated times drift ahead of real time
metadata:
  type: feedback
---

An isolated session has no allowed clock command: pwsh is refused, and poetry has no pyproject in
TaskMaster. `git -C <worktree> var GIT_COMMITTER_IDENT` prints `<name> <email> <epoch> <tz>`, which
is the current time. Example: `1790771310 -0400` = 2026-09-30T12:28:30Z = 08-28 local.

**Why:** on #945 preparation I wrote per-receipt `completed_at` values by estimation. When I finally
read the clock, three of them were later than the real current time, so they were fabricated rather
than approximate. Separately, the orchestrator-state validator requires both `started_at` and
`completed_at` on every `delegation_receipts.agents[]` entry. It accepts a date-only value, which is
the honest fallback once the times were not captured. See [[evidence-timestamps-can-be-synthesized]].

**How to apply:** run `git var GIT_COMMITTER_IDENT` immediately before and after each delegation,
and write both values into the receipt as you go. Never back-fill a receipt time from memory.
