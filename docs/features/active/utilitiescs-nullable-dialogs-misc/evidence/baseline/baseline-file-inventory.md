# Phase 0 — Baseline File Inventory

- Timestamp: 2026-07-19T10-53
- Task: [P0-T2]
- Issue: #374

Line counts via `wc -l`; `#nullable enable` presence via `grep -q`. Confirmed: none of the 18
files currently carries `#nullable enable` (all report `no`).

## Remediation Targets (12 substantive, Batches A–E)

| Batch | File | Lines | #nullable enable | Classification |
|---|---|---|---|---|
| A | UtilitiesCS/Dialogs/DelegateButtonTemplate.cs | 20 | no | remediation-target |
| A | UtilitiesCS/Dialogs/FolderNotFoundViewer.cs | 52 | no | remediation-target |
| A | UtilitiesCS/Dialogs/MyBoxViewer.cs | 127 | no | remediation-target |
| A | UtilitiesCS/Dialogs/InputBoxViewer.cs | 59 | no | remediation-target |
| B | UtilitiesCS/Dialogs/ActionButton.cs | 186 | no | remediation-target |
| B | UtilitiesCS/Dialogs/DelegateButton.cs | 185 | no | remediation-target |
| B | UtilitiesCS/Dialogs/FunctionButton.cs | 363 | no | remediation-target |
| C | UtilitiesCS/Dialogs/InputBox.cs | 98 | no | remediation-target |
| C | UtilitiesCS/Dialogs/NotImplementedDialog.cs | 56 | no | remediation-target |
| D | UtilitiesCS/Dialogs/MyBox.cs | 415 | no | remediation-target |
| E | UtilitiesCS/Dialogs/MyBoxModeless.cs | 127 | no | remediation-target |
| E | UtilitiesCS/Dialogs/YesNoToAll.cs | 109 | no | remediation-target |

## Verify-Only Misc (2)

| File | Lines | #nullable enable | Classification |
|---|---|---|---|
| UtilitiesCS/WindowsAPI/ExtraDeclarations.cs | 67 | no | verify-only |
| UtilitiesCS/Properties/AssemblyInfo.cs | 39 | no | verify-only |

## Designer-Excluded (4, NEVER opted in, NEVER edited)

| File | Lines | #nullable enable | Classification |
|---|---|---|---|
| UtilitiesCS/Dialogs/DelegateButtonTemplate.Designer.cs | 75 | no | Designer-excluded |
| UtilitiesCS/Dialogs/FolderNotFoundViewer.Designer.cs | 144 | no | Designer-excluded |
| UtilitiesCS/Dialogs/InputBoxViewer.Designer.cs | 169 | no | Designer-excluded |
| UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs | 177 | no | Designer-excluded |

## Summary

- Total files enumerated: 18 (16 in `UtilitiesCS/Dialogs/` + 2 misc).
- Remediation targets receiving the pragma: 12 substantive + 2 verify-only = 14.
- Designer-excluded: 4.
- None currently carries `#nullable enable` (AC1 baseline: all files start null-oblivious).
- All files are under the 500-line limit (largest: MyBox.cs at 415 lines) — no do-not-split flag needed.
