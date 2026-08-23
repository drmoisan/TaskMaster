# P5-T202 — Nine-unit numeric closure — BLOCKED (8 of 9 closed; one unreachable)

Timestamp: 2026-07-22T18-58Z

Command: `python3 <parse-per-unit-line-coverage> coverage-p5-branch-coverage-correction.2026-07-22T18-58.cobertura.xml`

EXIT_CODE: 0

## DECISION: REMEDIATION / ATOMIC REPLANNING REQUIRED

Eight of the nine P5-T185 units now report at least 90% line coverage from the authoritative P5-T201 Cobertura.
The ninth unit, `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16`, remains at 85.71% because its uncovered
lines 153-156 are **unreachable without a production change**. Per the delegation's binding constraint
("If you believe a production edit is required, STOP and report for replanning rather than editing") and the plan's
zero-production-file rule, no production edit was made and this task is left unchecked.

## Per-unit closure (P5-T201 Cobertura, dedup by source line)

| # | Unit | Previous | New | New % | Covering case(s) | Verdict |
|---:|---|---:|---:|---:|---|---|
| 1 | `<EnsureSurfaceAsync>d__21` | 28/43 (65.12%) | 42/43 | 97.67% | N2 case 1 (292-301), case 3 (310-313); case 2 targeted 315 | PASS (>=90%; 315 residual, see note) |
| 2 | `<RollbackAsync>d__28` | 6/9 (66.67%) | 9/9 | 100.00% | N1 case 5 (224-226) | PASS |
| 3 | `HandleSelectorOpenStateChanged()` | 4/5 (80.00%) | 5/5 | 100.00% | N1 case 2 (118) | PASS |
| 4 | `Reset()` | 4/5 (80.00%) | 5/5 | 100.00% | N1 case 4 (133) | PASS |
| 5 | `SetDroppedDown(bool)` | 5/6 (83.33%) | 6/6 | 100.00% | N1 case 1 (99) | PASS |
| 6 | `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` | 5/6 (83.33%) | 6/6 | 100.00% | N2 case 5 (413) | PASS |
| 7 | **`<CompleteOpenAsync>d__16`** | 24/28 (85.71%) | **24/28** | **85.71%** | N2 case 4 exercises the reachable outer recovery path; 153-156 unreachable | **BELOW — BLOCKED** |
| 8 | `<HandleSelectorOpenStateChanged>b__22_0()` | 7/8 (87.50%) | 8/8 | 100.00% | N1 case 3 (122) | PASS |
| 9 | `RetainCurrentSurface(...)` | 8/9 (88.89%) | 9/9 | 100.00% | N2 case 1 (324) | PASS |

Note on unit 1 line 315 (`throw;`): line 315 remains uncovered in the gate Cobertura, but unit 1 is at 97.67%
(>=90%) so the unit passes; 315 is not a threshold blocker.

## Root-cause of the unreachable unit (read-only diagnosis, no production edit)

`<CompleteOpenAsync>d__16` lines 153-156 are the inner recovery-failure catch:

```
catch (Exception exception)              // 147  (reachable; exercised by N2 case 4)
{
    try
    {
        await HandleOpenFailureAsync(exception, lease).ConfigureAwait(false);   // 151
    }
    catch (Exception recoveryFailure)    // 153  \
    {                                    // 154   |  UNREACHABLE
        _uiOperations.Report(recoveryFailure);   // 155  |
    }                                    // 156  /
}
```

For 153 to execute, `HandleOpenFailureAsync` must throw. `HandleOpenFailureAsync`
(`BreadcrumbDropDownOpenLifetime.cs` 335-359) wraps its whole body in `try { await _uiOperations.RunAsync(...,
reportFailure: false); } catch (Exception rollbackFailure) { _uiOperations.Report(rollbackFailure); }`. The only
statement that can throw out of that method is `_uiOperations.Report(rollbackFailure)` at line 357, which resolves to
`BreadcrumbUiDispatcher.Report` (lines 238-253):

```
internal void Report(Exception exception)
{
    if (exception == null) { throw new ArgumentNullException(nameof(exception)); }
    try { _errorSink(exception); }
    catch (Exception sinkException) { log.Error("Breadcrumb UI error sink failed.", sinkException); }
}
```

`Report` swallows every error-sink exception (it only rethrows when its argument is null, which cannot occur because
`rollbackFailure` is a caught non-null exception). Therefore `HandleOpenFailureAsync` can never fault, and
`CompleteOpenAsync`'s inner catch (153-156) is dead-defensive code unreachable through any injected seam.

Empirical confirmation (read-only diagnosis, reverted afterward): a throwaway variant of N2 case 4 was run under a
focused instrumented probe with the error sink configured to throw during the recovery report. Line-level Cobertura
still reported `<CompleteOpenAsync>d__16` = 24/28 with 153-156 uncovered, confirming the analytic result. That
experiment was reverted; the committed `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` is byte-identical
(`594d96f2…`) to the state the authoritative 170/170 gate ran on.

## No-regression of the seven previously-passing units

Zero production C# files changed and no test was deleted, so production coverage of these units cannot decrease.
Confirmed from the P5-T201 Cobertura (whole-type line coverage, all at or above their P5-185 member baselines):
Dispatcher 187/187 (baseline member 144/144), NavigationReadiness 96/96, Factory 44/45 (baseline member 16/16),
host-neutral Popup operations 223/247 (>=75/76), Hub 171/171 (baseline member 155/155), Attachment 111/111
(baseline member 80/80), Release 0/0 (no instrumented lines in this filtered run, as at baseline). No regression.

## Required plan revision (delta for atomic-planner)

Lines 153-156 cannot be closed by a test-only change. The plan's premise in P5-T185/P5-T195 that
`<CompleteOpenAsync>d__16` 153-156 is a "reachable ... late-callback branch" coverable by "new deterministic
behavioral tests only, with zero production files changed" is not satisfiable as written. One of the following plan
revisions is required before P5-T202/T203/T204 can pass:

1. Authorize a minimal production change to `BreadcrumbDropDownOpenLifetime.HandleOpenFailureAsync` so its
   recovery-failure path can propagate a genuine secondary (e.g., not swallowing a rollback-report failure), making
   153-156 reachable; or remove the dead inner catch and re-baseline the state-machine denominator; or
2. Reclassify `<CompleteOpenAsync>d__16` lines 153-156 as unreachable defensive code and adjust the ninth-unit
   acceptance for that state machine to its reachable maximum (24/28 = 85.71%) with a documented justification,
   instead of the flat >=90% line requirement — without adding any coverage exclusion.

## Output Summary

Eight of nine P5-T185 units reach >=90% line coverage (seven at 100%, EnsureSurfaceAsync at 97.67%) with zero
production-file changes; the seven previously-passing units did not regress. The ninth unit,
`<CompleteOpenAsync>d__16`, is blocked at 85.71%: its uncovered lines 153-156 are the inner recovery-failure catch,
which is unreachable because `BreadcrumbUiDispatcher.Report` swallows all error-sink exceptions, so
`HandleOpenFailureAsync` can never fault. This is confirmed both analytically and by a reverted focused probe.
Closing it requires a production change or an acceptance-criterion revision, so — per the delegation's explicit STOP
directive — no production edit was made, P5-T202/T203/T204 are left unchecked, and the finding is escalated for
atomic replanning.
