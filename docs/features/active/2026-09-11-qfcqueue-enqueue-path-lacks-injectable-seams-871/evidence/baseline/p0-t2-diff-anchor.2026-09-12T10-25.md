# P0-T2 — Diff anchor and pre-existing worktree paths

Timestamp: 2026-09-13T04-50
Command: git rev-parse HEAD ; git status --porcelain --untracked-files=all
EXIT_CODE: 0

BASE_SHA: d44b51932c6094dd54dc60e7c0f8d5f14d9e088d

## Capture ordering

Both spans were executed as the first two actions of this Phase 0 run, before any file this plan
creates or edits existed on disk. In particular the porcelain span below was captured before the
P0-T1 artifact was written and before the Phase 0 check-off marks were applied to the plan file, so
it reproduces the state of the worktree at the anchor rather than a state this plan produced.

## PreExistingWorktreePaths:

```
```

The porcelain span produced no output. The worktree carried no modified, staged, renamed or
untracked path at the anchor, and therefore the second carve-out of the Scope-lock rule contributes
an empty set for the remainder of this plan.

## Consequence for the Scope-lock rule

The plan preamble anticipated that this worktree might carry promotion-lifecycle residuals — a
staged rename of a potential entry into the promoted subdirectory, a staged addition under the
potential features directory, and modified and untracked files under the tracked agent-memory
directory. None of those are present here. The measured anchor set is empty. The Scope-lock rule is
therefore applied for the remainder of this plan with its `PreExistingWorktreePaths:` clause
contributing nothing, which makes the rule strictly narrower than the plan assumed and admits no
path that the plan did not already admit through the Write Set, the three evidence directories or
the tracked agent-memory directory of this worktree.

Output Summary: BASE_SHA recorded as d44b51932c6094dd54dc60e7c0f8d5f14d9e088d, a 40-character
hexadecimal sha. The porcelain status was empty, so `PreExistingWorktreePaths:` is an empty block
and the anchor carve-out of the Scope-lock rule is empty. Acceptance met.
