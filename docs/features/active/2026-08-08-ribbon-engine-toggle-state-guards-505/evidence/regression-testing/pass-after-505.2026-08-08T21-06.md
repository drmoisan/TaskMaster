# P3-T8 — Green After the Fix (R1-R5)

Timestamp: 2026-08-08T21-06

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<MSBUILD>' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'; & '<VSTEST>' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon'"
```

EXIT_CODE: 0 (build reported `0 Error(s)`; vstest exited 0)

## Output Summary

`Test Run Successful.` Full `TaskMaster.Test.Ribbon` namespace: Total **107** — Passed: **107**,
Failed: **0**, Skipped: **0**. Wall time 1.9596 s.

## Every P1-T4 and P3-T5 failure is now PASSED

Cross-reference of the fail-before artifacts:

- `<FEATURE>\evidence\regression-testing\fail-before-505.2026-08-08T20-52.md` (R1, R2, R3, R5)
- `<FEATURE>\evidence\regression-testing\fail-before-r4-xml.2026-08-08T21-04.md` (R4)

| Previously FAILED | R# | Now |
|---|---|---|
| `ToggleGetPressedCallbacks_MatchOfficeCheckBoxGetPressedSignature` | R1 | **Passed** |
| `GetPressedCallbacks_BeforeSetGlobals_ReturnFalseWithoutThrowing` | R2 | **Passed** |
| `ToggleClickHandlers_AreAsyncVoidAwaitedShape` | R5 | **Passed** |
| `ShowSaveInfoHandlers_AreAsyncVoidAwaitedShape` | R5 | **Passed** |
| `TryGetEngineName_ForEachEngineBackedControlId_ReturnsExpectedEngineName ("SpamSaveNetwork","Spam")` | R3 | **Passed** |
| `... ("SpamSaveLocal","Spam")` | R3 | **Passed** |
| `... ("GetSaveState","Spam")` | R3 | **Passed** |
| `... ("TriageSaveNetwork","Triage")` | R3 | **Passed** |
| `... ("TriageSaveLocal","Triage")` | R3 | **Passed** |
| `... ("TriageGetSaveState","Triage")` | R3 | **Passed** |
| `ControlIds_ContainsExactlyTheFourteenEngineBackedControlIds` | R3 | **Passed** |
| `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` | R4 | **Passed** |
| `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` | R4 | **Passed** |

`ToggleOnActionCallbacks_MatchOfficeCheckBoxOnActionSignature`, which passed pre-fix, still passes,
confirming the #506 rewrite did not regress the `onAction` signatures.

The 22 new seam tests (25 cases) all pass, as do the two untouched #507 tests
(`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing`,
`Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines`), confirming
`RibbonController.Engines => Globals?.Engines;` was not disturbed.

Binary outcome: **PASS** — the AC-15 red-then-green pair is complete for R1 through R5.
