---
name: prd-feature-deny-string-blames-the-marker-not-the-cwd
description: PRD_FEATURE_BLOCKED saying the '- Work Mode:' marker is absent quotes the CORRECT folder and misdirects — folder resolution succeeded and a relative Test-Path at line 108 failed; hits standalone children, not just parallel items
metadata:
  type: project
---

`enforce-prd-feature-before-planner.ps1` denies an `Agent(atomic-planner)` delegation with:

> `PRD_FEATURE_BLOCKED: resolved feature folder '<folder>', but its work mode could not be determined
> from '<folder>/issue.md' (the '- Work Mode:' marker is absent, unreadable, or unrecognized). Confirm
> that is the intended feature folder, then add or correct the '- Work Mode:' marker in that file so the
> prerequisite set can be derived.`

**Do not act on that instruction.** The marker is almost certainly already correct. The message quotes
the RIGHT folder, which makes it read as a content defect in a file you just wrote; it is not one.
Folder resolution succeeded. The failure is one layer later.

**Mechanism.** `Get-PrdFeatureIssueContent` line 107 builds `$issuePath = "$FeatureFolder/issue.md"` as
a repo-RELATIVE string, and line 108 stats it with `Test-Path -LiteralPath`. That resolves against the
hook process working directory, which is the Claude **session root** — not the worktree the child was
directed to. Line 109 returns `$null`, and `Resolve-PrdFeatureWorkMode` then reports the marker
unreadable.

**It is not parallel-specific.** Verified 2026-09-13 on issue 877 in a plain **standalone child
orchestrator** handed a dedicated worktree by a coordinator: no parallel run, no epic, no
`Parallel mode: true`. Any topology where the feature folder lives in a worktree other than the session
root triggers it. The sibling memory that records this only for parallel items understates the blast
radius.

**Two cheap checks that separate this from a genuine marker defect, in seconds.** Run both before
believing the message:

1. Grep `^- Work Mode:` in the **worktree** copy of `issue.md` — present and well-formed.
2. `Test-Path` the same repo-relative path under the **session root** — `False`.

That pair is decisive. A real marker defect fails check 1; this artifact fails only check 2.

**How to apply.** Escalate to the coordinator, which runs in the session root and satisfies the hook
natively, and record `blocked_reason: delegation_launch_failed` with the verbatim deny string. Refuse
all three workarounds: the hook's multi-candidate `orchestrator-state` fallback resolves a DIFFERENT
item's folder and would admit the delegation against a sibling's documents; shimming your folder into
the session root dirties a tree you do not own; self-authoring the plan defeats the gate the hook
exists to enforce. The only legitimate repair is the coordinator publishing the child's OWN genuine
`issue.md` to the path the hook reads.

Related: [[prd-feature-hook-parses-prompt-paths]], [[agent-worktree-hooks-resolve-to-agent-cwd]],
[[child-orchestrator-pr-hook-reads-session-root]], [[preimplementation-gate-reads-sibling-checkpoint]].
