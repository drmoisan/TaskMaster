## [P2-T7] Repository-Wide Coverage No-Regression Gate — Pass 2 (FAIL, further investigated)

- Timestamp: 2026-08-08T21-45
- Command: same as pass 1, re-run against the pass-2 `coverage-remediation-final.cobertura.xml`.
- EXIT_CODE: 0
- Output Summary: baseline `line-rate=0.858512 branch-rate=0.792359`; final (pass 2) `line-rate=0.858485 branch-rate=0.792574`.

### Gate evaluation against the fixed floor (0.858665 line / 0.792502 branch)

- `branch-rate = 0.792574 >= 0.792502`: PASS.
- `line-rate = 0.858485 >= 0.858665`: **FAIL** (short by `0.00018`, slightly worse than pass 1's `-0.000117`).

### Per-class diff, pass-2 final vs. Cycle-1 original (`coverage-final.cobertura.xml`)

| Class | Cycle-1 line-rate | Pass-2 line-rate | Cycle-1 branch-rate | Pass-2 branch-rate |
|---|---|---|---|---|
| `QuickFiler.Viewers.BreadcrumbItemViewerLifecycleCoordinator` (`BreadcrumbItemViewerLifecycleCoordinator.cs`, the sibling partial, not the R1 target `.Search.cs` part) | 0.939516 | 0.939516 | 0.688073 | 0.697248 (improved) |
| `QuickFiler.Viewers.BreadcrumbItemViewerLifecycleCoordinator` (**R1 target**, `BreadcrumbItemViewerLifecycleCoordinator.Search.cs`) | 1 | 1 | 0.5 | **1** (fixed, as intended) |
| `QuickFiler.Interfaces.PropertyStore` (`UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs`) | 0.844275 | 0.841221 | 0.864583 | 0.859375 |
| `UtilitiesCS.SubjectMapSco` (`UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs`) | 0.982759 | 0.965517 | 0.875 | 0.875 |
| `UtilitiesCS.HelperClasses.SegmentStopWatch` (`UtilitiesCS\HelperClasses\SegmentStopWatch.cs`) | 1 | 0.938144 | 1 | 0.96 |

### Cross-run analysis (3 independent full-suite coverage runs captured today, same unchanged-tree denominators of 111204 lines / 27928 branches in every run)

| Run | Scope | line-rate | branch-rate |
|---|---|---|---|
| P0-T8 baseline | tree **before** any test edit (unchanged, identical to Cycle-1's committed tree) | 0.858512 | 0.792359 |
| P2-T5 pass 1 final | tree with P1-T1/P1-T2 tests added | 0.858548 | 0.792717 |
| P2-T5 pass 2 final | tree with P1-T1/P1-T2 tests added | 0.858485 | 0.792574 |
| Cycle-1 original (`coverage-final.cobertura.xml`, captured 2026-08-08T11-41, several hours earlier) | tree identical to P0-T8 baseline | 0.858665 | 0.792502 |

**Key finding: the P0-T8 baseline — captured on the exact same, unmodified tree that produced Cycle-1's 0.858665, before this remediation added any test — already measured 0.858512, below the fixed floor.** This proves the shortfall against the fixed `0.858665` constant is not caused by this remediation's test additions; it is a pre-existing, reproducible session-to-session coverage measurement variance. The differing classes rotate between runs (`EfcHomeController.cs` in the P0-T8 diff; `SubjectMapSco.Orchestration.cs` in this pass-2 diff; `PropertyStore.cs` and `SegmentStopWatch.cs` — a wall-clock timing helper — recur in both), consistent with nondeterministic execution-order/timing effects under MSTest `ClassLevel` parallelization (`Workers: 24`), not a code regression: every run's `lines-valid`/`branches-valid` denominator is byte-identical (111204/27928), and all 6350 tests pass in every run.

**R1 scope is unaffected and independently verified:** in both pass 1 and pass 2, the R1 target file (`BreadcrumbItemViewerLifecycleCoordinator.Search.cs`) measures `branch-rate = 1` (4/4), `line-rate = 1` (5/5) — the R1 fix is robust and reproducible across independent runs. The one non-target file that changed measurably (`BreadcrumbItemViewerLifecycleCoordinator.cs`, the sibling partial) improved, not regressed.

**Disposition:** FAIL on the literal fixed-constant `line-rate >= 0.858665` gate for a second consecutive pass, with root cause now conclusively isolated to pre-existing, environment-dependent coverage measurement variance unrelated to this remediation (proven by the P0-T8 control measurement on the unchanged tree). Proceeding to one further Phase 2 restart (pass 3) per the plan's restart rule to gather one additional data point before final disposition; if pass 3 also fails to clear the fixed floor, this will be escalated in the completion report as a documented, root-caused deviation rather than retried indefinitely, since the remediation's own correctness (R1 fix, no production change, no existing test modified, all tests passing) is independently and repeatedly verified.
