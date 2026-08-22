---
name: absolute-zero-gate-on-sibling-owned-assembly
description: An epic child's plan must not assert an absolute "Failed 0" over a whole test assembly that also carries a wave-sibling's known failures; scope absolute zero to the classes the child owns
metadata:
  type: feedback
---

An epic-child plan must not state an acceptance gate the child cannot pass by its own effort. The recurring shape is an absolute `Failed 0` asserted over an entire test assembly, when that assembly also contains failing tests owned by a **concurrent wave sibling**. Scope the absolute-zero condition to the test classes the child owns, and use a baseline-subset condition for the rest of the assembly.

**Why:** in a wave with an empty dependency graph, siblings run concurrently and their fixes are not guaranteed to have merged. The executor then hits a red gate at a point where blocking is forbidden, and its only in-task recourses are to weaken a sibling's test or to record a false pass — both prohibited. Confirmed on #445 (quickfiler-suite-determinism-foundation): `P4-T5`/`P5-T6` pinned `QuickFiler.Test` to `Failed 0`, but that assembly carries #511/#571's two intermittently failing pump tests (`QfcItemController.InitializationTests.Part3.cs`) and #491's live `Form1`, and the runsettings set `<Workers>0</Workers>` — full CPU parallelism, exactly the load condition that makes them fail.

**How to apply:** when preflight proposes relaxing a gate, do not accept the relaxation on the agent's word. Verify the two facts yourself: that the tolerated failures really live in that assembly, and that the sibling really is a concurrent wave peer (`depends_on: []` on both, same wave in the epic manifest). Then require the relaxed gate to remain **falsifiable against the child's own regression**: a baseline-subset condition over test-NAME sets is sufficient, because a failure this child newly introduces is by construction absent from the baseline set, so the subset test still fails. Tolerating two named tests does not tolerate anything else in their class or file.

Two corollaries worth carrying forward:

- Unpinning `EXIT_CODE: 0` is correct once a pre-existing failure is tolerated, since `vstest` exits non-zero on any failure. Keep the `EXIT_CODE:` field mandatory in the evidence artifact; only the pinned value goes.
- A scoped run restricted to a class the child owns (via `FullyQualifiedName~`) should still pin `EXIT_CODE: 0` and an exact Passed count. The relaxation belongs only to the whole-assembly run.

Related: [[preflight-catches-vacuous-gates]], [[plan-phase0-paths-are-stale-in-epic-children]], [[epic-child-pr-gate-gotchas]].
