# Issue #480 — New `async: true` Exact-Count Test Fails Against the Unfixed Code

Timestamp: 2026-08-26T08-51
Task: [P1-T4] [expect-fail]

ExpectedExitCode: 1

## Build step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0 (3 warnings, 0 errors)

Not an analyzer or nullable gate (decision D2). The platform-switch spelling deviation is recorded in
`480-sync-tightened-fail.md` and applies to every project-level build in this plan.

## Test step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce" "/Logger:trx;LogFileName=480-async-fail.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\480-async-fail
```

EXIT_CODE: **1**

## Result

| Test | Outcome |
|---|---|
| `ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce` | **Failed** |

```
Total tests: 1
     Failed: 1
Test Run Failed.
```

Failure message from the TRX:

```
Test method QuickFiler.Controllers.Tests.QfcItemController_MailActionsTests.ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 2 times: t => t.Toggle(False)

Performed invocations:

   Mock<IQfcTipsDetails:1> (t):

      IQfcTipsDetails.Toggle(False)
      IQfcTipsDetails.Toggle(False)
```

## Interpretation

The `async: true` branch was previously untested. It exhibits the same #480 defect as the synchronous
branch: the unconditional dispatch at
`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:170` runs, and the `if (async)` branch then
dispatches a second time, so two `Toggle(false)` invocations reach the mock where one is expected.

Test routing: the method was placed in
`QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` per the plan's constraint C2 capacity
table, which supersedes research section 8.5's illustration of routing it into
`QfcItemController.EventWiringTests.cs`. `QfcItemController.FocusAndThemeTests.cs` cannot host it: it is
at its 497-line baseline with 3 spare lines.

The shared arrange helper `QfcItemControllerTestSupport.BuildExecutingViewer()` was appended to
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (additive only, appended after the last
existing member) because the equivalent `private static BuildExecutingViewer()` in
`QfcItemController.FocusAndThemeTests.cs:99-115` is not reachable from another test file.

TRX:
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/480-async-fail/480-async-fail.trx`

Output Summary: The new `async: true` exact-count test fails against the unfixed code with
`EXIT_CODE: 1` and the expected Moq message reporting 2 invocations where 1 was expected. This is the
fail-before evidence for the previously-untested #480 branch.
