# outlook-startup-ui-lockup-followup (Issue #148)

- Date captured: 2026-05-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-startup-ui-lockup-followup/ (Issue #148)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #148
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/148
- Last Updated: 2026-05-07
- Work Mode: full-bug

## Summary

Outlook and TaskMaster still lock up for an extended period during startup and during first email interactions while initial loading work continues without updating the UI. This follow-up bug remains unresolved after issue `#141` and should use the timing approach from issue `#139` to isolate remaining UI-thread-heavy startup and mail-selection paths.

## Environment

- OS/version: Windows 10/11 with Outlook desktop and the TaskMaster VSTO add-in enabled
- Python version: Not applicable; this path is a .NET Framework Outlook add-in startup and Outlook interaction path
- Command/flags used: Standard Outlook launch into `ThisAddIn.Application_Startup()` with no special flags
- Data source or fixture: Live Outlook profile with the normal TaskMaster startup data/configuration and enough mailbox content to click messages during the initial load window

## Steps to Reproduce

1. Start Outlook with the TaskMaster add-in enabled.
2. During the initial startup load window, observe the Outlook window while TaskMaster startup work is still running.
3. Click one or more emails before startup processing has fully completed.
4. Observe whether the Outlook and TaskMaster UI stops repainting or accepting interaction while background and startup operations continue.

## Expected Behavior

There should be no perceivable latency during Outlook startup or the first email interactions after launch. Startup coordination, data loading, and selection-driven updates should use `async`/`await`, keep UI-thread work minimal, and leave the Outlook window responsive and repainting throughout.

## Actual Behavior

The UI still locks up for an extended period during startup and when clicking emails during the initial load window. The UI does not visibly update while operations continue, which indicates that too much work is still being performed on the UI thread or resumed there too aggressively.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: No new log snippet is attached in this follow-up entry; existing startup timing context is already documented in issues `#141` and `#139`, and additional targeted instrumentation may still be required to isolate the remaining root cause.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

This is a follow-up to unresolved issue `#141`, informed by the startup timing instrumentation work in issue `#139`. The remaining problem appears to be that startup and early email-selection flows still perform too much work on the UI thread, or resume to it too often, so Outlook cannot repaint or reflect progress while the operations continue.

Likely affected areas to inspect include `TaskMaster/AppGlobals/ApplicationGlobals.cs`, `TaskMaster/AppGlobals/AppEvents.cs`, `TaskMaster/AppGlobals/AppToDoObjects.cs`, `TaskMaster/AppGlobals/AppAutoFileObjects.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Helper Classes/ConversationResolver.cs`, and shared `UtilitiesCS` data-loading helpers.

Related docs:
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/v1/issue.md`
- `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md`

## Proposed Fix / Validation Ideas

- [ ] Add targeted instrumentation for startup phases and email-selection/update paths so the remaining UI-bound segments can be measured separately
- [ ] Add or extend regression and unit coverage around startup coordination, initial selection handling, and shared data-loading helpers that can safely move off the UI thread
- [ ] Manually verify Outlook responsiveness during startup and first email clicks, confirming there is no perceivable latency and that the UI continues repainting while work completes

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch