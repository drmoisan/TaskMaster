# select-junk-folders (Issue #119)

- Date captured: 2026-04-05
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/select-junk-folders/ (Issue #119)
- Issue: #119
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/119
- Last Updated: 2026-04-05
- Work Mode: minor-audit

## Problem / Why

TaskMaster already stores separate Outlook folder settings for confirmed junk (`OlJunkCertain`) and potential junk (`JunkPotential`), but the current user experience is mostly reactive. Users only get a folder picker after a saved folder can no longer be resolved, which means there is no clear, intentional place to review or change these destinations before something breaks.

This creates friction when a mailbox is renamed, a folder is moved, a new store becomes primary, or the user simply wants to tune SpamBayes routing behavior. A dedicated UI entry point would make the junk-folder configuration discoverable, reduce error-driven prompts, and let users correct both settings without waiting for a failed lookup path.

## Proposed Behavior

Add a user-visible configuration entry point in the add-in UI for selecting the Outlook folders used for confirmed junk and potential junk. The screen should display the current selections, allow each folder to be changed independently through the Outlook folder picker, and save the chosen folders back to the existing settings fields using the same relative-path storage model that `AppOlObjects` already expects.

Saving should persist both values, refresh any cached junk-folder references, and let the rest of the add-in use the updated selections without relying on the current error-first fallback. Canceling should leave existing settings unchanged, and the UI should handle missing or invalid folders by guiding the user to reselect them instead of failing silently.

## Acceptance Criteria (early draft)

- [ ] The add-in exposes a clear UI entry point where the user can review the current `Junk Email` and `Junk Potential` folder selections without waiting for a folder-resolution failure.
- [ ] The user can change the confirmed junk folder and the potential junk folder independently, using the standard Outlook folder picker for each field.
- [ ] Choosing `Save` persists the selected folders to the existing settings keys used by `AppOlObjects` (`OlJunkCertain` and `JunkPotential`) in the relative-path format expected by the current folder-loading logic.
- [ ] After saving, subsequent junk or potential-junk operations use the updated folder selections without requiring the user to trigger the existing "folder not found" recovery path first.
- [ ] Choosing `Cancel` leaves both stored settings and active folder selections unchanged.
- [ ] If a previously saved folder no longer exists or cannot be resolved, the UI shows a clear re-selection path and does not overwrite the stored value with an invalid selection.

## Constraints & Risks

- The existing folder settings are cached in `AppOlObjects` (`_junkCertain` and `_junkPotential`), so the feature must define how those cached references are refreshed after the user saves new values.
- The current implementation stores folder paths relative to the Outlook root folder. That keeps settings portable within a store, but folder renames, mailbox changes, and store migrations can still invalidate saved paths.
- Outlook folder pickers and folder objects are COM-backed UI interactions, so the new entry point must remain compatible with the add-in's current threading and Outlook session assumptions.
- The repository already contains reactive recovery behavior in `LoadJunkCertain()` and `LoadJunkPotential()`. The new entry point should complement that fallback, not create a second inconsistent persistence path.
- Scope should stay limited to selecting and persisting junk-folder destinations; broader spam-manager redesign, classifier logic changes, or mailbox-provisioning workflows are out of scope for this feature.

## Test Conditions to Consider

- [ ] Unit coverage areas: reading existing settings into the UI model, saving updated `OlJunkCertain` and `JunkPotential` values, cancel behavior, cache refresh/reset behavior, and invalid or empty selection guards.
- [ ] Integration scenarios: open the entry point with valid existing folders, change only one folder, change both folders, save and reopen to confirm persistence, and recover after one of the saved folders is deleted, renamed, or moved.
- [ ] CLI/API examples: none expected for this feature unless a diagnostic or developer-only command is added later; the primary surface is an in-add-in UI flow.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/select-junk-folders/` folder from the template