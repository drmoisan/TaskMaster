---
name: 400-completeopenasync-unreachable-recovery-catch
description: "#400 P5-T185..204: CompleteOpenAsync d__16 inner recovery-failure catch (lines 153-156) is unreachable dead code; can't hit >=90% test-only because dispatcher.Report swallows sink exceptions"
metadata:
  type: project
---

In QuickFiler `BreadcrumbDropDownOpenLifetime.CompleteOpenAsync` (`<CompleteOpenAsync>d__16`), the inner
recovery-failure catch is unreachable dead-defensive code and cannot be covered by any test-only change.

**Why:** For that inner `catch (recoveryFailure)` to run, `HandleOpenFailureAsync` must throw. Its entire body is
`try { await _uiOperations.RunAsync(..., reportFailure:false); } catch (rollbackFailure) { _uiOperations.Report(rollbackFailure); }`.
The only statement that can throw out is the `Report` call, which resolves to `BreadcrumbUiDispatcher.Report` — and
that method swallows every error-sink exception (rethrows only when its argument is null, which cannot happen for a
caught non-null exception). So `HandleOpenFailureAsync` never faults; the inner catch is dead. Confirmed both
analytically and by a focused instrumented probe with a throwing error sink (still 24/28, lines 153-156 uncovered),
which was reverted.

**How to apply:** The #400 remediation plan (`remediation-plan.2026-07-21T21-37.md`) P5-T185/P5-T195 wrongly claim
these lines are a "reachable late-callback branch" coverable test-only to reach >=90%. They are not. The plan's
zero-production-file rule plus the flat >=90%-per-unit rule are jointly unsatisfiable for this state machine
(max reachable = 24/28 = 85.71%). Batch N1 (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`) and 4 of 5 N2 cases
(`BreadcrumbPopupBoundaryCoverageTests.Part2.cs`) close their 8 units cleanly (7 at 100%, EnsureSurfaceAsync 97.67%);
only CompleteOpenAsync blocks P5-T202/T203/T204. Escalated for replanning: either authorize a minimal production
change to make the recovery propagate / remove the dead catch, or revise the ninth-unit acceptance to its reachable
maximum. See [[project_400_csharpier_pipefiles_nonenforcing_gate]] for the same feature's P5 gate mechanics.

Also: the 17-class instrumented gate (`Workers=0`/24 ClassLevel + dotnet-coverage) intermittently deadlocks a
`BreadcrumbPopupControlDispatchTests` testhost during concurrent WinForms host-handle creation — same documented
stall the 16-22 baseline hit. Clear residual dotnet-coverage/testhost/vstest, confirm 0 with no respawn, re-run the
exact command; it passed 170/170 natural-exit-0 on the 5th attempt. Never cite a stalled attempt as a result.
