# Constraining Tests — Final Re-Run ([P7-T1])

Timestamp: 2026-08-28T06-11

Command (under `pwsh -NoProfile`, worktree root), with the nine fully qualified name fragments joined
by the literal `|` character:

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces|FullyQualifiedName~ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession|FullyQualifiedName~ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost|FullyQualifiedName~ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily|FullyQualifiedName~ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam|FullyQualifiedName~HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder|FullyQualifiedName~ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks|FullyQualifiedName~SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions|FullyQualifiedName~Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory" "/Logger:trx;LogFileName=constraining-tests-final.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\qa-gates\trx-p7-t1
```

The fragments are joined with the literal `|` character. `vstest.console.exe` rejects the word `OR` in
a `/TestCaseFilter:` expression.

EXIT_CODE: 0

## Baseline comparison, one row per name

| # | Test | Baseline ([P0-T13]) | Observed | Worse than baseline? |
| --- | --- | --- | --- | --- |
| 1 | `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` | Passed | **Passed** | no |
| 2 | `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` | Passed | **Passed** | no |
| 3 | `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost` | Passed | **Passed** | no |
| 4 | `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily` | Passed | **Passed** | no |
| 5 | `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam` | Passed | **Passed** | no |
| 6 | `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` | Passed | **Passed** | no |
| 7 | `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` | Passed | **Passed** | no |
| 8 | `SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions` | Passed | **Passed** | no |
| 9 | `Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory` | Passed | **Passed** | no |

**No name's outcome is worse than its baseline outcome.**

## Counts

| Measure | Value |
| --- | --- |
| Total | **9** |
| Passed | **9** |
| Failed | **0** |
| Run result | `Test Run Successful.` |

All nine baselines were `Passed`, which is the expected case this task describes, so the gate is
`EXIT_CODE: 0` with failed count 0 and is recorded as such. The branch that would leave this task
unchecked — a constraining test already red at baseline, which could not have constrained the fix — is
**not** triggered.

## What each test constrains, and why it survived

- **1** pins `host.Dispose()` `Times.Once()` on viewer disposal. D1's disposal type-tests the concrete
  `BreadcrumbDropDownHost`, so the `Mock<IBreadcrumbDropDownHost>` this test installs through the
  three-argument overload is not disposed by the new statement.
- **2** asserts no stale pooled theme is replayed. D2's retained value is overwritten by the subsequent
  light-theme call before the re-attach, so the replay carries the current theme; `[P2-T6]` records the
  reasoning in full.
- **3** pins the same-environment early return, which still fires before D1's new statement.
- **4** and **5** pin theme and `ControlHost` state immediately after configure-then-theme; D2's replay
  is confined to the newly-adopted branch and is therefore additive.
- **6** pins subscription order across a host swap; it stays green because `RecordingHost.Dispose`
  remains an empty body, which `[P2-T1]` preserved deliberately.
- **7** reconfigures with the same host and so takes the `UpdateRequestProviders` branch, which performs
  no theme call at all.
- **8** is the reference-comparison precedent D3 mirrors; `SetBridgeCoordinator` is unchanged.
- **9** passes without an ambient context because the surface-factory argument-null guard is evaluated
  before the operations argument; #475 part 2 reordered no constructor argument.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/trx-p7-t1/constraining-tests-final.trx`

Output Summary: EXIT_CODE 0. All **nine** constraining tests observed `Passed`, each matching its
`[P0-T13]` baseline of `Passed`; no name's outcome is worse than its baseline. Total 9, passed 9,
failed 0.
