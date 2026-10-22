# Acceptance Criteria Status (P8-T1)

Timestamp: 2026-10-09T14-49
Command: Grep `^- \[(x| )\] AC[0-9] ` over FEATURE/spec.md; git hash-object --no-filters over the nine CMDDIR helper scripts
EXIT_CODE: 0
Output Summary:
- Grep `^- \[x\] AC[1-6] ` count 6; `^- \[ \] AC7 ` count 1.
- Helper-script hashes re-read and equal to P0-T3 (helper-scripts.2026-10-09T14-04.md) for all nine scripts.

### Acceptance Criteria Status
- Source: FEATURE/spec.md
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC7

AC7: PENDING-CI (checked off by the item's orchestrator after merge and @dependabot recreate on PR #984; D7)

## Check-off citations

| AC | Evidence |
|---|---|
| AC1 | evidence/other/manifest-edit-facts.2026-10-09T14-17.md; evidence/regression-testing/pass-after-orphaned-hintpath.2026-10-09T14-17.md |
| AC2 | evidence/regression-testing/pass-after-redirect-sync.2026-10-09T14-21.md; evidence/qa-gates/ps-line-counts.2026-10-09T14-26.md; evidence/qa-gates/ps-coverage.2026-10-09T14-26.md |
| AC3 | evidence/regression-testing/fail-before-orphaned-hintpath.2026-10-09T14-15.md; evidence/regression-testing/pass-after-orphaned-hintpath.2026-10-09T14-17.md |
| AC4 | evidence/qa-gates/workflows-untouched.2026-10-09T14-31.md (P8-T2 repeats the committed-range form) |
| AC5 | evidence/qa-gates/ps-temp-file-audit.2026-10-09T14-26.md; evidence/qa-gates/coverage-comparison-powershell.2026-10-09T14-26.md; PowerShell loop iteration 2 and C# loop iteration 1 artifacts |
| AC6 | evidence/other/integration-rehearsal.2026-10-09T14-35.md (REHEARSAL-VERDICT: PASS) |
| AC7 | pending CI |

## Helper-script hashes (P0-T3 versus now)

| Script | P0-T3 | Now |
|---|---|---|
| 985-probe.ps1 | fd56601d907546ef6b0f2d4b374d22cca441eb56 | fd56601d907546ef6b0f2d4b374d22cca441eb56 |
| 985-restore.ps1 | 96658345a6001e5c22b5e5b4174404edbff12127 | 96658345a6001e5c22b5e5b4174404edbff12127 |
| 985-csharpier.ps1 | 4436583817876b8c12d968489805c1655f46ddfb | 4436583817876b8c12d968489805c1655f46ddfb |
| 985-msbuild.ps1 | 7b80d637d6f7abccbf4df86877ac561fc0a0c6dc | 7b80d637d6f7abccbf4df86877ac561fc0a0c6dc |
| 985-mstest.ps1 | f45cc585940fd1efe2c0b409573b5c815e2fe7d8 | f45cc585940fd1efe2c0b409573b5c815e2fe7d8 |
| 985-pester.ps1 | 74ddc15e1a935c4b5dbd0efa4a72b63d9fcc0913 | 74ddc15e1a935c4b5dbd0efa4a72b63d9fcc0913 |
| 985-junit.ps1 | 4df03fc0b964ac2d337c37141a5266f89b5c96a3 | 4df03fc0b964ac2d337c37141a5266f89b5c96a3 |
| 985-repair.ps1 | 5cc7c5caaa7f90e37076de4ead07ce66332fe948 | 5cc7c5caaa7f90e37076de4ead07ce66332fe948 |
| 985-nuget-update.ps1 | cb461c5cfd131c592f587cc6b583d468239280ea | cb461c5cfd131c592f587cc6b583d468239280ea |

## Open item

P7-T17 (rehearsal worktree and throwaway branch removal) is unchecked: the PreToolUse hook refused `git worktree remove` (EPIC_WORKTREE_REMOVAL_BLOCKED). The rehearsal worktree under the session scratchpad and the local branch rehearsal-985-throwaway remain; no remote branch exists.
