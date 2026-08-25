---
name: supersede-clause-leaves-hard-routing-residual
description: A "the plan's table supersedes the routing recorded here" clause does not neutralise a THIRD location that still names a concrete file; grep every hard file/route name, not just the clause's own section
metadata:
  type: project
---

When a spec is revised to defer a routing/allocation decision to the plan (for example "new tests are
routed per the plan's constraint C2 capacity table, which supersedes the illustrative routing recorded
here"), the revision typically edits the *governing-constraints* bullet and the matching *risk* bullet
and stops. A later "Regression tests to add or update, by issue" bullet that still says "placed in
`EventWiringTests.cs`" survives, and it reads as an instruction, not as illustration.

**Why:** feature 484 round 4. `spec.md`'s governing constraint 6 and risk R2 both deferred to the plan's
capacity table, but the per-issue #480 bullet still hard-named `EventWiringTests.cs`. Following the spec
instead of the plan projected that file to 374 + 106 + 26 = 506 lines — a breach of the 500-line ceiling
that is itself an acceptance criterion. The supersede clause made the contradiction *look* resolved
while leaving a concrete, load-bearing failure path. The same rewrite also left the file-size acceptance
criterion's "Specifically:" list naming the two files that receive ZERO added lines and omitting the
three that receive all of them.

**How to apply:** after any supersede/deferral clause is added, grep the whole document for every
concrete file name, path, or route token in the superseded class and confirm none survives as an
imperative. Then re-derive the arithmetic consequence of obeying each surviving mention — a residual is
only advisory if obeying it stays inside every hard limit. Check the *emphasis lists* attached to
universal criteria too: a "Specifically: A, B" rider that enumerates the low-risk items is a silent
inversion of the gate's intent even when the universal clause keeps it non-vacuous.

Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_preflight_citation_match_propagates_false_fact]].
