# quickfiler-webview2-incognito-arg-en-dash (Issue #463)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-webview2-incognito-arg-en-dash/ (Issue #463)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #463
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/463
- Last Updated: 2026-08-08
## Summary

The WebView2 additional-browser-arguments string uses a U+2013 EN DASH instead of the two ASCII
hyphens a Chromium switch requires, so the intended incognito mode is never applied. The defect is
present at three call sites across two controllers.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- UI path: WebView2 environment construction in the QuickFiler item controllers
- Data source or fixture: n/a

## Steps to Reproduce

1. Open an item viewer that initializes a WebView2 environment.
2. Inspect the resulting `CoreWebView2Environment` browser arguments, or observe that browsing data
   persists across sessions.
3. Observe that incognito mode is not in effect.

## Expected Behavior

The WebView2 environment is created with a valid Chromium `--incognito` switch and no browsing data
persists.

## Actual Behavior

The argument string is `"–incognito "`, whose first character is **U+2013 EN DASH**, not two ASCII
hyphen-minus characters. Chromium does not recognise the token, so the switch is silently ignored.

Affected sites:

- `QuickFiler/Controllers/EfcItemController.cs:184`
- `QuickFiler/Controllers/EfcItemController.cs:217`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:52`

The commented-out alternative directly above two of these sites (`EfcItemController.cs:182`, `:215`)
correctly uses ASCII in `"--disk-cache-size=1 "`, which is what makes the substitution visible on a
close read.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Browsing data that was intended to be discarded is retained. There is no crash and no visible error,
which is why the defect has persisted.

## Suspected Cause / Notes

Almost certainly an editor or documentation copy-paste that applied "smart dashes" autocorrection,
converting `--` to a single en dash. WebView2 ignores unrecognised switches silently, so there is no
runtime signal.

Because the same string appears in both the EFC and QFC item controllers, a single fix should cover
all three sites, and a guard test asserting the arguments contain only ASCII would prevent recurrence.

Discovered during preparation of issue #452 (epic #136) per-file coverage research. Out of scope there
under that feature's no-behavior-change constraint.

## Proposed Fix / Validation Ideas

- [ ] Replace the en dash with `--` at all three sites
- [ ] Add a test asserting the additional-browser-arguments string is pure ASCII and starts with `--`
- [ ] Consider a repository-wide check for non-ASCII dashes inside string literals passed to process/browser arguments
- [ ] Manual verification: confirm incognito behavior after the fix

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
