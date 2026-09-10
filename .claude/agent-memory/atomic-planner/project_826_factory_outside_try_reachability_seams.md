---
name: project-826-factory-outside-try-reachability-seams
description: "#826 preflight R1 seams: an injected-factory seam invoked OUTSIDE the try makes both catch bodies reachable, so a plan that declares them unreachable authorises a substitute test and a caveat check-off it never needed"
metadata:
  type: project
---

Plan `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/plan.2026-09-08T23-52.md`
survived five authoring rounds asserting the opposite of what the code does, because no round read
the *position* of the seam call relative to the `try`.

**Why:** `UtilitiesCS/Threading/TimeOutTask.cs` invokes `timeoutSourceFactory` in the `using var`
initialiser (lines 52-54) and does not open its `try` until line 61. Every earlier round enumerated
the `catch` clauses inside that `try` — `catch (TaskCanceledException)`, `catch (System.Exception e)`
gated by `strict` — concluded nothing escapes, and declared the caller's two catch bodies
unreachable. An exception the injected factory throws is caught by none of them and leaves
`RunWithTimeout` carrying its own type. Sibling feature 825 already shipped a test
(`UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` line 112) that exploits
exactly this, with the mechanism written out in its doc comment at lines 104-110.

**How to apply:** when a plan claims a seam-driven branch is unreachable, make the derivation record
the line number of the seam invocation AND the line number at which the `try` opens, and state
whether one is inside the other. Enumerating `catch` clauses is not sufficient and is what produced
five clean-looking rounds of a false claim. Cascading consequences once the claim inverted: the
`BRANCH: UNREACHABLE` path, its contract-pinning substitute test, the "one uncovered line for one
uncovered line" no-regression identity in the coverage-delta task, and the two spec check-off caveat
notes all had to go, and the single-test task became a two-test task, which in turn changed a TRX
`executed 1` gate to `executed 2` and every singular "the test named by P2-T2" reference.

Related seams found in the same round: a `.editorconfig` carrying lone carriage returns reports 670
lines to git and 1110 to `Select-String`, so two line numbers for the same line are both correct;
and a per-file parameterless-constructor gate is expressed as "count of `new CancellationTokenSource(`
equals count of `new CancellationTokenSource()`", because a bare zero-count on the first token is
unsatisfiable when the parameterless spelling contains it as a prefix.

See [[verify-citations-in-the-assigned-worktree]] and [[acceptance-edits-must-be-false-before-true-after]].
