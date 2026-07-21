# Precondition — File Shape (P0-T2, P0-T3)

Timestamp: 2026-07-09T21-56

## P0-T2 — TagController.cs line count

Command: `wc -l Tags/TagController.cs`
EXIT_CODE: 0
Output Summary: `Tags/TagController.cs` is 877 lines, within the expected 870-885 range
(spec states 877). Precondition satisfied: file exceeds the 500-line limit and is the
subject of the split.

## P0-T3 — IForm interface shape

Command: `grep -n "Text|KeyDown|Controls|Close()|KeyPreview|ShowDialog" UtilitiesCS/Interfaces/IWinForm/IForm.cs`
EXIT_CODE: 0
Output Summary: `UtilitiesCS/Interfaces/IWinForm/IForm.cs` exposes `bool KeyPreview { get; set; }`
(line 28), `void Close()` (line 76), `DialogResult ShowDialog()` (line 82), and
`DialogResult ShowDialog(IWin32Window owner)` (line 83). It does NOT expose a `Text` property,
a `KeyDown` event, or a `Controls` member. Confirmed: `ITagViewer` must declare `Caption`,
`ViewKeyDown`, and the option-panel abstraction explicitly because `IForm` does not provide them.
