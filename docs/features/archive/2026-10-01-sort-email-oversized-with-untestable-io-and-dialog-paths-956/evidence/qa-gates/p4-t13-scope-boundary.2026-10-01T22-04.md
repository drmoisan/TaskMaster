# P4-T13 Footprint and scope boundary

Timestamp: 2026-10-01T22-04
ITERATION: 1
Command: CMD-FOOTPRINT with MERGE-BASE f5b46df637de81a0f4a856152095544f859718cc (P0-T3) and INHERITED the fifteen quoted INHERITED-CLAUSE-A paths of P0-T3; the payload pairs `git diff --name-only f5b46df637de81a0f4a856152095544f859718cc` with `git status --porcelain --untracked-files=all` and runs `git diff --numstat f5b46df637de81a0f4a856152095544f859718cc -- UtilitiesCS/UtilitiesCS.csproj` and `git diff --numstat f5b46df637de81a0f4a856152095544f859718cc -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`; one pwsh -NoProfile -Command invocation beginning Set-Location to the item worktree
EXIT_CODE: 0
Output Summary:
FOOTPRINT-PATHS: 78
SUBTRACTED-CLAUSE-A: 15
SUBTRACTED-CLAUSE-B: 7
OUTSIDE-WRITE-SET: 0
WRITE-SET-MISSING: 0
RAW-DOC-PATHS: 0
NUMSTAT-UCS: 6	0	UtilitiesCS/UtilitiesCS.csproj
NUMSTAT-UCT: 2	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj

Notes: the two-dot diff compares the working tree with MERGE-BASE, so the branch commits made since P0-T3 (03efa278c, 29a3e7332, a9a0f9bdd and the evidence and plan commits) are inside the comparison; no main merge has occurred since P0-T3 (the branch history after MERGE-BASE contains only the merge a0e5383cf recorded by P0-T3 as BASE-SHA and the feature commits). Clause A (15 paths) and Clause B (7 `.claude/agent-memory/` paths) overlap: the seven Clause A agent-memory paths are the seven Clause B paths, so 15 distinct paths are subtracted. No Write Set path was subtracted.

Acceptance evaluation (P4-T13, all five required):
1. `OUTSIDE-WRITE-SET: 0`: HOLDS.
2. `WRITE-SET-MISSING: 0`: HOLDS.
3. `RAW-DOC-PATHS: 0`: HOLDS.
4. `NUMSTAT-UCS:` reads `6 0 UtilitiesCS/UtilitiesCS.csproj` and `NUMSTAT-UCT:` reads `2 0 UtilitiesCS.Test/UtilitiesCS.Test.csproj` (tab separated): HOLDS.
5. The subtracted Clause A (15) and Clause B (7) counts are recorded: HOLDS.
