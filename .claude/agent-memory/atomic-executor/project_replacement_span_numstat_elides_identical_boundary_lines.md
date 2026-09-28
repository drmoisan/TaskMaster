---
name: replacement-span-numstat-elides-identical-boundary-lines
description: Plan arithmetic that derives numstat insertions/deletions from the size of a replaced block is wrong whenever the old and new blocks share a first or last line, because git reports those as context
metadata:
  type: project
---

A plan that says "replace lines N to M with this K-line block" and then gates on
`git diff --numstat` reporting `K  (M-N+1)` is computing the wrong numbers whenever the old span and
the replacement block share their first line, their last line, or both. Git emits an unchanged line
as **context**, not as a deletion-plus-insertion, so the reported figures are the sizes of the
*interior* difference.

Measured on #895 (2026-09-17). `[P3-T1]` replaced a 9-line `<remarks>` XML-doc block with a 13-line
one; both open with `/// <remarks>` and close with `/// </remarks>` at the same indentation. The plan
gated `CHANGED_LINES=22` and numstat `13  9`. Observed: `CHANGED_LINES=18` and `11  7`, with hunk
header `@@ -364,7 +364,11 @@` naming the elision exactly. A later task then gated "the numstat
deletions figure is 9" and inherited the same error.

Net line count is unaffected and is the safe quantity: 466 + 13 - 9 and 466 + 11 - 7 both give 470.

**Why:** the arithmetic is done at authoring time against the *edit instruction*, which is a span
replacement, while the gate reads git's *rendering*, which is a minimal diff. Nothing in the plan
text reveals the gap, and the shared boundary lines are usually deliberate — here the plan's own
revision record had just added the opening and closing tags to the replacement block precisely so the
element the AC is worded about would survive, which is what created the elision.

**How to apply:** at preflight, treat any numstat literal derived from a replaced-span size as
suspect when the quoted replacement block's first or last line also appears in the quoted original.
Prefer gating the net line count of the file, plus a property-level assertion such as
"every changed line begins with `///`", both of which are convention-independent. During execution it
is too late to block: record the observed figures, quote the hunk header as the proof of cause,
show that the substantive claim (here comment-only, and the surviving element) is measured by a
different clause that does hold, and escalate the literal as a plan defect in the completion report.

Related: [[project_literal_assertions_inherit_research_arithmetic]],
[[project_msbuild_log_token_search_matches_csc_command_line]].
