# breadcrumb-bridge-keyboard-navigation-defects (Issue #737)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-bridge-keyboard-navigation-defects/ (Issue #737)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #737
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/737
- Last Updated: 2026-09-02
## Summary

Three consolidated findings from a code-review sweep, all in the breadcrumb WebView2 keyboard-bridge path: the inline JavaScript keydown map embedded in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` is missing an Enter binding entirely and has no viewport/scroll logic on arrow-key navigation, and a router test masks a gap in left-arrow collapse coverage — grouped as one issue since all three touch the same keyboard-routing path from WebView2 through to `FolderBreadcrumbBridgeRouter`.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C#/.NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2, inline JavaScript bridge
- Command/flags used: n/a — findings are from code review
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable in the usual sense — each finding below is a static code-review finding. See "Actual Behavior."

## Expected Behavior

Each finding's expected behavior is stated inline below.

## Actual Behavior

**1. Arrow-key navigation has no viewport/scroll logic (Source: #640).** The inline JS in `BreadcrumbDocumentAssets.cs` registers a `keydown` listener mapping `ArrowUp/Down/Left/Right` to `arrowKey` postMessage calls (confirmed present, `map = { ArrowLeft: 'Left', ArrowRight: 'Right', ArrowUp: 'Up', ArrowDown: 'Down' }`), but the inbound `render` handler that applies the resulting selection change has no logic to scroll the newly-selected row into view. On a long suggestion list, keyboard navigation can move selection off-screen with no visual feedback.

**2. No Enter-key binding at all (Source: #641).** The same keydown map has no `Enter` entry, so pressing Enter while a valid folder suggestion is highlighted in the WebView2-hosted breadcrumb selector does nothing — confirmed on `origin/main`. This is distinct from `EfcFormController`'s own `Keys.Return` binding at the `"Collection"` keyboard scope (which invokes `ActionOkAsync` when the item list itself has focus) — that binding does not receive the keystroke while the WebView2 breadcrumb popup owns keyboard focus, so a user navigating suggestions purely by keyboard has no way to commit a selection with Enter.

**3. Router test discards two Arrange-phase results without assertion (Source: #693).** `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs:368`, test `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`: the test calls `ArrowAsync(router, "left")` twice during Arrange with the results discarded, then only asserts on a third call's result. The first two calls' outcomes are unverified, so if either of them actually triggers an unexpected collapse (defeating the "nothing to collapse" premise the test name asserts), the test would not catch it.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations inline above; each finding was verified directly against `origin/main` during this consolidation pass (2026-09-02).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: finding 2 is a real usability gap (keyboard-only users cannot commit a highlighted suggestion), finding 1 is a visibility/UX gap, and finding 3 is a test-coverage gap rather than a live defect. None crash or corrupt data.

## Suspected Cause / Notes

Findings 1 and 2 share one root cause: the inline JS keydown map in `BreadcrumbDocumentAssets.cs` was built out incrementally (arrow keys first) and Enter was never added, and the `render` handler that consumes arrow-key selection changes was never extended with scroll-into-view logic. Finding 3 is unrelated in mechanism but shares the same file family (the breadcrumb router test suite) and was found during the same review pass.

## Proposed Fix / Validation Ideas

- [ ] Add an `Enter` entry to the JS keydown map, posting a `select`/`activate`-style message the C# side already has infrastructure to handle (mirroring the existing `arrowKey`/`renderedChildActivate` message shapes)
- [ ] Add scroll-into-view logic to the `render` handler's arrow-key selection-change path
- [ ] Assert on (or explicitly discard-with-comment) the two currently-unverified `ArrowAsync` results in `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
