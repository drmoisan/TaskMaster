# P6-T14 Scope containment and AC19 ancestry checks

Timestamp: 2026-09-29T22-26
Command: the P6-T14 pwsh payload of plan revision 1.16, verbatim, run from the item worktree root, with the BASE-SHA token transcribed as the recorded P0-T2 value cfbb2bd6113745d1ecc12f28a2488a7223e362cf (C7); then git status --porcelain.
EXIT_CODE: 0
Output Summary:
- FETCH-EXIT=0 (the third check read the pushed tip as fetched)
- MERGE-BASE-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c (40 characters; recorded and not compared with the P0-T2 MERGE-BASE: value)
- ANCESTRY-1-EXIT=0 (the P0-T2 merge base 177b6d78e is an ancestor of the current merge base)
- ANCESTRY-2-EXIT=0 (BASE-SHA cfbb2bd61 is an ancestor of HEAD)
- ANCESTRY-3-EXIT=0 (the pushed tip of this branch is an ancestor of HEAD: no force push)
- NO-FORCE-PUSH: none issued by this plan. The run pushed the branch fast-forward only, at the caller's instruction, and never with force.
- ANCESTRY-CONTROL-EXIT=1 (negative control: HEAD is not an ancestor of the P0-T2 merge base; this is the line that shows the three checks can fail)
- ANCESTRY-PREP-REF-EXIT=0 (recorded, not gated; expected 0)
- DIFF-PATHS=1758 (three-dot origin/main...HEAD)
- OUTSIDE-WRITE-SET=0 (no OUTSIDE| line)
- GOVERNANCE=0
- MCP-CONFIG=0
- BUILD-INPUTS=0
- PROD-CS=0
- SIBLING-2026-09-28=0
- Porcelain span: only paths under this feature folder (the plan file and six new qa-gates artifacts) and under .claude/agent-memory/ (pre-existing agent-memory changes, admitted by C8). No path under scripts/, tests/ or .github/, and no .cs, .csproj or .sln path.
- No STOP: SCOPE VIOLATION condition.
