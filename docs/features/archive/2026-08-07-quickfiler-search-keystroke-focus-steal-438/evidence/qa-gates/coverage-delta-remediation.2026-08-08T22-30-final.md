## [P2-T7] Repository-Wide Coverage No-Regression Gate — Final Disposition

- Timestamp: 2026-08-08T22-30
- Command: `pwsh -NoProfile -Command "[xml]$b = Get-Content .../coverage-remediation-baseline.cobertura.xml; [xml]$f = Get-Content .../coverage-remediation-final.cobertura.xml; '{0} {1} {2} {3}' -f $b.coverage.'line-rate', $b.coverage.'branch-rate', $f.coverage.'line-rate', $f.coverage.'branch-rate' ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: same-session baseline `line-rate=0.858512 branch-rate=0.792359`; adopted final (P2-T5 pass 6) `line-rate=0.85862 branch-rate=0.79286`.

### Gate evaluation against the fixed floor (0.858665 line / 0.792502 branch)

- `branch-rate = 0.79286 >= 0.792502`: **PASS.**
- `line-rate = 0.85862 >= 0.858665`: **FAIL** (short by `0.000045`, i.e. approximately 5 lines out of 111,204).

### Complete measurement history (all five clean, all-tests-passing coverage runs captured today)

| Run | line-rate | branch-rate | vs. fixed floor (0.858665 / 0.792502) |
|---|---|---|---|
| P0-T8 baseline (unchanged tree, **before** any test edit) | 0.858512 | 0.792359 | line FAIL, branch FAIL |
| P2-T5 pass 1 (with R1 tests) | 0.858548 | 0.792717 | line FAIL, branch PASS |
| P2-T5 pass 2 (with R1 tests) | 0.858485 | 0.792574 | line FAIL, branch PASS |
| P2-T5 pass 5 (with R1 tests) | 0.858629 | 0.792789 | line FAIL, branch PASS |
| P2-T5 pass 6 (with R1 tests, **adopted**) | 0.85862 | 0.79286 | line FAIL, branch PASS |
| Cycle-1 original (`coverage-final.cobertura.xml`, several hours earlier) | 0.858665 | 0.792502 | (reference constant) |

**Conclusive finding: `branch-rate` clears the fixed floor in every run that includes the R1 fix (4/4 for 4), and improves monotonically and materially over the same-session baseline in every run (baseline 0.792359 → final range 0.792574–0.79286). `line-rate` clusters tightly in a `0.858485`–`0.85862` band across five independent full-suite runs — never once reaching the historical `0.858665` constant, including on the P0-T8 baseline run captured on the exact unmodified tree that originally produced `0.858665`.**

This is decisive: the P0-T8 control measurement (unchanged tree, before any test was added) already measured `0.858512`, below `0.858665`. Therefore the shortfall against the fixed line-rate constant cannot be attributed to this remediation's test additions — it is pre-existing, reproducible, session-to-session coverage measurement variance in the repository's existing test suite, isolated (per the P0-T8 and pass-2 per-class diffs) to a small, rotating set of pre-existing, out-of-scope classes (`EfcHomeController.cs`, `PropertyStore.cs`, `SubjectMapSco.Orchestration.cs`, and `SegmentStopWatch.cs` — a wall-clock timing helper), none of which is the R1 target file and none of which is touched by this remediation. Two coverage-run attempts during this cycle additionally surfaced environment-load symptoms unrelated to R1: one hung testhost (killed and retried, consistent with the same near-end-of-suite stall position observed twice) and one genuine pre-existing flaky-test failure (`WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`, a timing-sensitive Dispatcher-yield test, unrelated file, not modified).

### R1's own scope is fully and repeatedly verified

- The R1 target file (`BreadcrumbItemViewerLifecycleCoordinator.Search.cs`) measures `branch-rate = 1` (4/4) and `line-rate = 1` (5/5) in **every** successful coverage run captured after the fix (P2-T5 passes 1, 2, 5, 6) — reproducible, not a one-off artifact.
- No production file is modified (confirmed at P2-T8).
- No existing test is modified (confirmed at P2-T8).
- All 6350 tests pass in the adopted final run.

### Disposition

**Deviation from the plan's literal P2-T7 acceptance, explicitly reported per the delegation prompt's reporting requirement.** The literal numeric acceptance `final line-rate >= 0.858665` is not met, after five full-suite retries (one Phase-2 restart cycle plus additional coverage-only retries following a hang and a pre-existing flaky-test failure) that consistently and tightly cluster just under that historical constant, with the shortfall root-caused and reproduced on the unmodified tree itself. Continuing to retry indefinitely was assessed as unlikely to change the outcome (five consecutive independent measurements, all in the same narrow band) and was stopped in favor of full, transparent reporting. `branch-rate` clears its fixed floor in every run. R1's own acceptance criterion (target-file branch coverage >= 75%) is met and exceeded (100%) reproducibly. This finding and its full evidence trail are escalated in the executor's completion report rather than resolved unilaterally, since resolving it would require either (a) accepting a documented, root-caused, non-remediation-caused environmental miss against a single historical sample, or (b) modifying out-of-scope legacy classes to chase a moving coverage target — both outside this plan's and this executor's authority to decide unilaterally.
