---
name: collect-pr-context-races-across-concurrent-children
description: collect_pr_context writes into the shared main checkout regardless of workspace_root, so concurrent epic children silently overwrite each other's PR context; require a head-SHA check, never a presence or mtime check
metadata:
  type: feedback
---

Tell every child in a concurrent fan-out that `mcp__drm-copilot__collect_pr_context` is NOT
worktree-safe, and that it must verify the context it reads belongs to its own branch by comparing
the artifact's recorded head SHA against its own `git rev-parse HEAD`.

**Why:** The tool returns `ok: true` and echoes worktree paths, but it writes its output into the
PRIMARY checkout, which every concurrent child shares. In the QuickFiler determinism epic, child
#491 overwrote child #449's context between #449's collection and its use; the file #449 then
copied resolved to `bug/quickfiler-test-form1-live-form-491-exec` and enumerated #491's evidence
folder. #449 caught it only because it compared head SHAs. A presence check and an mtime check both
PASS on a sibling's freshly written file, so neither detects the swap — the sibling's file is newer
than your own would have been. Filed as issue #589. Child #445 independently hit the adjacent
failure: the tool wrote to the primary checkout rather than the supplied `workspace_root`, claimed
`gh` was unavailable while `gh` was authenticated, and left a stale worktree copy recording a head
SHA one commit behind. The blast radius is a PR body that describes a different feature's diff and
passes review because it reads coherent.

**How to apply:** Put the hazard in the kickoff prompt of every child you launch concurrently, not
just in your own notes — the child is the one that calls the tool. Instruct it to (1) compare the
artifact's head SHA to its own `HEAD` before using it, and (2) fall back to authoring the PR body
from `git diff <base>...HEAD` on its own branch when the SHAs disagree, which is what both #445 and
#449 ended up doing. As epic-orchestrator you are exposed too, but only mildly: your own
`collect_pr_context` call for the final integration pull request runs after every child has
finished, so the contention window is closed — still perform the head-SHA check rather than assume
it. Related: [[project_pr_author_is_inline_skill_not_agent]] and
[[project_cross_child_annotation_fanin_debt]].
