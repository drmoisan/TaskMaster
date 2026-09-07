# P10-T14 — Evidence-path canonicality

Timestamp: 2026-08-28T01-59
Command: ls -1 docs/features/active/itemviewer-surface-defects-489/evidence/ ; test each of artifacts/baselines artifacts/baseline artifacts/qa artifacts/qa-gates artifacts/coverage artifacts/evidence for existence ; test docs/features/active/itemviewer-surface-defects-489/evidence/coverage for existence
EXIT_CODE: 0

## Immediate subdirectories of `FEATURE/evidence/`

```
baseline/
other/
qa-gates/
regression-testing/
```

| Subdirectory | Canonical kind | Verdict |
|---|---|---|
| `baseline/` | Yes | Permitted |
| `other/` | Yes | Permitted |
| `qa-gates/` | Yes | Permitted |
| `regression-testing/` | Yes | Permitted |

Every immediate subdirectory is one of the five canonical kinds `baseline`, `qa-gates`,
`regression-testing`, `issue-updates`, `other`. The fifth, `issue-updates/`, has not been created
because this batch posted no issue update; its absence is permitted — the rule constrains which
subdirectories may exist, not which must.

**There is no `FEATURE/evidence/coverage/` directory.** Existence test returns absent.

## Forbidden `artifacts/` sub-paths

| Path | Exists |
|---|---|
| `artifacts/baselines/` | **No** |
| `artifacts/baseline/` | **No** |
| `artifacts/qa/` | **No** |
| `artifacts/qa-gates/` | **No** |
| `artifacts/coverage/` | **No** |
| `artifacts/evidence/` | **No** |
| `artifacts/regression-testing/` | **No** |
| `artifacts/post-change/` | **No** |

None of the six the task enumerates exists, nor do the two further forbidden sub-paths the evidence
conventions name. The `artifacts/` directory itself contains only `hard_lock_prompt.txt` and
`orchestration/`, and `artifacts/orchestration/` is the one sub-path the conventions permit for
non-evidence orchestration use.

## Evidence tree contents

| Kind | Files |
|---|---|
| `baseline/` | 21 |
| `other/` | 2 |
| `qa-gates/` | 14 |
| `regression-testing/` | 64 |
| **Total** | **101** |

Extensions present across the whole tree: **80** `.md`, **19** `.trx`, **2** `.txt`. The two `.txt`
files are the `/v:normal` MSBuild file logs, written with the `.msbuild.txt` extension rather than
`.log`: `.gitignore:84` is `*.log`, so a `.log` artifact under `FEATURE/evidence/` could never be
committed and would be absent from the audit trail while every porcelain gate still reported clean.
Neither `.md` nor `.trx` nor `.msbuild.txt` is matched by any `.gitignore` rule.

**No helper script is retained under `evidence/`.** A search for `*.ps1` beneath
`FEATURE/evidence/` returns **0** files. Feature review's language-match is extension-only and
path-blind, so a single retained `.ps1` there would enter the PowerShell coverage denominator and
force a coverage FAIL. Every helper used during this batch was written to the system temp directory
outside the repository.

## Acceptance

| P10-T14 condition | Result |
|---|---|
| Every immediate subdirectory of `FEATURE/evidence/` is one of `baseline`, `qa-gates`, `regression-testing`, `issue-updates`, `other` | Met — four present, all canonical |
| There is no `FEATURE/evidence/coverage/` directory | Met |
| `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/coverage/` and `artifacts/evidence/` do not exist | Met — all six absent |

Output Summary: Evidence paths are **canonical**. `FEATURE/evidence/` has exactly four immediate
subdirectories — `baseline/`, `other/`, `qa-gates/`, `regression-testing/` — all of them canonical
kinds; `issue-updates/` is legitimately absent because no issue update was posted in this batch. There
is no `FEATURE/evidence/coverage/` directory. None of the six forbidden `artifacts/` sub-paths the
task enumerates exists, nor do `artifacts/regression-testing/` or `artifacts/post-change/`;
`artifacts/` holds only `hard_lock_prompt.txt` and the permitted `orchestration/`. The tree holds 101
files — 80 `.md`, 19 `.trx`, 2 `.msbuild.txt` — and **zero** `.ps1` helper scripts.
