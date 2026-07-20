---
name: epic-child-agent-memory-merge-conflicts
description: Parallel epic children conflict on shared .claude/agent-memory/*/MEMORY.md index files at integration-merge time; resolve by union
metadata:
  type: project
---

Parallel epic children merging into a shared integration branch routinely produce `mergeable: CONFLICTING` PRs whose ONLY conflicts are in `.claude/agent-memory/<agent>/MEMORY.md` index files — each child's subagents append an entry to the same index, so the append points collide.

**Why:** subagents (atomic-executor, feature-review) commit MEMORY.md index additions during each child; siblings that branched from the same integration tip all append near the end of the file.

**How to apply:** when a child→integration PR shows CONFLICTING/DIRTY with an otherwise clean feature (production dirs are disjoint across children), `git merge origin/<integration>` into the feature branch and resolve by UNION — strip the `<<<<<<< / ======= / >>>>>>>` marker lines and keep both sides' entries (`perl -i -ne 'print unless /^(<<<<<<<|=======|>>>>>>>)/'`). Then re-run the child's core verification (e.g. the isolated pragma-only build) post-merge before pushing, because the merge pulled in sibling production code. Distinct from [[parallel-epic-children-name-collisions]] (that is CS0101/CS0104 type-name collisions in shared namespaces; this is a git-index text conflict). Observed on #364 (helperclasses) vs siblings #368/#363/#367/#369, 2026-07-19.
