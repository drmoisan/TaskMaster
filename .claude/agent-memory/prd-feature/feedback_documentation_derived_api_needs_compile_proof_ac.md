---
name: documentation-derived-api-needs-compile-proof-ac
description: When a research record verifies a third-party API by reading shipped XML docs (or any non-IL source), the spec must carry the hedge into the text AND add an acceptance criterion demanding a compile-time proof plus a named fallback
metadata:
  type: feedback
---

When a research artifact establishes that a third-party member exists by reading the package's
shipped XML documentation file, a NuGet listing, or vendor prose — anything short of the IL or an
existing in-repo call site — the spec must do three things, not one:

1. Repeat the hedge verbatim in the spec ("the research read the shipped XML documentation file, not
   the IL"), rather than promoting it to a flat statement of fact.
2. Make the proof an acceptance criterion whose PASS condition is a *captured build log* showing the
   real call site compiling. Explicitly mark "a prose assertion of availability, or a citation of the
   package XML alone" as a FAIL, or the criterion is satisfiable by restating the research.
3. Name the fallback design in Proposed Fix so a `CS1061` does not silently push the executor back to
   the option the spec rejected.

**Why:** On #825 (2026-09-09) the delegating agent required exactly this shape for
`TimeProviderTaskExtensions.CreateCancellationTokenSource` in `Microsoft.Bcl.TimeProvider` 10.0.11.
The repository also has no restored `packages/` directory inside an agent worktree, so a NuGet
restore is a precondition of the proof — the experiment cannot be run casually, which is precisely
why it must be scheduled as a gate rather than assumed.

**How to apply:** Pair the compile proof with a runtime proof whenever one is cheap. On #825 the
deterministic regression test (an `ArmingBarrierTimeProvider` whose `Armed` signal completes only if
a timer was created on the injected provider) doubles as empirical verification of the shim's
semantics, so one test discharges an assumption the documentation could only suggest. Related:
[[ac-gates-verify-satisfiability]], [[invariant-and-trace-in-proposed-fix]].
