# Targeted Verification — RibbonExplorerXmlTests (Issue #185)

Timestamp: 2026-06-12T10-45

Command: vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /Tests:RibbonExplorerXml_IsWellFormedXml,RibbonExplorerXml_MenusContainOnlyMenuLegalControls,RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab,RibbonExplorerXml_TabMailCarriesNoCustomGroup

Note: `/InIsolation` is required for this Moq-based assembly. The filter restricts the run to
the two pre-existing RibbonExplorerXmlTests and the two new tests added in P1-T4/P1-T5.

EXIT_CODE: 0

Output Summary: Test Run Successful. Total tests: 4, Passed: 4, Failed: 0. Total time: 0.69s.

Per-test results (against the post-change RibbonExplorer.xml):
- RibbonExplorerXml_IsWellFormedXml ............................. Passed (40 ms) [pre-existing]
- RibbonExplorerXml_MenusContainOnlyMenuLegalControls .......... Passed (6 ms)  [pre-existing]
- RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab ..... Passed (5 ms)  [new, P1-T4]
- RibbonExplorerXml_TabMailCarriesNoCustomGroup ................ Passed (1 ms)  [new, P1-T5]

Confirms AC5: RibbonExplorer.xml remains well-formed, the existing RibbonExplorerXmlTests pass,
and the new regression tests assert the Taskmaster tab placement (four groups under the
Taskmaster tab) and that TabMail carries no custom group.
