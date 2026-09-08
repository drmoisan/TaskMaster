# [P6-T14] Evidence-artifact sanitisation scan

Timestamp: 2026-09-08T03-19

Command: for each file under `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/`, search its content case-insensitively for three tokens: the run-time-derived account token produced by `Split-Path -Leaf $env:USERPROFILE`, the run-time-derived machine token `$env:COMPUTERNAME`, and the literal drive-and-users path prefix formed from `C:`, a backslash, `Users` and a backslash.

The two host tokens are recorded here **by their derivation rather than by their value**, so this artifact does not itself introduce the tokens it exists to exclude. The account token was confirmed non-empty at 9 characters and the machine token was confirmed set, so neither search was vacuous. The scan script is exempt from its own scan because it names the tokens only as variables and is held in the session scratchpad, outside the evidence tree.

EXIT_CODE: 0

SCANNED_ARTIFACT_COUNT: 43

HOST_TOKEN_HIT_COUNT: 0

No hit was found, so no enumeration by file and line follows.

The scanned set is every file under the feature folder's `evidence/` tree at the time of the scan, across the sub-paths `baseline/`, `qa-gates/`, `regression-testing/` and `other/`. It explicitly includes `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/p6-t12-untracked-output-check.md` and `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/p6-t13-closure-summary.md`, both confirmed present in the enumeration.

This task runs after every other artifact-writing task in the plan, because a scan cannot cover a file that does not yet exist when it runs. The only artifact it cannot cover on the first pass is this one, which the second pass below covers.

Output Summary: 43 evidence artifacts scanned for three host tokens; zero hits.

## Second pass

The identical scan was re-run over this artifact alone, after the first pass had written it.

SECOND_PASS_SCANNED_ARTIFACT_COUNT: 1
SECOND_PASS_HOST_TOKEN_HIT_COUNT: 0
