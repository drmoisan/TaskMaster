---
name: per-file-line-budget-collides-with-verbose-comment-instructions
description: A plan that sets a no-growth per-file line budget AND tells you to write multi-line justification comments at each call site will overshoot; budget the comment lines before writing them.
metadata:
  type: project
---

An atomic plan can carry two acceptance conditions that pull opposite ways on the same file:
a per-file line budget (issue #799 decision D11 gave `FolderPredictor.cs` a no-growth budget
of 1003 lines) and a per-call-site instruction to write the exact expression
`Helper.Method(x, root)!` "with the null-forgiving operator and a one-line comment giving its
reason" at several sites.

**Why:** the planner's slack derivation assumed each added call site costs about two lines.
A faithful call site with a two-line reason comment, a hoisted root local, and a
CSharpier-wrapped `Select(...)` costs seven or more. On #799 the first faithful pass landed
at 1007 against a 1003 budget and needed three rounds of comment trimming to reach 1002.

**How to apply:** before writing the call sites, add up the plan's own stated collapses
(what shrinks) against a realistic per-site cost (what grows), and measure the file with
`(Get-Content -LiteralPath <path>).Count` immediately after each edit rather than at the end.
Keep reason comments to one or two lines, hoist repeated sub-expressions to a short local so
the call fits under CSharpier's 100-column print width on one line, and avoid a wrapped
lambda where a hoisted local avoids it. Do not solve the overshoot by replacing the plan's
literal expression with a private helper: the acceptance condition names the expression, and
the budget is satisfiable without deviating.

Related: [[project_csharpier_requires_blank_line_before_comment_breaking_numstat_bounds]],
[[project_appglobalstests_at_500_line_ceiling]].
