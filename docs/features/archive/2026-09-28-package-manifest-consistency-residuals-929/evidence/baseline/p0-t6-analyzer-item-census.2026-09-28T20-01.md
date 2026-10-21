# P0-T6 — Analyzer Include item census

Timestamp: 2026-09-30T09-17
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $items = @(); foreach ($p in @(git ls-files -- "*.csproj")) { $dir = Split-Path -Parent $p; foreach ($line in (Get-Content -LiteralPath $p)) { if ($line -match "Analyzer Include=""([^""]+)""") { $rel = $Matches[1]; $items += [pscustomobject]@{ Project = $p; Item = $rel; Resolves = (Test-Path -LiteralPath (Join-Path $dir $rel)) } } } }; "ANALYZER_ITEMS=$($items.Count) FILES=$(@($items | Select-Object -ExpandProperty Project -Unique).Count) UNRESOLVED=$(@($items | Where-Object { -not $_.Resolves }).Count)"; "SKEW_235=" + @(git grep -n "Meziantou.Analyzer.3.0.235" -- "*.csproj").Count; $items | Where-Object { -not $_.Resolves } | ForEach-Object { $_.Project + " " + $_.Item }'
EXIT_CODE: 0
Output Summary:
- ANALYZER_ITEMS=162 FILES=17 UNRESOLVED=0
- SKEW_235=0
- Unresolved (project, item) listing: empty

ANALYZER-ITEM-STATE: aligned

No back-fill was performed and none is permitted by this plan; the AC7 back-fill clause is not exercised.

The packages tree is ignored at .gitignore line 197 (`**/[Pp]ackages/*`), and the restore of P0-T5 is the only step that populated it (PACKAGE_DIRS_BEFORE=0, PACKAGE_DIRS_AFTER=172). Decision D8 governs.
