---
name: prd-feature-hook-picks-longest-active-path
description: enforce-prd-feature-before-planner.ps1 resolves the feature folder from the LONGEST docs/features/active/ token in the delegation prompt, so a deep evidence path in your instructions blocks the atomic-planner delegation
metadata:
  type: project
---

`.claude/hooks/enforce-prd-feature-before-planner.ps1` (`Find-PrdFeatureFolderFromPrompt`, lines
191-233) scans the delegation prompt for `docs[\\/]+features[\\/]+active[\\/]+[^\s"'` + "`" + `]+`, keeps the
**longest unique match**, and — if it ends in `.md` — strips the filename and treats the parent as the
feature folder. It then demands `spec.md` / `user-story.md` in that folder and fails closed with
`PRD_FEATURE_BLOCKED` when they are absent.

**Why this bites:** quoting a deep evidence path such as
`docs/features/active/<feature>/evidence/other/p4-t26-commit.<timestamp>.md` in an `Agent(atomic-planner)`
prompt makes `.../evidence/other` the longest match, so the hook resolves the feature folder to the
evidence subdirectory, finds no `issue.md` there, and denies the delegation. The block is a prompt-parsing
artifact, not a real lifecycle gap — the actual feature folder is fully populated.

**How to apply:** when delegating to `atomic-planner`, make sure no `docs/features/active/...` token in the
prompt is longer than the feature-root or plan path you want resolved. Either quote deep paths in
instruction form ("in its `git add` command, add a second pathspec so it reads ...") instead of pasting the
full literal, or elide the middle of the path. The token stops at whitespace, a double quote, a single
quote, or a backtick, so backticks around the path do not shorten the match.

Related: [[promotion-hook-matches-commit-message-text]] — the same failure mode in a different hook, where
text quoted for documentation is read as if it were the action.
