# P8-T2 Merge Record

Timestamp: 2026-10-06T17-53
Command: git rev-parse HEAD; git merge --no-ff origin/main -m "merge(959): origin/main into bug/sort-email-latent-logic-defects-959 for the pull request" (the attribution trailers supplied as two further -m paragraphs); git rev-parse HEAD; git log -1 --format=%P HEAD; git status --porcelain --untracked-files=all (each issued as one git -C <worktree> invocation); then Grep-tool counts over QuickFiler.Test/QuickFiler.Test.csproj
EXIT_CODE: 0 (scoped to the `git merge` invocation, under CSPROJ-CONFLICT: NONE)
ITERATION: 1
Output Summary: the merge completed with the ort strategy and no conflict ("Auto-merging .claude/agent-memory/orchestrator/MEMORY.md", "Auto-merging QuickFiler.Test/QuickFiler.Test.csproj", "Merge made by the 'ort' strategy."); the merge commit has two parents in the order pre-merge HEAD then origin/main; the worktree is clean; the merged QuickFiler.Test.csproj carries each of the four PD-17 Compile lines exactly once (lines 128, 205, 231, 232) and no conflict marker.

PRE-MERGE-HEAD: b3d2905b9d37e84829ebc912596462059283da50
MERGE-EXIT: 0
CSPROJ-CONFLICT: NONE
MERGE-HEAD-SHA: 9163994569e24c5c539a285724f9c8f9f6fd8a0e
MERGE-PARENTS: b3d2905b9d37e84829ebc912596462059283da50 f8ea1b5dcc6514bc0088bc80965c188bfd717557
POST-MERGE-PORCELAIN: 0
QFT-FILERCLEANUP-LINES: 1 (line 128)
QFT-PINCOUNT-LINES: 1 (line 205)
QFT-SYNCWORKER-LINES: 1 (line 231)
QFT-ARMINGFAKE-LINES: 1 (line 232)
QFT-MARKER-LINES: 0

Count method: one Grep-tool content search over QuickFiler.Test/QuickFiler.Test.csproj with the five backslash-free patterns joined by `|`; each returned line matches exactly one pattern, so the per-pattern counts above are read from the returned lines (four lines returned, none matching the marker pattern).

## Acceptance

1. MERGE-HEAD-SHA differs from PRE-MERGE-HEAD: PASS
2. MERGE-PARENTS equals PRE-MERGE-HEAD then ORIGIN-MAIN-SHA (f8ea1b5dcc6514bc0088bc80965c188bfd717557 from P8-T1): PASS
3. POST-MERGE-PORCELAIN: 0: PASS
4. The four QFT line counts are each 1: PASS
5. QFT-MARKER-LINES: 0: PASS
6. CSPROJ-CONFLICT recorded as NONE: PASS
