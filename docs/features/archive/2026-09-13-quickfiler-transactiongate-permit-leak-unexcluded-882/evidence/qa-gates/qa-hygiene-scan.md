# QA Hygiene Scan (P4-T22)

Timestamp: 2026-09-29T09-20
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $t = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); $h = [regex]::Escape($env:COMPUTERNAME); $r = [regex]::Escape((Get-Location).Path); $b = [string][char]92; $s = [string][char]47; $shape = "[A-Za-z]:[" + $b + $b + $s + "]Users[" + $b + $b + $s + "]"; $bash = $s + "c" + $s + "Users" + $s; $files = Get-ChildItem -Recurse -File -Path docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882; $pats = @(("(?i)" + $t), ("(?i)" + $h), ("(?i)" + $r), $shape, $bash); "PATTERN-COUNT=" + $pats.Count; $i = 0; foreach ($p in $pats) { $i = $i + 1; $hits = @($files | Select-String -Pattern $p); "PATTERN-" + $i + "-HITS=" + $hits.Count; foreach ($x in $hits) { "PATTERN-" + $i + "-FILE=" + $x.Filename + ":" + $x.LineNumber } }; "POSITIVE-CONTROL=" + @(Select-String -LiteralPath coverage/test-results/mstest-coverage-run.trx -Pattern ("(?i)" + $t)).Count; "POSITIVE-CONTROL-SHAPE=" + @(Select-String -LiteralPath coverage/test-results/mstest-coverage-run.trx -Pattern $shape).Count; "POSITIVE-CONTROL-HOST=" + @(Select-String -LiteralPath coverage/test-results/mstest-coverage-run.trx -Pattern ("(?i)" + $h)).Count; "POSITIVE-CONTROL-ROOT=" + @(Select-String -LiteralPath coverage/test-results/mstest-coverage-run.trx -Pattern ("(?i)" + $r)).Count; "FILES-SCANNED=" + @($files).Count'
EXIT_CODE: 0
Output Summary:
- PATTERN-COUNT=5
- PATTERN-1-HITS=0 (account token)
- PATTERN-2-HITS=0 (host name)
- PATTERN-3-HITS=0 (worktree root)
- PATTERN-4-HITS=0 (drive-letter Users-folder shape, either separator)
- PATTERN-5-HITS=0 (Git-Bash Users-folder form)
- POSITIVE-CONTROL=2941 (greater than 0)
- POSITIVE-CONTROL-SHAPE=2939 (greater than 0)
- POSITIVE-CONTROL-HOST=1472 and POSITIVE-CONTROL-ROOT=2939 (supplementary controls added so that patterns 2 and 3 are also shown able to hit)
- FILES-SCANNED=42 (the whole feature folder, including this plan file and spec.md, as the folder stood at scan time)
- No hit in any file, so no redaction was needed and no BEFORE-HITS/AFTER-HITS pair applies. No hit lies outside the Write Set.
- No XML-family file exists in the folder; no parse check is claimed.

Deviation (recorded, not concealed): the plan's literal payload builds the pattern list as `@("(?i)" + $t, "(?i)" + $h, "(?i)" + $r, $shape, $bash)`. In PowerShell the comma operator binds more tightly than `+`, so that expression evaluates to one string concatenation rather than five patterns. The first run of the literal payload (Timestamp 2026-09-29T09-20, exit 0) printed only `PATTERN-1-HITS=0` followed by `POSITIVE-CONTROL=2941` and `POSITIVE-CONTROL-SHAPE=2939`; a probe of the same expression returned an array count of 1. That run therefore did not test patterns 2 to 5. The payload above adds parentheses around each element, which preserves the plan's five patterns and its quoting rule, and adds a `PATTERN-COUNT=` line that shows five patterns were evaluated. The results above come from the corrected run.
