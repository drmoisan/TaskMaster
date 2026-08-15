---
name: plan-fenced-powershell-comments-look-like-headings
description: PowerShell/shell comment lines starting with `#` at column 0 inside a plan's fenced code block are heading-shaped; indent them so a line-based plan validator cannot read them as markdown headings
metadata:
  type: feedback
---

When a plan folds command text into fenced code blocks, any comment line that begins with `#`
at column 0 (`# 1. Enumerate ./lines/line`) is indistinguishable from a markdown `#` heading to a
line-based parser. Prefix such lines with two spaces (` # ...`) — still valid PowerShell, no longer
heading-shaped.

**Why:** the MCP plan validator is line-oriented and strict about heading form (see
[[plan-validator-phase-heading-constraint]]). A pseudo-code comment block inside a fence produced
eight extra "H1" lines in the #441 plan before they were indented away. The failure mode is silent:
the heading check may pass locally and fail in the validator, or worse, a nonconforming heading is
reported against a line the author never intended as a heading.

**How to apply:** after writing any plan containing fenced `powershell`/`bash` blocks, grep the plan
for `^#{1,6} ` and confirm every hit is a real heading (H1 title, `##` section, `### Phase N — ...`).
Same check catches stray `#` comments in embedded Python/YAML.
