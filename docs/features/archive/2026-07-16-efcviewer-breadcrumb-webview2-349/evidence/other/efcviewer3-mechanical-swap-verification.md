# EfcViewer3 Mechanical-Swap Verification (P7-T2)

Timestamp: 2026-07-18T11-30

Search commands (run at the worktree root, ripgrep/grep over `QuickFiler/`):

1. `grep -rn "new EfcViewer3|EfcViewer3(" QuickFiler/ --include=*.cs` (excluding the two EfcViewer3 files themselves)
   Result: NO matches — zero runtime construction sites of `EfcViewer3` anywhere in `QuickFiler/`.
2. `grep -rln "EfcViewer3" QuickFiler/ --include=*.cs`
   Result: only `QuickFiler/Viewers/EfcViewer3.cs` and `QuickFiler/Viewers/EfcViewer3.Designer.cs` reference the type.
3. `grep -n "FolderListBox" QuickFiler/Viewers/EfcViewer3.cs`
   Result: NO matches — the code-behind never touches the folder-list control.
4. `grep -rn "EfcViewer3" QuickFiler/Controllers/ "QuickFiler/Helper Classes/" --include=*.cs`
   Result: NO matches — zero controller/event wiring of EfcViewer3 or its folder-list control.
5. `grep -n "new EfcViewer" "QuickFiler/Helper Classes/EfcViewerQueue.cs"`
   Result: line 83 `return new EfcViewer();` — the sole runtime instantiation of an Efc form viewer is the concrete `EfcViewer`, confirming EfcViewer3 is dead code.

Post-swap state of `EfcViewer3.Designer.cs` (P7-T1):
- `FolderListBox` is now `Microsoft.Web.WebView2.WinForms.WebView2` in the same TableLayoutPanel cell (`Tlp.Controls.Add(FolderListBox, 2, 5)` unchanged, `SetColumnSpan(..., 14)`, `Dock = Fill`).
- Both `OLVColumn` declarations (`olvColumnFolder` width 1600, `olvColumnPercent` width 300) deleted.
- Zero `BrightIdeasSoftware` folder-list references remain in either EfcViewer3 file (search 3 above plus `grep -n "BrightIdeasSoftware|olvColumn"` over both files: no matches).
- NO event subscriptions or handlers were added in either EfcViewer3 file; `EfcViewer3.cs` is byte-identical to its pre-task state (`git diff --stat -- QuickFiler/Viewers/EfcViewer3.cs` is empty; no compile-fix edits were required).

Conclusion: the no-behavioral-wiring invariant holds — the swap is Designer-only and EfcViewer3 remains dead code.
