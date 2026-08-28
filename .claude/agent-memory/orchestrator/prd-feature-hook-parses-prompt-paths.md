---
name: prd-feature-hook-parses-prompt-paths
description: PRD_FEATURE_BLOCKED can be a false positive — the hook reads a docs/features/active/... path out of your prompt text and may pick a deep evidence subpath as the feature folder
metadata:
  type: project
---

`PRD_FEATURE_BLOCKED` on an `Agent(atomic-planner)` delegation is not always a real
prerequisite failure. The PreToolUse hook resolves the feature folder by scanning the
**delegation prompt text** for a `docs/features/active/...` path, and it can select a
deep subpath rather than the folder root.

Observed on epic child 464: quoting a plan task's `/ResultsDirectory:` argument in the
prompt caused the hook to treat
`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p5-t11`
as the feature folder. It then reported `spec.md` and `user-story.md` missing and the
work-mode marker unreadable, and failed closed to the strictest prerequisite set — even
though the real folder had `spec.md` present and `- Work Mode: full-bug` set correctly.

**Why:** the hook has no way to distinguish a path you are *quoting as data* from the
path naming the feature under work. Forward-slash `docs/features/active/...` strings
anywhere in the prompt are candidates. Backslash-spelled paths and the bare folder root
did not trigger it.

**How to apply:** when a delegation prompt must reference an evidence path, a
`/ResultsDirectory:` argument, or any nested path under a feature folder, do not spell it
out in full. Name the feature folder once at the top, then describe the nested edit
relationally — "change the final path segment of its `/ResultsDirectory:` argument from
`p5-t11` to `p5-t12`". The re-issued prompt succeeded with no other change.

Do not respond to this error by running the promotion or prd-feature step again: for a
`full-bug` child that would wrongly create a `user-story.md`, and for a resumed child it
would duplicate work already committed. Verify the folder on disk first
(see [[small-path-minor-audit-selection]] for the mode-to-document mapping).

Related: [[agent-worktree-hooks-resolve-to-agent-cwd]],
[[model-routing-hook-reads-canonical-path-only]] — both are cases of a hook resolving a
path differently from how the calling agent meant it.
