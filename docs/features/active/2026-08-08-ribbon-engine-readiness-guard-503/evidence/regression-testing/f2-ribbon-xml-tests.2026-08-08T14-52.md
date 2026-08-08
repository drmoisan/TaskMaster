# F2 — Ribbon-XML Regression Suite Against the Collapsed Resource (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P2-T5]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation /Settings:TaskMaster.runsettings /TestCaseFilter:'FullyQualifiedName~TaskMaster.Test.Ribbon.RibbonExplorerXmlTests'"`
EXIT_CODE: 0

The assembly under test is the one built by P2-T4, whose embedded resource was asserted to carry `EMBEDDED_GETENABLED_COUNT=8` and `EMBEDDED_TRIAGESETA_SINGLELINE=True` in `evidence/other/phase2-build.2026-08-08T14-52.md`.

## Output Summary

```text
  Passed RibbonExplorerXml_IsWellFormedXml [59 ms]
  Passed RibbonExplorerXml_MenusContainOnlyMenuLegalControls [9 ms]
  Passed RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab [7 ms]
  Passed RibbonExplorerXml_TabMailCarriesNoCustomGroup [2 ms]
  Passed RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback [10 ms]
  Passed RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls [50 ms]
  Passed RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled [< 1 ms]
  Passed RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer [2 ms]

Test Run Successful.
Total tests: 8
     Passed: 8
 Total time: 2.9839 Seconds
```

| Metric | Value |
|---|---|
| Total | 8 |
| Passed | 8 |
| Failed | **0** |
| Skipped | **0** |

## The four acceptance-criterion tests named by the task

| Criterion | Test | Result |
|---|---|---|
| **AC5** | `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` | **Passed** |
| **AC6** | `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` | **Passed** |
| **AC7** | `RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled` | **Passed** |
| **AC8** | `RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer` | **Passed** |

AC5 is now enforced by the **corrected, proven-non-vacuous** assertion from P1-T1, not by the short-circuiting form the review flagged. AC6 continues to enforce set equality independently. AC7 confirms every catalog id still resolves to a `<button>` element, which the collapse preserved. AC8 is a reflection assertion on `RibbonViewer` and is unaffected by the XML layout.

The four other tests in the class (`IsWellFormedXml`, `MenusContainOnlyMenuLegalControls`, `TaskMasterGroupsLiveUnderTaskmasterTab`, `TabMailCarriesNoCustomGroup`) also pass, confirming the collapse broke no structural invariant of the document.

Binary outcome satisfied: `EXIT_CODE: 0` with zero failed and zero skipped.
