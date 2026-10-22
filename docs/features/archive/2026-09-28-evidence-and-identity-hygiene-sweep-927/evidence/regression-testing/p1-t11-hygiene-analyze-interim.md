# P1-T11 Interim format and analyze pass over the six hygiene files

Timestamp: 2026-09-29T17-40
Command: pwsh -NoProfile -Command 'Import-Module PSScriptAnalyzer; $d = @(Invoke-ScriptAnalyzer -Path "scripts/hygiene" -Recurse) + @(Invoke-ScriptAnalyzer -Path "tests/scripts/hygiene" -Recurse); "DIAGNOSTICS=" + $d.Count; foreach ($x in $d) { "RULE| " + $x.RuleName + " | " + $x.Severity + " | " + $x.ScriptName + " | line " + $x.Line }'
EXIT_CODE: 0
Output Summary:
- Format pass (iteration 1): pwsh -NoProfile -Command 'Import-Module PSScriptAnalyzer; $n = 0; $six = @(Get-ChildItem -LiteralPath "scripts/hygiene", "tests/scripts/hygiene" -Filter "*.ps1" -File); "FILES=" + $six.Count; foreach ($f in $six) { ... Invoke-Formatter -ScriptDefinition $t; if ($o -cne $t) { [System.IO.File]::WriteAllText(...); $n++ } }; "REWRITTEN=" + $n' printed FILES=6 and REWRITTEN=0. No further iteration was needed; the ordered REWRITTEN figures are: 0.
- Analyzer: DIAGNOSTICS=0 (no RULE lines).
- Byte-order mark after the pass: the first three bytes of each of the six files are EF BB BF (Test-RepositoryHygiene.ps1, Test-RepositoryHygiene.Rules.ps1, Test-RepositoryHygiene.Git.ps1, Test-RepositoryHygiene.Tests.ps1, Test-RepositoryHygiene.Rules.Tests.ps1, Test-RepositoryHygiene.Git.Tests.ps1).
- Execution note: both payloads ran with a prefix that sets the location and [Environment]::CurrentDirectory to <repo-root> (the item worktree).
