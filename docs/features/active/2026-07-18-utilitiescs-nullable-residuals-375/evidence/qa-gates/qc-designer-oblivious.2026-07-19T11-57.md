# QC Designer Files Oblivious (P12-T8) — AC3

Timestamp: 2026-07-19T11-57

Command: per Designer file, `grep -c "#nullable" <file>` and `git status --short <file>`.

EXIT_CODE: 0

Output Summary: The 6 `*.Designer.cs` files under `OlFolderTools` carry NO `#nullable` pragma and are
unmodified on this branch:

| Designer file | #nullable pragma count | git status |
|---|---|---|
| FilterOlFolders/FilterOlFoldersViewer.Designer.cs | 0 | unmodified |
| FilterOlFolders/FolderInfoViewer.Designer.cs | 0 | unmodified |
| FilterOlFolders/OSBrowser.Designer.cs | 0 | unmodified |
| FilterOlFolders/OSFolder.Designer.cs | 0 | unmodified |
| FolderRemap/FolderRemapViewer.Designer.cs | 0 | unmodified |
| FolderRemap/FolderSelector.Designer.cs | 0 | unmodified |

All six remain in an oblivious nullable context and were not cross-blocked: the isolated pragma-only
gate (P12-T3 section B) reached zero CS86xx with the hand-written partial halves opted in while these
Designer halves stayed oblivious. AC3 satisfied.
