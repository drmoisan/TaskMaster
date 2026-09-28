---
name: line-ceiling-counts-total-lines
description: The 500-line file ceiling measures TOTAL lines, not non-blank lines; never "correct" a spec's total-line figure to a non-blank count
metadata:
  type: feedback
---

The 500-line ceiling in `.claude/rules/general-code-change.md` measures **total** line count. When a spec records a file as "N lines" for ceiling purposes, N is the total, newline-terminated line count. A non-blank or non-comment count is a different and wrong quantity for this gate.

**Why:** an earlier attempt at the #792 spec edit inverted exactly this, substituting non-blank counts for total counts, and had to be halted. The user called the convention out explicitly to prevent a repeat.

**How to apply:** when verifying or editing a file-size figure, measure total lines (e.g. `Grep` with pattern `^` in `count` mode, which counts every line). Do not "fix" a figure that looks high by re-measuring non-blank lines. Also re-measure in the tree the figure is meant to describe — see [[measure-line-counts-in-the-item-worktree]]; a stale session worktree yields off-by-N values that look like real drift.
