# P4-T3 Pre-rewrite eol listing

Timestamp: 2026-09-29T19-59
Command: pwsh -NoProfile -Command '$d = Join-Path $env:TEMP "hygiene-927"; git ls-files --eol -- . ":!.claude/" | Set-Content -LiteralPath (Join-Path $d "eol-before.txt") -Encoding utf8; $e = Get-Content -LiteralPath (Join-Path $d "eol-before.txt"); "EOL-LINES=" + $e.Count; "EOL-BINARY=" + @($e | Select-String -SimpleMatch "i/-text").Count; "EOL-CRLF=" + @($e | Select-String -SimpleMatch "w/crlf").Count; "EOL-LF=" + @($e | Select-String -SimpleMatch "w/lf").Count; "EOL-MIXED=" + @($e | Select-String -SimpleMatch "w/mixed").Count'
EXIT_CODE: 0
Output Summary:
- EOL-LINES=14850
- EOL-BINARY=44
- EOL-CRLF=14754
- EOL-LF=39
- EOL-MIXED=0
- The listing is held at SCRATCH\eol-before.txt (outside the repository, C5) for P4-T6 and P4-T9.
