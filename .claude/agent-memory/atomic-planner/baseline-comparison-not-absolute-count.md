---
name: baseline-comparison-not-absolute-count
description: Three plan-level anti-patterns that produced a 5-round preflight failure on #493 — absolute counts on unowned files, uninitialised fields, and signature-literal assertions the same task invalidates
metadata:
  type: feedback
---

A discarded #493 plan ran five preflight rounds without clearing; round 5 regressed the exact defect
round 4 had fixed. The three classes and the structural fixes that make them unrepresentable:

**1. Never assert an absolute count of diagnostics, warnings, or matches for a file the plan does
not create or own.** The old plan had one task assert "the diagnostic set naming
`FocusAndThemeTests.cs` is IDENTICAL to the Phase 0 baseline" and a later task assert "0 diagnostic
lines name that file". The second is strictly stronger, contradicts the first, and fails whenever
the baseline is non-empty for reasons outside the feature's control. Fix: capture the set ONCE in a
named Phase 0 artifact, state the comparison ONCE in one task, and have every later task **cite that
task's artifact** instead of rewording the condition. Put a `## Notes` rule naming the one task ID
allowed to state it. This is the general form of [[thread-granted-discharges-through-consumers]]:
a condition restated in two tasks will drift between them.

**2. Every field a plan instructs the executor to declare must carry a stated initializer**, or be
named as definitely assigned in a stated constructor (static field to static constructor; instance
field to *every* instance constructor). `/p:TreatWarningsAsErrors=true` promotes `CS0649`/`CS0169`
to build errors, so a bare `private static readonly object FieldLock;` makes every downstream
`EXIT_CODE: 0` build acceptance unreachable. Write the declarations out verbatim in a plan appendix;
do not leave the initializer to executor discretion.

**3. Never assert a source-text signature literal that your own instruction in the same task
changes.** The old plan told the executor to make a method `async` while asserting the pre-`async`
signature text. Fix ladder: (a) name a test and assert its pass count; (b) if a search is
unavoidable, assert a short single-line non-interpolated token and quote it verbatim in plan prose
outside the command span (G5/G6 exoneration); (c) prefer *absence-after-deletion* tokens, which are
consistent with the instruction that removes them. A compile-level fail-before is best expressed as
"N lines of the build log contain both `<the new test file name>` and `error CS`", never as an
error-code literal, because the message text of `CS0029` does not carry the method name.

**How to apply:** before handing a plan to preflight, sweep it for (i) any numeric or "zero"
acceptance clause whose subject is a file outside the Scope Lock, (ii) every `private`/`readonly`
declaration in a plan appendix, and (iii) every quoted source fragment that a sibling clause in the
same task mutates. Also apply rule 4 of the delegation: fewer, sharper tasks — the discarded plan
was ~59 KB, and size itself was the defect source, because each restated literal is a new
contradiction surface.
