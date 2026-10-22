# Remediation Cycle 1 Substitution 2 (Issue 930)

Timestamp: 2026-09-29T10-18

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath WORKTREE-ROOT; $bs = [string][char]92; $colon = [string][char]58; $sep = "[" + $bs + $bs + "/]"; $notsep = "[^" + $bs + $bs + "/" + $bs + "r" + $bs + "n]+"; $mask = $bs + "b[A-Za-z]" + $colon + $sep + "Program Files" + $sep + "Microsoft Visual Studio" + $sep + $notsep + $sep + $notsep + "(?=" + $sep + "Common7)"; $general = $bs + "b[A-Za-z]" + $colon + $sep + $bs + "S"; $ph = "VS-INSTALL-ROOT"; $open = "- [ ] AC7:"; $done = "- [x] AC7:"; $root = "docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930"; $dir = (Resolve-Path -LiteralPath $root).Path; $sha = [System.Security.Cryptography.SHA256]::Create(); foreach ($n in @("evidence/baseline/baseline-04-mstest-coverage.md", "evidence/qa-gates/final-06-mstest-coverage.md", "code-review.2026-09-29T00-45.md", "policy-audit.2026-09-29T00-45.md", "feature-audit.2026-09-29T00-45.md", "issue.md")) { $t = [System.IO.File]::ReadAllText((Join-Path $dir $n)); if ($n -eq "issue.md") { $m = $t.Replace($open, $done); $occ = ([regex]::Matches($t, [regex]::Escape($open))).Count } else { $m = [regex]::Replace($t, $mask, $ph); $occ = ([regex]::Matches($t, $mask)).Count }; $mh = [System.BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($m))).Replace("-", ""); $rh = [System.BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($t))).Replace("-", ""); $lines = $t.Split([char]10).Count; $pc = ([regex]::Matches($t, [regex]::Escape($ph))).Count; $dm = ([regex]::Matches($t, $general)).Count; $chg = ($mh -ne $rh); "FILE_STATE NAME=$n LINES=$lines OCC=$occ PLACEHOLDERS=$pc DRIVE_MATCHES=$dm MASK_CHANGES_TEXT=$chg MASKED_TEXTHASH=$mh RAW_TEXTHASH=$rh" }'

EXIT_CODE: 0

Output Summary:

The install-root prefix in evidence/qa-gates/final-06-mstest-coverage.md (line 12, one occurrence) was replaced with the placeholder VS-INSTALL-ROOT. Row printed for this file:

FILE_STATE NAME=evidence/qa-gates/final-06-mstest-coverage.md LINES=230 OCC=0 PLACEHOLDERS=1 DRIVE_MATCHES=0 MASK_CHANGES_TEXT=False MASKED_TEXTHASH=5ABFE0F8F342D3296ECD14C42641AA67C853ECB15475FA887235E2D24BD21A9A RAW_TEXTHASH=5ABFE0F8F342D3296ECD14C42641AA67C853ECB15475FA887235E2D24BD21A9A

Check against r1-file-state-baseline.md: RAW_TEXTHASH equals EXPECTED-TEXTHASH-2 (matched); PLACEHOLDERS=1; OCC=0; DRIVE_MATCHES=0; LINES=230 equals the baseline value.
