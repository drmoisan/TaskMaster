# breadcrumb-webview2-init-fails-resource-not-in-correct-state (Issue #792)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-webview2-init-fails-resource-not-in-correct-state/ (Issue #792)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #792
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/792
- Last Updated: 2026-09-06
## Summary

The breadcrumb `CoreWebView2` initialization fails intermittently with HRESULT 0x8007139F ("The group or resource is not in the correct state to perform the requested operation"), logged by both `WebView2BreadcrumbHost` and `EfcFormController`. The failure is logged and swallowed; the session continues with a breadcrumb host that never initialized, and a later `BreadcrumbUiDispatcher` dispatch fails in the same session. Because the #677 keyboard-lock mechanism is WebView2 focus retention, a half-initialized WebView2 is a plausible contributor to the sporadic keyboard lock, but that link is unconfirmed.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: QuickFiler launched from the ribbon (High Confidence button); add-in loaded from `TaskMaster\bin\Debug` built 2026-09-06 08:51 from `7c8ac9ae`
- Data source or fixture: live Outlook Inbox view

## Steps to Reproduce

1. Launch QuickFiler from the ribbon several times in one Outlook session.
2. Inspect `TaskMaster\bin\Debug\logs\debug_<date>.log` for `Breadcrumb CoreWebView2 initialization failed`.
3. Observe that the failure occurs on some launches (2 of 6 today: 08:55:22 and 10:06:51) and not others.

Not reproducible on demand.

## Expected Behavior

WebView2 initialization either succeeds, or fails with a clear surfaced error and a defined fallback state that cannot retain keyboard focus. A failed initialization should be retried or the host disposed, not left half-constructed.

## Actual Behavior

Two ERROR lines per occurrence (`WebView2BreadcrumbHost - Breadcrumb CoreWebView2 initialization failed: ... (HRESULT: 0x8007139F)` and `EfcFormController - Breadcrumb WebView2 initialization failed: ...`), then normal operation continues. A `BreadcrumbUiDispatcher - Breadcrumb UI dispatch failed.` error followed at 09:01:56 in the same session.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet (`debug_2026-09-06.log`):

```
2026-09-06 08:55:22,227 [VSTA_Main] ERROR QuickFiler.Viewers.WebView2BreadcrumbHost - Breadcrumb CoreWebView2 initialization failed: ... (HRESULT: 0x8007139F)
2026-09-06 08:55:22,286 [VSTA_Main] ERROR QuickFiler.Controllers.EfcFormController - Breadcrumb WebView2 initialization failed: ... (HRESULT: 0x8007139F)
2026-09-06 09:01:56,237 [VSTA_Main] ERROR QuickFiler.Viewers.BreadcrumbUiDispatcher - Breadcrumb UI dispatch failed.
2026-09-06 10:06:51,594 [VSTA_Main] ERROR QuickFiler.Viewers.WebView2BreadcrumbHost - Breadcrumb CoreWebView2 initialization failed: ... (HRESULT: 0x8007139F)
2026-09-06 10:06:51,661 [VSTA_Main] ERROR QuickFiler.Controllers.EfcFormController - Breadcrumb WebView2 initialization failed: ... (HRESULT: 0x8007139F)
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: the breadcrumb folder selector is unavailable on affected launches, and the half-initialized control is a candidate contributor to the sporadic Outlook keyboard lock tracked under the sibling QuickFiler Cancel-teardown issue filed the same day.

## Suspected Cause / Notes

- 0x8007139F (`ERROR_INVALID_STATE`) from `CoreWebView2Environment`/`EnsureCoreWebView2Async` typically indicates the control was initialized while its handle or parent was not yet in a valid state, or a second initialization was attempted against a control already mid-initialization or disposed. Both `WebView2BreadcrumbHost` and `EfcFormController` log the same failure, suggesting the initialization is attempted from two paths.
- Files to inspect: `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `QuickFiler/Controllers/EfcFormController.cs` (breadcrumb initialization), `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, and the pooled-viewer handler-retention history in `docs/features/potential/promoted/2026-08-07-webview2breadcrumbhost-handler-retention-pooled-viewer.md`.
- Related: #677 (keyboard hook leak; WebView2 focus retention mechanism), the sibling potential entry `2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects.md`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: initialization state machine of `WebView2BreadcrumbHost` (single initialization, guard against re-entry, disposed-host guard, failure leaves a defined non-focusable state).
- [ ] Integration scenario to retest: repeated QuickFiler launches in one session; confirm no `0x8007139F` and that a failed initialization cannot retain focus.
- [ ] Manual verification notes: live-Outlook log review across several launches.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
