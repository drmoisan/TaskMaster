# Consumer Classes and Unowned Call Sites Pass (P2-T5)

Timestamp: 2026-08-27T11-04
Task: [P2-T5]
Command: `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"FullyQualifiedName~QfcItemController_InitializationTests|FullyQualifiedName~QfcItemController_SeamFactoryTests|FullyQualifiedName~SetThemeDark_FromNormal_SelectsDarkNormalTheme|FullyQualifiedName~SetThemeLight_FromNormal_SelectsLightNormalTheme" /Logger:"trx;LogFileName=consumers.trx" /ResultsDirectory:TestResults\plan-logs\p2-t5`
EXIT_CODE: 0
Output Summary: `Test Run Successful.` Total tests 22, Passed 22, Failed 0. The failed-test set is
empty, which is a subset of the empty `BaselineFailedTests` set `P0-T12` recorded. Both
`SetThemeDark_FromNormal_SelectsDarkNormalTheme` and
`SetThemeLight_FromNormal_SelectsLightNormalTheme` appear in the passed-test list.

## Run summary

| Metric | Value |
| --- | --- |
| Verdict line | `Test Run Successful.` |
| Total tests | 22 |
| Passed | 22 |
| Failed | 0 |
| Skipped | 0 |

## Subset comparison against the P0-T12 baseline

Cited artifact, resolved per § Conventions from the stem `quickfiler-test-run-baseline`:
`<FEATURE>/evidence/baseline/quickfiler-test-run-baseline.2026-08-27T10-22.md`.

| Set | Contents |
| --- | --- |
| `BaselineFailedTests` recorded by `P0-T12` | (empty) |
| This run's failed fully-qualified test names | (empty) |
| Is this run's set a subset of the baseline set? | **yes** — the empty set is a subset of the empty set |

No absolute `Failed: 0` is asserted as the gating condition here, per the task text: the filter
reaches `QfcItemController_InitializationTests` (whose `Part3.cs` is not in § Scope Lock),
`QfcItemController_SeamFactoryTests`, and two tests in the sibling-owned
`QfcItemController.FocusAndThemeTests.cs`. The subset condition is the gate. It happens to reduce to
an absolute all-green result here because the Phase 0 baseline was itself fully green.

## The two named theme tests

Spec AC-6 requires these two by name, so they are absolute pass assertions:

| Test | Result | Duration |
| --- | --- | --- |
| `SetThemeDark_FromNormal_SelectsDarkNormalTheme` | Passed | 114 ms |
| `SetThemeLight_FromNormal_SelectsLightNormalTheme` | Passed | < 1 ms |

Both are in `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`, the sibling-owned
file this feature must not edit. They call
`QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` as bare statements at lines 452 and 468
and discard the return value. Their passing here confirms that the `void` to `IDisposable` return-type
change is source-compatible at a discarding call site and behaviourally non-regressive: a
method-invocation statement may discard a non-`void` result, so `CS0201` does not apply, and the
install-only-when-null rule is preserved so both tests observe the same field state as before.

Raw artifacts live under the git-ignored `TestResults/plan-logs/p2-t5/` tree; the TRX name is
controlled by `LogFileName=consumers.trx` so it carries no account or host name.
