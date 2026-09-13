# Base anchor, worktree identity and inherited residue (issue #839)

Timestamp: 2026-09-13T02-30

Command: git rev-parse --show-toplevel
Command: git rev-parse --abbrev-ref HEAD
Command: git rev-parse HEAD
Command: git merge-base origin/main HEAD
Command: git diff --name-only 2405a829d6afd3b12eb7c228d57158a97cb4e2ca HEAD
Command: git status --porcelain --untracked-files=all

EXIT_CODE: 0

WORKTREE-LEAF: bugs-2026-09-11-item-839
BRANCH: bug/createcancellationtoken-has-no-production-caller-839
HEAD-SHA: 590c67f7faecd3eab74556f360666d32d028139e
BASE-SHA: 2405a829d6afd3b12eb7c228d57158a97cb4e2ca

INHERITED-DIFF-PATHS:
docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/issue.md
docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md
docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/research/2026-09-12T18-05-createcancellationtoken-init-path-research.md
docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md
docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/user-story.md

INHERITED-PORCELAIN: NONE

Output Summary:
- WORKTREE-LEAF: bugs-2026-09-11-item-839. The plan's [P0-T8] acceptance names the expected leaf as agent-a63e372c42be95942. That literal is a STALE planner-worktree value: the plan was authored in a different preparation worktree that has since been detached. The orchestrator corrected it before execution began and supplied the measured leaf above. [P0-T8] is treated as PASSING on the corrected value. This is not a Decision D14 halt and not a "tree moved" halt.
- BRANCH: bug/createcancellationtoken-has-no-production-caller-839. The plan's acceptance names the expected branch as worktree-agent-a63e372c42be95942. That literal is stale for the same reason and was corrected by the orchestrator. [P0-T8] is treated as PASSING on the corrected value. The plan document itself was deliberately not edited to fix either literal.
- HEAD-SHA: 590c67f7faecd3eab74556f360666d32d028139e.
- BASE-SHA: 2405a829d6afd3b12eb7c228d57158a97cb4e2ca, byte-for-byte equal to the plan's BASE-SHA literal. Decision D4's substitution rule is therefore NOT applied and every anchored diff span in this plan keeps the literal unchanged. Note that origin/main has advanced past this SHA because a sibling item merged; the merge base is unmoved, and no merge, rebase or fetch-and-reset was performed at any point in this run.
- INHERITED-DIFF-PATHS count: 5. Every entry begins docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/, so every entry is a Write Set path. No entry falls outside the two prefixes the acceptance tolerates, so the Decision D14 halt condition is not met.
- INHERITED-PORCELAIN count: 0 lines outside the Write Set, recorded as NONE. The status command printed exactly one line, an untracked file at docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/phase0-instructions-read.md, which is the [P0-T1] artifact written by this run and is inside the Write Set, so it is not inherited residue. The Decision D10 residue rule remains in force by rule rather than by list for every later gate: agent-memory, potential-tree and parallel-tree paths may appear during the run and are excluded from every staging span and porcelain gate without being re-snapshotted here.
- The status invocation exits 0 whether or not it prints a line; the recorded EXIT_CODE is its observed value.
