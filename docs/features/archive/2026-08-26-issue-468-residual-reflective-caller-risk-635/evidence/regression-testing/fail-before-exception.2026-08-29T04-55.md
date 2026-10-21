# Fail-Before Exception Dossier (P3-T2) — discharges AC-14

- **Issue:** #635
- **Plan task:** [P3-T2]
- **Work Mode:** full-bug

Timestamp: 2026-08-29T06-37

## Output Summary

This item's work mode is `full-bug`, which normally requires a regression test that fails before the fix
and passes after it. No such test can exist here. This dossier records why a failing run is
structurally impossible and supplies the non-vacuity measurement as the alternative proof, citing by
path each artifact that constitutes it.

WhyFailingRunImpossible: This item changes no executable code, so there is no behavior that differs
before and after the work and no test whose result the work could change. A test asserting that a
repository search finds no genuine name-based caller of a removed member is a tautology at both ends:
it passes before the work for the same reason it passes after, because the property it asserts was
already true when the item began. No reproducible defect exists to redden such a test — the
specification records no steps to reproduce, no error, no incorrect output, and no user-visible
symptom.

## Alternative proof — the non-vacuity measurement

The evidence-and-timestamp-conventions skill permits an exception dossier in place of a failing run
provided it supplies an alternative proof. The alternative proof here is the non-vacuity measurement
itself: a measured, non-empty search scope with a fully classified hit set. It comprises five
artifacts, each cited by path.

1. **The scope census** —
   `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md`.
   Measures the Partition A search scope at 683 tracked files with a twelve-row extension census, the
   comparable AC-16 six-extension scope at 153 files, and the widening delta at 530 files. This is what
   makes the zero result a measurement rather than an assertion.

2. **The Partition A zero-hit result** —
   `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md`.
   Records the verbatim command, the literal output `(no output)`, `ExpectedExitCode: 1` and
   `EXIT_CODE: 1`, together with the `SearchScope:`, `SearchPatterns:` and `SearchResult:` fields the
   negative-claim rule requires.

3. **The control that proves the same pathspec reaches real content** —
   `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t2-partition-a-control.2026-08-29T04-55.md`.
   Runs the identical pathspec for the token `QfcCollectionController` and returns thirteen hits across
   four files, one of them an extensionless tracked file that the AC-16 six-extension search could never
   have reached. This is the element that discriminates a genuine absence from an unreachable corpus,
   and it is the closest available analogue to a fail-before run: it is a run of the same gate, over the
   same corpus, that produces a non-zero result.

4. **The total classification with its empty category G** —
   `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md`.
   Sweeps the same thirteen identifiers with the prose trees included, records a total of 2,337 hits,
   assigns every hit a category by a path-derived test, and shows the per-category counts summing to
   the total with the "genuine name-based caller" category empty.

5. **The fully enumerated 31-row hit set with its empty category G** —
   `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md`.
   Enumerates every one of the 31 tracked-`.cs` hits individually with its file, line, matched
   identifier and category, and shows the four category counts summing to 31 with the genuine-caller
   category empty.

Together these five establish, by measurement rather than by assertion, that the search set was
non-empty and reachable, that every hit it produced was classified, and that the class the item exists
to test for is empty.

## No unit test is added and no existing test is modified

No unit test is added and no existing test is modified by this item.

The reason is direct. A search-based test would encode a point-in-time measurement as a permanent gate
over prose files that legitimately accrete these identifiers, and would fail on the next evidence
artifact that quotes one of them. The measurement in [P1-T3] demonstrates the mechanism concretely:
the hit total over the prose trees moved from 2,229 at the specification's base commit to 2,337 at the
commit this item executes against, purely through documentation and agent-memory writes. A test
asserting any fixed count, or asserting zero over a scope that includes those trees, would break on the
next such write while the property it purports to guard remained unchanged.

The correct durable artifact for a property of this shape is the classification recorded in this item's
evidence, which is invariant under prose accretion: a new prose hit increments one category and the
total together and cannot increment the genuine-caller category.

## Auditable-absence record for this dossier

SearchScope: `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/regression-testing/` and the feature root `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/`. This feature is single-version, so no version sub-folder scope applies.

SearchPatterns: a failing regression run artifact under `evidence/regression-testing/`, and the exception-dossier pattern `fail-before-exception.*.md`.

SearchResult: no failing regression run artifact exists, for the structural reason recorded above. This dossier, at `evidence/regression-testing/fail-before-exception.2026-08-29T04-55.md`, is the exception dossier that stands in its place.
