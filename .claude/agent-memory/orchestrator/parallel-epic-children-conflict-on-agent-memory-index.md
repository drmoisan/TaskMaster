---
name: parallel-epic-children-conflict-on-agent-memory-index
description: Parallel epic children reliably conflict on .claude/agent-memory/<agent>/MEMORY.md when merging the updated integration tip; resolve by union
metadata:
  type: project
---

When a child feature orchestrator finishes after sibling children have already merged into the epic integration branch, its PR shows CONFLICTING/DIRTY and the merge/rebase conflict is almost always ONLY in `.claude/agent-memory/<agent>/MEMORY.md` (e.g. `atomic-executor/MEMORY.md`), not in production code.

**Why:** each child's atomic-executor (and feature-review) appends its own one-line entry to the top of the shared agent-memory index. Two children editing the same index region produce a textual conflict even though their code changes are in disjoint directories. On #363 (utilitiescs-nullable-extensions), siblings #367/#368 had merged; the only conflict was `atomic-executor/MEMORY.md`.

**How to apply:** merge `origin/<integration-branch>` into the feature branch, resolve the MEMORY.md conflict by UNION (keep both sides' entries — they are independent index lines), commit the merge, and re-run the isolated pragma/build gate on the merged tree to rule out cross-child code interaction (CS0101/CS0104). See [[parallel-epic-children-name-collisions]] for the code-collision case. After the merge the PR flips to CLEAN/MERGEABLE. Integration-base PRs get no CI (see [[project_epic_child_prs_no_ci]]), so merge proceeds on blocking_count==0 + CLEAN.

**Confirmed instances:** [[epic-child-agent-memory-merge-conflicts]] (#364 vs siblings #368/#363/#367/#369) and [[epic-child-rebase-shared-memory-conflict]] (a pre-PR rebase hitting the same MEMORY.md index region) are the same conflict recurring on different children — resolution is identical (union, then re-verify).
