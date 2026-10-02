# P2-T6 Scope Lock (Remediation Cycle 1)

Timestamp: 2026-09-29T10-57
Task: P2-T6 (remediation-plan.2026-09-29T10-00.md; refreshes the original P2-T6 stem)
Command: git -C <repo-root> status --porcelain -uall; git -C <repo-root> diff --name-only 177b6d78e -- scripts tests .github .vscode config
EXIT_CODE: 0

## Porcelain (verbatim)

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/orchestrator/MEMORY.md
 M docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md
 M docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/remediation-plan.2026-09-29T10-00.md
 M scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
?? .claude/agent-memory/atomic-executor/project_backtick_span_grep_gap_matches.md
?? .claude/agent-memory/atomic-planner/project_928_backtick_descoping_blast_radius_seams.md
?? .claude/agent-memory/orchestrator/preimplementation-gate-path-leg-has-no-parallel-resolution.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/other/r1-p1-t1-implementation-handoff.2026-09-29T10-48.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t1-format.iter2.2026-09-29T10-53.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t2-analyze.iter2.2026-09-29T10-53.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t3-test-coverage.iter2.2026-09-29T10-55.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t4-loop-closure.2026-09-29T10-56.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t5-file-size-and-untouched-neighbors.2026-09-29T10-57.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/regression-testing/r1-p1-t2-test-authoring.2026-09-29T10-49.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/regression-testing/r1-p1-t3-expect-fail.2026-09-29T10-50.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/regression-testing/r1-p1-t4-scope-part-file.2026-09-29T10-51.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/regression-testing/r1-p1-t5-entry-point-edit.2026-09-29T10-51.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/regression-testing/r1-p1-t6-pass-after.2026-09-29T10-52.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/remediation-baseline/phase0-instructions-read.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/remediation-baseline/r1-p0-t2-identity-and-state.2026-09-29T10-42.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/remediation-baseline/r1-p0-t3-tree-facts.2026-09-29T10-43.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/remediation-baseline/r1-p0-t4-format-baseline.2026-09-29T10-44.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/remediation-baseline/r1-p0-t5-analyze-baseline.2026-09-29T10-45.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/remediation-baseline/r1-p0-t6-test-baseline.2026-09-29T10-47.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/remediation-baseline/r1-p0-t7-part-file-credit-diagnostic.2026-09-29T10-47.md
```

Classification: every line names a Write Set path (the three PowerShell files, the original plan file, the remediation plan file), a path under the feature folder (the evidence artifacts), or a path under the .claude/agent-memory tree (pre-existing; not staged by this plan). No line names a path under docs/features/potential/promoted. No line names a path under scripts/ or tests/ other than the Write Set PowerShell files. No line reads `?? coverage.xml`. The remediation plan file is committed at bde728cd4 and appears as modified because of its task check-offs.

## Name-only diff against 177b6d78e over scripts, tests, .github, .vscode, config (verbatim)

```
scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
scripts/vscode/Invoke-MSTestWithCoverage.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

Every listed path is one of the four Write Set PowerShell files, and the two Scope files and the entry point are each listed. No path under .github, .vscode or config is changed (the CI workflows are untouched).

Output Summary:
- PASS. Footprint confined to the Write Set, the feature folder and the pre-existing agent-memory paths; no CI workflow, configuration or out-of-scope script is changed; no stray coverage.xml.
