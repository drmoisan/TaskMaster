# P4-T6 Post-rewrite eol and byte-order-mark parity

Timestamp: 2026-09-29T20-04
Command: the P4-T6 eol payload (git ls-files --eol -- . ":!.claude/" to SCRATCH\eol-after.txt, compared against SCRATCH\eol-before.txt for every path in the single anchored diff git diff --name-only HEAD -- . ":!docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/" ":!.claude/", membership by -contains); then pwsh -NoProfile -Command '$d = Join-Path $env:TEMP "hygiene-927"; & (Join-Path $d "Invoke-IdentifierRedaction.ps1") -VerifyBom 2>&1 | Tee-Object -FilePath (Join-Path $d "verify-bom.txt"); "BOM-EXIT=" + $LASTEXITCODE'
EXIT_CODE: 0
Output Summary:
- WRITTEN-COMPARED=1052 (equals the P4-T4 FILES-WRITTEN=1052)
- EOL-MISMATCH=0
- BOM-COMPARED=1052 (equals the P4-T4 FILES-WRITTEN=1052)
- BOM-MISMATCH=0
- BOM-EXIT=0
- EXIT_CODE 0 is the eol payload's exit code. The index side of the listing cannot change before the commit; P4-T9 repeats the index-side comparison after it.
