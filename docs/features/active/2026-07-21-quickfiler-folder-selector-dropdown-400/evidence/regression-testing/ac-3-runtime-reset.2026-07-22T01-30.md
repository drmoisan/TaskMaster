# AC-3 runtime reset

Timestamp: 2026-07-22T01:30:11.6564554Z

Command: `$p='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md'; $lines=@(Get-Content -LiteralPath $p | Where-Object { $_ -match '^\s*- \[[ xX]\] AC-' }); $normalized=($lines -replace '^([ \t]*- )\[[ xX]\]( AC-)','$1[ ]$2') -join "`n"; $bytes=[Text.Encoding]::UTF8.GetBytes($normalized); $sha=[Security.Cryptography.SHA256]::Create(); 'NORMALIZED_AC_WORDING_SHA256=' + ([BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-','').ToLowerInvariant()); 'CHECKED=' + @($lines | Where-Object { $_ -match '^\s*- \[[xX]\]' }).Count; 'OPEN=' + @($lines | Where-Object { $_ -match '^\s*- \[ \]' }).Count`

EXIT_CODE: 0

Output Summary: Before and after the one-marker edit, the normalized 19-AC wording SHA-256 is `cd05236b2bf9b6966ff02f48d6b9f06c468af5e162f79da2d37277329699ad1c`. Before the edit the inventory was 6 supported and 13 open. After the edit the inventory is 5 supported and 14 open. All 19 AC texts and their order are byte-for-byte unchanged after checkbox normalization; every marker except AC-3 is unchanged.

Exact one-marker diff:

```diff
 - [x] AC-3: Button activation and `SetFolderDroppedDown(true)` open a native `ToolStripDropDown`/`ToolStripControlHost` popup over `ItemViewer` sibling controls; while open it remains owned by and above that `ItemViewer` and is never configured as a global/system-wide topmost window.
 + [ ] AC-3: Button activation and `SetFolderDroppedDown(true)` open a native `ToolStripDropDown`/`ToolStripControlHost` popup over `ItemViewer` sibling controls; while open it remains owned by and above that `ItemViewer` and is never configured as a global/system-wide topmost window.
```
