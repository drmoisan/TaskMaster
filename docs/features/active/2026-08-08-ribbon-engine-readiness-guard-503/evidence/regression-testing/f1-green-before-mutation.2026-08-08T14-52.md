# F1 — Corrected Assertion Green Before the Mutation (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T4]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation /Settings:TaskMaster.runsettings /TestCaseFilter:'FullyQualifiedName~TaskMaster.Test.Ribbon.RibbonExplorerXmlTests'"`
EXIT_CODE: 0

This run establishes that the corrected assertion is **green against the unmutated resource**, which is the necessary control for the P1-T7 fail-proof. Without it, a failure at P1-T7 could not be attributed to the mutation rather than to the P1-T1 edit itself.

## Output Summary

```text
  Passed RibbonExplorerXml_IsWellFormedXml [62 ms]
  Passed RibbonExplorerXml_MenusContainOnlyMenuLegalControls [9 ms]
  Passed RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab [9 ms]
  Passed RibbonExplorerXml_TabMailCarriesNoCustomGroup [2 ms]
  Passed RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback [11 ms]
  Passed RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls [56 ms]
  Passed RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled [< 1 ms]
  Passed RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer [2 ms]

Test Run Successful.
Total tests: 8
     Passed: 8
 Total time: 3.0785 Seconds
```

| Metric | Value |
|---|---|
| Total | 8 |
| Passed | 8 |
| Failed | **0** |
| Skipped | **0** |

`RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` — the method changed by P1-T1 — is reported **Passed**.

The assembly under test is the one built by P1-T3, whose embedded resource was asserted to carry `EMBEDDED_GETENABLED_COUNT=8` in `evidence/other/phase1-build-premutation.2026-08-08T14-52.md`.

Binary outcome satisfied: `EXIT_CODE: 0` with zero failed and zero skipped.
