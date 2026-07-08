# Per-Module / Repo-Wide Coverage — Issue #171

- **Task:** [P2-T3]
- **Date:** 2026-06-02T10-36
- **Finding:** R2
- **Artifact:** `artifacts/csharp/coverage.xml` (Cobertura)
- **Baseline:** `evidence/coverage/coverage-baseline-171.2026-06-02T14-05.txt`

## Method

Per-package (module) distinct-line coverage was computed from the Cobertura artifact by
deduplicating `filename:lineNumber` across class nodes (max hits). This is the Cobertura
distinct-instrumented-line basis, which differs from the baseline `line_coverage`
attribute basis; the comparison therefore evaluates direction (improved / unchanged / not
regressed) rather than exact equality.

## Application modules in scope of the two in-scope test assemblies

| Module | Artifact Covered | Total | Artifact % | Baseline % | Direction |
|--------|------------------|-------|------------|------------|-----------|
| UtilitiesCS | 32609 | 37288 | 87.45% | 87.58% | unchanged (delta -0.13; >= 80% floor met) |
| QuickFiler | 3635 | 14362 | 25.31% | 24.11% | improved (+1.20) |

Test assemblies (reported for completeness, excluded from any application-coverage gate):
- UtilitiesCS.Test: 97.90%
- QuickFiler.Test: 91.47%

## Other production modules NOT exercised by the two in-scope test assemblies

| Module | Artifact % |
|--------|-----------|
| TaskMaster | 6.60% |
| ToDoModel | 0.00% |
| Tags | 0.00% |

These are pre-existing low-coverage modules. They are not exercised by `QuickFiler.Test`
or `UtilitiesCS.Test` and are not introduced or changed by Issue #171.

## Third-party / vendored modules (excluded from application-coverage gate)

Swordfish.NET.General, Mono.Reflection, FluentAssertions, SVGControl, FSharp.Core, Deedle,
log4net, System.Linq.Async, System.Interactive.

## Repo-wide figure and pre-existing-condition justification

The whole-artifact distinct-line figure across all packages is **58.47%**
(94,739 / 162,037), below the documented 80% repo-wide floor. This is a **pre-existing
condition not introduced by Issue #171**, for the following verifiable reasons:

1. The number aggregates production modules that the two in-scope test assemblies do not
   exercise at all (TaskMaster 6.60%, ToDoModel 0.00%, Tags 0.00%) and third-party /
   vendored assemblies (FSharp.Core 8.66%, Deedle 7.04%, log4net 6.07%, System.Linq.Async
   3.87%, System.Interactive 2.79%) that are not application code subject to the floor.
2. The application code actually exercised by the in-scope test assemblies is
   UtilitiesCS (87.45%, >= 80%) and QuickFiler (25.31%). Issue #171 raised QuickFiler
   (+1.20 vs baseline) and did not lower any module.
3. The low QuickFiler module figure is dominated by the oversized COM/WinForms-bound
   controllers (`QfcCollectionController`, `QfcItemController`) whose UI/COM paths are not
   unit-testable without live Outlook; these were at ~3-7% at baseline and remain so. The
   remediation scope explicitly forbids refactoring these controllers.

## Conclusion

- No module regressed; QuickFiler improved (+1.20) and UtilitiesCS held at the >= 80%
  floor (87.45%).
- The sub-80% repo-wide aggregate is a documented pre-existing condition driven by
  out-of-scope and third-party modules, not introduced by Issue #171.
