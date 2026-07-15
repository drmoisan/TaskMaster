# quickfiler-inline-image-cid-fix (Issue #326)

- Date captured: 2026-07-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-inline-image-cid-fix/ (Issue #326)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #326
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/326
- Last Updated: 2026-07-15
- Work Mode: full-bug

## Summary

In QuickFiler expanded mode, inline images referenced by `cid:` (Content-ID) do not render in the message body; they appear as broken or missing images. This is a child bug of the `folder-tree-percentage-ui` epic (manifest child 9004).

## Environment

- OS/version: Windows 11
- Python version: N/A (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: QuickFiler reading pane, expanded viewer mode
- Data source or fixture: An email whose HTML body references inline images by `cid:` (Content-ID), with the images present as attachments carrying a matching `Content-Id` / `PR_ATTACH_CONTENT_ID`

## Steps to Reproduce

1. Open QuickFiler and select a message whose HTML body embeds inline images via `<img src="cid:...">`.
2. Enlarge the pane to expanded viewer mode so the body region is visible.
3. Observe the message body rendered in the WebView2 control.

## Expected Behavior

Inline images referenced by `cid:` render in the message body, resolved against the matching attachment `Content-Id` / `PR_ATTACH_CONTENT_ID`, in both compact and expanded modes.

## Actual Behavior

`cid:` references do not resolve. Inline images appear broken or missing. Because compact and expanded modes use the identical render call and differ only in the on-screen size of the WebView2 control, the images are cropped or not visible in compact mode and become visibly broken only when the pane is enlarged in expanded mode.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: Body renders via a single `WebView2.NavigateToString(ItemHelper.Html)` call (`QuickFiler/Controllers/QfcItemController.EventWiring.cs` -> `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`). No `WebResourceRequested` handler or `SetVirtualHostNameToFolderMapping` mapping exists anywhere in the C# sources (verified by grep).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Root cause (verified against current code): the email body renders via a single `WebView2.NavigateToString(ItemHelper.Html)` call. No code resolves `cid:` references against attachment `Content-Id` / `PR_ATTACH_CONTENT_ID`; there is no `WebResourceRequested` handler or virtual host mapping. Expanded and compact modes use the identical rendering call, differing only in the WebView2 control's on-screen size (`TlpCellSnapShot.ApplyState`), so broken inline images are a missing-feature defect (`cid:` resolution never existed), not a mode-specific regression.

Files to inspect:
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs` (HTML body producer)
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs` (WebView2 init/wiring, `NavigateToString`)
- WebView2 setup/wiring for the item viewer

## Proposed Fix / Validation Ideas

- Add `cid:` reference resolution to the WebView2 body-rendering path so inline images render, for example via a `WebResourceRequested` handler or a virtual host mapping that resolves `cid:` references against attachment `Content-Id` / `PR_ATTACH_CONTENT_ID`.
- [ ] Unit coverage areas: `cid:` -> attachment resolution logic (host-neutral, injectable seam)
- [ ] Integration scenario to retest: expanded-mode body render with inline `cid:` images
- [ ] Manual verification notes: confirm compact-mode render call path is unchanged beyond the shared `cid:` resolution; do not touch the folder list, ComboBox, or scoring code

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
