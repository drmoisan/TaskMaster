Timestamp: 2026-08-11T14-30
Command: `Get-Content -LiteralPath docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md -Raw`
EXIT_CODE: 0
Output Summary: The existing upstream prompt contains every required contract provision; no upstream execution is claimed.

## Contract verification

| Required provision | Result | Prompt evidence |
|---|---|---|
| Names issue #494 | PASS | Objective states “TaskMaster issue #494.” |
| Identifies the upstream-only source boundary | PASS | Usage boundary directs work to the upstream customization source and prohibits direct TaskMaster generated-path edits. |
| Requires reconciled policy | PASS | Required upstream work item 2 requires reconciliation of thresholds, branch coverage, denominator, exclusion rules, and one authoritative policy source. |
| Requires fail-closed gate behavior | PASS | Required upstream work item 4 requires the hook’s missing-input behavior to agree with policy; acceptance criteria require the hook to fail closed for absent or invalid input. |
| Requires deterministic tests | PASS | Required upstream work item 5 requires deterministic tests and a below-threshold negative-path proof. |
| Requires publication information | PASS | Required upstream work item 6 requires regeneration or packaging and downstream release/publication instructions. |
| Requires every affected future TaskMaster path | PASS | Acceptance criteria require identifying every generated TaskMaster path affected by a future supported publication. |
| Requires #512 non-interference | PASS | Non-goals prohibit changing the C# toolchain command contract owned by issue #512. |

## Execution statement

This check inspected only the TaskMaster prompt artifact. It records no upstream source edit, generation, publication, validation command, or release execution.
