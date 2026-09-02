---
name: quickfiler-bug-family-446-shipped-issues-left-open
description: The quickfiler-bug-family-446 feature delivered #446, #448 and #426 to main via PR #625, but the PR targeted the epic integration branch rather than main, so its correct closing keywords never fired and the issues stayed OPEN
metadata:
  type: project
---

`docs/features/active/quickfiler-bug-family-446` on `main` delivered **#446, #448 and #426**
(plus the producer half of #427, scope 427-A, which was deliberately left open). Delivering commit
for #448: `7ebb98df fix(448): terminate the undo consumer, reset the idle timer on every take, reset
the task in finally`, an ancestor of `origin/main`. 27 of 28 ACs checked.

**Why this family matters — it is the counterexample to "this repo does not use closing keywords."**
Every earlier family left its siblings OPEN because no closing keyword was written. Here the
keywords WERE written, deliberately and verifiably: AC17 is an entire scope-containment criterion
asserting that the PR body "carries closing keywords for **#446, #448 and #426 only**", and it is
checked with evidence. The issues stayed OPEN anyway.

The mechanism is the PR BASE. `gh api repos/<owner>/<repo>/commits/<sha>/pulls` reports PR #625 with
`base=epic/quickfiler-bug-family-integration`, merged 2026-08-26. GitHub auto-closes a referenced
issue only when the PR merges into the repository's DEFAULT branch, so keywords on a PR targeting an
epic integration branch never fire. The work then reaches `main` later by an integration merge that
carries no keyword of its own.

**How to apply:** This is the sixth confirmed family and it generalizes the rule rather than
repeating it — issue state is decoupled from delivery on this surface whether or not closing keywords
were used, so never read OPEN as outstanding. When a bare-number grep finds a delivering commit, one
`gh api .../commits/<sha>/pulls` call showing a non-`main` base explains the OPEN state outright and
costs nothing. Any epic-surface family is a candidate for this variant, because the epic surface
merges into an integration branch by construction. See
[[verify-delivery-before-preparing-an-admission]].

Residual: only **AC28** (family-wide coverage) is unchecked. The feature-audit adjudicates it PARTIAL
and non-blocking — both operative sub-conditions PASS, and the literal whole-type >= 90% component is
a spec self-contradiction unreachable without violating AC18 by annexing five sibling-owned partial
files. Its stated remedy is a **maintainer spec amendment**, not code, so it flags no deliverable.
The audit's real residuals were routed through the promotion lifecycle (notably the
`QfcFormController.Actions.cs` testability-seam debt, code review CR-1); those promoted issues are
the admissible items, judged on their own merits.
