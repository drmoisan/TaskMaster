---
name: zero-hit-grep-gates-need-carveouts
description: "A 'grep returns zero hits' acceptance criterion is unsatisfiable unless the plan enumerates every legitimate non-target hit AND forbids denial-sentence replacement text"
metadata:
  type: feedback
---

When a plan task's acceptance is "grep for X returns no hits", two failure modes make it unsatisfiable by
construction. Both cost a full preflight revision pass on #494.

1. **Denial-sentence replacement text.** If the task says "remove the false claim about `quality-tiers.yml`"
   and the supplied replacement text says "There is no `quality-tiers.yml` mapping file", the grep hits the
   replacement. Write the task as *removal means deletion of the assertion, not replacement with a denial*,
   and state that the replacement must not contain the literal strings. Then re-read the appendix block and
   confirm it actually complies — the appendix is where the violation hides.
2. **Numerals that are not the numeral you mean.** A `75%` / `85%` sweep over `.claude/rules/quality-tiers.md`
   hits `| Mutation score | >= 75% | trend-only | none | none |` (a tier-dependent, non-coverage gate that is
   also out of a coverage feature's edit scope). `.claude/rules/general-unit-test.md` has the same
   `mutation score >= 75%` bullet. `Determinism (retry rate)` and `Format check: 100% pass.` are the same
   class. Scope the acceptance to *coverage-threshold* hits, enumerate the non-coverage hits by name with a
   classification, and require the artifact to list them as excluded.

**Why:** an executor cannot complete a task whose acceptance is false for reasons the edit cannot change, so
the plan stalls at the first honest check.

**How to apply:** before writing a zero-hit gate, read the target file and enumerate every current hit; each
one must be either deleted by a task in the plan or explicitly classified as out-of-set in the acceptance.
If a detection pattern is being implemented in code (e.g. `Test-CoverageNumeralAuthority`), pair it with a
negative test that feeds the non-coverage strings and asserts an empty hit list — see
[[enumerate-condition-outcomes-before-case-list]] for the analogous branch-coverage discipline.
