---
name: preparation-child-cwd-is-session-root-not-item-worktree
description: An orchestrator child's actual Bash process cwd is the session root, never the item worktree — so PreToolUse hooks read docs/ and artifacts/ from the session root; applies to EXECUTION-mode resumes too, not only preparation mode
metadata:
  type: feedback
---

When a parallel-orchestrator (or epic-planner) delegates a preparation-mode child with
`Agent(orchestrator, isolation: "worktree")`, the child does NOT get an automatically
isolated cwd. `git rev-parse --show-toplevel` from the child's own Bash tool returns the
SESSION ROOT (the parent orchestrator's own worktree), not any item-specific directory —
confirmed on issue #751's prep child: cwd was `TaskMaster-wt/2026-09-02T08-47` on branch
`TaskMaster-wt-2026-09-02T08-47` throughout, even after manually creating and populating
`TaskMaster-wt/prep-751` as the "isolated worktree" the delegation prompt described.

This matters because PreToolUse hooks that gate Agent-tool delegations (e.g.
`enforce-prd-feature-before-planner.ps1`, and `enforce-model-routing-receipt.ps1`'s
single-feature fallback path) resolve relative paths — `docs/features/active/<folder>/issue.md`,
`artifacts/orchestration/orchestrator-state.json` — against the hook PROCESS's cwd, i.e. the
session root, never against the item worktree the child created for its actual deliverable
branch/commits.

**Why:** `enforce-model-routing-receipt.ps1` only redirects to a per-item checkpoint when the
delegation prompt carries `Parallel mode: true` or `Epic mode: true` (see
model-routing-hook-reads-canonical-path-only.md). A plain `Preparation mode: true` marker gets
NO special-case handling in that hook — it falls through to the single-feature path, which reads
the SESSION ROOT's shared `artifacts/orchestration/orchestrator-state.json`. This may coincidentally
contain receipts for the exact agents you delegate to (from an unrelated prior run in the same
session), letting early delegations through by luck rather than correctness. The
`enforce-prd-feature-before-planner.ps1` hook has NO such redirect at all — it will always deny
an atomic-planner delegation with `PRD_FEATURE_BLOCKED` if the resolved feature folder's issue.md/
spec.md don't exist at the session-root-relative path, even though they exist correctly in your
item worktree.

**How to apply:** As soon as you create issue.md/spec.md/plan.md/research/ in your item worktree,
also copy (`cp`, not `git mv`) each into the SAME relative path under the session root (untracked
there — this mirrors the existing convention of stray untracked feature folders already observed
in session roots, e.g. `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/`).
Re-copy after every plan revision. Do NOT try to `cd` into the item worktree — Bash tool cwd resets
between calls per the harness, and even git -C doesn't change the PreToolUse hook's own process cwd.
The actual git commits/push for your deliverable branch still go through the item worktree via
`git -C <item-worktree-path> ...`; only the mirror copies are for hook satisfaction.

**This is not preparation-mode-specific.** Re-confirmed 2026-09-04 on issue #736 in an
EXECUTION-mode resume — a child launched with no isolation parameter, resuming at S5 against an
item worktree that already existed and already held every document. `Agent(atomic-planner)` was
denied twice in a row: first `PRD_FEATURE_BLOCKED ... the '- Work Mode:' marker is absent`, then,
after mirroring issue.md alone, `PRD_FEATURE_BLOCKED ... is missing: spec.md (work mode: full-bug)`.
Both files existed correctly in the item worktree the whole time. The hook resolves the folder NAME
from the delegation prompt text but reads its CONTENTS against the session root, so a partial mirror
just moves the error to the next required file. Mirror the whole folder in one step rather than
discovering the required set one denial at a time.

One hazard the mirror creates: it puts a second copy of plan.md on disk. Name the real absolute
path in every delegation prompt and say explicitly that the session-root copy is a decoy, or a
delegate may edit the mirror and its work is silently discarded.

See also [[model-routing-hook-reads-canonical-path-only]] for the parallel/epic-mode fix that this
gap does not share.
