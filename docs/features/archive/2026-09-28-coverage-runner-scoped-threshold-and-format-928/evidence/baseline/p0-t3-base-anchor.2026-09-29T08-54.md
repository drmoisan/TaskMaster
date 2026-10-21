# P0-T3 Base Anchor and Initial Tree State

Timestamp: 2026-09-29T08-54
Task: P0-T3
Command: git rev-parse --abbrev-ref HEAD; git merge-base HEAD origin/main; git status --porcelain -uall; git check-attr text -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/vscode/Invoke-MSTest.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1 (each run as git -C <repo-root> ...)
EXIT_CODE: 0

Branch:
bug/coverage-runner-scoped-threshold-and-format-928

Merge base:
177b6d78e1b2408e5aedbd794cef3aad6b7fb372

Porcelain (verbatim):
```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md
?? .claude/agent-memory/atomic-executor/project_backtick_span_grep_gap_matches.md
?? .claude/agent-memory/atomic-planner/project_928_backtick_descoping_blast_radius_seams.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/baseline/p0-t2-mode-and-ac-source.2026-09-29T08-53.md
?? docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/evidence/baseline/phase0-instructions-read.md
```
Path-class check: 3 lines under the feature folder (the plan check-off edits and two Phase 0 artifacts), 4 lines under .claude/agent-memory (pre-existing, not produced by this run). 0 lines under scripts/ or tests/.

check-attr (verbatim):
```
scripts/vscode/Invoke-MSTestWithCoverage.ps1: text: auto
scripts/vscode/Invoke-MSTest.ps1: text: auto
scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1: text: auto
tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1: text: auto
```

Output Summary:
- Branch equals the plan metadata branch (bug/coverage-runner-scoped-threshold-and-format-928).
- Merge base equals 177b6d78e1b2408e5aedbd794cef3aad6b7fb372.
- Every porcelain line is under the feature folder or the .claude/agent-memory tree; none under scripts/ or tests/.
- All four check-attr lines read `text: auto`.
- No stop condition fired (wrong-tree not triggered).
