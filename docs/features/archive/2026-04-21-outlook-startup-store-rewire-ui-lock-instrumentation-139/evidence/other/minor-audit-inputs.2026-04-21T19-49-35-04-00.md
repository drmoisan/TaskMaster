# Minor-Audit Inputs

Timestamp: 2026-04-21T19:49:35-04:00
Work Mode: minor-audit
Acceptance Criteria Section: `issue.md` contains an explicit `## Acceptance Criteria` section.
Plan Path: `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/plan.2026-04-21T16-50.md`
SearchScope: docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/
SearchPatterns: spec.md, user-story.md
SearchResult: none

Acceptance Criteria Checkboxes:
- [ ] `StoresWrapper.RewireOlObjectsAsync()` logs total filtered-store timing, total rewire timing, and per-store loop timing with the `[Startup timing]` prefix.
- [ ] `StoreWrapper.Init()` and `StoreWrapper.GetSmtpAddressFromStore()` log per-call elapsed milliseconds for the targeted Outlook COM boundaries identified in the research note.
- [ ] `StoreWrapper.Restore()` and `FolderMinimalWrapper.RestoreFromRelativePath()` log timing needed to distinguish folder-restoration delays from store-init delays.
- [ ] The diagnostic code compiles cleanly, uses the existing `log4net` infrastructure, and does not change the functional startup behavior beyond additional debug logging.
