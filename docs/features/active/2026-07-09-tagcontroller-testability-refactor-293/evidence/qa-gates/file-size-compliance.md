# File-Size Compliance (P3-T6)

Timestamp: 2026-07-09T22-16

Command: `wc -l Tags/TagController.cs Tags/TagController.Rendering.cs Tags/TagViewer.cs Tags/TagSelectionModel.cs Tags/ITagViewer.cs Tags/IUserPrompt.cs Tags/WinFormsUserPrompt.cs`
EXIT_CODE: 0

Output Summary: All production files in `Tags/` are at or under the 500-line limit after the split.

| File | Lines | <= 500 |
|---|---|---|
| Tags/TagController.cs | 435 | yes |
| Tags/TagController.Rendering.cs | 327 | yes |
| Tags/TagViewer.cs | 167 | yes |
| Tags/TagSelectionModel.cs | 224 | yes |
| Tags/ITagViewer.cs | 59 | yes |
| Tags/IUserPrompt.cs | 21 | yes |
| Tags/WinFormsUserPrompt.cs | 25 | yes |

Note: the original `Tags/TagController.cs` was 877 lines. To bring the main controller partial under
500 (it measured 523 after the initial Phase 3 split), the keyboard-navigation event handlers
(`OptionsPanel_PreviewKeyDown`, `OptionsPanel_KeyDown`, `TagViewer_KeyDown`, `SearchText_KeyDown`,
`SearchText_KeyUp`) were relocated into the navigation partial `TagController.Rendering.cs` per the
P3-T6 "split further before proceeding" directive. `TagLauncher.cs` and `CheckBoxController.cs`
file sizes are recorded after Phase 4 / Phase 6 changes in the final QA evidence.
