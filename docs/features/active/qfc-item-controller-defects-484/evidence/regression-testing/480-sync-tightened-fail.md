# Issue #480 — Tightened Synchronous Assertion Fails Against the Unfixed Code

Timestamp: 2026-08-26T08-47
Task: [P1-T2] [expect-fail]

ExpectedExitCode: 1

## Build step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0 (3 warnings, 0 errors)

This build is **not** an analyzer or nullable gate. Per decision D2 it exists only to produce a fresh
`QuickFiler.Test.dll` so the regression test can run. The analyzer and nullable gates are `[P7-T3]` and
`[P7-T4]`, which use `/t:Rebuild` against `TaskMaster.sln`.

**Deviation recorded.** The plan task text spells the platform switch `"/p:Platform=Any CPU"`. That is the
solution-level platform name. `QuickFiler.Test.csproj` declares the project-level name `AnyCPU`
(`QuickFiler.Test.csproj:12`, `:32`, `:36`), and a project-level build with `Platform=Any CPU` fails with
`error : The BaseOutputPath/OutputPath property is not set for project 'QuickFiler.Test.csproj'` because
no `'Debug|Any CPU'` property group exists. The switch was therefore spelled `/p:Platform=AnyCPU` for this
and every other project-level build in this plan. The solution-level gate tasks `[P7-T3]` and `[P7-T4]`
continue to use `"/p:Platform=Any CPU"` verbatim, which is correct at the solution level.

## Test step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ToggleNavigation_Synchronous_TogglesPositionTips" "/Logger:trx;LogFileName=480-sync-tightened-fail.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\480-sync-tightened-fail
```

EXIT_CODE: **1**

## Result

| Test | Outcome |
|---|---|
| `ToggleNavigation_Synchronous_TogglesPositionTips` | **Failed** |

```
Total tests: 1
     Failed: 1
Test Run Failed.
```

Failure message from the TRX:

```
Test method QuickFiler.Controllers.Tests.QfcItemController_FocusAndThemeTests.ToggleNavigation_Synchronous_TogglesPositionTips threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 2 times: t => t.Toggle(False)

Performed invocations:

   Mock<IQfcTipsDetails:1> (t):

      IQfcTipsDetails.Toggle(False)
      IQfcTipsDetails.Toggle(False)
```

## Interpretation

The failure is exactly the #480 defect. `ToggleNavigation(bool async)` performs an unconditional
`_itemPositionTips.Toggle(false)` dispatch at
`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:170` and then repeats it in the taken branch,
so two invocations reach the mock. The pre-existing `Times.AtLeastOnce()` assertion recorded as `Passed`
in `[P0-T13]` was satisfied by both 1 and 2 invocations and therefore masked the defect; the tightened
`Times.Once()` assertion does not.

TRX:
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/480-sync-tightened-fail/480-sync-tightened-fail.trx`

Output Summary: The tightened assertion fails against the unfixed code with `EXIT_CODE: 1` and the
expected Moq message reporting 2 invocations where 1 was expected. This is the fail-before evidence for
issue #480.
