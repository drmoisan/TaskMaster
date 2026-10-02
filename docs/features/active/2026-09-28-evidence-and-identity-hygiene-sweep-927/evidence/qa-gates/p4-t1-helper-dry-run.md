# P4-T1 Redaction helper authored and proved by a dry run

Timestamp: 2026-09-29T19-58
Command: pwsh -NoProfile -Command '$h = Join-Path (Join-Path $env:TEMP "hygiene-927") "Invoke-IdentifierRedaction.ps1"; "HELPER=" + (Test-Path $h); & $h -DryRun 2>&1 | Tee-Object -FilePath (Join-Path (Join-Path $env:TEMP "hygiene-927") "dry-run-1.txt"); exit $LASTEXITCODE'; then git status --porcelain
EXIT_CODE: 0
Output Summary:
- HELPER=True (the helper lives at the expression Join-Path $env:TEMP "hygiene-927" plus Invoke-IdentifierRedaction.ps1; the value is not recorded).
- FILES-SCANNED=14807
- FILES-SKIPPED-BINARY=43
- UTF16-SCANNED=1
- FILES-WITH-MATCHES=1052 (expected about 1,051)
- FILES-WRITTEN=0 (dry run)
- FEATURE-FOLDER-WRITTEN=0
- RULE1=52387, RULE2=5725, RULE3=220, RULE4=1, RULE5=139, RULE6=4, RULE7=0, RULE8=30 (substitution counts the write run would make; RULE8 is the D17 rule)
- XML-REWRITTEN=0, XML-REPARSE-FAILED=0
- Porcelain after the dry run: only the plan file and one uncommitted evidence artifact under this feature folder, plus .claude/agent-memory/ paths from earlier agents (admitted by C8). No other modified tracked path; the dry run wrote nothing.

Helper construction (D6, D17, D18; counts only, no identifier value):
- Population: git ls-files -z --eol -- . ":!.claude/"; an i/-text record is scanned only when it carries a UTF-16 byte-order mark.
- Rules 1 to 5 and 8 use IgnoreCase; rules 6 and 7 are case-sensitive and consume an enclosing angle-bracket pair. The account alternation is built at run time from the account, the profile leaf, the eight-dot-three leaf and the three legacy user tokens. Rule 3 is skipped for the two MCP configuration files; rules 6, 7 and 8 are skipped under this feature folder.
- Bytes in, bytes out: the byte-order mark is detected and re-emitted; a file that is not valid in its detected encoding is decoded and re-encoded as Latin-1, which round-trips every byte; there is no newline normalisation; only files with a non-zero substitution count are written. XML-family files receive the escaped placeholder forms and are re-parsed after writing.
- The -VerifyDiff and -VerifyBom modes are read-only. A write run with a non-empty written set replaces the SCRATCH log and the originals sub-directory.
