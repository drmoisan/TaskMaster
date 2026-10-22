# P2-T7 Footprint and Scope Boundary

Timestamp: 2026-10-02T01-22
Command: git rev-parse origin/main; then CMD-FOOTPRINT (pwsh -NoProfile -Command, first statement Set-Location -LiteralPath "WORKTREE"; whole-tree `git diff --name-only origin/main` paired with `git status --porcelain --untracked-files=all`; COMMITTED substituted from the P0-T3 COMMITTED-ON-BRANCH block, INHERITED substituted with nothing because P0-T3 recorded INHERITED-UNTRACKED: NONE, so the expression reads `@()`). No mechanical adaptation was needed; the payload prints no "removed"-like label and parsed unchanged.
EXIT_CODE: 0 (scoped to CMD-FOOTPRINT; the payload exits 0 exactly when RUNBOOK-IN-FOOTPRINT and WORKFLOW-IN-FOOTPRINT are True and OUTSIDE-COUNT is 0, all of which were printed as such)
Output Summary:
BASE-SHA-NOW: 34c2ed88cbb009f2f231453db87bc64d45a9bd51 (equals the P0-T3 BASE-SHA)
COMMITTED-SUBTRACTED: equals the P0-T3 COMMITTED-ON-BRANCH list (10 paths: 6 under .claude/agent-memory/, FEATURE/evidence/other/preflight-clearance.2026-10-02T00-38.md, FEATURE/issue.md, FEATURE/plan.2026-10-01T23-34.md, docs/features/potential/promoted/2026-09-30-dependabot-repair-runbook-and-workflow-comment-wording.md)
COMMITTED-NOW: 24 paths: the same 10 as above plus .github/workflows/dependabot-repair.yml, the RUNBOOK path under docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/, and 12 FEATURE/evidence/ artifacts written by Phases 0 and 1 (p0-t2 through p0-t9, phase0-instructions-read.md, p1-t2, p1-t6, p1-t8, p1-t9). The two production files were committed by the orchestrator after Phase 1, so each is accepted in this block rather than in PORCELAIN.
INHERITED-SUBTRACTED: NONE (equals the P0-T3 INHERITED-UNTRACKED: NONE)
PORCELAIN:
?? FEATURE/evidence/qa-gates/p2-t1-runbook-final.2026-10-02T01-15.md
?? FEATURE/evidence/qa-gates/p2-t2-actionlint.2026-10-02T01-16.md
?? FEATURE/evidence/qa-gates/p2-t3-workflow-width.2026-10-02T01-17.md
?? FEATURE/evidence/qa-gates/p2-t4-comment-only-diff.2026-10-02T01-18.md
?? FEATURE/evidence/qa-gates/p2-t5-pester-final.2026-10-02T01-19.md
?? FEATURE/evidence/qa-gates/toolchain-pass.md
THIS-ITEM-FOOTPRINT: 21 paths: .github/workflows/dependabot-repair.yml; the RUNBOOK path; and 19 paths under FEATURE/evidence/ (baseline p0-t2 through p0-t9 and phase0-instructions-read.md; qa-gates p1-t2, p1-t6, p1-t8, p1-t9, p2-t1 through p2-t5 and toolchain-pass.md)
OUTSIDE-SET: (empty)
RUNBOOK-IN-FOOTPRINT=True WORKFLOW-IN-FOOTPRINT=True OUTSIDE-COUNT=0
Acceptance observations: BASE-SHA-NOW equals the P0-T3 value; EXIT_CODE 0; both production files in the footprint; OUTSIDE-COUNT=0; each production file is accepted in the COMMITTED-NOW block (committed during the run by the orchestrator). The path FEATURE above abbreviates docs/features/active/2026-09-30-dependabot-repair-runbook-and-workflow-comment-wording-952.
