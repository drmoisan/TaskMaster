---
name: enumerate-condition-outcomes-before-case-list
description: When a plan task must deliver 100% branch coverage on a member, count condition outcomes (2 per condition in every ||/&& clause) and derive the test-case list from them — never write the case list from intuition
metadata:
  type: feedback
---

When a plan task commands tests for a member whose coverage requirement is 100% **branch** rate, derive the case list by counting condition outcomes, not by reasoning about which inputs "look distinct". Every condition in every `||`/`&&` clause contributes **two** outcomes. Walk each candidate case through the expression, mark which outcomes it drives, and only stop when all are marked.

**Why:** #418 remediation cycle 1, preflight blocker B-1. On `return a == b || (a != null && a.Length == 0) || (b != null && b.Length == 0);` — five conditions, ten outcomes — an intuitive seven-case list (both null; null/empty; empty/null; null/non-empty; equal; unequal same length; unequal lengths) reached only 8/10. The missing case was **non-empty/null**, which alone drives the two stragglers (`a.Length == 0` false and `b != null` false). Because two separate places required the artifact to *state* 100% for the owning class, the executor's only options would have been to write something false or to author an unplanned test mid-execution.

**How to apply:** read the member body before writing the case-enumeration task. For a null-pairing guard, expect all **four** orderings (null/null, null/value, value/null, value/value) plus the empty-vs-non-empty variants, not three. Confirm the tool's counting granularity against an existing artifact in the same repo rather than assuming: this feature's `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` records `if (file == null || parse == null)` at 4/4 conditions, which pins two outcomes per clause. State the outcome arithmetic inside the task text so preflight and the executor can both check it. Related: [[named-coverage-exception-verify-member-body]], [[coverage-gate-clr-invoked-private-members]], [[csharp-pure-move-extraction-pattern]].
