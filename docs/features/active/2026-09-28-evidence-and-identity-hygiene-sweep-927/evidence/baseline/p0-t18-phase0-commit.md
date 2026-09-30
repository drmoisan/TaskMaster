# P0-T18 Phase 0 evidence commit

Timestamp: 2026-09-29T09-12
Command: git add -- docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927; git commit -m "docs(927): phase 0 baselines for the evidence and identity hygiene sweep" -m "Co-Authored-By: Claude Opus 5.5 noreply@anthropic.com" -m "Claude-Session: https://claude.ai/code/session_01KNZiXntshsLY8vqqCHUvHm" -- docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927; git show --name-only --format= HEAD; git status --porcelain
EXIT_CODE: 0
Output Summary:
- Commit created: [bug/evidence-and-identity-hygiene-sweep-927 5de781c4d] docs(927): phase 0 baselines for the evidence and identity hygiene sweep; 18 files changed, 411 insertions(+), 17 deletions(-) (the 17 deletions are the plan's replaced check-off lines).
- git show lists 18 paths, every one under this feature folder: the seventeen Phase 0 baseline artifacts and the plan file. The promoted record was not staged.
- git status --porcelain after the commit lists only the two pre-existing .claude/agent-memory/atomic-planner paths (not owned by this item and never staged); no path under this feature folder, scripts/, tests/ or .github/ and no .cs or .csproj path.
- The Claude-Session trailer was appended as a further -m operand in the bracket-free form, as convention C9 permits when the executing session instructs it.
- This artifact is written after the commit and is swept into the P1-T13 commit.
