---
name: mcp-hardlock-and-review-mirror-quirks
description: resolve_execute_hard_lock_prompt needs an ABSOLUTE target when workspace_root is a worktree, and feature-review mirrors its audit artifacts into the shared session root where a sibling can commit them
metadata:
  type: project
---

Two mechanical facts confirmed while resuming epic child 444 from an agent worktree.

**1. `resolve_execute_hard_lock_prompt` requires an absolute `target`.** Passing the
repo-relative plan path together with an explicit worktree `workspace_root` fails:

```
ok:false  Error: Target file not found at docs/features/active/<feature>/plan.<TS>.md
```

The identical call with `target` as the ABSOLUTE path under the same `workspace_root`
returns `ok:true` and writes `<workspace_root>/artifacts/hard_lock_prompt.txt`. So the
resolver does not join `target` onto `workspace_root` the way the parameter docs imply.
The `execute-hard-lock` skill forbids reconstructing the prompt from any other source and
tells you to abort on `ok:false` — so a relative path looks like a hard block when it is
only a path-form bug. **Retry with the absolute path before reporting BLOCKED.**

Same shape as [[potential-to-issue-needs-absolute-path]]; treat absolute-path retry as the
default first move for any drm-copilot MCP tool that reports a missing target in a worktree.

**2. `Agent(feature-review)` writes its three audit artifacts to the worktree AND mirrors
them into the session root.** It did this deliberately, "for hook path resolution". The
mirrors land at the same relative path under the session checkout — which, in an epic wave,
is a *shared* directory that concurrently-live siblings also use and run `git add -A` in.
The mirrors are byte-identical to the worktree copies (verified by SHA-256), so the worktree
copies are authoritative and the mirrors are pure untracked pollution that a sibling can
sweep into the wrong PR.

**How to apply:** after feature-review returns, check the session root for a stray
`docs/features/active/<your-feature>/` directory. Verify the files are byte-identical to the
worktree copies, then delete only those mirrors. Note `rm -rf` is a blocked pattern here —
use plain `rm` on the named files plus `rmdir`.

Related: [[agent-worktree-hooks-resolve-to-agent-cwd]] and
[[child-orchestrator-pr-hook-reads-session-root]] — the PR-author hook genuinely does read the
session root, so body/receipt/checkpoint belong there, but audit artifacts do not.
