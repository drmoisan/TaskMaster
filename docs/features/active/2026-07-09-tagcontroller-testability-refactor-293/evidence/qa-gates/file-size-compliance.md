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

## Remediation Update (2026-07-09T23-15) — Full Tags + Tags.Test Enumeration

Timestamp: 2026-07-09T23-15

Command: `find Tags Tags.Test -name '*.cs' -exec wc -l {} + | sort -rn`
EXIT_CODE: 0

Output Summary: `Tags.Test/TagControllerSeamTests.cs` was 579 lines, exceeding the 500-line
limit (feature-review remediation-inputs.2026-07-09T22-52.md, one Blocking finding). It was split
by logical cohesion into `Tags.Test/TagControllerSeamTests.cs` (dialog-routed methods, auto-assign
flows, property forwarders, and the shared test-construction helpers) and
`Tags.Test/TagControllerSeamTests.KeyboardNavigation.cs` (keyboard and navigation handlers), both
declared `partial class TagControllerSeamTests` so the single `[TestClass]` identity and all 64
`[TestMethod]` bodies/assertions are preserved unchanged. `Tags.Test/Tags.Test.csproj` (legacy,
explicit `<Compile Include>`, no glob) was updated with a new `<Compile Include>` entry for the
added file. No production `Tags/*.cs` file was modified. Every file below is at or under the
500-line limit; the two generated `obj/Debug/*.AssemblyAttributes.cs` files are gitignored build
output and excluded from this source-tree enumeration.

| File | Lines | <= 500 |
|---|---|---|
| Tags/My Project/MyNamespace.Static.1.Designer.cs | 467 | yes |
| Tags.Test/TagControllerCoverageExpansionTests.cs | 466 | yes |
| Tags/TagController.cs | 435 | yes |
| Tags.Test/TagControllerSeamTests.cs | 392 | yes |
| Tags/TagController.Rendering.cs | 327 | yes |
| Tags/Helper Classes/CheckBoxController.cs | 257 | yes |
| Tags/My Project/MyNamespace.Static.2.Designer.cs | 241 | yes |
| Tags/TagViewer.Designer.cs | 235 | yes |
| Tags.Test/TagSelectionModelTests.cs | 228 | yes |
| Tags/TagSelectionModel.cs | 224 | yes |
| Tags.Test/TagControllerSeamTests.KeyboardNavigation.cs | 215 | yes |
| Tags/TagLauncher.cs | 169 | yes |
| Tags/TagViewer.cs | 167 | yes |
| Tags/CheckBoxController.cs | 167 | yes |
| Tags.Test/Fakes/FakeTagViewer.cs | 137 | yes |
| Tags/LauncherAutoAssign.cs | 112 | yes |
| Tags.Test/TagControllerTests.cs | 105 | yes |
| Tags.Test/CheckBoxControllerWiring.StaTests.cs | 104 | yes |
| Tags.Test/LauncherAutoAssignTests.cs | 99 | yes |
| Tags/My Project/Settings.Designer.cs | 87 | yes |
| Tags.Test/CheckBoxControllerDecisionTests.cs | 74 | yes |
| Tags/Resources.Designer.cs | 63 | yes |
| Tags.Test/TagControllerRendering.StaTests.cs | 63 | yes |
| Tags/Helper Classes/PrefixItem.cs | 60 | yes |
| Tags/ITagViewer.cs | 59 | yes |
| Tags/properties/AssemblyInfo.cs | 40 | yes |
| Tags/My Project/AssemblyInfo.cs | 35 | yes |
| Tags/WinFormsUserPrompt.cs | 25 | yes |
| Tags/AutoAssignInterface.cs | 22 | yes |
| Tags/IUserPrompt.cs | 21 | yes |
| Tags.Test/Properties/AssemblyInfo.cs | 20 | yes |
| Tags/My Project/MyNamespace.Static.3.Designer.cs | 15 | yes |
| Tags/My Project/Application.Designer.cs | 11 | yes |

Result: **PASS** — every `Tags/*.cs` and `Tags.Test/*.cs` file is at or under the 500-line limit;
the maximum is 467 lines (a pre-existing, unmodified Designer file).
