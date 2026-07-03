# Acceptance Criteria Traceability (P9-T7)

Timestamp: 2026-06-29T12-50

Maps AC1-AC7 (issue #227, qfc-item-controller-testability) to the satisfying tasks and evidence
artifacts, and records the deferrals and the exemption-boundary ratification note.

| AC | Status | Satisfying tasks | Evidence |
|---|---|---|---|
| AC1 — split into < 500-line partials, logical structure, no behavior change, existing tests pass | MET | P1-T1..T11, P1-T16 | `evidence/qa-gates/p1-file-sizes.2026-06-29T11-00.md`, `evidence/qa-gates/p1-tests-coverage.2026-06-29T11-00.md` |
| AC2 — `_itemViewer` field + ctor params -> `IItemViewer`; `Mock<IItemViewer>` injectable; concrete-bound `(ItemViewer)` seam for control-host paths | MET | P2-T1..T5, P7-T2..T7 | `evidence/qa-gates/p2-*.2026-06-29T11-10.md`, `evidence/qa-gates/p7-tests-coverage.2026-06-29T12-25.md` |
| AC3 — `IItemViewer` narrowed to intent members; raw control types removed; `ItemViewer` forwards and stays `[ExcludeFromCodeCoverage]` | MET | P3-T1..T5, P4-T1..T6, P5-T1..T5, P6-T1..T5 | `evidence/other/iitemviewer-final-shape.2026-06-29T12-05.md`, `evidence/qa-gates/p3-* .. p6-*` |
| AC4 — test files mirror partial structure, each < 500 lines, explicit csproj entries | MET | P7-T1..T8 | `evidence/qa-gates/p7-file-sizes.2026-06-29T12-25.md` |
| AC5 — affected testable non-exempt denominator >= 80%; new/extracted >= 90%; no changed-line regression; exemption boundary ratified | PARTIAL / REMEDIATION-REQUIRED | P8-T1..T3, P8-T7, P9-T4, P9-T5 | `evidence/qa-gates/p8-tests-coverage.2026-06-29T12-40.md`, `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`, `evidence/other/exemption-boundary.2026-06-29T12-40.md` |
| AC6 — no modified production file exceeds 500 lines | MET | P1-T12, P2-T6, P3-T6, P4-T7, P5-T6, P6-T6, P7-T8, P9-T6 | `evidence/qa-gates/final-file-sizes.2026-06-29T12-50.md` |
| AC7 — full C# toolchain passes in order, no regressions | MET | P1..P8 phase gates, P9-T1..T4 | `evidence/qa-gates/final-csharpier`, `final-analyzers`, `final-nullable`, `final-tests-coverage` (all 2026-06-29T12-50) |

## AC5 detail

- >= 80% affected testable non-exempt denominator: **MET** — 484/585 = 82.74%.
- Changed-line regression: **none** — split is verbatim; all clusters at/above baseline; strictly
  additive coverage.
- >= 90% new/extracted sub-target: **UNMET — remediation-required.** Aggregate extracted non-exempt
  is 82.74%; the genuinely-new narrowing logic is >= 90%, but verbatim-extracted, structurally
  un-coverable code holds the aggregate below 90%. Residual gap recorded in
  `coverage-delta.2026-06-29T12-50.md` and `p8-tests-coverage.2026-06-29T12-40.md`.
- Repo-wide floor: satisfied-with-documented-exception under the #223 authority-scoped precedent
  (`maintainer-decision.2026-06-29.md`); residual uplift tracked under #197.
- Exemption boundary: documented in `exemption-boundary.2026-06-29T12-40.md` (103 method-level
  exemptions across the COM/Outlook/WinForms categories and the P2-T4 concrete-bound control-host
  paths) and **awaiting maintainer ratification at review**.

## Deferrals (this cycle)

- **Injectable `Dispatcher`** (research Seam C, `UiThread.Dispatcher` static, ~20 call sites):
  deferred to #197. Not introduced because it would not lift the aggregate to 90% (binding
  constraint is the EventWiring inline async-registration lambda bodies, not the `Dispatcher`
  paths; best-case with `Dispatcher` covered ~86.8%).
- **Tip-collection abstraction** (`IList<Label>` tip collections on `IItemViewer`): retained as-is
  and deferred this cycle per AC3 scope.

## Disposition

AC1-AC4, AC6, AC7 met. AC5 is partial: the 80% testable-denominator floor and no-changed-line-
regression conditions are met; the 90% new/extracted sub-target is remediation-required and the
exemption boundary awaits maintainer ratification. Overall cycle outcome: **remediation-required on
the AC5 90% sub-target**, not a full PASS.
