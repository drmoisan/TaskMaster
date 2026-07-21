# Batch 7 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T09-55
- Task: [P7-T8]

## Opted-in files (6, ThemeHelpers + ToolTips)

- `ThemeHelpers/SystemThemeDetector.cs` — `OpenSubKey` result annotated `RegistryKey?` and `GetValue` result `object?`, consumed by the existing guards.
- `ThemeHelpers/Theme.cs` — optional-parameter defaults annotated (`IUiDispatcher? uiDispatcher = null`, `Action<string>? breadcrumbThemeNotifier = null`); reference-type fields set only by the full constructor use `= null!` (ctor-path-initialized contract), except `_breadcrumbThemeNotifier` which is genuinely optional (`Action<string>?`, guarded by `?.Invoke`).
- `ThemeHelpers/Theme.Rendering.cs` — same partial `Theme` type, opted in together; `menuItem as ToolStripMenuItem` deref uses behavior-preserving `!` under the `is` guard.
- `ThemeHelpers/ThemeControlGroup.cs` — per-group-type reference fields use `= null!`; event handlers annotated `object? sender` with behavior-preserving `!` on the cast.
- `ToolTips/QfcTipsDetails.cs` — `= null!` on ctor/async-init-populated fields; behavior-preserving `!` on the validated `_labelControl.Parent` casts/derefs.
- `ToolTips/TipsController.cs` — `= null!` on InitializeLabel-populated fields; behavior-preserving `!` on the validated `_labelControl.Parent` casts/derefs.

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only; isolated build is the authoritative CS86xx signal — see P0-T4).

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (pre-existing non-nullable CS0618/CS0168, unchanged). No new diagnostics introduced by Batch 7.
- Result: PASS. All 6 Batch-7 opted-in files reach zero CS86xx; the `Theme` partial type (`Theme.cs` + `Theme.Rendering.cs`) is opted in together for consistent field-null-state analysis.
