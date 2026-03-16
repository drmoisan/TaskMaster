---
title: "dark-mode-detection - Issue"
issue: "71"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-15T20-11"
status: "Draft"
status_color: "lightgrey"
version: "0.1"
---

# dark-mode-detection (Issue #71)

- Date captured: 2026-03-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dark-mode-detection/ (Issue #71)
- Issue: #71
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/71
- Last Updated: 2026-03-15

- Work Mode: minor-audit

## Problem / Why

When the user clicks `Sort Email`, the QuickFiler form currently opens using the add-in's existing dark theme path instead of detecting and following the active system theme. This creates an inconsistent experience when Windows is set to Light Mode, because the form can still launch in Dark Mode and look out of place relative to the rest of the desktop and Outlook session.

This feature would make the add-in feel native to the environment by selecting the correct theme automatically at launch time. It would also reduce the need for manual theme toggles or saved theme state to correct the UI after the form is already visible.

## Proposed Behavior

When the user launches `Sort Email`, the add-in should determine whether the current system theme is Light Mode or Dark Mode before the form is shown, then apply the matching QuickFiler theme (`LightNormal` or `DarkNormal`) for the initial render. The initial theme decision should come from the system theme instead of relying only on the add-in's persisted `DarkMode` setting.

If the system theme changes between launches, the next `Sort Email` launch should reflect the new theme automatically. If theme detection is temporarily unavailable, the add-in should still open successfully by falling back to the current saved theme or a safe default rather than failing to launch the form.

## Acceptance Criteria (early draft)

- [ ] With Windows set to Light Mode, launching `Sort Email` opens the QuickFiler form using the light theme on first render, including readable labels, buttons, list content, and input controls.
- [ ] With Windows set to Dark Mode, launching `Sort Email` opens the QuickFiler form using the dark theme on first render, including readable labels, buttons, list content, and input controls.
- [ ] Changing the system theme between two `Sort Email` launches causes the next launch to follow the new system theme without requiring a manual theme toggle inside the add-in.
- [ ] If system theme detection fails or is unavailable, the form still opens and uses a deterministic fallback theme rather than crashing or showing an uninitialized theme state.
- [ ] Existing `Sort Email` behavior unrelated to theming (folder search, save options, refresh, create folder, cancel/OK flows) continues to work without regression.

## Constraints & Risks

- The current implementation appears to use `Globals.Ol.DarkMode` plus the persisted `Properties.Settings.Default.DarkMode` value as the theme source, so this feature will need a clear rule for how system detection interacts with that existing state.
- Windows theme, Office theme, and Outlook host theme may not always match; implementation should define whether “follow the system” means Windows app theme specifically or another host-provided theme source.
- Theme selection must occur before the QuickFiler form is displayed to avoid visible flashing from one theme to another during startup.
- WinForms controls, custom themed controls, and any embedded HTML/WebView content may require separate validation to ensure all surfaces remain legible in both modes.
- Scope should remain limited to automatic theme detection for the `Sort Email` experience unless a broader add-in-wide theme-sync effort is explicitly approved.

## Test Conditions to Consider

- [ ] Unit coverage areas: theme-source detection abstraction, mapping from detected mode to `LightNormal` / `DarkNormal`, and fallback behavior when theme detection returns no value or throws an error.
- [ ] Integration scenarios: launch `Sort Email` from Outlook with Windows in Light Mode, launch again with Windows in Dark Mode, and verify the form opens in the correct theme without user intervention.
- [ ] Integration scenarios: verify that theme changes do not break keyboard shortcuts, list selection, folder search, save-option checkboxes, or button actions on the QuickFiler form.
- [ ] Manual regression checks: confirm there is no startup flicker from dark-to-light or light-to-dark after the form is displayed.
- [ ] CLI/API examples: not applicable for this feature because the behavior is triggered from the Outlook add-in UI rather than a public CLI or service API.

## Sync Summary (as of 2026-03-15T20-11)

- Canonical short description: Detect the Windows application theme before opening `Sort Email` and initialize QuickFiler with the matching light/dark theme.
- Current status: **Not Delivered**
- Work mode: `minor-audit`
- Requirements source of truth: `docs/features/active/2026-03-14-dark-mode-detection-71/issue.md`
- Current plan: `docs/features/active/2026-03-14-dark-mode-detection-71/plan.2026-03-14T16-03.md`
- Remote GitHub issue status: `OPEN` (`https://github.com/drmoisan/TaskMaster/issues/71`)
- What changed in this sync run:
	- Reconciled `plan.2026-03-14T16-03.md` with objective evidence.
	- Marked completed file-level implementation/build tasks that are already delivered.
	- Left feature-delivery tasks open where required audit evidence or end-user validation is still missing.

## Acceptance Criteria Evidence (partial, as of 2026-03-15T20-11)

| Criterion | Evidence | Verification command(s) |
|---|---|---|
| Automatic theme detection code path exists before form startup state is initialized | `TaskMaster/AppGlobals/AppOlObjects.cs` now initializes `_darkMode` from `SystemThemeDetector.IsSystemDarkMode()`; `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs` was added; project files include the new production/test files. | Evidence from current git diff for `TaskMaster/AppGlobals/AppOlObjects.cs`, `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs`, `UtilitiesCS/UtilitiesCS.csproj`, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` |
| Analyzer and nullable QA builds passed for the current workspace state | `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-analyzer-2026-03-14T16-03.md` and `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-nullable-2026-03-14T16-03.md` both record `EXIT_CODE: 0`. | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` |

### Open criteria

- Light-mode first-render behavior is not evidenced by any integration or UI-launch artifact.
- Dark-mode first-render behavior is not evidenced by any integration or UI-launch artifact.
- Theme-change-between-launches behavior is not evidenced.
- Deterministic fallback behavior is not evidenced; the current `SystemThemeDetectorTests.cs` still contains machine-dependent tests and does not prove the required fallback scenario.
- Non-theming `Sort Email` regression behavior is not evidenced.

## History / Prior Notes

## Next Step

- [ ] Promote to GitHub issue (feature request template) after confirming the intended theme source precedence (Windows theme vs. Office/Outlook theme).
- [ ] Create `docs/features/active/dark-mode-detection/` folder from the template once the issue is approved for implementation.