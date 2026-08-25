---
name: preflight-zerohit-identifier-and-red-test-straddle
description: Two preflight gate traps found on the #468 plan - a repo-wide zero-hit identifier sweep that collides with an unrelated live same-named member, and a full-suite "failed == 0" gate ordered between a deliberately-red test's compile-in and its fix
metadata:
  type: project
---

Two gate shapes that read as rigorous but are unsatisfiable as ordered. Both found at preflight on
`docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md`.

**1. A repo-wide zero-hit sweep for a bare method identifier.** A dead-code-removal plan asserted
"no non-`.cs` file references any of the twelve removed identifiers". Measured: 175 non-`.cs` files
match `LoadSequentialAsync` and 28 match `WireUpKeyboardHandler`. `QfcCollectionController.LoadSequentialAsync`
is dead, but `ApplicationGlobals.LoadSequentialAsync` / `AppToDoObjects.LoadSequentialAsync` /
`AppAutoFileObjects.LoadSequentialAsync` are live and unrelated, and every archived feature folder's
TRX, Cobertura XML, and prose quotes the name. The feature's own spec/research/plan quote all twelve.

**Why:** a bare method name is not a unique key across a repo, and `docs/features/**` is an
ever-growing corpus that quotes every identifier any feature has ever discussed. The same plan
correctly stated the rule ("none is repository-wide, because these identifiers legitimately appear in
`docs/features/**` prose") in its conventions and then violated it one section later.

**How to apply:** before accepting any zero-hit identifier gate, run the search. Scope it to
build-input file types (`.csproj`, `.resx`, `.config`, `.xaml`, `.json`) or require type-qualified
context, and word the acceptance as "every hit reviewed and none is a reference to
`<Type>.<Member>`", not "zero hits". Check the plan's own conventions section for a rule the task
contradicts. Related: [[project_preflight_absolute_zero_gate_on_sibling_owned_assembly]],
[[project_multipattern_gate_shared_qualifier_detachment]].

**2. A full-suite `failed == 0` gate ordered while an `[expect-fail]` test is compiled in.**
Phase 10 created a deliberately-red STA test (T1), added its `Compile Include` (T2), ran it red (T3),
landed a behaviour-preserving seam (T4), then demanded a full-suite run with "failed count of exactly
`0`" (T5) — with the red test still in `QuickFiler.Test.dll` and not fixed until T8. Phases 11 and 13
in the same plan got the order right (seam → seam suite → seam commit → red test), which is what made
the Phase 10 inversion visible.

**Why:** a seam's behaviour-preservation evidence is a whole-assembly run, so it is only expressible
while the assembly holds no known-red test. Once an `[expect-fail]` test's `Compile Include` lands,
every all-assembly `failed == 0` gate until its fix is unsatisfiable.

**How to apply:** walk each `[expect-fail]` test from its csproj insertion to its fix commit and
check every full-suite gate in that window. Fix by moving the seam + seam-suite + seam-commit ahead
of the red test, or by naming the known-red test in a `/TestCaseFilter` exclusion and stating the
expected failed count. Related: [[project_511_is_a_testhost_crash_not_n_failing_tests]].
