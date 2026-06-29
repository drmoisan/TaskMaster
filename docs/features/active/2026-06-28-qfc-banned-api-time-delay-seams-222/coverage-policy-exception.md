# Coverage Policy Exception — Issue #222

- **Exception ID:** 222-COV-001
- **Date:** 2026-06-28
- **Authority:** Dan Moisan (repo owner / maintainer, drmoisan; dan@danmoisan.org)
- **Scope:** Issue #222 — QuickFiler banned-API time/delay seam refactor
- **Resolves:** Feature-review finding R1 (Major) in `remediation-inputs.2026-06-28T19-57.md`
- **Modifies policy documents:** none

## Decision

The repository owner authorizes **Option C**: defer verification of the repository-wide
`>= 80%` C# line-coverage floor (General Unit Test Policy / AC7) for this change to the
PR CI coverage run, and accept the current repo-wide figure as a **pre-existing
condition** that is not introduced or regressed by issue #222.

This is an authority decision recorded by the maintainer. It does not alter
`CLAUDE.md`, `.claude/rules/*`, or any other policy document. It applies only to the
PR for issue #222.

## Basis

1. **The finding is an evidence/verification gap, not a code defect.** The canonical
   machine-readable artifact `artifacts/csharp/coverage.xml` was not generated locally,
   so the repo-wide floor could not be confirmed during local feature-review.
2. **Change-scope coverage gates pass and are independently evidenced.**
   - Changed-code **testable** coverage = 100% (6/6 changed testable lines), per
     `evidence/qa-gates/coverage-comparison.md` and `evidence/qa-gates/final-tests.md`.
   - No regression on changed lines; `QfcHomeController.Metrics.cs` class coverage rose
     +14.51 points; the QuickFiler package rose +0.72 points.
   - The 3 uncovered changed lines are ratified exemptions under the CLAUDE.md
     testable-denominator framework: `QfcHomeController.cs` L54/L77 (VSTO `LaunchAsync`
     lifecycle / Outlook Interop dependence without an injectable seam — clauses a/c) and
     `QfcHomeController.Metrics.cs` L222 (unreachable defensive branch under
     `BlockingCollection` semantics). Dossiers:
     `evidence/regression-testing/launchasync-test-scope.md` and
     `evidence/regression-testing/nonblockingproducer-delay-branch-scope.md`.
3. **The change is additive and behavior-preserving.** It adds 5 tests and swaps call
   sites to an injectable seam whose production default reproduces current behavior; it
   removes no coverage of existing code, so the repo-wide floor is not regressed.
4. **The repo-wide shortfall is a pre-existing legacy COM/VSTO/WinForms condition.** It is
   tracked separately under `feature/csharp-coverage-uplift`. Raising the whole-repo
   figure to 80% is a repository-scale effort outside the scope of this banned-API
   refactor.
5. **CI does not gate on the 80% floor.** `.github/workflows/ci.yml` runs the MSTest
   suite with `/EnableCodeCoverage` and uploads coverage artifacts (lines 95–136) but
   does not enforce an 80% threshold as a required check. The repo-wide floor is a
   feature-review policy judgment, not a CI blocker.

## Verification deferred to PR CI

The PR CI coverage run produces the repo-wide coverage artifacts. The repo-wide figure
is expected to remain at its pre-existing legacy level (well below the raw 80% floor)
because of the COM/VSTO/WinForms denominator; this is accepted per this exception and is
not a release blocker for issue #222.

## Tracking

Repository-wide coverage uplift remains tracked on `feature/csharp-coverage-uplift`.
This exception does not close or modify that effort.
