# P11-T14 bounded coverage-exception verification

Timestamp: 2026-08-04T11-15

Command: `Read runbooks/issue-400-repository-wide-powershell-coverage-exception.runbook.md; verify P11-T11 through P11-T13 evidence and read-only merge-base diff`

EXIT_CODE: 0

Output Summary: The authorized exception is limited to issue #400's pre-existing repository-wide PowerShell aggregate coverage debt. It neither authorizes coverage-policy mutation nor changes the mandatory format, analyzer, Pester, focused-coverage, or changed-line non-regression requirements.

| Runbook prerequisite | Verification |
| --- | --- |
| Plan of record retained | `remediation-plan.2026-07-21T21-37.md` remains the plan of record. |
| Mandatory PoshQC gates | P11-T11 formatter and Pester MCP gates passed. The folder analyzer retains the documented 16-finding inherited baseline; both changed paths have zero direct analyzer findings. |
| Focused wrapper coverage >=90% | P11-T12 records 99/110 commands, 90.00%. |
| Repository-wide diagnostic remains debt | P11-T7 records 0/2315 as an attribution diagnostic, not a passing aggregate result. |
| Changed-line non-regression | P11-T12 improved from 86/106 commands, 81.13%, to 99/110 commands, 90.00%. |
| Protected coverage inputs | P11-T12 and P11-T13 record unchanged hashes for configuration and runsettings. |

The runbook normally requires an orchestration-checkpoint human-interaction record. This task's plan expressly prohibits checkpoint modification, so this verification does not write the checkpoint. No runbook, configuration, runtime pin, package reference, source outside the two permitted PowerShell paths, or coverage-policy input was modified.
