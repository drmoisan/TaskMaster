# D4 — Full `QuickFiler.Test` Assembly Under the Affinity Guard ([P4-T8])

Timestamp: 2026-08-28T05-49

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=488-d4-full-suite.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p4-t8-d4-full-suite
```

EXIT_CODE: 0

## Counts

| Measure | Value |
| --- | --- |
| Total | **1198** |
| Passed | **1198** |
| Failed | **0** |
| Skipped | **0** |
| Total time | 9.2960 seconds |
| Run result | `Test Run Successful.` |

## Observed failing-test-name set

```
(empty)
```

## Subset comparison against `BASELINE_FAILURE_SET`

`[P0-T12]` recorded `BASELINE_FAILURE_SET` as **empty**, so the subset condition reduces to requiring
that the observed failing set is also empty. It is.

**No test name outside the baseline failure set appears in the observed set, because the observed set
is empty.** The constraint C6 escalation is therefore **not** triggered: no
`affinity-guard-blocker.md` was written, no test file outside the three owned ones was edited, and the
guard was not weakened to a null check.

## Count reconciliation

The total rose from the baseline's **1192** to **1198**, a delta of **+6**. That is exactly the number
of test methods this feature has added so far, and every one of them passes:

| Defect unit | Added tests | Names |
| --- | --- | --- |
| D1 | 1 | `ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement` |
| D2 | 1 | `ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost` |
| D3 | 2 | `InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException`, `InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` |
| D4 | 2 | `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic`, `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` |

The remaining four planned methods — one for D5 and three for #475, one of which replaces a deleted
test — land in Phases 5 and 6, bringing the net new-test delta to the **+9** that `[P8-T5]` checks.

## This settles the [P4-T2] discrepancy empirically

`[P4-T2]` recorded a discrepancy: the independently re-derived construction-site set contains **six
sites absent from constraint C6**, of which five are genuine additions to the tree and one is this
feature's own new file. One of those five, `Controllers/EfcItemController.CleanupTests.cs:41`,
installs **no synchronization context at all** before constructing the viewer, and so does not satisfy
the install-before-construct property C6 exists to protect.

`[P4-T2]`'s static analysis bounded that risk — none of the four affected files names any of the
guarded members, and a viewer with a null `UiSyncContext` meets the guard's null escape — but could
not settle it, because a guarded member could in principle be reached indirectly through controller
code rather than named in the test file.

**This run settles it.** The entire assembly, including every one of the nineteen executable
construction sites and all tests reachable from them, runs green against the delivered guard. No test
in `EfcItemController.CleanupTests.cs`, `EfcItemControllerTests.cs`, `BreadcrumbBridgeRouterTests.cs`,
or `QfcItemController.EventWiringTests.cs` fails, so no indirect path from those sites reaches the
guard in a state that would throw.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p4-t8-d4-full-suite/488-d4-full-suite.trx`

Output Summary: EXIT_CODE 0. **1198 total, 1198 passed, 0 failed** across the whole `QuickFiler.Test`
assembly. The observed failing-test-name set is **empty** and is therefore a subset of the empty
`BASELINE_FAILURE_SET`; no constraint C6 escalation was triggered. The +6 delta against the 1192-test
baseline accounts for exactly the six regression tests added so far. This run is the empirical proof
that the affinity guard breaks no existing harness, including the five sites `[P4-T2]` recorded as
absent from constraint C6.
