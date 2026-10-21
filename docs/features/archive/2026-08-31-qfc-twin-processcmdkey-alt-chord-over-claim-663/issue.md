# Bug: qfc-twin-processcmdkey-alt-chord-over-claim

- Work Mode: full-bug
- Issue: #663
- State: OPEN
- Label: bug
- Owner: drmoisan
- Last Updated: 2026-08-31T20-16

## Summary
`QfcFormViewer.ProcessCmdKey` claims every Alt chord, so Alt-key menu mnemonics such as Alt+F and Alt+M are swallowed and never reach `base.ProcessCmdKey`. This is the same defect fixed for the EFC surface as issue #467; the QFC twin was deliberately left untouched by feature #464 and remains live.

## Environment
- OS/version: Windows 11 Pro 10.0.26200
- Framework: .NET Framework 4.8 / VSTO Outlook add-in
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: `QuickFiler.Test`

## Steps to Reproduce
1. Open the QuickFiler form surface (`QfcFormViewer`).
2. Press Alt+F (or Alt+M) intending to open the corresponding menu.
3. Observe that the menu does not open.

## Expected Behavior
An Alt chord that corresponds to a menu mnemonic falls through to `base.ProcessCmdKey` so the menu opens. Only a bare Alt chord that the keyboard handler genuinely claims should be intercepted.

## Actual Behavior
`ProcessCmdKey` returns `true` for the whole Alt chord class, so the mnemonic is consumed and the menu never opens.

## Logs / Screenshots
- [ ] Attached minimal logs or screenshot
- Snippet: `QuickFiler/Viewers/QfcFormViewer.cs:56-73` and `QuickFiler/Controllers/QfcFormKeyHandler.cs:18`. Line numbers are pre-change locators from feature #464's spec and must be re-resolved by member name before use.

## Impact / Severity
Live, user-facing: menu mnemonics do not work on the QuickFiler form surface. Severity is moderate - the menus remain reachable by mouse - but it is a real interaction defect, and the EFC twin was judged worth fixing under issue #467.

## Source
From: docs/features/potential/2026-08-27-qfc-twin-processcmdkey-alt-chord-over-claim.md

## Promotion Note
The potential entry named under Source is not present on `origin/main` in either `docs/features/potential/` or `docs/features/potential/promoted/`. Issue #663 was already promoted before this preparation run, so the potential-to-issue promotion tool was not re-run; re-running it would have opened a duplicate issue. The active-feature-folder MCP tool therefore produced `spec.md` and the plan template but no `issue.md`, because it had no promoted source file to copy. This file reproduces the GitHub issue body verbatim and carries the authoritative `- Work Mode: full-bug` marker.

Because the work mode is `full-bug`, `spec.md` is the authoritative acceptance-criteria source for this feature. This file is context, not the AC source.
