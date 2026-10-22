# Evidence hygiene gate (P6-T11)

Timestamp: 2026-10-02T01-24
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $b = [char]92; $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950\evidence" -Recurse -File); "EVIDENCE_FILES=$($files.Count)"; "RAW_DOCUMENTS=$(@($files | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage", ".coveragexml", ".log") }).Count)"; $pattern = "[a-z]:[" + $b + $b + "/]+users[" + $b + $b + "/]+[a-z0-9_.~-]"; "PROFILE_PATH_LINES=$(@($files | Select-String -Pattern $pattern).Count)"' (plus a CLOCK echo, a PATTERN_LENGTH echo and a per-hit listing, which printed nothing)
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
EVIDENCE_FILES=55
RAW_DOCUMENTS=0
PATTERN_LENGTH=35 (the pattern was built from [char]92, so the Bash channel could not collapse its backslashes)
PROFILE_PATH_LINES=0

Pattern self-test (a separate in-memory probe; no file written): a backslash-separated drive-letter user-profile sample matched (True), the forward-slash form matched (True), and a REDACTED-PATH sample did not match (False). The zero count is therefore a real observation, not a vacuous one.

## Re-run after check-offs

Timestamp: 2026-10-02T01-26
Task: P6-T30
Command: the P6-T11 command above, re-run after P6-T29, plus one additional line scanning the files at the FEATURE root (spec.md, issue.md, plan.2026-10-01T07-11.md) with the same pattern.
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
EVIDENCE_FILES=57
RAW_DOCUMENTS=0
PROFILE_PATH_LINES=0
FEATURE_ROOT_PROFILE_PATH_LINES=0
