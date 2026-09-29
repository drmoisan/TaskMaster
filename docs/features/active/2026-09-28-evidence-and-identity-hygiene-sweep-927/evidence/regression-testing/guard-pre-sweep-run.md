# P1-T12 Guard over the pre-sweep tracked tree (expect-fail, AC1 observation)

Timestamp: 2026-09-29T17-41
Command: pwsh -NoProfile -Command '& ./scripts/hygiene/Test-RepositoryHygiene.ps1 2>&1 | Tee-Object -FilePath coverage/logs/927-guard-run.log; exit $LASTEXITCODE'
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary:
- GATE6 exited 1 after 1173 seconds of wall clock over 16,795 tracked records (about 4.4 GB of tracked content, of which about 4.2 GB is the raw XML and TRX documents under docs/features that Phase 2 removes).
- Count payload (verbatim from P1-T12): RAW=625, PROFILE=1214, UNREADABLE=0, HYGIENE Findings=1839.
- HYGIENE Findings=1839 equals RAW plus PROFILE (625 + 1214).
- RAW=625 equals RAW-POPULATION: 625 from the P0-T17 artifact (equality holds).
- PROFILE=1214 does not equal PROFILE-PATH-FILES: 1213 from the P0-T17 artifact (equality fails).
- REMOVAL-LIST-LINES: 625 (SCRATCH\raw-documents.txt written per D4 at the expression Join-Path $env:TEMP "hygiene-927"; its value is not recorded).
- No finding line is transcribed; counts only.
- STOP: AC1 ARITHMETIC MISMATCH

RECONCILIATION:
- SYMMETRIC-DIFFERENCE: 1 (paths only; the guard's profile-path file set against the gate-four file set measured on the same tree: ONLY-IN-GUARD=1, ONLY-IN-GATE4=0; GUARD-PROFILE-FILES=1214, GATE4-FILES=1213).
- The one path found only by the guard is docs/features/archive/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/2026-05-14T12-41-05Z/msbuild-analyzers.txt, the single UTF-16 tracked file (ONLY-IN-GUARD-ARE-UTF16=1). git grep -I omits it as binary; the guard decodes it by its byte-order mark, exactly as D2 describes.
- Cause of the mismatch: the P0-T17 UTF-16 census recorded UTF16-PROFILE-FILES: 0, so PROFILE-PATH-FILES was recorded as GATE4 plus 0 = 1213. That census ran its .NET regular expression through Bash, where the doubled backslash in the separator class arrives at pwsh as a single backslash, so the class matched only a forward slash and could not match the backslash-separated paths inside the UTF-16 file (known execution risk 2 in the delegation). The plan's own expectation for PROFILE-PATH-FILES is 1,214.
- Re-measurement on the current tree with the pattern delivered intact (verified in the same run by comparison with a [char]92 construction, RX-CANONICAL=True): GATE4-NOW=1213, UTF16-FILES-NOW=1, UTF16-PROFILE-FILES-NOW=1, RAW-UNION-NOW=625, TRACKED-TOTAL-NOW=16795 (16,590 at P0-T17; the difference is the origin/main merge at 6e6a86729, which changed none of the AC1 operands). The corrected operand is PROFILE-PATH-FILES = 1213 + 1 = 1214, which equals PROFILE=1214.
- The P0-T17 artifact was not edited; the plan's stop rule applies to the equality as written against the committed figure.

Execution notes:
- Every pwsh payload ran with a prefix that sets the location and [Environment]::CurrentDirectory to <repo-root> (the item worktree), because pwsh launched from Bash starts in the session checkout; regular-expression payloads were transcribed with the backslashes doubled again in the Bash source so pwsh receives the plan's pattern unchanged.
- The GATE6 console copy was discarded with Out-Null after Tee-Object; the full output is in coverage/logs/927-guard-run.log under the ignored coverage directory and is never committed.
