# P0-T3 Worktree context, diff anchor, inherited paths and gate readiness

Timestamp: 2026-10-01T20-37
Command: git -C WORKTREE <arguments>, one invocation per command, in the plan order:
- git rev-parse --abbrev-ref HEAD
- git rev-parse HEAD
- git rev-parse origin/main
- git merge-base HEAD origin/main
- git merge-base --is-ancestor MERGE-BASE HEAD
- git diff --exit-code 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f MERGE-BASE -- UtilitiesCS UtilitiesCS.Test scripts/vscode (run with --stat added for readability; --stat does not change the exit code)
- git diff --exit-code MERGE-BASE HEAD -- UtilitiesCS UtilitiesCS.Test (run with --stat added; exit code unaffected)
- git diff --name-only MERGE-BASE
- git status --porcelain --untracked-files=all
- git status --porcelain --untracked-files=all -- UtilitiesCS UtilitiesCS.Test
- Read tool: artifacts/orchestration/orchestrator-state.json (read only, not edited)
EXIT_CODE: 0
Output Summary:
BRANCH: bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956
BASE-SHA: a0e5383cfbfaae99e6bc7ac5f8c740c7f794a0c4
ORIGIN-MAIN-SHA: f5b46df637de81a0f4a856152095544f859718cc
MERGE-BASE: f5b46df637de81a0f4a856152095544f859718cc
MERGE-BASE-IS-ANCESTOR-EXIT: 0
CITED-TREE-EXIT: 0 (no output; 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f and MERGE-BASE are identical under UtilitiesCS, UtilitiesCS.Test and scripts/vscode)
BRANCH-SOURCE-EXIT: 0 (no output)
INHERITED-CLAUSE-A (union of `git diff --name-only MERGE-BASE` and `git status --porcelain --untracked-files=all`, captured before this artifact was written):
- .claude/agent-memory/atomic-planner/MEMORY.md
- .claude/agent-memory/atomic-planner/project_956_r1_spec_self_hit_and_raw_doc_name_seams.md
- .claude/agent-memory/orchestrator/MEMORY.md
- .claude/agent-memory/orchestrator/amend-ac-text-when-a-measurement-rule-is-ratified.md
- .claude/agent-memory/prd-feature/project_promotion_scaffold_metadata_defects.md
- .claude/agent-memory/task-researcher/MEMORY.md
- .claude/agent-memory/task-researcher/project_sortemail_split_and_prompt_seam_956.md
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/other/preflight-clearance.2026-10-01T20-25.md
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/issue.md
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/plan.2026-10-01T06-34.md
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/research/2026-10-01T07-00-sort-email-oversized-with-untestable-io-and-dialog-paths-research.md
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md
- docs/features/potential/promoted/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths.md
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/phase0-instructions-read.md (P0-T1 artifact, untracked)
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/p0-t2-mode-preconditions.2026-10-01T20-37.md (P0-T2 artifact, untracked; listed by porcelain under the label 20-38, renamed to 20-37 to match its true write time)
INHERITED-OUTSIDE-FEATURE: docs/features/potential/promoted/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths.md
WRITE-SET-IN-CLAUSE-A: none (no Write Set path is listed)
SOURCE-PORCELAIN: EMPTY
CHECKPOINT-ISSUE-NUM: 956
CHECKPOINT-FEATURE-FOLDER: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956
CHECKPOINT-ROUTE: large (route_id)
CHECKPOINT-LIFECYCLE-READY: true
PRE-IMPLEMENTATION GATE READY: YES
Note: the plan's Verified Repository Fact 12 recorded route_id `preparation`; the checkpoint now records `large` (orchestrator advanced it). The readiness rule requires only a non-ABSENT route, so the gate is ready.
Acceptance: all eight conditions hold; no absolute path is recorded in this artifact.
