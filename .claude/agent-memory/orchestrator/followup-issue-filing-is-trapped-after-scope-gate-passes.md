---
name: followup-issue-filing-is-trapped-after-scope-gate-passes
description: Once a plan's scope-boundary gate has passed and CI is green, there is no safe way to file a follow-up issue from the item worktree — gh issue create is hook-blocked and the MCP promotion path adds a file that falsifies the gate
metadata:
  type: project
---

Filing a follow-up issue at the END of an item run is trapped from both sides. Discovered 2026-09-07 on
parallel item #796 after feature review produced three non-blocking findings worth keeping.

**Side 1 — the direct route is hook-blocked.**

```
gh issue create --repo ... --title ... --body-file ...
-> PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved
   drm-copilot MCP promotion path
```

**Side 2 — the sanctioned route writes a file, and by then a file is exactly what you cannot add.**
`mcp__drm-copilot__new_potential_bug_entry` creates `docs/features/potential/<slug>.md`. Neither destination
is safe once the run has reached this point:

- **Item worktree.** The plan's scope-boundary gate (P9-T10 on #796) enumerates a *permitted* path set —
  feature folder, write-set paths, and this item's own promoted record. A new potential-entry path is in none
  of them, so committing it falsifies a gate that has already passed and been checked off. Worse, any new
  commit moves the branch head and cancels or supersedes the green CI run just measured, which the kickoff
  explicitly forbade. This is the same class as
  [[footprint-ac-forbids-onbranch-followup-promotion]], now confirmed to bind at the CI stage too.
- **Session worktree.** An untracked promotion file there can be swept onto an unrelated sibling's branch by
  a repository-wide stage, the failure mode in
  [[feedback_git_add_a_sweeps_unrelated_queued_promotions]].

**How to apply.** Do not force it, and do not silently drop the findings either. Both are avoidable:

1. Enumerate every finding in the **Follow-ups section of the PR body**. That persists on GitHub
   independently of any working tree and survives the feature folder's eventual archival.
2. The committed `code-review.<ts>.md` merges into main with the branch, so the detail survives there too.
3. Record a `followup_issue_filing_deferred` block in the checkpoint naming what would have been filed, why
   it was not, and that the parent should file it from a checkout where adding a potential-entry file has no
   scope-gate or CI consequence.

**The real lesson is about timing, not about the hooks.** Both obstacles are consequences of filing *after*
the scope gate and CI have locked the footprint. A latent defect noticed mid-run should be promoted while the
write set is still open, when the plan can absorb the extra path. Once P9's scope gate is checked off, the
window has closed. This does not contradict [[feedback_promote_latent_defects_to_issues]]: promotion is still
required, and the finding still has to reach a real issue — what changes is who files it and from where.
