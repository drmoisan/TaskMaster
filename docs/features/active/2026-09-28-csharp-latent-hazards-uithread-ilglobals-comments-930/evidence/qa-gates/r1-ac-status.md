# Remediation Cycle 1 Acceptance-Criteria Status (Issue 930)

Timestamp: 2026-09-29T10-26

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath WORKTREE-ROOT; $bs = [string][char]92; $colon = [string][char]58; $sep = "[" + $bs + $bs + "/]"; $notsep = "[^" + $bs + $bs + "/" + $bs + "r" + $bs + "n]+"; $mask = $bs + "b[A-Za-z]" + $colon + $sep + "Program Files" + $sep + "Microsoft Visual Studio" + $sep + $notsep + $sep + $notsep + "(?=" + $sep + "Common7)"; $general = $bs + "b[A-Za-z]" + $colon + $sep + $bs + "S"; $ph = "VS-INSTALL-ROOT"; $open = "- [ ] AC7:"; $done = "- [x] AC7:"; $root = "docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930"; $dir = (Resolve-Path -LiteralPath $root).Path; $sha = [System.Security.Cryptography.SHA256]::Create(); foreach ($n in @("evidence/baseline/baseline-04-mstest-coverage.md", "evidence/qa-gates/final-06-mstest-coverage.md", "code-review.2026-09-29T00-45.md", "policy-audit.2026-09-29T00-45.md", "feature-audit.2026-09-29T00-45.md", "issue.md")) { $t = [System.IO.File]::ReadAllText((Join-Path $dir $n)); if ($n -eq "issue.md") { $m = $t.Replace($open, $done); $occ = ([regex]::Matches($t, [regex]::Escape($open))).Count } else { $m = [regex]::Replace($t, $mask, $ph); $occ = ([regex]::Matches($t, $mask)).Count }; $mh = [System.BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($m))).Replace("-", ""); $rh = [System.BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($t))).Replace("-", ""); $lines = $t.Split([char]10).Count; $pc = ([regex]::Matches($t, [regex]::Escape($ph))).Count; $dm = ([regex]::Matches($t, $general)).Count; $chg = ($mh -ne $rh); "FILE_STATE NAME=$n LINES=$lines OCC=$occ PLACEHOLDERS=$pc DRIVE_MATCHES=$dm MASK_CHANGES_TEXT=$chg MASKED_TEXTHASH=$mh RAW_TEXTHASH=$rh" }'

EXIT_CODE: 0

Output Summary:

Printed row for issue.md after the AC7 marker flip:

FILE_STATE NAME=issue.md LINES=79 OCC=0 PLACEHOLDERS=0 DRIVE_MATCHES=0 MASK_CHANGES_TEXT=False MASKED_TEXTHASH=311DEA8957445B751C418057B3C26002FC6B27D8316B0CDFDA4B7080913BC02B RAW_TEXTHASH=311DEA8957445B751C418057B3C26002FC6B27D8316B0CDFDA4B7080913BC02B

- RAW_TEXTHASH equals EXPECTED-TEXTHASH-6 from r1-file-state-baseline.md (only the AC7 marker changed); OCC=0.
- Grep tool count of the fixed string `- [x] AC` in issue.md: 7 lines.
- Grep tool count of the fixed string `- [ ] AC` in issue.md: 0 lines.
- Confirming conditions verified before the edit: r1-drive-scan-final.md HITS_TOTAL=0 with controls met; r1-sanitize-final.md five zero counts with four controls at least 1; r1-subst-1.md through r1-subst-5.md each show RAW_TEXTHASH equal to its expected value; r1-toolchain-exemption.md shows both zero counts.

AC7: MET

Cited artifacts: r1-drive-scan-final.md, r1-sanitize-final.md, r1-subst-1.md, r1-subst-2.md, r1-subst-3.md, r1-subst-4.md, r1-subst-5.md, r1-footprint.md.
