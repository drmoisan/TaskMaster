---
name: cobertura-emits-line-nodes-for-some-comment-lines
description: The .coverage-to-Cobertura conversion emits hit line nodes for some comment lines, so "comment lines carry no line node" is not safe as a changed-line-coverage premise
metadata:
  type: project
---

Changed-line coverage plans routinely justify dropping comment lines from the denominator
with "a comment line carries no Cobertura `line` node at all". That is mostly true and
NOT reliably true.

Measured on issue #796 (2026-09-07), final Cobertura from
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`: of 144 comment-only changed lines across
five QuickFiler files, 139 carried no `line` node and **5 did** — lines 442-446 of
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, each with `hits=1`. Those are ordinary
`//` comments immediately above a mapped `if`. Lines 251-254 of the *same file*, also
`//` comments above a mapped `if`, carried no node. So the behaviour is not uniform even
within one file; the likely mechanism is the conversion mapping the full source span
preceding a statement, but that was not established.

**Why:** those five lines were all covered, so including them *raises* the changed-code
figure. A gate that silently benefits from a mechanism the plan asserts does not exist is
not a gate you can defend at review.

**How to apply:** compute the denominator mechanically as `changed lines ∩ lines carrying
a Cobertura line node` (which is what the plan usually literally specifies), then report a
second figure with all comment-only lines removed from BOTH numerator and denominator,
and state that the threshold holds on both. For #796: 40/41 = 97.56% mechanical,
35/36 = 97.22% comment-stripped, 40/42 = 95.24% with no exclusion at all. Report all
three and say plainly that none was selected after seeing which passed.

Related: [[changed-line-coverage-cobertura-vs-mscoverage-partial]],
[[processed-cobertura-filenames-use-backslash]] (group class nodes by the backslash
`filename`, and take the max `hits` when two nodes both carry a line).
