# Remediation Cycle 1 Drive-Rooted Scan Final (Issue 930)

Timestamp: 2026-09-29T10-22

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath WORKTREE-ROOT; $bs = [string][char]92; $colon = [string][char]58; $pat = $bs + "b[A-Za-z]" + $colon + "[" + $bs + $bs + "/]" + $bs + "S"; $root = "docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930"; $base = (Resolve-Path -LiteralPath $root).Path; $files = @(Get-ChildItem -Recurse -File -LiteralPath $root); "SCAN_FILES=$($files.Count)"; $hits = @($files | Select-String -Pattern $pat); "HITS_TOTAL=$($hits.Count)"; foreach ($h in $hits) { "HIT FILE=$([System.IO.Path]::GetRelativePath($base, $h.Path)) LINE=$($h.LineNumber)" }; $pos1 = "Z" + $colon + $bs + "Synthetic"; $pos2 = "Y" + $colon + "/Synthetic"; $neg = "https" + $colon + "//example.invalid/x"; $m1 = [bool]($pos1 -match $pat); $m2 = [bool]($pos2 -match $pat); $m3 = [bool]($neg -match $pat); "SYNTHETIC_BACKSLASH_MATCH=$m1"; "SYNTHETIC_SLASH_MATCH=$m2"; "SYNTHETIC_URL_MATCH=$m3"; $ctl = @(Select-String -LiteralPath coverage/930-final-coverage.log -Pattern $pat); "CONTROL_LOG_HITS=$($ctl.Count)"; $issue = Join-Path $base "issue.md"; $url = @(Select-String -LiteralPath $issue -SimpleMatch ("https" + $colon + "//")); "ISSUE_URL_LINES=$($url.Count)"'

EXIT_CODE: 0

Output Summary:

- SCAN_FILES=63 (at least BASELINE-SCAN-FILES 53 plus 10)
- HITS_TOTAL=0
- SYNTHETIC_BACKSLASH_MATCH=True
- SYNTHETIC_SLASH_MATCH=True
- SYNTHETIC_URL_MATCH=False
- CONTROL_LOG_HITS=26 (at least 1; the pattern still matches real data, so the zero hit count is not vacuous)
- ISSUE_URL_LINES=1

The seven baseline hit rows (r1-drive-scan-baseline.md) no longer appear.
