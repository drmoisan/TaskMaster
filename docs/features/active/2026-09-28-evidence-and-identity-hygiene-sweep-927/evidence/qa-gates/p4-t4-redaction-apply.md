# P4-T4 Redaction helper applied in write mode

Timestamp: 2026-09-29T20-02
Command: pwsh -NoProfile -Command '$d = Join-Path $env:TEMP "hygiene-927"; & (Join-Path $d "Invoke-IdentifierRedaction.ps1") 2>&1 | Tee-Object -FilePath (Join-Path $d "apply-run.txt"); exit $LASTEXITCODE'; then the P4-T4 modified-set payload over git diff --name-only HEAD -- . ":!docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/" ":!.claude/"
EXIT_CODE: 0
Output Summary:
- FILES-SCANNED=14807
- FILES-SKIPPED-BINARY=43
- UTF16-SCANNED=1 (the UTF-16 file P0-T17 counted; decoded by its mark and re-encoded as UTF-16 with the same mark)
- FILES-WITH-MATCHES=1052
- FILES-WRITTEN=1052 (equals FILES-WITH-MATCHES and MODIFIED)
- FEATURE-FOLDER-WRITTEN=0
- RULE1=52387, RULE2=5725, RULE3=220, RULE4=1, RULE5=139, RULE6=4, RULE7=0, RULE8=30
- XML-REWRITTEN=0, XML-REPARSE-FAILED=0
- MODIFIED=1052
- OUTSIDE-DOCS=0
- CODE-PATHS=0
- RESEARCH-NOTE=1
- No STOP: REDACTION OUTSIDE WRITE SET condition applies.

Ordering note: the P4-T8 PRE-CLEANUP UTF-16 census ran after the P4-T3 command completed and immediately before this task's write-mode invocation. It appended RX-LENGTH=40, UTF16-FILES=1, UTF16-PROFILE-FILES=1 and UTF16-IDENTIFIER-FILES=1 to SCRATCH\utf16-census.txt, which did not exist before that run.
