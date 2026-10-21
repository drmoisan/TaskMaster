# Phase 0 — Baseline Outcomes of the Nine Constraining Tests ([P0-T13])

Timestamp: 2026-08-28T05-17

Command: read from the `[P0-T12]` TRX at
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/baseline/trx-p0-t12/baseline-quickfiler-test.trx`,
by matching each `UnitTestResult` element's `testName` attribute and reading its `outcome` attribute.
No new test run was started for this task.
EXIT_CODE: 0

## Result rows

| # | Test | Baseline outcome | Constraint it imposes on this feature |
| --- | --- | --- | --- |
| 1 | `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` | **Passed** | `host.Dispose()` `Times.Once()` on viewer disposal. This assertion is what rules out both rejected D1 alternatives, and it is why D1's disposal must type-test the concrete `BreadcrumbDropDownHost` rather than `IBreadcrumbDropDownHost`. |
| 2 | `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` | **Passed** | "No stale pooled theme is replayed." The highest-risk interaction with D2's retained-theme replay. |
| 3 | `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost` | **Passed** | Same-environment configure must reuse the host and dispose nothing. Pins the same-environment early return that D1's new statement must follow. |
| 4 | `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily` | **Passed** | `Theme == "dark"` and `ControlHost == null` immediately after configure-then-theme. D2's replay must remain additive. |
| 5 | `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam` | **Passed** | The light-theme counterpart of row 4. |
| 6 | `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` | **Passed** | Subscription order `add`/`remove` across a host swap. Stays green because `RecordingHost.Dispose` is empty, which is why `[P2-T1]` must leave that body empty. |
| 7 | `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` | **Passed** | Reset-then-reconfigure with the same host must take the `UpdateRequestProviders` branch, which is why D2's replay is confined to the newly-adopted branch. |
| 8 | `SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions` | **Passed** | The reference-comparison precedent D3 mirrors. |
| 9 | `Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory` | **Passed** | Passes today without an ambient context because the `surfaceFactory ?? throw` is evaluated before the operations argument. #475 part 2 must not reorder those arguments. |

## Re-run set for [P7-T1]

**This set of nine names is the re-run set for `[P7-T1]`.** `[P7-T1]` re-runs exactly these nine under
a pipe-joined `/TestCaseFilter:` and compares each observed outcome against the baseline outcome
recorded above, requiring that no name's outcome is worse than its baseline.

All nine baseline outcomes are `Passed`. `[P7-T1]`'s acceptance therefore reduces to its stated
expected case: `EXIT_CODE: 0` with failed count 0 and all nine observed as `Passed`. The
"a constraining test that was already red cannot constrain this fix" branch, which would leave
`[P7-T1]` unchecked, is **not** triggered.

Output Summary: All **nine** constraining tests recorded outcome `Passed` at the baseline. This is the
re-run set for `[P7-T1]`, whose comparison consequently reduces to requiring nine `Passed` outcomes and
zero failures.
