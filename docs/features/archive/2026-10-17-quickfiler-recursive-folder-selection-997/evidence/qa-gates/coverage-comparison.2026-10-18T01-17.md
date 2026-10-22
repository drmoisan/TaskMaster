Timestamp: 2026-10-18T01-17
Command: python SCRATCH/issue-997-compare-coverage.py
EXIT_CODE: 1
Output Summary: Baseline repository lines 56931/66178 (86.03%); final 57247/66498 (86.09%); delta +0.06 percentage points.
Changed executable lines: baseline 201/206 (97.57%); final 528/535 (98.69%).
New measured production files: 379/384 (98.70%). New method units measured: 45.
Comparison basis: merge base of origin/main and HEAD to delivered working tree, plus explicit untracked production-file inventory; not committed HEAD range. Duplicate class records coalesced by source filename/line with maximum observed hits.
Detailed baseline: SCRATCH/issue-997-baseline-20261018005944.cobertura.xml; final: SCRATCH/issue-997-final-20261018011548.cobertura.xml. Source baseline revision and final revision recorded under SCRATCH; source/hunk coordinates use the actual working tree. Existing instrumentation/exclusions unchanged.

| File | Baseline covered/valid | Final covered/valid | Measurement |
| --- | --- | --- | --- |
| QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs | 0/1 (0.00%) | 0/1 (0.00%) | measured |
| QuickFiler/Properties/Resources.Designer.cs | 0/0 (no executable lines) | No instrumented executable lines | Existing coverage configuration; generated/viewer glue or embedded asset, composition/host contracts apply. |
| QuickFiler/Resources/FolderBreadcrumb.html | 0/0 (no executable lines) | No instrumented executable lines | Existing coverage configuration; generated/viewer glue or embedded asset, composition/host contracts apply. |
| QuickFiler/Resources/FolderBreadcrumb.js | 0/0 (no executable lines) | No instrumented executable lines | Existing coverage configuration; generated/viewer glue or embedded asset, composition/host contracts apply. |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Descendants.cs | 0/0 (no executable lines) | 30/30 (100.00%) | measured |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | 244/244 (100.00%) | 251/251 (100.00%) | measured |
| QuickFiler/Viewers/FolderBreadcrumbDocument.cs | 0/0 (no executable lines) | 4/4 (100.00%) | measured |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 0/0 (no executable lines) | No instrumented executable lines | Existing coverage configuration; generated/viewer glue or embedded asset, composition/host contracts apply. |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | 0/0 (no executable lines) | 109/110 (99.09%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs | 268/272 (98.53%) | 185/188 (98.40%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs | 0/0 (no executable lines) | 45/45 (100.00%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | 123/123 (100.00%) | 152/152 (100.00%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs | 52/52 (100.00%) | 68/68 (100.00%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Descendants.cs | 0/0 (no executable lines) | 51/51 (100.00%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs | 316/316 (100.00%) | 303/303 (100.00%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs | 178/178 (100.00%) | 203/203 (100.00%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs | 169/182 (92.86%) | 184/199 (92.46%) | measured |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | 118/120 (98.33%) | 128/130 (98.46%) | measured |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | 0/0 (no executable lines) | 140/144 (97.22%) | measured |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | 306/310 (98.71%) | 239/239 (100.00%) | measured |

| New method source | Method | Covered/valid |
| --- | --- | --- |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Descendants.cs | <DispatchDescendantMessageAsync>b__0 () | 6/6 (100.00%) |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Descendants.cs | <DispatchDescendantMessageAsync>b__1 () | 1/1 (100.00%) |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Descendants.cs | MoveNext () | 24/24 (100.00%) |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | .ctor (QuickFiler.Viewers.IWebViewMessenger, UtilitiesCS.OutlookObjects.Folder.IFolderHierarchyProvider, QuickFiler.Viewers.BreadcrumbUiDispatcher, string) | 18/18 (100.00%) |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs | <PostSelectorStateCore>b__49_0 (UtilitiesCS.OutlookObjects.Folder.BreadcrumbSelectorOptionState) | 5/5 (100.00%) |
| QuickFiler/Viewers/FolderBreadcrumbDocument.cs | get_Html () | 4/4 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | <ParseCells>b__12_0 (Newtonsoft.Json.Linq.JToken) | 19/20 (95.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | <ParseRows>b__11_0 (Newtonsoft.Json.Linq.JToken) | 13/13 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | <ParseSubfolders>b__13_0 (Newtonsoft.Json.Linq.JToken) | 16/16 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | <RowToJson>b__9_0 (UtilitiesCS.OutlookObjects.Folder.BreadcrumbCellRender) | 7/7 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | <SubfoldersToJson>b__10_0 (UtilitiesCS.OutlookObjects.Folder.BreadcrumbSubfolderRender) | 11/11 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | OptionalNodeIdentity (Newtonsoft.Json.Linq.JObject) | 3/3 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | ParseCells (Newtonsoft.Json.Linq.JArray) | 13/13 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | ParseRows (Newtonsoft.Json.Linq.JArray) | 4/4 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | ParseSubfolders (Newtonsoft.Json.Linq.JArray) | 5/5 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | RowToJson (UtilitiesCS.OutlookObjects.Folder.BreadcrumbRowRender) | 14/14 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.Render.cs | SubfoldersToJson (System.Collections.Generic.IReadOnlyList<UtilitiesCS.OutlookObjects.Folder.BreadcrumbSubfolderRender>) | 4/4 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs | .ctor (System.Collections.Generic.IReadOnlyList<UtilitiesCS.OutlookObjects.Folder.BreadcrumbRowRender>, int, string, string) | 12/12 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs | .ctor (UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbSegment) | 6/6 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs | <Attach>b__0 (UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbSegment) | 9/9 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs | <Attach>b__1 (UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode) | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs | Attach (System.Collections.Generic.IReadOnlyList<UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbSegment>) | 8/8 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs | Collapse () | 8/8 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs | Find (string) | 13/13 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | .ctor (string, string, bool, string, bool, bool, bool, System.Collections.Generic.IReadOnlyList<UtilitiesCS.OutlookObjects.Folder.BreadcrumbSubfolderRender>) | 20/20 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | <ProjectDescendant>b__0 (UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode) | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | <ProjectRow>b__1 (UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode) | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | ProjectDescendant (UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode, UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode) | 11/11 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | ProjectRow (UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateRow, int, bool, UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode) | 58/58 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs | TryDescendantValue (UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateModel, UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode, out string) | 14/14 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Descendants.cs | ActivateDescendant (string, string, int) | 22/22 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Descendants.cs | ActivateSubfolder (string, int) | 15/15 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Descendants.cs | CaptureDescendantSelection () | 4/4 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Descendants.cs | RestoreDescendantSelection (int) | 10/10 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs | .ctor (string, string, bool) | 16/16 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs | <.ctor>b__6_0 (UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbSegment) | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs | get_DescendantRoot () | 9/11 (81.82%) |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | SelectDescendant (int, UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode, int) | 5/5 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | <ActivateSelectorDescendant>b__0 () | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | <ToggleDescendantAsync>b__0 (UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateRow) | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | ActivateSelectorDescendant (string, string) | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs | IsCurrentRequest (int, UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateRow, UtilitiesCS.OutlookObjects.Folder.BreadcrumbDescendantNode, int, int) | 7/7 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | .ctor (UtilitiesCS.OutlookObjects.Folder.IFolderHierarchyProvider, string) | 8/8 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | <GetFolderItems>b__25_0 () | 1/1 (100.00%) |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | <GetSelectedFolder>b__24_0 () | 1/1 (100.00%) |

Findings: New method below 90%: UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs get_DescendantRoot (): 9/11 (81.82%)
