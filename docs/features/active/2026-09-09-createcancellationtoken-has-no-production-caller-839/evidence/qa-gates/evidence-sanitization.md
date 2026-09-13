# Evidence sanitisation gate — issue #839

Timestamp: 2026-09-13T06-24
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $acct = Split-Path -Leaf $env:USERPROFILE; $machine = $env:COMPUTERNAME; $files = @(Get-ChildItem -Recurse -File -LiteralPath docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence); "EVIDENCE_FILES=$($files.Count)"; "ACCOUNT_TOKEN_HITS=$(@($files | Select-String -SimpleMatch $acct).Count)"; "HOST_TOKEN_HITS=$(@($files | Select-String -SimpleMatch $machine).Count)"; "CONTROL_ACCOUNT_HITS=$(@(Get-Content -LiteralPath coverage/839-final-nullable.detailed.log | Select-String -SimpleMatch $acct).Count)"'
EXIT_CODE: 0

## Output Summary

EVIDENCE_FILES=34
ACCOUNT_TOKEN_HITS=0
HOST_TOKEN_HITS=0
CONTROL_ACCOUNT_HITS=19578

All four conditions hold. 34 evidence files were scanned, which is above the minimum of 25 this gate requires, so the scan is not passing by looking at an empty or near-empty tree. Neither the account name nor the machine name appears anywhere in the feature folder's evidence tree.

## The positive control is what makes the zero meaningful

A zero-hit result proves nothing on its own: an instrument that never matches anything also reports zero. The control reads the msbuild detailed log written by [P3-T3], which records absolute project paths and therefore certainly contains the account-name token. It reports `CONTROL_ACCOUNT_HITS=19578`, so the same matcher, the same token and the same `-SimpleMatch` comparison do find the token when it is present. The two zeros above are therefore genuine absences rather than a broken search.

No token value is written into this artifact. This artifact records counts only, so it does not itself become the leak the gate is checking for. Both tokens are read at run time from the environment rather than being spelled in the command.

No remediation pass was needed. No hit was found, so no artifact had to be edited and no second run of the gate was required; only this single run is recorded.

Across every artifact in this feature folder, the worktree's absolute path prefix is written as the token REPO-ROOT. Where a transcribed console line carried an absolute path, for example the msbuild project-output line in evidence/regression-testing/p1-build.md and the FluentAssertions stack frame in evidence/regression-testing/init-token-source-fail-before.md, the prefix was replaced and the replacement stated at the point of use.

## Command-transport note

`Set-Location -LiteralPath "REPO-ROOT";` was prepended to the plan's span. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; both the evidence-tree path and the control log path in the span are worktree-relative, and the control would have read the wrong tree's log without the prefix. The command semantics are unchanged.
