# F1 — Pass-After-Restore (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T10]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation /Settings:TaskMaster.runsettings /TestCaseFilter:'FullyQualifiedName~TaskMaster.Test.Ribbon.RibbonExplorerXmlTests'"`
EXIT_CODE: 0

## Output Summary

```text
  Passed RibbonExplorerXml_IsWellFormedXml [50 ms]
  Passed RibbonExplorerXml_MenusContainOnlyMenuLegalControls [7 ms]
  Passed RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab [7 ms]
  Passed RibbonExplorerXml_TabMailCarriesNoCustomGroup [1 ms]
  Passed RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback [9 ms]
  Passed RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls [47 ms]
  Passed RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled [< 1 ms]
  Passed RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer [2 ms]

Test Run Successful.
Total tests: 8
     Passed: 8
 Total time: 2.7982 Seconds
```

| Metric | Value |
|---|---|
| Total | 8 |
| Passed | 8 |
| Failed | **0** |
| Skipped | **0** |

`RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` is reported **Passed**. `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` — the AC6 sibling that also failed under the mutation — is likewise back to **Passed**.

## The fail-then-pass pair

| Stage | Artifact | Embedded count | AC5 test | Exit |
|---|---|---|---|---|
| Green before mutation | `evidence/regression-testing/f1-green-before-mutation.2026-08-08T14-52.md` | 8 | Passed | 0 |
| **Fail with mutation** | **`evidence/regression-testing/f1-fail-proof.2026-08-08T14-52.md`** | **7** | **Failed** | **1** |
| Restored | `evidence/regression-testing/f1-mutation-restored.2026-08-08T14-52.md` | 8 (on disk) | — | 0 |
| **Pass after restore** | this artifact | 8 | Passed | 0 |

The pair proves the corrected assertion is non-vacuous: the same command, the same test, and the same corrected source produce a **Failure** when and only when the `getEnabled` attribute is absent from the resource the test actually reads. Before the P1-T1 change, the identical mutation produced a silent pass.

Binary outcome satisfied: `EXIT_CODE: 0` with zero failed and zero skipped.
