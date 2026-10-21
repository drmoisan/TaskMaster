# P4-T5 Second redaction run (idempotence, AC11)

Timestamp: 2026-09-29T20-04
Command: the numstat hash payload over git diff --numstat HEAD -- . ":!.claude/" (as NUMSTAT-HASH-BEFORE); pwsh -NoProfile -Command '$d = Join-Path $env:TEMP "hygiene-927"; & (Join-Path $d "Invoke-IdentifierRedaction.ps1") 2>&1 | Tee-Object -FilePath (Join-Path $d "apply-run-2.txt"); exit $LASTEXITCODE'; the same numstat hash payload (as NUMSTAT-HASH-AFTER)
EXIT_CODE: 0
Output Summary:
- NUMSTAT-HASH-BEFORE=5590E8BE8529637EC4BE96EF2B96B65ED941153BEB98A1FA74584FB63DA250C9
- Second run: FILES-SCANNED=14807, FILES-SKIPPED-BINARY=43, UTF16-SCANNED=1, FILES-WITH-MATCHES=0, FILES-WRITTEN=0, FEATURE-FOLDER-WRITTEN=0, RULE1=0, RULE2=0, RULE3=0, RULE4=0, RULE5=0, RULE6=0, RULE7=0, RULE8=0, XML-REWRITTEN=0, XML-REPARSE-FAILED=0
- NUMSTAT-HASH-AFTER=5590E8BE8529637EC4BE96EF2B96B65ED941153BEB98A1FA74584FB63DA250C9
- The two numstat hashes are equal.
- Because this run's written set is empty, SCRATCH\redaction.log and the SCRATCH originals sub-directory are left as P4-T4 wrote them (helper contract).
