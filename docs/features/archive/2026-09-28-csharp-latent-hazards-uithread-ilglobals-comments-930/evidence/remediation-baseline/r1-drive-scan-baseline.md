# Remediation Cycle 1 Drive-Rooted Scan Baseline (Issue 930)

Timestamp: 2026-09-29T10-12

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath WORKTREE-ROOT; $bs = [string][char]92; $colon = [string][char]58; $pat = $bs + "b[A-Za-z]" + $colon + "[" + $bs + $bs + "/]" + $bs + "S"; $root = "docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930"; $base = (Resolve-Path -LiteralPath $root).Path; $files = @(Get-ChildItem -Recurse -File -LiteralPath $root); "SCAN_FILES=$($files.Count)"; $hits = @($files | Select-String -Pattern $pat); "HITS_TOTAL=$($hits.Count)"; foreach ($h in $hits) { "HIT FILE=$([System.IO.Path]::GetRelativePath($base, $h.Path)) LINE=$($h.LineNumber)" }; $pos1 = "Z" + $colon + $bs + "Synthetic"; $pos2 = "Y" + $colon + "/Synthetic"; $neg = "https" + $colon + "//example.invalid/x"; $m1 = [bool]($pos1 -match $pat); $m2 = [bool]($pos2 -match $pat); $m3 = [bool]($neg -match $pat); "SYNTHETIC_BACKSLASH_MATCH=$m1"; "SYNTHETIC_SLASH_MATCH=$m2"; "SYNTHETIC_URL_MATCH=$m3"; $ctl = @(Select-String -LiteralPath coverage/930-final-coverage.log -Pattern $pat); "CONTROL_LOG_HITS=$($ctl.Count)"; $issue = Join-Path $base "issue.md"; $url = @(Select-String -LiteralPath $issue -SimpleMatch ("https" + $colon + "//")); "ISSUE_URL_LINES=$($url.Count)"'

EXIT_CODE: 0

Output Summary:

SCAN_FILES: 53
HITS_TOTAL: 7

Hit rows (matched text is not transcribed):

- FILE=evidence\baseline\baseline-04-mstest-coverage.md LINE=11 MATCH=DRIVE-ROOTED-PATH
- FILE=evidence\qa-gates\final-06-mstest-coverage.md LINE=12 MATCH=DRIVE-ROOTED-PATH
- FILE=code-review.2026-09-29T00-45.md LINE=18 MATCH=DRIVE-ROOTED-PATH
- FILE=policy-audit.2026-09-29T00-45.md LINE=182 MATCH=DRIVE-ROOTED-PATH
- FILE=policy-audit.2026-09-29T00-45.md LINE=188 MATCH=DRIVE-ROOTED-PATH
- FILE=feature-audit.2026-09-29T00-45.md LINE=39 MATCH=DRIVE-ROOTED-PATH
- FILE=feature-audit.2026-09-29T00-45.md LINE=53 MATCH=DRIVE-ROOTED-PATH

Controls:

- SYNTHETIC_BACKSLASH_MATCH=True
- SYNTHETIC_SLASH_MATCH=True
- SYNTHETIC_URL_MATCH=False
- CONTROL_LOG_HITS=26 (at least 1; the raw final coverage log carries drive-rooted paths, so the pattern matches real data)
- ISSUE_URL_LINES=1 (issue.md carries a URL line that does not match)

BASELINE-SCAN-FILES: 53
