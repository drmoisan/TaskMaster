---
name: declaration-only-seam-task-for-fail-before
description: When regression tests reference internal seams that do not exist yet, the whole test ASSEMBLY goes red and no test in it can run — order compile-clean tests first, then open Phase 2 with a declaration-only seam task so every test fails at assertion time
metadata:
  type: feedback
---

A C# `[expect-fail]` phase whose tests reference not-yet-existing `internal` members produces a compile error, not a failing test. Because the failure is per-ASSEMBLY, it also destroys the assertion-time evidence of every other test in that phase, including ones that would have compiled fine.

Two ordering rules fix it:

1. **Order the fail-first phase so every compile-clean test is authored and RUN before the first compile-breaking one.** Reflection/structural tests and argument-guard tests against existing members compile today; marshalling tests through a new internal constructor and state assertions through a new internal observation member do not. Run and record the former group first.
2. **Open the fix phase with a declaration-only seam task.** It declares exactly the new members (constructor overload, `internal bool` observation properties, nullable collaborator field) with no behaviour change, and its acceptance is `EXIT_CODE: 0` on a `/t:Build`. The next task then runs the WHOLE test set and records "N discovered, N failed, 0 passed, 0 build errors" — that artifact, not the compile errors, is the authoritative fail-before record, and no behavioural fix precedes it.

Give each compile-time-red task its own falsifiable acceptance anyway: "a `[TestMethod]` named `<X>` exists in `<file>` AND the build is red with the error list recorded verbatim". The method name is a distinct literal per task, so two tasks that share the same broken build are still separately checkable. Quote every such method name in a plan prose block so rule G5 exonerates it.

**Why:** on the #476 plan, six of eight Phase 1 tasks referenced `internal WebView2BreadcrumbHost(` / `IsAttached` / `HasUiDispatcher`. Without this shape the plan would have claimed a fail-before it could not show, and a preflight reviewer cannot tell a genuine assertion failure from a typo that broke the build.

**How to apply:** whenever a bug plan's tests need a NEW seam, sort the phase by "compiles against HEAD" first, then add the declaration-only task and the whole-set fail-before run as the first two tasks of the fix phase. Related: [[acceptance-edits-must-be-false-before-true-after]], [[one-ac-per-checkoff-task]].
