# P2-T6 Scope Lock

Timestamp: 2026-09-29T09-27
Task: P2-T6
Command: git status --porcelain -uall; git diff --name-only 177b6d78e -- scripts tests .github .vscode config
EXIT_CODE: 0

Porcelain listing (verbatim):

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/orchestrator/MEMORY.md
 M docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md
?? .claude/agent-memory/atomic-executor/project_backtick_span_grep_gap_matches.md
?? .claude/agent-memory/atomic-planner/project_928_backtick_descoping_blast_radius_seams.md
?? .claude/agent-memory/orchestrator/preimplementation-gate-path-leg-has-no-parallel-resolution.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t1-format.iter1.2026-09-29T09-16.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t2-analyze.iter1.2026-09-29T09-18.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t4-loop-closure.2026-09-29T09-25.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/qa-gates/p2-t5-file-size-and-untouched-neighbors.2026-09-29T09-26.md
```

Anchored name-only diff (verbatim):

```
scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
scripts/vscode/Invoke-MSTestWithCoverage.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

Checks:

- Every porcelain line names a path under the feature folder or under the agent-memory tree beneath the .claude directory (the three modified MEMORY.md files and three untracked memory files are pre-existing session changes, not item work, and are not staged). No porcelain line names a path under scripts/ or tests/.
- Commit-state reading: the three Write Set PowerShell files the change touches are committed at 313d0b918, so they appear in the anchored diff and not in porcelain. The porcelain clause is a negative path-class clause and holds as written.
- Every path the name-only diff lists is a Write Set PowerShell file; no path under .github, .vscode or config is listed. Invoke-MSTest.ps1 is absent because the formatter did not rewrite it.
- No porcelain line reads `?? coverage.xml`.

Output Summary: PASS. The footprint over scripts, tests, .github, .vscode and config is exactly the entry point, the new part file and the new test file. The porcelain lists only feature-folder and pre-existing agent-memory paths.
