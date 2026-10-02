---
name: pwsh-array-literal-comma-binds-before-plus
description: In PowerShell @("a" + $x, "b" + $y) is ONE string (comma binds tighter than +), so a plan's multi-pattern scan loop silently tests one malformed pattern
metadata:
  type: project
---

`@("(?i)" + $t, "(?i)" + $h, $shape)` evaluates as `"(?i)" + ($t, "(?i)") + ($h, $shape)`, which is one concatenated string, so `foreach` runs once. Measured on issue #882 P4-T22: the hygiene scan printed only PATTERN-1-HITS=0, and a probe returned an array count of 1.

**Why:** A zero-hit gate over the collapsed pattern is vacuous for every intended pattern after the first, yet it exits 0 and looks green.

**How to apply:** During preflight or execution, when a plan builds an array with `+` inside its elements, parenthesize each element (`@(("(?i)" + $t), ...)`) and emit a `PATTERN-COUNT=` line. Record the correction as a deviation in the artifact. Related: [[project_multipattern_gate_shared_qualifier_detachment]].
