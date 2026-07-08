# Final QA — Net Coverage Comparison

Timestamp: 2026-06-14T08-22

Command: comparison of artifacts/csharp/coverage-firstparty.cobertura.xml (pre-feature baseline) vs artifacts/csharp/final-fullsuite.cobertura.xml (post-feature)

EXIT_CODE: 0

## Baseline (post-#197, authority 197-COV-001)

Aggregate production-only rate: 71.65%.

Pre-feature production-assembly line-rates (from coverage-firstparty.cobertura.xml):
- ToDoModel:  10.82%
- QuickFiler: 25.20%
- TaskMaster: 25.78%

## Post-feature production-assembly line-rates (final-fullsuite.cobertura.xml)

| Assembly | Pre | Post | Covered lines (post) | Valid lines |
|---|---|---|---|---|
| ToDoModel  | 10.82% | 25.22% | 957  | 3795  |
| QuickFiler | 25.20% | 30.57% | 4136 | 13530 |
| TaskMaster | 25.78% | 44.05% | 1507 | 3421  |

Each of the three feature assemblies shows a clear covered-line increase.

## Aggregate net change vs 71.65%

This feature added 99 passing MSTest unit tests and changed ZERO production lines (verified in
final-invariant-check). Consequently:
- The production-only DENOMINATOR (valid production lines) is unchanged.
- The production-only NUMERATOR (covered production lines) strictly increased on the named seams in
  all three assemblies.

Therefore the aggregate production-only rate strictly increases versus the 71.65% post-#197
baseline. A net increase is established.

Direct re-derivation of the exact aggregate percentage via the full Koverage production-only
pipeline (which also spans UtilitiesCS and applies the recorded vendored-package denominator method)
was not re-executed end-to-end here; the per-assembly covered-line increases above, combined with
the unchanged denominator, are sufficient to establish the required net increase. The new-code
coverage on every reachable targeted method is 100% (see inc1/inc2/inc3 coverage-delta artifacts).

## Aggregate new/changed-code coverage

New code = the 11 new test files plus targeted production methods. Every targeted production method
that is reachable without a prohibited production seam is covered at 100% line-rate (per-method
analysis in the increment deltas). The two documented Flag-and-Stop gaps
(ProjectEntry dialog branches; AppFileSystemFolderPaths.MatchBestSpecialFolder) are the only
targeted paths not covered, both authorized by the plan's Flag-and-Stop rule and recorded in
evidence/other.

## Outcome

PASS: measured covered-line increase on all three production assemblies; net production-only rate
increases versus 71.65% (denominator unchanged, numerator increased); aggregate new-code coverage
>= 90% on all reachable targeted methods. Two Flag-and-Stop gaps recorded, no production change.
