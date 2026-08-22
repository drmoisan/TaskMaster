---
name: preflight-absolute-zero-gate-on-sibling-owned-assembly
description: In an epic, a child plan that demands "Failed 0" on a whole test assembly can be unsatisfiable when a concurrent sibling child owns known intermittent failures in that same assembly; also, preparation-mode plans that pin an absolute WS path.
metadata:
  type: project
---

Two preflight defect classes found on the #445 keyboard-action-contract plan
(`docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/plan.<TS>.md`),
both invisible unless you cross-read the epic manifest.

**1. Absolute-zero test gate on an assembly a sibling child owns.**
The plan required `QuickFiler.Test` `Failed 0` unconditionally in two tasks, while applying a
baseline-subset tolerance repo-wide. The epic manifest
(`docs/features/epics/quickfiler-suite-determinism-foundation/epic.md`) documents two
*intermittently failing* tests in that same assembly
(`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`) owned by a different
wave-0 child (#511/#571) that is NOT guaranteed to have merged first.

**Why:** the gate can fail for a reason the executing child cannot fix, and blocking is forbidden
after `[P0-T1]`, so the executor is deadlocked or pressured to weaken a test.

**How to apply:** when preflighting an epic child, list every assembly the child asserts an absolute
pass/fail count on, then grep the epic manifest for known-failing tests in those assemblies. Where a
sibling owns a failure, the delta is to extend the plan's own baseline-subset rule to that assembly
while keeping absolute zero on the test classes the child actually owns. See
[[project-511-is-a-testhost-crash-not-n-failing-tests]] and
[[winformspumphost-tests-load-flaky]] for why that assembly is unreliable.

**2. Preparation-mode plan pinning an absolute workspace root.**
The plan's `## Resolved Environment` pinned `WS` to the *planning* worktree and labelled the block
"verified; use these, do not re-derive". That worktree was already deleted, and preparation-mode
plans execute later in a third, not-yet-created worktree, so no absolute literal is ever correct.

**Why:** the "do not re-derive" instruction forbids the one micro-action that would repair it, which
is what turns a stale path from self-healing into blocking.

**How to apply:** on any preparation-mode preflight, diff the plan's pinned workspace root against
`git worktree list` first. The delta is to replace the literal with
`git rev-parse --show-toplevel` resolved at execution time. Repo-local tool paths that live in the
PRIMARY clone (for example `<primary>/.dotnet-sdk/dotnet.exe`) are NOT the same defect: they resolve
from any worktree and stay valid.

**3. Iteration-2 resolution shape (verified 2026-08-21).** Both defects cleared on re-preflight. Two
things were worth proving rather than assuming:

- A "failing set is a SUBSET of the baseline failing set" condition is by itself sufficient to catch
  a genuine new regression anywhere in the assembly, including outside the classes the child owns:
  a new failure is absent from the baseline set, so the subset test fails. Adding "and no failure in
  <owned classes>" only converts the owned classes to absolute zero; it does not rescue the rest.
- Unpinning `EXIT_CODE:` on a vstest task that tolerates a pre-existing failure is coherent and NOT
  a weakening, provided the downstream AC check-off still conjoins "all stage exit codes are 0".
  vstest exits non-zero on any failure, so that conjunct silently re-imposes Failed 0 for the
  check-off while the escape clause records the gap.

**Namespace gotcha:** `QuickFiler.Test`'s test classes live in namespace `QuickFiler.Controllers.Tests`,
so a fully-qualified test name from that assembly does NOT contain the string `QuickFiler.Test`. Any
gate phrased as "the `QuickFiler.Test` portion of the baseline failing set" cannot be evaluated by
FQN string match; partition by run scope instead.
